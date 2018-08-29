namespace Common.Azure
{
    using System.ServiceModel;
    using System.ServiceModel.Web;

    /// <summary>
    /// Interface to define the WCF service to load the data
    /// </summary>
    [ServiceContract]
    public interface IDataLoader
    {
        /// <summary>
        /// Reloads the data from SCSM.
        /// </summary>
        /// <returns><c>True</c> if successfully reloaded</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/Reload", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool ReloadData();
    }
}
