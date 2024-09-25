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

    public Stream SaveToStream()
    {
        var memoryStream = new MemoryStream();

        var binaryWriter = new BinaryWriter(memoryStream);
        binaryWriter.Write(Version);

        var compressionStream = new LZ4Stream(memoryStream, LZ4StreamMode.Compress);
        Serializer.Serialize(compressionStream, world);

        // Important: Do not dispose the compressionStream or binaryWriter here
        compressionStream.Flush(); // Ensure everything is written
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream; // Return the MemoryStream which is still open
    }

    public void Load(string fileName)
    {
        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Load(fileStream);
        }
    }

    public void Load(Stream stream)
    {
        using (var binaryReader = new BinaryReader(stream))
        {
            Version = binaryReader.ReadUInt32();
            // if (Version != CurrentVersion)
            //   MessageBox.Show("Map Version is: " + Version + " whilst Rust is on: " + CurrentVersion);

            using (var compressionStream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
                world = Serializer.Deserialize<WorldData>(compressionStream);
        }
    }
}