using Microsoft.AspNetCore.Mvc;

namespace dgt.poc.adfsdocker.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
