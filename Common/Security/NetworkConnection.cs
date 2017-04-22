using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace Common.Security
{
    /// <summary>
    /// Manage a network connection.
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    public class NetworkConnection : IDisposable
    {
        string _networkName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConnection"/> class.
        /// </summary>
        /// <exception cref="Win32Exception">Thrown when a Window 32 error condition occurs.</exception>
        /// <param name="networkName">Name of the network.</param>
        /// <param name="credentials">The credentials.</param>
        public NetworkConnection(string networkName, NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(netResource, credentials.Password, userName, 0);

            if (result != 0)
            {
                throw new Win32Exception(result, "Error connecting to remote share. Check your credentials!");
            }
        }

        /// <summary>
        /// Finalizes an instance of the Common.Security.NetworkConnection class.
        /// </summary>
        ~NetworkConnection()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);
    }

    /// <summary>
    /// A net resource.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        /// <summary>
        /// The scope.
        /// </summary>
        public ResourceScope Scope;

        /// <summary>
        /// Type of the resource.
        /// </summary>
        public ResourceType ResourceType;

        /// <summary>
        /// Type of the display.
        /// </summary>
        public ResourceDisplaytype DisplayType;

        /// <summary>
        /// The usage.
        /// </summary>
        public int Usage;

        /// <summary>
        /// Name of the local.
        /// </summary>
        public string LocalName;

        /// <summary>
        /// Name of the remote.
        /// </summary>
        public string RemoteName;

        /// <summary>
        /// The comment.
        /// </summary>
        public string Comment;

        /// <summary>
        /// The provider.
        /// </summary>
        public string Provider;
    }

    /// <summary>
    /// Values that represent resource scopes.
    /// </summary>
    public enum ResourceScope : int
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Connected
        /// </summary>
        Connected = 1,
        /// <summary>
        /// An enum constant representing the global network option.
        /// </summary>
        GlobalNetwork,
        /// <summary>
        /// Remembered
        /// </summary>
        Remembered,
        /// <summary>
        /// Recent
        /// </summary>
        Recent,
        /// <summary>
        /// A context
        /// </summary>
        Context
    };

    /// <summary>
    /// Values that represent resource types.
    /// </summary>
    public enum ResourceType : int
    {
        /// <summary>
        /// Any
        /// </summary>
        Any = 0,

        /// <summary>
        /// A disk
        /// </summary>
        Disk = 1,

        /// <summary>
        /// A print
        /// </summary>
        Print = 2,

        /// <summary>
        /// Reserved
        /// </summary>
        Reserved = 8,
    }

    /// <summary>
    /// Values that represent display types of a resource.
    /// </summary>
    public enum ResourceDisplaytype : int
    {
        /// <summary>
        /// Generic
        /// </summary>
        Generic = 0x0,

        /// <summary>
        /// A domain
        /// </summary>
        Domain = 0x01,

        /// <summary>
        /// A server
        /// </summary>
        Server = 0x02,

        /// <summary>
        /// A share
        /// </summary>
        Share = 0x03,

        /// <summary>
        /// A file
        /// </summary>
        File = 0x04,

        /// <summary>
        /// A group
        /// </summary>
        Group = 0x05,

        /// <summary>
        /// A network
        /// </summary>
        Network = 0x06,

        /// <summary>
        /// A Root
        /// </summary>
        Root = 0x07,

        /// <summary>
        /// A Share admin
        /// </summary>
        Shareadmin = 0x08,

        /// <summary>
        /// A Directory
        /// </summary>
        Directory = 0x09,

        /// <summary>
        /// A Tree
        /// </summary>
        Tree = 0x0a,

        /// <summary>
        /// A Nds container
        /// </summary>
        Ndscontainer = 0x0b
    }
}
