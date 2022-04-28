using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GostopUI : MonoBehaviour
{
    public Text goCountText;

    public Text scoreText;

    public StopUI stopUI;
    
    public void AccomplishedCondition()
    {
        if (GameManager.Instance.isUserTurn)
        {
            goCountText.text = $"{GameManager.Instance.targetUserData.scoreData.goCount + 1}��";

            if (GameManager.Instance.isUserTurn)
            {
                scoreText.text = $"���� ��{GameManager.Instance.sc.GetCalculatedScore()}�� ȹ��";
            }
            else
            {
                scoreText.text = $"���� ��{GameManager.Instance.sc.GetCalculatedScore()}�� ȹ��";
            }

            this.gameObject.SetActive(true);
        }
        else
        {
            Stop();
        }
    }

    public void Go()
    {
        GameManager.Instance.targetUserData.scoreData.goCount++;
        this.gameObject.SetActive(false);
        GameManager.Instance.SetTurn();
    }

    public void Stop(bool isNagari = false)
    {
        this.gameObject.SetActive(false);
        
        stopUI.OnStop(isNagari);
    }
}
