using Common.Exceptions;
using Common.Logging;
using Common.Server.Logging;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Transactions;

namespace Common.Server.EF
{
    /// <summary>
    /// Base class to manage connection string
    /// </summary>
    public abstract class AccessBase
    {
        #region fields
        private ILogger logger;

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                if (logger == null)
                    logger = LoggerFactory.Instance.Create(typeof(AccessBase));
                return logger;
            }
        }

        #endregion

        #region properties

        /// <summary>
        /// Timeout to be used when accessing the context
        /// This timeout is to be used whenever a specific timeout has to be set
        /// For instance: TransactionScope class
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; protected set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessBase"/> class.
        /// </summary>
        public AccessBase()
        {
            FixEfProviderServicesProblem();
        }

        #region CheckConnection/Get connection methods
        /// <summary>
        /// Check if a connection to the database can be established.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns>
        ///   <c>True</c> when connection successful
        /// </returns>
        public bool CheckConnection(ILogger logger)
        {
            // set the logger
            this.logger = logger;
            // check connection within a transaction
            bool databaseConnection = false;
            try
            {
                // Logger.WriteInfo($"Check database connection: {GetDbConnectionString()}");  // comment that since already post logging
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (GetConnection(true))
                    {
                        databaseConnection = true;
                    }

                    scope.Complete();
                }

                Logger.WriteInfo("Database connection successful: {0}", GetDbConnectionString());
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex, "Exception while checking the database connection {0}{1}{0}.", Environment.NewLine, GetDbConnectionString());
            }

            return databaseConnection;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>Opened connection</returns>
        public DbConnection GetConnection()
        {
            return GetConnection(true);
        }


        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="open">if set to <c>true</c> open the connection.</param>
        /// <returns></returns>
        /// <exception cref="UserException">
        /// No connection to the database possible.
        /// </exception>
        private DbConnection GetConnection(bool open)
        {
            try
            {
                DbConnection connection;
                string provider = GetProviderName();
                switch (provider)
                {
                    case "System.Data.SqlClient":
                        connection = new SqlConnection(GetDbConnectionString());
                        break;
                    default:
                        throw new UserException(provider + " unknown!");
                }

                if (open)
                {
                    connection.Open();
                }

                return connection;
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex);
                throw new UserException("No connection to the database possible.", ExceptionType.Fatal, ex);
            }
        }
        #endregion

        #region DbModelBuilder - to use with CodeFirst Model

        protected DbCompiledModel dbCompiledModel;

        /// <summary>
        /// Gets the compiled database model.
        /// </summary>
        /// <returns></returns>
        protected DbCompiledModel GetCompiledDbModel()
        {
            if (dbCompiledModel == null)
            {
                var builder = new DbModelBuilder();
                var model = builder.Build(GetConnection());
                dbCompiledModel = model.Compile();
            }

            return dbCompiledModel;
        }
        #endregion

        #region EntityConnection - to use with EDMX DataModels
        /// <summary>
        /// Gets the entity connection.
        /// </summary>
        /// <returns></returns>
        protected EntityConnection GetEntityConnection()
        {
            // Initialize the EntityConnectionStringBuilder and set the provider name, the provider-specific connection string and the Metadata location.
            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = GetProviderName(),
                ProviderConnectionString = GetDbConnectionString(),
                Metadata = GetEntitiesMetadata()
            };
            var connection = new EntityConnection(entityBuilder.ToString());
            Timeout = new TimeSpan(0, 0, 0, connection.StoreConnection.ConnectionTimeout);
            return connection;
        }

        /// <summary>
        /// Gets the entity connection string.
        /// </summary>
        /// <returns></returns>
        protected string GetEntityConnectionString()
        {
            // Initialize the EntityConnectionStringBuilder and set the provider name, the provider-specific connection string and the Metadata location.
            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = GetProviderName(),
                ProviderConnectionString = GetDbConnectionString(),
                Metadata = GetEntitiesMetadata()
            };
            return entityBuilder.ToString();
        }

        #endregion

        #region abstract methods declaration

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        /// <returns>The database connection string</returns>
        protected abstract string GetDbConnectionString();

        /// <summary>
        /// Gets the entities metadata.
        /// </summary>
        /// <returns>The entities metadata</returns>
        protected abstract string GetEntitiesMetadata();

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        /// <returns>The name of the provider</returns>
        protected abstract string GetProviderName();
        #endregion


        /// <summary>
        /// Adjusts the timeout for stored procedures.
        /// </summary>
        /// <param name="context">The context.</param>
        protected void AdjustTimeout(ObjectContext context)
        {
            context.CommandTimeout = context.Connection.ConnectionTimeout;
            Timeout = new TimeSpan(0, 0, 0, context.Connection.ConnectionTimeout);
        }

        /// <summary>
        /// Fix the EF provider services problem by forcing the references.
        /// </summary>
        public static void FixEfProviderServicesProblem()
        {
            // The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            // for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            // Make sure the provider assembly is available to the running application. 
            // See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.
            Helper.Utils.EnsureStaticReference<System.Data.Entity.SqlServer.SqlProviderServices>();
        }
    }
}
