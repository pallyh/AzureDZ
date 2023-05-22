using WebApplicationMVC.Models;
using WebApplicationMVC.Services;

namespace WebApplicationMVC.Middleware
{
    public class AuthMiddleware
    {
        public static Models.UserModel? AuthUser
        {
            get; set; 
        }
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext,
            ICosmosDb cosmosDb)
        {
            // Authenticate
            String? userId = httpContext.Session.GetString("AuthUserId");
            if(userId is not null)
            {
                Guid guid = Guid.Parse(userId);

                var query = cosmosDb[0]
                    .GetItemLinqQueryable<UserModel>(true)
                    .Where(item => item.Type == "User"
                                && item.Id == guid);

                AuthUser = query.ToList().FirstOrDefault(); 
            }
            else
            {
                AuthUser = null;
            }
            await _next.Invoke(httpContext);
        }

    }
}
/* CosmosDB: реализовать форму регистрации нового пользователя
 * добавить переход на форму в панель Layout-а
 * При регистрации проверять на "занятость" логина и валидировать пароль
 */