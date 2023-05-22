using WebApplicationMVC.Middleware;
using WebApplicationMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Configuration
  .AddJsonFile("azuresettings.json", false, true);

// регистрируем "поставщика" подключения к БД Azure CosmosDB
builder.Services.AddSingleton<ICosmosDb, TodoCosmosDb>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors( 
    conf => 
        conf
        .AllowAnyMethod()
        .SetIsOriginAllowed( origin => true ) 
        .AllowAnyHeader()
        .AllowCredentials()
) ;
app.UseRouting();

app.UseSession();

// app.UseAuthorization();

app.UseMiddleware<DataMiddleware>();
app.Lifetime.ApplicationStopping.Register(DataMiddleware.SaveFeedback);

app.UseMiddleware<AuthMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
/*
  1. Создать сайт на ASP-MVC, опубликовать его на Azure, приложить 
  ссылку на сайт.
  2. Создать репозиторий для кодов сайта (на DevOps), добавить к 
  проекту .gitignore, опубликовать проект. Приложить ссылку на репозиторий
  3. Повторить ASP: Middleware, сериализация
 */
/*
  Middleware--->|
  Request -> M1 -> M2 ->  Controller -> Response
          <-   <-     <-             <-| 

  Program               Request 
  M1.ctor()             M1.Invoke()
  M2.ctor()             M2.Invoke()
                        Controller.ctor()
 */