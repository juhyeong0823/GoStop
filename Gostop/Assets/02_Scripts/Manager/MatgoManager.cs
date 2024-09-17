using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MatgoManager : Singleton<MatgoManager>
{
    [HideInInspector] public List<CardBase> useCardList = new List<CardBase>(); // 실제 사용할 패
    [HideInInspector] public List<CardBase> followUpCards = new List<CardBase>(); // 뒷패깔 때 쓸 용도

    public List<CardGrid> cardGrids = new List<CardGrid>(); // 카드를 깔 위치, 기존과 다르게 그냥 빈 곳을 찾아서 찾아간다
    [HideInInspector] public List<int> takenMonthList = new List<int>();  // 먹힌 놈들 월 저장

    public Queue<Action> takeCardCallbackQueue = new Queue<Action>();
    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // 두개의 패를 치고, 나중에 고르도록
    public Queue<CardBase> putCardQueue = new Queue<CardBase>(); // 골랐을 때 애도 먹어버리면 됨

    [SerializeField] private CardObj cardObjPrefab; 

    [HideInInspector] public bool isChoicing = false;
    [HideInInspector] public bool isShaking = false;
    [HideInInspector] public bool isUserTurn = true;
    [HideInInspector] public bool isGameProcessing = false;
    [HideInInspector] public bool isFirstChecking = true;
    [HideInInspector] public bool isGameFinished = false;

    public ScoreCounter sc = new ScoreCounter();
    public Rule rule = new Rule();
    public SaveManager saveManager = new SaveManager();

     public UserData user = new UserData();
     public TestAI ai = new TestAI();

    [HideInInspector] public UserData targetUser = null;
    [HideInInspector] public UserData restringUser = null;
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
        UserData taker = targetUser;
        UserData loser = restringUser;
        CardBase takeCard = CardFinder.FindJunkCard(loser.ownCards);

        if(takeCard != null)
        {
            takeCardCallbackQueue.Enqueue(() =>
            {
                loser.ownCards.Remove(takeCard);
                taker.ownCards.Add(takeCard);
                takeCard.transform.DOMove(taker.junkTrm.position, 0.15f, false).OnComplete(() => takeCard.transform.parent = taker.junkTrm);

                CheckScore(loser.ownCards, loser.scoreData);
                CheckScore(taker.ownCards, taker.scoreData);
            });
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
            TryExecuteTakeCardCallback();
        }
        
    }
    
    public void TryExecuteTakeCardCallback()
    {
        if (takeCardCallbackQueue.Count > 0)
        {
            takeCardCallbackQueue.Dequeue()?.Invoke();
            TryExecuteTakeCardCallback();
        }
        else
        {
            TakePairCard?.Invoke();
            CheckScore(targetUser.ownCards, targetUser.scoreData);

            if (rule.CanGo(sc.GetScore(targetUser.scoreData), targetUser.scoreData))
            {
                if (targetUser.utilizeCards.Count == 0)
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
                if (user.utilizeCards.Count == 0 && ai.userData.utilizeCards.Count == 0)
                {
                    UIManager.Instance.gostopUI.stopUI.OnStop(false,true);
                }
                else
                {
                    SetTurn();
                }
            }
        }

    }

    public void SetTurn()
    {
        if (isUserTurn)
        {
            isUserTurn = false;
            isGameProcessing = true;
            StartCoroutine(ai.Turn());
        }
        else
        {
            isUserTurn = true;
            isGameProcessing = false;
        }
    }

    public void CheckScore(List<CardBase> checkCardList, ScoreData scoreData)
    {
        sc.CheckCards(checkCardList, scoreData);
        int score = sc.GetScore(scoreData);
        UIManager.Instance.SetDatas(isUserTurn, score);

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
        MakeCardObj(cardGrid, clickedCard);
        OnShakedCallback.Invoke(); // FollowCard();
        OnShakedCallback = null;

        if(bShaked)
        {
            rule.Shake();
        }
    }

    public bool IsCardDoubleNine(CardBase card)
    {
        if((card.cardData.cardProperty & eProperty.Animal) != 0 && (card.cardData.cardProperty & eProperty.DoubleNine) != 0)
        {
            return true;
        }
        return false;
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
        if(IsCardDoubleNine(card))
        {
            //UI 띄우고 멍텅으로 쓸지 피로 쓸지
            // 비트연산으로 이넘값 지워주고,
            //밑의 코드를 콜백으로 받는, 버튼 함수 만들기

            // 근데 급한게 그게 아니니까 우선 똑같이..
            card.gameObject.SetActive(true);
            Destroy(card.matchingCardObj);
            cardGrid.GetComponent<SpriteLayout>().SortChildren();
            CardManager.Instance.TransportCard(card);
            if (cardGrid != null) cardGrid.placedCards.Remove(card);
            targetUser.ownCards.Add(card);
        }
        else
        {
            card.gameObject.SetActive(true);
            Destroy(card.matchingCardObj);
            cardGrid.GetComponent<SpriteLayout>().SortChildren();
            CardManager.Instance.TransportCard(card);
            if (cardGrid != null) cardGrid.placedCards.Remove(card);
            targetUser.ownCards.Add(card);
        }
    }
    
    public void MakeCardObj(CardGrid cardGrid, CardBase card)
    {
        CardObj obj = Instantiate(cardObjPrefab, Camera.main.ScreenToWorldPoint(card.transform.position), Quaternion.identity);
        obj.Init(card.cardData);

        card.gameObject.SetActive(false); // 내가 움직일거니까.. UI인 카드 꺼줬다가 OnScored에서 켜주기.
        card.transform.position = cardGrid.transform.position;
        card.matchingCardObj = obj.gameObject;
        obj.MoveCardToGrid(cardGrid);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCards.Add(item);
        useCardList.Clear(); // 애는 이제 쓸 일 없음.
    }

    public void SetTargetUserdata()
    {
        if (isUserTurn)
        {
            targetUser = user;
            restringUser = ai.userData;
        }
        else
        {
            targetUser = ai.userData;
            restringUser = user;
        }
    }

    CardGrid targetGrid = null;

    public Action OnShakedCallback;

    public void BombPutCard(List<CardBase> cards, CardGrid grid)
    {
        foreach(var card in cards)
        {
            grid.placedCards.Add(card);
            card.img.raycastTarget = false;
            MakeCardObj(grid, card);
            targetUser.utilizeCards.Remove(card);
        }
    }

    public void PutCard(CardBase card) // 내가 낸 그리드를 가지고 있으면 좋을듯?ai.scoreData
    {
        SetTargetUserdata();
        card.img.raycastTarget = false;
        targetGrid = null;
        targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        List<CardBase> cardList = CardManager.Instance.GetSameMonthCards(card);

        if (targetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            targetGrid = GetNullGrid();

            OnShakedCallback = () => FollowCord(card);

            if (cardList.Count >= 3)
            {
                targetUser.utilizeCards.Remove(card);
                UIManager.Instance.shakeUI.SetData(card, targetGrid);
                rule.Shake();
            }
            else
            {
                targetUser.utilizeCards.Remove(card);
                targetGrid.Set(card);

                MakeCardObj(targetGrid, card);
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
                        BombPutCard(cardList, targetGrid);

                        takeCardCallbackQueue.Enqueue(() =>
                        {
                            CardGrid grid = targetGrid;

                            while (grid.placedCards.Count > 0)
                            {
                                OnScored(grid.placedCards[0], grid);
                            }
                            grid.Reset();
                            TakeOtherPlayerCard();
                        });
                    }
                    else
                    {
                        targetUser.utilizeCards.Remove(card);
                        targetGrid.placedCards.Add(card);
                        MakeCardObj(targetGrid, card);
                    }
                    
                    break;
                case 2: // 두개 있으면 거기로 우선 가기 -> 따닥 가능성
                    if (cardList.Count == 2)
                    {
                        BombPutCard(cardList, targetGrid);

                        takeCardCallbackQueue.Enqueue(() =>
                        {
                            CardGrid grid = targetGrid;
                            rule.Bomb(1);

                            while (grid.placedCards.Count > 0)
                            {
                                OnScored(grid.placedCards[0], grid);
                            }
                            grid.Reset();
                            TakeOtherPlayerCard();
                        });
                    }
                    else
                    {
                        targetUser.utilizeCards.Remove(card);
                        targetGrid.placedCards.Add(card);
                        MakeCardObj(targetGrid, card);

                        choiceCallBackQueue.Enqueue(() =>
                        {
                            CardGrid grid = targetGrid;
                            ChoiceCard(grid.placedCards[0], grid.placedCards[1], grid);
                        });
                            
                        putCardQueue.Enqueue(card);
                    }
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    targetUser.utilizeCards.Remove(card);
                    targetGrid.placedCards.Add(card);
                    MakeCardObj(targetGrid, card);

                    takeCardCallbackQueue.Enqueue(() =>
                    {
                        CardGrid grid = targetGrid;
                        rule.Shake();

                        while (grid.placedCards.Count > 0)
                        {
                            OnScored(grid.placedCards[0], grid);
                        }

                        grid.Reset();
                        TakeOtherPlayerCard();
                    });
                    break;
            }

            FollowCord(card);
        }

    }//함수의 끝.

    public void FollowCord(CardBase putCard)
    {
        CardBase followCard = GetRandomCard(followUpCards);

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, followCard);

        if (followCardTargetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            followCardTargetGrid = GetNullGrid();
            followCardTargetGrid.Set(followCard);
            MakeCardObj(followCardTargetGrid, followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 뒷패를 깠는데 하나만 있다는 거면, 무조건 가져오면 됨.
                    followCardTargetGrid.placedCards.Add(followCard);
                    MakeCardObj(followCardTargetGrid, followCard);

                    takeCardCallbackQueue.Enqueue(() =>
                    {
                        if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                        {
                            rule.Kiss();
                        }

                        while (followCardTargetGrid.placedCards.Count > 0)
                        {
                            OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                        }

                        followCardTargetGrid.Reset();
                    });
                    
                    break;

                case 2: // 두 개가 있다는 거면 둘 중 하나를 선택하게 하면 됨.
                    followCardTargetGrid.placedCards.Add(followCard);
                    MakeCardObj(followCardTargetGrid, followCard);

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
                    followCardTargetGrid.placedCards.Add(followCard);
                    MakeCardObj(followCardTargetGrid, followCardTargetGrid.placedCards[0]);

                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth) // 앞에서 3개중에 하나 고르는 액션 삭제 해줘야함
                    {
                        putCardQueue.Clear();
                        choiceCallBackQueue.Clear();
                    }

                    takeCardCallbackQueue.Enqueue(() =>
                    {
                        if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                        {
                            //따닥
                        }

                        while (followCardTargetGrid.placedCards.Count > 0)
                        {
                            OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                        }

                        choiceCallBackQueue.Clear();
                        followCardTargetGrid.Reset();
                        TakeOtherPlayerCard();
                    });
                    
                    break;
            }
        }
        TakePairCard = (() =>
        {
            CardGrid grid = targetGrid;
            CardManager.Instance.TakePairCard(grid);
        });

        rule.Sweep();// 콜백으로 넣어주는거라 지금 있어야 할 듯 ..
        TryExecuteChoiceCallback();
    }

    public Action TakePairCard;
}
