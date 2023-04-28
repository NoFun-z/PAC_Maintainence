using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiagaraCollegeProject.Models;
using System.Diagnostics;
using NiagaraCollegeProject.Data;
namespace NiagaraCollegeProject.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PAC_Context _context;

        public HomeController(ILogger<HomeController> logger, PAC_Context context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

		public IActionResult DownloadFile(string fileName)
		{
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
			var fileExists = System.IO.File.Exists(filePath);

			if (!fileExists)
			{
				return NotFound();
			}

			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

			return File(fileStream, "application/octet-stream", fileName);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}