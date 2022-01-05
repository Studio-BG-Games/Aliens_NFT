using MapCraete;

public struct LobbyInformationCase
{
    public string PlayerUrl { get; }
    public MapData MapData { get; }

    public LobbyInformationCase(string playerUrl, MapData mapData)
    {
        PlayerUrl = playerUrl;
        MapData = mapData;
    }
}
