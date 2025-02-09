namespace Govorun.Media
{
    /// <summary>
    /// Класс главы книги.
    /// </summary>
    public class ChapterData
    {
        /// <summary>
        /// Название главы книги.
        /// </summary>
        public string Title = string.Empty;

        /// <summary>
        /// Время начала главы книги.
        /// </summary>
        public TimeSpan StartTime;

        /// <summary>
        /// Время конца главы книги.
        /// </summary>
        public TimeSpan EndTime;
    }
}
