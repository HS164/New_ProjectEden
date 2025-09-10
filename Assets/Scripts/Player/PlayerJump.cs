using UnityEngine;

[System.Serializable]
public class PlayerJump
{
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private int maxTimes;
    [SerializeField, ReadOnly] private int times;

    public void Reset()
    {
        times = 0;
    }

    public bool CanJump()
    {
        return times < maxTimes - 1;
    }

    public void CountUpJump()
    {
        times++;
    }

    public float Power
    {
        get => jumpPower;
    }

    public bool IsJump()
    {
        return times > 0;
    }
}
