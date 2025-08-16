namespace MemeArena.AI.BT.Nodes
{
    public class ConditionHasTarget : BTCondition
    {
        private readonly AIController _ctrl;
        public ConditionHasTarget(AIController ctrl) { _ctrl = ctrl; }
        protected override bool Evaluate() => _ctrl.HasTarget();
    }
}
