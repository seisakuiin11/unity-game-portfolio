using InGameData;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace InGameData
{
    /// <summary>
    /// 攻撃,ダメージ時のMP回復量
    /// </summary>
    public static class MP
    {
        /// <summary> 攻撃時 </summary>
        public const int Attack = 15;
        /// <summary> ダメージ時 </summary>
        public const int Damage = 10;
        /// <summary> より強力なダメージ時 </summary>
        public const int HighDamage = 20;
    }
    
    public enum Player
    {
        None,
        Mine,
        MyAttackFase,
        Enemy,
        EnemyAttackFase,
        Max
    }
    public struct Targets
    {
        public CharacterScript target;
        public List<CharacterScript> characterTarget;
        public List<CharacterScript> enemyTarget;
        public List<CharacterScript> canSelectTarget;
    }

    [System.Flags]
    public enum SelectMode
    {
        None    = 0,
        Enemy   = 1 << 0,
        Ally    = 1 << 1,
        Single  = 1 << 2,
        All     = 1 << 3,
        Self    = 1 << 4,
    }
    public static class SelectType
    {
        public const SelectMode EnemySingle = SelectMode.Enemy | SelectMode.Single;
        public const SelectMode EnemyAll = SelectMode.Enemy | SelectMode.All;

        public const SelectMode AllySingle = SelectMode.Ally | SelectMode.Single;
        public const SelectMode AllyAll = SelectMode.Ally | SelectMode.All;

        public const SelectMode Self = SelectMode.Single | SelectMode.Self;
    }
    public enum ValueTextType
    {
        Damage,
        Heal,
        Silde,
        DirectHP,
        None
    }

    public class BuffData
    {
        public BuffData() { }
        public BuffData(BuffData data) // 値のコピー
        {
            this.buffName = data.buffName;
            this.buffText = data.buffText;
            this.turn = data.turn;
            this.EndAction = data.EndAction;
        }

        public string buffName;
        public string buffText;
        public int turn;
        public Action<CharacterScript> EndAction;
    }
}

public class GameDirector : MonoBehaviour
{
    const sbyte PLAYER = 1, ENEMY = -1;
    const sbyte DEFULT = 0, SELECT = 1, ACTION = 2;

    [SerializeField] PlayerController playerController;
    [SerializeField] EnemyController enemyController;
    [SerializeField] Image ReActivePanel;   // リセット.パネル

    [Header("UI")]
    [SerializeField] GameObject fadeImg;
    [SerializeField] SpriteRenderer skillPhaseImg;
    [SerializeField] GameObject turnText_obj;
    [SerializeField] TextMeshProUGUI turnText_text;
    [SerializeField] Color skillPhaseColor;
    [SerializeField] Color attackPhaseColor;
    [SerializeField] Color enemyPhaseColor;
    [SerializeField] GameUIManager gameUI;

    List<CharacterScript> characters;
    List<EnemyScript> enemys;

