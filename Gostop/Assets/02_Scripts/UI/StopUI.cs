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
        MatgoManager.Instance.isGameFinished = true;

        if (MatgoManager.Instance.isUserTurn)
        {
            victoryOfDefeatText.text = "»ï¿¬»¶ ½Â¸®!";
        }
        else
        {
            victoryOfDefeatText.text = "»ï¿¬»¶ ÆÐ¹è..";
        }

        MatgoManager.Instance.saveManager.Save(MatgoManager.Instance.user.money);
    }

    public void OnStop(bool isPresident = false, bool isNagari = false)
    {
        moneyText.text = string.Empty;
        gameObject.SetActive(true);
        if (isNagari)
        {
            victoryOfDefeatText.text = "³ª°¡¸®, ¹¯°í ´õºí·Î!";
            MatgoManager.Instance.sc.nagariCount++;
        }
        else if(!isPresident)
        {
            if (MatgoManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "½Â¸®!";
                moneyText.text = $"+ {MatgoManager.Instance.sc.GetCalculatedScore()}¿ø";
            }
            else
            {
                victoryOfDefeatText.text = "ÆÐ¹è..";
                moneyText.text = $"- {MatgoManager.Instance.sc.GetCalculatedScore()}¿ø";
            }
            MatgoManager.Instance.sc.SetMoney();
        }
        else
        {
            if (MatgoManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "ÃÑÅë ½Â¸®!";
                moneyText.text = $"+ {MatgoManager.Instance.sc.GetPresidentMoney()}¿ø";
            }
            else
            {
                victoryOfDefeatText.text = "ÃÑÅë ÆÐ¹è..";
                moneyText.text = $"- {MatgoManager.Instance.sc.GetPresidentMoney()}¿ø";
            }
            MatgoManager.Instance.sc.SetMoney(10);
        }
        MatgoManager.Instance.saveManager.Save(MatgoManager.Instance.user.money);
    }


    public Button reStart;

    private void Start()
    {
        reStart.onClick.AddListener(() =>
        {
            MatgoManager.Instance.ReLoad();
        });
    }
}
