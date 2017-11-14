using System;
using System.Configuration;
using System.Net;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint
{
    public static class ClientContextExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ClientContextExtensions));

        public static void ExecuteQueryWithIncrementalRetry(this ClientRuntimeContext clientContext, int retryCount = 3, int delay = 100)
        {
            if (retryCount <= 0) throw new ArgumentException("Provide a retry count greater than zero.");
            if (delay <= 0) throw new ArgumentException("Provide a delay greater than zero.");

            var retryAttempts = 0;
            while (retryAttempts < retryCount)
            {
                try
                {
                    clientContext.ExecuteQuery();
                    return;
                }
                catch (WebException wex)
                {
                    var response = wex.Response as HttpWebResponse;
                    if (response != null && IsResponseThrottled(response))
                    {
                        Logger.Warn(
                            $"CSOM request frequency exceeded usage limits. Sleeping for {delay} seconds before retrying.");
                        System.Threading.Thread.Sleep(delay);
                        retryAttempts++;
                        delay *= 2;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    if (bool.Parse(ConfigurationManager.AppSettings["ThrowOnExceptions"]))
                    {
                        throw;
                    }
                }
            }

            throw new MaximumRetryAttemptedException($"Maximum retry attempts {retryCount}, has be attempted.");
        }

        private static bool IsResponseThrottled(HttpWebResponse response) =>
            response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503;
    }

    public class MaximumRetryAttemptedException : Exception
    {
        public MaximumRetryAttemptedException(string message) : base(message) { }
    }
}
