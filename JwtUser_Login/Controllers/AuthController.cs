using JwtUser_Login.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtUser_Login.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("Register")]
        public ActionResult<User> Register(UserDto userDto)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            user.PasswordHash = passwordHash;
            user.UserName = userDto.Username;
            return Ok(user);

        }
        [HttpPost("Login")]
        public ActionResult<User> Login(UserDto userDto)
        {
            if (user.UserName != userDto.Username)
            {
                return BadRequest("Yanlış İsim");
            }
            if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            {
                return BadRequest("Yanlış Şifre");
            }
            string token = CreateToken(user);
            return Ok(token);

        }
        private string CreateToken(User User)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,User.UserName),
                new Claim(ClaimTypes.Role,"Admin"),
                new Claim(ClaimTypes.Role,"User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
       
    }
}
