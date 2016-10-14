using UnityEngine;
using System.Collections;

public static class ExtensionMethods
{
    public static float Remap(this float a_Value, float a_From1, float a_To1, float a_From2, float a_To2)
    {
        return (a_Value - a_From1) / (a_To1 - a_From1) * (a_To2 - a_From2) + a_From2;
    }
}