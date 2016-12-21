using UnityEngine;
using System.Collections;

public class MyMaths{

    public static float increasingScale(float x)
    {
        return 1 - Mathf.Exp(-x * 8);
    }
    public static float decreasingScale(float x)
    {
        return 1 - Mathf.Exp((x - 1) * 9);
    }


    static public float getDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    static public float getDistance(int[] p1, int[] p2)
    {
        return getDistance(p1[0], p1[1], p2[0], p2[1]);
    }
}
