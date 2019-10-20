using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public const float EPSILON = 0.01f;

    static public bool Close(float a, float b, float epsilon = EPSILON)
    {
        return (a - b) < epsilon && (b - a) < epsilon;
    }

    static public bool Close(Vector3 a, Vector3 b, float epsilon = EPSILON)
    {
        return Close(a.x, b.x, epsilon) && Close(a.y, b.y, epsilon) && Close(a.z, b.z, epsilon);
    }
    
    static public Vector3 Interpolate(Vector3 A, Vector3 B, float t)
    {
        return t * B + (1 - t) * A;
    }

    static public float EaseOutCubic(float t)
    {
        return (--t) * t * t + 1;
    }

    public static float AngleBetweenVector2(Vector2 v1, Vector2 v2)    {
        float sign = Mathf.Sign(v1.x * v2.y - v1.y * v2.x);
        return Vector2.Angle(v1, v2) * sign;
    }
}
