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

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // �ΰ��� �и� ġ��, ���߿� ������
    public Queue<CardBase> putCardQueue = new Queue<CardBase>(); // ����� �� �ֵ� �Ծ������ ��

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
        OnShakedCallback.Invoke(); // �̰� ������ �������Ŷ� ��üũ�ϸ� �ȵɵ�..
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

    public void PutCard(CardBase card) // ���� �� �׸��带 ������ ������ ������?ai.scoreData
    {
        SetTargetUserData();

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
                        rule.Bomb(2); // ���ⰰ���� ���⼭?

                        OnScored(targetGrid.placedCards[0],targetGrid); // �ٴ� �� �԰�
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
                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
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

                case 3: // 3�� ������ �� �������� 
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
                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        rule.Kiss();
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
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
                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        //����
                    }


                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    OnScored(followCard); // ���� �� �͵� ��������.
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
