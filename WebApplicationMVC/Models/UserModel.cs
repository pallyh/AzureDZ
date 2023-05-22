using Newtonsoft.Json;

namespace WebApplicationMVC.Models
{
    public class UserModel
    {
        [JsonProperty(PropertyName = "id")]
        public Guid    Id { get; set; }
        public String? Name { get; set; } 
        public String? Password { get; set; }
        public String  Type { get; private set; } = "User";

        [JsonIgnore]
        public List<Feedback>? Feedbacks { get; set; }
    }
}
