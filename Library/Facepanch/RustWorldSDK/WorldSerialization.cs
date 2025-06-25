using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using LZ4;
using UnityEngine;

public class WorldSerialization
{
    public const uint PreviousVersion = 9u;
    public const uint CurrentVersion = 10u;

    public uint Version { get; private set; }
    public string Checksum { get; private set; }
    public long Timestamp { get; private set; }

    public WorldData world = new WorldData();

    public WorldSerialization()
    {
        Version = CurrentVersion;
        Checksum = null;
        Timestamp = 0L;
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
                    binaryWriter.Write(DateTimeOffset.Now.ToUnixTimeMilliseconds());

                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Compress))
                        Serializer.Serialize(compressionStream, world);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public Stream SaveToStream()
    {
        var memoryStream = new MemoryStream();
        var binaryWriter = new BinaryWriter(memoryStream);
        
        binaryWriter.Write(Version);
        binaryWriter.Write(DateTimeOffset.Now.ToUnixTimeMilliseconds());

        var compressionStream = new LZ4Stream(memoryStream, LZ4StreamMode.Compress);
        Serializer.Serialize(compressionStream, world);

        compressionStream.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    public void Load(string fileName)
    {
        try
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ReadWorldData(fileStream);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void Load(Stream stream)
    {
        ReadWorldData(stream);
    }

    private void ReadWorldData(Stream stream)
    {
        using (var binaryReader = new BinaryReader(stream))
        {
            Version = binaryReader.ReadUInt32();
            
            if (Version == PreviousVersion)
            {
                LoadWorldFromStream(stream);
                Version = CurrentVersion;
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            else if (Version == CurrentVersion)
            {
                Timestamp = binaryReader.ReadInt64();
                LoadWorldFromStream(stream);
            }
        }
    }

    private void LoadWorldFromStream(Stream stream)
    {
        using (var compressionStream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
            world = Serializer.Deserialize<WorldData>(compressionStream);
    }
}