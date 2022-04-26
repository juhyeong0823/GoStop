using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreCounter
{
    // 기준점이기때문에 1 뺀 값으로 설정해둠.
    const int ANIMAL_DATUM_LINE   = 4; 
    const int BAND_DATUM_LINE     = 4; 
    const int JUNK_DATUM_LINE     = 9;

    const int GODORI_DATUM_LINE   = 3; 
    const int LEAFBAND_DATUM_LINE = 3; 
    const int BLUEBAND_DATUM_LINE = 3; 
    const int REDBAND_DATUM_LINE  = 3;

    

    int magnification = 1;
    public int nagariCount = 0;
    

    void Reset(ScoreData sd)
    {
        magnification = 1;

        sd.GwangCount = 0;
        sd.BandCount = 0;
        sd.JunkCount = 0;
        sd.AnimalCount = 0;
        sd.GodoriCount = 0;
        sd.LeafBandCount = 0;
        sd.BlueBandCount = 0;
        sd.RedBandCount = 0;

        sd.isHavingBGwang = false;
        sd.isHavingBBand = false;
        sd.isHavingDoubleNine = false;
    }

    public void CheckCards(List<Card> cards, ScoreData sd)
    {
        Reset(sd);

        foreach (Card card in cards)
        {
            if ((card.cardData.cardProperty & eProperty.BGwang)            != 0)  { sd.GwangCount++;    sd.isHavingBGwang = true; }
            else if ((card.cardData.cardProperty & eProperty.Gwang)        != 0)    sd.GwangCount++;

            if ((card.cardData.cardProperty & eProperty.GODORI)            != 0)  { sd.GodoriCount++;   sd.AnimalCount++; }
            else if ((card.cardData.cardProperty & eProperty.ANIMAL)       != 0)    sd.AnimalCount++;

            if ((card.cardData.cardProperty & eProperty.LEAF_BAND)         != 0)  { sd.LeafBandCount++; sd.BandCount++; }
            else if ((card.cardData.cardProperty & eProperty.RED_BAND)     != 0)  { sd.RedBandCount++;  sd.BandCount++; }
            else if ((card.cardData.cardProperty & eProperty.BLUE_BAND)    != 0)  { sd.BlueBandCount++; sd.BandCount++; }
            else if ((card.cardData.cardProperty & eProperty.BBand)        != 0)  { sd.BandCount++;     sd.isHavingBBand = true; }

            if ((card.cardData.cardProperty & eProperty.DOUBLE_NINE)       != 0)  { sd.JunkCount+= 2;   sd.isHavingDoubleNine = true; }
            else if ((card.cardData.cardProperty & eProperty.DOUBLE)       != 0)    sd.JunkCount+=2;
            else if (card.cardData.cardProperty == eProperty.NONE)                sd.JunkCount++;
        }
    }


    public int GetMagnification(ScoreData winningPlayer, ScoreData other)
    {
        if (winningPlayer.AnimalCount >= 7)
            if (other.AnimalCount <= 0) magnification *= 2;

        if (winningPlayer.BandCount >= 5)
            if (other.BandCount <= 0) magnification *= 2;

        if (winningPlayer.GwangCount >= 3)
            if (other.GwangCount <= 0) magnification *= 2;

        if (winningPlayer.JunkCount >= 10)
            if (other.JunkCount <= 4 && other.JunkCount != 0) magnification *= 2;

        magnification *= (int)(Mathf.Pow(2, winningPlayer.shakedCount));

        return 1;
    }

    public int CalculateByGoCount(int score, int goCount)
    {
        int multiplyValue = 1;

        for(int i = 0; i< goCount; i++)
        {
            if (i > 2) multiplyValue *= 2;

            score += i + 1;
        }

        return score * multiplyValue;
    }

    public int CalculateByNagariCount(int score)
    {
        int multiplyValue = 1;

        for (int i = 0; i < nagariCount; i++) multiplyValue *= 2;

        return score * multiplyValue;
    }

    public int GetScore(ScoreData sd)
    {
        int score = 0;

        if (sd.GodoriCount == GODORI_DATUM_LINE) score += 5;
        if (sd.BlueBandCount == BLUEBAND_DATUM_LINE) score += 3;
        if (sd.RedBandCount == REDBAND_DATUM_LINE)   score += 3;
        if (sd.LeafBandCount == LEAFBAND_DATUM_LINE) score += 3;

        if (sd.GwangCount == 3) score += sd.isHavingBGwang ? 2 : 3;
        else if(sd.GwangCount == 4) score += 4;
        else if(sd.GwangCount == 5) score += 15;

        int plusValue = 0;

        plusValue = sd.JunkCount - JUNK_DATUM_LINE;
        if (plusValue > 0) score += plusValue;

        plusValue = sd.BandCount - BAND_DATUM_LINE;
        if (plusValue > 0) score += plusValue;

        plusValue = sd.AnimalCount - ANIMAL_DATUM_LINE;
        if (plusValue > 0) score += plusValue;

        score = CalculateByGoCount(score, sd.goCount);
        return score;
    }

    public int GetCalculatedScore(ScoreData sd, ScoreData other)
    {
        int score = GetScore(sd);

        score = CalculateByNagariCount(score);
        score *= GetMagnification(sd, other);

        return score;
    }
}
