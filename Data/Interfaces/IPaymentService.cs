using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IPaymentService
    {
        Task<ShoppingCart> CreateOrUpdatePayment(string Id, string user);
        Task<ActualOrder> UpdateOrderStatusPaymentSuccessful(string paymentIntedId);
        Task<ActualOrder> UpdateOrderStatusPaymentFailed(string paymentIntedId);
    }
}