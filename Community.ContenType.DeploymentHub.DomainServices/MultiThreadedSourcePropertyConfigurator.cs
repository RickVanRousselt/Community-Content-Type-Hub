using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Community.ContenType.DeploymentHub.Domain.Core;
using log4net;
using MoreLinq;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public class MultiThreadedSourcePropertyConfigurator : ISourcePropertyConfigurator
    {
        private readonly ISourcePropertyConfigurator _baseConfigurator;
        private readonly int _numberOfThreads;
        private readonly TaskFactory _taskFactory = new TaskFactory();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MultiThreadedSourcePropertyConfigurator));

        public MultiThreadedSourcePropertyConfigurator(ISourcePropertyConfigurator baseConfigurator, int numberOfThreads)
        {
            _baseConfigurator = baseConfigurator;
            _numberOfThreads = numberOfThreads;
        }

        public void ConfigureSources(ISet<SiteCollection> siteCollections, Hub sourceHub)
        {
            if (!siteCollections.Any()) return;
            var tasks = Batch(siteCollections)
                .Select(batch => new HashSet<SiteCollection>(batch))
                .Select(batch => _taskFactory.StartNew(() => _baseConfigurator.ConfigureSources(batch, sourceHub)))
                .ToList();

            Logger.Debug($"Started {tasks.Count} threads to configure sources on {siteCollections.Count} Target Site Collections.");
            Task.WaitAll(tasks.Cast<Task>().ToArray());
            Logger.Debug($"Finished configuring all sources");
        }

        private IEnumerable<IEnumerable<T>> Batch<T>(ICollection<T> input)
        {
            var queueSize = (int)Math.Ceiling((double)input.Count / _numberOfThreads);
            return input.Batch(queueSize);
        }
    }
}