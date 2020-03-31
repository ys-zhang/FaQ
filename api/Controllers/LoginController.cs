using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using api.Models;
using api.Controllers.AuthUtil;

namespace api.Controllers
{
    [Route("faq/[controller]")]
    [ApiController]
    [EnableCors("Debug")]
    public class LoginController: Controller
    {
        private readonly JwtEncoder _jwtEncoder;

        private static readonly AdminUser TestAdminUser = new AdminUser
        {
            Username = "admin",
            Password = "password",
            Roles = new List<AdminUserRole> { AdminUserRole.Admin }
        };
        
        private static readonly AdminUser TestReadonlyUser = new AdminUser
        {
            Username = "readonly",
            Password = "password",
            Roles = new List<AdminUserRole> { AdminUserRole.Readonly }
        };
        
        public LoginController(JwtEncoder jwtEncoder)
        {
            _jwtEncoder = jwtEncoder;
        }
        
        public class LoginResponse
        {
            public string Username { get; set; }
            public string AuthToken { get; set; }
            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
            public List<AdminUserRole> Roles { get; set; }
        }
        
        [HttpPost]
        public ActionResult<LoginResponse> Index([FromForm] string username, [FromForm] string password)
        {
            if (username == TestAdminUser.Username && password == TestAdminUser.Password)
            {
                var token = _jwtEncoder.CreateToken(TestAdminUser);
                return new LoginResponse
                {
                    Username = username,
                    AuthToken = token,
                    Roles = TestAdminUser.Roles
                };
            } 
            if (username == TestReadonlyUser.Username && password == TestReadonlyUser.Password)
            {
                var token = _jwtEncoder.CreateToken(TestReadonlyUser);
                return new LoginResponse
                {
                    Username = username,
                    AuthToken = token,
                    Roles = TestReadonlyUser.Roles
                };
            }  
            return NotFound($"User {username} not exits or wrong password");
        }
    }
}