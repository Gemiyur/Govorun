using LiteDB;

namespace Govorun.Models
{
    /// <summary>
    /// Класс главы книги.
    /// </summary>
    public class Chapter : BaseModel
    {
        private TimeSpan startTime;

        /// <summary>
        /// Позиция начала главы книги.
        /// </summary>
        public TimeSpan StartTime
        {
            get => startTime;
            set
            {
                startTime = value;
                OnPropertyChanged("StartTime");
            }
        }

        /// <summary>
        /// Позиция начала главы книги в виде строки.
        /// </summary>
        [BsonIgnore]
        public string StartTimeText => App.TimeSpanToString(StartTime);

        private TimeSpan endTime;

        /// <summary>
        /// Позиция конца главы книги.
        /// </summary>
        public TimeSpan EndTime
        {
            get => endTime;
            set
            {
                endTime = value;
                OnPropertyChanged("EndTime");
            }
        }

        /// <summary>
        /// Продолжительность главы книги.
        /// </summary>
        [BsonIgnore]
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Продолжительность главы книги в виде строки.
        /// </summary>
        [BsonIgnore]
        public string DurationText => App.TimeSpanToString(Duration);

        private string title = string.Empty;

        /// <summary>
        /// Название главы книги.
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
