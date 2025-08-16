// Meme Online Battle Arena - TeamUtility
using UnityEngine;
public static class TeamUtility
{
    public static bool IsFriendly(GameObject a, GameObject b)
    {
        if (!a || !b) return false;
        var ta = a.GetComponentInParent<Team>();
        var tb = b.GetComponentInParent<Team>();
        if (ta == null || tb == null) return false;
        return ta.TeamIndex.Value == tb.TeamIndex.Value && ta.TeamIndex.Value != 0;
    }
}
