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
            victoryOfDefeatText.text = "�￬�� �¸�!";
        }
        else
        {
            victoryOfDefeatText.text = "�￬�� �й�..";
        }

        GameManager.Instance.saveManager.Save(GameManager.Instance.user.money);
    }

    public void OnStop(bool isPresident = false, bool isNagari = false)
    {
        moneyText.text = string.Empty;
        gameObject.SetActive(true);
        if (isNagari)
        {
            victoryOfDefeatText.text = "������, ���� �����!";
            GameManager.Instance.sc.nagariCount++;
        }
        else if(!isPresident)
        {
            if (GameManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "�¸�!";
                moneyText.text = $"+ {GameManager.Instance.sc.GetCalculatedScore()}��";
            }
            else
            {
                victoryOfDefeatText.text = "�й�..";
                moneyText.text = $"- {GameManager.Instance.sc.GetCalculatedScore()}��";
            }

            GameManager.Instance.sc.SetMoney();
            GameManager.Instance.saveManager.Save(GameManager.Instance.user.money);
        }
        else
        {
            if (GameManager.Instance.isUserTurn)
            {
                victoryOfDefeatText.text = "���� �¸�!";
                moneyText.text = $"+ {GameManager.Instance.sc.GetPresidentMoney()}��";
            }
            else
            {
                victoryOfDefeatText.text = "���� �й�..";
                moneyText.text = $"- {GameManager.Instance.sc.GetPresidentMoney()}��";
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
