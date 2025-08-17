namespace MemeArena.Combat
{
    /// <summary>
    /// Enumeration of possible damage types.  Extend as needed for status
    /// effects or elemental interactions.  Not currently used by the AI but
    /// provided for completeness.
    /// </summary>
    public enum DamageType
    {
        Generic,
        Melee,
        Ranged,
        Fire,
        Ice
    }
}