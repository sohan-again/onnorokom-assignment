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
        
        var conn = builder.Configuration.GetConnectionString("Default")
                   ?? "Host=localhost;Port=5432;Database=school_admission;Username=school;Password=school";
        builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

        var app = builder.Build();
        app.MapControllers();

        app.Run();
    }
}
