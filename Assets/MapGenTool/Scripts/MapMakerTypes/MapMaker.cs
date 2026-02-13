using System.Collections.Generic;
using UnityEngine;
enum MapMakerType {Blob, Level};
public abstract class MapMaker
{
        public abstract Vector2 GenerateMap(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        HashSet<Vector2Int> border_chunks, 
        HashSet<Vector2Int> path_chunks, 
        MajorObjective[] critical_locs,
        MapGenPreset gen_preset);

    public abstract void GeneratePOI(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        MajorObjective[] critical_locs
    );
}