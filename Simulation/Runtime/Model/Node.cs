using System.Collections.Generic;

namespace Simulation.Runtime
{
    public class Node
    {
        public string NodeID; 
        public string title;
        public string text;
        public List<string> tags;
        public List<Action> userActions;
        public List<Action> simulatorActions;
        public List<Response> responses;
    }
}
