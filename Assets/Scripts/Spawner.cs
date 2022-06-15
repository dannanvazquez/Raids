using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private GameObject spawnedPrefab;
    [SerializeField] private float spawnTime;
    [SerializeField] private bool instantSpawn;
    private float spawnTimeRemaining;
    GameObject spawnedItem = null;

    [ServerCallback]
    private void Start() {
        if (instantSpawn) {
            spawnTimeRemaining = 0;
        } else {
            spawnTimeRemaining = spawnTime;
        }
    }

    [ServerCallback]
    private void Update() {
        if (spawnedItem != null) return;
        if (spawnTimeRemaining > 0) {
            spawnTimeRemaining -= Time.deltaTime;
        } else {
            spawnedItem = Instantiate(spawnedPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(spawnedItem);
            spawnTimeRemaining = spawnTime;
        }
    }
}
