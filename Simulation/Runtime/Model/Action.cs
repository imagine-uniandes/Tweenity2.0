namespace Simulation.Runtime
{
    public class Action
    {
        public string ObjectAction;         // Name of GameObject (used to find MonoBehaviour)
        public string ActionName;           // Format: Script.Method
        public string ActionParams;         // Optional method params (still supported)
        public string ResponseID;           // ID of response to trigger (optional for simulator actions)
    }
}
