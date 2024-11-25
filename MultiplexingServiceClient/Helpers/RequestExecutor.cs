using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;

namespace Fic.Dataverse.Helpers
{
    public static class RequestExecutor
    {
        public static void ExecuteExecuteMultiple(ExecuteMultipleRequest request, int numberOfThreads, int batchSize, Func<ServiceClient> getServiceClient)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };

            Parallel.ForEach(
                request.Requests,
                parallelOptions,
                () =>
                {
                    return new
                    {
                        EMR = new ExecuteMultipleRequest()
                        {
                            Requests = new OrganizationRequestCollection(),
                            Settings = request.Settings
                        }
                    };
                },
                (req, loopState, index, threadLocalState) =>
                {
                    threadLocalState.EMR.Requests.Add(req);
                    if (threadLocalState.EMR.Requests.Count == batchSize)
                    {
                        Execute(threadLocalState.EMR, getServiceClient);
                        threadLocalState.EMR.Requests.Clear();
                    }
                    return threadLocalState;
                },
                (threadLocalState) =>
                {
                    if (threadLocalState.EMR.Requests.Count > 0)
                    {
                        Execute(threadLocalState.EMR, getServiceClient);
                    }
                }
            );
        }

        public static void ExecuteUpdateMultiple(UpdateMultipleRequest request, int numberOfThreads, int batchSize, Func<ServiceClient> getServiceClient)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };

            Parallel.ForEach(
                request.Targets.Entities,
                parallelOptions,
                () =>
                {
                    return new
                    {
                        UMR = new UpdateMultipleRequest()
                        {
                            Targets = new EntityCollection()
                            {
                                EntityName = request.Targets.EntityName,
                            }
                        }
                    };
                },
                (entity, loopState, index, threadLocalState) =>
                {
                    threadLocalState.UMR.Targets.Entities.Add(entity);
                    if (threadLocalState.UMR.Targets.Entities.Count == batchSize)
                    {
                        Execute(threadLocalState.UMR, getServiceClient);
                        threadLocalState.UMR.Targets.Entities.Clear();
                    }
                    return threadLocalState;
                },
                (threadLocalState) =>
                {
                    if (threadLocalState.UMR.Targets.Entities.Count > 0)
                    {
                        Execute(threadLocalState.UMR, getServiceClient);
                    }
                }
            );
        }

        public static void ExecuteUpsertMultiple(UpsertMultipleRequest request, int numberOfThreads, int batchSize, Func<ServiceClient> getServiceClient)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };

            Parallel.ForEach(
                request.Targets.Entities,
                parallelOptions,
                () =>
                {
                    return new
                    {
                        UMR = new UpsertMultipleRequest()
                        {
                            Targets = new EntityCollection()
                            {
                                EntityName = request.Targets.EntityName,
                            }
                        }
                    };
                },
                (entity, loopState, index, threadLocalState) =>
                {
                    threadLocalState.UMR.Targets.Entities.Add(entity);
                    if (threadLocalState.UMR.Targets.Entities.Count == batchSize)
                    {
                        Execute(threadLocalState.UMR, getServiceClient);
                        threadLocalState.UMR.Targets.Entities.Clear();
                    }
                    return threadLocalState;
                },
                (threadLocalState) =>
                {
                    if (threadLocalState.UMR.Targets.Entities.Count > 0)
                    {
                        Execute(threadLocalState.UMR, getServiceClient);
                    }
                }
            );
        }

        public static void ExecuteCreateMultiple(CreateMultipleRequest request, int numberOfThreads, int batchSize, Func<ServiceClient> getServiceClient)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };

            Parallel.ForEach(
                request.Targets.Entities,
                parallelOptions,
                () =>
                {
                    return new
                    {
                        CMR = new CreateMultipleRequest()
                        {
                            Targets = new EntityCollection()
                            {
                                EntityName = request.Targets.EntityName,
                            }
                        }
                    };
                },
                (entity, loopState, index, threadLocalState) =>
                {
                    threadLocalState.CMR.Targets.Entities.Add(entity);
                    if (threadLocalState.CMR.Targets.Entities.Count == batchSize)
                    {
                        Execute(threadLocalState.CMR, getServiceClient);
                        threadLocalState.CMR.Targets.Entities.Clear();
                    }
                    return threadLocalState;
                },
                (threadLocalState) =>
                {
                    if (threadLocalState.CMR.Targets.Entities.Count > 0)
                    {
                        Execute(threadLocalState.CMR, getServiceClient);
                    }
                }
            );
        }

        private static void Execute(OrganizationRequest request, Func<ServiceClient> getServiceClient)
        {
            var service = getServiceClient();

            while (true)
            {
                try
                {
                    var response = service.Execute(request);
                    break;
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (IsRetryableError(ex.Detail.ErrorCode))
                    {
                        var retryAfterMilliseconds = GetRetryAfterMilliseconds(ex);
                        Thread.Sleep(retryAfterMilliseconds);
                        continue;
                    }

                    throw;
                }
            }
        }

        private static bool IsRetryableError(int errorCode)
        {
            var retryableErrorCodes = new List<int>
            {
                -2147015902, // Number of requests exceeded the limit of 6000 over time window of 300 seconds.
                -2147015903, // Combined execution time of incoming requests exceeded limit of 1,200,000 milliseconds over time window of 300 seconds.
                -2147015898  // Number of concurrent requests exceeded the limit of 52.
            };

            return retryableErrorCodes.Contains(errorCode);
        }

        private static int GetRetryAfterMilliseconds(FaultException<OrganizationServiceFault> ex)
        {
            var retryAfterMilliseconds = 2000; // Default retry after 2 seconds

            if (ex.Detail.ErrorDetails.TryGetValue("Retry-After", out var retryAfter) && retryAfter is TimeSpan timeSpan)
            {
                retryAfterMilliseconds = Convert.ToInt32(timeSpan.TotalMilliseconds);
            }

            return retryAfterMilliseconds;
        }
    }
}