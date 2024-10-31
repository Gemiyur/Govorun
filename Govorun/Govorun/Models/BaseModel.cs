using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Govorun.Models
{
    /// <summary>
    /// Класс базовой модели, от которого наследуются все классы моделей.
    /// Реализует интерфейс INotifyPropertyChanged.
    /// </summary>
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
