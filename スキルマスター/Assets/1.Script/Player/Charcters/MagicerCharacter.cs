using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class MagicerCharacter : CharacterScript
{
    /// <summary>
    /// スキルを配列に格納していく
    /// </summary>
    protected override void SetSkillStanby()
    {
        maxType = 2;

        // 潜在覚醒時の効果
        arousalEffect = new Action[3] {
            ArousalType0, ArousalType1, ArousalType2
        };

        // スキル 処理
        action = new Func<Targets, Task>[2, 6] {
            { Attack, Skill_11,Skill_12, Skill_13, Skill_14, Skill_15 }, // 通常状態
            { Attack, Skill_21,Skill_22, Skill_23, Skill_14, Skill_15 }, // 覚醒：○○
        };
        // 通常攻撃の選択対象
        defultAttackDatas = new AttackData[2, 1] {
            { SetDefultAttack01() }, // 通常状態
            { SetDefultAttack01() }, // 覚醒：アイスクイーン
        };
    }

    /*** ----------------------- 潜在覚醒時の効果 ------------------------- ***/
    // ---------------------
    // 状態：ノーマル
    // ---------------------
    void ArousalType0()
    {

    }
    // ---------------------
    // 状態：アイスクイーン
    // ---------------------
    void ArousalType1()
    {
        // 魔法攻撃力
        var skillValue1 = Mathf.RoundToInt(defultMagicAttack * 0.6f);
        addMagicAttack += skillValue1;   // 魔法攻撃力+

        // 与ダメージ上昇
        var skillValue2 = 40;
        dealtDamageMultiplier += skillValue2;  // 与ダメージ+

        BuffData buff = new BuffData()
        {
            buffName = "潜在覚醒：アイスクイーン",
            buffText = $"魔法攻撃力+{skillValue1}、与ダメージ+{skillValue2}％",
            turn = -1,
            EndAction = (target) => { }
        };
        SetBuffData(buff);
    }
    // ---------------------
    // 状態：茨姫
    // ---------------------
    void ArousalType2()
    {

    }

    /*** -------------------- モード.ノーマル ID: 0 ----------------------- ***/

    // ---------------------
    // 通常攻撃
    // ---------------------
    AttackData SetDefultAttack01()
    {
        return new AttackData()
        {
            Name = "通常攻撃",
            Select = SelectMode.Single | SelectMode.Enemy,
            Text = () => $"敵一体に魔法攻撃力{defultAttackMultiplier}％のダメージを与える"
        };
    }
    async Task Attack(Targets target) // ID: 0
    {
        // エフェクト
        ef.Ice(target.target.AnimPos);
        
        await Task.Delay(100);

        AttackAction(target.target, ma * defultAttackMultiplier * 0.01f);

        // ダメージアニメーション
        var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
        charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
    }

    // ---------------------
    // マナチャージ
    // ---------------------
    async Task Skill_11(Targets target) // ID: 1
    {
        await Task.Delay(500);

        playerController.AddAP(skillData.Value1);
    }

    // ---------------------
    // アクティブチャージ
    // ---------------------
    int skill_12_Value;
    async Task Skill_12(Targets target) // ID: 2
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        playerController.AddMP(skillData.Value1);
        skill_12_Value = skillData.Value2;

        // 効果付与
        if(!playerController.apUseTimingAction.Contains(Skill_12_Process))
            playerController.apUseTimingAction.Add(Skill_12_Process);

        Action<CharacterScript> action = (target) => { playerController.apUseTimingAction.Remove(Skill_12_Process); };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = "",
            turn = skillData.Duration,
            EndAction = action
        };
        playerController.SetTurnCounter(buff, true);

        // キャラクターにバフをセットする
        BuffData buffc = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"AP1消費毎に、EP+{skill_12_Value} 効果累積不可",
            turn = skillData.Duration,
            EndAction = (target) => { }
        };
        foreach(var chara in target.characterTarget) chara.SetBuffData(new BuffData(buffc), true);
    }
    void Skill_12_Process(int ap) { playerController.AddMP(skill_12_Value * ap); }

    // ---------------------
    // アイシクルエッジ
    // ---------------------
    async Task Skill_13(Targets target) // ID: 3
    {
        var vl = skillData.Value1;

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            // エフェクト
            foreach (var chara in tg.enemyTarget) ef.IceEge(chara.AnimPos);
            await Task.Delay(500);
            AttackAction(tg.enemyTarget, ma * vl * 0.01f);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    // ---------------------
    // アイシクルランス
    // ---------------------
    async Task Skill_14(Targets target) // ID: 4
    {
        var vl = skillData.Value1;

        await Task.Delay(100);
        
        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            // エフェクト
            ef.IceEge(tg.target.AnimPos);
            await Task.Delay(500);
            AttackAction(tg.target, ma * vl * 0.01f);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    // ---------------------
    // アイシクルレイン
    // ---------------------
    async Task Skill_15(Targets target) // ID: 5
    {
        var vl = skillData.Value1;

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            // エフェクト
            foreach (var chara in tg.enemyTarget) ef.IceEge(chara.AnimPos);
            await Task.Delay(500);
            AttackAction(tg.enemyTarget, ma * vl * 0.01f);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    /*** -------------------- モード.アイスクイーン ID: 1 ----------------------- ***/

    // ---------------------
    // マナリチャージャー
    // ---------------------
    int skill_21_Value;
    int skill_21_Count;
    async Task Skill_21(Targets target) // ID: 1
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        skill_21_Count = skillData.Value3;
        await Task.Delay(500);

        skill_21_Value = skillData.Value2;
        playerController.AddAP(skillData.Value1);
        // 効果付与
        playerController.AddUseCardTimingAction(Skill_21_Process, true);

        Action<CharacterScript> action = (target) => { playerController.RemoveUseCardTimingAction(Skill_21_Process); };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = "",
            turn = skillData.Duration,
            EndAction = action
        };
        playerController.SetTurnCounter(buff, true);

        // キャラクターにバフをセットする
        BuffData buffc = new BuffData()
        {
            buffName = skillData.Name,
            buffText = "カード使用時、AP+" + skill_21_Value,
            turn = skillData.Duration,
            EndAction = (target) => { }
        };
        foreach (var chara in target.characterTarget) chara.SetBuffData(new BuffData(buffc), true);
    }
    void Skill_21_Process(CommonData.CardData cardData)
    {
        if (skill_21_Count == 0) return;

        playerController.AddAP(skill_21_Value);
        skill_21_Count--;
    }

    // ---------------------
    // ライジング
    // ---------------------
    int skill_22_Value;
    int skill_22_Record;
    BuffData buffs;
    async Task Skill_22(Targets target) // ID: 2
    {
        // エフェクト
        ef.Buff(AnimPos);

        await Task.Delay(500);

        skill_22_Value = skillData.Value1;
        if (!playerController.apUseTimingAction.Contains(Skill_22_Process))
            playerController.apUseTimingAction.Add(Skill_22_Process);

        // 終了処理
        Action<CharacterScript> action = (target) => 
        {
            dealtDamageMultiplier -= skill_22_Record;
            skill_22_Record = 0;
            playerController.apUseTimingAction.Remove(Skill_22_Process);
        };
        // プレイヤーにセット
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"APを1消費するごとに{charaName}の与ダメージ+{skill_22_Value}％",
            turn = skillData.Duration,
            EndAction = action
        };
        playerController.SetTurnCounter(buff, true);

        // キャラクターにセット
        buffs = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"味方がAPを1消費するごとにこのキャラの与ダメージ+{skill_22_Value}％\n与ダメージ+{skill_22_Record}％",
            turn = skillData.Duration,
            EndAction = (target) => { }
        };
        SetBuffData(buffs, true);

        // このスキルの発動分
        Skill_22_Process(skillData.Ap);
    }
    void Skill_22_Process(int ap) {
        dealtDamageMultiplier += skill_22_Value * ap;
        skill_22_Record += skill_22_Value * ap;

        buffs.buffText = $"味方がAPを1消費するごとにこのキャラの与ダメージ+{skill_22_Value}％\n与ダメージ+{skill_22_Record}％";
    }

    // ---------------------
    // アイシクルウェーブ
    // ---------------------
    async Task Skill_23(Targets target) // ID: 3
    {
        var skillTarget = target.enemyTarget;
        var skillValue = skillData.Value1;
        var skillValue2 = skillData.Value2;

        // 終了処理
        Action<CharacterScript> action = (target) => {
            target.takeDamageMultiplier -= skillValue;
        };
        // デバフ内容
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"被ダメージ+{skillValue}％",
            turn = skillData.Duration,
            EndAction = action
        };

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            var skillTarget = tg.enemyTarget;
            // デバフの付与
            foreach (var enemy in skillTarget) {
                enemy.takeDamageMultiplier += skillValue; // 被ダメ+
                enemy.SetBuffData(new BuffData(buff));
            }
            // エフェクト
            foreach (var chara in skillTarget) { ef.Debuff(chara.AnimPos); ef.IceEge(chara.AnimPos); }
            await Task.Delay(300);

            // 攻撃
            AttackAction(skillTarget, ma * skillValue2 * 0.01f);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    // ---------------------
    // ロンギヌスの槍
    // ---------------------
    // Skill_14

    // ---------------------
    // 天罰
    // ---------------------
    // Skill_15
}
