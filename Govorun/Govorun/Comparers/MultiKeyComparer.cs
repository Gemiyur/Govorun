namespace Govorun.Comparers
{
    /// <summary>
    /// Класс компаратора объектов с последовательным применением нескольких компараторов.
    /// </summary>
    /// <param name="comparers">Список компараторов для сравнения объектов.</param>
    /// <remarks>
    /// Примеры:<br/><br/>
    /// var myComparer = new MultiKeyComparer(<br/>
    /// [new IntKeyComparer(x => ((MyListItem)x).IntMember), new StringKeyComparer(x => ((MyListItem)x).StringMember)]);<br/><br/>
    /// MultiKeyComparer myComparer =<br/>
    /// new([new IntKeyComparer(x => ((MyListItem)x).IntMember), new StringKeyComparer(x => ((MyListItem)x).StringMember)]);
    /// </remarks>
    public class MultiKeyComparer(List<IComparer<object>> comparers) : IComparer<object>
    {
        /// <summary>
        /// Список компараторов для сравнения объектов.
        /// </summary>
        public List<IComparer<object>> Comparers { get; } = comparers;

        /// <summary>
        /// Сравнивает два объекта, последовательно применяя компараторы из списка компараторов до первого неравенства.
        /// </summary>
        /// <param name="x">Первый объект.</param>
        /// <param name="y">Второй объект.</param>
        /// <returns>Результат сравнения.</returns>
        public int Compare(object? x, object? y)
        {
            foreach (var comparer in Comparers)
            {
                int result = comparer.Compare(x, y);
                if (result != 0)
                    return result;
            }
            return 0;
        }
    }
}
