using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IShoppingCartRepo
    {
        #nullable enable
        Task<ShoppingCart> GetCartAsync(string CartId,string? user);
        Task<ShoppingCart> UpdateCartAsync(ShoppingCart Cart,string? user);
        Task<bool> DeleteCartAsync(string CartId,string? user);
        #nullable disable
    }
}