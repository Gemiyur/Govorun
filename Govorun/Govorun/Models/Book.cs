using LiteDB;
using System.IO;

namespace Govorun.Models
{
    /// <summary>
    /// Класс книги.
    /// </summary>
    public class Book : BaseModel
    {
        /// <summary>
        /// Идентификатор книги.
        /// </summary>
        public int BookId { get; set; }

        private string title = string.Empty;

        /// <summary>
        /// Название книги.
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

        /// <summary>
        /// Список авторов книги.
        /// </summary>
        [BsonRef("Authors")]
        public List<Author> Authors { get; set; } = [];

        /// <summary>
        /// Возвращает список авторов книги в виде строки.
        /// </summary>
        [BsonIgnore]
        public string AuthorsText =>
            App.ListToString(Authors, ", ", x => ((Author)x).SurnameName, StringComparer.CurrentCultureIgnoreCase);

        private string lector = string.Empty;

        /// <summary>
        /// Чтец книги.
        /// </summary>
        public string Lector
        {
            get => lector;
            set
            {
                lector = value ?? string.Empty;
                OnPropertyChanged("Lector");
            }
        }

        private string comment = string.Empty;

        /// <summary>
        /// Комментарий к книге.
        /// </summary>
        public string Comment
        {
            get => comment;
            set
            {
                comment = value ?? string.Empty;
                OnPropertyChanged("Comment");
            }
        }

        private string filename = string.Empty;

        /// <summary>
        /// Файл книги с полным путём.
        /// </summary>
        public string FileName // { get; set; } = string.Empty;
        {
            get => filename;
            set
            {
                filename = value ?? string.Empty;
                OnPropertyChanged("FileName");
            }
        }

        /// <summary>
        /// Существует ли файл книги.
        /// </summary>
        [BsonIgnore]
        public bool FileExists => File.Exists(FileName);

        /// <summary>
        /// Продолжительность аудио книги.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Продолжительность аудио книги в виде строки.
        /// </summary>
        [BsonIgnore]
        public string DurationText => App.TimeSpanToString(Duration);

        /// <summary>
        /// Позиция, с которой надо продолжить воспроизведение.
        /// </summary>
        public TimeSpan PlayPosition { get; set; }

        /// <summary>
        /// Находится ли книга в состоянии прослушивания.
        /// </summary>
        [BsonIgnore]
        public bool Listening => PlayPosition > TimeSpan.Zero;

        /// <summary>
        /// Список разделов книги.
        /// </summary>
        public List<Chapter> Chapters { get; set; } = [];

        /// <summary>
        /// Список закладок книги.
        /// </summary>
        public List<Bookmark> Bookmarks { get; set; } = [];
    }
}
