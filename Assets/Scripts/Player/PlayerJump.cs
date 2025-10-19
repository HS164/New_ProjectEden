using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class PlayerJump
{
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private int maxTimes;
    [SerializeField, ReadOnly] private int times;
    [SerializeField, ReadOnly] private bool canGroundJudgement = false;

    public void Reset()
    {
        times = 0;
        canGroundJudgement = false;
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

    public bool CanGroundJudgement()
    {
        return canGroundJudgement;
    }

    public async UniTask DelayGroundJudgement()
    {
        await UniTask.WaitForSeconds(0.5f);

        canGroundJudgement = true;
    }
}
