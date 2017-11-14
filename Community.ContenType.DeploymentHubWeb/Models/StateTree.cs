using Newtonsoft.Json;

namespace Community.ContenType.DeploymentHubWeb.Models
{
    public class StateTree
    {
        [JsonProperty(PropertyName = "checkbox_disabled")]
        public bool CheckboxState { get; private set; }
        [JsonProperty(PropertyName = "opened")]
        public bool OpenNode { get; private set; }

        public StateTree(bool checkboxState, bool openNode)
        {
            CheckboxState = checkboxState;
            OpenNode = openNode;
        }
    }
}