using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    public List<CardDataSO> allCardsData = new List<CardDataSO>(); // �����ؼ� �� ����Ʈ

    public List<Card> myUtilizeCards = new List<Card>();     // ���� ��� ������ �е�
    public List<Card> othersUtilizeCards = new List<Card>(); // ��밡 ��� ������ �е�

    public Transform cvsTrm;

    [Header("�� �ǵ� ���� ��ġ")]
    [SerializeField] private Transform myUtilizeCardsTrm;
    [SerializeField] private Transform myGwangTrm;
    [SerializeField] private Transform myAnimalsTrm;
    [SerializeField] private Transform myBandTrm;
    [SerializeField] private Transform myPeeTrm;

    [Space(15)]

    [Header("��� �ǵ� ���� ��ġ")]
    [SerializeField] private Transform othersUtilizeCardsTrm;
    [SerializeField] private Transform othersPeeTrm;
    [SerializeField] private Transform othersBandTrm;
    [SerializeField] private Transform othersGwangTrm;
    [SerializeField] private Transform othersAnimalsTrm;

    public Card cardPrefab;

    GameManager gm;
    private void Awake()
    {
        gm = GameManager.Instance;
    }

    public void TransportCard(Card card)
    {
        Transform destination = null;
        eProperty e = card.cardData.cardProperty;

        if (gm.isUserTurn)
        {
            if ((e & eProperty.ANIMAL) != 0) destination = myAnimalsTrm;
            else if ((e & eProperty.Gwang) != 0) destination = myGwangTrm;
            else if ((e & eProperty.BLUE_BAND) != 0 || (e & eProperty.RED_BAND) != 0 || (e & eProperty.LEAF_BAND) != 0 || (e & eProperty.BBand) != 0) destination = myBandTrm;
            else if ((e & eProperty.NONE) == 0) destination = myPeeTrm;
        }
        else
        {
            if ((e & eProperty.ANIMAL) != 0) destination = othersAnimalsTrm;
            else if ((e & eProperty.Gwang) != 0) destination = othersGwangTrm;
            else if ((e & eProperty.BLUE_BAND) != 0 || (e & eProperty.RED_BAND) != 0 || (e & eProperty.LEAF_BAND) != 0 || (e & eProperty.BBand) != 0)  destination = othersBandTrm;
            else if ((e & eProperty.NONE) == 0) destination = othersPeeTrm;
        }

        card.transform.parent = destination;
    }

    
    void PushUtilizeCards(Transform parent, List<Card> pushCards) // ����� ī��� �����ؼ� UI�� ������
    {
        foreach (var card in pushCards) card.transform.parent = parent;
    }

    public void SetUtilizeCards()
    {
        myUtilizeCards.Sort((x, y) => x.cardData.cardMonth.CompareTo(y.cardData.cardMonth));
        PushUtilizeCards(myUtilizeCardsTrm, myUtilizeCards);
        PushUtilizeCards(othersUtilizeCardsTrm, othersUtilizeCards);
    }

    public void SetUseCardList()
    {
        foreach (var item in allCardsData)
        {
            Card card = Instantiate(cardPrefab, cvsTrm);
            card.Init(item);
            gm.useCardList.Add(card);
            card.gameObject.SetActive(false);
        }
    }

    void GiveCard(List<Card> targetList)
    {
        Card card = gm.GetRandomCard();
        targetList.Add(card);
        card.gameObject.SetActive(true);
    }

    void PlaceCard() // �˾Ƽ� 4�� ����
    {
        Card card = gm.GetRandomCard(); // �������� �ϳ� �̰�
        CardGrid grid = CardFinder.GetSameMonthCardsGrid(gm.cardGrids, card);
        if (grid == null)
        {
            grid = gm.GetNullGrid();
        }

        grid.Set(card);
        card.gameObject.SetActive(true);
    }

    public void SetPlayersUtilizeCard()
    {
        for (int i = 0; i < 4; i++) GiveCard(myUtilizeCards);
        for (int i = 0; i < 4; i++) GiveCard(othersUtilizeCards);

        for (int i = 0; i < 4; i++) PlaceCard();

        for (int i = 0; i < 4; i++) GiveCard(myUtilizeCards);
        for (int i = 0; i < 4; i++) GiveCard(othersUtilizeCards);

        for (int i = 0; i < 4; i++) PlaceCard();

        for (int i = 0; i < 2; i++) GiveCard(myUtilizeCards);
        for (int i = 0; i < 2; i++) GiveCard(othersUtilizeCards);
    }

    public List<Card> GetSameMonthCards(Card card)
    {
        return myUtilizeCards.FindAll((x) => x.cardData.cardMonth == card.cardData.cardMonth);
    }


    public void TakePairCard(CardGrid putGrid) // ���� �� ���ε�, �ΰ��� �ִ°Ÿ� ��������
    {
        if (putGrid.placedCards.Count == 2)
        {
            while (putGrid.placedCards.Count != 0)
            {
                gm.OnScored(putGrid.placedCards[0], putGrid);
            }

            putGrid.Reset();
        }
    }
}
