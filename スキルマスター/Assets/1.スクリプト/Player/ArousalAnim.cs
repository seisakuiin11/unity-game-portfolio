using DG.Tweening;
using UnityEngine;

public class ArousalAnim : MonoBehaviour
{
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 centerPos;
    [SerializeField] Vector3 endPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void Init()
    {
        gameObject.SetActive(true);
        transform.localPosition = startPos;
        //transform.localScale = new Vector3(0f, 1f, 1f);

        //transform.DOScaleX(1f, 0.4f);
        transform.DOLocalMove(centerPos, 0.3f).OnComplete(() => { transform.DOLocalRotate(Vector3.forward * 389, 0.3f, RotateMode.FastBeyond360); });
        DOVirtual.DelayedCall(1.2f, () =>
        {
            //transform.DOScaleX(0f, 0.4f);
            transform.DOLocalMove(endPos, 0.3f).OnComplete(() => { gameObject.SetActive(false); Destroy(gameObject); });
        });
    }
}
