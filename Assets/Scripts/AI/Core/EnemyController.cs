using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Back-compat alias for code that expects 'EnemyController'.
    /// Inherits AIController behavior.
    /// </summary>
    [System.Obsolete("Use AIController. EnemyController is a legacy alias and will be removed." )]
    [UnityEngine.AddComponentMenu("")]
    public class EnemyController : AIController { }
}
