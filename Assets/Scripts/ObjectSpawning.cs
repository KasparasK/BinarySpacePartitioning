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

    private Vector3 coinsSpawnAreaTopRight;
    private Vector3 coinsSpawnAreaBotLeft;

    Vector3 objBoundsTopRight, objBoundsTopLeft, objBoundsBotRight, objBoundsBotLeft;

    private Partition treasureRoot;
    private Partition coinsRoot;

    private float objectY;

    public void Setup (GameObject spawnBoundsObj)
    {
        this.spawnBoundsObj = spawnBoundsObj;
    }

    public void ClearPartitions()
    {
        treasureRoot = null;
    }

    void FirstObject()
    {
        SetSpawnArea();
        treasureRoot = new Partition(spawnBotLeftCorner, spawnTopRightCorner, null);
        
    }

    void FirstCoint()
    {
        objectY = combinedBounds.center.y + combinedBounds.extents.y;
        //Debug.Log(spawnTopRightCorner - spawnBotLeftCorner);

        Vector3 botOffset = new Vector3(0.05f, 0, 0.14f);
        Vector3 topOffset = new Vector3(0.05f, 0, 0.07f);
        coinsSpawnAreaTopRight = spawnTopRightCorner - topOffset;
        coinsSpawnAreaBotLeft = spawnBotLeftCorner + botOffset;

        coinsRoot = new Partition(coinsSpawnAreaBotLeft, coinsSpawnAreaTopRight, null);
      //  Debug.Log((spawnTopRightCorner - topOffset) - (spawnBotLeftCorner + botOffset));
    }

    public Vector3? GetCoinPosition(GameObject coin)
    {
        if(coinsRoot == null)
            FirstCoint();

        combinedBounds = HelperMethods.GetCombinedBounds(coin);


        Vector3 center = combinedBounds.center;
        Vector3 extents = combinedBounds.extents;

        SetObjectBoundsPoints(center, extents, objectY);
    
        List<Partition> viable = new List<Partition>();
        coinsRoot.GetPartionsOfSize(combinedBounds.size, ref viable);

        if (viable.Count == 0)
        {
            Debug.LogError("no viable coin spot found");
            return null;
        }
        Vector3 pivotOffset = Vector3.zero;//center - (objBoundsBotLeftRot + objBoundsBotRightRot) / 2;//kadangi pivotas ne centre o apacioj

        int rngPart = Random.Range(0, viable.Count);

        objSpawnAreaTopRightCorner = new Vector3(
            viable[rngPart]._topRight.x - extents.x - pivotOffset.x,
            objectY,
            viable[rngPart]._topRight.z - extents.z - pivotOffset.z
        );

        objSpawnAreaBotLeftCorner = new Vector3(
            viable[rngPart]._botLeft.x + extents.x - pivotOffset.x,
            objectY,
            viable[rngPart]._botLeft.z + extents.z - pivotOffset.z
        );

        float x = Random.Range(objSpawnAreaBotLeftCorner.x, objSpawnAreaTopRightCorner.x);
        float z = Random.Range(objSpawnAreaBotLeftCorner.z, objSpawnAreaTopRightCorner.z);

        objBoundsTopRight = new Vector3(
            x + extents.x + pivotOffset.x,
            objectY,
            z + extents.z + pivotOffset.z
        );

        objBoundsBotLeft = new Vector3(
            x - extents.x + pivotOffset.x,
            objectY,
            z - extents.z + pivotOffset.z
        );

        viable[rngPart].PartitionThis(
            objBoundsBotLeft,
            objBoundsTopRight
        );

        Vector3 randPos = new Vector3(x, objectY, z);

        return randPos;

    }

    GameObject ResetPosition(GameObject toReset)
    {
        toReset.transform.position = Vector3.zero;
        return toReset;
    }
    public PositionAndRotation GetTreasureSpawnPosition(GameObject meshRoot,float yPos,Quaternion desiredRotation)
    {
        ResetPosition(meshRoot);

        meshRoot.transform.rotation = desiredRotation;

        objectY = yPos;
        if (treasureRoot == null)
            FirstObject();

        PositionAndRotation posAndRot = new PositionAndRotation();

        combinedBounds = HelperMethods.GetCombinedBounds(meshRoot);
        Vector3 center = combinedBounds.center;
        Vector3 extents = combinedBounds.extents;

        SetObjectBoundsPoints(center, extents, objectY);

        List<Partition> viable = new List<Partition>();

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

        int rngPart = Random.Range(0, viable.Count);

        objSpawnAreaTopRightCorner = new Vector3(
            viable[rngPart]._topRight.x - rotatedExtents.x - pivotOffset.x,
            objectY,
            viable[rngPart]._topRight.z - rotatedExtents.z - pivotOffset.z
        );

        objSpawnAreaBotLeftCorner = new Vector3(
            viable[rngPart]._botLeft.x + rotatedExtents.x - pivotOffset.x,
            objectY,
            viable[rngPart]._botLeft.z + rotatedExtents.z - pivotOffset.z
        );

        float x = Random.Range(objSpawnAreaBotLeftCorner.x, objSpawnAreaTopRightCorner.x);
        float z = Random.Range(objSpawnAreaBotLeftCorner.z, objSpawnAreaTopRightCorner.z);

        objBoundsTopRight = new Vector3(
            x + rotatedExtents.x + pivotOffset.x,
            objectY, 
            z + rotatedExtents.z + pivotOffset.z
            );

        objBoundsBotLeft = new Vector3(
            x - rotatedExtents.x + pivotOffset.x,
            objectY,
            z - rotatedExtents.z + pivotOffset.z
            );

        viable[rngPart].PartitionThis(
            objBoundsBotLeft,
            objBoundsTopRight
        );


        Vector3 randPos = new Vector3(x, objectY, z);

        posAndRot.position = randPos;

        return posAndRot;
    }

    void SetSpawnArea()
    {
        spawnBounds = spawnBoundsObj.GetComponent<Renderer>().bounds;
        Vector3 center = spawnBounds.center;

        spawnTopRightCorner = new Vector3(center.x + spawnBounds.extents.x, center.y, center.z + spawnBounds.extents.z);
        spawnBotLeftCorner = new Vector3(center.x - spawnBounds.extents.x, center.y, center.z - spawnBounds.extents.z);
    }
    void SetObjectBoundsPoints(Vector3 center, Vector3 extents, float y)
    {
        objBoundsTopRight = new Vector3(center.x + extents.x, y, center.z + extents.z);
        objBoundsTopLeft = new Vector3(center.x - extents.x, y, center.z + extents.z);
        objBoundsBotRight = new Vector3(center.x + extents.x, y, center.z - extents.z);
        objBoundsBotLeft = new Vector3(center.x - extents.x, y, center.z - extents.z);
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
            VisualizePartitions(coinsRoot);

        }

        VisualizePartitions(treasureRoot);
        VisualizePartitions(coinsRoot);
        //   Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
        float size = 0.02f;

         Gizmos.DrawSphere(coinsSpawnAreaTopRight, size);
         Gizmos.DrawSphere(coinsSpawnAreaBotLeft, size);
         
        /*   Gizmos.DrawSphere(objBoundsTopRightRot, size);
           Gizmos.DrawSphere(objBoundsTopLeftRot, size);
           Gizmos.DrawSphere(objBoundsBotRightRot, size);
           Gizmos.DrawSphere(objBoundsBotLeftRot, size);*/
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

    private const float minPartitionEdgeLength = 0;//0.2f;
    public Partition(Vector3 botLeft, Vector3 topRight, Partition parent, bool isUsed = false)
    {
        _botLeft = botLeft;
        _topRight = topRight;
        this.isUsed = isUsed;

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

    bool DistTooSmall(Vector3 i, Vector3 j)
    {
        if (Vector3.Distance(i, j) <= minPartitionEdgeLength)
            return true;

        return false;
    }

    public void PartitionThis(Vector3 botLeft, Vector3 topRight)
    {
        partitions = new List<Partition>();
        float y = botLeft.y;

        bool[] skipIDs = new bool[9];

        Vector3 maintR = topRight;
        Vector3 mainbL = botLeft;

        Partition0(ref skipIDs, y, ref mainbL, botLeft);
        Partition2(ref skipIDs, y, ref mainbL, ref maintR, botLeft, topRight);
        Partition6(ref skipIDs, y, ref mainbL, ref maintR, botLeft, topRight);
        Partition8(ref skipIDs, y, ref maintR, topRight);

        Partition1(skipIDs, y, botLeft, topRight);
        Partition3(skipIDs, y, botLeft, topRight);
        Partition5(skipIDs, y, botLeft, topRight);
        Partition7(skipIDs, y, botLeft, topRight);

        CenterPartition(mainbL, maintR);
    }

    void Partition0(ref bool[] skipIDs, float y, ref Vector3 mainbL, Vector3 originalBotLeft)
    {
        Vector3 tR = originalBotLeft; //top right
        Vector3 bL = _botLeft;//bot left

        Vector3 tL = new Vector3(bL.x, y, tR.z);
        Vector3 bR = new Vector3(tR.x, y, bL.z);

        if (DistTooSmall(tR, tL) && DistTooSmall(bR, tR)) //visi edge per mazi, jungt 3 segmentus
        {
            mainbL = bL;
            skipIDs[0] = true;
            skipIDs[1] = true;
            skipIDs[3] = true;
        }
        else if (DistTooSmall(tR, tL)) //tip virsutinis edge
        {
            mainbL = tL;
            skipIDs[1] = true;
        }
        else if (DistTooSmall(bR, tR)) //tip soninis edge
        {
            mainbL = bR;
            skipIDs[3] = true;
        }
        if (!skipIDs[0])
            partitions.Add(new Partition(bL, tR, this)); //0
    }
    void Partition2(ref bool[] skipIDs, float y, ref Vector3 mainbL, ref Vector3 maintR, Vector3 originalBotLeft, Vector3 originalTopRight)
    {
        Vector3 tR = new Vector3(originalBotLeft.x, y, _topRight.z);
        Vector3 bL = new Vector3(_botLeft.x, y, originalTopRight.z);

        Vector3 tL = new Vector3(bL.x, y, tR.z);
        Vector3 bR = new Vector3(tR.x, y, bL.z);

        if (DistTooSmall(tR, tL) && DistTooSmall(bR, tR)) //visi edge per mazi, jungt 3 segmentus
        {
            if (skipIDs[0] != true) // jei  0 partition nepajungtas
                mainbL = new Vector3(_botLeft.x, y, originalBotLeft.z);

            if (skipIDs[8] != true) // jei  8 partition nepajungtas
                maintR = new Vector3(originalTopRight.x, y, _topRight.z);



            skipIDs[2] = true;
            skipIDs[1] = true;
            skipIDs[5] = true;
        }
        else if (DistTooSmall(tR, tL)) //tik apatinis edge
        {
            if (skipIDs[0] != true)
                mainbL = new Vector3(_botLeft.x, y, originalBotLeft.z);
            skipIDs[1] = true;
        }
        else if (DistTooSmall(bR, tR)) //tip soninis edge
        {
            if (skipIDs[8] != true)
                maintR = new Vector3(originalTopRight.x, y, _topRight.z);
            skipIDs[5] = true;
        }
        if (!skipIDs[2])
            partitions.Add(new Partition(bL, tR, this));//2
    }
    void Partition6(ref bool[] skipIDs, float y, ref Vector3 mainbL, ref Vector3 maintR, Vector3 originalBotLeft, Vector3 originalTopRight)
    {
        Vector3 tR = new Vector3(_topRight.x, y, originalBotLeft.z);
        Vector3 bL = new Vector3(originalTopRight.x, y, _botLeft.z);
        Vector3 tL = new Vector3(bL.x, y, tR.z);
        Vector3 bR = new Vector3(tR.x, y, bL.z);

        if (DistTooSmall(tR, tL) && DistTooSmall(bR, tR)) //visi edge per mazi, jungt 3 segmentus
        {
            if (skipIDs[0] != true) // jei jau 0 partition pajungtas
                mainbL = new Vector3(originalBotLeft.x, y, _botLeft.z);

            if (skipIDs[8] != true) // jei jau 8 partition pajungtas
                maintR = new Vector3(_topRight.x, y, originalTopRight.z);



            skipIDs[6] = true;
            skipIDs[3] = true;
            skipIDs[7] = true;
        }
        else if (DistTooSmall(tR, tL)) //tik virsutinis edge
        {
            if (skipIDs[8] != true) // jei jau 8 partition pajungtas
                maintR = new Vector3(_topRight.x, y, originalTopRight.z);
            skipIDs[7] = true;
        }
        else if (DistTooSmall(bR, tR)) //tip soninis edge
        {
            if (skipIDs[0] != true) // jei jau 0 partition pajungtas
                mainbL = new Vector3(originalBotLeft.x, y, _botLeft.z);
            skipIDs[3] = true;
        }

        if (!skipIDs[6])
            partitions.Add(new Partition(bL, tR, this));//2
    }
    void Partition8(ref bool[] skipIDs, float y, ref Vector3 maintR, Vector3 originalTopRight)
    {
        Vector3 tR = _topRight; //top right
        Vector3 bL = originalTopRight;//bot left

        Vector3 tL = new Vector3(bL.x, y, tR.z);
        Vector3 bR = new Vector3(tR.x, y, bL.z);

        if (DistTooSmall(tR, tL) && DistTooSmall(bR, tR)) //visi edge per mazi, jungt 3 segmentus
        {
            maintR = tR;
            skipIDs[8] = true;
            skipIDs[7] = true;
            skipIDs[5] = true;
        }
        else if (DistTooSmall(tR, tL)) //tik apatinis edge
        {
            maintR = bR;
            skipIDs[7] = true;
        }
        else if (DistTooSmall(bR, tR)) //tip apatinis edge
        {
            maintR = tL;
            skipIDs[5] = true;
        }
        if (!skipIDs[8])
            partitions.Add(new Partition(bL, tR, this)); //0
    }

    void Partition1(bool[] skipIDs, float y, Vector3 originalBotLeft, Vector3 originalTopRight)
    {
        Vector3 tR = new Vector3(originalBotLeft.x, y, originalTopRight.z);
        Vector3 bL = new Vector3(_botLeft.x, y, originalBotLeft.z);

        if (!skipIDs[1])
            partitions.Add(new Partition(bL, tR, this)); //1
    }
    void Partition3(bool[] skipIDs, float y, Vector3 originalBotLeft, Vector3 originalTopRight)
    {
        Vector3 tR = new Vector3(originalTopRight.x, y, originalBotLeft.z);
        Vector3 bL = new Vector3(originalBotLeft.x, y, _botLeft.z);
        if (!skipIDs[3])
            partitions.Add(new Partition(bL, tR, this));
    }
    void Partition5(bool[] skipIDs, float y, Vector3 originalBotLeft, Vector3 originalTopRight)
    {
        Vector3 tR = new Vector3(originalTopRight.x, y, _topRight.z);
        Vector3 bL = new Vector3(originalBotLeft.x, y, originalTopRight.z);
        if (!skipIDs[5])
            partitions.Add(new Partition(bL, tR, this));
    }
    void Partition7(bool[] skipIDs, float y, Vector3 originalBotLeft, Vector3 originalTopRight)
    {
        Vector3 tR = new Vector3(_topRight.x, y, originalTopRight.z);
        Vector3 bL = new Vector3(originalTopRight.x, y, originalBotLeft.z);
        if (!skipIDs[7])
            partitions.Add(new Partition(bL, tR, this));
    }

    void CenterPartition(Vector3 mainbL, Vector3 maintR)
    {
        partitions.Add(new Partition(mainbL, maintR, this, true));
    }
}
