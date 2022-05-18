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
            myScoreText.text = $"{score}��";
            myMoneyText.text = $"���� �� : {MatgoManager.Instance.user.money}��";
        }
        else
        {
            aiScoreText.text = $"{score}��";
            //aiMoneyText.text = "�� ģ������ ���� ����. �� ������ ���� ���°Ŷ��..";
        }
    }



}
