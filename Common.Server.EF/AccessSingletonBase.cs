namespace Common.Server.EF
{
    /// <summary>
    /// AccessBase with single instance
    /// </summary>
    /// <typeparam name="T">Type of the class</typeparam>
    public abstract class AccessSingletonBase<T> : AccessBase where T : AccessSingletonBase<T>, new()
    {
        /// <summary>
        /// The single instance of this class
        /// </summary>
        private static T instance;

        /// <summary>
        /// Gets the single instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get { return instance ?? (instance = new T()); }
        }
    }
}
