using System;
using Godot;

public struct MathUtils
{
    public static int Wrap(int kX, int kLowerBound, int kUpperBound)
    {
        int range_size = kUpperBound - kLowerBound + 1;

        if (kX < kLowerBound)
            kX += range_size * ((kLowerBound - kX) / range_size + 1);

        return kLowerBound + (kX - kLowerBound) % range_size;
    }

    public static float LerpAngle(float from, float to, float weight)
    {
        if (Math.Abs(from - to) >= Math.PI)
        {
            if (from > to)
            {
                from = NormalizeAngle(from) - 2.0f * (float) Math.PI;
            }
            else
            {
                to = NormalizeAngle(to) - 2.0f * (float) Math.PI;
            }
        }
        return Mathf.Lerp(from, to, weight);
    }

    public static float NormalizeAngle(float angle)
    {
        float normalizedAngle = (angle % 360 + 360) % 360;
        return normalizedAngle;
    }
}