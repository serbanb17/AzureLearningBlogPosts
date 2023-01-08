using Newtonsoft.Json;

namespace AzureLearningBlogPosts.Common.Utilities
{
    public static class JsonParsing
    {
        public static bool TryParseJson<T>(string value, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(value, settings);
            return success && result != null;
        }
    }
}
