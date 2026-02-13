using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT
public class LevelMaker : MapMaker
{
    public override Vector2 GenerateMap( // return map center
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        HashSet<Vector2Int> border_chunks, 
        HashSet<Vector2Int> path_chunks, 
        MajorObjective[] critical_locs,
        MapGenPreset gen_preset)
    {
        #region GenMap - Spine
        Vector2Int start_pos = MapGenScript.START_POS;
        Vector2 unit_circ = Random.insideUnitCircle;
        Vector2 random_dir = new Vector2(unit_circ.x + Mathf.Sign(unit_circ.x) * Random.Range(1.25f, 1.75f), unit_circ.y*0.9f).normalized;

        float poi_partition = 1f/gen_preset.objectives;
        Vector2Int[] all_poi = new Vector2Int[critical_locs.Length];

        // initialize starting point
        all_poi[0] = start_pos;
        critical_locs[0] = new MajorObjective(all_poi[0], null, false);
        all_chunks[start_pos] = critical_locs[0].main_chunk;
        path_chunks.Add(start_pos);
        int total_minor_poi = 0;
        
        // create major poi at all points in all_poi except for the start
        for (int i = 1; i < all_poi.Length; i++) 
        {
            Vector2 new_pos = start_pos + random_dir * gen_preset.map_size * poi_partition * i;
            float lat_scale = gen_preset.map_size * poi_partition * gen_preset.variance_scale;
            new_pos += new Vector2(random_dir.y, -random_dir.x).normalized * Random.Range(-lat_scale, lat_scale);
            all_poi[i] = new Vector2Int((int)new_pos.x, (int)new_pos.y);
            critical_locs[i] = new MajorObjective(all_poi[i], critical_locs[i-1]);
            critical_locs[i-1].SetNextPOI(critical_locs[i]);
            
            int generate_poi = Random.Range(0, gen_preset.minor_poi_per_objective);
            critical_locs[i-1].GenerateMinorPOI(generate_poi, poi_partition * gen_preset.map_size * 0.5f);
            total_minor_poi += generate_poi;
        }

        // Draw a path between all poi
        for (int i = 1; i < all_poi.Length; i++)
        {
            int dx = all_poi[i].x - all_poi[i-1].x, dy = all_poi[i].y - all_poi[i-1].y;
            float diag_dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            for (float d = 1; d <= diag_dist; d++)
            {
                float t = d / (float)diag_dist;
                Vector2Int offset_vec = new Vector2Int(
                    (int)Mathf.Round(all_poi[i-1].x + dx * t), 
                    (int)Mathf.Round(all_poi[i-1].y + dy * t)
                    );
                all_chunks[offset_vec] = new MapChunk(offset_vec);
                path_chunks.Add(offset_vec);
            }

            foreach (Vector2Int m_poi in critical_locs[i].minor_poi)
            {
                int poi_dx = m_poi.x - all_poi[i].x, poi_dy = m_poi.y - all_poi[i].y;
                float poi_diag_dist = Mathf.Max(Mathf.Abs(poi_dx), Mathf.Abs(poi_dy));
                for (float d = 1; d <= poi_diag_dist; d++)
                {
                    float t = d / (float)poi_diag_dist;
                    Vector2Int offset_vec = new Vector2Int(
                        (int)Mathf.Round(all_poi[i].x + poi_dx * t), 
                        (int)Mathf.Round(all_poi[i].y + poi_dy * t)
                        );
                    all_chunks[offset_vec] = new MapChunk(offset_vec);
                }
            }
        }
        #endregion

        #region GenMap - Body
        // set up more chunks around poi path
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        List<Vector2Int> list_buffer = new List<Vector2Int>(); // use for branching
        
        // queue starting path chunks
        foreach(Vector2Int chunk in all_chunks.Keys)
        {
            foreach (Vector2Int dir in Directions2D.eight_directions)
            {
                Vector2Int chunk_dir = chunk + dir;
                if (!all_chunks.ContainsKey(chunk_dir) && !in_chunk_queue.Contains(chunk_dir))
                {
                    chunk_queue.Enqueue(chunk_dir);
                    in_chunk_queue.Add(chunk_dir);
                }
            }
        }
        // add adjacent chunks
        foreach(Vector2Int in_queue in in_chunk_queue)
        {
            foreach (Vector2Int adj in Directions2D.four_directions)
            {
                Vector2Int chunk_dir_adj = in_queue + adj;
                if (!all_chunks.ContainsKey(chunk_dir_adj) && !in_chunk_queue.Contains(chunk_dir_adj))
                {
                    border_chunks.Add(chunk_dir_adj);
                }
            }
        }

        // random propagation to adjacent chunks
        int chunks = gen_preset.map_size * gen_preset.objectives * gen_preset.map_scale + 
                    (int)(total_minor_poi * gen_preset.map_size * 0.5f * poi_partition);
        for (int i = 0; i < chunks && chunk_queue.Count > 0; i++) {
            // remove chunk from queue add chunk to all chunks
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Remove(curr_chunk);
            all_chunks[curr_chunk] = new MapChunk(curr_chunk);

            //queue a random adjacent chunk
            int randint = Random.Range(0, border_chunks.Count);
            Vector2Int new_chunk = border_chunks.ElementAt(randint);

            if ((i+1 + chunk_queue.Count) < chunks) // dont add anymore chunks if the queue is at the limit
            {
                if (in_chunk_queue.Add(new_chunk)) {
                    border_chunks.Remove(new_chunk);
                    chunk_queue.Enqueue(new_chunk);
                }
                foreach (Vector2Int adjacent in Directions2D.four_directions) // update adjacent chunks
                {
                    Vector2Int new_adj_chunk = new_chunk + adjacent;
                    if (!all_chunks.ContainsKey(new_adj_chunk) && !in_chunk_queue.Contains(new_adj_chunk))
                    {
                        // add to adjacent chunks list/dict
                        border_chunks.Add(new_adj_chunk);
                    }
                }
            }
        }

        // fill isolated border chunks
        list_buffer.Clear();  // use list buffer to destroy all  surrounded borders
        foreach(Vector2Int border in border_chunks)
        {
            bool adj_to_empty = false;
            foreach(Vector2Int dir in Directions2D.eight_directions)
            {
                // convert border chunks that aren't diagonally adjacent to a completely empty area into normal chunks
                Vector2Int border_dir = border + dir;
                if (!border_chunks.Contains(border_dir) && !all_chunks.ContainsKey(border_dir))
                {
                    adj_to_empty = true;
                    break;
                }
            }
            if (!adj_to_empty)
            {
                list_buffer.Add(border);
                all_chunks[border] = new MapChunk(border);
            }
        }
        foreach(Vector2Int border in list_buffer)
        {
            border_chunks.Remove(border);
        }
        #endregion

        #region GenMap - Chunks

        // assign relevancy values to chunks starting from
        chunk_queue.Clear();
        in_chunk_queue.Clear();
        chunk_queue.Enqueue(start_pos);

        MapChunk start_chunk = all_chunks[start_pos];
        start_chunk.dist_from_start = 0;
        start_chunk.path_relevancy = 0;

        for (int i = 0; i < all_chunks.Keys.Count; i++)
        {
            Vector2Int chunk_pos = chunk_queue.Dequeue();
            MapChunk chunk = all_chunks[chunk_pos];

            // get closest POI and be part of that poi's "territory"
            float lowest_mag = Mathf.Infinity;
            MajorObjective closest_poi = critical_locs[0];
            foreach(MajorObjective poi in critical_locs)
            {
                float vec_mag = (chunk_pos - poi.main_chunk.position).magnitude;
                if (vec_mag < lowest_mag)
                {
                    lowest_mag = vec_mag;
                    closest_poi = poi;
                }
            }
            chunk.domain_center_chunk = closest_poi.main_chunk.position;
            closest_poi.territory_chunks.Add(chunk);

            // get chunk & path distance from the starting pos
            foreach (Vector2Int dir in Directions2D.eight_directions)
            {
                Vector2Int next_chunk_pos = chunk_pos + dir;
                if (all_chunks.ContainsKey(next_chunk_pos))
                {
                    MapChunk next_chunk = all_chunks[next_chunk_pos];
                    // set surrounding dist values. If it exists, see if you can make a lower dist value
                    int dist_value = 2 + (dir.x == 0 || dir.y == 0 ? 0 : 1);
                    int dist_start_value = chunk.dist_from_start + dist_value;
                    if (all_chunks[next_chunk_pos].dist_from_start == -1)
                    {
                        chunk_queue.Enqueue(next_chunk_pos);
                        next_chunk.dist_from_start = dist_start_value;
                    } 
                    else if (all_chunks[next_chunk_pos].dist_from_start > dist_start_value)
                    {
                        next_chunk.dist_from_start = dist_start_value;
                    }

                    // set path relevancy. Path chunks automatically have path relevancy 0
                    int dist_path_value = chunk.path_relevancy + dist_value;
                    if (next_chunk.path_relevancy == -1)
                    {
                        if (path_chunks.Contains(next_chunk_pos))
                        {
                            next_chunk.path_relevancy = 0;
                        }
                        else
                        {
                            next_chunk.path_relevancy = dist_path_value;
                        }
                    }
                    else if (next_chunk.path_relevancy > dist_path_value)
                    {
                        next_chunk.path_relevancy = dist_path_value;
                        foreach (Vector2Int path_dir in Directions2D.eight_directions)
                        {
                            int re_dist_value = 2 + (dir.x == 0 || dir.y == 0 ? 0 : 1);
                            int re_dist_path_value = next_chunk.path_relevancy + re_dist_value;
                            Vector2Int re_path_dir = next_chunk_pos + dir;
                            if (all_chunks.ContainsKey(re_path_dir))
                            {
                                MapChunk re_next_chunk = all_chunks[re_path_dir];
                                if (re_next_chunk.path_relevancy > re_dist_path_value)
                                {
                                    re_next_chunk.path_relevancy = re_dist_path_value;
                                }
                            }
                        }
                    }

                }
            }
        }

        // get neighbors
        foreach (Vector2Int curr_chunk in all_chunks.Keys)
        {
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

        return Vector2.zero;
        #endregion
    }

    public override void GeneratePOI(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        MajorObjective[] critical_locs,
        MapGenPreset gen_preset)
    {

    }
    
}