namespace Govorun.Comparers
{
    /// <summary>
    /// Класс компаратора объектов по их ключам типа string.
    /// </summary>
    /// <param name="keyGetter">Функция извлечения ключа типа string.</param>
    /// <param name="comparisonType">Правила сортировки строк ключей.</param>
    /// <param name="descending">Нужно ли сортировать в порядке убывания ключа.</param>
    /// <remarks>
    /// По-умолчанию используются:<br/>
    /// - правила сортировки текущего языка без учёта регистра символов<br/>
    /// - сортировка в порядке возрастания ключа<br/><br/>
    /// Примеры:<br/><br/>
    /// var myComparer = new StringKeyComparer(x => ((MyListItem)x).StringMember);<br/><br/>
    /// StringKeyComparer myComparer = new(x => ((MyListItem)x).StringMember);
    /// </remarks>
    public class StringKeyComparer(
        Func<object, string> keyGetter,
        StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase,
        bool descending = false) : IComparer<object>
    {
        /// <summary>
        /// Функция извлечения ключа типа string.
        /// </summary>
        private readonly Func<object?, string> keyGetter = (Func<object?, string>)keyGetter;

        /// <summary>
        /// Правила сортировки.
        /// </summary>
        public StringComparison ComparisonType { get; set; } = comparisonType;

        /// <summary>
        /// Нужно ли сортировать в порядке убывания ключа.
        /// </summary>
        public bool Descending { get; set; } = descending;

        /// <summary>
        /// Сравнивает два объекта по их ключам типа string.
        /// </summary>
        /// <param name="x">Первый объект.</param>
        /// <param name="y">Второй объект.</param>
        /// <returns>Результат сравнения.</returns>
        public int Compare(object? x, object? y) =>
            keyGetter == null
                ? 0
                : Descending
                    ? CompareResult.Invert(string.Compare(keyGetter(x), keyGetter(y), ComparisonType))
                    : string.Compare(keyGetter(x), keyGetter(y), ComparisonType);
    }
}
