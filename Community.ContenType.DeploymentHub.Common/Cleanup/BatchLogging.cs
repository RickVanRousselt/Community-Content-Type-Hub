using System;
using System.IO;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Configuration;

namespace Community.ContenType.DeploymentHub.Common.Cleanup
{
    public class BatchLogging
    {
        /// <summary>
        /// Cleanup all log files that have expired.
        /// In App.Config set the AppSetting "log4net.CleanupLogFilesAfterDays" to set the number of days to keep log files.
        /// If no value is configured, the default value is 30 days
        /// </summary>
        public static void CleanupLogFiles(string logFilePath)
        {
            var cutOffDate = DateTime.Today.AddDays(-1 * LogConfigAppSettings.CleanupLogFilesAfterDays);
            var logFiles = Directory.GetFiles(logFilePath, "*.*", SearchOption.AllDirectories);
            var filesToDelete = logFiles.Where(f => File.GetLastWriteTime(f) < cutOffDate);
            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    //Make sure that cleanup does not crash your application. if the system is unable to remove a log file, a next run will try again.
                }
            }
        }
    }
}
