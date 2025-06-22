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
    /// <param name="notificationMode">Действие, вызванное событием CollectionChanged.<br/>
    /// Должно быть Add (по умолчанию) или Reset.</param>
    /// <remarks>
    /// Не работает если количество элементов в коллекции больше одного.<br/>
    /// Выбрасывает исключение "Операции с диапазоном не поддерживаются".<br/>
    /// Почему так - непонятно. Разбираться пока нет желания.
    /// </remarks>
    public void AddRange(IEnumerable<T> collection,
                         NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        if (notificationMode != NotifyCollectionChangedAction.Add &&
            notificationMode != NotifyCollectionChangedAction.Reset)
            throw new ArgumentException("Режим должен быть либо Add, либо Reset для AddRange.", "NotificationMode");
        if (collection == null)
            throw new ArgumentNullException("Коллекция не должна быть null.");
        CheckReentrancy();
        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            foreach (var i in collection)
                Items.Add(i);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return;
        }
        var startIndex = Count;
        var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
        foreach (var i in changedItems)
            Items.Add(i);
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            changedItems, startIndex));
    }

    /// <summary> 
    /// Удаляет первое появление каждого элемента в указанной коллекции из ObservableCollection.
    /// </summary> 
    /// <param name="collection">Коллекция удаляемых элементов.</param>
    /// <param name="notificationMode">Действие, вызванное событием CollectionChanged.<br/>
    /// Должно быть Remove или Reset (по умолчанию).</param>
    /// <remarks>
    /// С notificationMode = Remove стартовый индекс удаленных элементов не устанавливается.
    /// </remarks>
    public void RemoveRange(IEnumerable<T> collection,
                            NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
    {
        if (notificationMode != NotifyCollectionChangedAction.Remove &&
            notificationMode != NotifyCollectionChangedAction.Reset)
            throw new ArgumentException("Режим должен быть либо Remove, либо Reset для RemoveRange.", "NotificationMode");
        if (collection == null)
            throw new ArgumentNullException("Коллекция не должна быть null.");
        CheckReentrancy();
        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            foreach (var i in collection)
                Items.Remove(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return;
        }
        var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
        for (var i = 0; i < changedItems.Count; i++)
        {
            if (!Items.Remove(changedItems[i]))
            {
                changedItems.RemoveAt(i);
                i--;
            }
        }
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, -1));
    }

    /// <summary> 
    /// Очищает текущую коллекцию и заменяет её указанным элементом.
    /// </summary> 
    /// <param name="item">Элемент, которым заменяются элементы коллекции.</param>
    public void Replace(T item) => ReplaceRange(new T[] { item });

    /// <summary> 
    /// Очищает текущую коллекцию и заменяет её элементами указанной коллекции.
    /// </summary> 
    /// <param name="collection">Коллекция, элементами которой заменяются элементы коллекции.</param>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException("Коллекция не должна быть null.");
        Items.Clear();
        AddRange(collection, NotifyCollectionChangedAction.Reset);
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
