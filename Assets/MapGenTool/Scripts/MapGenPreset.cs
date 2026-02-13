using UnityEngine;

[System.Serializable]
public class MapGenPreset
{
    
    public enum AdjacentType {four_directions, eight_directions};
    [Header("Basic")]
    [Tooltip("The general size of the map")]
    [Range(10, 1000)] public int map_size = 40;
    [Tooltip("Increases distance between POIs and sets multiplier for map size")]
    [Range(1f, 20f)] public int map_scale = 1;
    [Tooltip("Set the visual noisiness of the terrain")]
    [Range(1f, 1000f)]public float perlin_scale = 10;

    public const float VarianceCap = 4f;
    [Tooltip("Affects how spreadout or lanky the resulting map's shape is")]
    [Range(0f, VarianceCap)] public float variance_scale = 1;
    [Tooltip("The size of the individual chunks which make up the entire map. The smallest unit of measurement for what is produced by the map")]
    [Range(2f, 32f)] public int chunk_size = 8; // size of each "chunk spawned by the generator
    [Tooltip("The width of the map border")]
    [Range(1, 4)] public int border_width = 4; // each chunk is 12z minimap tiles


    [Header("Map Objects")]
    [Tooltip("The total amount of main objectives")]
    [Range(1, 24)] public int objectives = 3;
    [Tooltip("The maximum possible amount of side objectives")]
    [Range(1, 24)] public int minor_poi_per_objective = 7; // extra goodies that might appear on the map

    public void OnGenValidate()
    {

    }
}