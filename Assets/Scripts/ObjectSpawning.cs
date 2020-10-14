using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PositionAndRotation
{
    public Vector3 position;
    public Quaternion rotation;
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
            Debug.LogError("no viable treasure spot found");
            return null;
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
           Gizmos.DrawSphere(objBoundsTopRight, size);
    /*       Gizmos.DrawSphere(objBoundsTopLeft, size);
        Gizmos.DrawSphere(objBoundsBotRight, size);*/
           Gizmos.DrawSphere(objBoundsBotLeft, size);
        /*    Gizmos.DrawSphere(objSpawnAreaTopRightCorner, size);
            Gizmos.DrawSphere(objSpawnAreaBotLeftCorner, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(objBoundsBotLeft, size);
            Gizmos.DrawSphere(objBoundsTopRight, size);*/
        //  Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
    }


    #endregion

}

public class Partition
{
    public List<Partition> partitions;

    public Vector3 _botLeft;
    public Vector3 _topRight;
    public Vector3 size;
    public Vector3 center;

    public bool isUsed;

    public Partition(Vector3 botLeft, Vector3 topRight, Partition parent, bool isUsed = false)
    {
        _botLeft = botLeft;
        _topRight = topRight;

        SetPartitionParams(botLeft, topRight);
        
        this.isUsed = isUsed;
    }

    void SetPartitionParams(Vector3 botLeft, Vector3 topRight)
    {
        center = (botLeft + topRight) / 2;
        size = topRight - botLeft;
    }
    bool IsABiggerThanB(Vector3 a, Vector3 b)
    {
        if (a.x >= b.x && a.z >= b.z)
            return true;

        return false;
    }

    public void GetAllPartitions(ref List<Partition> added)
    {
        added.Add(this);
        if (partitions != null)
        {
            for (int i = 0; i < partitions.Count; i++)
            {
                partitions[i].GetAllPartitions(ref added);
            }
        }
    }

    public void GetPartionsOfSize(Vector3 size, ref List<Partition> added)
    {
        if (partitions == null)
        {
            if (!isUsed && IsABiggerThanB(this.size, size))
                added.Add(this);
        }
        else
        {
            for (int i = 0; i < partitions.Count; i++)
            {
                partitions[i].GetPartionsOfSize(size, ref added);
            }
        }
    }

    public void PartitionThis(Vector3 botLeft, Vector3 topRight)
    {
        partitions = new List<Partition>();

        float y = botLeft.y;

        Side0(botLeft, y);
        Side1(botLeft,topRight,y);
        Side2(topRight, y);
        Side3(botLeft, topRight, y);

        CenterPartition(botLeft, topRight);
    }
    //0 1 2
    //7 8 3
    //6 5 4
    // 7,0 column (side 0)
    // 1,2 row    (side 1)
    // 3,4 column (side 2)
    // 5,6 row    (side 3)

    void Side0(Vector3 objBotLeft, float y)
    {
        Vector3 tR = new Vector3(objBotLeft.x, y, _topRight.z);
        Vector3 bL = new Vector3(_botLeft.x, y, objBotLeft.z);
        partitions.Add(new Partition(bL, tR, this));

    }
    void Side1(Vector3 objBotLeft, Vector3 objTopRight, float y)
    {
        Vector3 tR = new Vector3(_topRight.x, y, _topRight.z);
        Vector3 bL = new Vector3(objBotLeft.x, y, objTopRight.z);
        partitions.Add(new Partition(bL, tR, this));

    }

    void Side2( Vector3 objTopRight, float y)
    {
        Vector3 tR = new Vector3(_topRight.x, y, objTopRight.z);
        Vector3 bL = new Vector3(objTopRight.x, y, _botLeft.z);
        partitions.Add(new Partition(bL, tR, this));

    }

    void Side3(Vector3 objBotLeft, Vector3 objTopRight, float y)
    {
        Vector3 tR = new Vector3(objTopRight.x, y, objBotLeft.z);
        Vector3 bL = new Vector3(_botLeft.x, y, _botLeft.z);
        partitions.Add(new Partition(bL, tR, this));

    }

    void CenterPartition(Vector3 mainbL, Vector3 maintR)
    {
        partitions.Add(new Partition(mainbL, maintR, this, true));
    }
}
