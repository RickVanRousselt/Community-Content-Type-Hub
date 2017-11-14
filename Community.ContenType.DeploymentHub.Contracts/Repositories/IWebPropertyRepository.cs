using System.Collections.Generic;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IWebPropertyRepository
    {
        Dictionary<string, object> GetProperties();
        void SetProperty(string key, object value);
        May<T> MaybeGetProperty<T>(string key);
    }
}