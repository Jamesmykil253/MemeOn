namespace MemeArena.AI.BT
{
    public enum BTStatus { Success, Failure, Running }

    public abstract class BTNode
    {
        public abstract BTStatus Tick();
    }
}
