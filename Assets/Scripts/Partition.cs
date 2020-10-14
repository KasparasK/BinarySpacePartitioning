
using System.Collections.Generic;
using UnityEngine;

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
        Side1(botLeft, topRight, y);
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

    void Side2(Vector3 objTopRight, float y)
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
