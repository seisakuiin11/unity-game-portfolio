using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommonData;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataBaseManager : MonoBehaviour
{
    // データ取得(DBアクセス) =======================
    [Header("Google設定")]
    string spreadsheetId = "1xeynz2aq8QwMqTc5Mw9YTLqXzMtsrnJgiDsDPdn1OVU";
    string range = "CardData!A2:J";                       // A列?J列 カードデータシート
    string rangeArousal = "キャラクター_潜在覚醒!A2:E";   // A列?E列 潜在覚醒データシート
    string rangeChara = "キャラクター_パラメーター!A2:I"; // A列?I列 キャラデータシート

    Dictionary<string, CardData> cardDatas;
    List<CharaData> charaDatas;

    // <セーブ機能> ==================================
#if UNITY_EDITOR
    private static string SaveDirectory => Path.Combine(Application.dataPath, "SaveData"); // プロジェクト内に保存
#else
    private static string SaveDirectory => Application.persistentDataPath; // PC内に保存
#endif

    private void Awake()
    {
        // LoadData();
    }

    // データ取得 ===============================================================================================================================
    public void LoadData()
    {
        // スプレッドシートにアクセス
        string credentialPath = Path.Combine(Application.streamingAssetsPath, "healthy-system-473309-s9-74e99e5373fc.json");

        GoogleCredential credential;
        using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "UnityCardGame",
        });

        // カードデータ取得
        LoadCards(service);
        // キャラデータ取得
        LoadCharaData(service);

        // ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(SaveDirectory);
        string SavePath = Path.Combine(SaveDirectory, "save0.json");
        // セーブデータがあるか確認
        if (File.Exists(SavePath)) return;

        List<string> dekkiData = new List<string>() {
            "C111", "C111", "C111",
            "C112", "C112", "C112",
            "C113", "C113", "C113",
            "C114", "C114", "C114",
            "C115", "C115", "C115",
            "C211", "C211", "C212", "C212", "C213", "C213",
            "C214", "C214", "C215", "C215",
            "C311", "C311", "C311",
            "C312", "C312", "C312",
            "C313", "C313", "C313",
            "C314", "C314", "C314",
            "C315", "C315", "C315",
        };

        SaveData saveData = new SaveData { charaID = new int[3] {1,2,3}, dekki = new List<string>(dekkiData) };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    /// <summary>
    /// カードの全データを取得する
    /// </summary>
    void LoadCards(SheetsService service)
    {
        cardDatas = new Dictionary<string, CardData>();
        // カードデータ取得 ------------------------------------------------------------------------------------------------
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        ValueRange response = request.Execute();
        var values = response.Values;

        if (values != null && values.Count > 0)
        {
            foreach (var row in values)
            {
                // 行の長さが足りない場合に備えて安全にアクセス
                string GetValue(int index) => (row.Count > index) ? row[index].ToString() : "";

                var card = new CardData()
                {
                    CardId = GetValue(0),
                    Name = GetValue(1),
                    Ap = int.TryParse(GetValue(2), out var c) ? c : 0,
                    SelectId = int.TryParse(GetValue(3), out var s) ? s : 0,
                    NotSelect = GetValue(4) == "TRUE",
                    Duration = int.TryParse(GetValue(5), out var d) ? d : -1,
                    Value1 = int.TryParse(GetValue(6), out var v1) ? v1 : 0,
                    Value2 = int.TryParse(GetValue(7), out var v2) ? v2 : 0,
                    Value3 = int.TryParse(GetValue(8), out var v3) ? v3 : 0,
                    EffectText = GetValue(9),
                };

                if (string.IsNullOrEmpty(card.CardId)) continue;

                if (!cardDatas.ContainsKey(card.CardId))
                {
                    cardDatas.Add(card.CardId, card);
                }
                else
                {
                    Debug.LogWarning($"カードIDが重複しています: {card.CardId}");
                }
            }
        }
    }

    /// <summary>
    /// キャラクターデータ取得
    /// </summary>
    void LoadCharaData(SheetsService service)
    {
        charaDatas = new List<CharaData>();
        // 潜在覚醒の情報取得 ----------------------------------------------------------------------------
        var request = service.Spreadsheets.Values.Get(spreadsheetId, rangeArousal);
        ValueRange response = request.Execute();
        var values = response.Values;

        Dictionary<int, ArousalData> datas = new Dictionary<int, ArousalData>();

        if (values != null && values.Count > 0)
        {
            foreach (var row in values)
            {
                // 行の長さが足りない場合に備えて安全にアクセス
                string GetValue(int index) => (row.Count > index) ? row[index].ToString() : "";

                int num = int.Parse(GetValue(0)) * 10 + int.Parse(GetValue(2));

                var data = new ArousalData()
                {
                    Name = GetValue(3),
                    Text = GetValue(4),
                };

                if (!datas.ContainsKey(num))
                {
                    datas.Add(num, data);
                }
                else
                {
                    Debug.LogWarning($"カードIDが重複しています: {num}");
                }
            }
        }

        // キャラクターデータ取得 --------------------------------------------------------------------------
        request = service.Spreadsheets.Values.Get(spreadsheetId, rangeChara);
        response = request.Execute();
        values = response.Values;

        if (values != null && values.Count > 0)
        {
            foreach (var row in values)
            {
                // 行の長さが足りない場合に備えて安全にアクセス
                string GetValue(int index) => (row.Count > index) ? row[index].ToString() : "";

                var charaData = new CharaData()
                {
                    ID = int.Parse(GetValue(0)),
                    Name = GetValue(1),
                    HP = int.Parse(GetValue(2).Replace(",", "")),
                    Attack = int.Parse(GetValue(3).Replace(",", "")),
                    MagicAttack = int.Parse(GetValue(4).Replace(",", "")),
                    Deal = int.Parse(GetValue(5)),
                    Take = int.Parse(GetValue(7)),
                    arousals = new List<ArousalData>()
                };
                // 該当キャラクターの潜在覚醒情報を格納
                foreach (var key in datas)
                {
                    if (charaData.ID != key.Key / 10) continue;

                    // キャラクタースプライトを取得する
                    GetCharaSprite(key.Value, charaData.Name, key.Value.Name);

                    charaData.arousals.Add(key.Value);
                }

                charaDatas.Add(charaData);
            }
        }
    }
    async void GetCharaSprite(ArousalData data,string charaName, string arousalType)
    {
        Debug.Log($"Assets/3.素材/Character/{charaName}_{arousalType}.png");
        // 検索内容を定義
        var handle = Addressables.LoadAssetAsync<Sprite>($"Assets/3.素材/Character/{charaName}_{arousalType}.png");

        await handle.Task; // 検索実行

        // 結果判定
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            data.img = handle.Result;
        }
    }

    public Dictionary<string, CardData> GetCardData()
    {
        return cardDatas;
    }

    public List<CharaData> GetCharaData()
    {
        return charaDatas;
    }

    // セーブ機能 ===============================================================================================================================
    /// <summary> データをセーブする </summary>
    public void Save(SaveData data, string saveName = "save0")
    {
        // ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(SaveDirectory);

        string SavePath = Path.Combine(SaveDirectory, $"{saveName}.json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"セーブ完了: {SavePath}");
    }

    /// <summary> データをロードする </summary>
    public SaveData Load(string saveName = "save0")
    {
        string SavePath = Path.Combine(SaveDirectory, $"{saveName}.json");

        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("セーブデータが存在しません。新規作成します。");
            return new SaveData { charaID = new int[3], dekki = new List<string>() };
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"ロード完了: {SavePath}");
        return data;
    }

    /// <summary>
    /// セーブデータをすべて取得する
    /// </summary>
    public List<SaveData> LoadAll()
    {
        List<SaveData> datas = new List<SaveData>();
        var fileNames = GetAllSaveFiles();

        foreach (var file in fileNames)
        {
            string SavePath = Path.Combine(SaveDirectory, $"{file}.json");

            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            datas.Add(data);
        }

        return datas;
    }

    /// <summary>
    /// 保存されている全てのセーブファイル名を取得
    /// </summary>
    string[] GetAllSaveFiles()
    {
        if (!Directory.Exists(SaveDirectory)) return new string[0];
        string[] files = Directory.GetFiles(SaveDirectory, "*.json");
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]); // ファイル名だけ返す
        }
        return files;
    }
}
// ====================================================================================================================
namespace CommonData
{
    [Serializable]
    public class SaveData
    {
        public int[] charaID;       // キャラID
        public List<string> dekki;  // デッキ(カードIDを格納)
    }

