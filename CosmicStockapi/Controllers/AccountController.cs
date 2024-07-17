using System.Security.Claims;
using AutoMapper;
using CosmicStockapi.ErrorHandling;
using CosmicStockapi.Extension;
using Data.Interfaces;
using Data.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs;

namespace CosmicStockapi.Controllers
{
    public class AccountController:BaseController
    {
        private readonly IApplicationUser _user;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICreateEntity _create;
        private readonly IUnitOfWork _unitOfWork;
        public AccountController(IApplicationUser user, UserManager<AppUser> userManager,IMapper mapper,ICreateEntity create,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _create = create;
            _mapper = mapper;
            _userManager = userManager;
            _user = user;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterDTO>> SignInUser(RegisterModel register)
        {
            if(await _userManager.Users.AnyAsync(x =>x.Email == register.Email))return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"This email already exist in our database!"}});
            var mappedInfo = _mapper.Map<RegisterModel,RegisterDTO>(register);
            var info = await _user.SiginUp(mappedInfo);
            /*Create some logic in the future to count the amount of registered users
            per day so you can do the big data stuff much easier*/
            if(info == null) return BadRequest(new ApiErrorResponse(400));
            if(info.RegisterErrors is not null) 
            {                   
                var errors = new List<string>();
                foreach(var item in info.RegisterErrors)
                {
                    errors.Add(item.Description);
                }
                // var errors = string.Join(" ",info.RegisterErrors.Select(e=>e.Description));
                var badInfo = new ApiValidationResponse
                {
                    Errors = errors
                };
                return new BadRequestObjectResult(badInfo);
            }
            return Ok(info);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<LoginDTO>> LoginUser(LoginModel login)
        {
            var mappedInfo = _mapper.Map<LoginModel,LoginDTO>(login);
            var info = await _user.Login(mappedInfo);
            /*Create some logic in the future to count the amount of login users
            per day so you can do the big data stuff much easier*/
            if(info == null) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"Email or Password is incorrect!"}});
            return Ok(info);
        } 

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<IdentityResult>> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            var info = await _user.ForgotPassword(forgotPasswordDTO);
            if(info == null) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"Email does not exist!"}});
            return info;
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<IdentityResult>> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var info = await _user.ResetPassword(resetPasswordDTO);
            if(info == null) return BadRequest(new ApiErrorResponse(400,"something went wrong! Failed to reset password!"));
            return Ok(info);
        }

        [HttpGet("Address/{Id}")]
        [Authorize]
        public async Task<ActionResult<UserAddressDTO>> GetAddressById(int Id)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _unitOfWork.Repository<UserAddressToSaveOrNot>().GetFirstOrDefault(x=>x.Id == Id && x.AppUser == currentUser );
            return Ok(_mapper.Map<UserAddressToSaveOrNot,UserAddressDTO>(info));
        }


        [HttpGet("Address")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserAddressDTO>>> GetUserAddress()
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _unitOfWork.Repository<UserAddressToSaveOrNot>().GetAll(x=>x.AppUser == currentUser,null,"State");
            if(info == null) return Ok();
            var mappedInfo = _mapper.Map<IEnumerable<UserAddressToSaveOrNot>,IEnumerable<UserAddressDTO>>(info);
            return Ok(mappedInfo);
        }

        [HttpPost("Address")]
        [Authorize]
        public async Task<ActionResult<UserAddressDTO>> AddAnAddress([FromBody]UserAddressDTO userAddress)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _create.CreateUserAddress(currentUser,userAddress);
            if(info == null) return BadRequest(new ApiErrorResponse(400,"something went wrong! Your address was not saved!"));
            var mappedInfo = _mapper.Map<UserAddressToSaveOrNot,UserAddressDTO>(info);
            return Ok(mappedInfo);
        }
        
        [HttpPatch("Address")]
        [Authorize]
        public async Task<ActionResult<UserAddressDTO>> PatchAnAddress([FromBody]UserAddressDTO userAddress)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var address = _mapper.Map<UserAddressDTO, UserAddress>(userAddress);
            var info = await _create.CreateAddressToSaveOrNot(currentUser,address);
            if(info == null) return Ok();
            var mappedInfo = _mapper.Map<UserAddressToSaveOrNot,UserAddressDTO>(info);
            return Ok(mappedInfo);
        }

        [HttpDelete("Address/{Id}")]
        [Authorize]
        public async Task<ActionResult<int>> DeleteAddress(int Id)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var infoToDelete = await _create.DeleteAnAddress(Id,currentUser);
            if(infoToDelete == 0) return BadRequest(new ApiErrorResponse(400,"We ran into a problem!"));
            return Ok();
        }

        [HttpPost("WishList/{Id}")]
        [Authorize]
        public async Task<ActionResult<WishedProductDTO>> AddToWishlist(int Id)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _create.AddToWishList(Id,currentUser);
            if(info is null) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"You already added this item to your wish list!"}});
            return Ok(_mapper.Map<WishedProducts,WishedProductDTO>(info));
        }

        [HttpGet("WishList")]
        [Authorize]
        public async Task<ActionResult<PagerList<WishedProductDTO>>> GetUserWishlist([FromQuery]PageParams pageParams)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _unitOfWork.Repository<WishedProducts>().GetAllParams(pageParams,x=>x.User == currentUser);
            Response.AddPaginationHeader(info.CurrentPage,info.PageSize,info.TotalCount,info.TotalPages);
            return Ok(_mapper.Map<IEnumerable<WishedProducts>,IEnumerable<WishedProductDTO>>(info));
        }

        [HttpDelete("WishList/{Id}")]
        [Authorize]
        public async Task<ActionResult> RemovedWishedItem(int Id)
        {   
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            await _create.RemoveItemFromWishList(Id,currentUser);
            return Ok();
        } 

        [HttpGet("States")]
        public async Task<ActionResult<IEnumerable<State>>> GetAllStates()
        {
            var data = await _unitOfWork.Repository<State>().GetAll();
            return Ok(data);
        }

        [HttpPatch("UpdateUser")]
        [Authorize]
        public async Task<ActionResult<UserInformation>> UpdateUser(UserInformation registerDTO)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _user.PatchUserRegisterInfo(currentUser,registerDTO);
            if(info is null) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"Something went wrong!"}});
            if(info.passwordInfo == false) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"Your entered the wrong password!"}});
            return Ok(info);
        }

        [HttpPost("ConfirmEmail")]
        public async Task<ActionResult<IdentityResult>> ConfirmEmail(ConfirmEmailInfoModel infoModel)
        {
            var info = await _user.ConfirmEmailInformation(infoModel);
            if(info == null) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"Confirmation link expired!"}});
            if(info.LoginErrors is not null) 
            {                   
                var errors = new List<string>();
                foreach(var item in info.LoginErrors)
                {
                    if(item.Description == "Invalid token.")
                    {
                        item.Description = "Expired link!";
                    }
                    errors.Add(item.Description);
                }
                var badInfo = new ApiValidationResponse
                {
                    Errors = errors
                };
                return new BadRequestObjectResult(badInfo);
            }
            return Ok(info);
        }

        [HttpPost("EmailChange")]
        [Authorize]
        public async Task<ActionResult<IdentityResult>> RequestToChangeEmail()
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _user.BeginRequestToChangeEmail(currentUser);
            return Ok(info);
        }

        [HttpPost("RequestToChange")]
        public async Task<ActionResult<Object>> ActualRequestToChangeEmail(EmailChangeModel emailChange)
        {
            if(await _userManager.Users.AnyAsync(x =>x.Email == emailChange.NewEmail)) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"Email already exist!"}});
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _user.ActualRequestToChangeEmail(currentUser,emailChange);
            if(info.GetType() == typeof(IdentityResult))
            {
                return new BadRequestObjectResult(info);
            }
            return Ok(info);
        }

        [HttpPost("SecondRequest")]
        public async Task<ActionResult<IdentityResult>> SecondEmailRequestToConfirm()
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var info = await _user.SecondRequestToConfirmEmail(currentUser);
            if(info is null) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{"User does not Exist"}});
            if(!info.Succeeded) 
            {
                var errors = new List<string>();
                foreach(var item in info.Errors)
                {
                   errors.Add(item.Description);
                }
                return new BadRequestObjectResult(new ApiValidationResponse(errors));
            };
            return Ok(info);
        }

        [HttpPost("Refund/{Id}")]
        [Authorize]
        public async Task<ActionResult<String>> RequestARefund(int Id, RefundModelDTO refundModelDTO)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var data = await _create.RefundOrderedItem(Id,refundModelDTO,currentUser);
            if (data == "You already requested a refund for this item!") return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{data}});
            if (data == "You do not qualify for a refund. See refund policy for details!") return new BadRequestObjectResult(new ApiValidationResponse{Errors = new []{data}});
            if(data == null) return BadRequest(new ApiErrorResponse(400,"Theres a problem creating your refund request"));
            return Ok();
        } 

        
        [HttpGet("Refund/{Id}")]
        [Authorize]
        public async Task<ActionResult<RefundModelDTO>> GetRefundInfo(int Id)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var data = await _unitOfWork.Repository<RefundModel>().GetFirstOrDefault(x=>x.ProductId == Id && x.currentUser == currentUser);
            return Ok(_mapper.Map<RefundModel,RefundModelDTO>(data));
        }

        [HttpPost("CancelRefund/{Id}")]
        [Authorize]
        public async Task<ActionResult<Object>> CancelRefundRequest(int Id,[FromBody]int orderId)
        {
            var data = await _create.CancelRefundItemOrOrder(Id,orderId);
            if(data is null)
            {
                return BadRequest(new ApiErrorResponse(400,"There was a problem."));
            }
            if(data.GetType() == typeof(OrderedProducts))
            {
                return Ok();
            }
            return Ok();
        }

        [HttpPost("CancelItem/{Id}")]
        [Authorize]
        public async Task<ActionResult> CancelAnItem(int Id,[FromBody] int ItemId)
        {
            await _create.ActualCancel(Id,ItemId);
            return Ok();
        }
    }
}