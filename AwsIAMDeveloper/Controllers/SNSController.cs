using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AwsIAMDeveloper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SNSController : ControllerBase
    {
        IAmazonSimpleNotificationService _snsClient { get; set; }
        public SNSController(IAmazonSimpleNotificationService snsClient)
        {
          
            this._snsClient = snsClient;
        }
        [HttpPost("CreateTopic/{topicName}/{displayValue}")]
        public async Task<int> CreateTopic(string topicName,string displayValue)
        {
            var topicRequest = new CreateTopicRequest
            {
                Name = topicName
            };
            var topicResponse = await _snsClient.CreateTopicAsync(topicRequest);
            var topiAttrRequest = new SetTopicAttributesRequest
            {
                TopicArn = topicResponse.TopicArn,
                AttributeName="DisplayName",
                AttributeValue=displayValue
            };
            await _snsClient.SetTopicAttributesAsync(topiAttrRequest);
            return (int)topicResponse.HttpStatusCode;
        }
        [HttpPost("CreateEmailSubscription/{topicName}/{endPoint}")]
        public async Task<int> CreateEmailSubscription(string topicName,string endPoint)
        {
            var topicResponse = _snsClient.FindTopicAsync(topicName);
            var subscriptionRequest = new SubscribeRequest();
            subscriptionRequest.TopicArn = topicResponse.Result.TopicArn;
            subscriptionRequest.Protocol = "Email";
            subscriptionRequest.Endpoint = endPoint;
            var response = await _snsClient.SubscribeAsync(subscriptionRequest);

            return (int)response.HttpStatusCode;
        }
        [HttpPost("PublishMessage/{topicName}/{messageBody}/{subject}")]
        public async Task<int> PublishMessage(string topicName, string messageBody, string subject)
        {
            var topicArn = _snsClient.FindTopicAsync(topicName).Result.TopicArn;
            var publishRequest = new PublishRequest();
            publishRequest.TopicArn = topicArn;
            publishRequest.Message = messageBody;
            publishRequest.Subject = subject;
            var response = await _snsClient.PublishAsync(publishRequest);

            return (int)response.HttpStatusCode;
        }
        [HttpPost("Unsubscribe/{topicName}/{subscriptionName}")]
        public async Task<int> Unsubscribe(string topicName, string subscriptionName)
        {
            ListSubscriptionsByTopicRequest listSubscriptionsByTopicRequest = new ListSubscriptionsByTopicRequest();
            listSubscriptionsByTopicRequest.TopicArn = _snsClient.FindTopicAsync(topicName).Result.TopicArn;
            var subs = _snsClient.ListSubscriptionsByTopicAsync(listSubscriptionsByTopicRequest).Result.Subscriptions;
            var subscriptionArn = subs.Where(x => x.Endpoint.ToLower().Trim('+') == subscriptionName).Select(x => x.SubscriptionArn).FirstOrDefault();
            var unsubscribeRequest = new UnsubscribeRequest
            {
                SubscriptionArn = subscriptionArn
            };
            var unsubscribeResponse = await _snsClient.UnsubscribeAsync(unsubscribeRequest);

            return (int)unsubscribeResponse.HttpStatusCode;
        }
        [HttpDelete]
        public async Task<int> DeleteTopic(string topicName)
        {
            DeleteTopicRequest deleteTopicRequest = new DeleteTopicRequest();
            deleteTopicRequest.TopicArn = _snsClient.FindTopicAsync(topicName).Result.TopicArn;
            var response = await _snsClient.DeleteTopicAsync(deleteTopicRequest);
            return (int)response.HttpStatusCode;
        }
        [HttpGet]
        public async Task<List<Topic>> ListAllTopic()
        {
            var request =new ListTopicsRequest();
            var response = await _snsClient.ListTopicsAsync(request);
            return response.Topics;
        }

    }
}

