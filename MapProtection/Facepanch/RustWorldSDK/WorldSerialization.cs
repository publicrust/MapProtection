using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using LZ4;
using System.Windows;
using UnityEngine;

public class WorldSerialization
{
    public const uint CurrentVersion = 9;

    public uint Version { get; set; }

    public WorldData world = new WorldData();

    public WorldSerialization()
    {
        Version = CurrentVersion;
    }

    [ProtoContract]
    public class WorldData
    {
        [ProtoMember(1)] public uint size = 4000;
        [ProtoMember(2)] public List<MapData> maps = new List<MapData>();
        [ProtoMember(3)] public List<PrefabData> prefabs = new List<PrefabData>();
        [ProtoMember(4)] public List<PathData> paths = new List<PathData>();
    }

    [ProtoContract]
    public class MapData
    {
        [ProtoMember(1)] public string name;
        [ProtoMember(2)] public byte[] data;
    }

    [ProtoContract]
    [Serializable]
    public class PathData
    {
        public PathData()
        {
        }

        public PathData(PathData pathData)
        {
            this.name = pathData.name;
            this.spline = pathData.spline;
            this.start = pathData.start;
            this.end = pathData.end;
            this.innerPadding = pathData.innerPadding;
            this.outerPadding = pathData.outerPadding;
            this.innerFade = pathData.innerFade;
            this.outerFade = pathData.outerFade;
            this.randomScale = pathData.randomScale;
            this.width = pathData.width;
            this.meshOffset = pathData.meshOffset;
            this.terrainOffset = pathData.terrainOffset;
            this.splat = pathData.splat;
            this.topology = pathData.topology;
            this.nodes = new List<VectorData>();
        }

        // Token: 0x0400000C RID: 12
        [ProtoMember(1)]
        public string name;

        // Token: 0x0400000D RID: 13
        [ProtoMember(2)]
        public bool spline;

        // Token: 0x0400000E RID: 14
        [ProtoMember(3)]
        public bool start;

        // Token: 0x0400000F RID: 15
        [ProtoMember(4)]
        public bool end;

        // Token: 0x04000010 RID: 16
        [ProtoMember(5)]
        public float width;

        // Token: 0x04000011 RID: 17
        [ProtoMember(6)]
        public float innerPadding;

        // Token: 0x04000012 RID: 18
        [ProtoMember(7)]
        public float outerPadding;

        // Token: 0x04000013 RID: 19
        [ProtoMember(8)]
        public float innerFade;

        // Token: 0x04000014 RID: 20
        [ProtoMember(9)]
        public float outerFade;

        // Token: 0x04000015 RID: 21
        [ProtoMember(10)]
        public float randomScale;

        // Token: 0x04000016 RID: 22
        [ProtoMember(11)]
        public float meshOffset;

        // Token: 0x04000017 RID: 23
        [ProtoMember(12)]
        public float terrainOffset;

        // Token: 0x04000018 RID: 24
        [ProtoMember(13)]
        public int splat;

        // Token: 0x04000019 RID: 25
        [ProtoMember(14)]
        public int topology;

        // Token: 0x0400001A RID: 26
        [ProtoMember(15)]
        public List<VectorData> nodes;

        // Token: 0x0400001B RID: 27
        [ProtoMember(16)]
        public int hierarchy;
    }


    [Serializable]
    [ProtoContract]
    public class PrefabData
    {
        [ProtoMember(1)] public string category;
        [ProtoMember(2)] public uint id;
        [ProtoMember(3)] public VectorData position;
        [ProtoMember(4)] public VectorData rotation;
        [ProtoMember(5)] public VectorData scale;


        public PrefabData() { }
        public PrefabData(string category, uint id, VectorData position, VectorData rotation, VectorData scale)
        {
            this.category = category;
            this.id = id;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    //[Serializable]
    //[ProtoContract]
    //public class PathData
    //{
    //    [ProtoMember(1)] public string name;
    //    [ProtoMember(2)] public bool spline;
    //    [ProtoMember(3)] public bool start;
    //    [ProtoMember(4)] public bool end;
    //    [ProtoMember(5)] public float width;
    //    [ProtoMember(6)] public float innerPadding;
    //    [ProtoMember(7)] public float outerPadding;
    //    [ProtoMember(8)] public float innerFade;
    //    [ProtoMember(9)] public float outerFade;
    //    [ProtoMember(10)] public float randomScale;
    //    [ProtoMember(11)] public float meshOffset;
    //    [ProtoMember(12)] public float terrainOffset;
    //    [ProtoMember(13)] public int splat;
    //    [ProtoMember(14)] public int topology;
    //    [ProtoMember(15)] public VectorData[] nodes;
    //}

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

    public MapData GetMap(string name)
    {
        for (int i = 0; i < world.maps.Count; i++)
            if (world.maps[i].name == name) return world.maps[i];
        return null;
    }

    public void AddMap(string name, byte[] data)
    {
        var map = new MapData();

        map.name = name;
        map.data = data;

        world.maps.Add(map);
    }

    public void Save(string fileName)
    {
        try
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(Version);

                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Compress))
                        Serializer.Serialize(compressionStream, world);
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    public void Load(string fileName)
    {
        try
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    Version = binaryReader.ReadUInt32();
                   // if (Version != CurrentVersion)
                   //   MessageBox.Show("Map Version is: " + Version + " whilst Rust is on: " + CurrentVersion);

                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Decompress))
                        world = Serializer.Deserialize<WorldData>(compressionStream);
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}