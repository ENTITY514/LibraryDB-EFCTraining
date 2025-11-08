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
        //Обновляем БД
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Удаление базы данных");

            await context.Database.EnsureDeletedAsync();
            Console.WriteLine("База данных удалена");

            await context.Database.MigrateAsync();
            Console.WriteLine("Миграции выполнены");
        }

        //Добавляем издателей - Эксмо, Казахстан, Атамура 
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Добавление издателей");

            var newPublishers = new List<PublisherEntity>
            {
                new PublisherEntity { Name = "Эксмо"},
                new PublisherEntity { Name = "Казахстан"},
                new PublisherEntity { Name = "Атамура"},
            };

            await context.Publishers.AddRangeAsync(newPublishers);
            await context.SaveChangesAsync();

            Console.WriteLine("Издатели добавлены");
        }

        //Добавляем книги Задача №2
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

        //Выводим список книг с авторами. Задача №3
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 3 (Все книги, сортировка по названию)");

            var allBooks = await context.Books
                .AsNoTracking()
                .OrderBy(book => book.Title)
                .ToListAsync();

            foreach (var book in allBooks)
            {
                Console.WriteLine($"{book.Title} - автор - {book.Author}");
            }
        }

        //Фильтрация по цене. Задача №4
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 4 (Фильтрация по цене)");

            var allBooks = await context.Books
                .AsNoTracking()
                .Where(book => book.Price > 20m)
                .OrderBy(book => book.Title)
                .ToListAsync();

            foreach (var book in allBooks)
            {
                Console.WriteLine($"{book.Title} - автор - {book.Author} - стоит:{book.Price}");
            }
        }

        //Выводим только названия книг. Задача №5
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 5 (Выводим только авторов)");

            var bookTitles = await context.Books
                .AsNoTracking()
                .Select(book => book.Title)
                .ToListAsync();

            foreach (var title in bookTitles)
            {
                Console.WriteLine($"{title}");
            }
        }

        //Выводим книги с издателями. Задача №6
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 6 (Выводим книги с издателями)");

            var booksWithPublisher = await context.Books
                .AsNoTracking()
                .Select(book => new { name = book.Title, publisherName = book.PublisherEntity.Name })
                .ToListAsync();

            foreach (var book in booksWithPublisher)
            {
                Console.WriteLine($"{book.name} под издательством {book.publisherName}");
            }
        }

        //Создаем жанры
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 7 (Создание и связывание жанров)");

            var fantasy = new GenreEntity { Name = "Фантастика" };
            var philosophy = new GenreEntity { Name = "Философия" };
            var war = new GenreEntity { Name = "Военное дело" };

            await context.Genres.AddRangeAsync(fantasy, philosophy, war);

            var books = await context.Books.ToListAsync();
            var bookMap = books.ToDictionary(b => b.Title);

            war.Books.Add(bookMap["Искусство войны"]);
            war.Books.Add(bookMap["Книга Пяти Колец"]);
            philosophy.Books.Add(bookMap["О дивный новый мир"]);
            fantasy.Books.Add(bookMap["О дивный новый мир"]);
            philosophy.Books.Add(bookMap["1984"]);
            fantasy.Books.Add(bookMap["1984"]);
            fantasy.Books.Add(bookMap["Дюна"]);

            bookMap["Искусство войны"]?.Genres.Add(war);
            bookMap["Книга Пяти Колец"]?.Genres.Add(war);
            bookMap["О дивный новый мир"]?.Genres.Add(philosophy);
            bookMap["О дивный новый мир"]?.Genres.Add(fantasy);
            bookMap["1984"]?.Genres.Add(philosophy);
            bookMap["1984"]?.Genres.Add(fantasy);
            bookMap["Дюна"]?.Genres.Add(fantasy);

            await context.SaveChangesAsync();

            Console.WriteLine("3 жанра созданы и связаны с 5 книгами.");
        }

        //Выводим жанры с количеством книг в каждом. Задача №7
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 7 (Выводим жанры с количеством книг)");

            var genresWithBook = await context.Genres
                .AsNoTracking()
                .Select(genre => new {name = genre.Name, bookCount = genre.Books.Count})
                .ToListAsync();

            foreach (var genre in genresWithBook)
            {
                Console.WriteLine($"Жанр {genre.name} содержит {genre.bookCount} книг.");
            }
        }
        // Добавляем 8 читателей. Задача №8
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 8 (Добавление читателей)");

            var newReaders = new List<ReaderEntity>
            {
                new ReaderEntity { FullName = "Иванов Иван Иванович" },
                new ReaderEntity { FullName = "Петрова Анна Сергеевна" },
                new ReaderEntity { FullName = "Сидоров Олег Викторович" },
                new ReaderEntity { FullName = "Кузнецова Елена Павловна" },
                new ReaderEntity { FullName = "Михайлов Дмитрий Андреевич" },
                new ReaderEntity { FullName = "Волкова Татьяна Николаевна" },
                new ReaderEntity { FullName = "Смирнов Алексей Геннадьевич" },
                new ReaderEntity { FullName = "Федорова Екатерина Борисовна" },
            };

            await context.Readers.AddRangeAsync(newReaders);
            await context.SaveChangesAsync();

            var readersFromDB = await context.Readers
                .AsNoTracking()
                .Select(r=>r.FullName)
                .ToListAsync();

            foreach(var readerName in readersFromDB)
            {
                Console.WriteLine($"Читатель: {readerName}");
            }

            Console.WriteLine("8 читателей добавлены.");
        }

        // Добавляем должников для задачи №9
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 9 (Добавление тестовой выдачи)");

            var reader = await context.Readers.FirstOrDefaultAsync(r => r.FullName == "Петрова Анна Сергеевна");
            var book1984 = await context.Books.FirstOrDefaultAsync(b => b.Title == "1984");
            var bookDune = await context.Books.FirstOrDefaultAsync(b => b.Title == "Дюна");

            if (reader == null || book1984 == null || bookDune == null) return;

            List<BorrowEntity> newBorrows = new List<BorrowEntity>
            {
                new BorrowEntity{
                    ReaderId = reader.Id,
                    BookId = book1984.Id,
                    BorrowDate = new DateOnly(2025, 10, 15),
                    ReturnDate = new DateOnly(2025, 11, 01)
                },
                new BorrowEntity
                {
                    ReaderId = reader.Id,
                    BookId = bookDune.Id,
                    BorrowDate = new DateOnly(2025, 11, 01),
                    ReturnDate = null
                }
            };

            await context.Borrows.AddRangeAsync(newBorrows);
            await context.SaveChangesAsync();

            var borrowBookOfReader = await context.Borrows
                .AsNoTracking()
                .Where(b =>b.ReaderId==reader.Id)
                .Include(b=>b.Book)
                .Select(b => new
                {
                    b.Book!.Title,
                    b.BorrowDate,
                    b.ReturnDate,
                })
                .ToListAsync();

            Console.WriteLine($"Читатель {reader.FullName} взял следующие книги:");
            foreach( var b in borrowBookOfReader)
            {
                Console.WriteLine(
                     $"{b.Title}. " +
                     $"Дата взятия: {b.BorrowDate}. " +
                     (b.ReturnDate != null ? $"Возвращена {b.ReturnDate}" : "Не возвращена")
                );
            }
        }
    }
}