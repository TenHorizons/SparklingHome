using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SparklingHome.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using SparklingHome.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SparklingHome.Services;
using Amazon.SQS.Model;

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
            List<Message> messages = new List<Message>();
            if (_signInManager.IsSignedIn(HttpContext.User))
            {
                messages = _notificationStreamService.GetMessages();
                ViewBag.Messages = messages;
            }

            return View();
        }

        [HttpGet]
        public IActionResult GetNewMessages()
        {

            var messages = _notificationStreamService.GetMessages();
            return Json(messages);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
