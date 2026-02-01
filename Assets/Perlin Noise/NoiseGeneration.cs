using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class NoiseGenerator : MonoBehaviour
{
    public float map_scale = 10f;
    public int map_width;
    public int map_height;
    public Vector2Int origin = Vector2Int.zero;

    public GameObject tile_instance;
    public GameObject[,] tiles = new GameObject[0,0];
    
    // determine terrain color and spawn
    [SerializeField] private SerializeTerrainDict[] terrain_types = new SerializeTerrainDict[0];
    public Dictionary<Color, TerrainSpawn[]> color_and_spawns;
    // terrain spawn class
    [System.Serializable] public class TerrainSpawn
    {
        [SerializeField] public GameObject spawn_object;
        [SerializeField] public int drop_chance = 1;
    }

    // dictionary serializer
    [System.Serializable] public class SerializeTerrainDict
    {
        [SerializeField] public Color color  = new Color(1,1,1,1);
        [SerializeField] public TerrainSpawn[] terrain_drops = new TerrainSpawn[0];
    }
    private Color[] color_depths;

    // temporary array buffer for spawning
    private List<GameObject> drop_table = new List<GameObject>();
    public void Start()
    {
        NewPerlin();
    }
    public void NewPerlin()
    {
        // initialize dictionary from interface serializer
        color_and_spawns = new Dictionary<Color, TerrainSpawn[]>{};
        foreach(SerializeTerrainDict terrain in terrain_types)
        {
            color_and_spawns.Add(terrain.color, terrain.terrain_drops);
        }
        
        // initialize color_depths based on color_and_spawns
        color_depths = new Color[color_and_spawns.Keys.Count];
        int i = 0;
        foreach(Color color in color_and_spawns.Keys)
        {
            color_depths[i] = color;
            i++;
        }
        
        // clear gameobject tiles that might've came before
        foreach(GameObject tile in tiles)
        {
            Destroy(tile);
        }
        
        tiles = new GameObject[map_width,map_height];
        Vector2 perlin_offset = new Vector2(Random.Range(100, -100), Random.Range(100, -100));
        for (int y = 0; y < map_height; y++)
        {
            for (int x = 0; x < map_width; x++)
            {
                float x_cord = (origin.x + x) / map_scale;
                float y_cord = (origin.y + y) / map_scale;
                float sample = Mathf.PerlinNoise(perlin_offset.x + x_cord, perlin_offset.y + y_cord);
                Vector2Int loc = new Vector2Int(x, y);
                GameObject new_tile = Instantiate(tile_instance, (Vector2)loc, Quaternion.identity);
                tiles[x,y] = new_tile;

                Color terrain_color = color_depths[(int)Mathf.Clamp(sample * color_depths.Length, 0, color_depths.Length-1)];

                new_tile.GetComponent<SpriteRenderer>().color = terrain_color;

                // spawn something on the terrain
                GameObject terrain_spawn = GetTerrainSpawn(color_and_spawns[terrain_color]);
                if (terrain_spawn)
                {
                    Instantiate(terrain_spawn, new_tile.transform);
                }


            }
        }

        Camera.main.transform.position = new Vector3(origin.x + map_width/2, origin.y + map_height/2, -10);
    }

    public GameObject GetTerrainSpawn(TerrainSpawn[] spawner)
    {
        drop_table.Clear();

        if(spawner.Length == 0)
        {
            return null;
        }

        foreach(TerrainSpawn spawn in spawner)
        {
            for (int i = 0; i < spawn.drop_chance; i++)
            {
                drop_table.Add(spawn.spawn_object);
            }
        }
        

        return drop_table[Random.Range(0, drop_table.Count)];
    }
}