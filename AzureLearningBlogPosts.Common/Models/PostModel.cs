namespace AzureLearningBlogPosts.Common.Models
{
    public class PostModel
    {
        public string id { get; set; }
        public string BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PusblishDateTime { get; set; }
    }
}
