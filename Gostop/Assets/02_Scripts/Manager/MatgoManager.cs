using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MatgoManager : Singleton<MatgoManager>
{
    [HideInInspector] public List<CardBase> useCardList = new List<CardBase>(); // ���� ����� ��
    [HideInInspector] public List<CardBase> followUpCards = new List<CardBase>(); // ���б� �� �� �뵵

    public List<CardGrid> cardGrids = new List<CardGrid>(); // ī�带 �� ��ġ, ������ �ٸ��� �׳� �� ���� ã�Ƽ� ã�ư���
    [HideInInspector] public List<int> takenMonthList = new List<int>();  // ���� ��� �� ����

    public Queue<Action> takeCardCallbackQueue = new Queue<Action>();
    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // �ΰ��� �и� ġ��, ���߿� ������
    public Queue<CardBase> putCardQueue = new Queue<CardBase>(); // ����� �� �ֵ� �Ծ������ ��

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
        if (grid.placedCards.Count == 0)  // 2�� �� �������� �˾Ҵµ� �������� ��������. 
        {
            putCardQueue.Dequeue(); // �ѹ� �����ϸ� �ϳ� ����� ��
            TryExecuteChoiceCallback(); // ������ �ֳ� Ȯ�����ְ�.
        }
        else
        {
            if(isUserTurn)
            {
                isChoicing = true; // ���� �ൿ ��� �ʿ��� ��. �������� ���� �ƹ��͵� ���ϰ� -> �гη� ���Ƶα� ������ �� ���� ��� �ֳ�?
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

    public void OnChooseCard(CardBase chosenCard, CardGrid cardGrid) // ī�尡 �ִ� ���� ����Ʈ ������� ��.
    {
        CardBase card = putCardQueue.Dequeue();
        OnScored(card,cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() => cardGrids.Find((x) => x.placedCards.Count == 0);

    public void OnScored(CardBase card, CardGrid cardGrid = null) // ī�带 ���� ��
    {
        if(IsCardDoubleNine(card))
        {
            //UI ���� �������� ���� �Ƿ� ����
            // ��Ʈ�������� �̳Ѱ� �����ְ�,
            //���� �ڵ带 �ݹ����� �޴�, ��ư �Լ� �����

            // �ٵ� ���Ѱ� �װ� �ƴϴϱ� �켱 �Ȱ���..
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

        card.gameObject.SetActive(false); // ���� �����ϰŴϱ�.. UI�� ī�� ����ٰ� OnScored���� ���ֱ�.
        card.transform.position = cardGrid.transform.position;
        card.matchingCardObj = obj.gameObject;
        obj.MoveCardToGrid(cardGrid);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCards.Add(item);
        useCardList.Clear(); // �ִ� ���� �� �� ����.
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

    public void PutCard(CardBase card) // ���� �� �׸��带 ������ ������ ������?ai.scoreData
    {
        SetTargetUserdata();
        card.img.raycastTarget = false;
        targetGrid = null;
        targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        List<CardBase> cardList = CardManager.Instance.GetSameMonthCards(card);

        if (targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
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
        else // �� ��� �ڿ������� �� �е� ��.
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // �ϳ� ������ �� ģ������ �켱 ����. -> �� ���ɼ�
                    if (cardList.Count == 3) // ��ź
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
                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
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

                case 3: // 3�� ������ �� �������� 
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

    }//�Լ��� ��.

    public void FollowCord(CardBase putCard)
    {
        CardBase followCard = GetRandomCard(followUpCards);

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, followCard);

        if (followCardTargetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
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
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
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

                case 2: // �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    followCardTargetGrid.placedCards.Add(followCard);
                    MakeCardObj(followCardTargetGrid, followCard);

                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth) //����
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

                case 3: // 3�� ������ �� �������� 
                    followCardTargetGrid.placedCards.Add(followCard);
                    MakeCardObj(followCardTargetGrid, followCardTargetGrid.placedCards[0]);

                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth) // �տ��� 3���߿� �ϳ� ���� �׼� ���� �������
                    {
                        putCardQueue.Clear();
                        choiceCallBackQueue.Clear();
                    }

                    takeCardCallbackQueue.Enqueue(() =>
                    {
                        if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                        {
                            //����
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

        rule.Sweep();// �ݹ����� �־��ִ°Ŷ� ���� �־�� �� �� ..
        TryExecuteChoiceCallback();
    }

    public Action TakePairCard;
}
