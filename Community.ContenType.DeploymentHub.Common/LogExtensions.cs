using log4net;
using log4net.Core;
using Newtonsoft.Json;
using System;

namespace Community.ContenType.DeploymentHub.Common
{
    public static class LogExtensions
    {
        public static MethodTraceLogger MethodTraceLogger(this ILog logger, params object[] args)
        {
            return new MethodTraceLogger(logger.Logger.Name, args);
        }

        public static void DebugReturns(this ILog logger, object returnedObject)
        {
            logger.DebugObject("Returned Object", returnedObject);
        }

        public static void DebugObject(this ILog logger, string message, object returnedObject)
        {
            if (!logger.IsDebugEnabled) return;
            try
            {
                var objectString = returnedObject != null ? JsonConvert.SerializeObject(returnedObject, Formatting.None) : "null";
                logger.Debug($"{message}: {objectString}");
            }
            catch (Exception)
            {
                logger.Debug($"{message}: {returnedObject ?? string.Empty}");
            }
        }

        public static Level ToLevel(this string logLevelValue)
        {
            switch (logLevelValue.ToUpperInvariant())
            {
                case "DEBUG":
                    return Level.Debug;
                case "WARN":
                    return Level.Warn;
                case "ERROR":
                    return Level.Error;
                case "FATAL":
                    return Level.Fatal;
                default:
                    return Level.Info;
            }
        }
    }
}
