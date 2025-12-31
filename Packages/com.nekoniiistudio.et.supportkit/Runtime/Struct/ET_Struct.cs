using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Vector3Ro
{
    public int x;
    public int y;
    public int z;
}
[Serializable]
public struct Vector2Ro
{
    public int x;
    public int y;
}
[Serializable]
public struct SquareRange2DInt
{
    public int minX;//include
    public int maxX;//include
    public int minY;//include
    public int maxY;//include

    public SquareRange2DInt(int minX, int maxX, int minY, int maxY)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }

    public int XSize => maxX - minX;
    public int YSize => maxY - minY;
}
[Serializable]
public struct SquareRange2D
{
    public float minX;
    public float minY;
    public float maxX;
    public float maxY;

    public SquareRange2D(float minX, float minY, float maxX, float maxY)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }
}
[Serializable]
public struct D_line
{
    public Vector2 start;
    public Vector2 end;
    public D_line (Vector2 _start, Vector2 _end)
    {
        start = _start;
        end = _end;
    }
}
[Serializable]
public struct IDfloat
{
    public string ID;
    public float value;

    public IDfloat(string iD, float value)
    {
        ID = iD;
        this.value = value;
    }
}
[Serializable]
public struct IDint
{
    public string ID;
    public int value;

    public IDint(string iD, int value)
    {
        ID = iD;
        this.value = value;
    }
}
[Serializable]
public struct IDtype<T>
{
    public string ID;
    public T value;

    public IDtype(string ID, T value)
    {
        this.ID = ID;
        this.value = value;
    }
}

