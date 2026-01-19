using System;
using System.Collections.Generic;
using CommonData;
using DG.Tweening;
using InGameData;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] float spaceWidth;
    [SerializeField] float maxWidth;
    [SerializeField] float dowmScale;
    [SerializeField] PlayerController playerController;
    [SerializeField] CardScript CardPrefab;      // カード.プレハブ
    [SerializeField] GameObject CardPearent;     // 生成する親元
    [SerializeField] GameObject handActivePanel; // 手札のアクティブ可.パネル
    [SerializeField] Transform activeCardAnchor; // 選択したカードが中心へと上る最終ポジション

    Dictionary<string, CardData> cardData;      // カード情報
    List<string> dekkiData;                     // デッキの中身
    List<CardScript> cardList;                  // 手札
    Dictionary<string, Action<string, SelectMode, CardData>> process;
    CharacterScript[] characters;
    WeponScript wepon;
    EquipmentScript equipment;
    ItemScript item;

    public bool skillPhase;
    public int useCardNum;
    int dekkiIdx;
    bool handActiveFlag;
    //-----------------------------------------------------------
    public List<Action<CardData>> useCardTimingAction; // カード使用時に行う処理
    //-----------------------------------------------------------


    public void Init(SaveData data, Dictionary<string, CardData> _cardData)
    {
        // カードデータ取得
        cardData = _cardData;

        wepon = new WeponScript();
        equipment = new EquipmentScript();
        item = new ItemScript();
        wepon.Init();
        equipment.Init();
        item.Init();

        // デッキの取得
        dekkiData = new List<string>();
        for (int i = 0; i < data.dekki.Count; i++)
        {
            dekkiData.Add(data.dekki[i]);
        }
        // デッキシャッフル
        for (int i = 0; i < dekkiData.Count; i++)
        {
            int num = UnityEngine.Random.Range(0, dekkiData.Count);
            string a = dekkiData[i];
            dekkiData[i] = dekkiData[num];
            dekkiData[num] = a;
        }

        useCardTimingAction = new List<Action<CardData>>();
        cardList = new List<CardScript>();
        process = new Dictionary<string, Action<string, SelectMode, CardData>>()
        {
            { "C", CharcarcterAction }, // キャラクタースキルカード
            { "W", WeponAction },       // 武器カード
            { "E", EquipmentAction },   // 装備カード
            { "I", ItemAction },        // アイテムカード
            { "N", NormalAction }         // アイテムカード
        };
        useCardNum = -1;
        handActiveFlag = true;
    }

    public void SetCharacter(CharacterScript[] _characters) { characters = _characters; }

    /// <summary> ドロー(複数枚可) </summary>
    /// <param name="count">引く枚数</param>
    public void Draw(int count)
    {
        List<CardScript> _cards = new List<CardScript>();
        for (int i = 0; i < count; i++)
        {
            CardScript card = Instantiate(CardPrefab, CardPearent.transform);

            string id = dekkiData[dekkiIdx];
            // キャラクタースキルカードなら
            if (id.Substring(0, 1) == "C") id = CharacterSkillID(id);

            card.Init(cardData[id], this);
            cardList.Add(card);
            _cards.Add(card);

            // デッキをループするようにする
            dekkiIdx = (dekkiIdx + 1) % dekkiData.Count;
        }
        SetPosCard(_cards);

        foreach (var _card in _cards)
        {
            Vector3 pos = _card.transform.localPosition;
            _card.transform.position -= Vector3.up * 400;
            _card.transform.DOLocalMove(pos, 0.3f);
        }
    }
    string CharacterSkillID(string id)
    {
        // 該当キャラクターのスキルカードではない
        int charID = int.Parse(id.Substring(1, 1));
        int skillID = int.Parse(id.Substring(3, 1));
        int typeID = 0;

        // キャラクターを検索、typeIDを取得する
        foreach (var character in characters)
        {
            if (character.charID != charID) continue;
            // 死亡していたら
            if (character.dedFlag) return "N001";
            typeID = character.GetTypeID();
        }

        return $"C{charID}{typeID + 1}{skillID}";
    }

    // ポジションセット
    void SetPosCard(List<CardScript> notAnimCards = null)
    {
        if (cardList == null || cardList.Count == 0) return;

        float width = spaceWidth * (cardList.Count - 1);
        if (width > maxWidth) width = maxWidth;
        float harfWidth = width / 2;

        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].SetIndex(i); // 表示の順番を設定
            //cardList[i].transform.localPosition = Vector3.zero;

            // 対象外カードが存在しないなら
            if(notAnimCards == null)
            {
                cardList[i].transform.localPosition = new Vector3(cardList[i].transform.localPosition.x, 0, 0);
                if(cardList.Count <= 1) cardList[i].transform.DOLocalMoveX(0, 0.1f); // 0で割らないため
                else cardList[i].transform.DOLocalMoveX(width * i / (cardList.Count - 1) - harfWidth, 0.1f);
                continue;
            }

            bool flg = false;
            // アニメーション対象外のカードか調べる
            foreach (var card in notAnimCards) {
                if (card == cardList[i]) { flg = true; break; }
            }

            if (cardList.Count <= 1) cardList[i].transform.DOLocalMoveX(0, 0.1f); // 0で割らないため
            // アニメーション
            if (flg && cardList.Count <= 1) cardList[i].transform.localPosition = new Vector3(0, 0, 0);
            else if (flg) cardList[i].transform.localPosition = new Vector3(width * i / (cardList.Count - 1) - harfWidth, 0, 0);
            else cardList[i].transform.DOLocalMoveX(width * i / (cardList.Count - 1) - harfWidth, 0.1f);
        }
    }

    // 手札にカーソルを合わせた時　拡大縮小
    public void HandActive()
    {
        // 自分のターンか確認
        if(!playerController.CheckTurn() || !skillPhase || handActiveFlag) return;

        CardPearent.transform.DOScale(Vector3.one, 0.08f);
        Vector3 endPos = CardPearent.transform.position + (Vector3.up * 120);
        CardPearent.transform.DOMove(endPos, 0.08f);
        handActivePanel.SetActive(false);

        handActiveFlag = true;
    }
    public void NotHandActive()
    {
        // カードを使用しようとしていたら
        if (useCardNum >= 0) { SetPosCard(); cardList[useCardNum].useFlag = false; }

        if (!handActiveFlag) return;

        CardPearent.transform.DOScale(Vector3.one * dowmScale, 0.08f);
        Vector3 endPos = CardPearent.transform.position + (Vector3.down * 120);
        CardPearent.transform.DOMove(endPos, 0.08f);
        handActivePanel.SetActive(true);

        handActiveFlag = false;
    }

    /// <summary> 使用したカードを削除する </summary>
    public void DestroyCard(out int ap)
    {
        ap = 0; // 通常攻撃AP

        if (useCardNum < 0) return;

        // カード使用時効果
        foreach (var action in useCardTimingAction) action(cardList[useCardNum].data);

        ap = cardList[useCardNum].data.Ap; // スキルAP

        // 使用したカードを削除
        if ((SelectMode)cardList[useCardNum].data.SelectId == SelectMode.None) FadeDestroy(cardList[useCardNum].gameObject);
        else Destroy(cardList[useCardNum].gameObject);
        cardList.RemoveAt(useCardNum);
        SetPosCard();

        useCardNum = -1;
    }
    void FadeDestroy(GameObject obj)
    {
        DOVirtual.DelayedCall(0.4f, () =>
        {
            obj.transform.DOScaleX(0, 0.2f).OnComplete(() => {
                Destroy(obj);
            });
        });
    }

    public void TypeChange(int _charID , int typeID)
    {
        foreach(var card in cardList)
        {
            var data = card.data;
            // キャラクタースキルカードではない
            if (data.CardId.Substring(0, 1) != "C") continue;

            // 該当キャラクターのスキルカードではない
            int charID = int.Parse(data.CardId.Substring(1, 1));
            if (charID != _charID) continue;

            int skillID = int.Parse(data.CardId.Substring(3, 1));
            string id = $"C{charID}{typeID+1}{skillID}";

            // 死亡キャラなら
            if (typeID < 0) id = "N001";

            card.TypeChange(cardData[id]);
        }
    }

    // カードのIDを取得
    // 先頭ID:Cでキャラクターにアクセス　キャラIDをもとにキャラクターを検索、アクセスする　末尾の数字を渡す
    public void ReceveCard(int cardNum, CardData data)
    {
        NotHandActive();

        useCardNum = cardNum;
        CardScript card = cardList[useCardNum];
        string cardType = data.CardId.Substring(0,1);
        string cardID = data.CardId.Substring(1);
        SelectMode mode = (SelectMode)data.SelectId;
        if(data.NotSelect) mode = SelectMode.None;

        if (!playerController.CheckAP(data.Ap)) return;

        Debug.Log(mode);
        // カード種別に処理を行う(ターゲット選択)
        process[cardType](cardID, mode, data);
        // カードを中央へ
        card.useFlag = true;
        card.transform.position = new Vector3(activeCardAnchor.position.x, card.transform.position.y, 0);
        card.transform.DOMove(activeCardAnchor.position, 0.2f);
    }

    /*** ----------------------------------------- カード種別 ------------------------------------------------ ***/
    // キャラクターカード C
    void CharcarcterAction(string id, SelectMode mode, CardData data)
    {
        int charID = int.Parse(id.Substring(0,1));
        int skillID = int.Parse(id.Substring(2,1));

        for (int i = 0; i < characters.Length; i++) {
            if (characters[i].charID != charID) continue;

            characters[i].SetSkillID(skillID, data);
            playerController.SelectMode(characters[i], mode);
            break;
        }
    }

    // 武器カード W
    void WeponAction(string id, SelectMode mode, CardData data)
    {
        int charID = int.Parse(id.Substring(0, 1));
        int skillID = int.Parse(id.Substring(1, 2));

        wepon.SetSkillID(charID, skillID-1, data);
        playerController.SelectMode(wepon, mode);
    }

    // 装備カード A
    void EquipmentAction(string id, SelectMode mode, CardData data)
    {
        int charID = int.Parse(id.Substring(0, 1));
        int skillID = int.Parse(id.Substring(1, 2));

        equipment.SetSkillID(charID, skillID-1, data);
        playerController.SelectMode(equipment, mode);
    }

    // アイテムカード I
    void ItemAction(string id, SelectMode mode, CardData data)
    {
        int charID = int.Parse(id.Substring(0, 1));
        int skillID = int.Parse(id.Substring(1, 2));

        item.SetSkillID(charID, skillID-1, data);
        playerController.SelectMode(item, mode);
    }

    // 効果を失ったカード
    void NormalAction(string id, SelectMode mode, CardData data)
    {
        // ドロー
        Draw(data.Value1);
        CharacterScript none = null;
        playerController.SelectMode(none, mode);
    }
}
