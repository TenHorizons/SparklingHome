using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Net;
using System.Security.Claims;

namespace SparklingHome.Controllers
{
    public class NotificationController : Controller
    {

        private const string queueURL = "https://sqs.us-east-1.amazonaws.com/042655344227/SparklingHomeNotification";


        private List<string> getKeysConnection()
        {
            List<string> keysList = new List<string>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration configure = builder.Build();

            keysList.Add(configure["keys:key1"]);
            keysList.Add(configure["keys:key2"]);
            keysList.Add(configure["keys:key3"]);

            return keysList;
        }

        public async Task<bool> QueueMessage(string reservationId, string customerId)
        {
            List<string> keyValues = getKeysConnection();
            var SQSConnection = new AmazonSQSClient(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);
            try
            {
                var request = new SendMessageRequest
                {
                    QueueUrl = queueURL,
                    MessageBody = "Your reservation " + reservationId + " has been approved",
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "userId",
                            new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = customerId
                            }
                        }
                    }
                };


                var response = await SQSConnection.SendMessageAsync(request);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonSQSException)
            {
                return false;
            }
        }

        [HttpGet]
        public async Task<JsonResult> PollMessage(string userUid)
        {
            List<string> keyValues = getKeysConnection();
            var SQSConnection = new AmazonSQSClient(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);

            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueURL,
                    WaitTimeSeconds = 10,
                    MessageAttributeNames = new List<string> { "userId" },
                    AttributeNames = new List<string> { "All" }
                };

                
                var response = await SQSConnection.ReceiveMessageAsync(request);

                var messages = response.Messages
                    .Where(m => m.MessageAttributes
                        .TryGetValue("userId", out var attribute) &&
                        attribute.StringValue == userUid
                    ).ToList();

                return Json(messages);

            }
            catch (AmazonSQSException)
            {
                return Json("");
            }


        }

    }
}
