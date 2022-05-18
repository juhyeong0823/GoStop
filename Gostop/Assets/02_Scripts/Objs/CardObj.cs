using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardObj : MonoBehaviour
{
    public CardDataSO cardData;

    public void Init(CardDataSO cardData)
    {
        this.cardData = cardData;
        GetComponent<SpriteRenderer>().sprite = cardData.icon   ;
      
    }

    public void MoveCardToGrid(CardGrid grid)
    {
        transform.DOMove(grid.transform.position, 0.15f, true).OnComplete(() =>
        {
            transform.parent = grid.transform;
            grid.GetComponent<SpriteLayout>().SortChildren();
        });
    }
}
