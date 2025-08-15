Usage Notes for AISpawnerManager
	1	Prefab Assignment
	◦	Assign the AI_StationarySentinel prefab in the inspector to aiPrefab.
	◦	Make sure it’s registered in NetworkManager > Network Prefabs.
	2	Spawn Points
	◦	Create empty GameObjects in the scene for each spawn location.
	◦	Drag them into spawnPoints list in the inspector.
	3	AI Death Hook
	◦	In AIController, when entering Dead state, call: csharp CopyEdit   FindObjectOfType<AISpawnerManager>()?.HandleAIDeath(NetworkObjectId);
	◦	  
	4	Respawn Control
	◦	Toggle allowRespawn and set respawnDelay as desired.
	5	Dynamic Spawning
	◦	You can register new spawn points at runtime via: csharp CopyEdit   myAISpawnerManager.RegisterSpawnPoint(newTransform);
	◦	  
	6	Server-Only
	◦	All spawning/despawning logic runs server-side.
	◦	Clients receive AI positions and states through Netcode replication.
