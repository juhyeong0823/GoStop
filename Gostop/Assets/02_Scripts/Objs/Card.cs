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
        if (MatgoManager.Instance.isGameProcessing)
        {
            Debug.Log("���ư���~");
            return;
        }

        MatgoManager.Instance.PutCard(this);
    }
}
