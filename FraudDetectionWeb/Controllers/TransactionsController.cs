using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace FraudDetectionWeb.Controllers
{
    [Authorize(Roles = "admin,subscriber")]
    public class TransactionsController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfiguration _configuration;

        public TransactionsController(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _accessor = accessor;
        }

        public IActionResult Index()
        {
            return View();
        }    
        public async Task<JsonResult> LoadData(int draw, int start, int length, string? search, float? amount, int? type)
        {
            var trans = new TransactionsService(_configuration);

            var result = await trans.OnGetTransactionsAsync(draw, start, length, search, amount, type, GetSubscribeId());
            return Json(result.Value);
        }

        public bool IsInRole(string role)
        {
            return _accessor.HttpContext?.User.IsInRole(role) ?? false;
        }

        public int? GetSubscribeId()
        {
            var principal = _accessor.HttpContext?.User;
            var claim = principal.FindFirst("SubscribeId");
            return !string.IsNullOrEmpty(claim?.Value) ? int.Parse(claim?.Value) : null;
        }
    }
}
