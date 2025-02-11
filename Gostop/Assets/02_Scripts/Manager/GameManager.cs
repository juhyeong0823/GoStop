using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public List<CardBase> useCardList = new List<CardBase>(); // 실제 사용할 패
    [HideInInspector] public Queue<CardBase> followUpCardQueue = new Queue<CardBase>(); // 뒷패깔 때 쓸 용도

    public List<CardGrid> cardGrids = new List<CardGrid>(); // 카드를 깔 위치, 기존과 다르게 그냥 빈 곳을 찾아서 찾아간다
    public List<int> takenMonthList = new List<int>();  // 먹힌 놈들 월 저장

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // 두개의 패를 치고, 나중에 고르도록
    public Queue<CardBase> putCardQueue = new Queue<CardBase>(); // 골랐을 때 애도 먹어버리면 됨

    public bool isChoicing = false;
    public bool isShaking = false;
    public bool isUserTurn = true;

    public bool isFirstChecking = true;
    public bool isGameFinished = false;


    public ScoreCounter sc = new ScoreCounter();
    public Rule rule = new Rule();
    public SaveManager saveManager = new SaveManager();


    public UserData user = new UserData();
    public TestAI ai = new TestAI();

    public UserData targetUserData = null;

    private void Start()
    {
        user.money = saveManager.Load().money;
        UIManager.Instance.SetDatas(true, 0);
        GameStart();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ReLoad();
    }

    void GameStart()
    {
        CardManager.Instance.SetUseCardList();
        CardManager.Instance.SetPlayersUtilizeCard(user,ai.userData);
        CardManager.Instance.SetUtilizeCards(user, ai.userData);

        rule.CheckPresident();
        SetFollowCardQueue();
    }

    public void ReLoad()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void TakeOtherPlayerCard()
    {
        if (isUserTurn)
        {
            CardBase takeCard = null;
            takeCard = ai.userData.ownCards.Find((x) => (x.cardData.cardProperty & eProperty.Junk) != 0);

            if(takeCard == null)
            {
                takeCard = ai.userData.ownCards.Find((x) => (x.cardData.cardProperty & eProperty.Double) != 0
                                                             || (x.cardData.cardProperty & eProperty.DoubleNine) != 0);
            }

            if(takeCard != null)
            {
                ai.userData.ownCards.Remove(takeCard);
                user.ownCards.Add(takeCard);
                takeCard.transform.DOMove(user.junkTrm.position, 0.15f, false);
            }
        }
        else
        {
            CardBase takeCard = null;
            takeCard = user.ownCards.Find((x) => (x.cardData.cardProperty & eProperty.Junk) != 0);

            if (takeCard == null)
            {
                takeCard = user.ownCards.Find((x) => (x.cardData.cardProperty & eProperty.Double) != 0
                                                             || (x.cardData.cardProperty & eProperty.DoubleNine) != 0);
            }
            
            if (takeCard != null)
            {
                user.ownCards.Remove(takeCard);
                ai.userData.ownCards.Add(takeCard);
                takeCard.transform.DOMove(ai.userData.junkTrm.position, 0.15f, false);
            }
        }
    }

    public void TryExecuteChoiceCallback()
    {
        if (isGameFinished) return;


        if (choiceCallBackQueue.Count > 0)
        {
            choiceCallBackQueue.Dequeue()?.Invoke();
        }
        else
        {
            OnTurnFinished();
        }
    }


    public void SetTurn()
    {
        if (isUserTurn)
        {
            isUserTurn = false;
            StartCoroutine(ai.Turn());
        }
        else
        {
            isUserTurn = true;
        }
    }

    public void OnTurnFinished()
    {
        CheckScore(targetUserData.ownCards, targetUserData.scoreData);
        rule.Sweep();
    }
    
    public void CheckScore(List<CardBase> checkCardList, ScoreData scoreData)
    {
        sc.CheckCards(checkCardList, scoreData);
        int score = sc.GetScore(scoreData);
        UIManager.Instance.SetDatas(isUserTurn, score);

        if(rule.CanGo(score, scoreData))
        {
            if(targetUserData.utilizeCards.Count == 0)
            {
                UIManager.Instance.gostopUI.stopUI.OnStop();
            }
            else
            {
                UIManager.Instance.gostopUI.AccomplishedCondition();
            }
        }
        else
        {
            if(user.utilizeCards.Count == 0 && ai.userData.utilizeCards.Count == 0)
            {
                UIManager.Instance.gostopUI.stopUI.OnStop(false, true);
            }
            else
            {
                SetTurn();
            }
        }
    }

    public CardBase GetRandomCard(List<CardBase> useCardList)
    {
        int rand = UnityEngine.Random.Range(0, useCardList.Count);
        CardBase retCard = useCardList[rand];
        useCardList.RemoveAt(rand);
        return retCard;
    }

    public void ChoiceCard(CardBase card1, CardBase card2, CardGrid grid)
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

    public void OnShaked(CardBase clickedCard, CardGrid cardGrid, bool bShaked = true)
    {
        clickedCard.img.raycastTarget = false;
        cardGrid.Set(clickedCard);
        OnShakedCallback.Invoke(); // 이거 없으면 지랄난거라 널체크하면 안될듯..
        OnShakedCallback = null;

        if(bShaked)
        {
            rule.Shake();
        }
    }

    public void OnChooseCard(CardBase chosenCard, CardGrid cardGrid) // 카드가 있던 곳임 리스트 지워줘야 해.
    {
        CardBase card = putCardQueue.Dequeue();
        OnScored(card,cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() => cardGrids.Find((x) => x.placedCards.Count == 0);

    public void OnScored(CardBase card, CardGrid cardGrid = null) // 카드를 얻을 때
    {
        CardManager.Instance.TransportCard(card);
        if (cardGrid != null) cardGrid.placedCards.Remove(card);
        targetUserData.ownCards.Add(card);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCardQueue.Enqueue(item);
        useCardList.Clear(); // 애는 이제 쓸 일 없음.
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

    public void PutCard(CardBase card) // 내가 낸 그리드를 가지고 있으면 좋을듯?ai.scoreData
    {
        SetTargetUserData();

        targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);
        List<CardBase> cardList = CardManager.Instance.GetSameMonthCards(card);

        if (targetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            targetGrid = GetNullGrid();
            OnShakedCallback = () => FollowCord(card);

            if (cardList.Count >= 3)
            {
                targetUserData.utilizeCards.Remove(card);
                UIManager.Instance.shakeUI.SetData(card, targetGrid);
                rule.Shake();
            }
            else
            {
                targetUserData.utilizeCards.Remove(card);
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
                    if (cardList.Count == 3) // 폭탄
                    {
                        foreach (var item in cardList) targetUserData.utilizeCards.Remove(item);
                        rule.Bomb(2); // 연출같은거 여기서?

                        OnScored(targetGrid.placedCards[0],targetGrid); // 바닥 패 먹고
                        while (cardList.Count > 0)
                        {
                            OnScored(cardList[0]);
                            cardList.RemoveAt(0);
                        }
                        TakeOtherPlayerCard();
                    }
                    else
                    {
                        targetUserData.utilizeCards.Remove(card);
                        card.MoveCardToGrid(targetGrid);
                    }
                    
                    break;
                case 2: // 두개 있으면 거기로 우선 가기 -> 따닥 가능성
                    if (cardList.Count == 2)
                    {
                        foreach (var item in cardList) targetUserData.utilizeCards.Remove(item);
                        rule.Bomb(1);

                        while (targetGrid.placedCards.Count > 0) OnScored(targetGrid.placedCards[0], targetGrid);

                        while (cardList.Count > 0)
                        {
                            OnScored(cardList[0]);
                            cardList.RemoveAt(0);
                        }
                        TakeOtherPlayerCard();
                    }
                    else
                    {
                        targetUserData.utilizeCards.Remove(card);

                        card.MoveCardToGrid(targetGrid);

                        choiceCallBackQueue.Enqueue(() => 
                            ChoiceCard(targetGrid.placedCards[0], targetGrid.placedCards[1], targetGrid));

                        putCardQueue.Enqueue(card);
                    }
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    targetUserData.utilizeCards.Remove(card);
                    while (targetGrid.placedCards.Count > 0)
                    {
                        OnScored(targetGrid.placedCards[0],targetGrid);
                    }

                    OnScored(card);
                    rule.Shake();
                    targetGrid.Reset();

                    TakeOtherPlayerCard();
                    break;
            }

            FollowCord(card);
        }

    }//함수의 끝.

    public void FollowCord(CardBase putCard)
    {
        CardBase followCard = followUpCardQueue.Dequeue();

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
                        rule.Kiss();
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // 두 개가 있다는 거면 둘 중 하나를 선택하게 하면 됨.
                    followCard.MoveCardToGrid(followCardTargetGrid);

                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth) //뻑임
                    {
                        rule.Paulk();
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
                        //따닥
                    }


                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    OnScored(followCard); // 내가 낸 것도 가져오기.
                    choiceCallBackQueue.Clear();
                    followCardTargetGrid.Reset();
                    TakeOtherPlayerCard();
                    break;
            }
        }
        CardManager.Instance.TakePairCard(targetGrid);
        TryExecuteChoiceCallback();


        targetGrid = null;
    }
}
