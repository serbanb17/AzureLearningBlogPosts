using System;
using AzureLearningBlogPosts.Common.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace AzureLearningBlogPosts.ProcessNewPostFunctionApp
{
    public class ProcessNewPostFunction
    {
        private const string QueryContainerByIdFormat = "SELECT * FROM {0} c WHERE c.id = @id";
       
        [FunctionName("ProcessNewPostFunction")]
        public async Task Run(
            [ServiceBusTrigger("%NewPostsQueueName%", Connection = "ServiceBusConnection")] 
            Message message,
            MessageReceiver messageReceiver,
            [CosmosDB("%DatabaseName%", "%BlogsContainerName%", Connection = "CosmosDbConnection")]
            CosmosClient cosmosClient,
            [CosmosDB("%DatabaseName%", "%PostsContainerName%", Connection = "CosmosDbConnection")]
            IAsyncCollector<PostModel> postItems,
            ILogger log)
        {
            string dbName = Environment.GetEnvironmentVariable("DatabaseName");
            string blogsContainerName = Environment.GetEnvironmentVariable("BlogsContainerName");
            Container blogsContainer = cosmosClient.GetDatabase(dbName).GetContainer(blogsContainerName);

            string payload = Encoding.UTF8.GetString(message.Body);
            PostModel newPost = JsonSerializer.Deserialize<PostModel>(payload);

            var blogQuery = new QueryDefinition(string.Format(QueryContainerByIdFormat, blogsContainerName)).WithParameter("@id", newPost.BlogId);
            
            if(await NoItemReturned(blogsContainer, blogQuery))
            {
                await messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken);
                log.LogError($"Blog with id {newPost.BlogId} does not exist!");
            }
            else
            {
                newPost.id = Guid.NewGuid().ToString();
                await postItems.AddAsync(newPost);
                await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
                log.LogInformation($"Post with id {newPost.id} was stored in database.");
            }
        }

        private async Task<bool> NoItemReturned(Container container, QueryDefinition queryDefinition)
        {
            using (FeedIterator<dynamic> resultSet = container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await resultSet.ReadNextAsync();
                    if (response.Count > 0)
                    {
                        dynamic item = response.First();
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
