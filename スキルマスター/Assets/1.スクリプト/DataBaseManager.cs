using CommonData;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

    // StreamingAssets内に保存するCSVのファイル名
    const string CsvCards = "CardData.csv";
    const string CsvArousal = "ArousalData.csv";
    const string CsvChara = "CharaData.csv";

    Dictionary<string, CardData> cardDatas;
    List<CharaData> charaDatas;

    // StreamingAssets パス
    private static string StreamingAssetsDir => Application.streamingAssetsPath;


    // データ取得 ===============================================================================================================================
    public void LoadData()
    {
        // CSVファイルをGoogleスプレッドシートから取得する
        FetchAndSaveCSV();

        // カードデータ取得
        LoadCards();
        // キャラデータ取得
        LoadCharaData();


        // セーブデータが存在しない場合は作成
        string SavePath = Path.Combine(StreamingAssetsDir, "save0.json");

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
    /// Google Sheetsの3シートをそれぞれCSVとしてStreamingAssetsに保存する。
    /// </summary>
    void FetchAndSaveCSV()
    {
        // ネットワークに接続しているか
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("ネットワークに接続されていないため、スプレッドシートの更新をスキップします。");
            return;
        }

        // 認証情報の取得
        string credentialPath = Path.Combine(Application.streamingAssetsPath, "healthy-system-473309-s9-74e99e5373fc.json");
        
        // スプレッドシートにアクセス
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

        // 各シートをCSVに変換して保存
        SaveSheetAsCsv(service, range, Path.Combine(StreamingAssetsDir, CsvCards));
        SaveSheetAsCsv(service, rangeArousal, Path.Combine(StreamingAssetsDir, CsvArousal));
        SaveSheetAsCsv(service, rangeChara, Path.Combine(StreamingAssetsDir, CsvChara));
    }

    /// <summary>
    /// スプレッドシートデータを取得し、CSVファイルとして保存
    /// </summary>
    void SaveSheetAsCsv(SheetsService service, string sheetRange, string outputPath)
    {
        var request = service.Spreadsheets.Values.Get(spreadsheetId, sheetRange);
        ValueRange response = request.Execute();
        var values = response.Values;

        if (values == null || values.Count == 0)
        {
            Debug.LogWarning($"シートが空か取得できませんでした: {sheetRange}");
            return;
        }

        // IList<IList<object>> → CSV文字列に変換
        var lines = new System.Text.StringBuilder();
        foreach (var row in values)
        {
            // セル内のカンマ・改行はダブルクォートでエスケープ
            var escapedCells = new List<string>();
            foreach (var cell in row)
            {
                string cellStr = cell?.ToString() ?? "";
                if (cellStr.Contains(",") || cellStr.Contains("\"") || cellStr.Contains("\n"))
                    cellStr = "\"" + cellStr.Replace("\"", "\"\"") + "\"";
                escapedCells.Add(cellStr);
            }
            lines.AppendLine(string.Join(",", escapedCells));
        }

        File.WriteAllText(outputPath, lines.ToString(), System.Text.Encoding.UTF8);
        Debug.Log($"CSV保存完了: {outputPath}");
    }

    /// <summary>
    /// カードの全データを取得する
    /// </summary>
    void LoadCards()
    {
        cardDatas = new Dictionary<string, CardData>();
        // カードデータ取得 ------------------------------------------------------------------------------------------------
        string csvPath = Path.Combine(StreamingAssetsDir, CsvCards);
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSVが見つかりません: {csvPath}");
            return;
        }

        foreach (var line in File.ReadAllLines(csvPath, System.Text.Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = ParseCsvLine(line);
            string GetValue(int i) => (cols.Count > i) ? cols[i] : "";

            var card = new CardData()
            {
                CardId = GetValue(0),
                Name = GetValue(1),
                Ap = int.TryParse(GetValue(2), out var ap) ? ap : 0,
                SelectId = int.TryParse(GetValue(3), out var sid) ? sid : 0,
                NotSelect = GetValue(4) == "TRUE",
                Duration = int.TryParse(GetValue(5), out var dur) ? dur : -1,
                Value1 = int.TryParse(GetValue(6), out var v1) ? v1 : 0,
                Value2 = int.TryParse(GetValue(7), out var v2) ? v2 : 0,
                Value3 = int.TryParse(GetValue(8), out var v3) ? v3 : 0,
                EffectText = GetValue(9),
            };

            if (string.IsNullOrEmpty(card.CardId)) continue;

            if (!cardDatas.ContainsKey(card.CardId))
                cardDatas.Add(card.CardId, card);
            else
                Debug.LogWarning($"カードIDが重複しています: {card.CardId}");
        }
    }

    /// <summary>
    /// キャラクターデータ取得
    /// </summary>
    void LoadCharaData()
    {
        charaDatas = new List<CharaData>();
        // 潜在覚醒の情報取得 ----------------------------------------------------------------------------
        // ① 覚醒データ読み込み
        string arousalPath = Path.Combine(StreamingAssetsDir, CsvArousal);
        if (!File.Exists(arousalPath))
        {
            Debug.LogError($"CSVが見つかりません: {arousalPath}");
            return;
        }
 
        var datas = new Dictionary<int, ArousalData>();
        foreach (var line in File.ReadAllLines(arousalPath, System.Text.Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = ParseCsvLine(line);
            string GetValue(int i) => (cols.Count > i) ? cols[i] : "";
 
            int key = int.Parse(GetValue(0)) * 10 + int.Parse(GetValue(2));
            var data = new ArousalData()
            {
                Name = GetValue(3),
                Text = GetValue(4),
            };
 
            if (!datas.ContainsKey(key))
                datas.Add(key, data);
            else
                Debug.LogWarning($"覚醒データが重複しています: {key}");
        }
 
        // ② キャラクターデータ読み込み
        string charaPath = Path.Combine(StreamingAssetsDir, CsvChara);
        if (!File.Exists(charaPath))
        {
            Debug.LogError($"CSVが見つかりません: {charaPath}");
            return;
        }
 
        foreach (var line in File.ReadAllLines(charaPath, System.Text.Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = ParseCsvLine(line);
            string GetValue(int i) => (cols.Count > i) ? cols[i] : "";
 
            var charaData = new CharaData()
            {
                ID           = int.Parse(GetValue(0)),
                Name         = GetValue(1),
                HP           = int.Parse(GetValue(2).Replace(",", "")),
                Attack       = int.Parse(GetValue(3).Replace(",", "")),
                MagicAttack  = int.Parse(GetValue(4).Replace(",", "")),
                Deal         = int.Parse(GetValue(5)),
                Take         = int.Parse(GetValue(7)),
                arousals     = new List<ArousalData>()
            };
 
            foreach (var kvp in datas)
            {
                if (charaData.ID != kvp.Key / 10) continue;
                GetCharaSprite(kvp.Value, charaData.Name, kvp.Value.Name);
                charaData.arousals.Add(kvp.Value);
            }
 
            charaDatas.Add(charaData);
        }
    }

    /// <summary>
    /// CSV行を正しくパースする（ダブルクォートのエスケープに対応）
    /// </summary>
    List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // エスケープされた "" をスキップ
                }
                else if (c == '"')
                    inQuotes = false;
                else
                    current.Append(c);
            }
            else
            {
                if (c == '"')
                    inQuotes = true;
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                    current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result;
    }

    async Task GetCharaSprite(ArousalData data,string charaName, string arousalType)
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
        //Directory.CreateDirectory(StreamingAssetsDir);

        string SavePath = Path.Combine(StreamingAssetsDir, $"{saveName}.json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"セーブ完了: {SavePath}");
    }

    /// <summary> データをロードする </summary>
    public SaveData Load(string saveName = "save0")
    {
        string SavePath = Path.Combine(StreamingAssetsDir, $"{saveName}.json");

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

        // jsonファイルをソートするための条件
        Regex saveFilePattern = new Regex(@"^save\d+$");

        foreach (var file in fileNames)
        {
            // マッチング 検査
            if (!saveFilePattern.IsMatch(file)) continue;

            Debug.Log(file);
            string SavePath = Path.Combine(StreamingAssetsDir, $"{file}.json");

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
        if (!Directory.Exists(StreamingAssetsDir)) return new string[0];

        string[] files = Directory.GetFiles(StreamingAssetsDir, "*.json");

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