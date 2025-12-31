using UnityEngine;
public class VirtualPoint : EPositionItem
{
    public int iD;
    public Vector3 position;
    public Vector3 Position => position;

    public VirtualPoint(int iD, Vector3 position)
    {
        this.iD = iD;
        this.position = position;
    }
}