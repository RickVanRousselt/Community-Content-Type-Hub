using System;
using System.Collections.Generic;

namespace Community.ContenType.DeploymentHub.Common.Contracts.Exceptions
{
    public class CreateSharePointContextException : Exception
    {
        public CreateSharePointContextException(List<string> createSharePointContextErrors) : base(CreateMessage(createSharePointContextErrors))
        {
        }

        public static string CreateMessage(List<string> createSharePointContextErrors)
        {
            var message = "Unable to create SharePoint context. Detailed errors: ";
            foreach (var createSharePointContextError in createSharePointContextErrors)
            {
                message += createSharePointContextError + ";";
            }
            return message.TrimEnd(';');
        }
    }
}
