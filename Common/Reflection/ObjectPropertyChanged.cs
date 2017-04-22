namespace Common.Reflection
{
    /// <summary>
    /// Map a change with objectId, the name of the property and old and new values
    /// </summary>
    [System.Serializable]
    public class ObjectPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPropertyChanged"/> class.
        /// </summary>
        /// <param name="objectId">The object identifier.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="changedValue">The changed value.</param>
        public ObjectPropertyChanged(string objectId, string propertyName, string previousValue, string changedValue)
        {
            ObjectId = objectId;
            PropertyName = propertyName;
            PreviousValue = previousValue;
            ProposedChangedValue = changedValue;
        }

        /// <summary>
        /// Gets or sets the object identifier.
        /// </summary>
        /// <value>
        /// The object identifier.
        /// </value>
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the previous value.
        /// </summary>
        /// <value>
        /// The previous value.
        /// </value>
        public string PreviousValue { get; set; }

        /// <summary>
        /// Gets or sets the proposed changed value.
        /// </summary>
        /// <value>
        /// The proposed changed value.
        /// </value>
        public string ProposedChangedValue { get; set; }
    }
}
