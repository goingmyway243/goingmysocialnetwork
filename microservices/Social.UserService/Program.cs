
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Social.UserService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySQLConnection"),
        new MySqlServerVersion(new Version(8, 0, 3))
    )
#if DEBUG
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
#endif
);

// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Version = "v1",
//         Title = "Social User Service API",
//         Description = "API for managing users in the Social Network application"
//     });
// });

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social User Service API"));
// }

app.MapControllers();
app.UseHttpsRedirection();

app.Run();