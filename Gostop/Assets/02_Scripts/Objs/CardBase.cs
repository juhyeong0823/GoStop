using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

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
        grid.placedCards.Add(this);
        transform.DOMove(grid.transform.position, 0.15f, true).OnComplete(() => transform.parent = grid.transform);
    }
}
