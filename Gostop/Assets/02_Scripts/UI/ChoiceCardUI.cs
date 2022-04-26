using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceCardUI : MonoBehaviour
{
    
    [SerializeField] private Image lCardImg;
    [SerializeField] private Image rCardImg;

    [HideInInspector] public Card lCard;
    [HideInInspector] public Card rCard;
    [HideInInspector] public CardGrid grid;

    public void SetData(Card lCard, Card rCard, CardGrid grid)
    {
        this.lCard = lCard;
        this.rCard = rCard;
        this.grid = grid;

        lCardImg.sprite = this.lCard.cardData.icon;
        rCardImg.sprite = this.rCard.cardData.icon;
        gameObject.SetActive(true);
    }

    void ClearData()
    {
        this.lCard =  null;
        this.rCard = null;
        this.grid = null;

        lCardImg.sprite = null;
        rCardImg.sprite = null;

        GameManager.Instance.isChoicing = false;
        gameObject.SetActive(false);

    }

    public void ChooseLCard()
    {
        GameManager.Instance.OnChooseCard(lCard, grid);
        ClearData();
    }

    public void ChooseRCard()
    {
        GameManager.Instance.OnChooseCard(rCard, grid);
        ClearData();
    }
}
