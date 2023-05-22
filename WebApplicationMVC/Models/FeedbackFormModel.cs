namespace WebApplicationMVC.Models
{
    // Модель приема отзыва с формы CosmosDB
    public class FeedbackFormModel
    {
        public String? Author { get; set; }
        public String? Password { get; set; }
        public String? Text { get; set; }
        public int?    Vote { get; set; }
    }
}
