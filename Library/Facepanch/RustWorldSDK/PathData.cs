using ProtoBuf;

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

    [ProtoMember(1)]
    public string name;

    [ProtoMember(2)]
    public bool spline;

    [ProtoMember(3)]
    public bool start;

    [ProtoMember(4)]
    public bool end;

    [ProtoMember(5)]
    public float width;

    [ProtoMember(6)]
    public float innerPadding;

    [ProtoMember(7)]
    public float outerPadding;

    [ProtoMember(8)]
    public float innerFade;

    [ProtoMember(9)]
    public float outerFade;

    [ProtoMember(10)]
    public float randomScale;

    [ProtoMember(11)]
    public float meshOffset;

    [ProtoMember(12)]
    public float terrainOffset;

    [ProtoMember(13)]
    public int splat;

    [ProtoMember(14)]
    public int topology;

    [ProtoMember(15)]
    public List<VectorData> nodes;

    [ProtoMember(16)]
    public int hierarchy;
}