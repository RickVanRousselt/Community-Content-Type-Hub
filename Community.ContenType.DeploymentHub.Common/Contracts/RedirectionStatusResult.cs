using System.Collections.Generic;

namespace Community.ContenType.DeploymentHub.Common.Contracts
{
    public class RedirectionStatusResult
    {
        private RedirectionStatus _redirectionStatus;
        private List<string> _redirectionErrors;

        public RedirectionStatusResult(RedirectionStatus redirectionStatus, List<string> redirectionErrors)
        {
            _redirectionErrors = redirectionErrors;
            if (_redirectionErrors == null)
            {
                _redirectionErrors = new List<string>();
            }

            _redirectionStatus = redirectionStatus;
        }

        public RedirectionStatus RedirectionStatus
        {
            get
            {
                return _redirectionStatus;
            }
        }

        /// <summary>
        /// This method returns a list of reasons why the redirect is not possible
        /// </summary>
        /// <returns>List of errors</returns>
        public List<string> RedirectionErrors
        {
            get
            {
                return _redirectionErrors;
            }
        }

        /// <summary>
        /// This method creates a string containing all the reasons why a redirect is not possible
        /// </summary>
        /// <returns></returns>
        public string GetRedirectionErrorString()
        {
            string errors = string.Empty;
            foreach (var redirectionError in _redirectionErrors)
            {
                errors += redirectionError + ";";
            }
            return errors.TrimEnd(';');
        }
    }
}
