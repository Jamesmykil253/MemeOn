using System;
using System.Linq;
using UnityEngine;

namespace MemeArena.UI
{
    /// <summary>
    /// Deprecated alias. Migrates to MemeArena.HUD.PlayerHUDBinder at runtime and disables itself.
    /// </summary>
    [Obsolete("Use MemeArena.HUD.PlayerHUDBinder instead.")]
    [AddComponentMenu("")] // hide from Add Component
    public class PlayerUIBinder : MonoBehaviour
    {
        private void Reset() => Migrate();
        private void Awake() => Migrate();

        private void Migrate()
        {
            var t = Type.GetType("MemeArena.HUD.PlayerHUDBinder, MemeArena.Runtime");
            if (t == null)
            {
                t = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                    .FirstOrDefault(x => x.FullName == "MemeArena.HUD.PlayerHUDBinder");
            }
            if (t != null && typeof(Component).IsAssignableFrom(t))
            {
                if (GetComponent(t) == null)
                {
                    gameObject.AddComponent(t);
                }
            }
            // Disable this deprecated component to avoid duplicate binding
            var behaviour = this as Behaviour;
            if (behaviour != null) behaviour.enabled = false;
        }
    }
}
