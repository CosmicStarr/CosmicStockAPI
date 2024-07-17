
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Models.DTOs;

namespace Data.Classes
{
    public class ApplicationUser:IApplicationUser
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _token;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public ApplicationUser(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, 
            ITokenService Token,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _token = Token;
            _signInManager = signInManager;
            _userManager = userManager;
        }



        public async Task<IdentityResult> ForgotPassword(ForgotPasswordDTO user)
        {
            var currentUser = await _userManager.FindByEmailAsync(user.Email);
            if(currentUser == null || !await _userManager.IsEmailConfirmedAsync(currentUser)) return null;
            var tokenToGenerate = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
            var forgotPasswordUrl = new UriBuilder(_configuration["ReturnPath:resetPassword"]);
            var uriQuery = HttpUtility.ParseQueryString(forgotPasswordUrl.Query);
            uriQuery["token"] = tokenToGenerate;
            uriQuery["email"] = currentUser.Email;
            forgotPasswordUrl.Query = uriQuery.ToString();
            var urlMessage = forgotPasswordUrl.ToString();
            string text = $"<div style=\"text-align: center; width:100%; padding:16px;\"><div style=\"font-family: Arial, Helvetica, sans-serif; font-size:18px;text-align: center;border-style:solid; padding:16px; width:60%; margin:auto; border-width:.1px; border-color:rgb(0, 0, 0,0.1);\"><h2 style=\"margin-bottom:16px;\">Password Reset</h2><p style=\"margin-bottom: 16px;\">If you lost your or don't remember your password, click here to reset it!</p> <a style=\"padding:16px; text-shadow: .1rem .1rem .3rem rgb(17, 17, 17); box-shadow:.1rem .1rem .3rem rgb(0, 0, 0); background-color: rgb(0, 255, 255); color: rgb(255, 255, 255); text-decoration: none;\"href=\"{urlMessage}\"> Reset Your Password </a> <p style=\"margin-top:16px;\"> If you did not request this link, disregard this email. Only a person or persons with access to your email can request this link and reset your password.</p></div></div>";
            text = "<html lang=\"en\"><body>" + Environment.NewLine + text + Environment.NewLine + "</body></html>";
            await _emailSender.SendEmailAsync(currentUser.Email,"Reset Your Password",text);
            return IdentityResult.Success;
        } 

        public async Task<IdentityResult> ResetPassword(ResetPasswordDTO resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if(user == null) return null;
            var role = await _userManager.GetRolesAsync(user);
            var Claim = await _userManager.GetClaimsAsync(user); 
            var newUser = await _userManager.ResetPasswordAsync(user,resetPassword.Token,resetPassword.NewPassword);
            if(newUser.Succeeded) return newUser;
            return null;
        }  

        public async Task<LoginDTO> ConfirmEmailInformation(ConfirmEmailInfoModel confirmEmailInfo)
        {
            var user = await _userManager.FindByIdAsync(confirmEmailInfo.UserId);
            if(user is null)
            {
                return null;
            }
            var results = await _userManager.ConfirmEmailAsync(user,confirmEmailInfo.Token);
            if(results.Succeeded) 
            {
                return new LoginDTO
                {
                    Email = user.Email,
                    Token = await _token.CreateToken(user),
                    EmailConfirmed = user.EmailConfirmed
                };
            }
            else
            {
                var registerDTOErrorList = new LoginDTO();
                var badInfo = registerDTOErrorList.LoginErrors = new List<IdentityError>();
                foreach(var item in results.Errors)
                {
                    var errors = new IdentityError
                    {
                        Code = item.Code,
                        Description = item.Description
                    };
                    badInfo.Add(errors);
                }
               return registerDTOErrorList;

            }
        }


        
        public async Task<LoginDTO> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if(user == null) return null;
            var result = await _signInManager.PasswordSignInAsync(user,loginDTO.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return null;
            }
            return new LoginDTO
            {
                Email = loginDTO.Email,
                Token = await _token.CreateToken(user),
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public async Task<RegisterDTO> SiginUp(RegisterDTO registerModelDTO)
        {
            var newUser = new AppUser()
            {
                Email = registerModelDTO.Email,
                UserName = registerModelDTO.Email,
                SecurityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(64))
            };
            var results = await _userManager.CreateAsync(newUser,registerModelDTO.Password);
            if(results.Succeeded)
            {
                if(newUser.Email == "NormandJean1@yahoo.com")
                {         
                    await _roleManager.CreateAsync(new IdentityRole(StaticInfo.AdminRole));
                    await _userManager.AddToRolesAsync(newUser,new[] {StaticInfo.AdminRole});
                    var claim = new Claim("JobDepartment", StaticInfo.Job);
                    await _userManager.AddClaimAsync(newUser,claim);
                }
                var user = await _userManager.FindByEmailAsync(newUser.Email);
                var tokenToGenerate =  await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmEmailUrl = new UriBuilder(_configuration["ReturnPath:confirmEmail"]);
                var uriQuery = HttpUtility.ParseQueryString(confirmEmailUrl.Query);
                uriQuery["token"] = tokenToGenerate;
                uriQuery["userId"] = user.Id;
                confirmEmailUrl.Query = uriQuery.ToString();
                var urlMessage = confirmEmailUrl.ToString();
                string text = $"<div style=\"text-align: center; width:100%; padding:16px;\"><div style=\"font-family: Arial, Helvetica, sans-serif; font-size:18px;text-align: center;border-style:solid; padding:16px; width:60%; margin:auto; border-width:.1px; border-color:rgb(0, 0, 0,0.1);\"><h2 style=\"margin-bottom:16px;\">Confirm your Email</h2><p style=\"margin-bottom: 16px;\"> Follow the link below to verify your email. The link is vaild for only 4 hours.</p> <div style=\"padding:16px; margin-top:16px; margin-bottom:16px; text-shadow: .1rem .1rem .3rem rgb(17, 17, 17); box-shadow:.1rem .1rem .3rem rgb(0, 0, 0); background-color: rgb(0, 255, 255); color: rgb(255, 255, 255); text-decoration: none; border:none;\"> <a href=\"{urlMessage}\" style=\"text-decoration: none;\"> Confirm email </a></div></div>";
                text = "<html lang=\"en\"><body>" + Environment.NewLine + text + Environment.NewLine + "</body></html>";
                await _emailSender.SendEmailAsync(user.Email,"Confirm your email!",text);
            } 
            else
            {
                var registerDTOErrorList = new RegisterDTO();
                var badInfo = registerDTOErrorList.RegisterErrors = new List<IdentityError>();
                foreach(var item in results.Errors)
                {
                    var errors = new IdentityError
                    {
                        Code = item.Code,
                        Description = item.Description
                    };
                    badInfo.Add(errors);
                }
               return registerDTOErrorList;
            }

            //Add some logic for user roles in the future 
            return new RegisterDTO
            {
                Email = newUser.Email,
                Token =  await _token.CreateToken(newUser)
            };
        }

