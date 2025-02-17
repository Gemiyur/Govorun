using LiteDB;

namespace Govorun.Models
{
    /// <summary>
    /// Класс закладки книги.
    /// </summary>
    public class Bookmark : BaseModel
    {
        private TimeSpan position;

        /// <summary>
        /// Позиция закладки книги.
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

        /// <summary>
        /// Позиция закладки книги в виде строки.
        /// </summary>
        [BsonIgnore]
        public string PositionText => App.TimeSpanToString(Position);

        private string title = string.Empty;

        /// <summary>
        /// Название закладки книги.
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
