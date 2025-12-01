using Microsoft.AspNetCore.Mvc;

namespace FiitFlowReactApp.Server.Controllers
{
    public class StudentSubjectsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
