using FraudDetectionWeb.DTOs;
using FraudDetectionWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Dapper.SqlMapper;
using System.Security.Claims;

namespace FraudDetectionWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = new UserService(_configuration);

            var result = await user.SignInAsync(request.Username, request.Password);
            if(result != null)
			{
				// Create user claims
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
					new Claim(ClaimTypes.Name, result.Username),
					new Claim(ClaimTypes.Email, result.Email ?? ""),
					new Claim(ClaimTypes.MobilePhone, result.PhoneNumber ?? ""),
					new Claim("IsAdmin", result.IsAdmin.ToString())
				};

				var identity = new ClaimsIdentity(claims, "4TpayAuth");
				var principal = new ClaimsPrincipal(identity);

				// Sign in
				await HttpContext.SignInAsync("4TpayAuth", principal);

				return Redirect("/Transactions/Index");
			}
			return View();
        }
    }
}
