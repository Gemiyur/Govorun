using LiteDB;
using Govorun.Models;

namespace Govorun.Tools
{
    /// <summary>
    /// Статический класс методов работы с базой данных.
    /// </summary>
    public static class Db
    {
        public static LiteDatabase GetDatabase() => new(App.DbName);

        public static long Shrink()
        {
            using var db = GetDatabase();
            return db.Rebuild();
        }

        public static void GenerateTestDb()
        {
            using var db = GetDatabase();

            var book = new Book() { Title = "Книга 1", Author = "Иван Петров", Lector = "Вера Надеждина" };
            var chapter = new Chapter { Title = "Книга 1. Часть 1" };
            var subchapter = new Chapter { Title = "Книга 1. Часть 1. Глава 1" };
            chapter.Chapters.Add(subchapter);
            subchapter = new Chapter { Title = "Книга 1. Часть 1. Глава 2" };
            chapter.Chapters.Add(subchapter);
            subchapter = new Chapter { Title = "Книга 1. Часть 1. Глава 3" };
            chapter.Chapters.Add(subchapter);
            book.Chapters.Add(chapter);
            chapter = new Chapter { Title = "Книга 1. Часть 2" };
            subchapter = new Chapter { Title = "Книга 1. Часть 2. Глава 1" };
            chapter.Chapters.Add(subchapter);
            subchapter = new Chapter { Title = "Книга 1. Часть 2. Глава 2" };
            chapter.Chapters.Add(subchapter);
            book.Chapters.Add(chapter);
            InsertBook(book, db);

            book = new Book() { Title = "Книга 2", Author = "Пётр Сидоров", Lector = "Любовь Надеждина" };
            chapter = new Chapter { Title = "Книга 2. Часть 1" };
            subchapter = new Chapter { Title = "Книга 2. Часть 1. Глава 1" };
            chapter.Chapters.Add(subchapter);
            subchapter = new Chapter { Title = "Книга 2. Часть 1. Глава 2" };
            chapter.Chapters.Add(subchapter);
            subchapter = new Chapter { Title = "Книга 2. Часть 1. Глава 3" };
            chapter.Chapters.Add(subchapter);
            book.Chapters.Add(chapter);
            chapter = new Chapter { Title = "Книга 2. Часть 2" };
            subchapter = new Chapter { Title = "Книга 2. Часть 2. Глава 1" };
            chapter.Chapters.Add(subchapter);
            subchapter = new Chapter { Title = "Книга 2. Часть 2. Глава 2" };
            chapter.Chapters.Add(subchapter);
            book.Chapters.Add(chapter);
            InsertBook(book, db);
        }

        public static ILiteCollection<Book> GetBooksCollection(LiteDatabase db) => db.GetCollection<Book>("Books");

        public static Book GetBook(int bookId)
        {
            using var db = GetDatabase();
            return GetBook(bookId, db);
        }

        public static Book GetBook(int bookId, LiteDatabase db) => GetBooksCollection(db).FindById(bookId);

        public static List<Book> GetBooks()
        {
            using var db = GetDatabase();
            return GetBooks(db);
        }

        public static List<Book> GetBooks(LiteDatabase db) =>
            GetBooksCollection(db)
                .FindAll()
                .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

        public static List<Book> GetAuthorBooks(string author)
        {
            using var db = GetDatabase();
            return GetAuthorBooks(author, db);
        }

        public static List<Book> GetAuthorBooks(string author, LiteDatabase db) =>
            GetBooksCollection(db)
                .FindAll()
                .Where(x => x.Author != null && x.Author.Equals(author, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

        public static List<Book> GetLectorBooks(string lector)
        {
            using var db = GetDatabase();
            return GetLectorBooks(lector, db);
        }

        public static List<Book> GetLectorBooks(string lector, LiteDatabase db) =>
            GetBooksCollection(db)
                .FindAll()
                .Where(x => x.Lector != null && x.Lector.Equals(lector, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

        public static int InsertBook(Book book)
        {
            using var db = GetDatabase();
            return InsertBook(book, db);
        }

        public static int InsertBook(Book book, LiteDatabase db) => GetBooksCollection(db).Insert(book);

        public static bool UpdateBook(Book book)
        {
            using var db = GetDatabase();
            return UpdateBook(book, db);
        }

        public static bool UpdateBook(Book book, LiteDatabase db) => GetBooksCollection(db).Update(book);
    }
}
