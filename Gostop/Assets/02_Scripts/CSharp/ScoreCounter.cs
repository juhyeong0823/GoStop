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

    public int jum = 100; // 점당 n원
    public int PresdientScore = 10;
    int magnification = 1;
    public int nagariCount = 0;

    public bool CheckPaulkCount(ScoreData scoreData)
    {
        if(scoreData.paulkCount >= 3)
        {
            return true;
        }
        return false;
    }


    public void SetData(int jum)
    {
        this.jum = jum; 
    }

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

    public void CheckCards(List<CardBase> cards, ScoreData sd)
    {
        Reset(sd);

        foreach (CardBase card in cards)
        {
            if ((card.cardData.cardProperty & eProperty.Junk)         != 0)    sd.JunkCount++;
            if ((card.cardData.cardProperty & eProperty.DoubleNine)   != 0)    sd.isHavingDoubleNine = true; 
            if ((card.cardData.cardProperty & eProperty.Double)       != 0)    sd.JunkCount += 2;

            if ((card.cardData.cardProperty & eProperty.Godori)       != 0)    sd.GodoriCount++; 
            if ((card.cardData.cardProperty & eProperty.Animal)       != 0)    sd.AnimalCount++;

            if ((card.cardData.cardProperty & eProperty.BGwang)       != 0)    sd.isHavingBGwang = true; 
            if ((card.cardData.cardProperty & eProperty.Gwang)        != 0)    sd.GwangCount++;

            if ((card.cardData.cardProperty & eProperty.LeafBand)     != 0)    sd.LeafBandCount++;
            if ((card.cardData.cardProperty & eProperty.RedBand)      != 0)    sd.RedBandCount++; 
            if ((card.cardData.cardProperty & eProperty.BlueBand)     != 0)    sd.BlueBandCount++;
            if ((card.cardData.cardProperty & eProperty.BBand)        != 0)    sd.isHavingBBand = true;  
            if ((card.cardData.cardProperty & eProperty.Band)         != 0)    sd.BandCount++; 
        }
    }

    int GetMagnification(ScoreData winningPlayer, ScoreData other)
    {
        magnification = 1;

        if (winningPlayer.AnimalCount >= 7 && other.AnimalCount <= 0) magnification *= 2;

        if (winningPlayer.BandCount >= 5 && other.BandCount <= 0) magnification *= 2;

        if (winningPlayer.GwangCount >= 3 && other.GwangCount <= 0) magnification *= 2;

        if (winningPlayer.JunkCount >= 10 && other.JunkCount <= 4 && other.JunkCount != 0) magnification *= 2;

        if (other.goCount >= 1) magnification *= 2;

        for(int i =0; i< winningPlayer.shakedCount; i++)
        {
            magnification *= 2;
        }


        Debug.Log($"magnification : {magnification}");
        return magnification;
    }

    public int CalculateByGoCount(int score, int goCount)
    {
        int multiplyValue = 1;

        for(int i = 0; i < goCount; i++)
        {
            score += 1;

            if (i >= 2) multiplyValue *= 2;
        }

        Debug.Log($"점수 : {score}, 고에 의한 계산 점수 : {score * multiplyValue}");

        return score * multiplyValue;
    }

    public int CalculateByNagariCount(int score)
    {
        int multiplyValue = 1;

        for (int i = 0; i < nagariCount; i++) multiplyValue *= 2;

        Debug.Log($"점수 : {score}, 나가리에 의한 계산 점수 : {score * multiplyValue}");
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

        return score;
    }

    public int GetCalculatedScore(int defineScore = 0)
    {
        ScoreData sd = null;
        ScoreData other = null;

        if (MatgoManager.Instance.isUserTurn)
        {
            sd = MatgoManager.Instance.user.scoreData;
            other = MatgoManager.Instance.ai.userData.scoreData;
        }
        else
        {
            other = MatgoManager.Instance.user.scoreData;
            sd = MatgoManager.Instance.ai.userData.scoreData;
        }

        int score = 0;
        if (defineScore == 0)
        {
            score = GetScore(sd);

            score = CalculateByGoCount(score, MatgoManager.Instance.targetUser.scoreData.goCount);
            score = CalculateByNagariCount(score);
            score *= GetMagnification(sd, other);
            score *= jum;
        }
        else
        {
            score = defineScore;
        }
        

        return score;
    }

    public void SetMoney(int score = 0)
    {
        if (MatgoManager.Instance.isUserTurn)
        {
            MatgoManager.Instance.user.money += MatgoManager.Instance.sc.GetCalculatedScore(score);
        }
        else
        {
            MatgoManager.Instance.user.money -= MatgoManager.Instance.sc.GetCalculatedScore(score);
        }
    }

    public int GetPresidentMoney()
    {
        return PresdientScore * jum;
    }
}
