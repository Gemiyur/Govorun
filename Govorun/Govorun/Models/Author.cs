using LiteDB;

namespace Govorun.Models
{
    /// <summary>
    /// Класс автора.
    /// </summary>
    public class Author : BaseModel
    {
        /// <summary>
        /// Идентификатор автора.
        /// </summary>
        public int AuthorId { get; set; }

        private string name = string.Empty;

        /// <summary>
        /// Имя автора.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value ?? string.Empty;
                OnPropertyChanged("Name");
                OnPropertyChanged("NameSurname");
                OnPropertyChanged("SurnameName");
            }
        }

        private string surname = string.Empty;

        /// <summary>
        /// Фамилия автора.
        /// </summary>
        public string Surname
        {
            get => surname;
            set
            {
                surname = value ?? string.Empty;
                OnPropertyChanged("Surname");
                OnPropertyChanged("NameSurname");
                OnPropertyChanged("SurnameName");
            }
        }

        /// <summary>
        /// Имя и фамилия автора.
        /// </summary>
        [BsonIgnore]
        public string NameSurname => GetFullName(Name, Surname);

        /// <summary>
        /// Фамилия и имя автора.
        /// </summary>
        [BsonIgnore]
        public string SurnameName => GetFullName(Surname, Name);

        /// <summary>
        /// Возвращает полное имя автора, составленное из имени и фамилии в заданном порядке.
        /// </summary>
        /// <param name="name1">Первое имя.</param>
        /// <param name="name2">Второе имя.</param>
        /// <returns>Полное имя автора.</returns>
        private static string GetFullName(string name1, string name2)
        {
            if (name1 == string.Empty)
                return name2;
            else if (name2 == string.Empty)
                return name1;
            else
                return $"{name1} {name2}";
        }
    }
}
