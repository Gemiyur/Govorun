using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Govorun.Models;

namespace Govorun.Tools
{
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
        public static bool BookHasAuthor(Book book, int authorId) => book.Authors.Any(x => x.AuthorId == authorId);

        /// <summary>
        /// Возвращает список книг указанного автора.
        /// </summary>
        /// <param name="authorId">Идентификатор автора.</param>
        /// <returns>Список книг указанного автора.</returns>
        public static List<Book> GetAuthorBooks(int authorId) => AllBooks.FindAll(x => BookHasAuthor(x, authorId));
    }
}
