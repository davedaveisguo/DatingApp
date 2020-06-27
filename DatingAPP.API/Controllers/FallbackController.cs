using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace DatingAPP.API.Controllers
{
    public class FallbackController: Controller
    {
        // view support
         public IActionResult Index()
         {
             return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
             "wwwroot", "index.html"),"text/HTML");
         }
    }
}