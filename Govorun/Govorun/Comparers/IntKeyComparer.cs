namespace Govorun.Comparers
{
    /// <summary>
    /// Класс компаратора объектов по их ключам типа int.
    /// </summary>
    /// <param name="keyGetter">Функция извлечения ключа типа int.</param>
    /// <param name="descending">Нужно ли сортировать в порядке убывания ключа.</param>
    /// <remarks>
    /// Примеры:<br/><br/>
    /// var myComparer = new IntKeyComparer(x => ((MyListItem)x).IntMember);<br/><br/>
    /// IntKeyComparer myComparer = new(x => ((MyListItem)x).IntMember);
    /// </remarks>
    public class IntKeyComparer(Func<object, int> keyGetter, bool descending = false) : IComparer<object>
    {
        /// <summary>
        /// Функция извлечения ключа типа int.
        /// </summary>
        private readonly Func<object?, int> keyGetter = (Func<object?, int>)keyGetter;

        /// <summary>
        /// Нужно ли сортировать в порядке убывания ключа.
        /// </summary>
        public bool Descending { get; set; } = descending;

        /// <summary>
        /// Сравнивает два объекта по их ключам типа int.
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
