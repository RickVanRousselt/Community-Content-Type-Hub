using Community.ContenType.DeploymentHubWeb.Models;
using System.Collections.Generic;

namespace Community.ContenType.DeploymentHubWeb.ViewModels
{
    public class PromoteIndexViewModel : TreeViewModel
    {
        public PromoteIndexViewModel(IList<DeploymentGroupSubTree> treeData, bool promoteSourceFound, string nextPillar) : base(treeData)
        {
            PromoteSourceFound = promoteSourceFound;
            NextPillar = nextPillar;
        }

        public bool PromoteSourceFound { get; }
        public string NextPillar { get; }
    }
}