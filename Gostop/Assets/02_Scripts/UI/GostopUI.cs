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
        if (MatgoManager.Instance.isUserTurn)
        {
            goCountText.text = $"{MatgoManager.Instance.targetUser.scoreData.goCount + 1}°í";
            scoreText.text = $"½ºÅé ½Ã{MatgoManager.Instance.sc.GetCalculatedScore()}¿ø È¹µæ";
            this.gameObject.SetActive(true);
        }
        else
        {
            Stop();
        }
    }

    public void Go()
    {
        MatgoManager.Instance.targetUser.scoreData.goCount++;
        this.gameObject.SetActive(false);
        MatgoManager.Instance.SetTurn();
    }

    public void Stop(bool isNagari = false)
    {
        this.gameObject.SetActive(false);
        
        stopUI.OnStop(isNagari);
    }
}
