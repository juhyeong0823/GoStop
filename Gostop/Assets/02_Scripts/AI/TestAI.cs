using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestAI
{
    public UserData userData = new UserData();

    public IEnumerator Turn()
    {
        yield return new WaitForSeconds(1f);
        MatgoManager.Instance.PutCard(MatgoManager.Instance.GetRandomCard(userData.utilizeCards));
    }
}
