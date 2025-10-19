using DocumentFormat.OpenXml.Drawing;
using UnityEngine;

public class PlayerPowerManager : SingletonBehaviour<PlayerPowerManager>
{
    private PlayerPowerEnum powerLevel = PlayerPowerEnum.GALE;

    [SerializeField] private float powerGauge = 0;
    [SerializeField] private float lightningPowerUpValue = 50;
    [SerializeField] private float godPowerUpValue = 100;

    private void Start()
    {
        SetPowerLevel();
    }

    public void ChargeGauge()
    {
        powerGauge++;
        SetPowerLevel();
    }

    private void SetPowerLevel()
    {
        if (powerGauge >= godPowerUpValue)
        {
            powerLevel |= PlayerPowerEnum.GOD;
            powerLevel |= PlayerPowerEnum.LIGHTNING;
        }
        else if (powerGauge >= lightningPowerUpValue)
        {
            powerLevel &= ~PlayerPowerEnum.GOD;
            powerLevel |= PlayerPowerEnum.LIGHTNING;
        }
        else
        {
            powerLevel = PlayerPowerEnum.GALE;
        }
    }

    public bool HasPowerLevel(PlayerPowerEnum powerLevel)
    {
        return (this.powerLevel & powerLevel) == powerLevel;
    }
}
