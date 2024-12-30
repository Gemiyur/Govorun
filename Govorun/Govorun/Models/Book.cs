using LiteDB;
using System.Windows.Media.Imaging;

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

        private string comment = string.Empty;

        /// <summary>
        /// Описание книги.
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
            App.ListToString(Authors, ", ", x => ((Author)x).NameSurname, StringComparer.CurrentCultureIgnoreCase);

        private string lector = string.Empty;

        /// <summary>
        /// Чтец книги.
        /// </summary>
        public string Lector
        {
            get => lector;
            set
            {
                lector = value;
                OnPropertyChanged("Lector");
            }
        }

        /// <summary>
        /// Файл книги с полным путём.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

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
        /// Изображение обложки книги.
        /// </summary>
        [BsonIgnore]
        public BitmapFrame? Picture => PictureData != null ? App.GetBitmap(PictureData) : null;

        /// <summary>
        /// Массив байт изображения обложки книги.
        /// </summary>
        public byte[]? PictureData;

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
