using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.DTOs;

namespace Data.Interfaces
{
    public interface IOrderRepository
    {
        Task<ActualOrder> CreateOrderAsync(string Email,string CartId,UserAddress address,UserAddress billingAddress,decimal deliveryPrice, decimal tax,string status);
        Task UpdateOrderStatus(int Id, string status);
        Task UpdateOrderedItemsData(int Id,IEnumerable<OrderedProductsDTO> orderedProductsDTO);
        Task<string> EmailToSend(ActualOrder actualOrder, Decimal subTotal, Decimal tax);
    }
}