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
