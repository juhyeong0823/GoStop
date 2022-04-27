using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerDownHandler
{
    public CardDataSO cardData;

    GameManager gm;

    public Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        gm = GameManager.Instance;
    }

    public void Init(CardDataSO cardData)
    {
        this.cardData = cardData;
        this.GetComponent<Image>().sprite = cardData.icon;
        this.name = cardData.name;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(GameManager.Instance.isUserTurn)
        {
            gm.PutCard(this);
        }
    }

    public void MoveCardToGrid(CardGrid grid)
    {
        this.transform.parent = grid.transform;
        grid.placedCards.Add(this);
    }
}
