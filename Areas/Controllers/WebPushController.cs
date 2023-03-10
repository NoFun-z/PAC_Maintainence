using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiagaraCollegeProject.Data;
using WebPush;

namespace NiagaraCollegeProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WebPushController : Controller
    {
        private readonly PAC_Context _context;
        private readonly IConfiguration _configuration;

        public WebPushController(PAC_Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Members");
        }
        public async Task<IActionResult> Send(int id)
        {
            var member = await _context.Members
                .Include(e => e.Subscriptions)
                .Include(m => m.PAC)
                .Include(m => m.AcademicDivision)
                .Include(m => m.Salutation)
                .Include(d => d.MemberDocuments)
                .Where(e => e.ID == id)
                .FirstOrDefaultAsync();
            return View(member);
        }

        [HttpPost, ActionName("Send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id, string FullName)
        {
            var payload = Request.Form["payload"];
            var subs = await _context.Subscriptions
                .Where(s => s.MemberID == id)
                .ToListAsync();

            string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];

            int count = 0;
            foreach (var sub in subs)
            {
                var pushSubscription = new PushSubscription(sub.PushEndpoint, sub.PushP256DH, sub.PushAuth);
                var vapidDetails = new VapidDetails("mailto:youremail@example.com", vapidPublicKey, vapidPrivateKey);
                try
                {
                    var webPushClient = new WebPushClient();
                    webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
                    count++;
                }
                catch (WebPushException ex)
                {
                    var statusCode = ex.StatusCode;
                    TempData["message"] = "Error Sending Notification to " + FullName +
                        ". Failed with Status Code " + (int)statusCode;
                    return RedirectToAction("Index", "Members");
                }
            }

            string plural = "";
            if (count > 1)
            {
                plural = "s";
            }
            TempData["message"] = "Sent Notification to " + count +
                " Subscription" + plural + " for " + FullName;
            return RedirectToAction("Index", "Members");
        }
    }
}
