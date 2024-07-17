using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Models.DTOs;

namespace Data.Classes
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShoppingCartRepo _shoppingCart;
        private readonly IPaymentService _paymentService;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public OrderRepository(IUnitOfWork unitOfWork,IShoppingCartRepo shoppingCart,IPaymentService paymentService,IEmailSender emailSender,IMapper mapper)
        {
            _mapper = mapper;
            _paymentService = paymentService;
            _emailSender = emailSender;
            _shoppingCart = shoppingCart;
            _unitOfWork = unitOfWork;
            
        }
        public async Task<ActualOrder> CreateOrderAsync(string Email,string CartId, UserAddress address,UserAddress billingAddress,decimal deliveryPrice,decimal tax, string status)
        {
            var cartData = await _shoppingCart.GetCartAsync(CartId,Email);
            var listOfCartItems = new List<OrderedProducts>();
           
            //Taking items in the shopping cart and adding them to items being ordered
            foreach(var item in cartData.ShoppingCartItems)
            {
                //mapping product price from db to cart items just in case
                var prodFromDB = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x=>x.Id == item.Id);
                var orderProdPics = await CovertToPicsForOrders(item.imageUrl);
                var nuOrderitems = new OrderedProducts(item.Id,item.Name,item.Description,prodFromDB.price,item.Amount,orderProdPics,item.Category,item.Brand,Email);
                if(!nuOrderitems.CancelItem.HasValue)
                {
                    nuOrderitems.CancelItem = false;
                }
                if(!nuOrderitems.RequestRefund.HasValue)
                {
                    nuOrderitems.RequestRefund = false;
                }
                listOfCartItems.Add(nuOrderitems);
            }
            var subTotal = listOfCartItems.Sum(x=>x.price * x.Amount);
            var deliveryPlusTax = tax + deliveryPrice;
            var TrueTotal = subTotal + deliveryPlusTax;
            var ActualOrder = new ActualOrder(listOfCartItems,Email,address,billingAddress,status,subTotal,tax,TrueTotal,cartData.PaymentID,deliveryPrice);
            if(ActualOrder != null)
            {
                var existingOrder = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x => x.PaymentId == ActualOrder.PaymentId);
                if (existingOrder != null)
                {
                    _unitOfWork.Repository<ActualOrder>().Update(existingOrder);
                    await _paymentService.CreateOrUpdatePayment(cartData.Id,address.AppUser);
                    await _unitOfWork.Complete();
                    return existingOrder;
                } 
               
            }
            var currentUser = await _unitOfWork.Repository<AppUser>().GetFirstOrDefault(x=>x.Email == address.AppUser);
            currentUser.PhoneNumber = address.PhoneNumber;
             _unitOfWork.Repository<ActualOrder>().Add(ActualOrder);
            var info =  await _unitOfWork.Complete();
            if (info <= 0) return null;
            return ActualOrder;
        }

        public async Task<string> EmailToSend(ActualOrder actualOrder, Decimal subTotal, Decimal tax)
        {
            var body = new StringBuilder();
            body.Append("<html lang=\"en\">");
            body.Append("<div style=\"width: auto;\">");
            body.Append("<div style=\"text-align: center;  width: 355px; margin: auto; border: 1px solid #0000001c; border-radius: 5px;\">");
            body.Append("<div style=\"line-height: 24px; background-color: #00ffff; padding: 16px;\">");
            body.Append("<h4>ORDER CONFIRMATION</h4>");
            body.Append("<p>Thank you for your order!</p>");
            body.Append("<p>You can find your purchase information below.</p>");
            body.Append("<p>Once payments are cleared, Your order will be shipped! You will receive an updated email on your order.</p>");
            body.Append("</div>");
            body.Append("<div style=\"padding-left: 16px; padding-right: 16px;\">");
            body.Append("<h4 style=\"margin-top: 16px; margin-bottom: 16px;\">Order Summary</h4>");
            body.Append("<div style=\"width: 100%; margin: auto; border: 1px solid #00000080;\">");
            body.Append("<table style=\"width: 100%; background-color: aqua;\">");
            body.Append("<tr><th style=\"width: 40%;\">Product Name</th><th style=\"width: 40%;\">Price</th><th style=\"width: 40%;\">QTY</th></tr>");
            foreach(var item in actualOrder.OrderedProducts)
            body.Append($"<tr><td style=\"border: 1px solid rgba(0, 0, 0, 0.5);\">{item.Name}</td><td style=\"border: 1px solid rgba(0, 0, 0, 0.5); text-align: right;\">${item.price}</td><td style=\"border: 1px solid rgba(0, 0, 0, 0.5); text-align: right;\">{item.Amount}</td></tr>");
            body.Append("</table>");
            body.Append("</div>");
            body.Append("<h4 style=\"margin-top: 16px; margin-bottom: 16px;\">Order Total</h4>");
            body.Append("<div style=\"width: 100%; display: flex;\">");
            body.Append("<div style=\"width: 50%; text-align: left;\">");
            body.Append("<p>Subtotal</p>");
            body.Append("<p>Shipping</p>");
            body.Append("<p>Tax</p>");
            body.Append("</div>");
            body.Append("<div style=\"width: 50%; text-align: right;\">");
            body.Append($"<p>${subTotal}</p>");
            body.Append($"<p>${actualOrder.ShippingHandlingPrice}</p>");
            body.Append($"<p>${tax}</p>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("<hr style=\"margin-top: 16px; margin-bottom: 16px;\">");
            body.Append("<div style=\"width: 100%; display: flex;\">");
            body.Append("<div style=\"width: 50%; text-align: left;\">");
            body.Append("<p>Total</p>");
            body.Append("</div>");
            body.Append("<div style=\"width: 50%; text-align: right;\">");
            body.Append($"<p><strong>${actualOrder.TrueTotal}</strong></p>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("<h4 style=\"margin-top: 16px; margin-bottom: 16px;\">Billing and Shipping</h4>");
            body.Append("<div style=\"display: flex; width: 100%; margin: auto;\">");
            body.Append("<div style=\"text-align: left; width: 50%;\">");
            body.Append("<div style=\"text-align: center;\">");
            body.Append("<h4>Billing</h4>");
            body.Append("</div>");
            body.Append($"<p>{actualOrder.BillingAddress.FirstName} {actualOrder.BillingAddress.LastName}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.AddressLine1}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.AddressLine2}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.City} , {actualOrder.BillingAddress.State} </p>");
            body.Append($"<p>{actualOrder.BillingAddress.ZipCode}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.PhoneNumber}</p>");
            body.Append("</div>");
            body.Append("<div style=\"text-align: left; width: 50%;\">");
            body.Append("<div style=\"text-align: center;\">");
            body.Append("<h4>Shipping</h4>");
            body.Append("</div>");
            body.Append($"<p>{actualOrder.ShippingAddress.FirstName} {actualOrder.ShippingAddress.LastName}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.AddressLine1}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.AddressLine2}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.City} , {actualOrder.ShippingAddress.State} </p>");
            body.Append($"<p>{actualOrder.ShippingAddress.ZipCode}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.PhoneNumber}</p>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("<button style=\"margin-top: 16px; cursor: pointer; margin-bottom: 16px; padding: 5px; background-color: aqua; color: #ffea01; border-radius: 5px; border: none; text-shadow: 2px 2px 5px #00000080; -webkit-text-stroke-width: 1px;\" >View Order</button>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("</html>");

            return await Task.FromResult(body.ToString());
        } 
        public async Task UpdateOrderedItemsData(int Id, IEnumerable<OrderedProductsDTO> orderedProductsDTO)
        {   
            var order = await _unitOfWork.Repository<OrderedProducts>().GetAll(x =>x.ActualOrderId == Id);
            var infoToChange = orderedProductsDTO.Where(x=>x.TimeToKeepCancelButtonOn == false);
            foreach(var item in order)
            {
                foreach(var info in infoToChange)
                {
                    item.TimeToKeepCancelButtonOn = info.TimeToKeepCancelButtonOn;
                }
            }
            await _unitOfWork.Complete();
        }
        private async Task<ICollection<PhotosForOrderedProducts>> CovertToPicsForOrders(ICollection<Photo> picsToConvert)
        {
            var listOfPics = new List<PhotosForOrderedProducts>();
            foreach(var item in picsToConvert)
            {
                var convertedPic = new PhotosForOrderedProducts
                {
                    Id = item.Id,
                    IsMain = item.IsMain,
                    PhotoUrl = item.PhotoUrl,
                    PublicId = item.PublicId,
                };
                listOfPics.Add(convertedPic);
            }
           return await Task.FromResult(listOfPics);
        }

        public async Task UpdateOrderStatus(int Id, string status)
        {
            var obj = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x =>x.Id == Id,"OrderedProducts");
            if(obj != null)
            {
                var itemToUpdateStatus = obj.OrderedProducts.FirstOrDefault(x=>x.ActualOrderId == Id);
                obj.OrderStatus = status;
            }
        }


    }
}