using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : CardBase
{
    public override void Init(CardDataSO cardData)
    {
        this.cardData = cardData;
        img.sprite = cardData.icon;
        this.name = cardData.name;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!GameManager.Instance.isUserTurn || GameManager.Instance.isChoicing || GameManager.Instance.isShaking) return;
        img.raycastTarget = false; ;
        GameManager.Instance.PutCard(this);
    }
}
