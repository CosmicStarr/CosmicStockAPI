using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CosmicStockapi.ErrorHandling;
using CosmicStockapi.Extension;
using CosmicStockapi.helper;
using Data.Interfaces;
using Data.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs;

namespace CosmicStockapi.Controllers
{
    [Authorize]
    public class OrderController:BaseController
    {
        private readonly IOrderRepository _order;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICreateEntity _createEntity;
        private readonly IEmailSender _emailSender;
    
        public OrderController(IOrderRepository order, IMapper mapper, IUnitOfWork unitOfWork, UserManager<AppUser> userManager, ICreateEntity createEntity,IEmailSender emailSender)
        {
            _createEntity = createEntity;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _mapper = mapper;
            _order = order;
        }
        
        [HttpGet("{Id}")]
        public async Task<ActionResult<IEnumerable<ActualOrderDTO>>> GetOrderAsync(int Id)
        {
            //Retrieving a single order
            await _unitOfWork.Repository<OrderedProducts>().GetAll(x =>x.ActualOrderId == Id,null,"imageUrl");
            var order = await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.Id == Id,x=>x.OrderBy(x=>x.OrderDate),"OrderedProducts,ShippingAddress");
            var data = _mapper.Map<IEnumerable<ActualOrder>,IEnumerable<ActualOrderDTO>>(order);
            foreach(var item in data)
            {
               await _order.UpdateOrderedItemsData(Id,item.OrderedProducts);
            }
            return Ok(data);
        }

