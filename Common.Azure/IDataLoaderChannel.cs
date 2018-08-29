namespace Common.Azure
{
    using System.ServiceModel;

    /// <summary>
    /// The channel for the data loader service
    /// </summary>
    /// <seealso cref="Common.Azure.IDataLoader" />
    /// <seealso cref="System.ServiceModel.IClientChannel" />
    public interface IDataLoaderChannel : IDataLoader, IClientChannel
    {
    }
}
