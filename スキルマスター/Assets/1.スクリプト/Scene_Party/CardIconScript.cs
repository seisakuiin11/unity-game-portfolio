using CommonData;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardIconScript : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] Image img;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI apText;
    [SerializeField] TextMeshProUGUI durationText;
    [SerializeField] TextMeshProUGUI dicText;
    [SerializeField] TextMeshProUGUI countText;

    string PearentFile = "Assets/3.素材/Card/";
    int state;
    CardData data;
    int count;
    CardTypeListUI cardTypeListUI;
    CardIconScript moveIcon;
    PartyDirecter directer;
    Vector3 defultPos;


    public void Init(CardData _data, int _state, CardIconScript _moveIcon = null, PartyDirecter _directer = null)
    {
        data = _data;
        state = _state;
        moveIcon = _moveIcon;
        directer = _directer;
        count = 0;

        Addressables.LoadAssetAsync<Sprite>(PearentFile + data.CardId + ".png").Completed += OnLoadImage;

        // CardTypeListUIを検索
        cardTypeListUI = FindAnyObjectByType<CardTypeListUI>(FindObjectsInactive.Include);
        // CardTypeListUIがなければ警告
        if (cardTypeListUI == null) Debug.LogError("CardTypeListUIオブジェクトを配置してください");

        nameText.text = data.Name;
        apText.text = data.Ap.ToString();
        if (data.Duration < 0) durationText.text = "継続ターン：-";
        else durationText.text = "継続ターン：" + data.Duration;
        dicText.text = data.EffectText;
        countText.text = "+"+count;

        if (moveIcon == null) return;

        defultPos = moveIcon.transform.position;
    }
    void OnLoadImage(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite sp = handle.Result;
            img.sprite = sp;
        }
    }

    public void CountUp(int num)
    {
        count += num;
        countText.text = "+" + count;
    }
    public int GetCount() { return count; }

    // クリック時(主に右クリック時)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 右クリックなら
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            cardTypeListUI.OpenUI(data.CardId);
        }
    }

    // ドラッグ開始時
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (state == 0) return;

        moveIcon.Init(data, 0);
        moveIcon.CountUp(count);
    }

    // ドラッグ中
    public void OnDrag(PointerEventData eventData)
    {
        if(state == 0) return;

        moveIcon.transform.position = eventData.position;
    }

    // ドラッグ終了時
    public void OnEndDrag(PointerEventData eventData)
    {
        if (state == 0) return;

        moveIcon.transform.position = defultPos;
        if (state == 1) if(directer.AddDekkiCard(eventData.position, data.CardId)) CountUp(1);
        if (state == 2) if(directer.RemoveDekkiCard(eventData.position, data.CardId)) CountUp(-1);
    }
}
