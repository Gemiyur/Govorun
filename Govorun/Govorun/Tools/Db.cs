using LiteDB;
using Govorun.Models;

namespace Govorun.Tools;

#region Задачи (TODO).

#endregion

/// <summary>
/// Статический класс методов работы с базой данных.
/// </summary>
/// <remarks>
/// Содержит основные (базовые) методы: вставка, удаление и обновление.<br/>
/// Методы удаления обеспечивают целостность данных,<br/>
/// не позволяя удалять авторов и серии, если на них есть ссылки у книг.
/// </remarks>
public static class Db
{
    public static LiteDatabase GetDatabase() => new(App.DbName);

    public static long Shrink()
    {
        using var db = GetDatabase();
        return db.Rebuild();
    }

    #region Получение коллекций.

    public static ILiteCollection<Book> GetBooksCollection(LiteDatabase db) => db.GetCollection<Book>("Books");

    public static ILiteCollection<Author> GetAuthorsCollection(LiteDatabase db) => db.GetCollection<Author>("Authors");

    public static ILiteCollection<Cycle> GetCyclesCollection(LiteDatabase db) => db.GetCollection<Cycle>("Cycles");

    #endregion

    #region Книги.

    public static Book GetBook(int bookId)
    {
        using var db = GetDatabase();
        return GetBook(bookId, db);
    }

    public static Book GetBook(int bookId, LiteDatabase db) =>
        GetBooksCollection(db)
            .Include(x => x.Authors)
            .Include(x => x.Cycle)
            .FindById(bookId);

    public static List<Book> GetBooks()
    {
        using var db = GetDatabase();
        return GetBooks(db);
    }

    public static List<Book> GetBooks(LiteDatabase db) =>
        GetBooksCollection(db)
            .Include(x => x.Authors)
            .Include(x => x.Cycle)
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

    public static bool DeleteBook(int bookId, LiteDatabase db) => GetBooksCollection(db).Delete(bookId);

    // TODO: Метод удаления списка книг здесь не нужен. Удаление списка книг должно быть в другом месте приложения.

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
        GetAuthorsCollection(db)
            .FindAll()
            .OrderBy(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

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

    public static bool DeleteAuthor(int authorId, LiteDatabase db)
    {
        var booksCollection = GetBooksCollection(db);
        if (booksCollection.Exists(x => x.Authors.Select(a => a.AuthorId).Any(id => id == authorId)))
            return false;
        return GetAuthorsCollection(db).Delete(authorId);
    }

    public static bool UpdateAuthor(Author author)
    {
        using var db = GetDatabase();
        return UpdateAuthor(author, db);
    }

    public static bool UpdateAuthor(Author author, LiteDatabase db) => GetAuthorsCollection(db).Update(author);

    #endregion

    #region Серии.

    public static Cycle GetCycle(int cycleId)
    {
        using var db = GetDatabase();
        return GetCycle(cycleId, db);
    }

    public static Cycle GetCycle(int cycleId, LiteDatabase db) => GetCyclesCollection(db).FindById(cycleId);

    public static List<Cycle> GetCycles()
    {
        using var db = GetDatabase();
        return GetCycles(db);
    }

    public static List<Cycle> GetCycles(LiteDatabase db) =>
        GetCyclesCollection(db)
            .FindAll()
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    public static int InsertCycle(Cycle cycle)
    {
        using var db = GetDatabase();
        return InsertCycle(cycle, db);
    }

    public static int InsertCycle(Cycle cycle, LiteDatabase db) => GetCyclesCollection(db).Insert(cycle);

    public static bool DeleteCycle(int cycleId)
    {
        using var db = GetDatabase();
        return DeleteCycle(cycleId, db);
    }

    public static bool DeleteCycle(int cycleId, LiteDatabase db)
    {
        var booksCollection = GetBooksCollection(db);
        if (booksCollection.Exists(x => x.Cycle != null && x.Cycle.CycleId == cycleId))
            return false;
        return GetCyclesCollection(db).Delete(cycleId);
    }

    public static bool UpdateCycle(Cycle cycle)
    {
        using var db = GetDatabase();
        return UpdateCycle(cycle, db);
    }

    public static bool UpdateCycle(Cycle cycle, LiteDatabase db) => GetCyclesCollection(db).Update(cycle);

    #endregion
}
