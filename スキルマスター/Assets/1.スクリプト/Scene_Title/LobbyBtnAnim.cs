using DG.Tweening;
using UnityEngine;

public class LobbyBtnAnim : MonoBehaviour
{
    [SerializeField] GameObject btnIcon;
    [SerializeField] GameObject btnTitle;
    [SerializeField] GameObject btnFrame;
    [SerializeField] Vector3 moveLength;

    Vector3 defultPos;

    private void Awake()
    {
        defultPos = btnTitle.transform.position;
    }

    /// <summary>
    /// ボタンにカーソルが重なったとき
    /// </summary>
    public void HobberBtnAnimEnter()
    {
        btnIcon.transform.rotation = Quaternion.identity;
        btnIcon.transform.DORotate(new Vector3(0, 360, 0), 0.4f, RotateMode.FastBeyond360);
        btnTitle.transform.DOMove(defultPos + moveLength, 0.2f);
        btnFrame.transform.DOScale(Vector3.one * 1.02f, 0.2f);
    }
    /// <summary>
    /// ボタンからカーソルが遠のいたとき
    /// </summary>
    public void HobberBtnAnimExit()
    {
        btnTitle.transform.DOMove(defultPos, 0.2f);
        btnFrame.transform.DOScale(Vector3.one, 0.2f);
    }
}
