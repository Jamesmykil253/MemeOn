using System.Collections;
using Unity.Netcode;
using UnityEngine;
using MemeArena.Players;
using MemeArena.Combat;

namespace MemeArena.Game
{
    /// <summary>
    /// Goal/Neutral zones.
    /// - Own goal: heal and channel deposit (0.5s per coin)
    /// - Enemy goal: slow while inside
    /// - Neutral: no heal, no slow, contestable
    /// Requires: Trigger Collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DepositZone : NetworkBehaviour
    {
        public enum ZoneType { TeamGoal, Neutral }
        public ZoneType zoneType = ZoneType.TeamGoal;

        [Header("Team Goal Settings")]
        public int teamId = 0;
        public float healPerSecond = 10f;
        public float depositSecondsPerCoin = 0.5f;

        [Header("Enemy Goal Slow")]
        [Range(0.1f, 1f)] public float enemySlowMultiplier = 0.7f;

        // Track ongoing deposits per player
        readonly System.Collections.Generic.Dictionary<NetworkObject, Coroutine> _deposits
            = new System.Collections.Generic.Dictionary<NetworkObject, Coroutine>();

        private void OnTriggerStay(Collider other)
        {
            if (!IsServer) return;

            // Work with parent objects to be robust to child colliders
            var nob = other.GetComponentInParent<NetworkObject>();
            if (nob == null) return;
            var move = other.GetComponentInParent<MemeArena.Players.PlayerMovement>();
            var tid = other.GetComponentInParent<MemeArena.Network.TeamId>();
            var inv = other.GetComponentInParent<MemeArena.Players.PlayerInventory>();
            var health = other.GetComponentInParent<MemeArena.Combat.NetworkHealth>();

            if (zoneType == ZoneType.TeamGoal && tid != null)
            {
                if (tid.team == teamId)
                {
                    // heal friends over time using frame delta because OnTriggerStay is per-frame
                    if (health != null)
                    {
                        int amt = Mathf.CeilToInt(healPerSecond * Time.deltaTime);
                        if (amt > 0) health.Heal(amt);
                    }

                    // Start/continue deposit if has coins
                    if (inv != null && inv.Coins.Value > 0)
                    {
                        if (!_deposits.ContainsKey(nob))
                        {
                            _deposits[nob] = StartCoroutine(DepositRoutine(nob, inv));
                        }
                    }
                }
                else
                {
                    // Enemy on this goal -> slow while inside
                    if (move != null) move.SetExternalSpeedMultiplier(enemySlowMultiplier);
                }
            }
            // Neutral: nothing passive; contest rules can be added here
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;
            var nob = other.GetComponentInParent<NetworkObject>();
            if (nob == null) return;

            // Cancel channel on exit
            if (_deposits.TryGetValue(nob, out var co))
            {
                StopCoroutine(co);
                _deposits.Remove(nob);
            }

            // Reset slow when leaving enemy goal
            var move = other.GetComponentInParent<MemeArena.Players.PlayerMovement>();
            var tid = other.GetComponentInParent<MemeArena.Network.TeamId>();
            if (move != null && tid != null && zoneType == ZoneType.TeamGoal && tid.team != teamId)
            {
                move.SetExternalSpeedMultiplier(1f);
            }
        }

    IEnumerator DepositRoutine(NetworkObject player, MemeArena.Players.PlayerInventory inv)
        {
            // Channel time scales with coin count at start of channel
            int coins = Mathf.Max(0, inv.Coins.Value);
            if (coins == 0) { yield break; }
            float channel = depositSecondsPerCoin * coins;

            float t = 0f;
            while (t < channel)
            {
                // Interrupt if player left or lost NetworkObject/Inventory
                if (player == null || inv == null) yield break;
                // Interrupt if coins changed to 0 during channel
                if (inv.Coins.Value <= 0) yield break;

                t += Time.deltaTime;
                yield return null;
            }

            // Complete deposit
            int deposited = inv.DepositAll();
            if (deposited > 0 && MatchManager.Instance != null)
            {
                var tid = player.GetComponent<MemeArena.Network.TeamId>();
                MatchManager.Instance.AddScore(tid.team, deposited);
            }

            _deposits.Remove(player);
        }
    }
}
