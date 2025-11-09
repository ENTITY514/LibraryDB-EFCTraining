using EntityFrameworkCoreLearning.Data;
using EntityFrameworkCoreLearning.Data.Models;
using EntityFrameworkCoreLearning.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using TaskStatus = EntityFrameworkCoreLearning.Utils.TaskStatus;

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
            ConsoleOutputHelper.WriteTaskTitle("Добавление издателей");

            var newPublishers = new List<PublisherEntity>
            {
                new PublisherEntity { Name = "Эксмо"},
                new PublisherEntity { Name = "Казахстан"},
                new PublisherEntity { Name = "Атамура"},
            };

            await context.Publishers.AddRangeAsync(newPublishers);
            await context.SaveChangesAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);
            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine("Издатели добавлены");
            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        //Добавляем книги Задача №2
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

            // Использование ConsoleOutputHelper для вывода названия задачи
            ConsoleOutputHelper.WriteTaskTitle("Задача 2: Добавление 5 книг");

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

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);
            ConsoleOutputHelper.WriteTaskResult($"{newBooks.Count} книг добавлено успешно и связано с издателями.");
        }

        //Выводим список книг с авторами. Задача №3
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 3: Все книги, сортировка по названию");

            var allBooks = await context.Books
                .AsNoTracking()
                .OrderBy(book => book.Title)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var book in allBooks)
            {
                resultBuilder.AppendLine($"{book.Title} - автор - {book.Author}");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        //Фильтрация по цене. Задача №4
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 4: Фильтрация по цене (дороже 20)");

            var allBooks = await context.Books
                .AsNoTracking()
                .Where(book => book.Price > 20m)
                .OrderBy(book => book.Title)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var book in allBooks)
            {
                resultBuilder.AppendLine($"{book.Title} - автор - {book.Author} - стоит:{book.Price.ToString("N2")}");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        //Выводим только названия книг. Задача №5
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 5: Выводим только названия книг");

            var bookTitles = await context.Books
                .AsNoTracking()
                .Select(book => book.Title)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var title in bookTitles)
            {
                resultBuilder.AppendLine($"{title}");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        //Выводим книги с издателями. Задача №6
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 6: Выводим книги с издателями");

            var booksWithPublisher = await context.Books
                .AsNoTracking()
                .Select(book => new { name = book.Title, publisherName = book.PublisherEntity.Name })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var book in booksWithPublisher)
            {
                resultBuilder.AppendLine($"{book.name} под издательством {book.publisherName}");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        //Создаем жанры
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 7: Создание и связывание жанров (Многие-ко-Многим)");

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

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);
            ConsoleOutputHelper.WriteTaskResult("3 жанра (Фантастика, Философия, Военное дело) созданы и связаны с 5 книгами через отношение Many-to-Many.");
        }

        //Выводим жанры с количеством книг в каждом. Задача №7
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 7: Выводим жанры с количеством книг");

            var genresWithBook = await context.Genres
                .AsNoTracking()
                .Select(genre => new { name = genre.Name, bookCount = genre.Books.Count })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var genre in genresWithBook)
            {
                resultBuilder.AppendLine($"Жанр {genre.name} содержит {genre.bookCount} книг.");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }
        // Добавляем 8 читателей. Задача №8
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 8: Добавление 8 читателей");

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
                .Select(r => r.FullName)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var readerName in readersFromDB)
            {
                resultBuilder.AppendLine($"Читатель: {readerName}");
            }
            resultBuilder.AppendLine($"{newReaders.Count} читателей добавлено успешно.");

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Добавляем должников для задачи №9
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 9: Добавление тестовой выдачи (2 записи для Петровой)");

            var reader = await context.Readers.FirstOrDefaultAsync(r => r.FullName == "Петрова Анна Сергеевна");
            var book1984 = await context.Books.FirstOrDefaultAsync(b => b.Title == "1984");
            var bookDune = await context.Books.FirstOrDefaultAsync(b => b.Title == "Дюна");

            if (reader == null || book1984 == null || bookDune == null)
            {
                ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Failure);
                ConsoleOutputHelper.WriteTaskResult("Ошибка: Не найден читатель или книга для добавления тестовой выдачи.");
                return;
            }

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
                .Where(b => b.ReaderId == reader.Id)
                .Include(b => b.Book)
                .Select(b => new
                {
                    b.Book!.Title,
                    b.BorrowDate,
                    b.ReturnDate,
                })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"Читатель {reader.FullName} взял следующие книги ({borrowBookOfReader.Count} записей):");

            foreach (var b in borrowBookOfReader)
            {
                resultBuilder.AppendLine(
                     $"{b.Title}. " +
                     $"Дата взятия: {b.BorrowDate}. " +
                     (b.ReturnDate != null ? $"Возвращена {b.ReturnDate}" : "Не возвращена")
                );
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Выводим просроченных должников для задачи №10
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 10: Вывод просроченных книг");

            DateOnly maxBorrowDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

            var borrowBookOfReader = await context.Borrows
                .AsNoTracking()
                .Where(b => b.ReturnDate == null && b.BorrowDate < maxBorrowDate)
                .Include(b => b.Book)
                .Select(b => new
                {
                    b.Book!.Title,
                    b.BorrowDate,
                    b.ReturnDate,
                })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            
            resultBuilder.AppendLine($"Найдено {borrowBookOfReader.Count} просроченных выдач:");
            foreach (var b in borrowBookOfReader)
            {
                resultBuilder.AppendLine(
                        $"{b.Title}. " +
                        $"Дата взятия: {b.BorrowDate}. " +
                        (b.ReturnDate != null ? $"Возвращена {b.ReturnDate}" : "Не возвращена")
                );
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Считаем книги авторов №11
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 11: Считаем количество книг по авторам");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .GroupBy(b => b.Author, (k, g) => new { authorName = k, count = g.Count() }) // Исправлено: g.ToList().Count() заменено на g.Count() для эффективности
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var b in booksOfAuthors)
            {
                resultBuilder.AppendLine($"{b.authorName} имеет {b.count} книг.");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Средняя цена книг по издателям №12
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 12: Средняя цена книг по издателю");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .GroupBy(b => b.PublisherEntity.Name, (k, g) => new { name = k, price = g.Average(book => book.Price) })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            foreach (var b in booksOfAuthors)
            {
                resultBuilder.AppendLine($"Издательство {b.name} имеет среднюю цену в {b.price.ToString("N2")} долларов.");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Книги которые не брали №13
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 13: Книги, которые никогда не брали");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .Where(b => b.Borrows.Count == 0)
                .Select(b => b.Title)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            
            resultBuilder.AppendLine($"Найдено {booksOfAuthors.Count} книг, которые никто не брал:");
            foreach (var b in booksOfAuthors)
            {
                resultBuilder.AppendLine($"Книгу {b} никогда не брали.");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Последнии 5 добавленных книг №14
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 14: Последние добавленные книги (по году)");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .OrderByDescending(b => b.Year)
                .Select(b => new { b.Title, b.Year })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();

            resultBuilder.AppendLine("Список книг, отсортированный по году издания (от новых к старым):");
            foreach (var b in booksOfAuthors)
            {
                resultBuilder.AppendLine($"Книгу {b.Title} была написана в {b.Year} году.");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Топ читаемых книг №15

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 15: Топ-3 самых читаемых книг");

            var booksOfAuthors = await context.Books
                .AsNoTracking()
                .Where(b => b.Borrows.Count > 0)
                .Select(b => new { b.Title, count = b.Borrows.Count })
                .OrderByDescending(b => b.count)
                .Take(3)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();

            resultBuilder.AppendLine("Топ-3 самых читаемых книг:");
            int rank = 1;
            foreach (var b in booksOfAuthors)
            {
                resultBuilder.AppendLine($"{rank++}. Книгу {b.Title} взяли {b.count} раз.");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Добавляем отзывы 
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача: Добавление 9 отзывов");

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

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);
            ConsoleOutputHelper.WriteTaskResult($"{newReviews.Count} отзывов добавлено успешно для разных книг.");
        }

        // Выводим хорошие и популярные книги №16
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 16: Расчет среднего рейтинга для книг с отзывами");

            var books = await context.Books
                .AsNoTracking()
                .Where(b => b.Reviews.Count > 0)
                .Select(b => new { b.Title, rating = b.Reviews.Average(r => r.Rating) })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            
            resultBuilder.AppendLine($"Найдено {books.Count} книг со средним рейтингом:");
            foreach (var book in books)
            {
                // Округляем рейтинг для вывода
                resultBuilder.AppendLine($"Книга с названием {book.Title} имеет рейтинг {book.rating.ToString("N2")}");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Выводим хорошие и популярные книги №17
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 17: Книги с высоким рейтингом (более 1 отзыва, средний рейтинг > 4)");

            var books = await context.Books
                .AsNoTracking()
                .Where(b => b.Reviews.Count > 1)
                .Select(b => new { b.Title, rating = b.Reviews.Average(r => r.Rating) })
                .Where(b => b.rating > 4)
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"Найдено {books.Count} 'успешных' книг:");
            foreach (var book in books)
            {
                resultBuilder.AppendLine($"Книга с названием {book.Title} имеет рейтинг {book.rating.ToString("N2")}");
            }
            
            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Выводим хорошие и популярные книги №19
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 19: Пагинация (Выборка данных в памяти)");

            int maxPageCount = 0;
            int pageSize = 2;

            var books = await context.Books.AsNoTracking().OrderBy(b => b.Id).ToListAsync();
            int totalItems = books.Count();
            maxPageCount = Convert.ToInt32(books.Count() / pageSize);

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"Всего книг: {totalItems}. Размер страницы: {pageSize}. Цикл пройдет {maxPageCount + 1} раз.");

            for (int page = 0; page <= maxPageCount; page++)
            {
                var bookOfPage = await context.Books.OrderBy(b=>b.Id).Skip(page * pageSize).Take(pageSize).ToListAsync();

                resultBuilder.AppendLine($"\n--- Страница {page} ---");

                if (bookOfPage.Any())
                {
                    foreach (var book in bookOfPage)
                    {
                        resultBuilder.AppendLine($"[Книга]: {book.Title}");
                    }
                }
                else
                {
                    // Эта ветка нужна, если последний элемент попал в 'лишнюю' страницу из-за целочисленного деления (если maxPageCount рассчитан с округлением вниз)
                    resultBuilder.AppendLine("На этой странице нет книг (конец списка).");
                }
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }

        // Какой то сложный запрос №20
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            ConsoleOutputHelper.WriteTaskTitle("Задача 20: Сложный запрос с проекцией в DTO");

            var books = await context.Books.AsNoTracking()
                .Where(b => b.Title == "Книга Пяти Колец")
                .Select(b => new
                {
                    b.Title,
                    b.Author,
                    PublisherName = b.PublisherEntity.Name,
                    AvgRating = b.Reviews.Average(r => r.Rating),
                    BorrowCount = b.Borrows.Count()
                })
                .ToListAsync();

            ConsoleOutputHelper.WriteTaskStatus(TaskStatus.Success);

            var resultBuilder = new StringBuilder();
            if (books.Any())
            {
                var bookDTO = books.First();
                resultBuilder.AppendLine($"DTO для книги: {bookDTO.Title}");
                resultBuilder.AppendLine($"Автор: {bookDTO.Author}");
                resultBuilder.AppendLine($"Издатель: {bookDTO.PublisherName}");
                resultBuilder.AppendLine($"Средний рейтинг: {bookDTO.AvgRating.ToString("N2")}");
                resultBuilder.AppendLine($"Количество выдач: {bookDTO.BorrowCount}");
            }

            ConsoleOutputHelper.WriteTaskResult(resultBuilder.ToString());
        }
    }

}