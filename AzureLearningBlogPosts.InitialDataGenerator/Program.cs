using AzureLearningBlogPosts.Common.Models;
using System.Text.Json;

const string LoremIpsumFilePath = @"Local\LoremIpsum.txt";
const int MinWordsTitle = 7;
const int MaxWordsTitle = 15;
const int MinPostsCount = 3;
const int MaxPostsCount = 10;
const int MinWordsContent = 50;
const int MaxWordsContent = 150;
const string BlogsOutputFilePath = @"Local\blogs.json";
const string PostsOutputFilePath = @"Local\posts.json";

string GetRandomWords(List<string> words, int min, int max, Random random)
{
    string result = string.Empty;
    int noWords = random.Next(min, max);
    for (int idx = 0; idx < noWords; idx++)
    {
        int randIdx = random.Next(0, words.Count);
        result += words[randIdx] + " ";
    }
    return result;
}

string loremIpsumText = File.ReadAllText(LoremIpsumFilePath);
List<string> loremIpsumWords = loremIpsumText.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
Random random = new Random();
List<BlogModel> blogs = Enumerable.Range(0, 10).Select(_ => new BlogModel
{
    id = Guid.NewGuid().ToString(),
    Title = GetRandomWords(loremIpsumWords, MinWordsTitle, MaxWordsTitle, random),
    PostsCount = random.Next(MinPostsCount, MaxPostsCount),
    LastPostsCountUpdate = DateTime.Now.AddMonths(-5).AddHours(-1 * random.Next(24, 120))
}).ToList();

List<PostModel> posts = blogs.SelectMany(b => Enumerable.Range(0, b.PostsCount).Select(_ => new PostModel
{
    id = Guid.NewGuid().ToString(),
    BlogId = b.id,
    Title = GetRandomWords(loremIpsumWords, MinWordsTitle, MaxWordsTitle, random),
    Content = GetRandomWords(loremIpsumWords, MinWordsContent, MaxWordsContent, random),
    PusblishDateTime = DateTime.Now.AddMonths(-3).AddHours(-1 * random.Next(100, 500))
})).ToList();

string serializedBlogs = JsonSerializer.Serialize(blogs, new JsonSerializerOptions() { WriteIndented = true });
string serializedPosts = JsonSerializer.Serialize(posts, new JsonSerializerOptions() { WriteIndented = true });
File.WriteAllText(BlogsOutputFilePath, serializedBlogs);
Console.WriteLine($"Written file {BlogsOutputFilePath}");
File.WriteAllText(PostsOutputFilePath, serializedPosts);
Console.WriteLine($"Written file {PostsOutputFilePath}");
