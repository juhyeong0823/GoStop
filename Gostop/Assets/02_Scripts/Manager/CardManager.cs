using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : Singleton<CardManager>
{
    public List<CardDataSO> allCardsData = new List<CardDataSO>(); // �����ؼ� �� ����Ʈ

    public Transform cvsTrm;

    [Header("�� �ǵ� ���� ��ġ")]
    [SerializeField] private Transform myUtilizeCardsTrm;
    [SerializeField] private Transform myGwangTrm;
    [SerializeField] private Transform myAnimalsTrm;
    [SerializeField] private Transform myBandTrm;
    [SerializeField] private Transform myJunkTrm;

    [Space(15)]

    [Header("��� �ǵ� ���� ��ġ")]
    [SerializeField] private Transform othersUtilizeCardsTrm;
    [SerializeField] private Transform othersJunkTrm;
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
            if ((e & eProperty.Animal) != 0) destination = myAnimalsTrm;
            else if ((e & eProperty.Gwang) != 0) destination = myGwangTrm;
            else if ((e & eProperty.Band) != 0) destination = myBandTrm;
            else  destination = myJunkTrm;
        }
        else
        {
            if ((e & eProperty.Animal) != 0) destination = othersAnimalsTrm;
            else if ((e & eProperty.Gwang) != 0) destination = othersGwangTrm;
            else if ((e & eProperty.Band) != 0)  destination = othersBandTrm;
            else destination = othersJunkTrm;
        }

        card.transform.parent = destination;

    }

    
    void PushUtilizeCards(Transform parent, List<Card> pushCards) // ����� ī��� �����ؼ� UI�� ������
    {
        foreach (var card in pushCards) card.transform.parent = parent;
    }

    public void SetUtilizeCards(UserData user, UserData ai)
    {
        user.utilizeCards.Sort((x, y) => x.cardData.cardMonth.CompareTo(y.cardData.cardMonth));
        PushUtilizeCards(myUtilizeCardsTrm, user.utilizeCards);
        PushUtilizeCards(othersUtilizeCardsTrm, ai.utilizeCards);
    }

    public void SetUseCardList()
    {
        foreach (var item in allCardsData)
        {
            Card card = Instantiate(cardPrefab, cvsTrm);
            card.Init(item);
            gm.useCardList.Add(card);
            card.img.raycastTarget = false;
            card.gameObject.SetActive(false);
        }
    }

    void GiveCard(UserData userData)
    {
        Card card = gm.GetRandomCard(gm.useCardList);
        userData.utilizeCards.Add(card);
        card.gameObject.SetActive(true);
        card.img.raycastTarget = true;
    }

    void PlaceCard() // �˾Ƽ� 4�� ����
    {
        Card card = gm.GetRandomCard(gm.useCardList); // �������� �ϳ� �̰�
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

    public List<Card> GetSameMonthCards(Card card)
    {
        return GameManager.Instance.user.utilizeCards.FindAll((x) => x.cardData.cardMonth == card.cardData.cardMonth);
    }


    public void TakePairCard(CardGrid putGrid) // ���� �� ���ε�, �ΰ��� �ִ°Ÿ� ��������
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