        public async Task<IdentityResult> SecondRequestToConfirmEmail(string User)
        {
            var user = await _userManager.FindByEmailAsync(User);
            if(user is null)
            {
                return null;
            }
            var tokenToGenerate =  await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmEmailUrl = new UriBuilder(_configuration["ReturnPath:confirmEmail"]);
            var uriQuery = HttpUtility.ParseQueryString(confirmEmailUrl.Query);
            uriQuery["token"] = tokenToGenerate;
            uriQuery["userId"] = user.Id;
            confirmEmailUrl.Query = uriQuery.ToString();
            var urlMessage = confirmEmailUrl.ToString();
            string text = $"<div style=\"text-align: center; width:auto; padding:16px;\"><div style=\"font-family: Arial, Helvetica, sans-serif; font-size:18px;text-align: center;border-style:solid; padding:16px; width:60%; margin:auto; border-width:.1px; border-color:rgb(0, 0, 0,0.1);\"><h2 style=\"margin-bottom:16px;\">Confirm your Email</h2><p style=\"margin-bottom: 16px;\"> Follow the link below to verify your email. The link is vaild for only 4 hours.</p> <div style=\"padding:16px; margin-top:16px; margin-bottom:16px; text-shadow: .1rem .1rem .3rem rgb(17, 17, 17); box-shadow:.1rem .1rem .3rem rgb(0, 0, 0); background-color: rgb(0, 255, 255); color: rgb(255, 255, 255); text-decoration: none; border:none;\"> <a href=\"{urlMessage}\" style=\"text-decoration: none;\"> Confirm email </a></div></div>";
            text = "<html lang=\"en\"><body>" + Environment.NewLine + text + Environment.NewLine + "</body></html>";
            await _emailSender.SendEmailAsync(user.Email,"Confirm your email!",text);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> BeginRequestToChangeEmail(string User)
        {
            var user = await _userManager.FindByEmailAsync(User);
            if(user is null)
            {
                return null;
            }
            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                //Change if you run into errors
                return IdentityResult.Failed();
            }
            if(user.ChangeEmailCount > 1)
            {
                //Create a wait time logic. Ex: if a user already changed their email, that user has to wait one day to do it again.
                if(user.GetWaitTime().Duration().Days !> 1)
                {
  
                  
                }
            }
            var tokenToGenerate =  await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmEmailUrl = new UriBuilder(_configuration["ReturnPath:requestEmailChange"]);
            var uriQuery = HttpUtility.ParseQueryString(confirmEmailUrl.Query);
            uriQuery["token"] = tokenToGenerate;
            uriQuery["userId"] = user.Id;
            confirmEmailUrl.Query = uriQuery.ToString();
            var urlMessage = confirmEmailUrl.ToString();
            string text = $"<div style=\"text-align: center; width:auto; padding:16px;\"><div style=\"font-family: Arial, Helvetica, sans-serif; font-size:18px;text-align: center;border-style:solid; padding:16px; width:60%; margin:auto; border-width:.1px; border-color:rgb(0, 0, 0,0.1);\"><h2 style=\"margin-bottom:16px;\">Verify Your Request!</h2><p style=\"margin-bottom: 16px;\"> Hello! We received a request to change the email associated with this account. If you did not make this request, disreguard the email!. Otherwise, the link  below is vaild for only 4 hours.</p> <div style=\"padding:16px; margin-top:16px; margin-bottom:16px; text-shadow: .1rem .1rem .3rem rgb(17, 17, 17); box-shadow:.1rem .1rem .3rem rgb(0, 0, 0); background-color: rgb(0, 255, 255); color: rgb(255, 255, 255); border:none;\"><a href=\"{urlMessage}\" style=\"text-decoration: none;\"> Change Your Email </a></div></div>";
            text = "<html lang=\"en\"><body>" + Environment.NewLine + text + Environment.NewLine + "</body></html>";
            await _emailSender.SendEmailAsync(user.Email,"Change Email!",text);
            return IdentityResult.Success;
        }
 
