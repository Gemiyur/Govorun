using LiteDB;

namespace Govorun.Models
{
    /// <summary>
    /// Класс раздела книги.
    /// </summary>
    public class Chapter : BaseModel
    {
        private TimeSpan startTime;

        /// <summary>
        /// Позиция начала раздела в файле части книги.
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

        private TimeSpan endTime;

        /// <summary>
        /// Позиция конца раздела в файле части книги.
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
        /// Продолжительность аудио раздела книги.
        /// </summary>
        [BsonIgnore]
        public TimeSpan Duration => EndTime - StartTime;

        private string title = string.Empty;

        /// <summary>
        /// Название раздела книги.
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Список подразделов раздела книги.
        /// </summary>
        public List<Chapter> Chapters { get; set; } = [];
    }
}
