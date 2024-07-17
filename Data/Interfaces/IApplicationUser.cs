using Microsoft.AspNetCore.Identity;
using Models.DTOs;

namespace Data.Interfaces
{
    public interface IApplicationUser
    {
        Task<RegisterDTO> SiginUp(RegisterDTO registerModelDTO);
        Task<LoginDTO> Login(LoginDTO loginDTO);
        Task<IdentityResult> ForgotPassword(ForgotPasswordDTO user);
        Task<IdentityResult> ResetPassword(ResetPasswordDTO resetPassword);
        Task<IdentityResult> BeginRequestToChangeEmail(string User);
        Task<Object> ActualRequestToChangeEmail(string User, EmailChangeModel emailChangeModel);
        Task<IdentityResult> SecondRequestToConfirmEmail(string User);
        Task<UserInformation> PatchUserRegisterInfo(string user,UserInformation registerDTO);
        Task<LoginDTO> ConfirmEmailInformation(ConfirmEmailInfoModel confirmEmailInfo);
    }
}