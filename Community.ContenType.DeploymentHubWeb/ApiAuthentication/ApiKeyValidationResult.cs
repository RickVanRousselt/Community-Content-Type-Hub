namespace Community.ContenType.DeploymentHubWeb.ApiAuthentication
{
    public class ApiKeyValidationResult
    {
        public ApiKeyValidationResult(bool isValid, string errorMessages)
        {
            IsValid = isValid;
            ErrorMessages = errorMessages;
        }

        public bool IsValid { get; }
        public string ErrorMessages { get; }
    }
}