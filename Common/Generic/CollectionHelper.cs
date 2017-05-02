using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Common.Generic
{
    /// <summary>
    /// Utilities for collections.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Adds the item to dictionary list once.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="checkListContains">if set to <c>true</c> check if list contains item before add.</param>
        public static void AddItemToDictionaryListOnce<TKey, TValue>(Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue item, bool checkListContains = false)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new List<TValue>());
            }
            if (!checkListContains || (checkListContains && !dictionary[key].Contains(item)))
            {
                dictionary[key].Add(item);
            }
        }

        /// <summary>
        /// Sort the enumerable converted as list using the default comparer
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>The sorted list from the collection</returns>
        public static IReadOnlyList<T> ToListAndSort<T>(this IEnumerable<T> collection)
        {
            var list = collection as List<T>;
            if (list == null)
                list = new List<T>(collection);
            list.Sort();
            return list;
        }

        /// <summary>
        /// Enumerates a Linked List while condition lasts (even over lastnode)
        /// </summary>
        /// <typeparam name="T">The Type of the Linked List</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        /// Elements of a linked list
        /// </returns>
        public static IEnumerable<LinkedListNode<T>> EnumerateNodes<T>(this LinkedList<T> list, Delegate condition, int index = 0)
        {
            var node = list.First;

            while ((bool)condition.DynamicInvoke((new object[] { node })))
            {
                yield return node;
                node = node != null ? node.Next : list.First;
            }
        }

        /// <summary>
        /// An IEnumerable&lt;T&gt; extension method that applies an operation to all items in this collection.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="list">  The list.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list != null)
            {
                foreach (T item in list)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Get a read only list of the source. First convert to List and then to a ReadOnlyCollection.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The ReadOnlyCollection from the source</returns>
        public static ReadOnlyCollection<TSource> ToReadOnlyList<TSource>(this IEnumerable<TSource> source)
        {
            return new ReadOnlyCollection<TSource>(source.ToList());
        }

        /// <summary>
        /// Creates a list with the single item
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>A list of T</returns>
        public static IReadOnlyList<T> CreateList<T>(this T item)
        {
            return new List<T> { item };
        }

        /// <summary>
        /// Creates a list from the array, creating an empty list if array is null.
        /// </summary>
        /// <typeparam name="T">Type of the item in the collection</typeparam>
        /// <param name="array">The array.</param>
        /// <returns>A list of T</returns>
        public static IReadOnlyList<T> CreateList<T>(this T[] array)
        {
            if (array == null)
                array = new T[] { };
            return new List<T>(array);
        }

        /// <summary>
        /// Adds the item of the list to the collection but ensure they are only once.
        /// </summary>
        /// <typeparam name="T">Type of the item in the collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="list">The list.</param>
        public static void AddRangeUnique<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            if (collection != null && list != null)
            {
                foreach (T item in list)
                {
                    collection.AddUnique(item);
                }
            }
        }

        /// <summary>
        /// Adds the item to the collection only if not already present.
        /// </summary>
        /// <typeparam name="T">Type of the item in the collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="item">The item.</param>
        public static void AddUnique<T>(this ICollection<T> collection, T item)
        {
            if (collection != null && item != null)
            {
                if (!collection.Contains(item))
                    collection.Add(item);
            }
        }

        /// <summary>
        /// For each item in the list, starting with the last one, apply the delete action and remove the item from the variable-size list.
        /// If the list is fixed, only apply the delete action on each item.
        /// </summary>
        /// <typeparam name="T">Type of the item in the collection</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="delete">The delete action.</param>
        public static void DeleteItemsInList<T>(this ICollection<T> list, Action<T> delete)
        {
            if (list is IList && !((IList)list).IsFixedSize)
            {
                while (list.Count > 0)
                {
                    T last = list.Last();
                    list.Remove(last);
                    delete?.Invoke(last);
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    delete?.Invoke(list.ElementAt(i));
                }
            }
        }

        /// <summary>
        /// An IList&lt;T&gt; extension method that dispose item in list.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="list">The list.</param>
        public static void DisposeItemsInList<T>(this IList<T> list) where T : IDisposable
        {
            DeleteItemsInList(list, item => item.Dispose());
        }
    }
}
