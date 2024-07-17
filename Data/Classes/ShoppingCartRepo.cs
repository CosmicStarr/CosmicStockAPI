using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Data.Classes
{
    public class ShoppingCartRepo : IShoppingCartRepo
    {
        private readonly IDatabase _database;
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartRepo(IConnectionMultiplexer Redis,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _database = Redis.GetDatabase();
        }
        public async Task<bool> DeleteCartAsync(string CartId,string user)
        {
            var savedCart = await _unitOfWork.Repository<ShoppingCartSessionId>().GetFirstOrDefault(x=>x.ActualShoppingCartId == CartId && x.ApplicationUser == user );
            if(savedCart is not null)
            {
                _unitOfWork.Repository<ShoppingCartSessionId>().Remove(savedCart);
            }
            await _unitOfWork.Complete();
            return await _database.KeyDeleteAsync(CartId);
        }
        #nullable enable
        public async Task<Models.ShoppingCart> GetCartAsync(string CartId,string? user)
        {
        #nullable disable
            if(CartId == "undefined" || CartId == "null")
            {
                return null;
            }
            var CartData = await _database.StringGetAsync(CartId);
            return CartData.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ShoppingCart>(CartData);
        }

        public async Task<Models.ShoppingCart> UpdateCartAsync(Models.ShoppingCart Cart,string user)
        {
            var savedCart = await _unitOfWork.Repository<ShoppingCartSessionId>().GetFirstOrDefault(x=>x.ApplicationUser == user);
            if(savedCart is null && user is not null)
            {
                var cartInformation = new ShoppingCartSessionId
                {
                    ActualShoppingCartId = Cart.Id,
                    ApplicationUser = user,
                };
                _unitOfWork.Repository<ShoppingCartSessionId>().Add(cartInformation);
                await _unitOfWork.Complete();
                var UCart = await _database.StringSetAsync(Cart.Id,JsonSerializer.Serialize(Cart));
                if(!UCart) return null;
                return await GetCartAsync(Cart.Id,user);
            }
            else if(savedCart is not null)
            {   
                var UpdatedCart = await _database.StringSetAsync(savedCart.ActualShoppingCartId,JsonSerializer.Serialize(Cart));
                if(!UpdatedCart) return null;
                return await GetCartAsync(savedCart.ActualShoppingCartId,user);
            }
            else
            {
                var guestCart = await _database.StringSetAsync(Cart.Id,JsonSerializer.Serialize(Cart));
                if(!guestCart) return null;
                return await GetCartAsync(Cart.Id,user);
            }
        }
    }
}