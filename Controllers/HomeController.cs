using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SparklingHome.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using SparklingHome.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using System.Security.Claims;
using SparklingHome.Services;
using System.Linq;

namespace SparklingHome.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NotificationStreamService _notificationStreamService;
        private readonly SignInManager<SparklingHomeUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, SignInManager<SparklingHomeUser> signInManager, NotificationStreamService notificationStreamService)
        {
            _logger = logger;
            _notificationStreamService = notificationStreamService;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetNewMessages()
        {

            List<Message> messagesFound = _notificationStreamService.GetMessages();
            string userUid = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = messagesFound
                .Where(m => m.MessageAttributes
                    .TryGetValue("userId", out var attribute) &&
                    attribute.StringValue == userUid
                ).ToList();

            return Json(messages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
