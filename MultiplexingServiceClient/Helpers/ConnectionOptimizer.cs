using System.Net;

namespace Fic.Dataverse.Helpers
{
    public static class ConnectionOptimizer
    {
        public static void OptimizeConnection()
        {
            // Change max connections from .NET to a remote service default: 2
            ServicePointManager.DefaultConnectionLimit = 65000;
            // Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4
            ThreadPool.SetMinThreads(100, 100);
            // Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server
            ServicePointManager.Expect100Continue = false;
            // Can decrease overall transmission overhead but can cause delay in data packet arrival
            ServicePointManager.UseNagleAlgorithm = false;
        }
    }
}
