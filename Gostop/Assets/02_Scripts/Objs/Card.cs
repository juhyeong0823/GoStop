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
        if (GameManager.Instance.isProcessingGame)
        {
            Debug.Log("돌아가요~");
            return;
        }

        img.raycastTarget = false;
        GameManager.Instance.PutCard(this);
    }
}
