using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Govorun.Media;
using Govorun.Models;

namespace Govorun
{
    #region Задачи (TODO).

    #endregion

    /// <summary>
    /// Класс приложения.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Имя файла базы данных с полным путём.
        /// </summary>
        public static string? DbName { get; set; }

        /// <summary>
        /// Маски имён файлов книг.
        /// </summary>
        public static string[] BookFileMasks = ["*.m4b", "*.mp3"];

        /// <summary>
        /// Аналог System.Windows.Forms.Application.DoEvents.
        /// </summary>
        public static void DoEvents() =>
            Current?.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

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
            Path.GetExtension(filename).Equals(".db", StringComparison.CurrentCultureIgnoreCase)
                ? filename
                : filename + ".db";

        /// <summary>
        /// Возвращает BitmapImage из указанного файла изображения.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns></returns>
        public static BitmapImage GetBitmap(string path) => new BitmapImage(new Uri(path, UriKind.Relative));

        /// <summary>
        /// Возвращает книгу из указанного файла.
        /// В выходной параметр tag возвращает данные из тега.
        /// </summary>
        /// <param name="filename">Имя файла книги с полным путём.</param>
        /// <param name="tag">Данные тега из файла книги.</param>
        /// <returns>Книга.</returns>
        public static Book GetBookFromFile(string filename, out TrackData tag)
        {
            var book = new Book();
            tag = new TrackData(filename);
            book.Title = tag.Title;
            book.FileName = filename;
            book.Duration = tag.Duration;
            book.FileSize = new FileInfo(filename).Length;
            foreach (var chapter in tag.Chapters)
            {
                book.Chapters.Add(new Chapter()
                {
                    Title = chapter.Title,
                    StartTime = chapter.StartTime,
                    EndTime = chapter.EndTime
                });
            }
            return book;
        }

        #region Диалоги выбора файла и папки книг.

        /// <summary>
        /// Возвращает диалог выбора файла книги.
        /// </summary>
        public static OpenFileDialog PickBookFileDialog => new()
        {
            AddToRecent = false,
            Title = "Выбрать файл книги",
            Filter = $"Файлы книг|{ListToString(BookFileMasks, ";")}"
        };

        /// <summary>
        /// Возвращает диалог выбора папки с файлами книг.
        /// </summary>
        public static OpenFolderDialog PickBooksFolderDialog => new()
        {
            AddToRecent = false,
            Multiselect = true,
            Title = "Выбрать папку с файлами книг",
        };

        /// <summary>
        /// Возвращает диалог выбора файла базы данных.
        /// </summary>
        public static SaveFileDialog PickDatabaseDialog => new()
        {
            AddToRecent = false,
            CheckFileExists = false,
            OverwritePrompt = false,
            Title = "Файл базы данных",
            Filter = $"Файлы базы данных|*.db"
        };

        #endregion

        #region Получение строковых представлений значений.

        /// <summary>
        /// Возвращает строковое представление указанного логического значения на русском языке.
        /// </summary>
        /// <param name="value">Логическое значение.</param>
        /// <returns>Строковое представление логического значения на русском языке.</returns>
        public static string BoolToString(bool value) => value ? "Да" : "Нет";

        /// <summary>
        /// Возвращает строку, содержащую строки списка с указанным разделителем.
        /// </summary>
        /// <param name="list">Список строк.</param>
        /// <param name="separator">Разделитель.</param>
        /// <returns>Строка, содержащая строки списка с указанным разделителем.</returns>
        public static string ListToString(IEnumerable<string> list, string separator)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
                sb.Append(item == list.First() ? item : separator + item);
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает строку строк, извлечённых из списка объектов, с указанным разделителем.
        /// </summary>
        /// <param name="list">Список объектов.</param>
        /// <param name="separator">Разделитель.</param>
        /// <param name="stringSelector">Функция извлечения строки из объекта.</param>
        /// <returns>Строка строк, извлечённых из списка объектов, с указанным разделителем.</returns>
        public static string ListToString(IEnumerable<object> list, string separator, Func<object, string> stringSelector)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
                sb.Append(item == list.First() ? stringSelector(item) : separator + stringSelector(item));
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает строку отсортированных строк, извлечённых из списка объектов, с указанным разделителем.
        /// </summary>
        /// <param name="list">Список объектов.</param>
        /// <param name="separator">Разделитель.</param>
        /// <param name="stringSelector">Функция извлечения строки из объекта.</param>
        /// <param name="comparer">Компаратор строк.</param>
        /// <returns>Строка отсортированных строк, извлечённых из списка объектов, с указанным разделителем.</returns>
        public static string ListToString(IEnumerable<object> list, string separator,
                                          Func<object, string> stringSelector, IComparer<string> comparer)
        {
            var strings = list.Select(item => stringSelector(item)).ToList();
            strings.Sort(comparer);
            return ListToString(strings, separator);
        }

        /// <summary>
        /// Возвращает строковое представление указанного интервала времени в формате h:mm:ss.
        /// </summary>
        /// <param name="timeSpan">Интервал времени.</param>
        /// <returns>Строковое представление интервала времени.</returns>
        public static string TimeSpanToString(TimeSpan timeSpan) => timeSpan.ToString(@"h\:mm\:ss");

        #endregion
    }

}
