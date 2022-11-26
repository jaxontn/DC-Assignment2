using Microsoft.AspNetCore.Mvc;

namespace WebGUI.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "User";
            return View();
        }
    }
}
