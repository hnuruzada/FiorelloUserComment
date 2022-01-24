using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiorelloBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles ="SuperAdmin,Admin")]
    public class DashboardController : Controller
    {   
        public IActionResult Index()
        {
            return View();
        }
    }
}
