using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapChunk
{
    public Vector2 position;
    public int dist_from_final = -1;

    public Vector2Int[] neighbor_chunks {get; private set;} // get initialized in map manager

    public MapChunk(Vector2Int pos)
    {
        position = pos;
    }

    public void SetNeighbors(List<Vector2Int> directions)
    {
        neighbor_chunks = directions.ToArray();
    }
}

public class MapQuad : MapChunk
{
    public (Vector2Int, Vector2Int, Vector2Int, Vector2Int) four_corners;

    public MapQuad(Vector2Int pos, Vector2Int pos2, Vector2Int pos3, Vector2Int pos4) : base(pos)
    {
        four_corners = (pos, pos2, pos3, pos4);
        position = (Vector2)(pos + pos2 + pos3 + pos4)/4;
        dist_from_final = 67;
    }
}