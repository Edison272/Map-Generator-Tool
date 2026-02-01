using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Tilemaps;

using Random = UnityEngine.Random;

public class MapGenScript : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] Tilemap Ground;
    [SerializeField] Tilemap Wall;
    [Header("Tiles")]
    [SerializeField] TileBase ground_tile;
    [SerializeField] TileBase wall_tile;
    [Header("Generation Presets")]
    public MapGenPreset gen_preset;
    public static readonly Vector2Int START_POS = Vector2Int.zero;
    // each chunk expands to the TOP RIGHT when it becomes a 2D area instead of a single point
    public Dictionary<Vector2Int, MapChunk> all_chunks = new Dictionary<Vector2Int, MapChunk>();
    private Dictionary<Vector2Int, Vector2Int> adj_chunks = new Dictionary<Vector2Int, Vector2Int>();

    // important chunks/positions
    public Vector2Int spawn_chunk {get; private set;} // where the player spawns in
    public Vector2Int final_chunk {get; private set;} // chunk with the main objective
    public Vector2 map_center {get; private set;} // duh

    public MapChunk[] critical_locs {get; private set;} // start & final + POI

    // format is chunk, chunk + 1,0 , chunk + 1,1 , chunk + 0,1
    public HashSet<MapQuad> quads {get; private set;} = new HashSet<MapQuad>();

    HashSet<Vector3Int> draw_ground = new HashSet<Vector3Int>();
    HashSet<Vector3Int> draw_border = new HashSet<Vector3Int>();

    [Header("Gizmo Stuff")]
    [SerializeField] bool show_chunks = true;
    [SerializeField] bool show_adj_chunks = true;
    [SerializeField] bool show_critical_chunks = true;
    [SerializeField] bool show_quads = true;

    public void GenerateMap()
    {
        critical_locs = new MapChunk[2 + gen_preset.objectives];
        GenerateChunks();
        GeneratePOI();
        DrawMap();
    }

    void OnDrawGizmosSelected()
    {
        if (show_chunks)
        {
            foreach (Vector2Int chunk in all_chunks.Keys)
            {
                DrawChunk(chunk, Color.white);
            }
        }
        if (show_adj_chunks)
        {
            foreach (Vector2Int chunk in adj_chunks.Keys)
            {
                DrawChunk(chunk, Color.grey);
            }
        }
        if (show_critical_chunks)
        {
            //DrawChunk(START_POS, Color.black);
            if (all_chunks.Keys.Count > 0) // only perform if something actually geenrated
            {
                DrawChunk(final_chunk, Color.red);
                DrawChunk(spawn_chunk, Color.green);
                foreach (MapChunk chunk in critical_locs)
                {
                    if (chunk is MapQuad)
                    {
                        MapQuad quad = (MapQuad)chunk;
                        DrawQuad(quad.four_corners, Color.yellow);
                        DrawStar(quad.position, Color.yellow);
                    }

                    foreach(MapChunk other_chunk in critical_locs)
                    {
                        Debug.DrawLine((Vector2)chunk.position * gen_preset.chunk_size, (Vector2)other_chunk.position * gen_preset.chunk_size);
                    }
                }
            }
        }
        if (show_quads) {
            foreach (MapQuad quad in quads)
            {
                DrawQuad(quad.four_corners, Color.cyan);
            }
        }
    }

    #region Generate Chunks
    private void GenerateChunks() // generate the chunks and declare the start & final pos
    {
        // initialize data holders
        all_chunks.Clear();
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        adj_chunks.Clear();
        List<Vector2Int> list_buffer = new List<Vector2Int>(); // use for branching
        Vector2Int[] adjacent_array = gen_preset.four_adj_tiles ? Directions2D.four_directions : Directions2D.eight_directions;
        Directions2D.DirArray dir_array_type = gen_preset.four_adj_tiles ? Directions2D.DirArray.HORZ_WEIGHT_FOUR : Directions2D.DirArray.HORZ_WEIGHT_EIGHT;
        
        // get the first chunk
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int> {};
        chunk_queue.Enqueue(START_POS);
        in_chunk_queue.Add(START_POS);
        foreach (Vector2Int adjacent in adjacent_array) // update adjacent chunks
        {
            Vector2Int new_chunk = START_POS + adjacent;
            adj_chunks[new_chunk] = new_chunk;
        }

        // values for final pos and spawn pos
        final_chunk = START_POS;
        for (int i = 0; i < gen_preset.map_chunks && chunk_queue.Count > 0; i++) {
            // add chunk to the hashset
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Remove(curr_chunk);
            all_chunks[curr_chunk] = new MapChunk(curr_chunk);

            // final pos is furthest from the start position
            if (curr_chunk.sqrMagnitude > final_chunk.sqrMagnitude)
            {
                final_chunk = curr_chunk;
            } 

            // see where the current chunk can branch to
            Directions2D.ValidPositionsFromPoint(list_buffer, dir_array_type, curr_chunk, all_chunks, in_chunk_queue);
            // preform semi-random branch
            int branches = Random.Range(gen_preset.min_chunk_branching, gen_preset.max_chunk_branching + 1);
            for (int b = 0; b < branches; b++)
            {
                Vector2Int new_chunk = curr_chunk;
                int randint = 0;
                if (list_buffer.Count > 0) // prevent overlapping with preexisting chunks
                {
                    randint = Random.Range(0, list_buffer.Count);
                    new_chunk = list_buffer[randint];
                    list_buffer.RemoveAt(randint);
                } 
                else
                {
                    // if the algo still needs to branch but all adjacent spots are taken, just queue a random adjacent chunk
                    randint = Random.Range(0, adj_chunks.Keys.Count);
                    new_chunk = adj_chunks.Keys.ElementAt(randint);
                }
                if ((i+1 + chunk_queue.Count) < gen_preset.map_chunks) // make sure the chunks in queue dont go outta control
                {
                    if (in_chunk_queue.Add(new_chunk)) {
                        adj_chunks.Remove(new_chunk);
                        chunk_queue.Enqueue(new_chunk);
                    }

                    foreach (Vector2Int adjacent in adjacent_array) // update adjacent chunks
                    {
                        Vector2Int new_adj_chunk = new_chunk + adjacent;
                        if (!all_chunks.ContainsKey(new_adj_chunk) && !in_chunk_queue.Contains(new_adj_chunk))
                        {
                            // add to adjacent chunks list/dict
                            adj_chunks[new_adj_chunk] = new_adj_chunk;
                        }
                    }
                }
            }
        }
        // get spawn position based on tile step distance
        // set neighbors of each chunk
        chunk_queue.Clear();
        chunk_queue.Enqueue(final_chunk);
        all_chunks[final_chunk].dist_from_final = 0;
        for (int i = 0; i < all_chunks.Count; i ++)
        {
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            list_buffer.Clear();
            foreach (Vector2Int dir in Directions2D.eight_directions) // establish all chunk's dist from the final
            {
                Vector2Int next_chunk = curr_chunk + dir;
                if (all_chunks.ContainsKey(next_chunk) && all_chunks[next_chunk].dist_from_final == -1)
                {
                    all_chunks[next_chunk].dist_from_final = all_chunks[curr_chunk].dist_from_final + 1;
                    chunk_queue.Enqueue(next_chunk);
                }

                // find neighbors
                if (all_chunks.ContainsKey(next_chunk))
                {
                    list_buffer.Add(dir);
                }
            }
            // set neighbors
            all_chunks[curr_chunk].SetNeighbors(list_buffer);

            if (chunk_queue.Count == 0)
            {
                break;
            }
        }

        // find spawn chunk, map center, draw tiles, and chunk connections
        spawn_chunk = final_chunk;
        map_center = Vector2.zero;
        foreach(Vector2Int pos in all_chunks.Keys)
        {
            map_center += pos;
            if (all_chunks[pos].dist_from_final == all_chunks[spawn_chunk].dist_from_final)
            {
                if ((final_chunk - pos).sqrMagnitude > (final_chunk - spawn_chunk).sqrMagnitude)
                spawn_chunk = pos;
            } 
            else if (all_chunks[pos].dist_from_final > all_chunks[spawn_chunk].dist_from_final)
            {
                spawn_chunk = pos;
            }
        }
        map_center /= all_chunks.Keys.Count;

        // declare first and last pos of critical locs
        critical_locs[0] = all_chunks[spawn_chunk];
        critical_locs[1] = all_chunks[final_chunk];
    }
    #endregion

    #region Generate POI
    private void GeneratePOI() // generate potential POI based off of chunks in the map
    {
        quads.Clear();
        foreach (Vector2Int chunk in all_chunks.Keys)
        {
            // add all four quads to the hashset. it'll figure it out
            Vector2Int right = chunk + Directions2D.eight_directions[0];
            Vector2Int top_corner = chunk + Directions2D.eight_directions[1];
            Vector2Int up = chunk + Directions2D.eight_directions[2];
            if (all_chunks.Keys.Contains(right) && all_chunks.Keys.Contains(top_corner) && all_chunks.Keys.Contains(up)) 
            {
                
                quads.Add(new MapQuad(chunk, right, top_corner, up));
            }
        }

        for (int i = 2; i < critical_locs.Length; i++) // fill in betweens of the list
        {
            float highest_short = -Mathf.Infinity;
            foreach(MapQuad mq in quads)
            {   
                float shortest_dist = Mathf.Infinity;
                if (Array.IndexOf(critical_locs, mq) != -1)
                {
                    continue;
                }

                for (int l = 0; l < critical_locs.Length; l++)
                {
                    if (critical_locs[l] == null)
                    {
                        continue; // exit loop bc theres no more critical locs to compare to
                    }
                    float dist = (mq.position - critical_locs[l].position).sqrMagnitude;
                    if (dist < shortest_dist)
                    {
                        shortest_dist = dist;
                    }
                }

                if (shortest_dist > highest_short)
                {
                    critical_locs[i] = mq;
                    highest_short = shortest_dist;
                }
            }
        }

    }
#endregion

    private void Pathfind() // declare pathway chunks between all POI
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

        foreach(Vector3Int chunk in draw_ground)
        {
            Ground.SetTile(chunk, ground_tile);
        }
        foreach(Vector3Int chunk in draw_border)
        {
            if (!Ground.GetTile(chunk))
            {
                Wall.SetTile(chunk, wall_tile);
            }
            
        }
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

    #region Serialization
    public void OnValidate()
    {
        gen_preset.OnGenValidate();
    }
    #endregion
}
