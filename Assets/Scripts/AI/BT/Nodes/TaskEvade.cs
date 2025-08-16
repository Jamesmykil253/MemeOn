using UnityEngine;

namespace MemeArena.AI.BT.Nodes
{
    public class TaskEvade : BTAction
    {
        private readonly AIController _ctrl;
        public TaskEvade(AIController ctrl) { _ctrl = ctrl; }

        public override BTStatus Tick()
        {
            _ctrl.MoveDeterministic(new Vector3(1f, 0f, 0f), Time.fixedDeltaTime);
            return BTStatus.Running;
        }
    }
}
