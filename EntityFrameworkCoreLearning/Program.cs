using EntityFrameworkCoreLearning.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Здесь ты будешь выполнять свои задачи
        // Например, вызовем метод для демонстрации
        // await RunTasks(host.Services);

        await host.RunAsync(); // Для реального приложения, для консоли можно упростить
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseNpgsql(connectionString)
                           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                           .UseSnakeCaseNamingConvention()); // Для PostgreSQL это — дисциплина.

                // Здесь можно регистрировать твои сервисы/классы для решения задач
                // services.AddScoped<TaskRunner>(); 
            });
}
