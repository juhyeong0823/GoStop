using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShakeUI : MonoBehaviour
{
    [SerializeField] private Image lCardImg;
    [SerializeField] private Image centerCardImg;
    [SerializeField] private Image rCardImg;

    [HideInInspector] public Card lCard;
    [HideInInspector] public Card centerCard;
    [HideInInspector] public Card rCard;
    [HideInInspector] public CardGrid grid;

    public void SetData(Card lCard, Card centerCard, Card rCard, CardGrid grid)
    {
        gameObject.SetActive(true);

        this.lCard = lCard;
        this.centerCard = centerCard;
        this.rCard = rCard;
        this.grid = grid;

        lCardImg.sprite = this.lCard.cardData.icon;
        centerCardImg.sprite = this.centerCard.cardData.icon;
        rCardImg.sprite = this.rCard.cardData.icon;
    }

    void ClearData()
    {
        lCard = null;
        centerCard = null;
        rCard = null;
        grid = null;

        lCardImg.sprite = null;
        centerCardImg.sprite = null;
        rCardImg.sprite = null;

        gameObject.SetActive(false);
    }

    public void ChooseLCard()
    {
        GameManager.Instance.OnShaked(lCard, grid);
        ClearData();
    }

    public void ChooseCenterCard()
    {
        GameManager.Instance.OnShaked(centerCard, grid);
        ClearData();
    }

    public void ChooseRCard()
    {
        GameManager.Instance.OnShaked(rCard, grid);
        ClearData();
    }
}
