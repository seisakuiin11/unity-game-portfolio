using InGameData;
using CommonData;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("キャラクター プレハブ")]
    [SerializeField] CharacterScript[] PlayerPlefab;
    [SerializeField] GameObject PlayerPearent;
    [SerializeField] Vector3[] playerPos;
    [SerializeField] CharacterSprite charaSprite; // アニメーション,エフェクトなどの対象オブジェクト

    [SerializeField, Header("ゲーム開始時の手札枚数")] int startHandNum;
    [SerializeField, Header("ターン開始時追加AP")] int startTurnAP;
    [SerializeField, Header("ターン開始時追加MP")] int startTurnMP;
    [SerializeField, Header("攻撃時追加MP")] int attackMP;
    /// <summary> 攻撃時のMP回復量 </summary>
    public int AttackMP { get { return attackMP; } }
    [SerializeField, Header("ダメージ時追加MP")] int damageMP;
    /// <summary> ダメージ時のMP回復量 </summary>
    public int DamageMP { get { return damageMP; } }
    [SerializeField, Header("覚醒に必要なMP")] int arousalMP;
    [SerializeField, Header("貯めこめる最大MP")] int arousalMPMax;
    [SerializeField] CardManager dekki;
    [SerializeField] GameDirector director;

    [Header("UI")]
    [SerializeField] PlayerUIManager playerUI;

    bool arousalFlag;
    bool turn;
    int ap;
    int mp;
    List<CharacterScript> characters;
    SupportItems supportItem;
    CharacterScript arousalPlayer;
    List<CharaData> charaDatas;

    //-----------------------------------------------------------
    List<BuffData> buffDatas; // ターン経過時の処理
    // ターン開始時に行う処理
    // ターン終了時に行う処理
    public List<Action<int>> apUseTimingAction; // 行動値を消費したときに行う処理
    // カードを使用する時に行う処理
    //-----------------------------------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buffDatas = new List<BuffData>();
        apUseTimingAction = new List<Action<int>>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*** 初期化 ***/
    public List<CharacterScript> Init()
    {
        // キャラクター情報を取得
        var manager = FindAnyObjectByType<DataBaseManager>();
        charaDatas = manager.GetCharaData();
        int num = GameManager.Instance.saveDataIndex;
        SaveData data = manager.Load($"save{num}");

        int underIndex = 0;
        int[] layerIndex = new int[3];
        characters = new List<CharacterScript>();
        // キャラクターの生成
        for (int i = 0; i < data.charaID.Length; i++)
        {
            CharacterScript player = Instantiate(PlayerPlefab[data.charaID[i]-1], PlayerPearent.transform);
            player.transform.localPosition = playerPos[i];
            layerIndex[i] = (int)playerPos[i].y / -100;
            if(underIndex > layerIndex[i]) underIndex = layerIndex[i];
            player.Init(director, Player.Mine, Instantiate(charaSprite), layerIndex[i], charaDatas[data.charaID[i]-1], _playerController: this);
            characters.Add(player);
        }
        // 奥行の調整
        for(int i = 0; i < characters.Count; i++) characters[i].transform.SetSiblingIndex(layerIndex[i] - underIndex);

        // デッキの初期化
        dekki.Init(data, manager.GetCardData());
        // カードマネージャーにキャラクター情報を渡す
        dekki.SetCharacter(characters.ToArray());
        // 手札の獲得
        Draw(startHandNum);
        dekki.NotHandActive();

        // UIの初期化
        playerUI.Init(this, arousalMP);
        playerUI.UpdateAPUI(ap);
        playerUI.UpdateEPUI(mp);

        return characters;
    }

    public void ReActive()
    {
        arousalFlag = false;

        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].TargetSelectMode(0, Player.None);
        }

        // 手札を下げる
        dekki.NotHandActive();
    }

    // AP ----------------------------------------------------------------------------------
    /*** AP回復(ターン開始時に追加する) ***/
    public void AddAP(int num)
    {
        ap += num;
        playerUI.UpdateAPUI(ap);
    }
    /*** AP消費 ***/
    public void RemoveAP(int num)
    {
        // AP消費時の効果
        foreach (var action in apUseTimingAction) action(num);

        ap -= num;
        playerUI.UpdateAPUI(ap);
    }

    /*** APが足りるか確認 ***/
    public bool CheckAP(int num)
    {
        if (ap < num)
        {
            playerUI.ShowCautionText("AP");
        }
        return ap >= num;
    }

    // MP ---------------------------------------------------------------------------------
    /*** MP回復(ターン開始時,AP消費で回復する) ***/
    public void AddMP(int num)
    {
        mp += num;

        if(mp > arousalMPMax) mp = arousalMPMax;

        playerUI.UpdateEPUI(mp);
    }

    /*** MP消費 ***/
    void RemoveMP(int num)
    {
        mp -= num;

        playerUI.UpdateEPUI(mp);
    }

    /*** ドロー ***/
    public void Draw(int num)
    {
        dekki.Draw(num);
    }

    /*** 自分のターンか確認 ***/
    public bool CheckTurn()
    {
        return turn;
    }

    /// <summary> ターン経過後に行う処理リスト </summary>
    public void SetTurnCounter(BuffData buff, bool updateFlag = false)
    {
        if (updateFlag)
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

    public void AddUseCardTimingAction(Action<CardData> action, bool updateFlag = false)
    {
        if (updateFlag)
        {
            // 同一の処理を検索、削除
            foreach (var item in dekki.useCardTimingAction)
            {
                if (item != action) continue;

                dekki.useCardTimingAction.Remove(item);
                break;
            }
        }

        dekki.useCardTimingAction.Add(action);
    }
    public void RemoveUseCardTimingAction(Action<CardData> action)
    {
        dekki.useCardTimingAction.Remove(action);
    }

    /*** =================================== ターン開始時 =================================== ***/
    public void StartTurn()
    {
        turn = true;
        dekki.SetSkillPhase(true);
        int drawNum = 1;

        // 残りターン確認
        for (int i = buffDatas.Count - 1; i >= 0; i--)
        {
            if (buffDatas[i].turn != 0) continue;

            buffDatas[i].EndAction(null);
            buffDatas.RemoveAt(i);
        }
        // ターン確認とフェイズ切り替え
        foreach (var chara in characters) { chara.ChangePhase(Player.Mine); chara.CheckTurnProgress(); }

        Draw(drawNum);
        AddAP(startTurnAP);
        AddMP(startTurnMP);

        // ターン経過
        for (int i = 0; i < buffDatas.Count; i++) buffDatas[i].turn--;
        foreach(var chara in characters) chara.RemoveTurn();
    }

    /// <summary>
    /// 攻撃フェイズに切り替える
    /// </summary>
    public void AttackPhase()
    {
        foreach (var chara in characters) chara.ChangePhase(Player.MyAttackFase);
        dekki.SetSkillPhase(false);
    }

    /*** ターゲット選択(AP確認) ***/
    public void SelectMode(CharacterScript _activePlayer, SelectMode mode)
    {
        // IDをもとにターゲット条件を変更
        // ターゲット条件をもとにターゲット選択
        var targetList = director.GetTarget(_activePlayer, Player.Mine, mode);
        if(targetList == null) return;

        // 最優先候補のターゲットを選択
        director.IsAction(targetList[0]);
    }
    public void SelectMode(SupportItems items, SelectMode mode)
    {
        supportItem = items;
        var targetList = director.GetTarget(null, Player.Mine, mode);
        if (targetList == null) return;

        // 最優先候補のターゲットを選択
        director.IsAction(targetList[0]);
    }

    /*** キャラクターがアクションを起こした後 ***/
    public void IsAction(Targets target)
    {
        // 潜在覚醒の選択なら
        if (arousalFlag)
        {
            SelectArousalType(target);
            arousalFlag = false;
            return;
        }

        // アイテムカードを使用する
        if(supportItem != null) supportItem.Action(target);
        supportItem = null;

        dekki.DestroyCard(out int _ap);
        RemoveAP(_ap);
    }

    /// <summary> 潜在覚醒するキャラクターを選択する </summary>
    public void SelectLatentArousal()
    {
        // MPが足りていなければ
        if(mp < arousalMP)
        {
            playerUI.ShowCautionText("EP");
            return;
        }

        var targetList = director.GetTarget(null, Player.Mine, InGameData.SelectType.AllySingle);
        // 潜在覚醒をしていないキャラクターを優先して選択
        bool flag = false;
        foreach ( var target in targetList ) { if(target.GetTypeID() == 0 ) { flag = true; director.IsAction(target); break; } }
        if(!flag) { director.IsAction(targetList[0]); }
        arousalFlag = true;
    }

    void SelectArousalType(Targets target)
    {
        // 潜在覚醒していたら、潜在スキルを発動する
        if(target.target.GetTypeID() != 0) { return; }

        // 潜在覚醒選択画面
        arousalPlayer = target.target;
        // 潜在覚醒タイプ選択画面表示
        playerUI.ShowArousalUI(arousalPlayer.charID, arousalPlayer.maxType, charaDatas[arousalPlayer.charID - 1].arousals);
    }

    /*** 潜在覚醒(UIのカードクリック時) ***/
    public void LatentArousal(int typeID)
    {
        // MP消費
        RemoveMP(arousalMP);

        // 潜在覚醒
        arousalPlayer.LatentArousal(typeID);
        dekki.TypeChange(arousalPlayer.charID, typeID);

        // UI更新
        director.UpdateUI();

        // 一定ターン経過後に戻す
    }
    /*** =================================== ターン終了時 =================================== ***/
    public void EndTurn()
    {
        turn = false;

        // 残りターン確認
        for (int i = buffDatas.Count-1; i >= 0; i--) {
            if (buffDatas[i].turn != 0) continue;

            buffDatas[i].EndAction(null);
            buffDatas.RemoveAt(i);
        }
        foreach (var chara in characters) { chara.CheckTurnProgress(); chara.ChangePhase(Player.Mine); }
    }

    public bool CheckGameOver()
    {
        bool flag = true;

        foreach (var chara in characters)
        {
            // 死亡判定
            if(chara.dedFlag) { dekki.TypeChange(chara.charID, -1); continue; }

            flag = false;
        }

        return flag;
    }
}