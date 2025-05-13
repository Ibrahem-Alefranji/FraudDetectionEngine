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
            var trans = new TransactionsService(_configuration);

            var result = trans.GetAll();
            return View(result);
        }
    }
}
