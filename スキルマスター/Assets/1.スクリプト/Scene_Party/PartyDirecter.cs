using CommonData;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PartyDirecter : MonoBehaviour
{
    public enum State
    {
        Main,
        Member,
        Card,
    }

    [Header("各種シーン")]
    [SerializeField] GameObject mainScene;
    [SerializeField] GameObject MemberScene;
    [SerializeField] GameObject CardScene;
    Dictionary<State, GameObject> scene;
    State state;
    [Header("フェイドパネル"), SerializeField] GameObject fadePanel;

    [System.Serializable]
    class CharaWindow
    {
        public RectTransform rectTransform;
        public Image charaImg;
        public TextMeshProUGUI charaText;

        public void SetData(int charaID, Sprite img = null, string text = "")
        {
            // キャラIDが設定されていなければ
            if (charaID < 0)
            {
                charaImg.sprite = null;
                charaImg.gameObject.SetActive(false);
                charaText.text = "";
                return;
            }

            charaImg.gameObject.SetActive(true);
            charaImg.sprite = img;
            charaImg.SetNativeSize();
            charaText.text = text;
        }
    }
    [Header("メイン画面")]
    [SerializeField] CharaWindow[] mainCW;
    [SerializeField] TextMeshProUGUI partyName;
    [Header("ステータスUI")]
    [SerializeField] StatusUI statusUI;
    [SerializeField] CardListUI cardListUI;

    [Header("メンバー設定画面")]
    [SerializeField] CharaWindow[] memberSetCW;
    [SerializeField] CharaIconScript charaIconPrefab;
    [SerializeField] Transform charaIconParent;

    [Header("デッキ編集画面")]
    [SerializeField] Image[] charaIcons;
    [SerializeField] CardIconScript cardPrefab;
    [SerializeField] Transform cardListParent;
    [SerializeField] RectTransform cardListArea;
    [SerializeField] Transform dekkiListParent;
    [SerializeField] RectTransform dekkiListArea;
    [SerializeField] TextMeshProUGUI dekkiCountText;
    int[] charaIDList;
    Dictionary<string, CardIconScript> cardList;
    Dictionary<string, CardIconScript> dekkiList;

    List<CharaData> charaDatas;
    Dictionary<string, CardData> cardDatas;
    SaveData[] saveDatas = new SaveData[5];
    SaveData data;
    int index;
    int dekkiCount;
    bool statusUIFlag;

    void Awake()
    {
        scene = new Dictionary<State, GameObject>() {
            { State.Main, mainScene },
            { State.Member, MemberScene },
            { State.Card, CardScene },
        };
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (statusUIFlag) return;

        if(state == State.Card)
        {
            ScrollCardList();
            ScrollDekkiList();
        }
    }

    void Init()
    {
        DataBaseManager dataBase = FindAnyObjectByType<DataBaseManager>();
        charaDatas = dataBase.GetCharaData();
        cardDatas = dataBase.GetCardData();
        var datas = dataBase.LoadAll().ToArray();
        int count = 0;
        foreach(var sd in datas) { saveDatas[count] = sd; count++; }
        if(count < saveDatas.Length) saveDatas[count] = new SaveData() { charaID = new int[3], dekki = new List<string>() };

        MainSceneInit();
        MemberSceneInit();
        DekkiSceneInit();

        fadePanel.SetActive(true);
        fadePanel.transform.localScale = new Vector3(0, 1, 1);
        fadePanel.transform.position = new Vector3(0, fadePanel.transform.position.y, 0);

        statusUI.Init();
        cardListUI.Init(40); // デッキの最大数

        foreach (var vl in scene) vl.Value.SetActive(false);
        state = State.Main;
        scene[state].SetActive(true);
    }

    /* --- 前のシーンに戻る --- */
    public void BackScene()
    {
        // ステータスUIを表示していたら
        if(statusUIFlag) { statusUIFlag = false; statusUI.CloseStatusUI(); return; }

        // ロビーへ戻る
        if(state == State.Main) { GameManager.Instance.LoadScene(GameManager.Scene.Lobby); return; }

        ChangeScene(state - 1);
    }

    async void ChangeScene(State _state)
    {
        fadePanel.transform.localScale = new Vector3(0, 1, 1);
        fadePanel.transform.position = new Vector3(0, fadePanel.transform.position.y, 0);
        fadePanel.transform.DOScaleX(1, 0.3f);
        fadePanel.transform.DOMoveX(Screen.width / 2, 0.3f);

        await Task.Delay(300); // 0.2秒

        scene[state].SetActive(false);
        state = _state;
        scene[state].SetActive(true);

        fadePanel.transform.DOScaleX(0, 0.3f);
        fadePanel.transform.DOMoveX(Screen.width, 0.3f);
    }

    bool CheckDropBox(Vector3 point, RectTransform rect)
    {
        Vector3[] posList = new Vector3[4];
        rect.GetWorldCorners(posList);

        if(point.x < posList[0].x || point.x > posList[2].x) return false;
        if(point.y < posList[0].y || point.y > posList[2].y) return false;

        return true;
    }

    // メインシーン ===============================================================================
    void MainSceneInit()
    {
        index = 0;
        SetPartyData(0);
    }
    
    /* --- パーティー編集 --- */
    public void EditStart()
    {
        // シーン遷移
        ChangeScene(State.Member);

        data = saveDatas[index];
        partyName.text = "パーティー." + (index + 1);
        // 次のシーンの画像,名前を設定
        for (int i = 0; i < memberSetCW.Length; i++)
        {
            int charaNum = data.charaID[i] - 1;

            // キャラIDが設定されていなければ
            if (charaNum < 0) memberSetCW[i].SetData(charaNum);
            else memberSetCW[i].SetData(charaNum, charaDatas[charaNum].arousals[0].img, charaDatas[charaNum].Name);
        }
    }

    /* --- パーティー情報をセット --- */
    public void SetPartyData(int num)
    {
        // 何番目の編成データか(Null除外)
        do {
            index = (index + num) % saveDatas.Length;
            if (index < 0) index = saveDatas.Length - 1;
        } while (saveDatas[index] == null);

        SaveData _data = saveDatas[index];
        partyName.text = "パーティー." + (index+1);
        // 画像,名前を設定
        for (int i = 0; i < mainCW.Length; i++) {
            int charaNum = _data.charaID[i] - 1;

            // キャラIDが設定されていなければ
            if (charaNum < 0) mainCW[i].SetData(charaNum);
            else mainCW[i].SetData(charaNum, charaDatas[charaNum].arousals[0].img, charaDatas[charaNum].Name);
        }
    }

    /* --- キャラクター情報を表示 --- */
    public void OpenStatusUI(int num)
    {
        // メインシーンの場合、パーティーの何番目かが渡ってくる
        if(state == State.Main) { num = saveDatas[index].charaID[num]-1; }

        statusUIFlag = true;
        statusUI.OpenStatusUI(charaDatas[num], charaDatas[num].arousals[0].img);
    }

    /* --- カード情報を表示 --- */
    public void OpenCardListUI()
    {
        List<CardData> cards = new List<CardData>();
        foreach(var id in saveDatas[index].dekki) cards.Add(cardDatas[id]);
        cardListUI.OpenUI(cards);
    }

    // メンバー編集シーン =============================================================================
    void MemberSceneInit()
    {
        charaIconParent.position += Vector3.left * 300 * (charaDatas.Count-1) / 2;
        // キャラアイコンを生成
        for (int i = 0; i < charaDatas.Count; i++)
        {
            var obj = Instantiate(charaIconPrefab, charaIconParent);
            obj.transform.position = charaIconParent.position;
            obj.transform.position += Vector3.right * 300 * i;

            obj.Init(i, charaDatas[i].arousals[0].img, charaDatas[i].Name, charaIconPrefab, this);
        }
        charaIconPrefab.enabled = false;
    }

    /* --- メンバー決定 --- */
    public void SaveMember()
    {
        // シーン遷移
        ChangeScene(State.Card);

        charaIDList = data.charaID;
        UpdateCardListPos();

        // アイコンのキャラクターを変更 ----------------------------------
        for(int i = 0; i < charaIcons.Length; i++)
        {
            int charID = data.charaID[i % 3] - 1;
            charaIcons[i].sprite = charaDatas[charID].arousals[0].img;
            charaIcons[i].SetNativeSize();
        }

        // デッキ生成 ----------------------------------
        dekkiCount = 0;
        foreach (var id in data.dekki)
        {
            // キャラクタースキルカードなら、該当キャラクターがパーティーにいるか確認
            if (id.Substring(0, 1) == "C")
            {
                int charID = int.Parse(id.Substring(1, 1));

                // パーティー内のキャラクターのスキルか確認
                bool flag = false;
                foreach (var i in charaIDList) { if (i == charID) { flag = true; break; } }
                if (!flag) continue;
            }

            // なければ生成
            if (!dekkiList.ContainsKey(id))
            {
                var obj = Instantiate(cardPrefab, dekkiListParent);
                obj.enabled = true;
                obj.Init(cardDatas[id], 2, cardPrefab, this);
                dekkiList.Add(id, obj);
            }

            cardList[id].CountUp(1);
            dekkiList[id].CountUp(1);
            dekkiCount++;
        }
        SetPosDekkiCard();
        dekkiCountText.text = dekkiCount + "枚";
    }

    // パーティーにキャラクターを追加,変更
    public void SetCharaID(int id, Vector3 point)
    {
        int idx = -1;
        // パーティーの何番目に入れるか
        for (int i = 0; i < memberSetCW.Length; i++)
        {
            if(!CheckDropBox(point, memberSetCW[i].rectTransform)) continue;

            idx = i;
            data.charaID[i] = id+1;
            memberSetCW[i].SetData(id, charaDatas[id].arousals[0].img, charaDatas[id].Name);
            break;
        }

        if (idx < 0) return;
        // パーティー内に重複キャラがいた場合
        for(int i = 0; i < data.charaID.Length; i++)
        {
            if(i == idx || data.charaID[i] != id+1) continue; // 変更を加えた箇所

            // 削除
            data.charaID[i] = 0;
            memberSetCW[i].SetData(-1);
        }
    }

    // デッキ編集シーン ===============================================================================
    void DekkiSceneInit()
    {
        // カード一覧を生成
        cardList = new Dictionary<string, CardIconScript>();
        foreach(var vl in cardDatas)
        {
            if (vl.Key.Substring(0, 1) == "N") continue; // 除外カード

            // キャラクタースキルカードなら
            if (vl.Key.Substring(0, 1) == "C")
            {
                int typeID = int.Parse(vl.Key.Substring(2, 1));
                if (typeID != 1) continue; // ノーマル状態のスキル以外は除外
            }

            var obj = Instantiate(cardPrefab, cardListParent);
            obj.Init(vl.Value, 1, cardPrefab, this);
            cardList.Add(vl.Key, obj);
        }
        cardPrefab.enabled = false;

        dekkiList = new Dictionary<string, CardIconScript>();
    }

    void UpdateCardListPos()
    {
        int count = 0;
        foreach(var vl in cardList)
        {
            // 一度非表示
            vl.Value.gameObject.SetActive(false);

            // キャラクタースキルカードなら
            if (vl.Key.Substring(0, 1) == "C")
            {
                int charID = int.Parse(vl.Key.Substring(1, 1));
                int skillID = int.Parse(vl.Key.Substring(3, 1));

                // パーティー内のキャラクターのスキルか確認
                bool flag = false;
                foreach (var i in charaIDList) { if (i == charID) { flag = true; break; } }
                if (!flag) continue;
            }

            // ポジション更新
            int x = 0, y = 0;
            if (count > 0)
            {
                x = count % 5;
                y = count / 5;
            }
            vl.Value.gameObject.SetActive(true);
            vl.Value.transform.position = cardListParent.position;
            vl.Value.transform.position += new Vector3(250f * x, -340 * y, 0);
            count++;
        }
    }

    /* ---セーブ --- */
    public void SaveDekki()
    {
        // デッキ枚数が40枚未満なら
        if (dekkiCount < 40) return;

        // セーブ
        FindAnyObjectByType<DataBaseManager>().Save(data, $"save{index}");
        saveDatas[index] = data;
        // 新規セーブなら隣にからデータを生成
        if (index+1 < saveDatas.Length && saveDatas[index+1] == null) saveDatas[index+1] = new SaveData() { charaID = new int[3], dekki = new List<string>() };
        SetPartyData(0);

        // シーン遷移
        ChangeScene(State.Main);
    }

    public bool AddDekkiCard(Vector3 point, string id)
    {
        if(dekkiCount >= 40) return false;
        // 指定範囲内でドロップしているか確認
        if(!CheckDropBox(point, dekkiListArea)) return false;

        // なければ生成
        if (!dekkiList.ContainsKey(id))
        {
            var obj = Instantiate(cardPrefab, dekkiListParent);
            obj.enabled = true;
            obj.Init(cardDatas[id], 2, cardPrefab, this);
            dekkiList.Add(id, obj);
            SetPosDekkiCard();
        }

        // 最大4枚
        if (dekkiList[id].GetCount() >= 4) return false;

        data.dekki.Add(id);
        dekkiList[id].CountUp(1);
        dekkiCount++;
        dekkiCountText.text = dekkiCount + "枚";
        return true;
    }
    public bool RemoveDekkiCard(Vector3 point, string id)
    {
        // 指定範囲内でドロップしているか確認
        if (!CheckDropBox(point, cardListArea)) return false;

        data.dekki.Remove(id);
        cardList[id].CountUp(-1);
        dekkiCount--;
        dekkiCountText.text = dekkiCount + "枚";

        // デッキ内にまだあるか確認
        if (dekkiList[id].GetCount() > 1) return true;

        // 削除
        Destroy(dekkiList[id].gameObject);
        dekkiList.Remove(id);
        SetPosDekkiCard();

        return true;
    }

    void SetPosDekkiCard()
    {
        int count = 0;
        foreach (var vl in dekkiList)
        {
            // ポジション更新
            int x = 0, y = 0;
            if (count > 0)
            {
                x = count % 2;
                y = count / 2;
            }
            vl.Value.transform.position = dekkiListParent.position;
            vl.Value.transform.position += new Vector3(250f * x, -340 * y, 0);
            count++;
        }
    }

    void ScrollCardList()
    {
        Vector3 point = Input.mousePosition;
        if (!CheckDropBox(point, cardListArea)) return;

        // スクロールのベクトルを取得（x, y）
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (scroll.y != 0f)
        {
            cardListParent.Translate(0, -scroll.y * 20, 0);
        }
    }

    void ScrollDekkiList()
    {
        Vector3 point = Input.mousePosition;
        if (!CheckDropBox(point, dekkiListArea)) return;

        // スクロールのベクトルを取得（x, y）
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (scroll.y != 0f)
        {
            dekkiListParent.Translate(0, -scroll.y * 20, 0);
        }
    }
}
