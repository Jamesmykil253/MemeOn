using UnityEngine;
using System.Collections;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using MemeArena.UI;
using MemeArena.Players;
using MemeArena.Combat;

namespace MemeArena.HUD
{
    /// <summary>
    /// Wires the local player's components to the HUD widgets at runtime.
    /// Keep each widget decoupled; this binder merely finds components and assigns references.
    /// Best practice: place on a Canvas in the HUD scene/prefab.
    /// </summary>
    public class PlayerHUDBinder : MonoBehaviour
    {
        [Header("Optional explicit targets (auto-found if null)")]
        [SerializeField] private HealthBarUI healthBar;
        [SerializeField] private XPBarUI xpBar;
        [SerializeField] private LevelUI levelUI;
        [SerializeField] private CoinCounterUI coinUI;
        [SerializeField] private BoostedAttackUI boostedUI;

        private void Awake()
        {
            // Try to find widgets locally if not assigned
            if (healthBar == null) healthBar = GetComponentInChildren<HealthBarUI>(true);
            if (xpBar == null) xpBar = GetComponentInChildren<XPBarUI>(true);
            if (levelUI == null) levelUI = GetComponentInChildren<LevelUI>(true);
            if (coinUI == null) coinUI = GetComponentInChildren<CoinCounterUI>(true);
            if (boostedUI == null) boostedUI = GetComponentInChildren<BoostedAttackUI>(true);
        }

        private void OnEnable()
        {
            StartCoroutine(BindWhenReady());
        }

        private IEnumerator BindWhenReady()
        {
            NetworkObject localPlayer = null;
            // Wait up to a few seconds for local player to spawn
            float t = 0f;
            while (localPlayer == null && t < 5f)
            {
                localPlayer = FindLocalPlayer();
                if (localPlayer == null)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            if (localPlayer == null) yield break;

            var health = localPlayer.GetComponentInChildren<NetworkHealth>();
            var stats = localPlayer.GetComponentInChildren<PlayerStats>();
            var inv = localPlayer.GetComponentInChildren<PlayerInventory>();
            var boosted = localPlayer.GetComponentInChildren<BoostedAttackTracker>();

            if (healthBar != null && health != null) healthBar.SetSource(health);
            if (xpBar != null && stats != null) xpBar.SetSource(stats);
            if (levelUI != null && stats != null) levelUI.SetSource(stats);
            if (coinUI != null && inv != null) coinUI.SetSource(inv);
            if (boostedUI != null && boosted != null) boostedUI.SetSource(boosted);
        }

        private NetworkObject FindLocalPlayer()
        {
            foreach (var no in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                if (no.IsOwner && no.IsPlayerObject)
                    return no;
            }
            return null;
        }
    }
}
