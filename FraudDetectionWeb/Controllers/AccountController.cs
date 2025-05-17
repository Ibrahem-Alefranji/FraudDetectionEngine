using FraudDetectionWeb.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FraudDetectionWeb.Services;

namespace FraudDetectionWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult AccessDenied()
        {
            return View();
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
					new Claim("IsAdmin", result.IsAdmin.ToString()),
					new Claim(ClaimTypes.Role, result.IsAdmin ? "admin" : "subscriber"),
					new Claim("SubscribeId", GetSubscribIdByUserId(result.Id, result.IsAdmin))
				};

				var identity = new ClaimsIdentity(claims, "4TpayAuth");
				var principal = new ClaimsPrincipal(identity);

				// Sign in
				await HttpContext.SignInAsync("4TpayAuth", principal);

				return Redirect("/Transactions/Index");
			}
			return View();
        }

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync("4TpayAuth");

			return RedirectToAction("Login");
		}
		private string GetSubscribIdByUserId(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return string.Empty;
            }

            var user = new SubscriptionService(_configuration);
            var result = user.GetSingleByUserId(userId);
            return result != null ? result.Id.ToString() : string.Empty;
        }
    }
}
