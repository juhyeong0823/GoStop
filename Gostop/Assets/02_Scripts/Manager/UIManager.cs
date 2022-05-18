using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public ChoiceCardUI choiceUI;
    public ShakeUI shakeUI;
    public GostopUI gostopUI;


    public Text myScoreText;
    public Text aiScoreText;

    public Text myMoneyText;
    public Text aiMoneyText;

    public void SetDatas(bool isUserTurn, int score)
    {
        if(isUserTurn)
        {
            myScoreText.text = $"{score}Á¡";
            myMoneyText.text = $"°¡Áø µ· : {MatgoManager.Instance.user.money}¿ø";
        }
        else
        {
            aiScoreText.text = $"{score}Á¡";
            //aiMoneyText.text = "ÀÌ Ä£±¸¿¡°Õ µ·ÀÌ ¾ø¾î. ³Í °ÅÁöÀÇ µ·À» »°´Â°Å¶ó°í..";
        }
    }



}