        [HttpGet("PurchaseInfo/{Id}")]
        public async Task<ActionResult<ActualOrderDTO>> GetPurchaseOrder(int Id)
        {
            //Retrieving the order a user just placed and sending an email
            var orders = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.Id == Id,"BillingAddress,ShippingAddress,OrderedProducts");
            var EmailBody = await _order.EmailToSend(orders,orders.Subtotal,orders.Tax);
            await _emailSender.SendEmailAsync(orders.Email,"Order Confirmation",EmailBody);
            return Ok(_mapper.Map<ActualOrder,ActualOrderDTO>(orders));
        }

        [HttpGet("User")]
        public async Task<ActionResult<PagerList<ActualOrderDTO>>> GetActualOrders([FromQuery]PageParams pageParams)
        {
            //Retrieving all the order for the signed in user.
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            await _unitOfWork.Repository<OrderedProducts>().GetAll(x=>x.AppUser == currentUserId,null,"imageUrl");
            var Orders = await _unitOfWork.Repository<ActualOrder>().GetAllParams(pageParams,x =>x.Email == currentUserId,x =>x.OrderByDescending(x =>x.OrderDate),"OrderedProducts,ShippingAddress");
            Response.AddPaginationHeader(Orders.CurrentPage,Orders.PageSize,Orders.TotalCount,Orders.TotalPages);
            return Ok(_mapper.Map<PagerList<ActualOrder>,PagerList<ActualOrderDTO>>(Orders));
        }


        // [HttpGet("OrderSort")]
        // public async Task<ActionResult<IEnumerable<ActualOrderDTO>>> GetActualOrdersWithinTimeframe([FromQuery]PageParams pageParams,string Search)
        // {
        //     var Orders = await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderDate >= DateTimeOffset.Now.AddDays(-30),x=>x.OrderByDescending(x=>x.OrderDate),"OrderedItems,ShippingAddress");
        //     if(!string.IsNullOrEmpty(pageParams.Sort))
        //     {
        //         switch (pageParams.Sort)
        //         {
        //             case "order3M": Orders = await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderDate >= DateTimeOffset.Now.AddDays(-90),x=>x.OrderByDescending(x=>x.OrderDate),"OrderedItems,ShippingAddress");
        //             break;
        //             case "order6M": Orders = await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderDate >= DateTimeOffset.Now.AddDays(-180),x=>x.OrderByDescending(x=>x.OrderDate),"OrderedItems,ShippingAddress");
        //             break;
        //             default: Orders = await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderDate >= DateTimeOffset.Now.AddDays(-30),x=>x.OrderByDescending(x=>x.OrderDate),"OrderedItems,ShippingAddress"); break;
        //         }
        //     }
        //     if(!string.IsNullOrEmpty(Search))
        //     {
        //         Orders = await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderedProducts.Any(x=>x.Name.ToLower().Contains(Search)),x=>x.OrderByDescending(x=>x.OrderDate > DateTimeOffset.Now.AddDays(-30)));
        //         if(pageParams.Sort != null)
        //         {
        //             switch (pageParams.Sort)
        //             {
        //                 case "order3M": await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderedProducts.Any(x=>x.Name.ToLower().Contains(Search)),x=>x.OrderByDescending(x=>x.OrderDate > DateTimeOffset.Now.AddDays(-90)));
        //                 break;
        //                 case "order6M":await _unitOfWork.Repository<ActualOrder>().GetAll(x =>x.OrderedProducts.Any(x=>x.Name.ToLower().Contains(Search)),x=>x.OrderByDescending(x=>x.OrderDate > DateTimeOffset.Now.AddDays(-180)));
        //                 break;
        //             }
        //         }
                
        //     } 
        //     return Ok(_mapper.Map<IEnumerable<ActualOrder>,IEnumerable<ActualOrderDTO>>(Orders));
        // }




        [HttpPost]
        public async Task<ActionResult<ActualOrder>> CreateOrder(OrderDTO orderDTO)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if(orderDTO.ShippingAddress.AppUser == null || orderDTO.BillingAddress.AppUser == null) 
            {
                orderDTO.ShippingAddress.AppUser = currentUser;
                orderDTO.BillingAddress.AppUser = currentUser;
                orderDTO.ShippingAddress.AddressType = "shipping";
                orderDTO.BillingAddress.AddressType = "billing";
            }
            var Address = _mapper.Map<UserAddressDTO, UserAddress>(orderDTO.ShippingAddress);
            if(orderDTO.ShippingAddress.SaveAddressInfo == true)
            {
                await _createEntity.CreateAddressToSaveOrNot(currentUser,Address);
            }
            var BillAddress = _mapper.Map<UserAddressDTO, UserAddress>(orderDTO.BillingAddress);
            if(orderDTO.BillingAddress.SaveAddressInfo == true)
            {
                await _createEntity.CreateAddressToSaveOrNot(currentUser,BillAddress);
            }
            var status = StaticInfo.Pending;
            /*checking to see if an order address in the database matches the same order address that is comming in
            if thats not the case, go to the next code block and create a new order*/
            var orderAddress = await _unitOfWork.Repository<UserAddress>().GetAll(x=>x.AppUser == Address.AppUser);
            if(orderAddress.Count() != 0)
            {
                var shipping = orderAddress.FirstOrDefault(x=>x.AddressType == Address.AddressType);
                var newShippingAddress = new UserAddress();
                if(shipping is not null)
                {
                   var updatedShipping = await _createEntity.UpdateUserOrderAddress(shipping,Address);
                   newShippingAddress = updatedShipping;
                }
                else
                {
                    newShippingAddress = Address;
                }
               
                var billing = orderAddress.FirstOrDefault(x=>x.AddressType == BillAddress.AddressType);
                var newBillAddress = new UserAddress();
                if(billing is not null)
                {
                    var updatedBilling = await _createEntity.UpdateUserOrderAddress(billing,BillAddress);
                    newBillAddress = updatedBilling;
                }
                else
                {
                    newBillAddress = BillAddress;
                }

                if(orderDTO.ShippingAddress.ShippingIsBilling == true)
                {
                    var order = await _order.CreateOrderAsync(currentUser,orderDTO.Id,newShippingAddress,newShippingAddress,orderDTO.ShippingHandlingPrice,orderDTO.Tax,status);
                    if (order == null) return BadRequest(new ApiErrorResponse(400, "Problem Creating Order!"));
                    return Ok(order);
                }
                if(newShippingAddress is not null && newBillAddress is null)
                {
                    var order = await _order.CreateOrderAsync(currentUser,orderDTO.Id,newShippingAddress,BillAddress,orderDTO.ShippingHandlingPrice,orderDTO.Tax,status);
                    if (order == null) return BadRequest(new ApiErrorResponse(400, "Problem Creating Order!"));
                    return Ok(order);
                }
                if(newBillAddress is not null && newShippingAddress is null)
                {
                    var order = await _order.CreateOrderAsync(currentUser,orderDTO.Id,Address,newBillAddress,orderDTO.ShippingHandlingPrice,orderDTO.Tax,status);
                    if (order == null) return BadRequest(new ApiErrorResponse(400, "Problem Creating Order!"));
                    return Ok(order);
                }
                if(newShippingAddress is not null && newBillAddress is not null)
                {
                    var order = await _order.CreateOrderAsync(currentUser,orderDTO.Id,newShippingAddress,newBillAddress,orderDTO.ShippingHandlingPrice,orderDTO.Tax,status);
                    if (order == null) return BadRequest(new ApiErrorResponse(400, "Problem Creating Order!"));
                    return Ok(order);
                }
            }
            if(orderDTO.ShippingAddress.ShippingIsBilling == true)
            {
                var order = await _order.CreateOrderAsync(currentUser,orderDTO.Id,Address,Address,orderDTO.ShippingHandlingPrice,orderDTO.Tax,status);
                if (order == null) return BadRequest(new ApiErrorResponse(400, "Problem Creating Order!"));
                return Ok(order);
            }
            var Order = await _order.CreateOrderAsync(currentUser,orderDTO.Id,Address,BillAddress,orderDTO.ShippingHandlingPrice,orderDTO.Tax,status);
            if (Order == null) return BadRequest(new ApiErrorResponse(400, "Problem Creating Order!"));
            return Ok(Order);
        }
    }
}