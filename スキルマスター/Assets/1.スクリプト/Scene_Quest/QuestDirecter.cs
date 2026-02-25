using System.Collections.Generic;
using System.Threading.Tasks;
using CommonData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestDirecter : MonoBehaviour
{
    public enum State
    {
        Quest,
        Party
    }

    [SerializeField, Header("ロード画面")] GameObject loadUI;
    [SerializeField] GameObject fadePanel;

    [Header("クエスト選択画面")]
    [SerializeField] GameObject questUI;
    [SerializeField] GameObject[] questBtns_Capsule;
    [SerializeField] Image[] enemyImgs;
    [SerializeField] Sprite[] enemySprites;

    [Header("パーティ選択画面")]
    [SerializeField] GameObject partyUI;
    [SerializeField] TextMeshProUGUI partyName;
    [SerializeField] Image[] charaImg;
    [SerializeField] TextMeshProUGUI[] charaText;

    [SerializeField, Header("カードリストUI")] CardListUI cardListUI;
    [SerializeField, Header("ステータスUI")] StatusUI statusUI;
    bool statusUIFlag;

    State state;
    Dictionary<State, GameObject> scene;
    int saveIndex;
    bool[] questsFlag;
    List<SaveData> saveDatas;
    List<CharaData> charaDatas;
    Dictionary<string, CardData> cardDatas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.DefultBGM();

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            BackScene();
        }
    }

    void Init()
    {
        // データのロード
        var manager = FindAnyObjectByType<DataBaseManager>();
        saveDatas = manager.LoadAll();
        charaDatas = manager.GetCharaData();
        cardDatas = manager.GetCardData();

        scene = new Dictionary<State, GameObject> {
            { State.Quest, questUI },
            { State.Party, partyUI },
        };

        // フェイドパネルの初期化
        fadePanel.SetActive(true);
        fadePanel.transform.localScale = new Vector3(0, 1, 1);
        fadePanel.transform.position = new Vector3(0, fadePanel.transform.position.y, 0);

        // 画面の表示
        foreach (var obj in scene) obj.Value.SetActive(false);
        state = State.Quest;
        scene[state].SetActive(true);

        // 挑戦可能クエストのアクティブ化
        questsFlag = GameManager.Instance.GetStageClearFlags();
        bool clearFlag = false;
        for(int i = 0; i < questsFlag.Length; i++)
        {
            // クエスト張り紙を生成 (仮プログラム)
            questBtns_Capsule[i].SetActive(clearFlag);
            bool flag = !clearFlag;
            clearFlag = !questsFlag[i];
            if (questsFlag[i]) { enemyImgs[i].sprite = enemySprites[i]; enemyImgs[i].SetNativeSize(); }
            questsFlag[i] = flag;
        }

        statusUI.Init();
        cardListUI.Init(40); // デッキの最大枚数
    }

    /// <summary> 前のシーンに戻る </summary>
    public void BackScene()
    {
        // ステータスUIを表示していたら
        if (statusUIFlag) { statusUIFlag = false; statusUI.CloseStatusUI(); return; }

        AudioManager.Instance.CancelSE();
        // 最初の画面なら
        if (state == 0) { GameManager.Instance.LoadScene(GameManager.Scene.Lobby); return; }

        // 前のシーンに戻る
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

    // クエスト選択画面 ============================================================================================
    public void QuestBtnClick(int _index)
    {
        if (!questsFlag[_index]) return;

        AudioManager.Instance.EnterSE();

        // ステージ番号
        GameManager.Instance.stageIndex = _index;

        PartyInit();
    }

    // パーティ選択画面 ============================================================================================
    void PartyInit()
    {
        // 前のシーンから遷移する
        ChangeScene(State.Party);

        // セーブデータをセット
        SetPartyData(0);
    }

    /// <summary>
    /// 指定数隣のセーブデータをセットする
    /// </summary>
    public void SetPartyData(int index)
    {
        // 何番目のセーブデータ
        saveIndex = (saveIndex + index) % saveDatas.Count;
        if (saveIndex < 0) saveIndex = saveDatas.Count - 1;

        partyName.text = "パーティー." + (saveIndex + 1);
        // 画像,テキスト 代入
        SaveData data = saveDatas[saveIndex];
        for (int i = 0; i < data.charaID.Length; i++)
        {
            int num = data.charaID[i] - 1;
            charaImg[i].sprite = charaDatas[num].arousals[0].img;
            charaImg[i].SetNativeSize();
            charaText[i].text = charaDatas[num].Name;
        }
    }

    public async void GameStart()
    {
        AudioManager.Instance.EnterSE();

        loadUI.SetActive(true);
        // 何番目のセーブデータを使用するか
        GameManager.Instance.saveDataIndex = saveIndex;

        await Task.Delay(500);

        GameManager.Instance.LoadScene(GameManager.Scene.Game); // ロードあり
    }

    /*** === キャラクター情報を表示 === ***/
    public void OpenStatusUI(int index)
    {
        statusUIFlag = true;

        int num = saveDatas[saveIndex].charaID[index]-1;
        statusUI.OpenStatusUI(charaDatas[num], charaDatas[num].arousals[0].img);
    }

    /*** === デッキ情報を表示 === ***/
    public void OpenCardListUI()
    {
        List<CardData> cards = new List<CardData>();
        foreach (var id in saveDatas[saveIndex].dekki) cards.Add(cardDatas[id]);
        cardListUI.OpenUI(cards);
    }
}
