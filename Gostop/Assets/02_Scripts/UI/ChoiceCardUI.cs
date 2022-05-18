using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceCardUI : MonoBehaviour
{
    [SerializeField] private Image lCardImg;
    [SerializeField] private Image rCardImg;

    [HideInInspector] public CardBase lCard;
    [HideInInspector] public CardBase rCard;
    [HideInInspector] public CardGrid grid;

    public GameObject choiceDoubleNineUI;
    public CardBase doubleNineCard;

    public void SetData(CardBase lCard, CardBase rCard, CardGrid grid)
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

        MatgoManager.Instance.isChoicing = false;
        gameObject.SetActive(false);

    }

    public void ChooseLCard()
    {
        MatgoManager.Instance.OnChooseCard(lCard, grid);
        ClearData();
    }

    public void ChooseRCard()
    {
        MatgoManager.Instance.OnChooseCard(rCard, grid);
        ClearData();
    }

    public void ChoiceDoubleNine(CardBase doubleNine)
    {
        choiceDoubleNineUI.gameObject.SetActive(true);
        doubleNineCard = doubleNine;
    }

    public void OnChoiceDoubleNine(bool useAsJunk)
    {
        if (useAsJunk)
        {
            //5doubleNineCard.cardData.cardProperty = 
        }
        else
        {

        }
    }
}
