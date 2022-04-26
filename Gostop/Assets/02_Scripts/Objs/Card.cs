using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerDownHandler
{
    public CardDataSO cardData;

    GameManager gm;

    private void Awake()
    {
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
        gm.PutCard(this);

    }
}
