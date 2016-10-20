using UnityEngine;

public static class ExtensionMethods
{
    public static float Remap(this float a_Value, float a_From1, float a_To1, float a_From2, float a_To2)
    {
        return (a_Value - a_From1) / (a_To1 - a_From1) * (a_To2 - a_From2) + a_From2;
    }

    public static int Choose(params int[] a_Integers)
    {
        int value = a_Integers[randomValue];
        return value;
    }

    public static float Choose(params float[] a_Floats)
    {
        float value = a_Floats[randomValue];
        return value;
    }
}