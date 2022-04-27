using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public List<Card> ownCards = new List<Card>();         // 상대가 먹은 패들
    public List<Card> utilizeCards = new List<Card>(); // 상대가 사용 가능한 패들
    public ScoreData scoreData = new ScoreData();
}
