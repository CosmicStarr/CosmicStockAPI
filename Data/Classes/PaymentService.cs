using Microsoft.Extensions.Configuration;
using Stripe;

namespace Data.Classes
{
    public class PaymentService : IPaymentService
    {
        private readonly IShoppingCartRepo _shoppingCartRepo;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string USD = "usd";
        public PaymentService(IUnitOfWork unitOfWork, IConfiguration configuration,IShoppingCartRepo shoppingCartRepo)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _shoppingCartRepo = shoppingCartRepo;
            
        }
        public async Task<ShoppingCart> CreateOrUpdatePayment(string Id, string user)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var CartData = await _shoppingCartRepo.GetCartAsync(Id,user);
            if(CartData == null) return null;
            // var shippingInfo = 0m;
            foreach (var item in CartData.ShoppingCartItems)
            {
                var prodData = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x =>x.Id == item.Id);
                if(item.price != prodData.price)
                {
                    item.price = prodData.price;
                }
            }

            var stripeService = new PaymentIntentService();

            PaymentIntent intent;

            if(string.IsNullOrEmpty(CartData.PaymentID))
            {
                var options = new PaymentIntentCreateOptions
                {
                    //Add shipping logic to be calculated!
                    Amount = (long) CartData.ShoppingCartItems.Sum(x=>x.Amount * (x.price * 100)),
                    Currency = USD,
                    PaymentMethodTypes = new List<string>
                    {
                        "card"
                    },
                };
                intent = await stripeService.CreateAsync(options);
                CartData.PaymentID = intent.Id;
                CartData.ClientSecret = intent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    //Add shipping logic to be calculated!
                    Amount = (long) CartData.ShoppingCartItems.Sum(x=>x.Amount * (x.price * 100)),
                };
                await stripeService.UpdateAsync(CartData.PaymentID,options);
            }
            await _shoppingCartRepo.UpdateCartAsync(CartData, user);
            return CartData;
        }

        public async Task<ActualOrder> UpdateOrderStatusPaymentFailed(string paymentIntedId)
        {
            var orderData = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.PaymentId == paymentIntedId);
            if(orderData is null) return null;
            orderData.PaymentStatus = PaymentStatus.PaymentFailed;
            orderData.OrderStatus = StaticInfo.NotYet;
            _unitOfWork.Repository<ActualOrder>().Update(orderData);
            await _unitOfWork.Complete();
            return orderData;
        }

        public async Task<ActualOrder> UpdateOrderStatusPaymentSuccessful(string paymentIntedId)
        {
            var orderData = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.PaymentId == paymentIntedId);
            if(orderData is null) return null;
            orderData.PaymentStatus = PaymentStatus.PaymentRecevied;
            orderData.OrderStatus = StaticInfo.GoAhead;
            _unitOfWork.Repository<ActualOrder>().Update(orderData);
            await _unitOfWork.Complete();
            return orderData;
        }
    }
}