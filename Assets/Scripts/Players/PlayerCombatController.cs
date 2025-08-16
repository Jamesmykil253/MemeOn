using Unity.Netcode;
using UnityEngine;
using MemeArena.Combat;

namespace MemeArena.Players
{
    public class PlayerCombatController : NetworkBehaviour
    {
        public GameObject projectilePrefab;
        public int damage = 10;

        [ServerRpc]
        public void ServerFire()
        {
            if (!IsServer) return;
            if (!projectilePrefab) return;

            var go = Instantiate(projectilePrefab, transform.position + transform.forward * 0.6f + Vector3.up * 0.8f, transform.rotation);
            var proj = go.GetComponent<ProjectileServer>();
            if (!proj) proj = go.AddComponent<ProjectileServer>();
            proj.Launch(gameObject, damage, 22f, 3f);
            go.GetComponent<NetworkObject>()?.Spawn();
        }
    }
}
