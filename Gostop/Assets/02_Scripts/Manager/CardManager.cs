using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : Singleton<CardManager>
{
    public List<CardDataSO> allCardsData = new List<CardDataSO>(); // 복사해서 쓸 리스트

    public Transform cvsTrm;


    public CardBase cardPrefab;
    public CardBase bombPaybackCardObj;

    GameManager gm;
    private void Awake()
    {
        gm = GameManager.Instance;
    }

    public void TransportCard(CardBase card)
    {
        Transform destination = null;
        eProperty e = card.cardData.cardProperty;

        if ((e & eProperty.Animal) != 0) destination = gm.targetUserData.animalsTrm;
        else if ((e & eProperty.Gwang) != 0) destination = gm.targetUserData.gwangTrm; 
        else if ((e & eProperty.Band) != 0) destination = gm.targetUserData.bandTrm; 
        else  destination = gm.targetUserData.junkTrm; 

        card.transform.parent = destination;

    }

    public void OnDroppedBomb(int paybackCardCount,Transform parent)
    {
        for(int i = 0; i< paybackCardCount; i++)
        {
            CardBase paybackCard = Instantiate(bombPaybackCardObj, parent);
            GameManager.Instance.targetUserData.utilizeCards.Add(paybackCard);
        }
    }

    void PushUtilizeCards(Transform parent, List<CardBase> pushCards) // 사용할 카드들 정렬해서 UI로 보내기
    {
        foreach (var card in pushCards) card.transform.parent = parent;
    }

    public void SetUtilizeCards(UserData user, UserData ai)
    {
        user.utilizeCards.Sort((x, y) => x.cardData.cardMonth.CompareTo(y.cardData.cardMonth));
        PushUtilizeCards(user.utilizeCardsTrm, user.utilizeCards);
        PushUtilizeCards(ai.utilizeCardsTrm, ai.utilizeCards);

    }

    public void SetUseCardList()
    {
        foreach (var item in allCardsData)
        {
            CardBase card = Instantiate(cardPrefab, cvsTrm);
            card.Init(item);
            gm.useCardList.Add(card);
            card.img.raycastTarget = false;
            card.gameObject.SetActive(false);
        }
    }

    void GiveCard(UserData userData)
    {
        CardBase card = gm.GetRandomCard(gm.useCardList);
        userData.utilizeCards.Add(card);
        card.gameObject.SetActive(true);
        card.img.raycastTarget = true;
    }

    void PlaceCard() // 알아서 4번 돌림
    {
        CardBase card = gm.GetRandomCard(gm.useCardList); // 랜덤으로 하나 뽑고
        CardGrid grid = CardFinder.GetSameMonthCardsGrid(gm.cardGrids, card);
        if (grid == null)
        {
            grid = gm.GetNullGrid();
        }

        grid.Set(card);
        card.gameObject.SetActive(true);
    }

    public void SetPlayersUtilizeCard(UserData user, UserData ai)
    {
        for (int i = 0; i < 4; i++) GiveCard(user);
        for (int i = 0; i < 4; i++) GiveCard(ai);

        for (int i = 0; i < 4; i++) PlaceCard();

        for (int i = 0; i < 4; i++) GiveCard(user);
        for (int i = 0; i < 4; i++) GiveCard(ai);

        for (int i = 0; i < 4; i++) PlaceCard();

        for (int i = 0; i < 2; i++) GiveCard(user);
        for (int i = 0; i < 2; i++) GiveCard(ai);
    }

    public List<CardBase> GetSameMonthCards(CardBase card)
    {
        List<CardBase> list = new List<CardBase>();
        try
        {
            list = GameManager.Instance.targetUserData.utilizeCards.FindAll((x) => x.cardData.cardMonth == card.cardData.cardMonth);
        }
        catch { }

        return list;
    }


    public void TakePairCard(CardGrid putGrid) // 내가 낸 패인데, 두개만 있는거면 가져오기
    {
        if (putGrid.placedCards.Count == 2)
        {
            while (putGrid.placedCards.Count != 0)
            {
                gm.OnScored(putGrid.placedCards[0],putGrid);
            }

            putGrid.Reset();
        }
    }
}
