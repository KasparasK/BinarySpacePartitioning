using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PositionAndRotation
{
    public Vector3 position;
    public Quaternion rotation;
    public bool viablePosFound;
}

public class ObjectSpawning : MonoBehaviour
{
    private GameObject spawnBoundsObj;

    private Bounds spawnBounds;
    private Bounds combinedBounds;

    private Vector3 spawnTopRightCorner;
    private Vector3 spawnBotLeftCorner;

    private Vector3 objSpawnAreaTopRightCorner;
    private Vector3 objSpawnAreaBotLeftCorner;

    private Vector3 objBoundsTopRight, objBoundsBotLeft;//, objBoundsTopLeft, objBoundsBotRight;

    private Partition treasureRoot;

    private float topOffeset, botOffset, rightOffset, leftOffset;

    private System.Random rng;
    public void Setup (GameObject spawnBoundsObj, float topOffeset, float botOffset, float rightOffset, float leftOffset)
    {
        this.topOffeset = topOffeset;
        this.botOffset = botOffset;
        this.rightOffset = rightOffset;
        this.leftOffset = leftOffset;

        this.spawnBoundsObj = spawnBoundsObj;
    }

    public void ClearPartitions()
    {
        treasureRoot = null;
    }

    void FirstObject()
    {
        rng = new System.Random();
        SetTotalSpawnArea();
        treasureRoot = new Partition(spawnBotLeftCorner, spawnTopRightCorner, null);
        
    }
    void SetTotalSpawnArea()
    {
        spawnBounds = spawnBoundsObj.GetComponent<Renderer>().bounds;
        Vector3 center = spawnBounds.center;

        spawnTopRightCorner = new Vector3(center.x + spawnBounds.extents.x, center.y, center.z + spawnBounds.extents.z);
        spawnBotLeftCorner = new Vector3(center.x - spawnBounds.extents.x, center.y, center.z - spawnBounds.extents.z);

        spawnTopRightCorner -= new Vector3(topOffeset, 0, leftOffset);
        spawnBotLeftCorner += new Vector3(botOffset, 0, rightOffset);

    }

    GameObject ResetPosition(GameObject toReset)
    {
        toReset.transform.position = Vector3.zero;
        return toReset;
    }
    public PositionAndRotation GetSpawnPosition(GameObject meshRoot,Quaternion desiredRotation)
    {
        PositionAndRotation posAndRot = new PositionAndRotation();
        List<Partition> viable = new List<Partition>();

        meshRoot = ResetPosition(meshRoot);
        meshRoot.transform.rotation = desiredRotation;

        if (treasureRoot == null)
            FirstObject();

        combinedBounds = HelperMethods.GetCombinedBounds(meshRoot);

        Vector3 center = combinedBounds.center;
        Vector3 extents = combinedBounds.extents;

        SetObjectBoundsPoints(center, extents);

        Vector3 rotatedSize =  objBoundsTopRight - objBoundsBotLeft ;
        
        treasureRoot.GetPartionsOfSize(rotatedSize, ref viable);

        if (viable.Count == 0)
        {
            posAndRot.viablePosFound = false;
            Debug.LogError("no viable treasure spot found");
            return posAndRot;
        }

        posAndRot.rotation = desiredRotation;

        Vector3 rotatedExtents = rotatedSize / 2;
        Vector3 pivotOffset = center;

        int rngPart = rng.Next(0, viable.Count);

        SetViableObjSpawnArea(viable[rngPart],rotatedExtents, pivotOffset, center);

        Vector3 pos = GetRandomSpawnPosition();

        ChangeObjectBoundsAcordingToSpawnPos(pos, rotatedExtents, pivotOffset);

        viable[rngPart].PartitionThis(
            objBoundsBotLeft,
            objBoundsTopRight
        );
        posAndRot.viablePosFound = true;
        posAndRot.position = pos;

        return posAndRot;
    }

    void SetViableObjSpawnArea(Partition partition, Vector3 rotatedExtents, Vector3 pivotOffset, Vector3 center)
    {
        objSpawnAreaTopRightCorner = new Vector3(
            partition._topRight.x - rotatedExtents.x - pivotOffset.x,
            rotatedExtents.y - center.y, // exactly on top of spawn plane
            partition._topRight.z - rotatedExtents.z - pivotOffset.z
        );

        objSpawnAreaBotLeftCorner = new Vector3(
            partition._botLeft.x + rotatedExtents.x - pivotOffset.x,
            rotatedExtents.y - center.y, // exactly on top of spawn plane
            partition._botLeft.z + rotatedExtents.z - pivotOffset.z
        );
    }

    Vector3 GetRandomSpawnPosition() //in a viable area
    {
        float x = Random.Range(objSpawnAreaBotLeftCorner.x, objSpawnAreaTopRightCorner.x);
        float y = objSpawnAreaBotLeftCorner.y;
        float z = Random.Range(objSpawnAreaBotLeftCorner.z, objSpawnAreaTopRightCorner.z);

        return  new Vector3(x, y, z);
    }

    void ChangeObjectBoundsAcordingToSpawnPos(Vector3 pos, Vector3 rotatedExtents, Vector3 pivotOffset)
    {
            objBoundsTopRight = new Vector3(
            pos.x + rotatedExtents.x + pivotOffset.x,
            pos.y + rotatedExtents.y + pivotOffset.y,
            pos.z + rotatedExtents.z + pivotOffset.z
        );

            objBoundsBotLeft = new Vector3(
            pos.x - rotatedExtents.x + pivotOffset.x,
            pos.y - rotatedExtents.y + pivotOffset.y,
            pos.z - rotatedExtents.z + pivotOffset.z
        );
    }
   
    void SetObjectBoundsPoints(Vector3 center, Vector3 extents)
    {
        objBoundsTopRight = new Vector3(center.x + extents.x,center.y + extents.y, center.z + extents.z);
    /*    objBoundsTopLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
        objBoundsBotRight = new Vector3(center.x + extents.x,center.y - extents.y, center.z - extents.z);*/
        objBoundsBotLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
    }

    #region Debuging/visualization
    void VisualizePartitions(Partition root)
    {
        List<Partition> partitions = new List<Partition>();
        root.GetAllPartitions(ref partitions);
        for (int i = 0; i < partitions.Count; i++)
        {
            if (partitions[i].isUsed)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireCube(partitions[i].center, partitions[i].size);
        }
    }
    void OnDrawGizmos()
    {

        if (Application.isPlaying)
        {
            VisualizePartitions(treasureRoot);
        //    VisualizePartitions(coinsRoot);

        }

        VisualizePartitions(treasureRoot);
      //  VisualizePartitions(coinsRoot);
        //   Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
        float size = 0.1f;

       /*  Gizmos.DrawSphere(coinsSpawnAreaTopRight, size);
         Gizmos.DrawSphere(coinsSpawnAreaBotLeft, size);
         */
        //   Gizmos.DrawSphere(objBoundsTopRight, size);
    /*       Gizmos.DrawSphere(objBoundsTopLeft, size);
        Gizmos.DrawSphere(objBoundsBotRight, size);*/
        //   Gizmos.DrawSphere(objBoundsBotLeft, size);
        /*    Gizmos.DrawSphere(objSpawnAreaTopRightCorner, size);
            Gizmos.DrawSphere(objSpawnAreaBotLeftCorner, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(objBoundsBotLeft, size);
            Gizmos.DrawSphere(objBoundsTopRight, size);*/
        //  Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
    }


    #endregion

}