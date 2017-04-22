using System.Collections.Generic;

namespace Common.Generic
{
    /// <summary>
    /// Bi dictionary to retrieve key from value and value from key
    /// </summary>
    /// <typeparam name="TFirst">first key</typeparam>
    /// <typeparam name="TSecond">second key</typeparam>
    public class BiDictionary<TFirst, TSecond>
    {
        private IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        private IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        /// <summary>
        /// Determines whether the first dictionary contain the specified key.
        /// </summary>
        /// <param name="first">The first.</param>        
        /// <returns>
        /// 	<c>true</c> if data are already there; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TFirst first)
        {
            return firstToSecond.ContainsKey(first);
        }

        /// <summary>
        /// Determines whether the second dictionary contain the specified key.
        /// </summary>
        /// <param name="second">The second.</param>        
        /// <returns>
        /// 	<c>true</c> if data are already there; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TSecond second)
        {
            return secondToFirst.ContainsKey(second);
        }

        /// <summary>
        /// Adds the specified tuple.
        /// </summary>
        /// <param name="first">The first key.</param>
        /// <param name="second">The second key.</param>
        public void Add(TFirst first, TSecond second)
        {
            if (!firstToSecond.ContainsKey(first) && !secondToFirst.ContainsKey(second))
            {
                firstToSecond.Add(first, second);
                secondToFirst.Add(second, first);
            }
        }

        // Note potential ambiguity using indexers (e.g. mapping from int to int)
        // Hence the methods as well...
        /// <summary>
        /// Gets the <see cref="System.Collections.Generic.IList&lt;TSecond&gt;"/> with the specified first.
        /// </summary>
        /// <value></value>
        public TSecond this[TFirst first]
        {
            get { return GetByFirst(first); }
        }

        /// <summary>
        /// Gets the <see cref="System.Collections.Generic.IList&lt;TFirst&gt;"/> with the specified second.
        /// </summary>
        /// <value></value>
        public TFirst this[TSecond second]
        {
            get { return GetBySecond(second); }
        }

        /// <summary>
        /// Gets the by first.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <returns>The second</returns>
        public TSecond GetByFirst(TFirst first)
        {
            return this.firstToSecond[first];
        }

        /// <summary>
        /// Gets the by second.
        /// </summary>
        /// <param name="second">The second.</param>
        /// <returns>the first</returns>
        public TFirst GetBySecond(TSecond second)
        {
            return this.secondToFirst[second];
        }

        /// <summary>
        /// Gets all firsts.
        /// </summary>
        /// <value>The firsts.</value>
        public ICollection<TFirst> Firsts
        {
            get { return this.firstToSecond.Keys; }
        }

        /// <summary>
        /// Gets all seconds.
        /// </summary>
        /// <value>The seconds.</value>
        public ICollection<TSecond> Seconds
        {
            get { return this.secondToFirst.Keys; }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        /// <exception cref="System.ArgumentOutOfRangeException">BiDictionary inner dictionaries don't have the same amount of objects: {firstToSecond.Count} != {secondToFirst.Count}!</exception>
        public int Count
        {
            get
            {
                if (firstToSecond.Count != secondToFirst.Count)
                    throw new System.ArgumentOutOfRangeException($"BiDictionary inner dictionaries don't have the same amount of objects: {firstToSecond.Count} != {secondToFirst.Count}!");
                return firstToSecond.Count;
            }
        }
    }
}
