using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class SpawningControler : MonoBehaviour
{
    public GameObject spawnBoundsObj;

    public GameObject objToSpawn;

    public ObjectSpawning objectSpawning;

    private List<GameObject> _spawned;
    [Range(-180,180)]
    public float xAngle;
    [Range(-180, 180)]
    public float yAngle;
    [Range(-180, 180)]
    public float zAngle;

    public float topOffeset, botOffset, rightOffset, leftOffset;

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
        objectSpawning.Setup(spawnBoundsObj, topOffeset, botOffset, rightOffset, leftOffset);


        for (int i = 0; i < 4; i++)
        {
          //  Quaternion desiredRotation = Quaternion.Euler(xAngle, yAngle, zAngle);
            Quaternion desiredRotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));

            PositionAndRotation pos = objectSpawning.GetSpawnPosition(objToSpawn, desiredRotation);
            if (pos.viablePosFound)
            {
                GameObject obs = Instantiate(objToSpawn, pos.position, pos.rotation);

                spawned.Add(obs);
            }
        }

   
    }
}
