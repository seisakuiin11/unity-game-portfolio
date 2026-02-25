using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TitleDirecter : MonoBehaviour
{
    [SerializeField] GameObject title;
    [SerializeField] GameObject lobby;
    [SerializeField] GameObject load;
    [Header("ロビー画面")]
    [SerializeField] GameObject partyBtn;
    [SerializeField] GameObject questBtn;
    [SerializeField] float moveLength;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.DefultBGM();

        if (GameManager.Instance.GetScene() == GameManager.Scene.Title) {
            title.SetActive(true);
            lobby.SetActive(false);
        }
        else if (GameManager.Instance.GetScene() == GameManager.Scene.Lobby)
        {
            title.SetActive(false);
            lobby.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ロビーへ進む
        if(GameManager.Instance.GetScene() == GameManager.Scene.Title && Input.GetKeyDown(KeyCode.Space))
        {
            NextScene();
        }

        // キー操作(Escape)
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            // ロビーなら
            if (GameManager.Instance.GetScene() == GameManager.Scene.Lobby) BackScene();
            // タイトルなら
            else if (GameManager.Instance.GetScene() == GameManager.Scene.Title) GameEnd();
        }
    }

    public async void NextScene() // ロビーへ進む
    {
        AudioManager.Instance.StartSE();
        load.SetActive(true); // 表示

        await Task.Delay(100);

        GameManager.Instance.LoadScene(GameManager.Scene.Lobby); // ロードが入る

        await Task.Delay(100);

        title.SetActive(false);
        lobby.SetActive(true);
        load.SetActive(false); // 非表示

        // ボタンのアニメーション
        partyBtn.transform.Translate(Vector3.left * moveLength);
        questBtn.transform.Translate(Vector3.right * moveLength);
        partyBtn.transform.DOMoveX(partyBtn.transform.position.x + moveLength, 0.2f);
        questBtn.transform.DOMoveX(questBtn.transform.position.x - moveLength, 0.2f);
    }

    public void BackScene() // タイトルへ戻る
    {
        AudioManager.Instance.CancelSE();

        GameManager.Instance.LoadScene(GameManager.Scene.Title);
        title.SetActive(true);
        lobby.SetActive(false);
    }

    public void GameEnd() //ゲームプレイ終了
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();//ゲームプレイ終了
#endif
    }

    public async void QuestBtnClick()
    {
        AudioManager.Instance.EnterSE();

        load.SetActive(true);
        await Task.Delay(100);

        GameManager.Instance.LoadScene(GameManager.Scene.Quest);
    }

    public async void PartyBtnClick()
    {
        AudioManager.Instance.EnterSE();

        load.SetActive(true);
        await Task.Delay(100);

        GameManager.Instance.LoadScene(GameManager.Scene.Party);
    }
}
