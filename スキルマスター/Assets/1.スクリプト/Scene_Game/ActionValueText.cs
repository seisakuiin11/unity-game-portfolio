using DG.Tweening;
using TMPro;
using UnityEngine;
using InGameData;
using System.Collections.Generic;

public class ActionValueText : MonoBehaviour
{
    [SerializeField] float moveY;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Color[] colors;

    Dictionary<ValueTextType, Color> valueColor;

    void Awake()
    {
        valueColor = new Dictionary<ValueTextType, Color>();
        for (int i = 0; i < colors.Length; i++)
        {
            valueColor.Add((ValueTextType)i, colors[i]);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int value, Vector3 pos, ValueTextType type)
    {
        transform.position = pos + Random.insideUnitSphere * 100;
        transform.parent = GameObject.Find("CreateRoot").transform;
        text.text = value.ToString();
        text.color = valueColor[type];

        transform.DOMoveY(pos.y + moveY, 0.3f);

        Destroy(this.gameObject, 0.6f);
    }
}
