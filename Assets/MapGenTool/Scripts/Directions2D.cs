using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public static class Directions2D 
{
    public enum DirArray {FOUR, EIGHT, HORZ_WEIGHT_FOUR, HORZ_WEIGHT_EIGHT};
    public static readonly Vector2Int[] four_directions = {
        new Vector2Int(1, 0), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(0, -1)
    };

    public static readonly Vector2Int[] eight_directions = {
        new Vector2Int(1, 0), 
        new Vector2Int(1, 1), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, -1), 
        new Vector2Int(0, -1),
        new Vector2Int(1, -1), 
    };
    public static readonly Vector2Int[] horz_weight_four_dir = {
        new Vector2Int(1, 0), 
        new Vector2Int(1, 0), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, 0), 
        new Vector2Int(0, -1)
    };
    public static readonly Vector2Int[] horz_weight_eight_dir = { // heavily favor horizontal movement, then favor vertical movement, and lastly diagonal movement
        new Vector2Int(1, 0), 
        new Vector2Int(1, 0), 
        new Vector2Int(1, 0), 
        new Vector2Int(1, 1), 
        new Vector2Int(0, 1), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, -1), 
        new Vector2Int(0, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1), 
    };

    public static Vector2Int GetRandomDirection(DirArray direction_array)
    {
        Vector2Int direction = Vector2Int.zero;
        switch(direction_array)
        {
            case DirArray.FOUR:
                direction = four_directions[Random.Range(0, four_directions.Length)];
                break;
            case DirArray.EIGHT:
                direction = eight_directions[Random.Range(0, eight_directions.Length)];
                break;
            case DirArray.HORZ_WEIGHT_FOUR:
                direction = horz_weight_four_dir[Random.Range(0, horz_weight_four_dir.Length)];
                break;
            case DirArray.HORZ_WEIGHT_EIGHT:
                direction = horz_weight_eight_dir[Random.Range(0, horz_weight_eight_dir.Length)];
                break;
        }
        return direction;
    }


    public static void DirectionsFromPoint(List<Vector2Int> list_pointer, DirArray direction_array, bool randomize = false)
    {
        Vector2Int[] dir_pointer = four_directions;
        switch(direction_array)
        {
            case DirArray.FOUR:
                dir_pointer = four_directions;
                break;
            case DirArray.EIGHT:
                dir_pointer = eight_directions;
                break;
            case DirArray.HORZ_WEIGHT_FOUR:
                dir_pointer = horz_weight_four_dir;
                break;
            case DirArray.HORZ_WEIGHT_EIGHT:
                dir_pointer = horz_weight_eight_dir;
                break;
        }

        list_pointer.Clear();
        foreach(Vector2Int dir in dir_pointer)
        {
            list_pointer.Add(dir);
        }

        if (randomize) // switch around the list values to randomize it
        {
            for (int i = 0; i < list_pointer.Count; i++)
            {
                Vector2Int temp = list_pointer[i];
                int randint = Random.Range(0, list_pointer.Count);
                list_pointer[i] = list_pointer[randint];
                list_pointer[randint] = temp;
            }
        }
    }

    public static void ValidPositionsFromPoint(List<Vector2Int> list_pointer, DirArray direction_array, Vector2Int curr_chunk, Dictionary<Vector2Int, MapChunk> placed_chunks, HashSet<Vector2Int> queue_chunks)
    {
        Vector2Int[] dir_pointer = four_directions;
        switch(direction_array)
        {
            case DirArray.FOUR:
                dir_pointer = four_directions;
                break;
            case DirArray.EIGHT:
                dir_pointer = eight_directions;
                break;
            case DirArray.HORZ_WEIGHT_FOUR:
                dir_pointer = horz_weight_four_dir;
                break;
            case DirArray.HORZ_WEIGHT_EIGHT:
                dir_pointer = horz_weight_eight_dir;
                break;
        }

        list_pointer.Clear();
        foreach(Vector2Int dir in dir_pointer)
        {
            Vector2Int pos = curr_chunk + dir;
            if (!placed_chunks.ContainsKey(pos) && !queue_chunks.Contains(pos)) // only add to list if it aint overlappign w anything
            {
                list_pointer.Add(pos);
            }
        }
    }

    
}