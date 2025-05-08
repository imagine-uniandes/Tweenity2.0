namespace Simulation.Runtime
{
    public enum ActionInstructionType
    {
        Action,
        Wait,
        Remind
    }

    public class Action
    {
        public string ObjectAction;
        public string ActionName;
        public string ActionParams;
        public ActionInstructionType Type;
        public string ResponseID;
    }

}
