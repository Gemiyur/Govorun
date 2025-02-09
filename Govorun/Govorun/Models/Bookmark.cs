namespace Govorun.Models
{
    /// <summary>
    /// Класс закладки.
    /// </summary>
    public class Bookmark : BaseModel
    {
        private TimeSpan position;

        /// <summary>
        /// Позиция закладки в файле части книги.
        /// </summary>
        public TimeSpan Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged("Position");
            }
        }

        private string title = string.Empty;

        /// <summary>
        /// Название закладки.
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                title = value ?? string.Empty;
                OnPropertyChanged("Title");
            }
        }
    }
}
