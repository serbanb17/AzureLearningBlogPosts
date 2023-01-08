### <center>Azure learning: Blog posts</center>

&emsp;This is a dummy project created in .NET 6 to experiment with Azure services(Functions, Service Bus, Cosmos DB). The solution stores blog articles received over http into a database.

<center>[&#128498;] -----> [&#128386;] -----> [&#128498;] -----> [DB] <-----> [&#128498;]</center><br/>
<center>-1-----------2-----------3-----------4-------------5-</center>

1. **Input function**. Function with http trigger. It checks if request body can be parsed into a blog post entity. If so, it will add the post a new posts queue.
2. **New posts queue**. Service bus queue.
3. **Process new post function**. Functinon with service bus queue trigger. It receives messages from the new posts queue. If the post entity contains a valid blog id, it will be stored in the database.
4. **Cosmos DB**. Data items will be stored in two container, for blog and posts items.
5. **Count posts function**. Function with timer trigger. Each times it's trigger, it will update the posts count field of each blog, if it's outdated, and the last update timestamp.

&emsp;The solution contains a function app project for each function instance and a console app that generates two input json files (blogs and posts) that can be uploaded to the Cosmos DB containers as mock data. Parameters can be edited in `Program.cs` file.

##### &emsp;Useful links during development
- [Azure Functions triggers and bindings ](https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-getting-started?tabs=dotnet)
- [Azure Cosmos DB Partial Document Update: Getting Started](https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-getting-started?tabs=dotnet)
- [Introducing Bulk support in the .NET SDK](https://devblogs.microsoft.com/cosmosdb/introducing-bulk-support-in-the-net-sdk/)
