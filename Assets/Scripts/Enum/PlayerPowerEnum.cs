using System;

[Flags]
public enum PlayerPowerEnum
{
    GALE = 1 << 0,       // 疾風
    LIGHTNING = 1 << 1,  // 迅雷
    GOD = 1 << 2,        // 神速
}
