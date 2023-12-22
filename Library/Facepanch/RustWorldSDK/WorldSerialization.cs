using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using LZ4;
using System.Windows;

public class WorldSerialization
{
    public const uint CurrentVersion = 9;

    public uint Version { get; set; }

    public WorldData world = new WorldData();

    public WorldSerialization()
    {
        Version = CurrentVersion;
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

    public void Load(string fileName)
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
}