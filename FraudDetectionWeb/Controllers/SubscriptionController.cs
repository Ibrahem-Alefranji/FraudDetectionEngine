using FraudDetectionWeb.Models;
using FraudDetectionWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetectionWeb.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly IConfiguration _configuration;

        public SubscriptionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var subs = new SubscriptionService(_configuration);

            var result = subs.GetAll();
            return View(result);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Subscription subscription)
        {
            var subs = new SubscriptionService(_configuration);
            var result = subs.Create(subscription);

            return View();
        }
    }
}
