using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public List<CardBase> useCardList = new List<CardBase>(); // ���� ����� ��
    [HideInInspector] public Queue<CardBase> followUpCardQueue = new Queue<CardBase>(); // ���б� �� �� �뵵

    public List<CardGrid> cardGrids = new List<CardGrid>(); // ī�带 �� ��ġ, ������ �ٸ��� �׳� �� ���� ã�Ƽ� ã�ư���
    public List<int> takenMonthList = new List<int>();  // ���� ��� �� ����

    public Queue<Action> takeCardCallbackQueue = new Queue<Action>();
    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // �ΰ��� �и� ġ��, ���߿� ������
    public Queue<CardBase> putCardQueue = new Queue<CardBase>(); // ����� �� �ֵ� �Ծ������ ��

    public bool isChoicing = false;
    public bool isShaking = false;
    public bool isUserTurn = true;
    public bool isProcessingGame = false;


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
                takeCardCallbackQueue.Enqueue(() =>
                {
                    ai.userData.ownCards.Remove(takeCard);
                    user.ownCards.Add(takeCard);
                    takeCard.transform.DOMove(user.junkTrm.position, 0.15f, false);

                    CheckScore(ai.userData.ownCards, ai.userData.scoreData);
                    CheckScore(user.ownCards, user.scoreData);
                });
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
                takeCardCallbackQueue.Enqueue(() =>
                {
                    user.ownCards.Remove(takeCard);
                    ai.userData.ownCards.Add(takeCard);
                    takeCard.transform.DOMove(ai.userData.junkTrm.position, 0.15f, false);
                    CheckScore(ai.userData.ownCards, ai.userData.scoreData);
                    CheckScore(user.ownCards, user.scoreData);
                });
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
            CheckScore(targetUserData.ownCards, targetUserData.scoreData);
        }

    }

    public void SetTurn()
    {
        if (isUserTurn)
        {
            Debug.Log("������");
            isUserTurn = false;
            isProcessingGame = true;
            StartCoroutine(ai.Turn());
        }
        else
        {
            Debug.Log("AI��");
            isUserTurn = true;
            isProcessingGame = false;
        }
    }


    public void CheckScore(List<CardBase> checkCardList, ScoreData scoreData)
    {
        sc.CheckCards(checkCardList, scoreData);
        int score = sc.GetScore(scoreData);
        UIManager.Instance.SetDatas(isUserTurn, score);

        if(rule.CanGo(score, scoreData))
        {
            Debug.Log("CanGo");
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
            Debug.Log("Go����!");
            if (user.utilizeCards.Count == 0 && ai.userData.utilizeCards.Count == 0)
            {
                UIManager.Instance.gostopUI.stopUI.OnStop(true);
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
        OnShakedCallback.Invoke(); // FollowCard();
        OnShakedCallback = null;

        if(bShaked)
        {
            rule.Shake();
        }
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
        CardManager.Instance.TransportCard(card);
        if (cardGrid != null) cardGrid.placedCards.Remove(card);
        targetUserData.ownCards.Add(card);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCardQueue.Enqueue(item);
        useCardList.Clear(); // �ִ� ���� �� �� ����.
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

    public void PutTerm(List<CardBase> cards, CardGrid grid)
    {
        foreach(var item in cards)
        {
            item.MoveCardToGrid(grid);
        }
    }

    public void PutCard(CardBase card) // ���� �� �׸��带 ������ ������ ������?ai.scoreData
    {
        SetTargetUserData();

        targetGrid = null;
        targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);
        List<CardBase> cardList = CardManager.Instance.GetSameMonthCards(card);

        if (targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
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
        else // �� ��� �ڿ������� �� �е� ��.
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // �ϳ� ������ �� ģ������ �켱 ����. -> �� ���ɼ�
                    if (cardList.Count == 3) // ��ź
                    {
                        foreach (var item in cardList) targetUserData.utilizeCards.Remove(item);
                        PutTerm(cardList, targetGrid);

                        takeCardCallbackQueue.Enqueue(() =>
                        {
                            CardGrid grid = targetGrid;
                            rule.Bomb(2); // ���ⰰ���� ���⼭?

                            while (grid.placedCards.Count > 0)
                            {
                                OnScored(grid.placedCards[0], grid);
                                grid.placedCards.RemoveAt(0);
                            }
                            TakeOtherPlayerCard();
                        });
                        
                    }
                    else
                    {
                        targetUserData.utilizeCards.Remove(card);
                        card.MoveCardToGrid(targetGrid);
                    }
                    
                    break;
                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
                    if (cardList.Count == 2)
                    {
                        foreach (var item in cardList) targetUserData.utilizeCards.Remove(item);
                        PutTerm(cardList, targetGrid);

                        takeCardCallbackQueue.Enqueue(() =>
                        {
                            CardGrid grid = targetGrid;
                            rule.Bomb(1);

                            while (grid.placedCards.Count > 0)
                            {
                                OnScored(grid.placedCards[0], grid);
                            }
 
                            TakeOtherPlayerCard();
                        });
                       
                    }
                    else
                    {
                        targetUserData.utilizeCards.Remove(card);
                        card.MoveCardToGrid(targetGrid);

                        choiceCallBackQueue.Enqueue(() =>
                        {
                            CardGrid grid = targetGrid;
                            ChoiceCard(grid.placedCards[0], grid.placedCards[1], grid);
                        });
                            
                        putCardQueue.Enqueue(card);
                    }
                    break;

                case 3: // 3�� ������ �� �������� 
                    targetUserData.utilizeCards.Remove(card);
                    card.MoveCardToGrid(targetGrid);
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
        CardBase followCard = followUpCardQueue.Dequeue();

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, followCard);

        if (followCardTargetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            followCardTargetGrid = GetNullGrid();
            followCardTargetGrid.Set(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
                    followCard.MoveCardToGrid(followCardTargetGrid);
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
                    followCard.MoveCardToGrid(followCardTargetGrid);

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
                    followCard.MoveCardToGrid(followCardTargetGrid);

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
