using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureLearningBlogPosts.Common.Models;
using AzureLearningBlogPosts.CountPostsFunctionApp.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureLearningBlogPosts.CountPostsFunctionApp
{
    public class CountPostsFunction
    {
        private const string QueryBlogPostCountsFormat = "SELECT c['id'], c['PostsCount'] FROM {0} c";
        private const string QueryPostCountByBlogIdFormat = "SELECT COUNT(1) Count FROM {0} c WHERE c.BlogId = @id";

        [FunctionName("CountPostsFunction")]
        public async Task Run(
            [TimerTrigger("%TriggerCronExpression%")]
            TimerInfo myTimer,
            [CosmosDB("%DatabaseName%", "%BlogsContainerName%", Connection = "CosmosDbConnection")]
            CosmosClient cosmosClient,
            ILogger log)
        {
            log.LogInformation($"Timer trigger function {nameof(CountPostsFunction)} executed at: {DateTime.Now}");

            string dbName = Environment.GetEnvironmentVariable("DatabaseName");
            string blogsContainerName = Environment.GetEnvironmentVariable("BlogsContainerName");
            string postsContainerName = Environment.GetEnvironmentVariable("PostsContainerName");
            cosmosClient.ClientOptions.AllowBulkExecution = true;
            Container blogsContainer = cosmosClient.GetDatabase(dbName).GetContainer(blogsContainerName);
            Container postsContainer = cosmosClient.GetDatabase(dbName).GetContainer(postsContainerName);
            
            var blogIdsQuery = new QueryDefinition(string.Format(QueryBlogPostCountsFormat, blogsContainerName));
            using (FeedIterator<BlogModel> resultSet = blogsContainer.GetItemQueryIterator<BlogModel>(blogIdsQuery))
            {
                while (resultSet.HasMoreResults)
                {
                    var patchTasks = new List<Task>();
                    FeedResponse<BlogModel> response = await resultSet.ReadNextAsync();
                    foreach (BlogModel blog in response)
                    {
                        var postsCountQuery = new QueryDefinition(string.Format(QueryPostCountByBlogIdFormat, postsContainerName)).WithParameter("@id", blog.id);
                        CountModel countResult = await GetFirstQuerytem<CountModel>(postsContainer, postsCountQuery);
                        if (blog.PostsCount != countResult.Count)
                        {
                            var patchOperations = new PatchOperation[]
                            {
                                PatchOperation.Replace($"/PostsCount", countResult.Count),
                                PatchOperation.Replace($"/LastPostsCountUpdate", DateTime.UtcNow)
                            };
                            patchTasks.Add(blogsContainer.PatchItemAsync<BlogModel>(blog.id, new PartitionKey(blog.id), patchOperations));
                        }
                    }
                    try
                    {
                        await Task.WhenAll(patchTasks);
                    }
                    catch(Exception ex)
                    {

                    }
                    log.LogInformation($"Updated post count for {patchTasks.Count} blogs.");
                }
            }
        }

        private async Task<T> GetFirstQuerytem<T>(Container container, QueryDefinition queryDefinition)
        {
            using (FeedIterator<T> resultSet = container.GetItemQueryIterator<T>(queryDefinition))
            {
                if (resultSet.HasMoreResults)
                {
                    FeedResponse<T> response = await resultSet.ReadNextAsync();
                    foreach (T item in response)
                    {
                        return item;
                    }
                }
            }
            return default;
        }
    }
}
