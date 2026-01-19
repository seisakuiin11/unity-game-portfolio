using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("キャラアイコン")]
    [SerializeField] Image[] charaIcons;
    [SerializeField] Image[] enemyIcons;
    [Header("ステータスUI"), SerializeField]
    StatusUI statusUI;
    [Header("テキストメッセージ")]
    [SerializeField] GameObject message;
    [SerializeField] TextMeshProUGUI messageText;

    [SerializeField] GameObject DontPanel;
    [SerializeField] GameObject SelectTimingImg;

    List<CharacterScript> characters;
    List<EnemyScript> enemys;

    public void Init(List<CharacterScript> _characters, List<EnemyScript> _enemys)
    {
        characters = _characters;
        enemys = _enemys;

        DontPanel.SetActive(false);
        SelectTimingImg.SetActive(false);
        message.SetActive(false);
        UpdateData();

        statusUI.Init();
    }

    /// <summary> プレイヤーの操作を無効化 </summary>
    public void DontAction(bool flag) { DontPanel.SetActive(flag); }

    /// <summary> ターゲット選択時のシャドウエフェクト </summary>
    public void SelectTiming(bool flag) { SelectTimingImg.SetActive(flag); }

    /// <summary> アイコン情報の更新 </summary>
    public void UpdateData()
    {
        // アイコン画像の取得
        for (int i = 0; i < characters.Count; i++)
        {
            charaIcons[i].sprite = characters[i].CharaSprite;
            charaIcons[i].SetNativeSize();
        }

        // エネミーのアイコン
        for (int i = enemyIcons.Length - 1; i >= enemys.Count; i--)
        { // 不必要
            enemyIcons[i].transform.parent.gameObject.SetActive(false);
        }
        for (int i = 0; i < enemys.Count; i++)
        {
            enemyIcons[i].transform.parent.gameObject.SetActive(true);
            enemyIcons[i].sprite = enemys[i].CharaSprite;
            enemyIcons[i].SetNativeSize();
        }
    }

    public void ClickIcon_Chara(int index)
    {
        if (index >= characters.Count) return;
        statusUI.OpenStatusUI(characters[index]);
    }
    public void ClickIcon_Enemy(int index)
    {
        if (index >= enemys.Count) return;
        statusUI.OpenStatusUI(enemys[index]);
    }

    /// <summary>
    /// 画面上部にテキストメッセージを表示する
    /// </summary>
    public void TextMessage(bool flag, string text, float time = -1)
    {
        message.SetActive(flag);
        messageText.text = text;

        if (time <= 0) return;

        DOVirtual.DelayedCall(time, () => { message.SetActive(false); });
    }
}
