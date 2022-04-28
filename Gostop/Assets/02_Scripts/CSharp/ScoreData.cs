using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreData
{
    public int GwangCount = 0;
    public int BandCount = 0;
    public int JunkCount = 0; // 피
    public int AnimalCount = 0;
    public int GodoriCount = 0;
    public int LeafBandCount = 0;
    public int BlueBandCount = 0;
    public int RedBandCount = 0;

    public bool isHavingBGwang = false;
    public bool isHavingBBand = false;
    public bool isHavingDoubleNine = false;

    public int shakedCount = 0;
    public int goCount = 0;
    public int paulkCount = 0;

    public int saidGoScore = 100; // 처음엔 100으로 해서 else if문에 안 걸리도록.
    public bool bFirstGo = true;


}
