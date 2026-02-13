using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class MajorObjective
{
    public MapChunk main_chunk;
    public MajorObjective prev = null;
    public MajorObjective next_poi = null;
    public Vector2Int[] minor_poi = new Vector2Int[0];
    public List<MapChunk> territory_chunks = new List<MapChunk>();
    public Vector2Int position => main_chunk.position;

    [Header("Objective Point")]
    public GameObject objective_point;

    public MajorObjective(Vector2Int position, MajorObjective prev = null, bool is_captured = false)
    {
        main_chunk = new MapChunk(position);
        SetupPOI(prev, is_captured);
    }

    public MajorObjective(MapChunk main_chunk, MajorObjective prev = null, bool is_captured = false)
    {
        this.main_chunk = main_chunk;
        SetupPOI(prev, is_captured);
    }

    private void SetupPOI(MajorObjective prev, bool is_captured)
    {
        if (prev != null)
        {
            this.prev = prev;
        } else
        {
            this.prev = this;
        }

        // GameObject objective_resource = Resources.Load<GameObject>("MapObjects/Objective");
        // objective_point = MonoBehaviour.Instantiate(
        //     objective_resource, 
        //     main_chunk.center_position * MapGenScript.chunk_size, 
        //     Quaternion.identity);
    }

    public void SetNextPOI(MajorObjective next)
    {
        this.next_poi = next;
    }

    public void SetMinorPOI(Vector2Int[] minor_poi_arr)
    {
        minor_poi = minor_poi_arr;
    }

    public void GenerateMinorPOI(int amount, float size)
    {
        if (amount <= 0)
        {
            return;
        }
        
        if (!(next_poi == null) && !(prev == null))
        {
            Vector2 splinter_dir = ((Vector2)prev.main_chunk.position + next_poi.main_chunk.position - 2*main_chunk.position).normalized;
            minor_poi = new Vector2Int[amount];
            float poi_partition = 1/amount;
            for (int i = 0; i < amount; i++)
            {
                splinter_dir *= -1;
                Vector2 new_pos = main_chunk.position + splinter_dir * (size * 0.5f + Random.Range(size * poi_partition, size * 0.5f));
                float lat_scale = size;
                new_pos += new Vector2(splinter_dir.y, -splinter_dir.x).normalized * Random.Range(-lat_scale, lat_scale);
                minor_poi[i] = new Vector2Int((int)new_pos.x, (int)new_pos.y);
            }
        }
    }
}