using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class PlayerJump
{
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private int maxTimes;
    [SerializeField, ReadOnly] private int times;
    [SerializeField, ReadOnly] private bool canGroundJudgment = false;

    public void Reset()
    {
        times = 0;
        canGroundJudgment = false;
    }

    public bool CanJump()
    {
        return times < maxTimes;
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

    public bool CanGroundJudgment()
    {
        return canGroundJudgment;
    }

    public async UniTask DelayGroundJugment()
    {
        await UniTask.WaitForSeconds(0.5f);

        canGroundJudgment = true;
    }
}
