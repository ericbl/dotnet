namespace Common.Azure
{
    using Logging;
    using Microsoft.ServiceBus;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    /// <summary>
    /// Utilities to use the service bus
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Refreshes the API data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public static void RefreshApiData(ILogger logger)
        {
            RunActionOnWebBindingChannel(ch => ch.ReloadData(), AzureSettings.Default.ApiName, logger);
        }

        /// <summary>
        /// Creates the web binding channel.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="logger">The logger.</param>
        public static void RunActionOnWebBindingChannel(Action<IDataLoaderChannel> action, string serviceName, ILogger logger)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var binding = new WebHttpRelayBinding(EndToEndWebHttpSecurityMode.Transport, RelayClientAuthenticationType.None);
            var uri = ServiceBusEnvironment.CreateServiceUri("https", AzureSettings.Default.BusNamespace, serviceName);
            using (ChannelFactory<IDataLoaderChannel> cf = new WebChannelFactory<IDataLoaderChannel>(binding, uri))
            {
                cf.Endpoint.Behaviors.Add(GetTransportClientEndpointBehavior());
                IDataLoaderChannel channel = cf.CreateChannel();
                // Run the action
                try
                {
                    action(channel);
                }
                catch (CommunicationException)
                {
                    logger.WriteError("Communication error on service {0}, please ensure that the service is properly running on the URI {1}!", serviceName, uri);
                }
                catch (Exception e)
                {
                    logger.WriteError(e, "Exception on service {0} in the URI {1}!", serviceName, uri);
                }

                // close and dispose the channel;
                channel.Close();
                channel.Dispose();
                if (cf != null)
                    cf.Close();
            }
        }

        /// <summary>
        /// Gets the transport client endpoint behavior.
        /// </summary>
        /// <returns>The transport client endpoint behavior.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "design")]
        public static TransportClientEndpointBehavior GetTransportClientEndpointBehavior()
        {
            return new TransportClientEndpointBehavior
            {
                TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", AzureSettings.Default.BusKey)
            };
        }
    }
}
