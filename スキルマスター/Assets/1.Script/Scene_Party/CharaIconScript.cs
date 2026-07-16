using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharaIconScript : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] Image img;
    [SerializeField] TextMeshProUGUI text;

    int id;
    CharaIconScript charaIcon;
    Vector3 defultPos;
    PartyDirecter directer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int _id, Sprite _img, string _text, CharaIconScript moveIcon = null, PartyDirecter _directer = null)
    {
        id = _id;
        img.sprite = _img;
        img.SetNativeSize();
        text.text = _text;
        directer = _directer;

        if (moveIcon == null) return;

        charaIcon = moveIcon;
        defultPos = charaIcon.transform.localPosition;
    }

    // ドラッグ開始時
    public void OnBeginDrag(PointerEventData eventData)
    {
        charaIcon.Init(0, img.sprite, text.text);
    }

    // ドラッグ中
    public void OnDrag(PointerEventData eventData)
    {
        charaIcon.transform.position = eventData.position;
    }

    // ドラッグ終了時
    public void OnEndDrag(PointerEventData eventData)
    {
        charaIcon.transform.localPosition = defultPos;
        directer.SetCharaID(id, eventData.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        directer.OpenStatusUI(id);
    }
}
