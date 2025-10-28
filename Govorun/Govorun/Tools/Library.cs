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

    /// <summary>
    /// Возвращает список слушаемых книг.
    /// </summary>
    /// <remarks>Книги отсортированы по названию.</remarks>
    public static List<Book> ListeningBooks =>
        [.. Books.FindAll(x => x.Listening).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

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
    /// Статический конструктор.
    /// </summary>
    static Library()
    {
        Books = Db.GetBooks();
    }

    /// <summary>
    /// Возвращает имеет ли указанный автор книги.
    /// </summary>
    /// <param name="authorId">Идентификатор автора.</param>
    /// <returns>Имеет ли указанный автор книги.</returns>
    public static bool AuthorHasBooks(int authorId) => Books.Any(x => BookHasAuthor(x, authorId));

    /// <summary>
    /// Возвращает имеет ли указанная серия книги.
    /// </summary>
    /// <param name="cycleId">Идентификатор серии.</param>
    /// <returns>Имеет ли указанная серия книги.</returns>
    public static bool CycleHasBooks(int cycleId) => Books.Any(x => BookInCycle(x, cycleId));

    /// <summary>
    /// Возвращает имеет ли указанный тег книги.
    /// </summary>
    /// <param name="tagId">Идентификатор тега.</param>
    /// <returns>Имеет ли указанный тег книги.</returns>
    public static bool TagHasBook(int tagId) => Books.Any(x => BookHasTag(x, tagId));

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
    /// Возвращает имеет ли указанная книга указанный тег.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <param name="tagId">Идентификатор тега.</param>
    /// <returns>Имеет ли указанная книга указанный тег.</returns>
    public static bool BookHasTag(Book book, int tagId) => book.Tags.Exists(x => x.GenreId == tagId);

    /// <summary>
    /// Возвращает есть ли книга с указанным именем файла.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Есть ли книга с указанным именем файла.</returns>
    public static bool BookWithFileExists(string filename) =>
        Books.Exists(x => x.FileName.Equals(filename, StringComparison.CurrentCultureIgnoreCase));

    /// <summary>
    /// Возвращает книгу с указанным идентификатором.
    /// </summary>
    /// <param name="bookId">Идентификатор книги.</param>
    /// <returns>Книга с указанным идентификатором.</returns>
    public static Book? GetBook(int bookId) => Books.Find(x => x.BookId == bookId);

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
        var comparer = new SmartStringKeyComparer(x => ((Book)x).CycleNumbers);
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
    public static List<Book> GetTagBooks(int tagId) =>
        [.. Books.FindAll(x => BookHasTag(x, tagId)).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// Закрывает открытые окна указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public static void CloseBookWindows(Book book)
    {
        var bookInfoWindow = App.FindBookInfoWindow();
        if (bookInfoWindow != null && bookInfoWindow.Book == book)
            bookInfoWindow.Close();
        var bookmarksWindow = App.FindBookmarksWindow();
        if (bookmarksWindow != null && bookmarksWindow.Book == book)
            bookmarksWindow.Close();
        var chaptersWindow = App.FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book)
            chaptersWindow.Close();
    }

    /// <summary>
    /// Удаляет книгу из библиотеки и возвращает удалось ли удалить книгу.
    /// </summary>
    /// <param name="book">Книга для удаления.</param>
    /// <returns>Удалось ли удалить книгу.</returns>
    public static bool DeleteBook(Book book)
    {
        CloseBookWindows(book);
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
            CloseBookWindows(book);
            if (!collection.Delete(book.BookId))
                continue;
            result.Add(book);
            Books.Remove(book);
        }
        return result;
    }

    /// <summary>
    /// Обновляет открытые окна указанной книги.
    /// </summary>
    /// <param name="book">Книга.</param>
    public static void UpdateBookWindows(Book book)
    {
        var bookInfoWindow = App.FindBookInfoWindow();
        if (bookInfoWindow != null && bookInfoWindow.Book == book)
            bookInfoWindow.UpdateBook();
        var bookmarksWindow = App.FindBookmarksWindow();
        if (bookmarksWindow != null && bookmarksWindow.Book == book)
            bookmarksWindow.UpdateBook();
        var chaptersWindow = App.FindChaptersWindow();
        if (chaptersWindow != null && chaptersWindow.Book == book)
            chaptersWindow.UpdateBook();
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
            {
                var bookInfoWindow = App.FindBookInfoWindow();
                if (bookInfoWindow != null && bookInfoWindow.Book == book)
                    bookInfoWindow.UpdateFile();
                result.Add(book);
            }
        }
        return result;
    }
}
