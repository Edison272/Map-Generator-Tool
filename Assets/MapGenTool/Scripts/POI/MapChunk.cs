using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapChunk
{
    public Vector2Int position;
    public Vector2 world_position;
    public Vector2 center_position;
    public Vector2 world_center_position;

    public Vector2Int domain_center_chunk = Vector2Int.zero;
    public int dist_from_final = -1;
    public int dist_from_start = -1;
    public int path_relevancy = -1; // how far away the chunk is from the main path. 0 is on the path

    public Vector2Int[] neighbor_chunks {get; private set;} // get initialized in map manager
    private MapGenScript map_gen_script;

    public MapChunk(Vector2Int pos)
    {
        position = pos;
        center_position = new Vector2(pos.x + 0.5f, pos.y + 0.5f);
        world_center_position = pos * MapGenScript.chunk_size;
        
    }

    public void SetNeighbors(List<Vector2Int> directions)
    {
        neighbor_chunks = directions.ToArray();
    }
}
