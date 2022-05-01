using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StopUI : MonoBehaviour
{
    public Text victoryOfDefeatText;
    public Text moneyText;

    public void PaulkWin()
    {
        moneyText.text = string.Empty;
        gameObject.SetActive(true);
        GameManager.Instance.isGameFinished = true;

        if (GameManager.Instance.isUserTurn)
        {
            victoryOfDefeatText.text = "»ï¿¬»¶ ½Â¸®!";
        }
        else
        {
            victoryOfDefeatText.text = "»ï¿¬»¶ ÆÐ¹è..";
        }

        GameManager.Instance.saveManager.Save(GameManager.Instance.user.money);
    }

    public void OnStop(bool isPresident = false, bool isNagari = false)
    {
        moneyText.text = string.Empty;
        gameObject.SetActive(true);
        if (isNagari)
        {
            victoryOfDefeatText.text = "³ª°¡¸®, ¹¯°í ´õºí·Î!";
            GameManager.Instance.sc.nagariCount++;
        }
        else if(!isPresident)
        {
            if (GameManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "½Â¸®!";
                moneyText.text = $"+ {GameManager.Instance.sc.GetCalculatedScore()}¿ø";
            }
            else
            {
                victoryOfDefeatText.text = "ÆÐ¹è..";
                moneyText.text = $"- {GameManager.Instance.sc.GetCalculatedScore()}¿ø";
            }

            GameManager.Instance.sc.SetMoney();
            GameManager.Instance.saveManager.Save(GameManager.Instance.user.money);
        }
        else
        {
            if (GameManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "ÃÑÅë ½Â¸®!";
                moneyText.text = $"+ {GameManager.Instance.sc.GetPresidentMoney()}¿ø";
            }
            else
            {
                victoryOfDefeatText.text = "ÃÑÅë ÆÐ¹è..";
                moneyText.text = $"- {GameManager.Instance.sc.GetPresidentMoney()}¿ø";
            }
            GameManager.Instance.sc.SetMoney();
        }
        GameManager.Instance.saveManager.Save(GameManager.Instance.user.money);
    }


    public Button reStart;

    private void Start()
    {
        reStart.onClick.AddListener(() =>
        {
            GameManager.Instance.ReLoad();
        });
    }
}
