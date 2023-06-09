using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Range
{
    public float min;
    public float max;
    public float diff;

    public Range(float min, float max)
    {
        this.min = min;
        this.max = max;
        diff = max - min;
    }

    public float GetValueInRange(float value)
    {
        return Mathf.Clamp(value, min, max);
    }
    public float GetRandomValue()
    {
        return UnityEngine.Random.Range(min, max);
    }

#if UNITY_EDITOR
    public void ValidateValues()
    {
        if (min > max)
        {
            float temp = max;
            max = min;
            min = temp;
        }
    }
#endif
}

