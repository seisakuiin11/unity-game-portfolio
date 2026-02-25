using UnityEngine;

public class EffectCliater : MonoBehaviour
{
    [SerializeField] EffectScript[] effects;
    const int SLASH = 0, SMASH = 1, MAGIC = 2, HEAL = 3, BUFF = 4, DEBUFF = 5;
    const int ICE = 6, ICE_EGE = 7;

    /// <summary> スラッシュ攻撃 </summary>
    public void Slash(Vector3 pos)
    {
        Instantiate(effects[SLASH]).Play(pos);
        AudioManager.Instance.SlashSE();
    }
    /// <summary> スマッシュ攻撃 </summary>
    public void Smash(Vector3 pos)
    {
        Instantiate(effects[SMASH]).Play(pos);
        AudioManager.Instance.BlowSE();
    }
    /// <summary> 魔法攻撃 </summary>
    public void Magic(Vector3 pos)
    {
        Instantiate(effects[MAGIC]).Play(pos);
    }
    /// <summary> 回復 </summary>
    public void Heal(Vector3 pos)
    {
        Instantiate(effects[HEAL]).Play(pos);
    }
    /// <summary> バフ効果 </summary>
    public void Buff(Vector3 pos)
    {
        Instantiate(effects[BUFF]).Play(pos);
        AudioManager.Instance.BafuSE();
    }
    /// <summary> デバフ効果 </summary>
    public void Debuff(Vector3 pos)
    {
        Instantiate(effects[DEBUFF]).Play(pos);
    }
    /// <summary> 氷魔法攻撃 </summary>
    public void Ice(Vector3 pos)
    {
        Instantiate(effects[ICE]).Play(pos);
        AudioManager.Instance.IceSE();
    }
    /// <summary> 上級氷魔法攻撃 </summary>
    public void IceEge(Vector3 pos)
    {
        Instantiate(effects[ICE_EGE]).Play(pos);
        AudioManager.Instance.IceSE();
    }
}
