using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Card> placedCard = new List<Card>(); // 바닥에 깔려있는 카드

    public List<Card> allCards = new List<Card>(); // 복사해서 쓸 리스트

    public List<Card> useCardList = new List<Card>(); // 실제 사용할 패
    public Queue<Card> followUpCardQueue = new Queue<Card>(); // 뒷패깔 때 쓸 용도

    public List<CardGrid> cardGrids = new List<CardGrid>(); // 카드를 깔 위치, 기존과 다르게 그냥 빈 곳을 찾아서 찾아간다

    public List<Card> myCards = new List<Card>();
    public List<Card> othersCards = new List<Card>();

    [Header("내 피들 놓을 위치")]
    [SerializeField] private Transform myPeeTrm;    
    [SerializeField] private Transform myBandTrm;    
    [SerializeField] private Transform myGwangTrm;    
    [SerializeField] private Transform myAnimalsTrm;

    [Space(15)]

    [Header("상대 피들 놓을 위치")]
    [SerializeField] private Transform othersPeeTrm;    
    [SerializeField] private Transform othersBandTrm;    
    [SerializeField] private Transform othersGwangTrm;    
    [SerializeField] private Transform othersAnimalsTrm;    

    public CardGrid GetNullGridPos() // 빈 곳 받아오기
    {
        return cardGrids.Find((x) => x.bPlacedCardIsExist == false); // 설치된 친구가 있는지 없는지.
    }

    public void OnScored(Card card) // 카드를 얻을 때
    {
        // 종류에 따라 위치 분류해주고, 부모 다르게 해줘야 함 그리드 레이아웃으로 처리할거야 겹쳐있는거

        myCards.Add(card);
    }
}
