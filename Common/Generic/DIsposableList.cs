using System;
using System.Collections.Generic;

namespace Common.Generic
{
    /// <summary>
    /// Extends the <seealso cref="List{T}"/> to implement <seealso cref="IDisposable"/>and call Dispose for each items in the list.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>    
    public class DisposableList<T> : List<T>, IDisposable
        where T : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases the unmanaged resources used by the Common.Generic.DIsposableList&lt;T&gt; and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.DisposeItemsInList();
                    Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DIsposableList() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