    public class CardData
    {
        public string CardId;      // カードID
        public string Name;        // スキルネーム
        public int Ap;             // コスト
        public int SelectId;       // 選択対象
        public bool NotSelect;     // 選択するかどうか
        public int Duration;       // 継続ターン（- は 0 として扱う）
        public int Value1;         // 数値.1
        public int Value2;         // 数値.2
        public int Value3;         // 数値.3
        public string EffectText;  // 効果
    }

    /// <summary>
    /// キャラクターデータ
    /// </summary>
    public class CharaData
    {
        /// <summary> キャラID </summary>
        public int ID;
        /// <summary> キャラの名前 </summary>
        public string Name;
        /// <summary> 基礎HP </summary>
        public int HP;
        /// <summary> 基礎攻撃力 </summary>
        public int Attack;
        /// <summary> 基礎魔法攻撃力 </summary>
        public int MagicAttack;
        /// <summary> 与ダメージ </summary>
        public int Deal;
        /// <summary> 被ダメージ </summary>
        public int Take;
        /// <summary> 潜在覚醒(タイプ,説明) </summary>
        public List<ArousalData> arousals = new List<ArousalData>();
    }

    public class ArousalData
    {
        public string Name;
        public string Text;
        public Sprite img;
    }

    // ステージごとの敵情報
    [Serializable]
    public class Round { public List<int> enemysID = new List<int>(); }
    [Serializable]
    public class Stage { public bool clearFlag; public List<Round> rounds = new List<Round>(); }
}