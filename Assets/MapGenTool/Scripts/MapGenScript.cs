using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;

using Random = UnityEngine.Random;

public class MapGenScript : MonoBehaviour
{
    [SerializeField] GameObject MapObject;
    [Header("Tilemaps")]
    [SerializeField] Tilemap Ground;
    [SerializeField] Tilemap Wall;
    [Header("Drawing Tiles")]
    [SerializeField] TileBase[] ground_tiles;
    [SerializeField] TileBase wall_tile;
    [SerializeField] TileBase path_tile;
    [Header("Generation Presets")]
    [SerializeField] MapMakerType map_maker_type;
    [SerializeField] MapMaker map_maker;
    public MapGenPreset gen_preset;
    public static int chunk_size;
    public static readonly Vector2Int START_POS = Vector2Int.zero;
    // each chunk expands to the TOP RIGHT when it becomes a 2D area instead of a single point
    public Dictionary<Vector2Int, MapChunk> all_chunks = new Dictionary<Vector2Int, MapChunk>();
    private HashSet<Vector2Int> border_chunks = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> path_chunks = new HashSet<Vector2Int>(); // chunks containing the paths between POI
    // important chunks/positions
    public Vector2Int spawn_chunk {get; private set;} // where the player spawns in
    public Vector2Int final_chunk {get; private set;} // chunk with the main objective
    public Vector2 map_center {get; private set;} // duh

    public MajorObjective[] critical_locs = new MajorObjective[0]; // start & final + POI

    [Header("Draw Map")]
    public float perlin_scale = 10;
    HashSet<Vector3Int> draw_ground = new HashSet<Vector3Int>();
    HashSet<Vector3Int> draw_path = new HashSet<Vector3Int>();
    HashSet<Vector3Int> draw_border = new HashSet<Vector3Int>();

    [Header("Objective Point")]
    public GameObject objective_point_prefab;

    [Header("Gizmo Stuff")]
    [SerializeField] bool show_chunks = true;
    [SerializeField] bool show_border_chunks = true;
    [SerializeField] bool show_critical_chunks = true;
    [SerializeField] bool show_minor_poi = true;
    [SerializeField] bool show_start_dist_heatmap = true;
    [SerializeField] bool show_path_dist_heatmap = true;
    [SerializeField] bool show_poi_territories = true;

    public void GenerateMap()
    {
        switch(map_maker_type)
        {
            case MapMakerType.Blob:
                map_maker = new BlobMaker();
                break;
            case MapMakerType.Level:
                map_maker = new LevelMaker();
                break;
        }
        GenerateChunks();
        GeneratePOI();
        GetPOIPaths();
        DrawMap();

        spawn_chunk = critical_locs[0].main_chunk.position;
        final_chunk = critical_locs[critical_locs.Length-1].main_chunk.position;
    }

#region Generate Chunks
    private void GenerateChunks() // generate the chunks and declare the start & final pos
    {
        // initialize data holders
        all_chunks.Clear();
        border_chunks.Clear();
        path_chunks.Clear();
        critical_locs = new MajorObjective[1 + gen_preset.objectives];

        map_center = map_maker.GenerateMap(all_chunks, border_chunks, path_chunks, critical_locs, gen_preset);
    }
#endregion

#region Generate POI
    private void GeneratePOI() // generate potential POI based off of chunks in the map
    {
        map_maker.GeneratePOI(all_chunks, critical_locs, gen_preset);
    }
    #endregion
    private void GetPOIPaths() // declare pathway chunks between all POI
    {

    }

#region Draw Map
    private void DrawMap() // put tiles on the map ts
    {
        Ground.ClearAllTiles();
        Wall.ClearAllTiles();

        draw_ground.Clear();
        draw_border.Clear();

        foreach(Vector2Int pos in all_chunks.Keys)
        {
            foreach(Vector2Int dir in all_chunks[pos].neighbor_chunks)
            {
                for (int i = 0; i <= gen_preset.chunk_size/2; i++)
                {
                    Vector2Int chunk_vec = pos * gen_preset.chunk_size + dir * i;
                    int size_iter = gen_preset.chunk_size/2 + gen_preset.border_width;
                    for (int x = -size_iter; x < size_iter; x++)
                    {
                        for (int y = -size_iter; y < size_iter; y++)
                        {
                            Vector2Int draw_offset = new Vector2Int(x, y);
                            if (Mathf.Abs(x) <= gen_preset.chunk_size/2 && Mathf.Abs(y) <= gen_preset.chunk_size/2)
                            {
                                draw_ground.Add((Vector3Int)(draw_offset + chunk_vec));
                            }
                            else
                            {
                                draw_border.Add((Vector3Int)(draw_offset + chunk_vec));
                            }
                            
                        }
                    }
                }
            }
        }

        foreach(Vector3Int chunk in draw_border)
        {
            if (!draw_ground.Contains(chunk))
            {
                Wall.SetTile(chunk, wall_tile);
            }
            
        }

        Vector2 perlin_offset = new Vector2(Random.Range(100, -100), Random.Range(100, -100));
        foreach(Vector3Int chunk in draw_ground)
        {
            float x_cord = chunk.x / perlin_scale + 0.0001f;
            float y_cord = chunk.y / perlin_scale + 0.0001f;
            float sample = Mathf.PerlinNoise(perlin_offset.x + x_cord, perlin_offset.y + y_cord);
            //Debug.Log((int)Mathf.Clamp(sample * ground_tiles.Length, 0, ground_tiles.Length-1));
            TileBase new_tile = ground_tiles[(int)Mathf.Clamp(sample * ground_tiles.Length, 0, ground_tiles.Length-1)];
            Ground.SetTile(chunk, new_tile);
        }
    }
#endregion

#region Save Map File
    public void SaveMapFile()
    {
        string filename = "MapAsset";
        string local_path = "Assets/MapGenTool/SavedMaps/" + filename + ".prefab";

        local_path = AssetDatabase.GenerateUniqueAssetPath(local_path);

        PrefabUtility.SaveAsPrefabAssetAndConnect(MapObject, local_path, InteractionMode.UserAction);
    }
#endregion
#region Tools 
    public Vector2 GetChunkWorldPos(Vector2Int chunk)
    {
        Vector2 pos = Vector2.zero;
        if (all_chunks.ContainsKey(chunk))
        {
            pos = all_chunks[chunk].position * gen_preset.chunk_size;
        }
        return pos;
    }
    private void DrawChunk(Vector2Int chunk_pos, Color line_color)
    {
        // Square Shape
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, 
            (chunk_pos + Vector2.up*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, 
            line_color 
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.down*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.down*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, 
            (chunk_pos + Vector2.up*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, 
            line_color
            );
        // Crosses
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, 
            line_color
            );
    }

