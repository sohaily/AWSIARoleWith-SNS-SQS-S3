using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwsIAMDeveloper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SQSController : ControllerBase
    {
        IAmazonSQS _sqlClient { get; set; }
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection sqsConfig;

        public SQSController(IAmazonSQS sqlClient, IConfiguration configuration)
        {
            this.configuration = configuration;
            sqsConfig = configuration.GetSection("AwsSQSQueueUrls");
            _sqlClient = sqlClient;
        }
        [HttpPost("SendMessageToAmazonSQS/{QueueName}/{messageBody}/{PostedBy}")]
        public async Task<int> SendMessageToAmazonSQS(string QueueName, string messageBody , string PostedBy)
        {
            var queueUrl = sqsConfig.GetSection(QueueName).Value;
            int delayTime = 0;
            var request = new SendMessageRequest
            {
                DelaySeconds = (int)TimeSpan.FromSeconds(delayTime).TotalSeconds,
                MessageAttributes = new Dictionary<String, MessageAttributeValue>
                {
                    {"PostedBy",new MessageAttributeValue{DataType="String", StringValue=PostedBy} }
                },
                MessageBody = messageBody,
                QueueUrl = queueUrl
            };
            var response = await _sqlClient.SendMessageAsync(request);
            return (int)response.HttpStatusCode;
        }
        [HttpPost("ReadMessageFromAmazonSQS/{QueueName}")]
        public async Task<List<string>> ReadMessageFromAmazonSQS(string QueueName)
        {
            var queueUrl = sqsConfig.GetSection(QueueName).Value;
            var messageList = new List<string>();
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = queueUrl;
            receiveMessageRequest.MaxNumberOfMessages = 1;
            var response = await _sqlClient.ReceiveMessageAsync(receiveMessageRequest);
            if (response.Messages.Any())
            {
                foreach (var message in response.Messages)
                {
                    messageList.Add(message.Body);
                    var deleteMessageRequest = new DeleteMessageRequest();
                    deleteMessageRequest.QueueUrl = queueUrl;
                    deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                    var result = _sqlClient.DeleteMessageAsync(deleteMessageRequest).Result;
                }
            }
            return messageList;
        }
        [HttpDelete("DeleteQueue/{queueUrl}")]
        public async Task<int> DeleteQueue(string queueUrl)
        {
            var request = new DeleteQueueRequest
            {
                QueueUrl =queueUrl
            };
            var response = await _sqlClient.DeleteQueueAsync(request);
            return (int)response.HttpStatusCode;
        }
    }
}
