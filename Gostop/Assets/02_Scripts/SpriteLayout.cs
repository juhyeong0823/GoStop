using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLayout : MonoBehaviour
{
    private const int baseLayer = 5;
    public List<Transform> children = new List<Transform>();
    [SerializeField] private float spacing;

    private void Start()
    {
        SortChildren();
    }

    public void SortChildren()
    {
        children = new List<Transform>(GetComponentsInChildren<Transform>());
        children.RemoveAt(0);
        for (int i = 0; i < children.Count; i++)
        {
            children[i].position = transform.position;
            children[i].Translate(new Vector2(i * spacing, 0));
            children[i].GetComponent<SpriteRenderer>().sortingOrder = baseLayer + i;
        }
    }

}
