using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using WebApplicationMVC.Controllers;
using WebApplicationMVC.Middleware;
using WebApplicationMVC.Models;
using Microsoft.Azure.Cosmos;
using WebApplicationMVC.Services;

namespace WebApplicationMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICosmosDb _cosmosDb;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            ICosmosDb cosmosDb)
        {
            _logger = logger;
            _configuration = configuration;
            _cosmosDb = cosmosDb;
        }
        [HttpGet]
        public async Task<ViewResult> CosmosAsync()
        {
            // Получение данных из контейнера по типу сообщений
            // var sqlQueryText = "SELECT * FROM c WHERE c.Type='Feedback' ";
            var feedbacks = _cosmosDb[0]
                .GetItemLinqQueryable<Feedback>(true)
                .Where(item => item.Type == "Feedback")
                .ToList();


            // Получение данных из контейнера по типу пользователей
            // sqlQueryText = "SELECT * FROM c WHERE c.Type='User' ";
            var users = _cosmosDb[0]
                .GetItemLinqQueryable<UserModel>(true)
                .Where(item => item.Type == "User")
                .ToList();

            // Проработка связей ("навигационных свойств")
            // авторы отзывов
            foreach( Feedback feedback in feedbacks )
            {
                feedback.Author = users.Find(u => u.Id == feedback.AuthorId);
            }
            // отзывы автора (коллекция отзывов каждого из авторов) 
            

            ViewData["Feedbacks"] = feedbacks;
            ViewData["Users"] = users;
            return View();
        }

        [HttpPost("/Home/Cosmos")]
        public async Task<RedirectResult> CosmosPostAsync(FeedbackFormModel form)
        {
            var user = _cosmosDb[0]
                .GetItemLinqQueryable<UserModel>(true)
                .Where(item => item.Type == "User" 
                    && item.Name == form.Author
                    && item.Password == form.Password)
                .ToList().FirstOrDefault();

            if( user != null )  // авторизован
            {
                Feedback feedback = new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = user.Id,
                    Moment = DateTime.Now,
                    Text = form.Text,
                    Vote = form.Vote
                };
                await _cosmosDb[0].CreateItemAsync(feedback);
            }
            return Redirect("/Home/Cosmos");
        }
        /*
        [HttpGet]
        public async Task<ViewResult> CosmosAsync()
        {
            // Получение данных из контейнера
            var sqlQueryText = "SELECT * FROM c ";
            QueryDefinition queryDefinition = new(sqlQueryText);
            FeedIterator<Feedback> queryResultSetIterator =
                _cosmosDb[0].GetItemQueryIterator<Feedback>(queryDefinition);

            List<Feedback> feedbacks = new();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Feedback> currentResultSet = 
                    await queryResultSetIterator.ReadNextAsync();
                foreach (Feedback feedback in currentResultSet)
                {
                    feedbacks.Add(feedback);
                }
            }
            ViewData["Feedbacks"] = feedbacks;

            return View();
        }

        [HttpPost("/Home/Cosmos")]
        public async Task<RedirectResult> CosmosPostAsync(Feedback feedback)
        {
            feedback.Id = Guid.NewGuid();
            feedback.Moment = DateTime.Now;

            // CosmosClient cosmosClient = new(endpoint, apiKey);
            // Database database = cosmosClient.GetDatabase(databaseId);
            // Container container = database.GetContainer(containerId);

            // ItemResponse<Feedback> result =
                await _cosmosDb[0].CreateItemAsync(feedback);

            return Redirect("/Home/Cosmos");
        }
        */


        [HttpPost("/Home/Auth")]
        public RedirectResult Auth(String userLogin, String userPassword)
        {
            var user = _cosmosDb[0]
                .GetItemLinqQueryable<UserModel>(true)
                .Where(item => item.Type == "User"
                    && item.Name == userLogin
                    && item.Password == userPassword)
                .ToList().FirstOrDefault();

            if (user != null)  // авторизован
            {
                this.HttpContext.Session.SetString("AuthUserId", user.Id.ToString());
            }
            return Redirect("/");
        }

        [HttpGet]
        public IActionResult Index()
        {
            this.HttpContext.Session.SetString("sKey", "From session");

            ViewData["Message"] = _configuration.GetSection("AppData")
                .GetValue<String>("Data1") + " "
                + _configuration.GetSection("MyAppData")
                .GetValue<String>("MyData1");

            ViewData["Feedbacks"] = DataMiddleware.Feedbacks;
            // DirectoryInfo dir = new DirectoryInfo("C:\\home\\data");
            try
            {
                String fn = "~\\test.txt";
                // System.IO.File.CreateText("C:\\home\\LogFiles\\test.txt").WriteLine("Hello"); ViewData["f"] = "OK";

                ViewData["f"] = System.IO.File.ReadAllText(fn);
            }
            catch (Exception ex)
            {
                ViewData["f"] = "Nop. " + ex.Message;
            }


            return View();
        }

        [HttpPost("/Home/Index")]
        public IActionResult IndexPost(Feedback feedback)
        {
            feedback.Id = Guid.NewGuid();
            feedback.Moment = DateTime.Now;
            DataMiddleware.Feedbacks?.Add(feedback);
            // DataMiddleware.SaveFeedback();
            return Redirect("/");
        }

        public ViewResult Search()
        {
            var BingCofig = _configuration.GetSection("BingSearch");
            String endpoint = BingCofig.GetValue<String>("Endpoint");  // "https://api.bing.microsoft.com/";
            String service = BingCofig.GetValue<String>("AllSearch");  // "v7.0/search";
            String apiKey = BingCofig.GetValue<String>("Key");

            String getParams = String.Join("&", new String[]
            {
                "q=" + WebUtility.UrlEncode("Pink Unicorn"),
                "mkt=uk-UA",
                "count=21"
            });

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            // var result = httpClient.GetAsync(endpoint + service + "?" + getParams).Result;
            String responseBody = httpClient.GetAsync(endpoint + service + "?" + getParams).Result
                .Content.ReadAsStringAsync().Result;

            ViewBag.JsonResult = responseBody;
            ViewData["SearchResult"] = JsonSerializer.Deserialize<SearchResult>(responseBody);

            return View();
        }

        [HttpGet]
        public async Task<ViewResult> TranslateAsync([FromQuery]String? lang)
        {
            String[] SupportedLanguages = { "en", "uk", "ru", "de", "ja" };
            ViewData["SupportedLanguages"] = SupportedLanguages;
            if (lang != null && !SupportedLanguages.Contains(lang))
            {
                ViewData["Error"] = "Unsupported language";
                return View();
            }
            lang ??= SupportedLanguages[1];   // lang = lang ?? "uk";

            var TranslatorConfig = _configuration.GetSection("Translator");
            String endpoint = TranslatorConfig.GetValue<String>("Endpoint"); 
            String service = TranslatorConfig.GetValue<String>("Service");
            String region = TranslatorConfig.GetValue<String>("Region");
            String apiKey = TranslatorConfig.GetValue<String>("Key");

            String textToTranslate = "Квітка папороті";  // "Pink Unicorn";
            String requestBody = JsonSerializer.Serialize(
                new TranslateText[] {
                    new TranslateText() { Text = textToTranslate }
                });
            String getParams = String.Join("&", new String[]
            {
                //"from=en",
                "to=" + lang
            });
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {   // From sample
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + service + getParams);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                // Send the request and get response.
                HttpResponseMessage response = 
                    await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                ViewData["result"] = result;
                ViewData["TranslateResult"] = JsonSerializer
                    .Deserialize<TranslateResult[]>(result);

                ViewData["origin"] = new Translation
                {
                    text = textToTranslate,
                    to = "en"
                };

            }
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["fromSession"] =
                this.HttpContext.Session.GetString("sKey");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public record TranslateResult
    {
        public List<Translation>? translations { get; set; }
    }

    public record Translation
    {
        public String? text { get; set; }
        public String? to { get; set; }
    }

    public class TranslateText
    {
        public String? Text { get; set; }
    }    


    public class SearchResult
    {
        public String? _type { get; set; }
        public QueryContext? queryContext { get; set; }
        public WebPage? webPages { get; set; }
        public Images? images { get; set; }
        public Videos? videos { get; set; }
    }
    public class WebPage
    {
        public String? webSearchUrl { get; set; }
        public int totalEstimatedMatches { get; set; }
        public List<WebPageValue>? value { get; set; }
    }
    public class WebPageValue
    {
        public String? id { get; set; }

        public String? name { get; set; }
        public String? url { get; set; }
        public String? thumbnailUrl { get; set; }
        public bool isFamilyFriendly { get; set; }
        public String? displayUrl { get; set; }
        public String? snippet { get; set; }
        public String? dateLastCrawled { get; set; }
        public String? language { get; set; }
        public bool isNavigational { get; set; }
    }
    public class QueryContext
    {
        public String? originalQuery { get; set; }
        public bool askUserForLocation { get; set; }
    }
    
    public class ImageValue
    {
        public String? id { get; set; }
        public String? webSearchUrl { get; set; }
        public String? name { get; set; }
        public String? thumbnailUrl { get; set; }
        public String? contentUrl { get; set; }
        public String? hostPageUrl { get; set; }
        public String? contentSize { get; set; }
        public String? encodingFormat { get; set; }
        public String? hostPageDisplayUrl { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public Thumbnail thumbnail { get; set; }
             
    }
    public struct Thumbnail {
        public int width { get; set; }
        public int height { get; set; }
    }
    public class Images
    {
        public String? id { get; set; }
        public String? readLink { get; set; }
        public String? webSearchUrl { get; set; }
        public bool isFamilyFriendly { get; set; }
        public List<ImageValue>? value { get; set; }
    }

    public class VideoValue
    {
        public String? webSearchUrl { get; set; }
        public String? name { get; set; }
        public String? description { get; set; }
        public String? thumbnailUrl { get; set; }
        public String? datePublished { get; set; }
        public bool isAccessibleForFree { get; set; }
        public String? contentUrl { get; set; }
        public String? hostPageUrl { get; set; }
        public String? encodingFormat { get; set; }
        public String? hostPageDisplayUrl { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public String? duration { get; set; }
        public String? motionThumbnailUrl { get; set; }
        public String? embedHtml { get; set; }
        public bool allowHttpsEmbed { get; set; }
        public int viewCount { get; set; }
        public Thumbnail thumbnail { get; set; }
        public bool allowMobileEmbed { get; set; }
        public bool isSuperfresh { get; set; }
        public List<Named>? publisher { get; set; }
    }
    public class Named
    {
        public String? Name { get; set; }
    }
    public class Videos
    {
        public String? id { get; set; }
        public String? readLink { get; set; }
        public String? webSearchUrl { get; set; }
        public bool isFamilyFriendly { get; set; }
        public List<VideoValue> value { get; set; }
        public String? scenario { get; set; }
    }
}