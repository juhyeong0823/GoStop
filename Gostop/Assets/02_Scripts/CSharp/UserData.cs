using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public List<Card> ownCards = new List<Card>();         // ��밡 ���� �е�
    public List<Card> utilizeCards = new List<Card>(); // ��밡 ��� ������ �е�
    public ScoreData scoreData = new ScoreData();
}
