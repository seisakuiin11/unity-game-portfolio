using System.Threading.Tasks;
using UnityEngine;

public class EffectScript : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] float animTime;

    /// <summary>
    /// アニメーションを再生する
    /// </summary>
    public async void Play(Vector3 pos, float scale = 1f)
    {
        this.transform.position = pos;
        this.transform.localScale = Vector3.one * scale;

        anim.SetTrigger("Play");

        await Task.Delay((int)(animTime * 1000));

        Destroy(this.gameObject);
    }
}
