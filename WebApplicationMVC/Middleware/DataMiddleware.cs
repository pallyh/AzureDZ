using System.Text.Json;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Middleware
{
    public class DataMiddleware
    {
        public static List<Feedback>? Feedbacks;
        public static String DirectoryPath = "";
        public static String Filename = "feedback.json";

        private readonly RequestDelegate _next;

        public DataMiddleware(RequestDelegate next)
        {
            _next = next;
            try
            {
                var tempDirectoryPath = Environment.GetEnvironmentVariable("TEMP") ?? @"C:\local\Temp\";
                var filePath = Path.Combine(tempDirectoryPath, Filename);
                using (var reader = new StreamReader(filePath))
                    Feedbacks = JsonSerializer.Deserialize<List<Feedback>>(
                        reader.ReadToEnd());
                DirectoryPath = tempDirectoryPath;
            }
            catch
            {
                try
                {
                    using (var reader = new StreamReader(Filename))
                        Feedbacks = JsonSerializer.Deserialize<List<Feedback>>(
                            reader.ReadToEnd());
                }
                catch
                {
                    Feedbacks = new();
                }
            }
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);
        }

        public static void SaveFeedback()
        {
            var filePath = Path.Combine(DirectoryPath, Filename);

            using (var writer = new StreamWriter(filePath))
            {
                writer.Write(
                    JsonSerializer.Serialize(Feedbacks));
            }
        }
    }
}