        public async Task<Object> ActualRequestToChangeEmail(string User, EmailChangeModel emailChangeModel)
        {
            var user = await _userManager.FindByEmailAsync(User);
            var tokenToGenerate = await _userManager.GenerateChangeEmailTokenAsync(user,emailChangeModel.NewEmail);
            var infoToReturn = await _userManager.ChangeEmailAsync(user,emailChangeModel.NewEmail,tokenToGenerate);
            if(!infoToReturn.Succeeded)
            {
                return IdentityResult.Failed(infoToReturn.Errors.ToArray());
            }
            else
            {
                var result = await _signInManager.PasswordSignInAsync(user,emailChangeModel.CurrentPassword, isPersistent: false, lockoutOnFailure: false);
                if(!result.Succeeded)
                {
                    return IdentityResult.Failed();
                }
                user.UserName = emailChangeModel.NewEmail;
                user.NormalizedUserName = emailChangeModel.NewEmail;
                var addInfo = await _unitOfWork.Repository<UserAddressToSaveOrNot>().GetAll(x=>x.AppUser == user.Email);
                if(addInfo.Count() != 0)
                {
                    foreach(var item in addInfo)
                    {
                        item.AppUser = emailChangeModel.NewEmail;
                    }
                }
                var wishInfo = await _unitOfWork.Repository<WishedProducts>().GetAll(x=>x.User == user.Email);
                if(wishInfo.Count() != 0)
                {
                    foreach(var item in wishInfo)
                    {
                        item.User = emailChangeModel.NewEmail;
                    }
                }
                var rateInfo = await _unitOfWork.Repository<ProductRating>().GetAll(x=>x.User == user.Email);
                if(rateInfo.Count() != 0)
                {
                    foreach(var item in rateInfo)
                    {
                        item.User = emailChangeModel.NewEmail;
                    }
                }
                var refundInfo = await _unitOfWork.Repository<RefundModel>().GetAll(x=>x.currentUser == user.Email);
                if(refundInfo.Count() != 0)
                {
                    foreach(var item in refundInfo)
                    {
                        item.currentUser = emailChangeModel.NewEmail;
                    }
                }
                var cartSessionInfo = await _unitOfWork.Repository<ShoppingCartSessionId>().GetFirstOrDefault(x=>x.ApplicationUser == user.Email);
                if(cartSessionInfo is not null)
                {
                    cartSessionInfo.ApplicationUser = emailChangeModel.NewEmail;
                }
                var orderInfo = await _unitOfWork.Repository<ActualOrder>().GetAll(x=>x.Email == user.Email);
                if(orderInfo.Count() != 0)
                {
                    foreach(var item in orderInfo)
                    {
                        item.Email = emailChangeModel.NewEmail;
                    }
                }
                user.ChangeEmailCount ++;
                user.ChangeEmailDateInfo = DateTime.Now;
                await _unitOfWork.Complete();
            }
            var nuUser = await _userManager.FindByEmailAsync(emailChangeModel.NewEmail);
            return new EmailChangeModel
            {
                NewEmail = nuUser.Email,
                token = await _token.CreateToken(nuUser)
            };
        }

        public async Task<UserInformation> PatchUserRegisterInfo(string user,UserInformation userInformation)
        {
            var appUser = await _unitOfWork.Repository<AppUser>().GetFirstOrDefault(x=>x.Email == user);
            if(appUser is not null)
            {
                if(await _userManager.CheckPasswordAsync(appUser,userInformation.CurrentPassword))
                {
                    await _userManager.ChangePasswordAsync(appUser,userInformation.CurrentPassword,userInformation.NewPassword);
                    var results = await _userManager.UpdateAsync(appUser);
                    if(!results.Succeeded)
                    {
                        return null;
                    }
                    return new UserInformation
                    {
                        Email = user,
                        Token = await _token.CreateToken(appUser),
                        passwordInfo = true
                    };
                }
                else
                {
                    return new UserInformation
                    {
                        passwordInfo = false
                    };
                }
            }
            return null;
        }
    }
}