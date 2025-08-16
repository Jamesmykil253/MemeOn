using UnityEngine;

namespace MemeArena.AI.BT.Nodes
{
    public class TaskAimAtTarget : BTAction
    {
        private readonly AIController _ctrl;
        public TaskAimAtTarget(AIController ctrl) { _ctrl = ctrl; }

        public override BTStatus Tick()
        {
            var t = _ctrl.TargetTransform();
            if (!t) return BTStatus.Failure;
            _ctrl.FaceToward(t.position, Time.fixedDeltaTime);
            return BTStatus.Success;
        }
    }
}
