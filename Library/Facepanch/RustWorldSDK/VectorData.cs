﻿using ProtoBuf;
using UnityEngine;

[Serializable]
[ProtoContract]
public class VectorData
{
    [ProtoMember(1)] public float x;
    [ProtoMember(2)] public float y;
    [ProtoMember(3)] public float z;

    public VectorData()
    {
    }

    public VectorData(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static implicit operator VectorData(Vector3 v)
    {
        return new VectorData(v.x, v.y, v.z);
    }

    public static implicit operator VectorData(Quaternion q)
    {
        return q.eulerAngles;
    }

    public static implicit operator Vector3(VectorData v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static implicit operator Quaternion(VectorData v)
    {
        return Quaternion.Euler(v);
    }

}