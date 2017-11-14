namespace Community.ContenType.DeploymentHubWeb.Models
{
    public class PullSiteCollectionResult
    {
        public PullSiteCollectionResult(bool pullSucceeded, string errorMessage)
        {
            PullSucceeded = pullSucceeded;
            ErrorMessage = errorMessage;
        }

        public bool PullSucceeded { get; }
        public string ErrorMessage { get; }
    }
}