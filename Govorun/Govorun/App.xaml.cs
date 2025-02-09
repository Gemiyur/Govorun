using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Govorun
{
    #region Задачи (TODO).

    // TODOL: Файл Reset.png для кнопки сброса позиции прослушивания книги пока оставлен на всякий случай.

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
        /// Аналог System.Windows.Forms.Application.DoEvents.
        /// </summary>
        public static void DoEvents() =>
            Current?.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

        /// <summary>
        /// Возвращает BitmapImage из указанного файла изображения.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns></returns>
        public static BitmapImage GetBitmap(string path) => new BitmapImage(new Uri(path, UriKind.Relative));

        /// <summary>
        /// Возвращает логическое значение из логического значения, допускающего неопределённое значение.
        /// </summary>
        /// <param name="value">Логическое значение, допускающее неопределённое значение.</param>
        /// <returns>Логическое значение.</returns>
        public static bool SimpleBool(bool? value) => value ?? false;

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
