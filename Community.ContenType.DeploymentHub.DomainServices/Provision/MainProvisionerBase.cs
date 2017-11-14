using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.DomainServices.Provision
{
    public abstract class MainProvisionerBase : IMainProvisionerBase
    {
        private readonly IWebPropertyRepository _webPropertyRepository;
        private List<IVersionProvisioner> _versionProvisionners;
        private const string SettingKey = "AppDataVersion";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainProvisionerBase));

        protected MainProvisionerBase(IWebPropertyRepository webPropertyRepository)
        {
            _webPropertyRepository = webPropertyRepository;
        }

        public void Provision(Hub hub)
        {
            using (Logger.MethodTraceLogger(hub))
            {
                if (_versionProvisionners != null)
                {
                    Version currentVersion = GetCurrentVersion();
                    Logger.Debug("MainProvisionerBase -> current version:" + currentVersion);

                    //loop versions
                    Version version = new Version("0.0.0.0");
                    foreach (var versionProvisionner in _versionProvisionners)
                    {
                        version = versionProvisionner.GetVersion();

                        Logger.Debug("MainProvisionerBase -> version:" + version);

                        if (version > currentVersion)
                        {
                            Logger.Debug("MainProvisionerBase -> Start provisioning:" + version);
                            versionProvisionner.Provision(hub);
                        }
                    }
                    _webPropertyRepository.SetProperty(SettingKey, version.ToString());
                    _webPropertyRepository.SetProperty(Configs.IsHub, true);
                }
            }
        }

        protected virtual Version GetCurrentVersion()
        {
            using (Logger.MethodTraceLogger())
            {
                //get current version
                var maybeCurrentVersion = _webPropertyRepository.MaybeGetProperty<string>(SettingKey);

                var currentVersion = maybeCurrentVersion.Select(x => new Version(x)).Else(new Version("0.0.0.0"));

                Logger.Debug("GetCurrentVersionBase -> " + currentVersion);
                
                return currentVersion;
            }
        }

        public Version GetLastDataVersion()
        {
            using (Logger.MethodTraceLogger())
            {
                if (_versionProvisionners != null && _versionProvisionners.Count > 0)
                {
                    return _versionProvisionners[_versionProvisionners.Count - 1].GetVersion();
                }
                return new Version("0.0.0.0");
            }
        }

        public void SetNextVersionProvisioner(IVersionProvisioner versionProvisioner)
        {
            using (Logger.MethodTraceLogger(versionProvisioner))
            {
                if (_versionProvisionners == null)
                {
                    _versionProvisionners = new List<IVersionProvisioner>();
                }

                _versionProvisionners.Add(versionProvisioner);
            }
        }

        public bool IsProvisioned()
        {
            return _webPropertyRepository.MaybeGetProperty<string>(Configs.IsHub).Select(isHub => isHub.ToLowerInvariant() == "true").Else(false) 
                && GetCurrentVersion() == GetLastDataVersion();
        }
    }

    public class ProvisionOperationFailedException : Exception
    {
        public ProvisionOperationFailedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}