using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CommonData;

public class StatusUI : MonoBehaviour
{
    private static StatusUI instance;
    public static StatusUI Instance => instance;

    [Header("ステータスUI"), SerializeField]
    GameObject statusUI;
    [Header("キャラImg"), SerializeField]
    Image charaImg;
    [Header("テキストエリア"), SerializeField]
    GameObject textArea;
    [Header("キャラクターの名前"), SerializeField]
    TextMeshProUGUI charaName;
    [Header("覚醒タイプ"), SerializeField] 
    TextMeshProUGUI type;
    [Header("HP"), SerializeField] 
    TextMeshProUGUI hp;
    [Header("シールド"), SerializeField]
    TextMeshProUGUI silde;
    [Header("攻撃力"), SerializeField] 
    TextMeshProUGUI attack;
    [Header("魔法攻撃力"), SerializeField] 
    TextMeshProUGUI magicAttack;
    [Header("与ダメージ"), SerializeField] 
    TextMeshProUGUI dealtDamage;
    [Header("被ダメージ"), SerializeField] 
    TextMeshProUGUI takeDamage;

    [Header("効果")]
    [SerializeField] Transform effectBoxParent;
    [SerializeField] EffectTextBox effectBox;
    List<EffectTextBox> effectBoxs;

    bool openFlag;
    Vector3 defultPos;


    /// <summary> 初期化 </summary>
    public void Init()
    {
        // instanceがすでにあったら自分を消去する。
        if (instance && this != instance)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        effectBoxs = new List<EffectTextBox> { effectBox };
        defultPos = textArea.transform.position;

        statusUI.SetActive(false);
        if(silde) silde.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!openFlag) return;

        // スクロールのベクトルを取得（x, y）
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (scroll.y != 0f)
        {
            ScrollTextArea(scroll.y);
        }
    }

    /// <summary>
    /// ステータスUIを表示する
    /// </summary>
    public void OpenStatusUI(CharaData data, Sprite img)
    {
        openFlag = true;
        statusUI.SetActive(true);

        if (data == null) return;

        // テキストの代入 -------------------------------------------
        charaImg.sprite = img;
        charaImg.SetNativeSize();
        textArea.transform.position = defultPos;
        charaName.text = "名前" + data.Name;
        hp.text = data.HP.ToString();
        attack.text = data.Attack.ToString();
        magicAttack.text = data.MagicAttack.ToString();
        dealtDamage.text = data.Deal + "％";
        takeDamage.text = data.Take + "％";

        // 潜在覚醒のテキストボックスリスト-------------------------
        // テキストボックスが足りているか確認
        while (effectBoxs.Count < data.arousals.Count)
        {
            effectBoxs.Add(Instantiate(effectBox, effectBoxParent));
        }

        float height = 0;
        for (int i = 0; i < effectBoxs.Count; i++)
        {
            // 不必要なものを非表示
            if (i >= data.arousals.Count) { effectBoxs[i].gameObject.SetActive(false); continue; }

            effectBoxs[i].gameObject.SetActive(true);
            effectBoxs[i].transform.localPosition = Vector3.down * height;
            height += effectBoxs[i].SetData(0, "潜在覚醒：" + data.arousals[i].Name, data.arousals[i].Text);
        }
    }

    /// <summary>
    /// ステータスUIを表示する
    /// </summary>
    public void OpenStatusUI(CharacterScript target)
    {
        openFlag = true;
        statusUI.SetActive(true);

        if (target == null) return;

        // ステータスデータをセット
        charaImg.sprite = target.CharaSprite;
        charaImg.SetNativeSize();
        textArea.transform.position = defultPos;
        charaName.text = target.CharaName;
        type.text = "覚醒タイプ：" + target.ArousalType;
        hp.text = target.nowHp + " / " + target.maxHp;
        if(target.silde > 0) { silde.gameObject.SetActive(true); silde.text = "+" + target.silde; }
        else silde.gameObject.SetActive(false);
        attack.text = target.at.ToString();
        magicAttack.text = target.ma.ToString();
        dealtDamage.text = target.dealtDamageMultiplier + "％";
        takeDamage.text = target.takeDamageMultiplier + "％";

        // 効果リスト
        // 足りない分を生成する
        while (effectBoxs.Count < target.buffDatas.Count)
        {
            effectBoxs.Add(Instantiate(effectBox, effectBoxParent));
        }

        float height = 0;
        // データをセット
        for (int i = 0; i < effectBoxs.Count; i++)
        {
            // 不必要なものは非表示
            if (i >= target.buffDatas.Count) { effectBoxs[i].gameObject.SetActive(false); continue; }

            var data = target.buffDatas[i];
            effectBoxs[i].gameObject.SetActive(true);
            effectBoxs[i].transform.localPosition = Vector3.down * height;
            height += effectBoxs[i].SetData(data.turn, "・" + data.buffName, data.buffText);
        }
    }

    /// <summary>
    /// ステータスUIを非表示にする
    /// </summary>
    public void CloseStatusUI()
    {
        openFlag = false;
        statusUI.SetActive(false);
    }

    void ScrollTextArea(float value)
    {
        textArea.transform.Translate(0, -value*10, 0);

        if(textArea.transform.position.y < defultPos.y) textArea.transform.position = defultPos;
    }
}
