using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectTextBox : MonoBehaviour
{
    [Header("縦幅"), SerializeField]
    float titleHeight;
    [Header("継続ターン"), SerializeField]
    TextMeshProUGUI turn;
    [Header("効果名"), SerializeField]
    TextMeshProUGUI title;
    [Header("効果内容"), SerializeField]
    TextMeshProUGUI dic;
    [Header("効果内容背景画像"), SerializeField]
    RectTransform textBoxImg;

    public float SetData(int _turn, string _title, string _dic)
    {
        if(_turn < 0) turn.text = "残り - ターン";
        else turn.text = $"残り{_turn}ターン";
        title.text = _title;

        dic.text = _dic;
        dic.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(textBoxImg);
        float height = titleHeight + textBoxImg.rect.height;

        return height;
    }
}
