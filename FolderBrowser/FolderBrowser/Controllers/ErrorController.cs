using Microsoft.AspNetCore.Mvc;

namespace FolderBrowser.Controllers
{
    public class ErrorController : Controller
    {
        ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        public IActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }
}
