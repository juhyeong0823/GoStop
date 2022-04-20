using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ScriptableObjects/CardData")]
public class CardDataSO : ScriptableObject
{
    public eProperty cardProperty;

    public int cardMonth;
    public Sprite icon;
}
