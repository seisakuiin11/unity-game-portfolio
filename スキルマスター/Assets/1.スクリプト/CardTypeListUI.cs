using CommonData;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardTypeListUI : MonoBehaviour
{
    [SerializeField] GameObject cardTypeListUI;
    [SerializeField] CardIconScript cardPrefab;
    [SerializeField] Transform cardParent;
    [SerializeField] float cardSpace;

    List<CardIconScript> cards;
    Dictionary<string, CardData> cardDatas;
    List<CharaData> charaDatas;

    void Start()
    {
        Init();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        cards = new List<CardIconScript>() { cardPrefab };
        var dataBase = FindAnyObjectByType<DataBaseManager>();
        cardDatas = dataBase.GetCardData();
        charaDatas = dataBase.GetCharaData();
        cardTypeListUI.SetActive(false);
    }

    /// <summary>
    /// UIを表示する
    /// </summary>
    public void OpenUI(string cardID)
    {
        cardTypeListUI.SetActive(true);

        // カードタイプを判定　キャラカードなら潜在覚醒の数に応じて枚数を増やす
        string cardType = cardID.Substring(0, 1);

        // キャラクタースキルカードではなければ ---------------------------------------------
        if(cardType != "C")
        {
            cards[0].gameObject.SetActive(true);
            cards[0].transform.localPosition = Vector3.zero;
            cards[0].Init(cardDatas[cardID], 0);

            // 不必要なものは非表示
            for (int i = 1; i < cards.Count; i++) cards[i].gameObject.SetActive(false);

            return;
        } // --------------------------------------------------------------------------------

        // カードIDを分割
        int charID = int.Parse(cardID.Substring(1, 1));
        int skillID = int.Parse(cardID.Substring(3, 1));
        int arousalCount = charaDatas[charID - 1].arousals.Count;
        float harfWidth = cardSpace * (arousalCount-1) / 2;

        // 必要カード数を用意　均等配置
        while(cards.Count < arousalCount) cards.Add(Instantiate(cardPrefab, cardParent));

        // 必要データを渡す
        for(int i = 0; i < cards.Count; i++)
        {
            // 不必要なものは非表示
            if(i >= arousalCount) { cards[i].gameObject.SetActive(false); continue; }

            cards[i].gameObject.SetActive(true);
            cards[i].transform.localPosition = new Vector3(i * cardSpace - harfWidth, 0, 0);
            string id = $"C{charID}{i+1}{skillID}";
            cards[i].Init(cardDatas[id], 0);

            // タイプ名の記載
            var nameText = cards[i].transform.Find("TypeName").GetComponent<TextMeshProUGUI>();
            nameText.text = "タイプ：" + charaDatas[charID - 1].arousals[i].Name;
        }
    }

    /// <summary>
    /// UIを非表示にする
    /// </summary>
    public void CloseUI()
    {
        cardTypeListUI.SetActive(false);
    }
}
