using Unity.Netcode;
using UnityEngine;
using MemeArena.Players;

namespace MemeArena.Items
{
    /// <summary>
    /// Rotates visually; on server grants 1 coin if a player walks over it.
    /// Requires: NetworkObject + Trigger Collider on the coin.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class Coin : NetworkBehaviour
    {
        public float rotateDegPerSec = 90f;

        void Update()
        {
            // purely visual; can run on all clients
            transform.Rotate(0f, rotateDegPerSec * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            var inv = other.GetComponentInParent<PlayerInventory>();
            if (inv != null && other.CompareTag("Player"))
            {
                inv.AddCoins(1);
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }
}
