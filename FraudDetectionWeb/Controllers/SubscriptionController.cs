using FraudDetectionWeb.Models;
using FraudDetectionWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FraudDetectionWeb.Controllers
{
    [Authorize(Roles = "admin")]
    public class SubscriptionController : Controller
    {
        private readonly IConfiguration _configuration;

        public SubscriptionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string query = "", string country = "", string region = "")
        {
            var subs = new SubscriptionService(_configuration);

            var result = subs.GetAll(query, country, region);
            return View(result);
        }

        public async Task<IActionResult> Create()
        {
            var user = new UserService(_configuration);
            ViewBag.UserId = new SelectList(await user.GetAllSubscribUsersAsync(), "Id", "FullName");
            return View();
        }

        public IActionResult Update(int id)
        {
            var subs = new SubscriptionService(_configuration);
            var result = subs.GetSingle(id);

            return View(result);
        }

        [HttpPost]
        public IActionResult Create(Subscription subscription)
        {
            var subs = new SubscriptionService(_configuration);
            var result = subs.Create(subscription);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(Subscription subscription)
        {
            var subs = new SubscriptionService(_configuration);
            var result = subs.Update(subscription);

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var subs = new SubscriptionService(_configuration);
            var result = subs.Delete(id);

            return RedirectToAction("Index");
        }
    }
}
