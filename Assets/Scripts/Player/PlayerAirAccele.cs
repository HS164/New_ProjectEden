using UnityEngine;

[System.Serializable]
public class PlayerAirAccele
{
    [SerializeField] private float accelePower = 2f;
    [SerializeField] private int maxTimes = 1;
    [SerializeField, ReadOnly] private int times;

    public void Reset()
    {
        times = 0;
    }

    public bool CanAction()
    {
        return times < maxTimes;
    }

    public void CountUpAction()
    {
        times++;
    }

    public float Power
    {
        get => accelePower;
    }

    public bool IsAction()
    {
        return times > 0;
    }
}
