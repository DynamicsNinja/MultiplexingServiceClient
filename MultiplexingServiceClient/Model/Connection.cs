using Microsoft.PowerPlatform.Dataverse.Client;

namespace Fic.Dataverse.Model
{
    public class Connection
    {
        public int Counter { get; set; } = 0;

        public ServiceClient ServiceClient;
        public Connection(string connectionString)
        {
            ServiceClient = new ServiceClient(connectionString);
        }
        public Connection(ServiceClient serviceClient)
        {
            ServiceClient = serviceClient;
        }

        public Connection Clone()
        {
            var clonedService = ServiceClient.Clone();
            return new Connection(clonedService);
        }

        public ServiceClient GetServiceClient()
        {
            Counter++;

            ServiceClient.EnableAffinityCookie = false;
            return ServiceClient;
        }
    }
}
