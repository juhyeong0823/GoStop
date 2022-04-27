using System;

[Flags]
public enum eProperty
{
    None        = 0,
    Junk        = 1,
    Gwang       = 2,
    Godori      = 4,
    Animal      = 8,
    LeafBand    = 16,
    RedBand     = 32,
    BlueBand    = 64,
    Double      = 128,
    BGwang      = 256,
    DoubleNine  = 512,
    BBand       = 1024,
    Band        = 2048
}
