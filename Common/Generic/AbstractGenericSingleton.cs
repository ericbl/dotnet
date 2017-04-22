namespace Common.Generic
{
    /// <summary>
    /// Singleton pattern in an abstract generic way
    /// </summary>
    /// <typeparam name="T">Type of the class to instantiate, i.e. of the singleton instance</typeparam>
    /// <remarks>The new constraint specifies that any type argument in a generic class declaration must have a public parameterless constructor. To use the new constraint, the type cannot be abstract.</remarks>
    public abstract class AbstractGenericSingleton<T> 
        where T : AbstractGenericSingleton<T>, new()
    {
        /// <summary>
        /// The single instance of this class
        /// </summary>
        private volatile static T instance;
        /// <summary>
        /// Lock synchronization object
        /// </summary>
        private static object syncLock = new object();

        /// <summary>
        /// Gets the single instance.
        /// </summary>
        /// <value>The instance.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static T Instance
        {
            get { return instance ?? (instance = CreateInstance()); }
        }

        /// <summary>
        /// Creates the instance of T
        /// </summary>
        /// <returns>The created instance</returns>
        private static T CreateInstance()
        {
            lock (syncLock)
            {
                //Type typeT = typeof(T);
                //ConstructorInfo ci = typeT.GetConstructor(
                //    BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                //object o = ci.Invoke(null);
                //return o as T;
                return new T();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has been created.
        /// </summary>
        /// <value><c>true</c> if this instance is created; otherwise, <c>false</c>.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool IsInstanceCreated
        {
            get { return instance != null; }
        }
    }
}
