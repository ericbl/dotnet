using System;
using System.Collections.Generic;

namespace Common.Serialization
{
    /// <summary>
    /// Collect string in a list
    /// </summary>
    [Serializable()]
    public class CollectErrorMessages
    {
        /// <summary>
        /// Gets the error messages.
        /// </summary>
        /// <value>
        /// The error messages.
        /// </value>
        public IList<string> ErrorMessages { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectErrorMessages"/> class.
        /// </summary>
        public CollectErrorMessages()
        {
            ErrorMessages = new List<string>();
        }

        /// <summary>
        /// Adds the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void AddErrorMessage(string errorMessage)
        {
            ErrorMessages.Add(errorMessage);
        }

        /// <summary>
        /// Gets all messages.
        /// </summary>
        /// <value>
        /// All messages.
        /// </value>
        public string AllMessages
        {
            get
            {
                var result = "\n";
                foreach (var m in ErrorMessages)
                    result += m;
                result += "\n";
                return result;
            }
        }
    }
}
