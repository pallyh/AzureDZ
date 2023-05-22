using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using WebApplicationMVC.Middleware;
using WebApplicationMVC.Models;

namespace WebApplicationMVC.Controllers
{
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Feedback> Get()
        {
            return DataMiddleware.Feedbacks ?? new();
        }
        [HttpGet("{id}")]
        public string GetId(String id)
        {
            if (id == "loc")
            {
                return System.IO.File.ReadAllText(DataMiddleware.Filename);
            }
            else if (id == "tmp")
            {                
                return System.IO.File.ReadAllText(
                    Path.Combine(DataMiddleware.DirectoryPath, DataMiddleware.Filename));
            }
            else return "No";
        }

        
        [HttpPost]
        public IdModel Post([FromBody] IdModel id)
        {
            /*
             1. Проверяется наличие заголовка
             2. Извлекается значение, проверяется на валидность
             3. Пересчитывается и пересохраняется
             */
            String token = Request.Headers["Auth-Token"];  // 1.
            if (token == null) return null!;
            if (token != "1234") return null!;   // 2.

            Response.Headers.Add("Auth-Token", "1234");   // 3.

            return id;
            /* return DataMiddleware.Feedbacks != null
                    ? DataMiddleware.Feedbacks[0] 
                    : null!; */
        }
        /* Д.З. Реализовать передачу JSON всеми методами запросов
         * (из прошлого ДЗ), настроить CORS для правильной работы
         */
        [HttpDelete]
        public Feedback Delete()
        {
            return DataMiddleware.Feedbacks != null
                    ? DataMiddleware.Feedbacks[0]
                    : null!;
        }

        [HttpOptions]
        public IActionResult Options()
        {
            Response.Headers.AccessControlAllowOrigin = "*";
            Response.Headers.AccessControlAllowMethods = "GET, POST, PUT, DELETE";
            Response.Headers.AccessControlAllowHeaders = "Content-Type";

            return NoContent();
        }
        
        [HttpPatch]
        public String Patch()
        {
            return "Hello, patch";
        }
    }
    public class IdModel
    {
        public Guid Id { get; set; }
    }
}
