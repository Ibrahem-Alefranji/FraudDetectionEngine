using FraudDetectionWeb.Models;
using FraudDetectionWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetectionWeb.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IConfiguration _configuration;

        public TransactionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }    
        public async Task<JsonResult> LoadData(int draw,
        int start,
        int length,
        string? search,
        float? amount,
        int? type)
        {
            var trans = new TransactionsService(_configuration);

            var result = await trans.OnGetTransactionsAsync(draw, start, length, search, amount, type);
            return Json(result.Value);
        }
    }
}
