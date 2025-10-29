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
    /// Список всех авторов.
    /// </summary>
    public static readonly List<Author> Authors = [];

    /// <summary>
    /// Список всех серий.
    /// </summary>
    public static readonly List<Cycle> Cycles = [];

    /// <summary>
    /// Список всех жанров.
    /// </summary>
    public static readonly List<Genre> Genres = [];

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

        var authors = Books.SelectMany(x => x.Authors).Cast<Author>();
        Authors.AddRange(authors.Where(x => !Authors.Exists(a => a.AuthorId == x.AuthorId)));
        Authors.AddRange(Db.GetAuthors().Where(x => !Authors.Exists(a => a.AuthorId == x.AuthorId)));
        SortAuthors();

        var cycles = Books.Select(x => x.Cycle).Cast<Cycle>().Where(x => x != null);
        Cycles.AddRange(cycles.Where(x => !Cycles.Exists(c => c.CycleId == x.CycleId)));
        Cycles.AddRange(Db.GetCycles().Where(x => !Cycles.Exists(c => c.CycleId == x.CycleId)));
        SortCycles();

        var genres = Books.SelectMany(x => x.Genres).Cast<Genre>();
        Genres.AddRange(genres.Where(x => !Genres.Exists(g => g.GenreId == x.GenreId)));
        Genres.AddRange(Db.GetGenres().Where(x => !Genres.Exists(g => g.GenreId == x.GenreId)));
        SortGenres();
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
    /// <param name="genreId">Идентификатор тега.</param>
    /// <returns>Имеет ли указанный тег книги.</returns>
    public static bool GenreHasBook(int genreId) => Books.Any(x => BookHasGenre(x, genreId));

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
    /// <param name="genreId">Идентификатор тега.</param>
    /// <returns>Имеет ли указанная книга указанный тег.</returns>
    public static bool BookHasGenre(Book book, int genreId) => book.Genres.Exists(x => x.GenreId == genreId);

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

    #region Методы получения списков книг.

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
    public static List<Book> GetGenreBooks(int genreId) =>
        [.. Books.FindAll(x => BookHasGenre(x, genreId)).OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)];

    #endregion

    #region Методы сортировки списков.

    /// <summary>
    /// Сортирует список книг по названию.
    /// </summary>
    public static void SortBooks() => Books.Sort(new StringKeyComparer(x => ((Book)x).Title));

    /// <summary>
    /// Сортирует список авторов по фамилии-имени-отчеству.
    /// </summary>
    public static void SortAuthors() => Authors.Sort(new StringKeyComparer(x => ((Author)x).NameLastFirstMiddle));

    /// <summary>
    /// Сортирует список серий по названию.
    /// </summary>
    public static void SortCycles() => Cycles.Sort(new StringKeyComparer(x => ((Cycle)x).Title));

    /// <summary>
    /// Сортирует список жанров по названию.
    /// </summary>
    public static void SortGenres() => Genres.Sort(new StringKeyComparer(x => ((Genre)x).Title));

    #endregion

    #region Методы добавления, обновления и удаления.

    /// <summary>
    /// Добавляет книгу в библиотеку и возвращает удалось ли добавить книгу.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <returns>Удалось ли добавить книгу.</returns>
    public static bool AddBook(Book book)
    {
        var id = Db.InsertBook(book);
        if (id < 1)
            return false;
        book.BookId = id;
        Books.Add(book);
        SortBooks();
        return true;
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
    /// Обновляет книгу в библиотеке и возвращает удалось ли обновить книгу.
    /// </summary>
    /// <param name="book">Книга.</param>
    /// <returns>Удалось ли обновить книгу.</returns>
    public static bool UpdateBook(Book book)
    {
        if (Db.UpdateBook(book))
        {
            SortBooks();
            return true;
        }
        return false;
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
        SortBooks();
        return result;
    }

    /// <summary>
    /// Добавляет автора в библиотеку и возвращает удалось ли добавить автора.
    /// </summary>
    /// <param name="author">Автор.</param>
    /// <returns>Удалось ли добавить автора.</returns>
    public static bool AddAuthor(Author author)
    {
        var id = Db.InsertAuthor(author);
        if (id < 1)
            return false;
        author.AuthorId = id;
        Authors.Add(author);
        SortAuthors();
        return true;
    }

    /// <summary>
    /// Удаляет автора из библиотеки и возвращает удалось ли удалить автора.
    /// </summary>
    /// <param name="author">Автор.</param>
    /// <returns>Удалось ли удалить автора.</returns>
    public static bool DeleteAuthor(Author author)
    {
        if (!Db.DeleteAuthor(author.AuthorId))
            return false;
        Authors.Remove(author);
        return true;
    }

    /// <summary>
    /// Обновляет автора в библиотеке и возвращает удалось ли обновить автора.
    /// </summary>
    /// <param name="author">Автор.</param>
    /// <returns>Удалось ли обновить автора.</returns>
    public static bool UpdateAuthor(Author author)
    {
        if (Db.UpdateAuthor(author))
        {
            SortAuthors();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Добавляет серию в библиотеку и возвращает удалось ли добавить серию.
    /// </summary>
    /// <param name="cycle">Серия.</param>
    /// <returns>Удалось ли добавить серию.</returns>
    public static bool AddCycle(Cycle cycle)
    {
        var id = Db.InsertCycle(cycle);
        if (id < 1)
            return false;
        cycle.CycleId = id;
        Cycles.Add(cycle);
        SortCycles();
        return true;
    }

    /// <summary>
    /// Удаляет серию из библиотеки и возвращает удалось ли удалить серию.
    /// </summary>
    /// <param name="cycle">Серия.</param>
    /// <returns>Удалось ли удалить серию.</returns>
    public static bool DeleteCycle(Cycle cycle)
    {
        if (!Db.DeleteCycle(cycle.CycleId))
            return false;
        Cycles.Remove(cycle);
        return true;
    }

    /// <summary>
    /// Обновляет серию в библиотеке и возвращает удалось ли обновить серию.
    /// </summary>
    /// <param name="cycle">Серия.</param>
    /// <returns>Удалось ли обновить серию.</returns>
    public static bool UpdateCycle(Cycle cycle)
    {
        if (Db.UpdateCycle(cycle))
        {
            SortCycles();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Добавляет жанр в библиотеку и возвращает удалось ли добавить жанр.
    /// </summary>
    /// <param name="genre">Жанр.</param>
    /// <returns>Удалось ли добавить жанр.</returns>
    public static bool AddGenre(Genre genre)
    {
        var id = Db.InsertGenre(genre);
        if (id < 1)
            return false;
        genre.GenreId = id;
        Genres.Add(genre);
        SortGenres();
        return true;
    }

    /// <summary>
    /// Удаляет жанр из библиотеки и возвращает удалось ли удалить жанр.
    /// </summary>
    /// <param name="genre">Жанр.</param>
    /// <returns>Удалось ли удалить жанр.</returns>
    public static bool DeleteGenre(Genre genre)
    {
        if (!Db.DeleteGenre(genre.GenreId))
            return false;
        Genres.Remove(genre);
        return true;
    }

    /// <summary>
    /// Обновляет жанр в библиотеке и возвращает удалось ли обновить жанр.
    /// </summary>
    /// <param name="genre">Жанр.</param>
    /// <returns>Удалось ли обновить жанр.</returns>
    public static bool UpdateGenre(Genre genre)
    {
        if (Db.UpdateGenre(genre))
        {
            SortGenres();
            return true;
        }
        return false;
    }

    #endregion
}
