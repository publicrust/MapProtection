﻿using ProtoBuf;

[ProtoContract]
public class MapData
{
    [ProtoMember(1)] public string name;
    [ProtoMember(2)] public byte[] data;
}