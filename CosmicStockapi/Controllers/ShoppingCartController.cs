using System.Security.Claims;
using Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace CosmicStockapi.Controllers
{
    public class ShoppingCartController:BaseController
    {
        private readonly IShoppingCartRepo _shoppingCartRepo;
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartController(IShoppingCartRepo shoppingCartRepo,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _shoppingCartRepo = shoppingCartRepo;
        }

        [HttpGet]
        public async Task<ActionResult<ShoppingCart>> GetShoppingCartIdAsync(string Id)
        {
            var user = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var savedCart = await _unitOfWork.Repository<ShoppingCartSessionId>().GetFirstOrDefault(x=>x.ApplicationUser == user);
            if(savedCart is not null)
            {
                if(savedCart.GetTime().Duration().Days > 7 )
                {
                    var cartIdCheck = await _shoppingCartRepo.GetCartAsync(savedCart.ActualShoppingCartId,user);
                    if(cartIdCheck is not null)
                    {
                        if(cartIdCheck.ClientSecret is not null)
                        {
                            return Ok(cartIdCheck);
                        }
                       
                    }
                    _unitOfWork.Repository<ShoppingCartSessionId>().Remove(savedCart);
                    await DeleteCartAsync(Id);
                    await _unitOfWork.Complete();
                    return Ok(new ShoppingCart());
                }
                var cartId = await _shoppingCartRepo.GetCartAsync(savedCart.ActualShoppingCartId,user);
                return Ok(cartId);
            }
            else
            {
                if(Id != "null" && Id != "undefined" && user is not null)
                {
                    var cartInformation = new ShoppingCartSessionId
                    {
                        ActualShoppingCartId = Id,
                        ApplicationUser = user,
                    };
                    _unitOfWork.Repository<ShoppingCartSessionId>().Add(cartInformation);
                    await _unitOfWork.Complete();
                }
            }
            var CartId = await _shoppingCartRepo.GetCartAsync(Id,user);
            return Ok(CartId ?? new ShoppingCart());
        }
    
        
        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpDateShoppingCartAsync(ShoppingCart shoppingCart)
        {
            var user = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var Cart = await _shoppingCartRepo.UpdateCartAsync(shoppingCart,user);
            return Ok(Cart);
        }

        [HttpDelete]
        public async Task DeleteCartAsync(string id)
        {
            var user = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            await _shoppingCartRepo.DeleteCartAsync(id,user);
        }
    }
}