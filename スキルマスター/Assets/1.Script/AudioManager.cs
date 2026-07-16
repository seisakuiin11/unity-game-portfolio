using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance => instance;

    [SerializeField, Header("BGMプレイヤー")] AudioSource BGMPlayer;
    [SerializeField, Header("SEプレイヤー")] AudioSource SEPlayer;

    enum BGMState
    {
        None,
        Defult,
        Battle
    }
    BGMState bgm;
    [Header("BGM")] // ============================================
    [SerializeField, Header("デフォルトBGM")] AudioClip defultBGM;
    [SerializeField, Header("バトルBGM")] AudioClip battleBGM;

    [Header("SE")] // ============================================
    [SerializeField, Header("ボタンクリック")] AudioClip enterSE;
    [SerializeField, Header("キャンセルボタンクリック")] AudioClip cancelSE;
    [SerializeField, Header("ゲーム開始")] AudioClip startSE;
    [SerializeField, Header("フェイズ切り替え")] AudioClip changePhaseSE;
    [SerializeField, Header("ドロー")] AudioClip drawSE;
    [SerializeField, Header("カード使用")] AudioClip useCardSE;
    [SerializeField, Header("バフ")] AudioClip bafuSE;
    [SerializeField, Header("打撃")] AudioClip blowSE;
    [SerializeField, Header("斬撃")] AudioClip slashSE;
    [SerializeField, Header("アイス魔法")] AudioClip iceSE;

    public void Init()
    {
        // instanceがすでにあったら自分を消去する。
        if (instance && this != instance) return;

        instance = this;
    }

    // BGM ===================================================
    public void DefultBGM()
    {
        if (bgm == BGMState.Defult) return;
        
        bgm = BGMState.Defult;
        BGMPlayer.clip = defultBGM;
        BGMPlayer.Play();
    }
    public void BattleBGM()
    {
        if (bgm == BGMState.Battle) return;
        
        bgm = BGMState.Battle;
        BGMPlayer.clip = battleBGM;
        BGMPlayer.Play();
    }

    // SE ===================================================
    public void EnterSE()
    {
        SEPlayer.PlayOneShot(enterSE);
    }
    public void CancelSE()
    {
        SEPlayer.PlayOneShot(cancelSE);
    }
    public void StartSE()
    {
        SEPlayer.PlayOneShot(startSE);
    }
    public void ChangePhaseSE()
    {
        SEPlayer.PlayOneShot(changePhaseSE);
    }
    public void DrawSE()
    {
        SEPlayer.PlayOneShot(drawSE);
    }
    public void UseCardSE()
    {
        SEPlayer.PlayOneShot(useCardSE);
    }
    public void BafuSE()
    {
        SEPlayer.PlayOneShot(bafuSE);
    }
    public void BlowSE()
    {
        SEPlayer.PlayOneShot(blowSE);
    }
    public void SlashSE()
    {
        SEPlayer.PlayOneShot(slashSE);
    }
    public void IceSE()
    {
        SEPlayer.PlayOneShot(iceSE);
    }
}
