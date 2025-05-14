using FraudDetectionWeb.Models;
using FraudDetectionWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetectionWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Update(int id)
        {
            var subs = new UserService(_configuration);
            var result = subs.GetUserByIdAsync(id);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var subs = new UserService(_configuration);
            var result = await subs.CreateUserAsync(user);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(User user)
        {
            var subs = new UserService(_configuration);
            var result = await subs.UpdateUserAsync(user);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var subs = new UserService(_configuration);
            var result = await subs.DeleteUserAsync(id);

            return RedirectToAction("Index");
        }

        public async Task<JsonResult> LoadData(int draw, int start, int length, string? search)
        {
            var trans = new UserService(_configuration);

            var result = await trans.GetAllUsersAsync(draw, start, length, search);
            return Json(result.Value);
        }
    }
}
