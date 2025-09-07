using UnityEditor.Build.Pipeline;
using UnityEngine;

[System.Serializable]
public class MoveSensitivity
{
    [SerializeField] private float max;
    [SerializeField] private float min;
    [SerializeField] private float increaseValue;
    [SerializeField, ReadOnly]private float sensitivity;

    public void Init()
    {
        sensitivity = min;
    }

    public void SpeedUp()
    {
        sensitivity += increaseValue;
        Mathf.Clamp(sensitivity, min, max);
    }

    public float Sensitivity
    {
        get => sensitivity;
    }

    public float IncreaseValue
    {
        set => increaseValue = value;
    }
}
