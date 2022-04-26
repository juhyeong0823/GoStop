using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<CardDataSO> allCardsData = new List<CardDataSO>(); // �����ؼ� �� ����Ʈ

    [HideInInspector] public List<Card> useCardList = new List<Card>(); // ���� ����� ��
    [HideInInspector] public Queue<Card> followUpCardQueue = new Queue<Card>(); // ���б� �� �� �뵵

    [HideInInspector] public List<Card> myUtilizeCards = new List<Card>();     // ���� ��� ������ �е�
    [HideInInspector] public List<Card> othersUtilizeCards = new List<Card>(); // ��밡 ��� ������ �е�

    [HideInInspector] public List<Card> myCards = new List<Card>();            // ���� ���� �е�
    [HideInInspector] public List<Card> otherCards = new List<Card>();         // ��밡 ���� �е�

    public List<CardGrid> cardGrids = new List<CardGrid>(); // ī�带 �� ��ġ, ������ �ٸ��� �׳� �� ���� ã�Ƽ� ã�ư���
    

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // �ΰ��� �и� ġ��, ���߿� ������

    public Queue<Card> putCardQueue = new Queue<Card>(); // ����� �� �ֵ� �Ծ������ ��

    public Card cardPrefab;

    #region
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

    [Header("�� ī�� ������ ��ġ")]
    public Transform card1Trm;
    public Transform card2Trm;

    #endregion

    public bool isChoicing = false;
    public bool isUserTurn = true;
    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    void GameStart()
    {
        SetUseCardList();
        GiveAndPlaceCards();
        SetFollowCardQueue();
    }

    public Card GetRandomCard()
    {
        int rand = UnityEngine.Random.Range(0, useCardList.Count);
        Card retCard = useCardList[rand];
        useCardList.RemoveAt(rand);
        return retCard;
    }


    /*
    List<int> takenMonthList = new List<int>();  // ���� ��� �� ����
    public bool IsTakenMonthCard(Card card) // ���� ������
    {
        foreach (int month in takenMonthList)
        {
            if (month == card.cardData.cardMonth) return true;
        }
        return false;
    }
    */

    public void ChoiceCard(Card card1, Card card2, CardGrid grid)
    {
        if (grid.placedCards.Count == 0)  // ���̰ų� 3�� �� �������� �˾Ҵµ� �������� ��������. 
        {
            putCardQueue.Dequeue(); // �ѹ� �����ϸ� �ϳ� ����� ��
            TryExecuteChoiceCallback(); // ������ �ֳ� Ȯ�����ְ�.
        }
        else
        {
            isChoicing = true; // ���� �ൿ ��� �ʿ��� ��. �������� ���� �ƹ��͵� ���ϰ� -> �гη� ���Ƶα� ������ �� ���� ��� �ֳ�?
            UIManager.Instance.choiceUI.SetData(card1, card2, grid);
        }
    }

    public void OnChooseCard(Card chosenCard, CardGrid cardGrid) // ī�尡 �ִ� ���� ����Ʈ ������� ��.
    {
        Card card = putCardQueue.Dequeue();
        OnScored(card, cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() // �� �� �޾ƿ���
    {
        return cardGrids.Find((x) => x.placedCards.Count == 0); // ��ġ�� ģ���� �ִ��� ������.
    }

    public void OnScored(Card card, CardGrid cardGrid = null) // ī�带 ���� ��
    {
        card.transform.parent = GetDestination(card);
        myCards.Add(card);
        if (cardGrid != null) cardGrid.placedCards.Remove(card);
    }

    public void TryExecuteChoiceCallback()
    {
        if (choiceCallBackQueue.Count > 0)
        {
            choiceCallBackQueue.Dequeue()?.Invoke();
        }
        else
        {

        }
    }

    public void TryExecuteOtherTurn()
    {

    }

    public void SetGrid(CardGrid nullGrid, Card placeCard)
    {
        nullGrid.curPlacedCardMonth = placeCard.cardData.cardMonth; // ���� �� �׸���� �� ī���� ���� �ش��ϴ� �͸� �޾ƿ� �� ����!
        nullGrid.placedCards.Add(placeCard);
        placeCard.transform.parent = nullGrid.transform;
    }

    Transform GetDestination(Card card)
    {
        Transform destination = null;
        eProperty e = card.cardData.cardProperty;

        if (isUserTurn)
        {
            if ((e & eProperty.ANIMAL) != 0) destination = myAnimalsTrm;
            else if ((e & eProperty.Gwang) != 0) destination = myGwangTrm;
            else if ((e & eProperty.BLUE_BAND) != 0 || (e & eProperty.RED_BAND) != 0 || (e & eProperty.LEAF_BAND) != 0) destination = myBandTrm;
            else if ((e & eProperty.NONE) == 0) destination = myPeeTrm;
        }
        else
        {
            if ((e & eProperty.ANIMAL) != 0) destination = othersAnimalsTrm;
            else if ((e & eProperty.Gwang) != 0) destination = othersGwangTrm;
            else if ((e & eProperty.BLUE_BAND) != 0 || (e & eProperty.RED_BAND) != 0 || (e & eProperty.LEAF_BAND) != 0) destination = othersBandTrm;
            else if ((e & eProperty.NONE) == 0) destination = othersPeeTrm;
        }
        

        return destination;
    }

    public void ResetGrid(CardGrid grid)
    {
        grid.curPlacedCardMonth = -1;
    }


    void SetUseCardList()   
    {
        foreach (var item in allCardsData)
        {
            Card card = Instantiate(cardPrefab, cvsTrm);
            card.Init(item);
            useCardList.Add(card);
            card.gameObject.SetActive(false);
        }
    }
    

    void GiveCard(Transform parent, List<Card> targetList)
    {
        Card card = GetRandomCard();
        targetList.Add(card);
        card.transform.parent = parent;
        card.gameObject.SetActive(true);
    }
    void PlaceCard() // �˾Ƽ� 4�� ����
    {
        for (int i = 0; i < 4; i++)
        {
            Card card = GetRandomCard(); // �������� �ϳ� �̰�
            CardGrid grid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);
            if (grid == null)
            {
                grid = GetNullGrid();
            }

            SetGrid(grid, card);
            card.gameObject.SetActive(true);
        }
    }
    void GiveAndPlaceCards()
    {
        for (int i = 0; i < 4; i++) GiveCard(myUtilizeCardsTrm, myUtilizeCards);
        for (int i = 0; i < 4; i++) GiveCard(othersUtilizeCardsTrm, othersUtilizeCards);

        PlaceCard();

        for (int i = 0; i < 4; i++) GiveCard(myUtilizeCardsTrm, myUtilizeCards);
        for (int i = 0; i < 4; i++) GiveCard(othersUtilizeCardsTrm, othersUtilizeCards);

        PlaceCard();

        for (int i = 0; i < 2; i++) GiveCard(myUtilizeCardsTrm, myUtilizeCards);
        for (int i = 0; i < 2; i++) GiveCard(othersUtilizeCardsTrm, othersUtilizeCards);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCardQueue.Enqueue(item);
        useCardList.Clear(); // �ִ� ���� �� �� ����.
    }

    public CardGrid PutCard(Card card, out Card putCard) // ���� �� �׸��带 ������ ������ ������?
    {
        putCard = card;

        // ���� ���� ����ִ� �׸��带 ã���ϴ�.
        CardGrid targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        //�и� �� ���(?)�� ��밡���� �� ����Ʈ���� ����
        myUtilizeCards.Remove(card);

        if (targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            targetGrid = PlaceCardAtNullGrid(card);
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // �ϳ� ������ �� ģ������ �켱 ����. -> �� ���ɼ�
                    card.transform.parent = targetGrid.transform;
                    targetGrid.placedCards.Add(card);
                    break;
                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
                    choiceCallBackQueue.Enqueue(() =>
                       ChoiceCard(targetGrid.placedCards[0], targetGrid.placedCards[1], targetGrid));
                    putCardQueue.Enqueue(card);

                    card.transform.parent = targetGrid.transform;
                    targetGrid.placedCards.Add(card);

                    break;

                case 3: // 3�� ������ �� �������� 
                    while (targetGrid.placedCards.Count > 0)
                    {
                        OnScored(targetGrid.placedCards[0], targetGrid);
                    }

                    // ���� �� �͵� ��������.
                    OnScored(card, targetGrid);

                    ResetGrid(targetGrid);
                    break;
            }
        }

        return targetGrid;
    }

    public void FollowCard(Card putCard)
    {
        Card followCard = followUpCardQueue.Dequeue();

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, followCard);

        if (followCardTargetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            followCardTargetGrid = PlaceCardAtNullGrid(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
                    if (putCard == followCard)
                    {
                        Debug.Log("��");
                        // ���� ����
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    ResetGrid(followCardTargetGrid);
                    break;

                case 2: // '' �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    transform.parent = followCardTargetGrid.transform;
                    followCardTargetGrid.placedCards.Add(followCard);

                    if (putCard == followCard) //����
                    {
                        Debug.Log("��");
                    }
                    else
                    {
                        choiceCallBackQueue.Enqueue(() =>
                            ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                        putCardQueue.Enqueue(followCard);
                    }
                    break;

                case 3: // 3�� ������ �� �������� 
                    if (putCard == followCard)
                    {
                        Debug.Log("����");
                        //���� ����
                    }


                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    OnScored(followCard); // ���� �� �͵� ��������.
                    ResetGrid(followCardTargetGrid);
                    break;
            }
        }

        TryExecuteChoiceCallback();

    }//followCard ��

    public void TakePair_PutCard(CardGrid putGrid) // ���� �� ���ε�, �ΰ��� �ִ°Ÿ� ��������
    {
        if (putGrid.placedCards.Count == 2)
        {
            while (putGrid.placedCards.Count != 0)
            {
                OnScored(putGrid.placedCards[0], putGrid);
            }

            ResetGrid(putGrid);
        }
        else
        {
            Debug.Log("ī�� �� " + putGrid.placedCards.Count);
        }
    }

    public CardGrid PlaceCardAtNullGrid(Card placeCard)
    {
        CardGrid nullGrid = GetNullGrid();
        SetGrid(nullGrid, placeCard);

        return nullGrid;
    }
}
