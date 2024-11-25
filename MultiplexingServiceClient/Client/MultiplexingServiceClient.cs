using Fic.Dataverse.Helpers;
using Fic.Dataverse.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fic.Dataverse.Client
{
    public class MultiplexingServiceClient
    {
        private readonly object _lock = new();
        private List<Connection> Connections { get; set; } = new List<Connection>();
        private List<string> ConnectionStrings { get; set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the MultiplexingServiceClient class with the provided connection strings.
        /// Optimizes the connections after validating the connection strings.
        /// </summary>
        /// <param name="connectionStrings">
        ///  Connections strings for Dataverse environment
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws an ArgumentException if no connection strings are provided.
        /// </exception>
        public MultiplexingServiceClient(List<string> connectionStrings)
        {
            if (connectionStrings.Count == 0)
            {
                throw new ArgumentException("No connection strings provided.");
            }

            ConnectionStrings = connectionStrings;

            CheckConnectionStrings();

            ConnectionOptimizer.OptimizeConnection();
        }

        #region Public Methods

        /// <summary>
        /// Executes the given OrganizationRequest using multiple threads and batches. Supports CreateMultipleRequest, UpdateMultipleRequest, and ExecuteMultipleRequest.
        /// </summary>
        /// <param name="request">The OrganizationRequest to be executed.</param>
        /// <param name="numberOfThreads">The number of threads to use for execution. Default is 20.</param>
        /// <param name="batchSize">The size of each batch for execution. Default is 10.</param>
        /// <exception cref="ArgumentException">Thrown when the request type is not supported.</exception>
        public void Execute(OrganizationRequest request, int numberOfThreads = 20, int batchSize = 10)
        {
            if (numberOfThreads > 100)
            {
                throw new ArgumentException("Number of threads cannot be greater than 100.");
            }

            if (numberOfThreads < 1)
            {
                throw new ArgumentException("Number of threads cannot be less than 1.");
            }

            if (batchSize < 1)
            {
                throw new ArgumentException("Batch size cannot be less than 1.");
            }

            Initialize(numberOfThreads);

            switch (request)
            {
                case CreateMultipleRequest createMultipleRequest:
                    RequestExecutor.ExecuteCreateMultiple(createMultipleRequest, numberOfThreads, batchSize, GetServiceClient);
                    break;
                case UpdateMultipleRequest updateMultipleRequest:
                    RequestExecutor.ExecuteUpdateMultiple(updateMultipleRequest, numberOfThreads, batchSize, GetServiceClient);
                    break;
                case ExecuteMultipleRequest executeMultipleRequest:
                    RequestExecutor.ExecuteExecuteMultiple(executeMultipleRequest, numberOfThreads, batchSize, GetServiceClient);
                    break;
                case UpsertMultipleRequest upsertMultipleRequest:
                    RequestExecutor.ExecuteUpsertMultiple(upsertMultipleRequest, numberOfThreads, batchSize, GetServiceClient);
                    break;
                default:
                    throw new ArgumentException("Request type not supported.");
            }

            DisposeConnections();
        }
        #endregion

        #region Private Methods

        private ServiceClient GetServiceClient()
        {
            lock (_lock)
            {
                var connection = Connections.OrderBy(e => e.Counter).First();
                return connection.GetServiceClient();
            }
        }

        private void Initialize(int numberOfThreads)
        {
            var baseConnections = new List<Connection>();

            foreach (var connectionString in ConnectionStrings)
            {
                var connection = new Connection(connectionString);
                baseConnections.Add(connection);
                Connections.Add(connection);
            }

            for (int i = 0; i < numberOfThreads - baseConnections.Count; i++)
            {
                var baseConnectionIndex = i % baseConnections.Count;
                var connection = baseConnections[baseConnectionIndex];
                var clonedConnection = connection.Clone();
                Connections.Add(clonedConnection);
            }
        }

        private void DisposeConnections()
        {
            foreach (var connection in Connections)
            {
                connection.ServiceClient.Dispose();
            }
        }

        private void CheckConnectionStrings()
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ConnectionStrings.Count };

            Parallel.ForEach(ConnectionStrings, parallelOptions, connectionString =>
            {
                var service = new ServiceClient(connectionString);
                service.Execute(new WhoAmIRequest());
            });
        }

        #endregion    
    }
}