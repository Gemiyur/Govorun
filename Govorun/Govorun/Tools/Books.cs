using Govorun.Models;

namespace Govorun.Tools;

/// <summary>
/// Статический класс работы со списком книг.
/// </summary>
public static class Books
{
    /// <summary>
    /// Список всех книг.
    /// </summary>
    public static readonly List<Book> AllBooks;

    /// <summary>
    /// Возвращает список всех чтецов.
    /// </summary>
    public static List<string> Lectors =>
        AllBooks.Select(x => x.Lector)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList();

    /// <summary>
    /// Возвращает список всех переводчиков.
    /// </summary>
    public static List<string> Translators =>
        AllBooks.Select(x => x.Translator)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList();

    /// <summary>
    /// Возвращает список всех тегов.
    /// </summary>
    public static List<string> Tags
    {
        get
        {
            var allTags = new List<string>();
            foreach (var book in AllBooks)
            {
                allTags.AddRange(book.Tags.FindAll(x => !string.IsNullOrWhiteSpace(x) && !allTags.Contains(x)));
            }

            return allTags.Distinct().Order(StringComparer.CurrentCultureIgnoreCase).ToList();
        }
    }

    /// <summary>
    /// Статический конструктор.
    /// </summary>
    static Books()
    {
        AllBooks = Db.GetBooks();
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
        AllBooks.Exists(x => x.FileName.Equals(filename, StringComparison.CurrentCultureIgnoreCase));

    /// <summary>
    /// Возвращает список книг указанного автора.
    /// </summary>
    /// <param name="authorId">Идентификатор автора.</param>
    /// <returns>Список книг указанного автора.</returns>
    public static List<Book> GetAuthorBooks(int authorId) => AllBooks.FindAll(x => BookHasAuthor(x, authorId));

    /// <summary>
    /// Возвращает список книг указанной серии.
    /// </summary>
    /// <param name="cycleId">Идентификатор серии.</param>
    /// <returns>Список книг указанной серии.</returns>
    public static List<Book> GetCycleBooks(int cycleId) => AllBooks.FindAll(x => BookInCycle(x, cycleId));

    /// <summary>
    /// Возвращает книгу с указанным именем файла.
    /// </summary>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Книга с указанным именем файла.</returns>
    public static Book? GetBookWithFile(string filename) =>
        AllBooks.Find(x => x.FileName.Equals(filename, StringComparison.CurrentCultureIgnoreCase));

    /// <summary>
    /// Возвращает список слушаемых книг отсортированных по названию.
    /// </summary>
    /// <returns>Список слушаемых книг отсортированных по названию.</returns>
    public static List<Book> GetListeningBooks() =>
        AllBooks.FindAll(x => x.PlayPosition > TimeSpan.Zero)
                .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
}
