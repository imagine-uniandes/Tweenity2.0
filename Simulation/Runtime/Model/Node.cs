using System.Collections.Generic;
using Models;
using System.Linq;


namespace Simulation.Runtime
{
    public class Node
    {
            public string NodeID;                       // Unique, internal node reference
            public NodeType Type;                       // Used for runtime decisions (Start, Dialogue, etc.)
            public List<Response> responses;            // Defined paths to other nodes
            public List<Action> userActions;            // Interactive triggers tied to responses
            public List<Action> simulatorActions;       // Auto-run actions when entering the node

        public Response GetResponseByID(string responseID)
        {
            return responses.FirstOrDefault(r => r.ResponseID == responseID);
        }

        public Action GetUserAction(string objectName, string methodName)
        {
            return userActions.FirstOrDefault(a => a.ObjectAction == objectName && a.ActionName == methodName);
        }
    }
}
