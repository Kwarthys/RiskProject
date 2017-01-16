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

    static public float getDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    static public float getDistance(int[] p1, int[] p2)
    {
        return getDistance(p1[0], p1[1], p2[0], p2[1]);
    }

    static public float getAngle(int x1, int y1, int x2, int y2)
    {

        float dy = y2 - y1, dx = x2 - x1;

        float length = getDistance(x1, y1, x2, y2);

        float angle;

        if (dy > 0)
        {
            if (dx > 0)
            {
                angle = -Mathf.Asin(Mathf.Abs(dy) / (float)length);
            }
            else
            {
                angle = Mathf.Asin(Mathf.Abs(dy) / (float)length) + Mathf.PI;
            }
        }
        else
        {
            if (dx > 0)
            {
                angle = Mathf.Asin(Mathf.Abs(dy) / (float)length);
            }
            else
            {
                angle = Mathf.Asin(Mathf.Abs(dx) / (float)length) + Mathf.PI / 2;
            }
        }

        return angle*Mathf.Rad2Deg;
    }
}
