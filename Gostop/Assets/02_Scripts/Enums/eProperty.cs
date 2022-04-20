using System;

[Flags]
public enum eProperty
{
    NONE          = 0,
    Gwang         = 1 << 0,
    GODORI        = 1 << 1,
    ANIMAL        = 1 << 2,
    LEAF_BAND     = 1 << 4,
    RED_BAND      = 1 << 5,
    BLUE_BAND     = 1 << 6,
    DOUBLE_SCORE  = 1 << 7
}
