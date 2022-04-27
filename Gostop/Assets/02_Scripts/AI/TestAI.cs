using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestAI
{
    public UserData userData = new UserData();

    public void Turn()
    {
        Debug.Log("¤¾¤·");
        GameManager.Instance.PutCard(GameManager.Instance.GetRandomCard(userData.utilizeCards));
    }
}