    public void DrawQuad((Vector2Int, Vector2Int, Vector2Int, Vector2Int) quad, Color line_color)
    {
        // straights
        // Debug.DrawLine((Vector2)quad.Item1 * gen_preset.chunk_size, (Vector2)(quad.Item2 + Vector2Int.right) * gen_preset.chunk_size, line_color);
        // Debug.DrawLine((Vector2)(quad.Item2 + Vector2Int.right) * gen_preset.chunk_size, (Vector2)(quad.Item3 + Vector2Int.up + Vector2Int.right) * gen_preset.chunk_size, line_color);
        // Debug.DrawLine((Vector2)(quad.Item3 + Vector2Int.up + Vector2Int.right) * gen_preset.chunk_size, (Vector2)(quad.Item4 + Vector2Int.up) * gen_preset.chunk_size, line_color);
        // Debug.DrawLine((Vector2)(quad.Item4 + Vector2Int.up) * gen_preset.chunk_size, (Vector2)quad.Item1 * gen_preset.chunk_size, line_color);

        Debug.DrawLine((Vector2)quad.Item1 * gen_preset.chunk_size, (Vector2)(quad.Item2) * gen_preset.chunk_size, line_color);
        Debug.DrawLine((Vector2)(quad.Item2) * gen_preset.chunk_size, (Vector2)(quad.Item3) * gen_preset.chunk_size, line_color);
        Debug.DrawLine((Vector2)(quad.Item3) * gen_preset.chunk_size, (Vector2)(quad.Item4) * gen_preset.chunk_size, line_color);
        Debug.DrawLine((Vector2)(quad.Item4) * gen_preset.chunk_size, (Vector2)quad.Item1 * gen_preset.chunk_size, line_color);

        Debug.DrawLine((quad.Item1 + Vector2.down*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, (quad.Item2 + Vector2.down*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, line_color);
        Debug.DrawLine((quad.Item2 + Vector2.down*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, (quad.Item3 + Vector2.up*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, line_color);
        Debug.DrawLine((quad.Item3 + Vector2.up*0.5f + Vector2.right*0.5f) * gen_preset.chunk_size, (quad.Item4 + Vector2.up*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, line_color);
        Debug.DrawLine((quad.Item4 + Vector2.up*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, (quad.Item1 + Vector2.down*0.5f + Vector2.left*0.5f) * gen_preset.chunk_size, line_color);
    }

    public void DrawStar(Vector2 point, Color line_color)
    {
        foreach (Vector2Int dir in Directions2D.eight_directions)
        {
            Debug.DrawLine(point * gen_preset.chunk_size, point * gen_preset.chunk_size + dir * gen_preset.chunk_size/4, line_color);
        }
    }

    #endregion
    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (show_chunks)
        {
            foreach (Vector2Int chunk in all_chunks.Keys)
            {
                DrawChunk(chunk, Color.white);
            }
        }
        if (show_border_chunks)
        {
            foreach (Vector2Int chunk in border_chunks)
            {
                DrawChunk(chunk, Color.grey);
            }
        }
        if (show_critical_chunks)
        {
            DrawChunk(START_POS, Color.black);
            DrawChunk(final_chunk, Color.red);
            DrawChunk(spawn_chunk, Color.green);
            for (int i = 0; i < critical_locs.Length; i++)
            {
                MapChunk chunk = critical_locs[i].main_chunk;
                if (chunk.position != spawn_chunk && chunk.position != final_chunk)
                {
                    DrawChunk(chunk.position, Color.yellow);
                    if (show_minor_poi)
                    {
                        foreach(Vector2Int minor_poi in critical_locs[i].minor_poi)
                        {
                            Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)minor_poi * chunk_size);
                            DrawChunk(minor_poi, Color.cyan);
                        }
                    }
                }
                if (i < critical_locs.Length-1)
                {
                    Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)critical_locs[i+1].main_chunk.position * chunk_size);
                }
                // foreach(MapChunk other_chunk in critical_locs)
                // {
                //     Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)other_chunk.position * chunk_size);
                // }
            }
        }
    }
    #endregion
    #region Serialization
    public void OnValidate()
    {
        gen_preset.OnGenValidate();
        chunk_size = gen_preset.chunk_size;
    }
    #endregion
}
