using Newtonsoft.Json;

namespace WebApplicationMVC.Models
{
    public class Feedback
    {
        [JsonProperty(PropertyName = "id")]
        public Guid     Id { get; set; }
        public Guid     AuthorId { get; set; }
        public String?  Text { get; set; }
        public int?     Vote { get; set; }
        public DateTime Moment { get; set; }
        public String   Type { get; private set; } = "Feedback";

        [JsonIgnore]
        public UserModel? Author { get; set; }
    }
}
