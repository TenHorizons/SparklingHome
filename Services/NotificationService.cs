using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SparklingHome.Services
{
    public class NotificationStreamService : BackgroundService
    {

        private const string queueURL = "https://sqs.us-east-1.amazonaws.com/042655344227/SparklingHomeNotification";
        private static readonly List<Message> messages = new List<Message>();
        private static readonly HashSet<string> processedMessageIds = new HashSet<string>();


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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<string> keyValues = getKeysConnection();
            var SQSConnection = new AmazonSQSClient(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest()
                    {
                        QueueUrl = queueURL,
                        MessageAttributeNames = new List<string> { "userId" },
                        AttributeNames = new List<string> { "All" },
                        WaitTimeSeconds = 10,
                    };


                    var response = await SQSConnection.ReceiveMessageAsync(request);


                    foreach (var message in response.Messages)
                    {
                        if (messages.Select(e => e.Body).Contains(message.Body))
                        {
                            continue;
                        }
                        messages.Add(message);
                        
                    }
                }
                catch (AmazonSQSException ex)
                {
                    var x = ex;
                    throw new AmazonSQSException("error while fetching messages");
                }
            }
        }

        public List<Message> GetMessages()
        {
            return messages;
        }
    }
}
