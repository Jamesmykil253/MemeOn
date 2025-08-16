// Meme MOBA - CrowdControlServer (instrumented for debug color flashes)
using UnityEngine;
public static class CrowdControlServer
{
    public static void Knockback(GameObject targetRoot, Vector3 dir, float distance)
    {
        if (!targetRoot) return;
        var cc = targetRoot.GetComponent<CCTargetServer>() ?? targetRoot.AddComponent<CCTargetServer>();
        cc.ApplyKnockback(dir, distance);

        // Debug flash: cyan for knockback
        var dbg = targetRoot.GetComponentInChildren<DebugColorState>();
        if (dbg != null) dbg.SetStateColor(new Color(0.2f, 1f, 1f), 0.15f);
    }

    public static void Root(GameObject targetRoot, float duration)
    {
        if (!targetRoot) return;
        var cc = targetRoot.GetComponent<CCTargetServer>() ?? targetRoot.AddComponent<CCTargetServer>();
        cc.ApplyRoot(duration);

        // Debug flash: blue for root
        var dbg = targetRoot.GetComponentInChildren<DebugColorState>();
        if (dbg != null) dbg.SetStateColor(new Color(0.2f, 0.6f, 1f), 0.2f);
    }

    public static void Stun(GameObject targetRoot, float duration)
    {
        if (!targetRoot) return;
        var cc = targetRoot.GetComponent<CCTargetServer>() ?? targetRoot.AddComponent<CCTargetServer>();
        cc.ApplyStun(duration);

        // Debug flash: magenta for stun
        var dbg = targetRoot.GetComponentInChildren<DebugColorState>();
        if (dbg != null) dbg.SetStateColor(new Color(1f, 0.2f, 1f), 0.2f);
    }
}
