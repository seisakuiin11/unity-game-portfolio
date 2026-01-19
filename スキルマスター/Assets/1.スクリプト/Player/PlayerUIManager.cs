using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("生成ルート"), SerializeField]
    Transform createRoot;
    [Header("コスト不足テキスト"), SerializeField]
    TextMeshProUGUI cautionText;
    [Header("APテキスト"), SerializeField]
    TextMeshProUGUI apText;
    [Header("EPアイコン")]
    [SerializeField] Image epIcon;
    [SerializeField] Animator epIconAnim;
    [SerializeField] TextMeshProUGUI epText;
    [SerializeField] GameObject[] epCost;
    [SerializeField] float epCostSpace;
    [Header("潜在覚醒タイプ選択画面")]
    [SerializeField] GameObject arousalUI;
    [SerializeField] Image[] arousalCards;
    [SerializeField] ArousalAnim arousalAnim;

    PlayerController playerController;
    int arousalSP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Init(PlayerController _playerController, int _arousalSP)
    {
        playerController = _playerController;
        arousalSP = _arousalSP;
    }

    /// <summary> 消費コストがオーバー時のテキスト表示 </summary>
    /// <param name="type">足りないもの</param>
    public void ShowCautionText(string type)
    {
        cautionText.gameObject.SetActive(true);
        cautionText.text = "Is not enough " + type;
        DOVirtual.DelayedCall(2f, () => { cautionText.gameObject.SetActive(false); });
    }

    /// <summary> APの数値を更新 </summary>
    /// <param name="num">数値</param>
    public void UpdateAPUI(int num)
    {
        apText.text = num.ToString();
    }

    /// <summary> EPの数値を更新 </summary>
    /// <param name="num">数値</param>
    public void UpdateEPUI(int num)
    {
        int cnt = num / arousalSP;
        int per = num % arousalSP;

        epIcon.fillAmount = per / (float)arousalSP;
        epText.text = "EP:" + num;
        // 最大値に達していたら
        if (per == 0 && cnt > 0) epIcon.fillAmount = 1;

        foreach (var item in epCost) item.gameObject.SetActive(false);

        // EPが足りなければ、中断
        if (cnt <= 0) { epIconAnim.SetBool("Play", false); return; }

        epIconAnim.SetBool("Play", true);
        // EPコスト表示
        float width = epCostSpace * (cnt - 1);
        for(int i = 0; i < cnt; i++)
        {
            epCost[i].gameObject.SetActive(true);

            epCost[i].transform.localPosition = new Vector3(epCostSpace * i - (width/2), 0, 0);
        }
    }

    /// <summary> 潜在覚醒のボタンをクリックしたとき </summary>
    public void ClickArousalBtn()
    {
        playerController.SelectLatentArousal();
    }

    /// <summary> 潜在覚醒のタイプ選択画面表示 </summary>
    /// <param name="charID">キャラクターID</param>
    /// <param name="maxType">キャラクターの所持タイプ数</param>
    public void ShowArousalUI(int charID, int maxType, List<CommonData.ArousalData> arousalDatas)
    {
        // 選択画面表示
        arousalUI.SetActive(true);
        arousalUI.transform.localScale = new Vector3 (1f, 0f, 1f);
        arousalUI.transform.DOScaleY(1f, 0.3f);

        // 一度すべての選択肢を非表示
        foreach (var item in arousalCards) item.gameObject.SetActive(false);

        // 必要なものを表示
        int width = 300 * (maxType - 2);
        for (int i = 0; i < maxType - 1; i++)
        {
            // カードを表示
            arousalCards[i].gameObject.SetActive(true);

            var texts = arousalCards[i].GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = arousalDatas[i+1].Name;
            texts[1].text = arousalDatas[i+1].Text;

            var sprt = arousalCards[i];
            Addressables.LoadAssetAsync<Sprite>($"Assets/3.素材/ArousalCard/Arousal_{charID}-{i + 1}.png").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Sprite sp = handle.Result;
                    sprt.sprite = sp;
                }
            };

            // ポジション設定
            arousalCards[i].transform.localPosition = new Vector3(300 * i - (width / 2), 0, 0);
        }
    }

    /// <summary> 潜在覚醒のタイプカードをクリックしたとき </summary>
    /// <param name="index">何番目のキャラクター</param>
    public void ClickArousalCard(int index)
    {
        // 選択画面表示
        var obj = Instantiate(arousalAnim, createRoot);
        obj.Init();
        arousalUI.SetActive(false);
        playerController.LatentArousal(index);
    }
}
