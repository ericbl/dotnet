using System;
using System.Runtime.Serialization;

namespace Common.Exceptions
{
    /// <summary>
    /// Spezialisierter Exception Typ, der vom Service geworfen wird 
    /// um Userinfos und -Exceptions an den Client zu melden,
    /// </summary>
    [Serializable]
    public class UserException : Exception
    {
        /// <summary>
        /// Art der Exception
        /// </summary>
        private readonly ExceptionType exceptionType = ExceptionType.Info;

        /// <summary>
        /// Art der Exception
        /// </summary>
        /// <value>
        /// The type of the exception.
        /// </value>
        public ExceptionType ExceptionType
        {
            get
            {
                return this.exceptionType;
            }
        }

        /// <summary>
        /// Initialisiert das Objekt
        /// </summary>
        /// <param name="message">Meldung, die für den Benutzer verständlich formuliert ist</param>
        /// <param name="exceptionType">Art der Exception - Info, Error, Fatal, ... (Verwendung nur innerhalb des Client, geht bei Übertragung Host->Client verloren)</param>
        /// <param name="innerException">Exception die das Auslösen dieser Exception ausgelöst hat</param>
        public UserException(string message, ExceptionType exceptionType, Exception innerException)
            : base(message, innerException)
        {
            this.exceptionType = exceptionType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exceptionType">The exception typ.</param>
        public UserException(string message, ExceptionType exceptionType)
            : base(message)
        {
            this.exceptionType = exceptionType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UserException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public UserException(string message) : base(message) { }

        /// <summary>
        /// Initialisiert das Objekt
        /// </summary>
        /// <param name="format">Der Format-String mit den entsprechenden {n} zum Einfügen der Objekte</param>
        /// <param name="args">Ein Object-Array, das keinen oder mehrere Objekte zum Formatieren enthält</param>
        public UserException(string format, params object[] args)
            : base(String.Format(format, args))
        {
        }

        /// <summary>
        /// Initialisiert das Objekt
        /// </summary>
        /// <param name="innerException">Exception die das Auslösen dieser Exception ausgelöst hat</param>
        /// <param name="format">Der Format-Strin mit den entsprechenden {n} zum Einfügen der Objekte</param>
        /// <param name="args">Ein Object-Array, das keinen oder mehrere Objekte zum Formatieren enthält</param>
        public UserException(Exception innerException, string format, params object[] args)
            : this(String.Format(format, args), innerException)
        {
        }

        /// <summary>
        /// Initialisiert das Objekt
        /// </summary>
        /// <param name="exceptionType">Art der Exception - Info, Critical Error, ...</param>
        /// <param name="format">Der Format-Strin mit den entsprechenden {n} zum Einfügen der Objekte</param>
        /// <param name="args">Ein Object-Array, das keinen oder mehrere Objekte zum Formatieren enthält</param>
        public UserException(ExceptionType exceptionType, string format, params object[] args)
            : this(String.Format(format, args), exceptionType)
        {
        }

        /// <summary>
        /// Initialisiert das Objekt
        /// </summary>
        /// <param name="exceptionType">Art der Exception - Info, Error, Fatal, ...</param>
        /// <param name="innerException">Exception die das Auslösen dieser Exception ausgelöst hat</param>
        /// <param name="format">Der Format-String mit den entsprechenden {n} zum Einfügen der Objekte</param>
        /// <param name="args">Ein Object-Array, das keinen oder mehrere Objekte zum Formatieren enthält</param>
        public UserException(ExceptionType exceptionType, Exception innerException, string format, params object[] args)
            : this(String.Format(format, args), exceptionType, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected UserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="UserException"/> class from being created.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        private UserException()
            : base()
        { }
    }

    /// <summary>
    /// Art der Exception
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// Noch nicht definiert
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Information
        /// </summary>
        Info = 1,

        /// <summary>
        /// Error
        /// </summary>
        Error = 2,

        /// <summary>
        /// Fatal error
        /// </summary>
        Fatal = 3
    }
}
