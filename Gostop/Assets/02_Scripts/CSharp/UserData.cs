using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData
{
    public List<CardBase> ownCards = new List<CardBase>();         // 상대가 먹은 패들
    public List<CardBase> utilizeCards = new List<CardBase>(); // 상대가 사용 가능한 패들
    public ScoreData scoreData = new ScoreData();

    public int money = 0;

    public Transform utilizeCardsTrm;
    public Transform gwangTrm;
    public Transform animalsTrm;
    public Transform bandTrm;
    public Transform junkTrm;
}