    public bool endGame;
    sbyte turn;
    sbyte state;
    Player phase;
    SelectMode selectMode;
    Targets targets;
    CharacterScript activePlayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characters = new List<CharacterScript>();
        enemys = new List<EnemyScript>();
        turnText_obj.transform.localScale = new Vector3(1,0,1);

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) EndGame(Player.Mine);
    }

    // 初期化 =========================================================================================================
    void Init()
    {
        fadeImg.SetActive(true);

        // プレイヤー側の初期化(ついでにキャラも獲得)
        characters = playerController.Init();

        // エネミー側側の初期化(ついでにキャラも獲得)
        enemys = enemyController.Init();

        // ゲームUIの初期化
        gameUI.Init(characters, enemys);
        state = DEFULT;

        // 少し待ってゲーム開始
        DOVirtual.DelayedCall(0.3f, () => {
            // 画面切り替えの後、ゲームを開始
            fadeImg.transform.DOMoveX(4000f, 1f).OnComplete(StartGame);
        });
    }

    // ゲーム開始を宣言する =============================================================================================
    void StartGame()
    {
        // テキスト表示
        turnText_text.text = "Battle Start";
        turnText_obj.transform.DOScaleY(1f, 0.2f);

        DOVirtual.DelayedCall(1.2f, () => { // 表示待機
            turnText_obj.transform.DOScaleY(0f, 0.2f).OnComplete(() => { // アニメーション終了後
                // ラウンドの開始
                NextRound();

                // ターンの開始
                turn = ENEMY; // Playerスタート
                DOVirtual.DelayedCall(1.45f, () => { NextTurn(); });
            });
        });
    }

    // ゲーム終了を宣言する ----------------------------------------------------------------------------------------------
    async void EndGame(Player winner)
    {
        endGame = true;
        gameUI.DontAction(true);

        // テキスト表示
        turnText_text.text = winner == Player.Mine ? "Game Clear !" : "Game Over";
        turnText_obj.transform.DOScaleY(1f, 0.2f);

        await Task.Delay(1000);

        fadeImg.SetActive(true);
        fadeImg.transform.DOMoveX(960f, 1f);

        await Task.Delay(1200);

        // クエスト選択画面へ移動
        GameManager.Instance.LoadScene(GameManager.Scene.Quest);
    }

    // 次のラウンドへ =====================================================================================================
    public void NextRound()
    {
        int nowRound, maxRound;
        // エネミーの生成
        enemyController.StartRound(out nowRound, out maxRound);

        gameUI.UpdateData();

        // 「ラウンド 1/2」表示
        turnText_text.text = $"Round {nowRound}/{maxRound}";
        turnText_obj.transform.DOScaleY(1f, 0.2f);
        DOVirtual.DelayedCall(1.2f, () => { turnText_obj.transform.DOScaleY(0f, 0.2f); });
    }

    // 次のターンへ =======================================================================================================
    public void NextTurn()
    {
        if (state != DEFULT) return; // 何かしら行動していたら

        ReActive();

        // フェイズ移行(NONE除外)
        phase = (Player)((int)phase % (int)(Player.Max - 1) + 1);
        skillPhaseImg.gameObject.SetActive(false);
        skillPhaseImg.color = new Color(1, 1, 1, 0);

        switch (phase)
        {
            case Player.Mine: // 自分のターン ---------------

                turn = PLAYER;
                enemyController.EndTurn();
                playerController.StartTurn();
                gameUI.DontAction(false);

                // テキスト表示
                skillPhaseImg.gameObject.SetActive(true);
                skillPhaseImg.DOColor(new Color(1, 1, 1, 0.5f), 0.2f);
                
                turnText_text.text = "スキルフェーズ";
                turnText_text.color = skillPhaseColor;

                break;
            case Player.MyAttackFase: // アタックフェイズ --

                playerController.AttackPhase();

                // テキスト表示
                turnText_text.text = "アタックフェーズ";
                turnText_text.color = attackPhaseColor;
                break;
            case Player.Enemy: // 敵のターン ---------------

                turn = ENEMY;
                playerController.EndTurn();
                enemyController.StartTurn();
                gameUI.DontAction(true);

                // テキスト表示
                turnText_text.text = "エネミーターン";
                turnText_text.color = enemyPhaseColor;

                break;
            case Player.EnemyAttackFase: // 敵のアタックフェイズ --

                // enemyContrpller.AttackPhase
                NextTurn(); // 消してね

                return;
            default: // -----------------------------------
                break;
        }

        // テキスト表示
        turnText_obj.transform.DOScaleY(1f, 0.2f);
        DOVirtual.DelayedCall(1.2f, () => { turnText_obj.transform.DOScaleY(0f, 0.2f); });
    }

    // リセット 初期状態にする ==========================================================================================
    public void ReActive()
    {
        if(state != ACTION) state = DEFULT;

        for (int i = 0; i < enemys.Count; i++)
        {
            enemys[i].TargetSelectMode(0, Player.None);
        }

        // ターゲットカーソル非表示
        foreach (var enemy in enemys) enemy.IsTarget(0);
        foreach (var character in characters) character.IsTarget(0);

        playerController.ReActive();

        targets.target = null;
        ReActivePanel.color = new Color32(150, 150, 150, 0);
        gameUI.SelectTiming(false);
        gameUI.TextMessage(false, "");
    }
    // =================================================================================================================

    /// <summary>
    /// 選択可能ターゲットの取得
    /// </summary>
    /// <param name="_activePlayer">アクションを起こしている張本人</param>
    /// <param name="player">自分なのか,相手なのか</param>
    /// <param name="mode">対象となる選択条件</param>
    public List<CharacterScript> GetTarget(CharacterScript _activePlayer, Player player, SelectMode mode)
    {
        selectMode = mode;
        ReActive();
        state = SELECT;

        List<CharacterScript> targetList = new List<CharacterScript>();
        activePlayer = _activePlayer;
        if(activePlayer != null) activePlayer.activePlayer++;
        int isSelect = 0;

        // ターゲット選択をしない場合
        if(selectMode == SelectMode.None)
        {
            PlayerAction(targets);
            // プレイヤー側のアクション処理
            if (turn == PLAYER) playerController.IsAction(targets);
            return null;
        }

        // どちらを対象にするか
        if (mode.HasFlag(SelectMode.Enemy)) isSelect = 1; // エネミーを選択できるようにして、キャラクターを選択できないようにする
        else isSelect = -1; // 味方を選択できるようにして、エネミーを選択できないようにする

        // 選択可能キャラクターをハイライト
        // エネミーの選択状況
        for (int i = 0; i < enemys.Count; i++) {
            if(enemys[i].TargetSelectMode(isSelect, player)) targetList.Add(enemys[i]);
        }
        isSelect *= -1;

        // キャラクターの選択状況
        for (int i = 0; i < characters.Count; i++) {
            // 自分自身だけを選択可能にし、for分を抜ける
            if(mode.HasFlag(SelectMode.Self))
            {
                if (_activePlayer == characters[i]) { characters[i].TargetSelectMode(isSelect, player); targetList.Add(characters[i]); }
                else characters[i].TargetSelectMode(-isSelect, player);
                continue;
            }
            
            if(characters[i].TargetSelectMode(isSelect, player)) targetList.Add(characters[i]);
        }

        gameUI.SelectTiming(true);
        gameUI.TextMessage(true, "ターゲットを選択");

        targets.canSelectTarget = targetList;

        return targetList;
    }

    //*** === キャラクターにターゲットを渡して、アクションを起こす === ***//
    public void IsAction(CharacterScript target)
    {
        // ターゲット選択 1回目
        if (targets.target == null || (selectMode.HasFlag(SelectMode.Single) && targets.target != target))
        {
            targets.target = target;
            targets.characterTarget = characters;
            targets.enemyTarget = new List<CharacterScript>(enemys);

            // ターゲットカーソル表示
            // 一度非表示
            foreach (var enemy in enemys) enemy.IsTarget(0);
            foreach (var character in characters) character.IsTarget(0);

            // 全体選択あり
            if (selectMode.HasFlag(SelectMode.All))
            {
                // エネミーへの全体アクション
                if (selectMode.HasFlag(SelectMode.Enemy)) foreach (var enemy in enemys) enemy.IsTarget(2); // セカンドターゲット
                // プレイヤーへの全体アクション
                if (selectMode.HasFlag(SelectMode.Ally)) foreach (var chara in characters) chara.IsTarget(2); // セカンドターゲット
            }

            // 単体選択あり
            if(selectMode.HasFlag(SelectMode.Single) || selectMode.HasFlag(SelectMode.Self)) target.IsTarget(1); // メインターゲット

            return;
        }

        // 二度目のターゲット選択
        PlayerAction(targets);
        // プレイヤー側のアクション処理
        if (turn == PLAYER) playerController.IsAction(targets);

        ReActive();
    }

    async void PlayerAction(Targets targets)
    {
        if (activePlayer == null) return;

        state = ACTION;
        await activePlayer.Action(targets);
        activePlayer = null;

        await Task.Delay(500);

        // 勝利判定
        if (enemyController.CheckGameClear()) EndGame(Player.Mine);

        // 敗北判定
        if (playerController.CheckGameOver()) EndGame(Player.Enemy);

        gameUI.UpdateData();
        state = DEFULT;
    }

    public void UpdateUI() { gameUI.UpdateData(); }
}
