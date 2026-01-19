using System.Collections.Generic;
using CommonData;
using UnityEngine;
using UnityEngine.InputSystem;

// 渡されたカードの一覧を表示するUI
public class CardListUI : MonoBehaviour
{
    [SerializeField] GameObject cardListUI;
    [SerializeField] CardIconScript cardIcon;
    [SerializeField] Transform cardIconParent;
    [SerializeField] Vector3 distanceLength;

    List<CardIconScript> cards;
    bool flag;
    Vector3 defultPos;

    void Update()
    {
        if (!flag) return;

        ScrollUI();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="num">カード一覧の最大カード枚数</param>
    public void Init(int num)
    {
        cardListUI.SetActive(false);
        defultPos = cardIconParent.position;

        cards = new List<CardIconScript> { cardIcon };
        for (int i = 1; i < num; i++)
        {
            var obj = Instantiate(cardIcon, cardIconParent);
            obj.gameObject.SetActive(false);
            cards.Add(obj);
        }
    }

    /// <summary>
    /// カード一覧を表示する
    /// </summary>
    public void OpenUI(List<CardData> cardDatas)
    {
        flag = true;
        cardListUI.SetActive(true);
        cardIconParent.position = defultPos;

        // 足りなければ追加する
        while (cards.Count < cardDatas.Count)
        {
            var obj = Instantiate(cardIcon, cardIconParent);
            obj.gameObject.SetActive(false);
            cards.Add(obj);
        }

        // データの整理
        Dictionary<CardData, int> cardIDs = new Dictionary<CardData, int>();
        foreach (CardData cardData in cardDatas)
        {
            // リストになければ追加
            if(!cardIDs.ContainsKey(cardData)) { cardIDs.Add(cardData, 0); continue; }
            // リストにあるならカウントを増やす
            cardIDs[cardData]++;
        }

        // 表示
        int count = 0;
        foreach (var card in cardIDs)
        {
            cards[count].gameObject.SetActive(true);
            cards[count].transform.localPosition = new Vector3(distanceLength.x * (count % 5), distanceLength.y * (count / 5), 0);
            cards[count].Init(card.Key, 0);
            cards[count].CountUp(card.Value+1);
            count++;
        }
        // 不必要なものを非表示
        for (int i = count; i < cards.Count; i++) {
            cards[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// UIを閉じる
    /// </summary>
    public void CloseUI()
    {
        flag = false;
        cardListUI.SetActive(false);
    }

    /* === スクロール === */
    void ScrollUI()
    {
        // スクロールのベクトルを取得（x, y）
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (scroll.y == 0f) return;

        cardIconParent.Translate(0, -scroll.y * 10, 0);

        if (cardIconParent.position.y < defultPos.y) cardIconParent.position = defultPos;
    }
}
