using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBase : MonoBehaviour, IPointerDownHandler
{
    public CardDataSO cardData;

    [HideInInspector] public Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    public virtual void Init(CardDataSO cardData) { }


    public virtual void OnPointerDown(PointerEventData eventData) { }

    public void MoveCardToGrid(CardGrid grid)
    {
        this.transform.parent = grid.transform;
        grid.placedCards.Add(this);
    }
}
