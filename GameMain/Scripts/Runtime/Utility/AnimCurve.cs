using System;

public static class AnimCurve
{
    public static float InOutQuart(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2f;
        if (t < 1)
            return (float)(c / 2f * Math.Pow(t, 4f) + b);
        else
        {
            t -= 2f;
            return (float)(-c / 2f * (Math.Pow(t, 4f) - 2f) + b);
        }
    }

    public static float InoutCubic(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2f;
        if (t < 1)
            return c / 2f * t * t * t + b;
        else
        {
            t = t - 2f;
            return c / 2f * (t * t * t + 2f) + b;
        }
    }

    public static float InOutSine(float b, float to, float t, float d)
    {
        float c = to - b;
        return (float)(-c / 2f * (Math.Cos(Math.PI * t / d) - 1f) + b);
    }

    public static float InoutQuad(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d * 2f;
        if (t < 1f)
            return (float)(c / 2f * Math.Pow(t, 2f) + b);
        else
            return -c / 2f * ((t - 1f) * (t - 3f) - 1f) + b;
    }

    public static float OutQuad(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d;
        return (float)(-c * t * (t - 2f) + b);
    }

    public static float OutSine(float b, float to, float t, float d)
    {
        float c = to - b;
        return (float)(c * Math.Sin(t / d * 1.5708f) + b);
    }

    public static float InSine(float b, float to, float t, float d)
    {
        float c = to - b;
        return (float)(c * (Math.Sin((t / d - d) * 1.5708f) + 1) + b);
    }

    public static float OutCubic(float b, float to, float t, float d)
    {
        float c = to - b;
        t = t / d - 1f;
        return (float)(c * (Math.Pow(t, 3) + 1) + b);
    }
}
