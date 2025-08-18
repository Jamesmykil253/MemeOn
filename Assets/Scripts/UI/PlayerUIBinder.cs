using System;
using UnityEngine;

namespace MemeArena.UI
{
    /// <summary>
    /// Deprecated alias. Use MemeArena.HUD.PlayerHUDBinder instead.
    /// Kept to avoid breaking existing scenes/prefabs; has no unique behavior.
    /// </summary>
    [Obsolete("Use MemeArena.HUD.PlayerHUDBinder instead.")]
    [AddComponentMenu("")] // hide from Add Component
    public class PlayerUIBinder : MemeArena.HUD.PlayerHUDBinder { }
}
