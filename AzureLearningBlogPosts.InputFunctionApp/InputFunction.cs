using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System;
using AzureLearningBlogPosts.Common.Utilities;
using AzureLearningBlogPosts.Common.Models;

namespace AzureLearningBlogPosts.InputFunctionApp
{
    public static class InputFunction
    {
        [FunctionName("InputFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [ServiceBus("%NewPostsQueueName%", Connection = "ServiceBusConnection")] IAsyncCollector<dynamic> newPostsQueue,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (JsonParsing.TryParseJson<PostModel>(requestBody, out var post))
            {
                post.PusblishDateTime = DateTime.UtcNow;
                await newPostsQueue.AddAsync(post);
                log.LogInformation("New post added to queue.");
                return new OkResult();
            }

            log.LogError("Could not parse request body.");
            return new BadRequestResult();
        }
    }
}
