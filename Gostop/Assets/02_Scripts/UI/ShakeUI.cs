using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShakeUI : MonoBehaviour
{
    [HideInInspector] public CardBase clickedCard;
    [HideInInspector] public CardGrid grid;

    public void SetData(CardBase clickedCard, CardGrid grid)
    {
        MatgoManager.Instance.isShaking = true;
        gameObject.SetActive(true);

        this.clickedCard = clickedCard;
        this.grid = grid;
    }

    void ClearData()
    {
        grid = null;
        clickedCard = null;

        gameObject.SetActive(false);
        MatgoManager.Instance.isShaking = false;
    }

    public void Shake()
    {
        MatgoManager.Instance.OnShaked(clickedCard, grid);
        ClearData();
    }

    public void NotShake()
    {
        MatgoManager.Instance.OnShaked(clickedCard, grid, false);
        ClearData();
    }

}
