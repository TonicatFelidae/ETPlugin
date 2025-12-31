using UnityEngine;

public class VirtualLine
{
    public int iD;
    public int startID;
    public int endID;
    public float length;
    public VirtualLine(int iD, int startID, int endID, float length)
    {
        this.iD = iD;
        this.startID = startID;
        this.endID = endID;
        this.length = length;
    }
}
