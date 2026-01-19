using UnityEngine;
using CommonData;
using InGameData;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CharacterScript : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] protected Player player;
    [SerializeField] protected string charaName;
    [SerializeField] protected int hp;
    [SerializeField] protected int attack;
    [SerializeField] protected int magicAttack;

    [SerializeField] GameObject targetPanel;
    [SerializeField] GameObject notTargetPanel;

    [Header("UI")]
    [SerializeField] Slider hpBar;
    [SerializeField] GameObject activeImg;
    [SerializeField] GameObject targetCursoll;
    [SerializeField] ActionValueText valueText;

    [Header("キャラクターイラスト")]
    protected CharacterSprite charaImg;
    [SerializeField] Sprite[] charaSprites;
    [SerializeField] string[] typeNames;

    Vector3 defultPos;
    float moveLength = 5f;
    public int charID;
    [Header("各ステータス倍率")]
    public int addHP;                        // 追加HP
    public int addAttack;                    // 追加攻撃力
    public int addMagicAttack;               // 追加魔法攻撃力
    public int defultAttackMultiplier = 100; // 通常攻撃倍率
    public int dealtDamageMultiplier = 100;  // 与ダメージ倍率
    public int takeDamageMultiplier = 100;   // 被ダメージ倍率

    [HideInInspector] public int damage;     // 与える,受けるダメージ
    [HideInInspector] public int silde;      // シールド
    [HideInInspector] public int nowHp;      // 現在HP
    /// <summary> 最終最大HP </summary>
    public int maxHp => hp + addHP;
    /// <summary> 最終攻撃力 </summary>
    public int at => attack + addAttack;
    /// <summary> 最終魔法攻撃力 </summary>
    public int ma => magicAttack + addMagicAttack;

    // キャラクター基礎データ ------------------------------------------------
    /// <summary> エフェクト等を発生させるポジション </summary>
    public Vector3 AnimPos { get { return dedFlag ? Vector3.one * -100 : charaImg.transform.position; } } // 死んでいたら、遠い場所にする
    /// <summary> キャラクターの現在の画像 </summary>
    public Sprite CharaSprite { get { return charaImg.GetSprite(); } }
    /// <summary> キャラクターの名前 </summary>
    public string CharaName { get { return charaName; } }
    /// <summary> キャラクターの潜在覚醒のタイプ </summary>
    public string ArousalType { get { return typeNames[typeID]; } }
    /// <summary> 基礎HP </summary>
    public int defultHP { get { return hp; } }
    /// <summary> 基礎攻撃力 </summary>
    public int defultAttack { get { return attack; } }
    /// <summary> 基礎魔法攻撃力 </summary>
    public int defultMagicAttack { get { return magicAttack; } }
    /// <summary> 攻撃内容がどうなってるか </summary>
    public string SkillName { get { return skillName; } }
    // -----------------------------------------------------------------------

    [HideInInspector] public int maxType;           // 潜在覚醒の最大総数
    [HideInInspector] public bool selectFlag;       // 相手から選択できる状態
    [HideInInspector] public bool dedFlag;          // 死亡
    [HideInInspector] public bool activeFlag;       // アクションを起こしているか
    [HideInInspector] public int activePlayer;     // 現在行動しようとしている
    protected int typeID;                           // 潜在覚醒のタイプ
    protected Func<Targets, Task>[,] action;        // スキル アクション
    protected Action[] arousalEffect;               // 潜在覚醒時の効果
    protected SelectMode[,] actionOfSelectType;     // ターゲット対象
    protected int skillID;                          // スキルID
    protected CardData skillData;                   // スキルのデータ
    protected EquipItem equipItem;                  // 装備アイテム

    string skillName;                        // 攻撃の名前
    SelectMode selectMode;                   // 攻撃ターゲット
    List<Func<Targets, Task>> attackAction;  // 攻撃内容

    protected PlayerController playerController;
    protected EnemyController enemyController;
    protected EffectCliater ef;

    //-----------------------------------------------------------
    public List<Action<CharacterScript>> attackTimingAction;         // 攻撃時に行う処理
    public List<Action<CharacterScript>> defultAttackTimingAction;   // 通常攻撃時に行う処理
    public List<Action<CharacterScript>> damageTimingAction;         // 攻撃を受けたときに行う処理

    //-----------------------------------------------------------
    public List<BuffData> buffDatas;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        attackTimingAction = new List<Action<CharacterScript>>();
        defultAttackTimingAction = new List<Action<CharacterScript>>();
        damageTimingAction = new List<Action<CharacterScript>>();
        buffDatas = new List<BuffData>();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_director">ゲームの管理者</param>
    /// <param name="_player">プレイヤーor敵</param>
    /// <param name="_img">ワールド座標に存在するSprite</param>
    /// <param name="index">表示優先順位</param>
    /// <param name="_playerController"></param>
    /// <param name="_enemyController"></param>
    public virtual void Init(GameDirector _director, Player _player, CharacterSprite _sprt, int index, CharaData _data = null, PlayerController _playerController = null, EnemyController _enemyController = null)
    {
        playerController = _playerController;
        enemyController = _enemyController;
        player = _player;
        charaImg = _sprt;
        charaImg.SetIndex(index);
        selectFlag = true;
        ef = FindAnyObjectByType<EffectCliater>();

        // スキルの設定
        SetSkillStanby();
        // 攻撃内容の初期化
        attackAction = new List<Func<Targets, Task>>() { action[typeID, 0] };
        selectMode = actionOfSelectType[typeID, 0];

        // ステータス セット
        if (_data != null)
        {
            charaName = _data.Name;
            hp = _data.HP;
            attack = _data.Attack;
            magicAttack = _data.MagicAttack;
            dealtDamageMultiplier = _data.Deal;
            takeDamageMultiplier = _data.Take;
        }

        // イベントトリガーの設定　Inspectorには表示されないよ
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        CharacterScript target = this;
        entry.callback.AddListener((eventDate) => _director.IsAction(target));
        targetPanel.GetComponent<EventTrigger>().triggers.Add(entry);

        // UI初期化
        hpBar.maxValue = maxHp;
        hpBar.value = maxHp;
        nowHp = maxHp;

        // Imgの設定
        charaImg.SetSprite(charaSprites[typeID]);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint( new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 5f) );
        charaImg.transform.position = worldPos;

        // 登場アニメーション
        defultPos = charaImg.transform.position;
        charaImg.transform.position += Vector3.left * moveLength * (player == Player.Mine ? 1 : -1);
        charaImg.transform.DOMove(defultPos, 0.3f);
    }

    /*** ================================================================================== ***/
    public void AddSilde(int num)
    {
        if(dedFlag) return;

        silde += num;
    }

    /// <summary> 最大HPアップ </summary>
    public void MaxHpUp(int num)
    {
        if (dedFlag) return;

        addHP += num;

        hpBar.maxValue = maxHp;
        Heal(num);
    }

    /// <summary> 最大HPダウン </summary>
    public void MaxHpDown(int num)
    {
        if (dedFlag) return;

        addHP -= num;
        if (nowHp > maxHp) nowHp = maxHp;

        hpBar.maxValue = maxHp;
        hpBar.value = nowHp;
    }

    /// <summary> 回復 </summary>
    public void Heal(int num)
    {
        if (dedFlag) return;

        nowHp += num;
        if (nowHp > maxHp) nowHp = maxHp;
        hpBar.value = nowHp;
    }

    /// <summary> ダメージ(回復不可) </summary>
    public void Damage(int _damage, int _mp, ValueTextType damageType = ValueTextType.Damage)
    {
        if(dedFlag) return;
        
        damage = Mathf.RoundToInt(_damage * takeDamageMultiplier * 0.01f);
        
        // ダメージを受けた時の効果
        foreach (var action in damageTimingAction) action(this);

        // ダメージアニメーション
        charaImg.transform.DOShakePosition(0.2f, 0.5f, 20, fadeOut: false).OnComplete(() => { charaImg.transform.position = defultPos; });

        // シールドがあるなら ---------------------------------------
        if (silde > 0 && damageType != ValueTextType.DirectHP)
        {
            // シールドを削る
            if (damage > silde) { // シールドで防ぎきれないとき
                damage -= silde;
                // ダメージテキスト生成
                Instantiate(valueText).Init(silde, transform.position, ValueTextType.Silde);
                silde = 0;
            }
            else { // シールドがダメージ以上にあった場合
                silde -= damage;
                // ダメージテキスト生成
                Instantiate(valueText).Init(damage, transform.position, ValueTextType.Silde);
                damage = 0;
            }

            // ダメージが0になったら 先へ進まない
            if (damage <= 0) return;
        }

        nowHp -= damage;
        hpBar.value = nowHp;
        // ダメージテキスト生成
        Instantiate(valueText).Init(damage, transform.position, ValueTextType.Damage);

        // ダメージによるMP回復
        if (playerController != null) playerController.AddMP(_mp);

        // 死亡判定
        if (nowHp <= 0) Dead();
    }
    void Dead()
    {
        dedFlag = true;
        charaImg.SetColor(new Color(0.6f, 0.6f, 0.6f, 1f));
        notTargetPanel.SetActive(true);
        targetCursoll.SetActive(false);
    }
    /* -------------------------------------------------------------------------------------------- */
    /// <summary>
    /// フェイズ切り替え
    /// </summary>
    public void ChangePhase(Player _player)
    {
        player = _player;

        // 攻撃内容のリセット
        if (player == Player.Mine)
        {
            attackAction = new List<Func<Targets, Task>>() { action[typeID, 0] };
            selectMode = actionOfSelectType[typeID, 0];
        }
    }

    /// <summary>
    /// 潜在覚醒
    /// </summary>
    public void LatentArousal(int _typeID)
    {
        if (dedFlag) return;

        if (typeID != 0) // すでにタイプチェンジしていたら
        {
            return;
        }

        typeID = _typeID;
        charaImg.SetSprite(charaSprites[typeID]);
        arousalEffect[typeID]();

        if(skillName == null || skillName == "通常攻撃") ChangeAction("通常攻撃", actionOfSelectType[typeID,0], action[typeID, 0]);
    }

    /// <summary> 現在の潜在覚醒のタイプIDを返す </summary>
    public int GetTypeID()
    {
        return typeID;
    }

    /// <summary>
    /// 相手にダメージを与える
    /// </summary>
    protected void AttackAction(CharacterScript target, float _damage, int _mp = MP.Damage)
    {
        if (dedFlag) return;

        damage = Mathf.RoundToInt(_damage * dealtDamageMultiplier * 0.01f);

        // 攻撃時の効果発動
        foreach (var action in attackTimingAction) action(this);

        // ダメージを与える
        target.Damage(damage, _mp);

        // 攻撃によるMP回復
        if (playerController != null) playerController.AddMP(MP.Attack);
    }

    /// <summary>
    /// 相手にダメージを与える
    /// </summary>
    protected void AttackAction(List<CharacterScript> targets, float _damage, int _mp = MP.Damage)
    {
        if (dedFlag) return;

        damage = Mathf.RoundToInt(_damage * dealtDamageMultiplier * 0.01f);

        // 攻撃時の効果発動
        foreach (var action in attackTimingAction) action(this);

        // ダメージを与える
        foreach(var target  in targets) target.Damage(damage, _mp);

        // 攻撃によるMP回復
        if (playerController != null) playerController.AddMP(MP.Attack);
    }

    // スキルIDを取得
    public void SetSkillID(int id, CardData _data = null)
    {
        skillID = id; // ID取得
        skillData = _data;
    }

    /// <summary> このキャラクターを選択可能状態にできる </summary>
    /// <param name="flag"> 選択可能状態にするか　0:通常状態　1:選択する　-1:選択しない </param>
    /// <param name="_player"> 誰が選択を行っているのか </param>
    public bool TargetSelectMode(int flag, Player _player)
    {
        if (dedFlag) return false;

        if(flag == 0) { // 通常状態
            targetPanel.SetActive(false);
            notTargetPanel.SetActive(false);
            activeImg.SetActive(false);

            activePlayer = activePlayer == 0 ? 0 : (activePlayer - 1) * 2; // 0⇒0, 1⇒0, 2⇒2
            // スキルフェイズなら
            if (player == Player.Mine || player == Player.Enemy) activePlayer = 0;
            
            charaImg.SetColor(Color.white);
            // もう行動できないなら
            if(activePlayer == 2) charaImg.SetColor(new Color(0.6f, 0.6f, 0.6f, 1f));

            return false;
        }

        bool isSelect = true;
        if (flag < 0 || (_player != player && !selectFlag)) isSelect = false;

        targetPanel.SetActive(isSelect);
        notTargetPanel.SetActive(!isSelect);
        if (!isSelect) charaImg.SetColor(new Color(0.6f, 0.6f, 0.6f, 1f));


        if (activePlayer == 1) // 行動するキャラクターなら
        {
            activeImg.SetActive(true);
            charaImg.SetColor(Color.white);
        }

        return isSelect;
    }

    /// <summary>
    /// 選択しているかのターゲットカーソルを表示する
    /// </summary>
    /// <param name="_flag">0:非選択 1:ファーストターゲット 2:セカンドターゲット</param>
    public void IsTarget(int _flag)
    {
        if (dedFlag) return;

        bool flag = false;
        if(_flag > 0) flag = true;

        targetCursoll.SetActive(flag);

        if (_flag > 1) targetCursoll.transform.localScale = Vector3.one * 0.7f;
        else targetCursoll.transform.localScale = Vector3.one;
    }

    // ターゲットを取得して、スキルIDをもとにアクションを行う
    public async Task Action(Targets target)
    {
        // スキル発動
        if(player == Player.Mine)
        {
            await action[typeID, skillID](target);
            return;
        }

        activePlayer++;
        // 攻撃フェイズ
        if (skillName == "通常攻撃") {
            // 通常攻撃時効果
            foreach (var action in defultAttackTimingAction) action(this);
        }

        // 攻撃内容をすべて実行
        var tasks = new List<Task>();
        foreach (var ac in attackAction) tasks.Add(ac(target));
        // すべての攻撃が終わったら、終了
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// スキルを配列に格納していく
    /// </summary>
    protected virtual void SetSkillStanby()
    {
        maxType = 2;

        // スキル 処理
        action = new Func<Targets, Task>[2, 2] {
            { Attack, AllAttack }, // 通常状態
            { Attack, AllAttack }, // 覚醒：○○
        };
        // スキルの選択対象
        actionOfSelectType = new SelectMode[2, 2] {
            { SelectType.EnemySingle, SelectType.EnemyAll }, // 通常状態
            { SelectType.EnemySingle, SelectType.EnemyAll }, // 覚醒：○○
        };
    }

    // バフ関係 =============================================================================================
    /// <summary>
    /// 攻撃内容を書き換える
    /// </summary>
    /// <param name="_selectMode"></param>
    /// <param name="_action"></param>
    public void ChangeAction(string _skillName, SelectMode _selectMode, Func<Targets, Task> _action)
    {
        skillName = _skillName;
        selectMode = _selectMode;
        attackAction[0] = _action;

        ef.Buff(AnimPos);
    }
    /// <summary>
    /// 攻撃内容を追加する
    /// </summary>
    /// <param name="_action"></param>
    public void AddAction(Func<Targets, Task> _action)
    {
        attackAction.Add(_action);

        ef.Buff(AnimPos);
    }

    /// <summary> バフ効果付与 </summary>
    public void SetBuffData(BuffData buff, bool updateFlag = false)
    {
        if (dedFlag) return;

        if (updateFlag) // 更新するタイプのバフ
        {
            // 同一の処理を検索、削除
            foreach (var item in buffDatas)
            {
                if (item.buffName != buff.buffName) continue;

                buffDatas.Remove(item);
                break;
            }
        }

        buffDatas.Add(buff);
    }
    /// <summary>
    /// 指定バフを強制的に削除する
    /// </summary>
    public bool RemoveBuffData(string buffName, bool allFlag = false)
    {
        bool flag = false;

        // 検索 → 削除
        foreach (var item in buffDatas)
        {
            if(item.buffName != buffName) continue;

            buffDatas.Remove(item);
            flag = true;
            if (!allFlag) break; // 全削除ではない
        }

        return flag;
    }
    /// <summary> ターン経過 </summary>
    public void RemoveTurn()
    {
        foreach (var item in buffDatas) item.turn--;
    }
    /// <summary> ターン経過確認 ターンエンド処理 </summary>
    public void CheckTurnProgress()
    {
        activePlayer = 0;
        // 残りターン確認
        for (int i = buffDatas.Count - 1; i >= 0; i--)
        {
            if (buffDatas[i].turn != 0) continue;

            buffDatas[i].EndAction(this);
            buffDatas.RemoveAt(i);
        }
    }

    // ==========================================================================================================
    // 攻撃アクション - UIクリック時の処理
    public void DefaultAttack()
    {
        if (dedFlag) return;

        // 右クリックなら、キャラクター情報を表示
        if (Input.GetMouseButtonUp(1))
        {
            // キャラクターステータス表示
            StatusUI.Instance.OpenStatusUI(this);

            return;
        }

        if (activePlayer != 0) return;

        // プレイヤーの攻撃フェイズ
        if(player == Player.MyAttackFase) playerController.SelectMode(this, selectMode);
    }

    /// <summary>
    /// アイテムを装備する
    /// </summary>
    public void Equip(EquipItem _equipItem)
    {
        if (equipItem != null)
        {
            equipItem.EndAction(this);
            equipItem = null;
        }

        equipItem = _equipItem;
    }
    // ==========================================================================================================

    /*** -------------------- 通常状態(テスト) ID: 0 ----------------------- ***/

    async Task Attack(Targets target) // ID: 0
    {
        await Task.Delay(100);
        AttackAction(target.target, attack);
    }
    async Task AllAttack(Targets target) // ID: 1
    {
        await Task.Delay(100);

        AttackAction(target.enemyTarget, attack);
    }
}