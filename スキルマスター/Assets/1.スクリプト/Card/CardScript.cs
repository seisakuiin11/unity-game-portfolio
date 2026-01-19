using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using CommonData;

public class CardScript : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI apText;
    [SerializeField] TextMeshProUGUI durationText;
    [SerializeField] TextMeshProUGUI dicText;
    [SerializeField] GameObject useBtn;

    string PearentFile = "Assets/3.素材/Card/";
    int index;
    bool activeFlag;
    CardManager manager;
    CardTypeListUI cardTypeListUI;

    public bool useFlag;
    public CardData data;


    void Awake()
    {
        //PearentFile = "Assets/3.素材/Card/";
    }

    private void OnDestroy()
    {
        //アセットの解放
        //Addressables.Release(img.sprite);
    }

    public void SetIndex(int _index)
    {
        index = _index;
        this.transform.SetSiblingIndex(index);
    }

    public void Init(CardData _data, CardManager _manager)
    {
        data = _data;
        manager = _manager;
        Addressables.LoadAssetAsync<Sprite>(PearentFile + data.CardId + ".png").Completed += OnLoadImage;

        // CardTypeListUIを検索
        cardTypeListUI = FindAnyObjectByType<CardTypeListUI>(FindObjectsInactive.Include);
        // CardTypeListUIがなければ警告
        if (cardTypeListUI == null) Debug.LogError("CardTypeListUIオブジェクトを配置してください");

        nameText.text = data.Name;
        apText.text = data.Ap.ToString();
        if(data.Duration < 0) durationText.text = "継続ターン：-";
        else durationText.text = "継続ターン：" + data.Duration;
        dicText.text = data.EffectText;
    }
    void OnLoadImage(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite sp = handle.Result;
            img.sprite = sp;
        }
    }

    public void TypeChange(CardData _data)
    {
        Init(_data, manager); // 情報の更新
    }

    // カーソルを合わせた時の拡大縮小　表示優先度＋
    // 手札にカーソルを合わせた時　拡大縮小
    public void InCardMouseOver()
    {
        if (useFlag) return;

        this.transform.localPosition += Vector3.up * 30;
        this.transform.SetAsLastSibling();
    }
    public void OutCardMouseOver()
    {
        if (useFlag) return;

        this.transform.localPosition += Vector3.down * 30;
        this.transform.SetSiblingIndex(index);

        activeFlag = false;
        useBtn.SetActive(activeFlag);
    }

    // クリック時追跡　放したら元の場所に(ローカルポジション)
    public void ClickCard()
    {
        if (useFlag) return;

        // 左クリック
        if (Input.GetMouseButtonUp(0)) {
            //activeFlag = !activeFlag;
            //useBtn.SetActive(activeFlag);
            UseCard();
        }

        // 右クリック
        if (Input.GetMouseButtonUp(1)) {
            activeFlag = false;
            useBtn.SetActive(activeFlag);
            cardTypeListUI.OpenUI(data.CardId);
        }
    }

    public void UseCard()
    {
        //ClickCard();
        manager.ReceveCard(index, data);
    }
}
