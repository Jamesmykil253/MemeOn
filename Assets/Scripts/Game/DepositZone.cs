using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using MemeArena.Items;
using MemeArena.Network;

namespace MemeArena.Game
{
    /// <summary>
    /// Deposit zone for banking collected coins.  When a player of the same
    /// team enters and remains in the zone, their coins will be transferred
    /// to the match score after a cooldown.  This component requires a
    /// Collider with <c>IsTrigger</c> enabled and a NetworkObject.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkObject))]
    public class DepositZone : NetworkBehaviour
    {
        [Tooltip("Team that this deposit zone belongs to.  Only players on this team can deposit coins here.")]
        [SerializeField] private int teamId = 0;

        [Tooltip("Cooldown in seconds between deposit actions for each player.")]
        [SerializeField] private float depositCooldown = MemeArena.Network.ProjectConstants.Match.DepositCooldown;

        // Keep track of the last deposit time for each player by their network object id.
        private readonly Dictionary<ulong, float> _lastDepositTime = new Dictionary<ulong, float>();

        private void Reset()
        {
            // Ensure the collider is a trigger by default when added.
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            // Deposits are only processed on the server to maintain authority.
            if (!IsServer) return;

            // Only players can deposit coins.  Check the tag defined in constants.
            if (!other.CompareTag(ProjectConstants.Tags.Player)) return;

            // Ensure the player has a TeamId component and belongs to this deposit zone's team.
            var teamComponent = other.GetComponent<TeamId>();
            if (teamComponent == null || teamComponent.team != teamId) return;

            // Player must have an inventory to deposit.
            var inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            // Determine how long it's been since this player last deposited coins.
            ulong playerNetId = other.GetComponent<NetworkObject>().NetworkObjectId;
            if (_lastDepositTime.TryGetValue(playerNetId, out float lastTime))
            {
                if (Time.time - lastTime < depositCooldown) return;
            }

            // Withdraw coins from the player.  If none are held, nothing to deposit.
            int coins = inventory.WithdrawCoins();
            if (coins <= 0) return;

            // Add the coins to the match score.  Only the server should modify scores.
            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.AddScore(teamId, coins);
            }

            // Record the deposit time to enforce the cooldown.
            _lastDepositTime[playerNetId] = Time.time;
        }
    }
}