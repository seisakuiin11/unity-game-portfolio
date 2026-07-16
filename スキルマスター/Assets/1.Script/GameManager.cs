using UnityEngine;
using CommonData;

/// <summary>
/// ゲーム全体を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    public enum Scene
    {
        Title,
        Lobby,
        Party,
        Quest,
        Game,
        MaxScene
    }

    [SerializeField] AudioManager audioManager;

    [System.Serializable]
    class SceneName {
        public Scene scene;
        public string name;
    }
    [SerializeField] SceneName[] sceneNames;
    SceneName scene;

    // 各ステージのクリア状況と敵情報
    [SerializeField] Stage[] stages;

    /// <summary> 何番目のセーブデータを使用するか </summary>
    public int saveDataIndex = 0;
    /// <summary> 何番目のステージに挑むのか </summary>
    public int stageIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // instanceがすでにあったら自分を消去する。
        if (instance && this != instance)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        audioManager.Init();

        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Scene遷移で破棄されなようにする。      
        DontDestroyOnLoad(this);

        Init();
    }

    void Init()
    {
        // データベースの準備
        DataBaseManager.Instance = new DataBaseManager();

        scene = sceneNames[0];
    }

    /// <summary>
    /// シーンを切り替える
    /// </summary>
    /// <param name="_scene"></param>
    public void LoadScene(Scene _scene)
    {
        var oldScene = scene;
        scene = sceneNames[(int)_scene];

        // タイトルシーンから遷移するとき、データをロード
        if(oldScene.scene == Scene.Title) DataBaseManager.Instance.LoadData();

        // Scene遷移が不必要なら(同一シーンの場合があるため)
        if (oldScene.name == scene.name) return;

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
    }
    public Scene GetScene() { return scene.scene; }

    /// <summary>
    /// そのステージの敵情報
    /// </summary>
    /// <returns></returns>
    public Stage GetStageData()
    {
        return stages[stageIndex];
    }

    /// <summary>
    /// どのステージをクリアしているか返す
    /// </summary>
    /// <returns></returns>
    public bool[] GetStageClearFlags()
    {
        bool[] flags = new bool[stages.Length];

        for (int i = 0; i < stages.Length; i++)
        {
            flags[i] = stages[i].clearFlag;
        }

        return flags;
    }

    /// <summary>
    /// ステージクリア状態にする
    /// </summary>
    public void StageClear()
    {
        stages[stageIndex].clearFlag = true;
    }
}
