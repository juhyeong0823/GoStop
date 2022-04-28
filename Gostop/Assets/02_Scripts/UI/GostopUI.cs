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
            goCountText.text = $"{GameManager.Instance.targetUserData.scoreData.goCount + 1}°í";

            if (GameManager.Instance.isUserTurn)
            {
                scoreText.text = $"½ºÅé ½Ã{GameManager.Instance.sc.GetCalculatedScore()}¿ø È¹µæ";
            }
            else
            {
                scoreText.text = $"½ºÅé ½Ã{GameManager.Instance.sc.GetCalculatedScore()}¿ø È¹µæ";
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
