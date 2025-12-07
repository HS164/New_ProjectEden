using UnityEditor.Build.Pipeline;
using UnityEngine;

[System.Serializable]
public class MoveSensitivity
{
    [SerializeField] private float max;
    [SerializeField] private float min;
    [SerializeField] private float startSensitivity;
    [SerializeField] private float increaseValue;
    [SerializeField] private float acceleration;
    [SerializeField, ReadOnly] private float currentSpeed;
    [SerializeField, ReadOnly]private float sensitivity;

    public void Init()
    {
        sensitivity = min;
        currentSpeed = startSensitivity;
    }

    public void SpeedUp()
    {
        sensitivity += increaseValue;
        Mathf.Clamp(sensitivity, min, max);
    }

    public void Reset()
    {
        currentSpeed = startSensitivity;
    }

    private float CalcSensitivity()
    {
        if (currentSpeed < sensitivity)
        {
            currentSpeed += acceleration;
            Mathf.Clamp(currentSpeed, startSensitivity, sensitivity);
        }

        return currentSpeed;
    }

    public float Sensitivity
    {
        get => CalcSensitivity();
    }

    public float IncreaseValue
    {
        set => increaseValue = value;
    }
}
