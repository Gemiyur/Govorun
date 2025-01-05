namespace Govorun.Comparers
{
    /// <summary>
    /// Класс компаратора объектов по их ключам типа TimeSpan.
    /// </summary>
    /// <param name="keyGetter">Функция извлечения ключа типа TimeSpan.</param>
    /// <param name="descending">Нужно ли сортировать в порядке убывания ключа.</param>
    /// <remarks>
    /// Примеры:<br/><br/>
    /// var myComparer = new TimeSpanKeyComparer(x => ((MyListItem)x).TimeSpanMember);<br/><br/>
    /// TimeSpanKeyComparer myComparer = new(x => ((MyListItem)x).TimeSpanMember);
    /// </remarks>
    public class TimeSpanKeyComparer(Func<object, TimeSpan> keyGetter, bool descending = false) : IComparer<object>
    {
        /// <summary>
        /// Функция извлечения ключа типа TimeSpan.
        /// </summary>
        private readonly Func<object?, TimeSpan> keyGetter = (Func<object?, TimeSpan>)keyGetter;

        /// <summary>
        /// Нужно ли сортировать в порядке убывания ключа.
        /// </summary>
        public bool Descending { get; set; } = descending;

        /// <summary>
        /// Сравнивает два объекта по их ключам типа TimeSpan.
        /// </summary>
        /// <param name="x">Первый объект.</param>
        /// <param name="y">Второй объект.</param>
        /// <returns>Результат сравнения.</returns>
        public int Compare(object? x, object? y) =>
            keyGetter == null
                ? 0
                : Descending
                    ? CompareResult.Invert(keyGetter(x).CompareTo(keyGetter(y)))
                    : keyGetter(x).CompareTo(keyGetter(y));
    }
}
