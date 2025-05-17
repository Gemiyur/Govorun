using LiteDB;
using Govorun.Models;

namespace Govorun.Tools;

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

    //public static void GenerateTestDb()
    //{
    //    using var db = GetDatabase();

    //    var author = new Author() { Name = "Иван", Surname = "Петров" };
    //    InsertAuthor(author, db);
    //    var book = new Book() { Title = "Книга 1", Lector = "Вера Надеждина" };
    //    book.Authors.Add(author);
    //    var chapter = new Chapter { Title = "Книга 1. Часть 1. Глава 1" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 1. Часть 1. Глава 2" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 1. Часть 1. Глава 3" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 1. Часть 2. Глава 1" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 1. Часть 2. Глава 2" };
    //    book.Chapters.Add(chapter);
    //    var bookmark = new Bookmark { Position = new TimeSpan(0, 0, 25), Title = "Книга 1. Закладка 1" };
    //    book.Bookmarks.Add(bookmark);
    //    book.PlayPosition = new TimeSpan(0, 15, 0);
    //    InsertBook(book, db);

    //    author = new Author() { Name = "Пётр", Surname = "Сидоров" };
    //    InsertAuthor(author, db);
    //    book = new Book() { Title = "Книга 2", Lector = "Любовь Надеждина" };
    //    book.Authors.Add(author);
    //    chapter = new Chapter { Title = "Книга 2. Часть 1. Глава 1" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 2. Часть 1. Глава 2" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 2. Часть 1. Глава 3" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 2. Часть 2. Глава 1" };
    //    book.Chapters.Add(chapter);
    //    chapter = new Chapter { Title = "Книга 2. Часть 2. Глава 2" };
    //    book.Chapters.Add(chapter);
    //    bookmark = new Bookmark { Position = new TimeSpan(0, 0, 25), Title = "Книга 2. Закладка 1" };
    //    book.Bookmarks.Add(bookmark);
    //    book.PlayPosition = new TimeSpan(0, 0, 25);
    //    InsertBook(book, db);

    //    author = new Author() { Name = "Аркадий и Борис", Surname = "Стругацкие" };
    //    InsertAuthor(author, db);
    //    book = new Book() { Title = "Трудно быть богом", Lector = "Аудиоспектакль" };
    //    book.Authors.Add(author);
    //    InsertBook(book, db);
    //    book = new Book() { Title = "Понедельник начинается в субботу", Lector = "Александр Левашов" };
    //    book.Authors.Add(author);
    //    InsertBook(book, db);

    //    author = new Author() { Name = "С.", Surname = "Витицкий" };
    //    InsertAuthor(author, db);
    //    book = new Book() { Title = "Поиск предназначения, или Двадцать седьмая теорема этики", Lector = "" };
    //    book.Authors.Add(author);
    //    InsertBook(book, db);

    //    book = new Book() { Title = "История рыжего демона", Lector = "Аудиоспектакль" };
    //    author = new Author() { Name = "Роджер", Surname = "Желязны" };
    //    InsertAuthor(author, db);
    //    book.Authors.Add(author);
    //    author = new Author() { Name = "Роберт", Surname = "Шекли" };
    //    InsertAuthor(author, db);
    //    book.Authors.Add(author);
    //    InsertBook(book, db);
    //}

    #region Получение коллекций.

    public static ILiteCollection<Author> GetAuthorsCollection(LiteDatabase db) => db.GetCollection<Author>("Authors");

    public static ILiteCollection<Book> GetBooksCollection(LiteDatabase db) => db.GetCollection<Book>("Books");

    #endregion

    #region Авторы.

    public static Author GetAuthor(int authorId)
    {
        using var db = GetDatabase();
        return GetAuthor(authorId, db);
    }

    public static Author GetAuthor(int authorId, LiteDatabase db) => GetAuthorsCollection(db).FindById(authorId);

    public static List<Author> GetAuthors()
    {
        using var db = GetDatabase();
        return GetAuthors(db);
    }

    public static List<Author> GetAuthors(LiteDatabase db) =>
        GetAuthorsCollection(db).FindAll().OrderBy(x => x.NameLastFirst, StringComparer.CurrentCultureIgnoreCase).ToList();

    public static int InsertAuthor(Author author)
    {
        using var db = GetDatabase();
        return InsertAuthor(author, db);
    }

    public static int InsertAuthor(Author author, LiteDatabase db) => GetAuthorsCollection(db).Insert(author);

    public static bool DeleteAuthor(int authorId)
    {
        using var db = GetDatabase();
        return DeleteAuthor(authorId, db);
    }

    public static bool DeleteAuthor(int authorId, LiteDatabase db) => GetAuthorsCollection(db).Delete(authorId);

    public static bool UpdateAuthor(Author author)
    {
        using var db = GetDatabase();
        return UpdateAuthor(author, db);
    }

    public static bool UpdateAuthor(Author author, LiteDatabase db) => GetAuthorsCollection(db).Update(author);

    #endregion

    #region Книги.

    public static Book GetBook(int bookId)
    {
        using var db = GetDatabase();
        return GetBook(bookId, db);
    }

    public static Book GetBook(int bookId, LiteDatabase db) =>
        GetBooksCollection(db).Include(x => x.Authors).FindById(bookId);

    public static List<Book> GetBooks()
    {
        using var db = GetDatabase();
        return GetBooks(db);
    }

    public static List<Book> GetBooks(LiteDatabase db) =>
        GetBooksCollection(db)
            .Include(x => x.Authors)
            .FindAll()
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    public static int InsertBook(Book book)
    {
        using var db = GetDatabase();
        return InsertBook(book, db);
    }

    public static int InsertBook(Book book, LiteDatabase db) => GetBooksCollection(db).Insert(book);

    public static bool DeleteBook(int bookId)
    {
        using var db = GetDatabase();
        return GetBooksCollection(db).Delete(bookId);
    }

    public static bool DeleteBook(Book book)
    {
        using var db = GetDatabase();
        return GetBooksCollection(db).Delete(book.BookId);
    }

    public static void DeleteBooks(IEnumerable<Book> books)
    {
        using var db = GetDatabase();
        var collection = GetBooksCollection(db);
        foreach (var book in books)
            collection.Delete(book.BookId);
    }

    public static bool UpdateBook(Book book)
    {
        using var db = GetDatabase();
        return UpdateBook(book, db);
    }

    public static bool UpdateBook(Book book, LiteDatabase db) => GetBooksCollection(db).Update(book);

    public static void UpdateBooks(IEnumerable<Book> books)
    {
        using var db = GetDatabase();
        var collection = GetBooksCollection(db);
        foreach (var book in books)
            collection.Update(book);
    }

    #endregion
}
