using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SparklingHome.Controllers
{
    public class PromotionController : Controller
    {

        private const string TopicARN = "arn:aws:sns:us-east-1:042655344227:SparklingHomePromotions";

        private List<string> getKeysConnection()
        {
            List<string> keyList = new List<string>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();

            keyList.Add(configuration["keys:key1"]);
            keyList.Add(configuration["keys:key2"]);
            keyList.Add(configuration["keys:key3"]);

            return keyList;
        }

        public IActionResult AdminBroadcast(string? publishSuccessMessage)
        {
            if (publishSuccessMessage != null)
            {
                TempData["publishSuccessMessage"] = publishSuccessMessage;
            }
            return View();
        }

        public IActionResult CustomerSubscription(string? subscriptionSuccessMessage)
        {
            if (subscriptionSuccessMessage != null)
            {
                TempData["subscriptionSuccessMessage"] = subscriptionSuccessMessage;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeToNewsletter(string SubscriptionEmail)
        {
            List<string> keyValues = getKeysConnection();

            var SNSConnection = new AmazonSimpleNotificationServiceClient(
                keyValues[0],
                keyValues[1],
                keyValues[2],
                RegionEndpoint.USEast1
            );

            try
            {
                bool IsPremium = Request.Form["IsPremium"] == "on";

                SubscribeRequest request = new SubscribeRequest
                {
                    TopicArn = TopicARN,
                    Protocol = "email",
                    Endpoint = SubscriptionEmail,
                    Attributes = new Dictionary<string, string>
                    {
                        { "FilterPolicy", IsPremium == true ? 
                        "{\"premium\":[\"yes\"]}" : 
                        "{\"premium\":[\"no\"]}" }
                    }
                };

                await SNSConnection.SubscribeAsync(request);

                return RedirectToAction(
                    "CustomerSubscription",
                    "Promotion",
                    new { subscriptionSuccessMessage = "Thanks for subscribing to our newsletter! Kindly check your email to complete the confirmation process" }
                );

            }
            catch (AmazonSimpleNotificationServiceException exception)
            {
                return RedirectToAction(
                    "CustomerSubscription",
                    "Promotion",
                    new { errorMessage = "Whoops! We ran into an error while trying to sign you up. Please try again later." }
                );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(string PromotionSubject, string PromotionDetails)
        {
            List<string> keyValues = getKeysConnection();

            var SNSConnection = new AmazonSimpleNotificationServiceClient(
                keyValues[0],
                keyValues[1],
                keyValues[2],
                RegionEndpoint.USEast1
            );

            try
            {
                bool IsPremium = Request.Form["IsPremium"] == "on";

                PublishRequest request = new PublishRequest
                {
                    TopicArn = TopicARN,
                    Subject = PromotionSubject,
                    Message = PromotionDetails,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue> {
                        { 
                            "premium",
                            new MessageAttributeValue { 
                                DataType = IsPremium ? "String" : "String.Array",
                                StringValue = IsPremium ? "yes" : "[\"yes\", \"no\"]"
                            }
                        }
                    }
                };

                await SNSConnection.PublishAsync(request);

                return RedirectToAction(
                    "AdminBroadcast",
                    "Promotion",
                    new { publishSuccessMessage = "Promotion created! Subscribers will receive the latest offers via email" }
                );
            }
            catch (AmazonSimpleNotificationServiceException exception)
            {
                return RedirectToAction(
                    "AdminBroadcast",
                    "Promotion",
                    new { errorMessage = "Whoops! We ran into an error while trying to create the promotion. Please try again later." }
                ) ;
            }
        }

    }
}
