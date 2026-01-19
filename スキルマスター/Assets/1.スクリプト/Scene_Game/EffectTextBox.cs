using TMPro;
using UnityEngine;

public class EffectTextBox : MonoBehaviour
{
    [Header("縦幅"), SerializeField]
    float height;
    [Header("継続ターン"), SerializeField]
    TextMeshProUGUI turn;
    [Header("効果名"), SerializeField]
    TextMeshProUGUI title;
    [Header("効果内容"), SerializeField]
    TextMeshProUGUI dic;

    public float SetData(int _turn, string _title, string _dic)
    {
        if(_turn < 0) turn.text = "残り - ターン";
        else turn.text = $"残り{_turn}ターン";
        title.text = _title;
        dic.text = _dic;

        return height;
    }
}
