using UnityEngine;

[System.Serializable]
public class MapGenPreset
{
    
    public enum AdjacentType {four_directions, eight_directions};
    [Header("Basic")]
    [Range(10, 1000)] public int map_size = 40;
    [Range(1f, 20f)] public int map_scale = 1;

    public const float VarianceCap = 4f;
    [Range(0f, VarianceCap)] public float variance_scale = 1;
    [Range(2f, 32f)] public int chunk_size = 8; // size of each "chunk spawned by the generator
    [Range(1, 4)] public int border_width = 4; // each chunk is 12z minimap tiles
    public bool four_adj_tiles {get; private set;} = true ;

    [Header("Map Objects")]
    [Range(1, 24)] public int objectives = 3;
    [Range(1, 24)] public int minor_poi_per_objective = 7; // extra goodies that might appear on the map

    public void OnGenValidate()
    {

    }
}