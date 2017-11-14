using Community.ContenType.DeploymentHubWeb.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Community.ContenType.DeploymentHubWeb.ViewModels
{
    public class TreeViewModel
    {
        private readonly IList<DeploymentGroupSubTree> _deploymentGroupSubTrees;

        public TreeViewModel(IList<DeploymentGroupSubTree> deploymentGroupSubTrees)
        {
            _deploymentGroupSubTrees = deploymentGroupSubTrees;
        }

        public string DeploymentGroupSubTreesJson => JsonConvert.SerializeObject(_deploymentGroupSubTrees);
    }
}