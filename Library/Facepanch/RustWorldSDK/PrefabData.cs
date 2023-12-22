using ProtoBuf;

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