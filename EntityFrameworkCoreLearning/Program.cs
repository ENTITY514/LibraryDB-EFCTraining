using EntityFrameworkCoreLearning.Data;
using EntityFrameworkCoreLearning.Data.Models; // Убедись, что твои модели здесь
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await RunTasks(host.Services);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseNpgsql(connectionString)
                           .UseSnakeCaseNamingConvention());
            });

    private static async Task RunTasks(IServiceProvider serviceProvider)
    {

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Удаление базы данных");

            await context.Database.EnsureDeletedAsync();
            Console.WriteLine("База данных удалена");

            await context.Database.MigrateAsync();
            Console.WriteLine("Миграции выполнены");
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Добавление издателей");

            var newPublishers = new List<PublisherEntity>
            {
                new PublisherEntity { Name = "Эксмо"},
                new PublisherEntity { Name = "Қазақстан"},
                new PublisherEntity { Name = "Атамұра"},
            };

            await context.Publishers.AddRangeAsync(newPublishers);
            await context.SaveChangesAsync();

            Console.WriteLine("Издатели добавлены");
        }


        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 2 (Добавление книг)");

            List<PublisherEntity> publishers = await context.Publishers.ToListAsync();

            var newBooks = new List<BookEntity>
            {
                new BookEntity { Title = "Искусство войны", Author = "Сунь-цзы", Year = 2020, Price = 15.99m, PublisherEntity = publishers[0] },
                new BookEntity { Title = "Книга Пяти Колец", Author = "Миямото Мусаси", Year = 1645, Price = 22.50m, PublisherEntity = publishers[1]  },
                new BookEntity { Title = "О дивный новый мир", Author = "Олдос Хаксли", Year = 1932, Price = 18.00m, PublisherEntity = publishers[0]  },
                new BookEntity { Title = "1984", Author = "Джордж Оруэлл", Year = 1949, Price = 17.99m, PublisherEntity = publishers[2]  },
                new BookEntity { Title = "Дюна", Author = "Фрэнк Герберт", Year = 1965, Price = 25.00m, PublisherEntity = publishers[2]  }
            };

            await context.Books.AddRangeAsync(newBooks);
            await context.SaveChangesAsync();

            Console.WriteLine("5 книг добавлено.");
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 3 (Все книги, сортировка по названию)");

            var allBooks = await context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ToListAsync();

            foreach (var book in allBooks)
            {
                Console.WriteLine($"- {book.Title} ({book.Author})");
            }
        }
    }
}