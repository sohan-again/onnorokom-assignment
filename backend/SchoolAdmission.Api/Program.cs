namespace SchoolAdmission.Api;

using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using SchoolAdmission.Api.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // The React dev server and the Docker frontend both run on localhost:5173.
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
            p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
        
        var conn = builder.Configuration.GetConnectionString("Default")
                   ?? "Host=localhost;Port=5432;Database=school_admission;Username=school;Password=school";
        builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (var attempt = 1; ; attempt++)
            {
                try { db.Database.Migrate(); break; }
                catch (Exception ex) when (attempt < 15)
                {
                    Console.WriteLine($"Waiting for database ({attempt}/15): {ex.Message}");
                    Thread.Sleep(2000);
                }
            }
        }

        app.UseCors();

        app.MapControllers();

        app.Run();
    }
}
