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
        /// Статический конструктор.
        /// </summary>
        static Books()
        {
            AllBooks = Db.GetBooks();
        }
    }
}
