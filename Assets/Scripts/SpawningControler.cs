using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawningControler : MonoBehaviour
{
    public GameObject spawnBoundsObj;

    public GameObject objToSpawn;

    public ObjectSpawning objectSpawning;

    private List<GameObject> _spawned;

    private List<GameObject> spawned
    {
        get
        {
            if (_spawned == null)
            {
                _spawned = new List<GameObject>();
            }

            return _spawned;
        }
        set
        {
            _spawned = value;

        }
    }


    public void Spawn()
    {
        foreach (var obj in spawned)
        {
            DestroyImmediate(obj);
        }
        objectSpawning.ClearPartitions();

        objectSpawning.Setup(spawnBoundsObj);

         PositionAndRotation pos = objectSpawning.GetTreasureSpawnPosition(objToSpawn, 0.5f, true);

        GameObject obs =  Instantiate(objToSpawn, pos.position,Quaternion.Euler(pos.rotation));
        spawned.Add(obs);
    }
}
