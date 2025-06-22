using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Govorun.Tools;

public class ObservableCollectionExNew<T> : ObservableCollection<T>
{
    /// <summary> 
    /// Добавляет элементы указанной коллекции в конец коллекции ObservableCollection.
    /// </summary> 
    /// <param name="collection">Коллекция добавляемых элементов.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
            Items.Add(item);
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary> 
    /// Удаляет первое появление каждого элемента в указанной коллекции из ObservableCollection.
    /// </summary> 
    /// <param name="collection">Коллекция удаляемых элементов.</param>
    public void RemoveRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
            Items.Remove(item);
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary> 
    /// Очищает текущую коллекцию и заменяет её указанным элементом.
    /// </summary> 
    /// <param name="item">Элемент, которым заменяются элементы коллекции.</param>
    public void Replace(T item) => ReplaceRange([item]);

    /// <summary> 
    /// Очищает текущую коллекцию и заменяет её элементами указанной коллекции.
    /// </summary> 
    /// <param name="collection">Коллекция, элементами которой заменяются элементы коллекции.</param>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        Items.Clear();
        AddRange(collection);
    }

    /// <summary>
    /// Сортирует элементы коллекции в порядке возрастания ключа.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого параметром <paramref name="keySelector"/>.</typeparam>
    /// <param name="keySelector">Функция извлечения ключа из элемента.</param>
    public void Sort<TKey>(Func<T, TKey> keySelector) =>
        InternalSort(Items.OrderBy(keySelector));

    /// <summary>
    /// Сортирует элементы коллекции в порядке возрастания ключа, используя указанный компаратор.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого параметром <paramref name="keySelector"/>.</typeparam>
    /// <param name="keySelector">Функция извлечения ключа из элемента.</param>
    /// <param name="comparer">Компаратор <see cref="IComparer{T}"/> для сравнения ключей.</param>
    public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer) =>
        InternalSort(Items.OrderBy(keySelector, comparer));

    /// <summary>
    /// Сортирует элементы коллекции в порядке убывания ключа.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого параметром <paramref name="keySelector"/>.</typeparam>
    /// <param name="keySelector">Функция извлечения ключа из элемента.</param>
    public void SortDescending<TKey>(Func<T, TKey> keySelector) =>
        InternalSort(Items.OrderByDescending(keySelector));

    /// <summary>
    /// Сортирует элементы коллекции в порядке убывания ключа, используя указанный компаратор.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого параметром <paramref name="keySelector"/>.</typeparam>
    /// <param name="keySelector">Функция извлечения ключа из элемента.</param>
    /// <param name="comparer">Компаратор <see cref="IComparer{T}"/> для сравнения ключей.</param>
    public void SortDescending<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer) =>
        InternalSort(Items.OrderByDescending(keySelector, comparer));

    /// <summary>
    /// Перемещает элементы коллекции, так что их порядок будут такими же, как у указанной коллекции.
    /// </summary>
    /// <param name="sortedItems">Коллекция <see cref="IEnumerable{T}"/>, содержащая элементы в требуемом порядке.</param>
    private void InternalSort(IEnumerable<T> sortedItems)
    {
        var sortedItemsList = sortedItems.ToList();
        foreach (var item in sortedItemsList)
            Move(IndexOf(item), sortedItemsList.IndexOf(item));
    }
}
