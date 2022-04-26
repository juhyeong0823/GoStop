using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<CardDataSO> allCardsData = new List<CardDataSO>(); // 복사해서 쓸 리스트

    [HideInInspector] public List<Card> useCardList = new List<Card>(); // 실제 사용할 패
    [HideInInspector] public Queue<Card> followUpCardQueue = new Queue<Card>(); // 뒷패깔 때 쓸 용도

    [HideInInspector] public List<Card> myUtilizeCards = new List<Card>();     // 내가 사용 가능한 패들
    [HideInInspector] public List<Card> othersUtilizeCards = new List<Card>(); // 상대가 사용 가능한 패들

    [HideInInspector] public List<Card> myCards = new List<Card>();            // 내가 먹은 패들
    [HideInInspector] public List<Card> otherCards = new List<Card>();         // 상대가 먹은 패들

    public List<CardGrid> cardGrids = new List<CardGrid>(); // 카드를 깔 위치, 기존과 다르게 그냥 빈 곳을 찾아서 찾아간다
    

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // 두개의 패를 치고, 나중에 고르도록

    public Queue<Card> putCardQueue = new Queue<Card>(); // 골랐을 때 애도 먹어버리면 됨

    public Card cardPrefab;

    #region
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

    [Header("고를 카들 보여줄 위치")]
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
    List<int> takenMonthList = new List<int>();  // 먹힌 놈들 월 저장
    public bool IsTakenMonthCard(Card card) // 죽은 패인지
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
        if (grid.placedCards.Count == 0)  // 뻑이거나 3개 중 선택인줄 알았는데 따닥으로 먹은거죠. 
        {
            putCardQueue.Dequeue(); // 한번 실패하면 하나 빼줘야 해
            TryExecuteChoiceCallback(); // 다음거 있나 확인해주고.
        }
        else
        {
            isChoicing = true; // 뭔가 행동 제어가 필요할 듯. 선택중일 때는 아무것도 못하게 -> 패널로 막아두긴 했으나 더 좋은 방법 있나?
            UIManager.Instance.choiceUI.SetData(card1, card2, grid);
        }
    }

    public void OnChooseCard(Card chosenCard, CardGrid cardGrid) // 카드가 있던 곳임 리스트 지워줘야 해.
    {
        Card card = putCardQueue.Dequeue();
        OnScored(card, cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() // 빈 곳 받아오기
    {
        return cardGrids.Find((x) => x.placedCards.Count == 0); // 설치된 친구가 있는지 없는지.
    }

    public void OnScored(Card card, CardGrid cardGrid = null) // 카드를 얻을 때
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
        nullGrid.curPlacedCardMonth = placeCard.cardData.cardMonth; // 이제 이 그리드는 이 카드의 월에 해당하는 것만 받아올 수 있음!
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
    void PlaceCard() // 알아서 4번 돌림
    {
        for (int i = 0; i < 4; i++)
        {
            Card card = GetRandomCard(); // 랜덤으로 하나 뽑고
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
        useCardList.Clear(); // 애는 이제 쓸 일 없음.
    }

    public CardGrid PutCard(Card card, out Card putCard) // 내가 낸 그리드를 가지고 있으면 좋을듯?
    {
        putCard = card;

        // 같은 월이 깔려있는 그리드를 찾습니다.
        CardGrid targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        //패를 낸 사람(?)이 사용가능한 패 리스트에서 제거
        myUtilizeCards.Remove(card);

        if (targetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            targetGrid = PlaceCardAtNullGrid(card);
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 하나 있으면 그 친구한테 우선 가기. -> 뻑 가능성
                    card.transform.parent = targetGrid.transform;
                    targetGrid.placedCards.Add(card);
                    break;
                case 2: // 두개 있으면 거기로 우선 가기 -> 따닥 가능성
                    choiceCallBackQueue.Enqueue(() =>
                       ChoiceCard(targetGrid.placedCards[0], targetGrid.placedCards[1], targetGrid));
                    putCardQueue.Enqueue(card);

                    card.transform.parent = targetGrid.transform;
                    targetGrid.placedCards.Add(card);

                    break;

                case 3: // 3개 있으면 다 가져오고 
                    while (targetGrid.placedCards.Count > 0)
                    {
                        OnScored(targetGrid.placedCards[0], targetGrid);
                    }

                    // 내가 낸 것도 가져오기.
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

        if (followCardTargetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            followCardTargetGrid = PlaceCardAtNullGrid(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 뒷패를 깠는데 하나만 있다는 거면, 무조건 가져오면 됨.
                    if (putCard == followCard)
                    {
                        Debug.Log("쪽");
                        // 한장 뺏기
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    ResetGrid(followCardTargetGrid);
                    break;

                case 2: // '' 두 개가 있다는 거면 둘 중 하나를 선택하게 하면 됨.
                    transform.parent = followCardTargetGrid.transform;
                    followCardTargetGrid.placedCards.Add(followCard);

                    if (putCard == followCard) //뻑임
                    {
                        Debug.Log("뻑");
                    }
                    else
                    {
                        choiceCallBackQueue.Enqueue(() =>
                            ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                        putCardQueue.Enqueue(followCard);
                    }
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    if (putCard == followCard)
                    {
                        Debug.Log("따닥");
                        //한장 뺏기
                    }


                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    OnScored(followCard); // 내가 낸 것도 가져오기.
                    ResetGrid(followCardTargetGrid);
                    break;
            }
        }

        TryExecuteChoiceCallback();

    }//followCard 끝

    public void TakePair_PutCard(CardGrid putGrid) // 내가 낸 패인데, 두개만 있는거면 가져오기
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
            Debug.Log("카드 수 " + putGrid.placedCards.Count);
        }
    }

    public CardGrid PlaceCardAtNullGrid(Card placeCard)
    {
        CardGrid nullGrid = GetNullGrid();
        SetGrid(nullGrid, placeCard);

        return nullGrid;
    }
}
