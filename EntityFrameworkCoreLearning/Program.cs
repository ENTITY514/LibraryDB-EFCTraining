using EntityFrameworkCoreLearning.Data;
using EntityFrameworkCoreLearning.Data.Models;
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

        // Выводим просроченных должников для задачи №10
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 10 (Вывод просроченных книг)");

            DateOnly maxBorrowDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

            var borrowBookOfReader = await context.Borrows
                .AsNoTracking()
                .Where(b => b.ReturnDate==null&& b.BorrowDate < maxBorrowDate)
                .Include(b => b.Book)
                .Select(b => new
                {
                    b.Book!.Title,
                    b.BorrowDate,
                    b.ReturnDate,
                })
                .ToListAsync();

            foreach (var b in borrowBookOfReader)
            {
                Console.WriteLine(
                     $"{b.Title}. " +
                     $"Дата взятия: {b.BorrowDate}. " +
                     (b.ReturnDate != null ? $"Возвращена {b.ReturnDate}" : "Не возвращена")
                );
            }
        }

        // Считаем книги авторов №11
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 11 (Считаем количество книг авторов)");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .GroupBy(b => b.Author, (k, g) => new {authorName = k,count = g.ToList().Count() })
                .ToListAsync();

            foreach (var b in booksOfAuthors)
            {
                Console.WriteLine($"{b.authorName} иммет {b.count} книг.");
            }
        }

        // Средняя цена книг по издателям №12
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 12 (Считаем количество книг авторов)");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .GroupBy(b => b.PublisherEntity.Name, (k, g) => new { name = k, price = g.Average(book=>book.Price)})
                .ToListAsync();

            foreach (var b in booksOfAuthors)
            {
                Console.WriteLine($"Издательство {b.name} имеет среднюю цену в {b.price.ToString("N2")} долларов.");
            }
        }

        // Книги которые не брали №13
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 13 (Книги которые не брали)");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .Where(b => b.Borrows.Count==0)
                .Select(b => b.Title)
                .ToListAsync();

            foreach (var b in booksOfAuthors)
            {
                Console.WriteLine($"Книгу {b} никогда не брали.");
            }
        }

        // Последнии 5 добавленных книг №14
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 14 (Последние добавленные книги)");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .OrderByDescending(b => b.Year)
                .Select(b => new { b.Title, b.Year})
                .ToListAsync();

            foreach (var b in booksOfAuthors)
            {
                Console.WriteLine($"Книгу {b.Title} была написана {b.Year}.");
            }
        }

        // Топ читаемых книг №15
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача 15 (Топ читаемых книг)");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .Where(b=> b.Borrows.Count>0)
                .Select(b => new {b.Title, count = b.Borrows.Count})
                .OrderByDescending(b=>b.count)
                .Take(3)
                .ToListAsync();

            foreach (var b in booksOfAuthors)
            {
                Console.WriteLine($"Книгу {b.Title} взяли {b.count} раз.");
            }
        }

        // Добавляем отзывы 
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

            var books = await context.Books
                .AsNoTracking()
                .Select(b => new { b.Id, b.Title })
                .ToListAsync();

            var book1 = books.FirstOrDefault(b => b.Title == "Искусство войны");
            var book2 = books.FirstOrDefault(b => b.Title == "Книга Пяти Колец");
            var book3 = books.FirstOrDefault(b => b.Title == "О дивный новый мир");
            var book4 = books.FirstOrDefault(b => b.Title == "1984");

            var newReviews = new List<ReviewEntity>();
            if (book1 != null)
            {
                newReviews.Add(new ReviewEntity
                {
                    Rating = 5.0f,
                    Comment = "Шедевр военной стратегии!",
                    BookId = book1.Id
                });
                newReviews.Add(new ReviewEntity
                {
                    Rating = 4.5f,
                    Comment = "Отличная база для понимания конфликтов.",
                    BookId = book1.Id
                });
                newReviews.Add(new ReviewEntity
                {
                    Rating = 4.9f,
                    Comment = "Читается тяжело, но обязательно к прочтению.",
                    BookId = book1.Id
                });
            }

            if (book2 != null)
            {
                newReviews.Add(new ReviewEntity
                {
                    Rating = 4.8f,
                    Comment = "Глубокая философия фехтования и жизни.",
                    BookId = book2.Id
                });
                newReviews.Add(new ReviewEntity
                {
                    Rating = 4.0f,
                    Comment = "Интересный взгляд на путь воина.",
                    BookId = book2.Id
                });
            }

            if (book3 != null)
            {
                newReviews.Add(new ReviewEntity
                {
                    Rating = 3.5f,
                    Comment = "Мрачное, но актуальное антиутопическое видение.",
                    BookId = book3.Id
                });
                newReviews.Add(new ReviewEntity
                {
                    Rating = 4.0f,
                    Comment = "Провокационно и поучительно.",
                    BookId = book3.Id
                });
            }

            if (book4 != null)
            {
                newReviews.Add(new ReviewEntity
                {
                    Rating = 5.0f,
                    Comment = "Вечная классика антиутопии. Пять из пяти.",
                    BookId = book4.Id
                });
            }

            await context.Reviews.AddRangeAsync(newReviews);
            await context.SaveChangesAsync();

            Console.WriteLine($"{newReviews.Count} отзывов добавлено успешно.");
        }

        // Выводим хорошие и популярные книги №16
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача №16 (Считаем рейтинг)");

            var books = await context.Books
                .AsNoTracking()
                .Where(b=>b.Reviews.Count>0)
                .Select(b => new { b.Title, rating = b.Reviews.Average(r=>r.Rating) })
                .ToListAsync();

            foreach(var book in books)
            {
                Console.WriteLine($"Книга с названием {book.Title} имеет рейтинг {book.rating}");
            }
        }

        // Выводим хорошие и популярные книги №17
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача №17 (Считаем рейтинг для успешных книг)");

            var books = await context.Books
                .AsNoTracking()
                .Where(b => b.Reviews.Count > 1)
                .Select(b => new { b.Title, rating = b.Reviews.Average(r => r.Rating) })
                .Where(b=>b.rating>4)
                .ToListAsync();

            foreach (var book in books)
            {
                Console.WriteLine($"Книга с названием {book.Title} имеет рейтинг {book.rating}");
            }
        }

        // Выводим хорошие и популярные книги №19
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача №19 (Пагинация)");

            int maxPageCount = 0;
            int pageSize = 2;

            var books = await context.Books.AsNoTracking().OrderBy(b=>b.Id).ToListAsync();

            maxPageCount = Convert.ToInt32(books.Count() / pageSize);

            for (int page = 0; page <= maxPageCount; page++)
            {
                var bookOfPage = books.Skip(page * pageSize).Take(pageSize);

                Console.WriteLine($"На странице {page} доступны следующие книги");
                foreach (var book in bookOfPage)
                {
                    Console.WriteLine($"{book.Title}");
                }
            }
        }

        // Какой то сложный запрос №20
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Console.WriteLine("Выполнение: Задача №20 (Сложный запрос)");


            var bookDTO = await context.Books.AsNoTracking()
                .Where(b => b.Title=="Книга Пяти Колец")
                .Select(b => new { 
                    b.Title, 
                    b.Author,
                    PublisherName = b.PublisherEntity.Name,
                    AvgRating = b.Reviews.Average(r => r.Rating),
                    BorrowCount = b.Borrows.Count()
                }).FirstAsync();

            if (bookDTO != null)
            {
                Console.WriteLine($"Название книги {bookDTO.Title}");
                Console.WriteLine($"Автор книги {bookDTO.Author}");
                Console.WriteLine($"Издатель книги {bookDTO.PublisherName}");
                Console.WriteLine($"Рейтинг книги {bookDTO.AvgRating}");
                Console.WriteLine($"Количество взятий книги {bookDTO.BorrowCount}");
            }
        }
    }
}