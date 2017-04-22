using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Common.Strings
{
    /// <summary>
    /// String collection to have the message only once, but with history of occurrence
    /// </summary>
    [Serializable]
    public class StringCollectionWithHistogram
    {
        /// <summary>
        /// Store each message as key to ensure unicity
        /// </summary>
        private readonly IDictionary<string, int> msgHistogram;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringCollectionWithHistogram"/> class.
        /// </summary>
        /// <param name="useSynchronizedDictionary">If set to <c>true</c> use a synchronized dictionary (Threading safe!); otherwise a normal dictionary.</param>
        public StringCollectionWithHistogram(bool useSynchronizedDictionary = false)
        {
            if (useSynchronizedDictionary)
            {
                msgHistogram = new ConcurrentDictionary<string, int>();
            }
            else
            {
                msgHistogram = new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Add the message, if not null or empty, and only if not already added. Increment the counter of already added message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if just added; false otherwise</returns>
        public bool Add(string msg)
        {
            bool isAdded = false;
            if (!string.IsNullOrEmpty(msg))
            {
                msg = msg.Trim();
                if (!msgHistogram.ContainsKey(msg))
                {
                    msgHistogram.Add(msg, 1);
                    isAdded = true;
                }
                else
                {
                    msgHistogram[msg]++;
                }
            }

            return isAdded;
        }

        /// <summary>
        /// Replaces the specified source by the given target. Replace the key in the dictionary, the value of the target will be the one of the source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>True if successfuly replaced</returns>
        public bool Replace(string source, string target)
        {
            bool isReplaced = false;
            source = source.TrimNullable();
            target = target.TrimNullable();
            if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target) && msgHistogram.ContainsKey(source))
            {
                int frequency = msgHistogram[source];
                msgHistogram.Remove(source);
                msgHistogram.Add(target, frequency);
                isReplaced = true;
            }

            return isReplaced;
        }

        /// <summary>
        /// Adds the specified sub map.
        /// </summary>
        /// <param name="subMap">The sub map.</param>
        public void Add(StringCollectionWithHistogram subMap)
        {
            foreach (var msg in subMap.msgHistogram.Keys)
            {
                if (!msgHistogram.ContainsKey(msg))
                {
                    msgHistogram.Add(msg, 1);
                }
                else
                {
                    msgHistogram[msg] += subMap.msgHistogram[msg];
                }
            }
        }

        /// <summary>
        /// Adds the elements of the specified collection
        /// </summary>
        /// <param name="msgs">The collection of message to add once.</param>
        public void AddRange(ICollection<string> msgs)
        {
            foreach (var msg in msgs)
            {
                Add(msg);
            }
        }

        /// <summary>
        /// Clears the histogram.
        /// </summary>
        public void Clear()
        {
            this.msgHistogram.Clear();
        }

        /// <summary>
        /// Gets the messages on single line with separator.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="separator">The separator, ", " per default.</param>
        /// <returns>A string with all messages on a single line </returns>
        public string GetMessagesOnSingleLineWithSeparator(SortOrder sortOrder = SortOrder.Frequency, string separator = ", ")
        {
            return Helper.StringCollectionToString(GetAllMessagesSorted(sortOrder), separator);
        }

        /// <summary>
        /// SortOrder
        /// </summary>
        public enum SortOrder
        {
            /// <summary>
            /// Per name (default Sort)
            /// </summary>
            Name,

            /// <summary>
            /// Per frequency in the histogram
            /// </summary>
            Frequency,

            /// <summary>
            /// No sorting
            /// </summary>
            None
        }

        /// <summary>
        /// Sorts the histogram of the messages.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns>The list of the message</returns>
        public IList<string> GetAllMessagesSorted(SortOrder sortOrder = SortOrder.Frequency)
        {
            var list = new List<string>(msgHistogram.Keys);
            switch (sortOrder)
            {
                case SortOrder.Frequency:
                    list.Sort(Comparer);
                    break;
                case SortOrder.Name:
                    list.Sort();
                    break;
                case SortOrder.None:
                    break;
                default:
                    break;
            }

            return list;
        }

        /// <summary>
        /// The comparer computed once
        /// </summary>
        private Comparison<string> comparer;

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <value>The comparer.</value>
        private Comparison<string> Comparer
        {
            get { return comparer ?? (comparer = new Comparison<string>(CompareHistogramString)); }
        }

        /// <summary>
        /// Compares the string by their frequency in the dictionary.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>1 if x > y; 0 if equal, -1 otherwise</returns>
        private int CompareHistogramString(string x, string y)
        {
            int result = msgHistogram[x].CompareTo(msgHistogram[y]);
            if (result == 0)
            {
                result = x.CompareTo(y);
            }

            return result;
        }

        /// <summary>
        /// Gets the number of messages.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return msgHistogram.Keys.Count; }
        }
    }
}
