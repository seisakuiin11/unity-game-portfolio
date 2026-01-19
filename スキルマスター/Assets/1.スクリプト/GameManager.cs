using System.Threading.Tasks;
using UnityEngine;

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

    [System.Serializable]
    class SceneName {
        public Scene scene;
        public string name;
    }
    [SerializeField] SceneName[] sceneNames;
    SceneName scene;
    [SerializeField] DataBaseManager dataBaseManager;

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

        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Scene遷移で破棄されなようにする。      
        DontDestroyOnLoad(this);

        Init();
    }

    void Init()
    {
        scene = sceneNames[0];
    }

    public void LoadScene(Scene _scene)
    {
        var oldScene = scene;
        scene = sceneNames[(int)_scene];

        // タイトルシーンから遷移するとき、データをロード
        if(oldScene.scene == Scene.Title) dataBaseManager.LoadData();

        // Scene遷移が不必要なら(同一シーンの場合があるため)
        if (oldScene.name == scene.name) return;

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
    }
    public Scene GetScene() { return scene.scene; }
}
