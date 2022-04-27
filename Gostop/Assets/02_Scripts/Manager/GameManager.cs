using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public List<Card> useCardList = new List<Card>(); // 실제 사용할 패
    [HideInInspector] public Queue<Card> followUpCardQueue = new Queue<Card>(); // 뒷패깔 때 쓸 용도

    public List<Card> ownCards = new List<Card>();            // 내가 먹은 패들

    public List<CardGrid> cardGrids = new List<CardGrid>(); // 카드를 깔 위치, 기존과 다르게 그냥 빈 곳을 찾아서 찾아간다
    List<int> takenMonthList = new List<int>();  // 먹힌 놈들 월 저장

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // 두개의 패를 치고, 나중에 고르도록
    public Queue<Card> putCardQueue = new Queue<Card>(); // 골랐을 때 애도 먹어버리면 됨

    public bool isChoicing = false;
    public bool isUserTurn = true;
    bool isFirstChecking = true;

    public ScoreCounter sc = new ScoreCounter();


    public UserData user = new UserData();
    public TestAI ai = new TestAI();

    UserData targetUserData = null;

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
        CardManager.Instance.SetUseCardList();
        CardManager.Instance.SetPlayersUtilizeCard(user,ai.userData);
        CardManager.Instance.SetUtilizeCards(user, ai.userData);
        SetFollowCardQueue();
    }



    public void TryExecuteChoiceCallback()
    {
        if (choiceCallBackQueue.Count > 0)
        {
            choiceCallBackQueue.Dequeue()?.Invoke();
        }
        else
        {
            OnTurnFinished();
        }
    }

    public void OnTurnFinished()
    {
        CheckScore(targetUserData.ownCards, targetUserData.scoreData);

        //나중에 이거 구조를 시간마다 해버리는걸로 하고, 시간 다되면 선택 알아서 해버리는걸로 바꾸자
        if (isUserTurn)
        {
            isUserTurn = false;
            ai.Turn();
        }
        else
        {
            isUserTurn = true;
        }

    }

    public void CheckScore(List<Card> checkCardList, ScoreData scoreData)
    {
        sc.CheckCards(checkCardList, scoreData);
        int score = sc.GetScore(scoreData);
        if(score >= 7 )
        {
            Debug.Log("Go or Stop?");
        }
    }

    public Card GetRandomCard(List<Card> useCardList)
    {
        int rand = UnityEngine.Random.Range(0, useCardList.Count);
        Card retCard = useCardList[rand];
        useCardList.RemoveAt(rand);
        return retCard;
    }

    public void ChoiceCard(Card card1, Card card2, CardGrid grid)
    {
        if (grid.placedCards.Count == 0)  // 2개 중 선택인줄 알았는데 따닥으로 먹은거죠. 
        {
            putCardQueue.Dequeue(); // 한번 실패하면 하나 빼줘야 해
            TryExecuteChoiceCallback(); // 다음거 있나 확인해주고.
        }
        else
        {
            if(isUserTurn)
            {
                isChoicing = true; // 뭔가 행동 제어가 필요할 듯. 선택중일 때는 아무것도 못하게 -> 패널로 막아두긴 했으나 더 좋은 방법 있나?
                UIManager.Instance.choiceUI.SetData(card1, card2, grid);
            }
            else
            {
                OnChooseCard(card1, grid);
            }
        }
    }

    public void OnShaked(Card chosenCard, CardGrid cardGrid)
    {
        Debug.Log("ㅈ망");
        cardGrid.Set(chosenCard);
        OnShakedCallback.Invoke(); // 이거 없으면 지랄난거라 널체크하면 안될듯..
        OnShakedCallback = null;
    }

    public void OnChooseCard(Card chosenCard, CardGrid cardGrid) // 카드가 있던 곳임 리스트 지워줘야 해.
    {
        Card card = putCardQueue.Dequeue();
        OnScored(card,cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() => cardGrids.Find((x) => x.placedCards.Count == 0);

    public void OnScored(Card card, CardGrid cardGrid = null) // 카드를 얻을 때
    {
        CardManager.Instance.TransportCard(card);
        if (cardGrid != null) cardGrid.placedCards.Remove(card);

        card.img.raycastTarget = false;
        targetUserData.ownCards.Add(card);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCardQueue.Enqueue(item);
        useCardList.Clear(); // 애는 이제 쓸 일 없음.
    }

    void CheckMyCards() // 내가 가진 패 중, 같은 월 카드가 몇개인지 -> 폭탄 흔들기 체크용임
    {
        foreach(var card in ownCards)
        {
            int sameWithItemMonth = CardManager.Instance.GetSameMonthCards(card).Count;
            if (sameWithItemMonth > 3)
            {
                //첫 턴인지 아닌지 체크해서 처음이면 총통
                if(isFirstChecking)
                {
                    // 총통, 승리 처리 후 게임 데이터 등에 static으로 박아놔서 Double! 박아두기
                    isFirstChecking = false;
                }
                else
                {
                    // 여기는 흔들기 100%
                }
            }
            else if(sameWithItemMonth > 2)
            {
                // 폭탄 or 흔들기인데 이제 
                if(CardFinder.GetSameMonthCardsGrid(cardGrids,card) != null)
                {
                    // 여기는 폭탄
                }
                else
                {
                    // 여기는 흔들기
                }
            }
            else if(IsTakenMonthCard(card))
            {
                // 카드 위 커서, 살아 있는 패 커서로
            }
            else if(CardFinder.GetSameMonthCardsGrid(cardGrids, card) != null)
            {
                // 죽은 패
            }
            else
            {
                // 당장은 못 쓰는 패
            }
        }
    }



    public bool IsTakenMonthCard(Card card) // 죽은 패인지
    {
        foreach (int month in takenMonthList)
        {
            if (month == card.cardData.cardMonth) return true;
        }
        return false;
    }

    public void OnShakeOrBomb()
    {
        targetUserData.scoreData.shakedCount++;
    }    

    public void Paulk()
    {
        targetUserData.scoreData.paulkCount++;
    }

    public void TakeOtherPlayerCard(UserData myData, UserData otherData)
    {

    }

    public void Kiss()
    {
        //TakeOtherPlayerCard();
        Debug.Log("쪽");
    }



    public void SetTargetUserData()
    {
        if (isUserTurn)
        {
            targetUserData = user;
        }
        else
        {
            targetUserData = ai.userData;
        }
    }


    CardGrid targetGrid = null;

    public Action OnShakedCallback;

    public void PutCard(Card card) // 내가 낸 그리드를 가지고 있으면 좋을듯?ai.scoreData
    { 
        SetTargetUserData();

        targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        if (targetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            List<Card> cardList = CardManager.Instance.GetSameMonthCards(card);
            targetGrid = GetNullGrid();

            OnShakedCallback = () => FollowCord(card);

            if (cardList.Count >= 3)
            {
                UIManager.Instance.shakeUI.SetData(cardList[0], cardList[1], cardList[2], targetGrid);
                OnShakeOrBomb();
            }
            else
            {
                targetGrid.Set(card);
                FollowCord(card);
            }
        }
        else // 이 경우 자연스럽게 뒷 패도 깜.
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 하나 있으면 그 친구한테 우선 가기. -> 뻑 가능성
                    List<Card> cards = CardManager.Instance.GetSameMonthCards(card);
                    if (cards.Count == 3)
                    {
                        OnScored(targetGrid.placedCards[0],targetGrid); // 바닥 패 먹고
                        while (cards.Count > 0)
                        {
                            OnScored(cards[0]);
                            cards.RemoveAt(0);
                        }
                        OnShakeOrBomb();
                    }
                    else
                    {
                        card.MoveCardToGrid(targetGrid);
                    }
                    
                    break;
                case 2: // 두개 있으면 거기로 우선 가기 -> 따닥 가능성
                    card.MoveCardToGrid(targetGrid);

                    choiceCallBackQueue.Enqueue(() => 
                        ChoiceCard(targetGrid.placedCards[0], targetGrid.placedCards[1], targetGrid));
                    putCardQueue.Enqueue(card);
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    while (targetGrid.placedCards.Count > 0)
                    {
                        OnScored(targetGrid.placedCards[0],targetGrid);
                    }

                    OnScored(card);
                    OnShakeOrBomb();
                    targetGrid.Reset();
                    break;
            }

            FollowCord(card);
        }

    }//함수의 끝.

    public void FollowCord(Card putCard)
    {
        Card followCard = followUpCardQueue.Dequeue();

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, followCard);

        if (followCardTargetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            followCardTargetGrid = GetNullGrid();
            followCardTargetGrid.Set(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 뒷패를 깠는데 하나만 있다는 거면, 무조건 가져오면 됨.
                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        Kiss();
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // 두 개가 있다는 거면 둘 중 하나를 선택하게 하면 됨.
                    followCard.MoveCardToGrid(followCardTargetGrid);

                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth) //뻑임
                    {
                        Paulk();
                    }
                    else
                    {
                        choiceCallBackQueue.Enqueue(() =>
                            ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                        putCardQueue.Enqueue(followCard);
                    }
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        //TakeOtherPlayerCard();
                    }


                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    OnScored(followCard); // 내가 낸 것도 가져오기.
                    choiceCallBackQueue.Clear();
                    followCardTargetGrid.Reset();
                    break;
            }
        }

        CardManager.Instance.TakePairCard(targetGrid);
        TryExecuteChoiceCallback();

        targetGrid = null;
    }
}
