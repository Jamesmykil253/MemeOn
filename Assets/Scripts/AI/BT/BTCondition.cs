namespace MemeArena.AI.BT
{
    public abstract class BTCondition : BTNode
    {
        public override BTStatus Tick() => Evaluate() ? BTStatus.Success : BTStatus.Failure;
        protected abstract bool Evaluate();
    }
}
