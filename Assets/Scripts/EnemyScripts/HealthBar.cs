using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject bar;

    private float maxValue;
    private float barMult;

    private void Start()
    {
        barMult = 1 / maxValue;
    }

    public void ResetBar()
    {
        bar.transform.localScale = new Vector3(1, bar.transform.localScale.y, bar.transform.localScale.z);
    }
    public void SetMaxValue(float value)
    {
        if (value <= 0) return;
        maxValue = value;
        barMult = 1 / maxValue;
        ResetBar();
    }
    /// <summary>
    /// Scales bar on X Axis accourding to the maximum value and given value
    /// </summary>
    /// <param name="value"></param>
    public void UpdateBar(int value)
    {
        float xValue = value * barMult;
        if (value < 0) xValue = 0;
        bar.transform.localScale = new Vector3(xValue, bar.transform.localScale.y, bar.transform.localScale.z);
    }

    private void OnDestroy()
    {
    }
}
