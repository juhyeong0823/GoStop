using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    public List<CardDataSO> allCardsData = new List<CardDataSO>(); // 복사해서 쓸 리스트

    public List<Card> myUtilizeCards = new List<Card>();     // 내가 사용 가능한 패들
    public List<Card> othersUtilizeCards = new List<Card>(); // 상대가 사용 가능한 패들

    public Transform cvsTrm;

    [Header("내 피들 놓을 위치")]
    [SerializeField] private Transform myUtilizeCardsTrm;
    [SerializeField] private Transform myGwangTrm;
    [SerializeField] private Transform myAnimalsTrm;
    [SerializeField] private Transform myBandTrm;
    [SerializeField] private Transform myPeeTrm;

    [Space(15)]

    [Header("상대 피들 놓을 위치")]
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

    
    void PushUtilizeCards(Transform parent, List<Card> pushCards) // 사용할 카드들 정렬해서 UI로 보내기
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

    void PlaceCard() // 알아서 4번 돌림
    {
        Card card = gm.GetRandomCard(); // 랜덤으로 하나 뽑고
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


    public void TakePairCard(CardGrid putGrid) // 내가 낸 패인데, 두개만 있는거면 가져오기
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
