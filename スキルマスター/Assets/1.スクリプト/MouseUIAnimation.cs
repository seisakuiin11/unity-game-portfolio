using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseUIAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    enum AnimType
    {
        BIG,
        SMALE,
        PULSE,
        COLOR,
        FADE,
        MOVE,
        MOVELOOP,
        MOVERESET
    }

    [System.Serializable]
    class UIobj
    {
        public Image obj;
        [HideInInspector] public Vector3 defultPos;
        [HideInInspector] public Vector3 defultScale;
        [HideInInspector] public Color defultColor;
        public List<AnimType> animTypes;
    }

    [Header("永続的にアニメーションする"), SerializeField] List<UIobj> obj_start;
    [Header("PointerEnterEvent"), SerializeField] List<UIobj> obj_enter;
    [Header("PointerExitEvent"), SerializeField] List<UIobj> obj_exit;

    [Header("1ループのアニメーション時間"), SerializeField] float loopAnimTime = 2f;
    [Header("アニメーションにかかる時間"), SerializeField] float animTime = 0.2f;
    [Header("拡大スケール"), SerializeField] Vector3 bigScale = Vector3.one;
    [Header("縮小スケール"), SerializeField] Vector3 smaleScale = Vector3.one;
    [Header("収縮スケール"), SerializeField] Vector3 pulseScale = Vector3.one * 1.2f;
    [Header("変更後色"), SerializeField] Color changeColor = new Color(1, 1, 1, 0);
    [Header("明滅色"), SerializeField] Color FadeColor = new Color(1, 1, 1, 0);
    [Header("移動ベクトル"), SerializeField] Vector3 moveVec = new Vector3(0, 10, 0);

    Dictionary<AnimType, Action<UIobj>> anims;

    void Awake()
    {
        anims = new Dictionary<AnimType, Action<UIobj>>()
        {
            { AnimType.BIG, BigAnim },
            { AnimType.SMALE, SmaleAnim },
            { AnimType.PULSE, PulseAnim },
            { AnimType.COLOR, ColorChangeAnim },
            { AnimType.FADE, FadeAnim },
            { AnimType.MOVE, MoveAnim },
            { AnimType.MOVELOOP, MoveLoop },
            { AnimType.MOVERESET, MoveResetAnim },
        };
    }
    // スクリプトを追加したとき
    void Reset()
    {
        var img = GetComponent<Image>();
        var objLo = new UIobj() { obj = img, animTypes = new List<AnimType>() { AnimType.PULSE } };
        obj_start = new List<UIobj> { objLo };
        var objEn = new UIobj() { obj = img, animTypes = new List<AnimType>() { AnimType.BIG } };
        obj_enter = new List<UIobj> { objEn };
        var objEx = new UIobj() { obj = img, animTypes = new List<AnimType>() { AnimType.SMALE } };
        obj_exit = new List<UIobj> { objEx };
    }
    // アクティブ時
    void OnEnable()
    {
        foreach (var obj in obj_enter)
        {
            obj.defultPos = obj.obj.transform.position;
            obj.defultScale = obj.obj.transform.localScale;
            obj.defultColor = obj.obj.color;
        }

        foreach (var obj in obj_exit)
        {
            obj.defultPos = obj.obj.transform.position;
            obj.defultScale = obj.obj.transform.localScale;
            obj.defultColor = obj.obj.color;
        }

        foreach (var obj in obj_start)
        {
            obj.defultPos = obj.obj.transform.position;
            obj.defultScale = obj.obj.transform.localScale;
            obj.defultColor = obj.obj.color;
            foreach (var type in obj.animTypes) anims[type](obj);
        }
    }
    // 非アクティブ時
    void OnDisable()
    {
        foreach (var obj in obj_start)
        {
            obj.obj.DOKill();
            obj.obj.transform.DOKill();
            obj.obj.transform.position = obj.defultPos;
            obj.obj.transform.localScale = obj.defultScale;
            obj.obj.color = obj.defultColor;
        }
        foreach(var obj in obj_enter)
        {
            obj.obj.DOKill();
            obj.obj.transform.DOKill();
            obj.obj.transform.position = obj.defultPos;
            obj.obj.transform.localScale = obj.defultScale;
            obj.obj.color = obj.defultColor;
        }
    }

    // -------------------------
    // カーソルが入ったら
    // -------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var obj in obj_enter) {
            obj.obj.DOKill();
            obj.obj.transform.DOKill();
            foreach (var type in obj.animTypes) anims[type](obj);
        }
    }
    // -------------------------
    // カーソルが出て行ったら
    // -------------------------
    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var obj in obj_exit) {
            obj.obj.DOKill();
            obj.obj.transform.DOKill();
            foreach (var type in obj.animTypes) anims[type](obj);
        }
    }

    void BigAnim(UIobj obj)
    {
        Vector3 scale = new Vector3(obj.defultScale.x * bigScale.x, obj.defultScale.y * bigScale.y, obj.defultScale.z * bigScale.z);
        obj.obj.transform.DOScale(scale, animTime);
    }

    void SmaleAnim(UIobj obj)
    {
        Vector3 scale = new Vector3(obj.defultScale.x * smaleScale.x, obj.defultScale.y * smaleScale.y, obj.defultScale.z * smaleScale.z);
        obj.obj.transform.DOScale(scale, animTime);
    }

    void PulseAnim(UIobj obj)
    {
        Vector3 scale = new Vector3(obj.defultScale.x * pulseScale.x, obj.defultScale.y * pulseScale.y, obj.defultScale.z * pulseScale.z);
        obj.obj.transform.DOScale(scale, loopAnimTime).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void ColorChangeAnim(UIobj obj)
    {
        obj.obj.DOColor(changeColor, animTime);
    }

    void FadeAnim(UIobj obj)
    {
        obj.obj.DOColor(FadeColor, loopAnimTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void MoveAnim(UIobj obj)
    {
        obj.obj.transform.DOMove(obj.defultPos + moveVec, animTime);
    }

    void MoveLoop(UIobj obj)
    {
        obj.obj.transform.DOMove(obj.defultPos + moveVec, loopAnimTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void MoveResetAnim(UIobj obj)
    {
        obj.obj.transform.DOMove(obj.defultPos, animTime);
    }
}
