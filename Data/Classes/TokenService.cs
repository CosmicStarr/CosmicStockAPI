using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Data.Classes
{
    public class TokenService: ITokenService
    {
         private readonly IOptions<TokenSettings> _config;
        // private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly SymmetricSecurityKey _key;
        public TokenService(IOptions<TokenSettings> Config,UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _config = Config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Value.SecretKey));
            
        }
        public async Task<string> CreateToken(AppUser appUser)
        {
            //List of who the current user claim to be! 
            var ClaimsList = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.GivenName,appUser.Id),
               new Claim(JwtRegisteredClaimNames.Email,appUser.Email),
               new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            var roles = await _userManager.GetRolesAsync(appUser);
            var claim = await _userManager.GetClaimsAsync(appUser);
            foreach(var item in claim)
            {
                ClaimsList.Add(item);
            }
            ClaimsList.AddRange(roles.Select(x => new Claim(ClaimTypes.Role.Remove(0,56),x)));
            //Creating the Credentials for the current User.
            var authKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.Value.SecretKey));
            //Describing the Token, first, sec, and third part of the token.
            var Expires = DateTime.Now.AddDays(10);
            var creds = new SigningCredentials(authKey, SecurityAlgorithms.HmacSha512Signature);
            //Creating a token handler which will create the token
            var Tinfo = new JwtSecurityToken(issuer:_config.Value.VaildIssuer,audience:_config.Value.VaildAudience,claims:ClaimsList,signingCredentials:creds,expires:Expires);
            var TokenHandler = new JwtSecurityTokenHandler();
            //returning the actual token.
            return TokenHandler.WriteToken(Tinfo);
        }
    }
}