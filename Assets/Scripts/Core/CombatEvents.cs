// Meme Online Battle Arena - CombatEvents
using System;
public static class CombatEvents
{
    // victimId, attackerId, amount
    public static event Action<ulong, ulong, float> OnDamageReceived;
    // attackerId, victimId, amount
    public static event Action<ulong, ulong, float> OnSuccessfulHit;
    // attackerId, victimId, reason
    public static event Action<ulong, ulong, string> OnFailedHit;

    public static void RaiseDamageReceived(ulong victimId, ulong attackerId, float amount)
        => OnDamageReceived?.Invoke(victimId, attackerId, amount);

    public static void RaiseSuccessfulHit(ulong attackerId, ulong victimId, float amount)
        => OnSuccessfulHit?.Invoke(attackerId, victimId, amount);

    public static void RaiseFailedHit(ulong attackerId, ulong victimId, string reason)
        => OnFailedHit?.Invoke(attackerId, victimId, reason);
}
