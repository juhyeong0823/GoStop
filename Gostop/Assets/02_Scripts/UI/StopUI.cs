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
            victoryOfDefeatText.text = "�￬�� �¸�!";
        }
        else
        {
            victoryOfDefeatText.text = "�￬�� �й�..";
        }

        MatgoManager.Instance.saveManager.Save(MatgoManager.Instance.user.money);
    }

    public void OnStop(bool isPresident = false, bool isNagari = false)
    {
        moneyText.text = string.Empty;
        gameObject.SetActive(true);
        if (isNagari)
        {
            victoryOfDefeatText.text = "������, ���� �����!";
            MatgoManager.Instance.sc.nagariCount++;
        }
        else if(!isPresident)
        {
            if (MatgoManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "�¸�!";
                moneyText.text = $"+ {MatgoManager.Instance.sc.GetCalculatedScore()}��";
            }
            else
            {
                victoryOfDefeatText.text = "�й�..";
                moneyText.text = $"- {MatgoManager.Instance.sc.GetCalculatedScore()}��";
            }
            MatgoManager.Instance.sc.SetMoney();
        }
        else
        {
            if (MatgoManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "���� �¸�!";
                moneyText.text = $"+ {MatgoManager.Instance.sc.GetPresidentMoney()}��";
            }
            else
            {
                victoryOfDefeatText.text = "���� �й�..";
                moneyText.text = $"- {MatgoManager.Instance.sc.GetPresidentMoney()}��";
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
