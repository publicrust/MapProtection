﻿using ProtoBuf;

[ProtoContract]
public class WorldData
{
    [ProtoMember(1)] public uint size = 4000;
    [ProtoMember(2)] public List<MapData> maps = new List<MapData>();
    [ProtoMember(3)] public List<PrefabData> prefabs = new List<PrefabData>();
    [ProtoMember(4)] public List<PathData> paths = new List<PathData>();
}