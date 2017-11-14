using System;
using System.Diagnostics;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Configuration;
using log4net;
using Newtonsoft.Json;

namespace Community.ContenType.DeploymentHub.Common
{
    public class MethodTraceLogger : IDisposable
    {
        public static readonly ILog InternalLogger = LogManager.GetLogger(typeof(MethodTraceLogger));


        private readonly ILog _logger;
        private readonly object[] _args;
        private readonly string _methodSignature = "";
        private readonly Stopwatch _stopwatch;

        internal MethodTraceLogger(string name, params object[] args)
        {
            _logger = LogManager.GetLogger(name);

            if (!_logger.IsDebugEnabled) return;
            if (LogConfigAppSettings.EnableStopWatch)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }
            _args = args;
            _methodSignature = GetMethodSignature();
            _logger.Debug($"===> START: {_methodSignature}");
            WriteParameters();
        }

        public void Dispose()
        {
            if (!_logger.IsDebugEnabled) return;

            if (!LogConfigAppSettings.EnableStopWatch)
            {
                _logger.Debug($"===> END  : {_methodSignature}");
            }
            else
            {
                _stopwatch.Stop();
                _logger.Debug($"===> END  : {_methodSignature};Elapsed Time;{_stopwatch.Elapsed}");

            }
        }

        private void WriteParameters()
        {
            if (_args == null || _args.Length == 0) return;
            try
            {
                var parameters = JsonConvert.SerializeObject(_args, Formatting.None);
                _logger.Debug($" with parameters: {parameters}");
            }
            catch (Exception)
            {
                var parameters = string.Join(",", _args.Select(a => a?.ToString() ?? "null"));
                _logger.Debug($" with parameters: {parameters}");
            }
        }

        private static string GetMethodSignature()
        {
            var methodSignature = "UnknowMethod()";
            try
            {
                var stackTrace = new StackTrace();
                var methodBase = stackTrace.GetFrame(3).GetMethod();
                var methodName = methodBase.Name;
                var parameterList = string.Join(",", methodBase.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"));

                methodSignature = $"{methodName}({parameterList})";
            }
            catch (Exception e)
            {
                InternalLogger.Warn(e);
            }
            return methodSignature;
        }
    }
}
