namespace Govorun.Comparers
{
    /// <summary>
    /// Статический класс методов для результата сравнения.
    /// </summary>
    public static class CompareResult
    {
        /// <summary>
        /// Возвращает инвертированный результат сравнения.
        /// </summary>
        /// <param name="compareResult">Результат сравнения.</param>
        /// <returns>Инвертированный результат сравнения.</returns>
        public static int Invert(int compareResult)
        {
            if (compareResult < 0)
                return 1;
            if (compareResult > 0)
                return -1;
            else
                return 0;
        }
    }
}
