using System.Collections.Generic;
using UnityEngine;

public class TriangleSet
{
    public HashSet<Vector2> points;

    public TriangleSet(Vector2 point_1, Vector2 point_2, Vector2 point_3)
    {
        points = new HashSet<Vector2>() {point_1, point_2, point_3};
    }
}