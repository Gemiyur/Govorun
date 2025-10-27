using System.IO;
using LiteDB;
using Govorun.Models;

namespace Govorun.Tools;

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

    /// <summary>
    /// Возвращает указанное имя файла, гарантируя расширение .db.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Имя файла с расширением .db.</returns>
    /// <remarks>
    /// Если имя файла имеет расширение .db, то возвращает имя файла без изменений.<br/>
    /// Если имя файла имеет другое расширение, то к имени файла добавляет расширение .db.
    /// </remarks>
    public static string EnsureDbExtension(string filename) =>
        Path.GetExtension(filename).Equals(".litedb", StringComparison.CurrentCultureIgnoreCase)
            ? filename
            : filename + ".litedb";

    public static long Shrink()
    {
        using var db = GetDatabase();
        return db.Rebuild();
    }

    #region Получение коллекций.

    public static ILiteCollection<Book> GetBooksCollection(LiteDatabase db) => db.GetCollection<Book>("Books");

    public static ILiteCollection<Author> GetAuthorsCollection(LiteDatabase db) => db.GetCollection<Author>("Authors");

    public static ILiteCollection<Cycle> GetCyclesCollection(LiteDatabase db) => db.GetCollection<Cycle>("Cycles");

    public static ILiteCollection<Tag> GetTagsCollection(LiteDatabase db) => db.GetCollection<Tag>("Tags");

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
            .Include(x => x.Tags)
            .FindById(bookId);

    public static List<Book> GetBooks()
    {
        using var db = GetDatabase();
        return GetBooks(db);
    }

    public static List<Book> GetBooks(LiteDatabase db) =>
        [.. GetBooksCollection(db)
            .Include(x => x.Authors)
            .Include(x => x.Cycle)
            .Include(x => x.Tags)
            .FindAll()
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

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

    public static bool UpdateBook(Book book)
    {
        using var db = GetDatabase();
        return UpdateBook(book, db);
    }

    public static bool UpdateBook(Book book, LiteDatabase db) => GetBooksCollection(db).Update(book);

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
        [.. GetAuthorsCollection(db)
            .FindAll()
            .OrderBy(x => x.NameLastFirstMiddle, StringComparer.CurrentCultureIgnoreCase)];

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
        [.. GetCyclesCollection(db)
            .FindAll()
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

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

    #region Теги.

    public static Tag GetTag(int tagId)
    {
        using var db = GetDatabase();
        return GetTag(tagId, db);
    }

    public static Tag GetTag(int tagId, LiteDatabase db) => GetTagsCollection(db).FindById(tagId);

    public static List<Tag> GetTags()
    {
        using var db = GetDatabase();
        return GetTags(db);
    }

    public static List<Tag> GetTags(LiteDatabase db) =>
        [.. GetTagsCollection(db)
            .FindAll()
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

    public static int InsertTag(Tag tag)
    {
        using var db = GetDatabase();
        return InsertTag(tag, db);
    }

    public static int InsertTag(Tag tag, LiteDatabase db) => GetTagsCollection(db).Insert(tag);

    public static bool DeleteTag(int tagId)
    {
        using var db = GetDatabase();
        return DeleteTag(tagId, db);
    }

    public static bool DeleteTag(int tagId, LiteDatabase db)
    {
        var booksCollection = GetBooksCollection(db);
        if (booksCollection.Exists(x => x.Tags.Select(t => t.TagId).Any(id => id == tagId)))
            return false;
        return GetTagsCollection(db).Delete(tagId);
    }

    public static bool UpdateTag(Tag tag)
    {
        using var db = GetDatabase();
        return UpdateTag(tag, db);
    }

    public static bool UpdateTag(Tag tag, LiteDatabase db) => GetTagsCollection(db).Update(tag);

    #endregion
}
