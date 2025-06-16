using Gemiyur.Comparers;
using Govorun.Models;

namespace Govorun.Tools;

/// <summary>
/// Статический класс работы с библиотекой.
/// </summary>
/// <remarks>
/// Книги, авторы, серии, чтецы, переводчики.
/// </remarks>
public static class Library
{
    /// <summary>
    /// Список всех книг.
    /// </summary>
    public static readonly List<Book> Books;

    ///// <summary>
    ///// Список всех авторов.
    ///// </summary>
    //public static readonly List<Author> Authors;

    ///// <summary>
    ///// Список всех серий.
    ///// </summary>
    //public static readonly List<Cycle> Cycles;

    /// <summary>
    /// Возвращает список слушаемых книг.
    /// </summary>
    /// <remarks>Книги отсортированы по названию.</remarks>
    public static List<Book> ListeningBooks =>
        [.. Books.FindAll(x => x.PlayPosition > TimeSpan.Zero)
                 .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Возвращает список всех чтецов.
    /// </summary>
    /// <remarks>Чтецы отсортированы по названию.</remarks>
    public static List<string> Lectors =>
        [.. Books.Select(x => x.Lector)
                 .Where(x => !string.IsNullOrWhiteSpace(x))
                 .Distinct()
                 .Order(StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Возвращает список всех переводчиков.
    /// </summary>
    /// <remarks>Переводчики отсортированы по названию.</remarks>
    public static List<string> Translators =>
        [.. Books.Select(x => x.Translator)
                 .Where(x => !string.IsNullOrWhiteSpace(x))
                 .Distinct()
                 .Order(StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Возвращает список всех тегов.
    /// </summary>
    /// <remarks>Теги отсортированы по названию.</remarks>
    public static List<string> Tags
    {
        get
        {
            var allTags = new List<string>();
            foreach (var book in Books)
            {
                allTags.AddRange(book.Tags.FindAll(x => !string.IsNullOrWhiteSpace(x) && !allTags.Contains(x)));
            }
            return [.. allTags.Distinct().Order(StringComparer.CurrentCultureIgnoreCase)];
        }
    }

    /// <summary>
    /// Статический конструктор.
    /// </summary>
    static Library()
    {
        Books = Db.GetBooks();
        //Authors = Db.GetAuthors();
        //Cycles = Db.GetCycles();
    }

    /// <summary>
    /// Возвращает является ли указанный автор автором указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <param name="authorId">Идентификатор автора.</param>
    /// <returns>Является ли указанный автор автором указанной книги.</returns>
    public static bool BookHasAuthor(Book book, int authorId) => book.Authors.Exists(x => x.AuthorId == authorId);

    /// <summary>
    /// Возвращает входит ли указанная книга в указанную серию.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <param name="cycleId">Идентификатор серии.</param>
    /// <returns>Входит ли указанная книга в указанную серию.</returns>
    public static bool BookInCycle(Book book, int cycleId) => book.Cycle != null && book.Cycle.CycleId == cycleId;

    /// <summary>
    /// Возвращает есть ли книга с указанным именем файла.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Есть ли книга с указанным именем файла.</returns>
    public static bool BookWithFileExists(string filename) =>
        Books.Exists(x => x.FileName.Equals(filename, StringComparison.CurrentCultureIgnoreCase));

    /// <summary>
    /// Возвращает список книг указанного автора.
    /// </summary>
    /// <param name="authorId">Идентификатор автора.</param>
    /// <returns>Список книг указанного автора.</returns>
    /// <remarks>Книги отсортированы по названию.</remarks>
    public static List<Book> GetAuthorBooks(int authorId) =>
        [.. Books.FindAll(x => BookHasAuthor(x, authorId)).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Возвращает список книг указанной серии.
    /// </summary>
    /// <param name="cycleId">Идентификатор серии.</param>
    /// <returns>Список книг указанной серии.</returns>
    /// <remarks>Книги отсортированы по номеру в серии и названию при одинаковых номерах.</remarks>
    public static List<Book> GetCycleBooks(int cycleId)
    {
        var comparer = new MultiKeyComparer(
            [new IntKeyComparer(x => ((Book)x).CycleNumber), new StringKeyComparer(x => ((Book)x).Title)]);
        var books = Books.FindAll(x => BookInCycle(x, cycleId));
        books.Sort(comparer);
        return books;
    }

    /// <summary>
    /// Возвращает список книг с указанным тегом.
    /// </summary>
    /// <param name="tag">Тег.</param>
    /// <returns>Список книг с указанным тегом.</returns>
    /// <remarks>Книги отсортированы по названию.</remarks>
    public static List<Book> GetTagBooks(string tag) =>
        [.. Books.FindAll(x => x.Tags.Contains(tag)).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Возвращает книгу с указанным именем файла.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Книга с указанным именем файла.</returns>
    public static Book? GetBookWithFile(string filename) =>
        Books.Find(x => x.FileName.Equals(filename, StringComparison.CurrentCultureIgnoreCase));

    #region Методы добавления, удаления и обновления книг, авторов и серий.

    /// <summary>
    /// Удаляет книгу из библиотеки и возвращает удалось ли удалить книгу.
    /// </summary>
    /// <param name="book">Книга для удаления.</param>
    /// <returns>Удалось ли удалить книгу.</returns>
    public static bool DeleteBook(Book book)
    {
        if (!Db.DeleteBook(book.BookId))
            return false;
        Books.Remove(book);
        return true;
    }

    /// <summary>
    /// Удаляет список книг из библиотеки и возвращает список удалённых книг.
    /// </summary>
    /// <param name="books">Список книг для удаления.</param>
    /// <returns>Список удалённых книг.</returns>
    public static List<Book> DeleteBooks(IEnumerable<Book> books)
    {
        List<Book> result = [];
        using var db = Db.GetDatabase();
        var collection = Db.GetBooksCollection(db);
        foreach (var book in books)
        {
            if (!collection.Delete(book.BookId))
                continue;
            result.Add(book);
            Books.Remove(book);
        }
        return result;
    }

    /// <summary>
    /// Обновляет книги из списка в библиотеке и возвращает список обновлённых книг.
    /// </summary>
    /// <param name="books">Список книг для обновления.</param>
    /// <returns>Список обновлённых книг.</returns>
    public static List<Book> UpdateBooks(IEnumerable<Book> books)
    {
        List<Book> result = [];
        using var db = Db.GetDatabase();
        var collection = Db.GetBooksCollection(db);
        foreach (var book in books)
        {
            if (collection.Update(book))
                result.Add(book);
        }
        return result;
    }

    #endregion
}
