using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Card> placedCard = new List<Card>(); // �ٴڿ� ����ִ� ī��

    public List<Card> allCards = new List<Card>(); // �����ؼ� �� ����Ʈ

    public List<Card> useCardList = new List<Card>(); // ���� ����� ��
    public Queue<Card> followUpCardQueue = new Queue<Card>(); // ���б� �� �� �뵵

    public List<CardGrid> cardGrids = new List<CardGrid>(); // ī�带 �� ��ġ, ������ �ٸ��� �׳� �� ���� ã�Ƽ� ã�ư���

    public List<Card> myCards = new List<Card>();
    public List<Card> othersCards = new List<Card>();

    [Header("�� �ǵ� ���� ��ġ")]
    [SerializeField] private Transform myPeeTrm;    
    [SerializeField] private Transform myBandTrm;    
    [SerializeField] private Transform myGwangTrm;    
    [SerializeField] private Transform myAnimalsTrm;

    [Space(15)]

    [Header("��� �ǵ� ���� ��ġ")]
    [SerializeField] private Transform othersPeeTrm;    
    [SerializeField] private Transform othersBandTrm;    
    [SerializeField] private Transform othersGwangTrm;    
    [SerializeField] private Transform othersAnimalsTrm;    

    public CardGrid GetNullGridPos() // �� �� �޾ƿ���
    {
        return cardGrids.Find((x) => x.bPlacedCardIsExist == false); // ��ġ�� ģ���� �ִ��� ������.
    }

    public void OnScored(Card card) // ī�带 ���� ��
    {
        // ������ ���� ��ġ �з����ְ�, �θ� �ٸ��� ����� �� �׸��� ���̾ƿ����� ó���Ұž� �����ִ°�

        myCards.Add(card);
    }
}
