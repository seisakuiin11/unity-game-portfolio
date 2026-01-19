using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class TanckerCharacter : CharacterScript
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
            { Attack, Skill_11,Skill_22, Skill_23, Skill_24, Skill_25 }, // 覚醒：キャッスル
        };
        // 通常攻撃の選択対象
        actionOfSelectType = new SelectMode[2, 1] {
            { SelectType.EnemySingle }, // 通常状態
            { SelectType.EnemySingle }, // 覚醒：キャッスル
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
    // 状態：キャッスル
    // ---------------------
    void ArousalType1()
    {
        // HP上昇
        var skillValue1 = Mathf.RoundToInt(defultHP * 0.4f);
        MaxHpUp(skillValue1);

        // ドロー
        var skillValue2 = 3;
        playerController.Draw(skillValue2);

        BuffData buff = new BuffData()
        {
            buffName = "潜在覚醒：キャッスル",
            buffText = $"HP+{skillValue1}",
            turn = -1,
            EndAction = (target) => { }
        };
        SetBuffData(buff);
    }
    // ---------------------
    // 状態：ナイト・オブ・キング
    // ---------------------
    void ArousalType2()
    {

    }

    /*** -------------------- モード.ノーマル ID: 0 ----------------------- ***/

    // ---------------------
    // 通常攻撃
    // ---------------------
    async Task Attack(Targets target) // ID: 0
    {
        // ダメージアニメーション
        var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
        charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        ef.Smash(target.target.AnimPos);

        await Task.Delay(100);

        AttackAction(target.target, at * defultAttackMultiplier * 0.01f);
    }

    // ---------------------
    // 挑発
    // ---------------------
    bool skill_11_Flag;
    Action<CharacterScript> skill_11_Action;
    async Task Skill_11(Targets target) // ID: 1
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        if (skill_11_Flag) // 重複の場合、リセット
        {
            skill_11_Action(null);
        }

        var SkillTargets = target.characterTarget;
        var skillValue = skillData.Value1;

        // 他キャラクターを選択できないようにする
        foreach (var item in SkillTargets) item.selectFlag = item == this;
        // 味方全員の被ダメージ-
        foreach (var chara in SkillTargets) chara.takeDamageMultiplier -= skillValue;

        // 効果終了処理 自己バフ ----------------------------------------
        skill_11_Action = (target) => {
            // 他キャラクターを選択できるようににする
            foreach (var item in SkillTargets) item.selectFlag = true;
            skill_11_Flag = false;
        };
        BuffData buff = new BuffData()
        {
            buffName = "挑発.1",
            buffText = "他キャラクターは敵から選択されない",
            turn = skillData.Duration,
            EndAction = skill_11_Action
        };
        SetBuffData(buff, true); // このキャラにバフセット(重複なし)
        skill_11_Flag = true;

        // 効果終了処理 ------------------------------------------------
        Action<CharacterScript> action = (target) => { target.takeDamageMultiplier += skillValue; }; // 被ダメージ戻す

        BuffData buffc = new BuffData()
        {
            buffName = skillData.Name + ".2",
            buffText = $"被ダメージ-{skillValue}％",
            turn = skillData.Duration,
            EndAction = action
        };
        foreach (var chara in SkillTargets) chara.SetBuffData(new BuffData(buffc)); // 重複あり
    }

    // ---------------------
    // ヘイスト
    // ---------------------
    async Task Skill_12(Targets target) // ID: 2
    {
        await Task.Delay(500);

        playerController.AddMP(skillData.Value1); // EP+
        playerController.Draw(skillData.Value2);  // ドロー
    }

    // ---------------------
    // ヒートチャージ
    // ---------------------
    int skill_13_Value;
    async Task Skill_13(Targets target) // ID: 3
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        var SkillTargets = target.characterTarget;
        skill_13_Value = skillData.Value2;

        playerController.AddMP(skillData.Value1); // EP+
        foreach (var chara in SkillTargets)
        {
            if (chara.damageTimingAction.Contains(Skill_13_Process)) continue;
                chara.damageTimingAction.Add(Skill_13_Process);
        }

        // 効果終了処理
        Action<CharacterScript> action = (target) =>
        {
            target.damageTimingAction.Remove(Skill_13_Process);
        };
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"敵から攻撃を受けた時、EP+{skill_13_Value} 効果累積不可",
            turn = skillData.Duration,
            EndAction = action
        };

        foreach (var chara in SkillTargets) chara.SetBuffData(new BuffData(buff), true);
    }
    void Skill_13_Process(CharacterScript character) { playerController.AddMP(skill_13_Value); } // 覚醒値+

    // ---------------------
    // パーフェクトガード
    // ---------------------
    float skill_14_Value;
    async Task Skill_14(Targets target) // ID: 4
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        var skillTarget = target.characterTarget;
        var skillValue = Mathf.RoundToInt(defultHP * skillData.Value1 * 0.01f);
        skill_14_Value = skillData.Value2 * 0.01f;

        MaxHpUp(skillValue); // HP+ (自己バフ)
        // 味方にバフを付与
        foreach (var chara in skillTarget)
        {
            if(chara == this) continue;
            if(chara.damageTimingAction.Contains(Skill_14_Process)) continue;

            chara.damageTimingAction.Add(Skill_14_Process); // 付与
        }

        // 終了処理 自己バフ ---------------------------------
        Action<CharacterScript> action = (target) =>
        {
            MaxHpDown(skillValue);
        };
        // このキャラにバフセット
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = "最大HP+" + skillValue,
            turn = skillData.Duration,
            EndAction = action
        };
        SetBuffData(buff);

        // 終了処理 -----------------------------------------
        Action<CharacterScript> action2 = (target) =>
        {
            target.damageTimingAction.Remove(Skill_14_Process);
        };
        BuffData buffc = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"攻撃を受けるとき、{skill_14_Value * 100}％を{charaName}が肩代わりする 効果累積不可",
            turn = skillData.Duration,
            EndAction = action2
        };
        foreach (var chara in skillTarget)
        {
            if (chara == this) continue;
            chara.SetBuffData(new BuffData(buffc), true);
        }
    }
    void Skill_14_Process(CharacterScript character) // 味方が攻撃を受けたとき
    {
        var num = Mathf.RoundToInt(character.damage * skill_14_Value);
        character.damage -= num;
        Damage(num, 0); // 肩代わり
    }

    // ---------------------
    // シールド
    // ---------------------
    async Task Skill_15(Targets target) // ID: 5
    {
        // エフェクト
        ef.Buff(target.target.AnimPos);

        await Task.Delay(100);

        int vl = Mathf.RoundToInt(maxHp * skillData.Value2 * 0.01f);
        playerController.AddMP(skillData.Value1);
        target.target.AddSilde(vl);
    }

    /*** -------------------- モード.キャッスル ID: 1 ----------------------- ***/

    // ---------------------
    // 挑発・改
    // ---------------------
    // Skill_11

    // ---------------------
    // リスタート
    // ---------------------
    async Task Skill_22(Targets target) // ID: 2
    {
        // エフェクト
        ef.Buff(target.target.AnimPos);

        await Task.Delay(500);

        var skillValue = Mathf.RoundToInt(defultHP * skillData.Value2 * 0.01f);
        playerController.Draw(skillData.Value1); // ドロー
        target.target.MaxHpUp(skillValue); // HP+

        // 終了処理
        Action<CharacterScript> action = (target) =>
        {
            target.MaxHpDown(skillValue);
        };
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"HP+{skillValue}",
            turn = skillData.Duration,
            EndAction = action
        };

        target.target.SetBuffData(buff);
    }

    // ---------------------
    // アイ・アム・キャッスル
    // ---------------------
    async Task Skill_23(Targets target) // ID: 3
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        var skillTargets = target.characterTarget;
        var skillValue1 = skillData.Value1;
        var skillValue2 = Mathf.RoundToInt(defultHP * skillData.Value2 * 0.01f);

        foreach (var chara in skillTargets) {
            chara.takeDamageMultiplier -= skillValue1; // 被ダメ-
            chara.MaxHpUp(skillValue2); // HP+
        }

        // 終了処理
        Action<CharacterScript> action = (target) =>
        {
            target.takeDamageMultiplier += skillValue1;
            target.MaxHpDown(skillValue2);
        };
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"被ダメージ-{skillValue1}％、HP+{skillValue2}",
            turn = skillData.Duration,
            EndAction = action
        };

        foreach (var chara in skillTargets) chara.SetBuffData(new BuffData(buff));
    }

    // ---------------------
    // パーフェクトシールド
    // ---------------------
    int skill_24_Count;
    int skill_24_Value1;
    int skill_24_Value2;
    async Task Skill_24(Targets target) // ID: 4
    {
        // エフェクト
        ef.Buff(AnimPos);

        skill_24_Count = skillData.Value3;
        await Task.Delay(500);

        var skillTarget = target.enemyTarget;
        skill_24_Value1 = skillData.Value1;
        skill_24_Value2 = skillData.Value2;
        // 敵の攻撃に格納 攻撃回数で管理
        foreach (var enemy in skillTarget)
        {
            if (enemy.attackTimingAction.Contains(Skill_24_Process)) continue;
            enemy.attackTimingAction.Add(Skill_24_Process);
        }

        // 終了処理
        Action<CharacterScript> action = (target) =>
        {
            // 敵につけた効果を消す
            foreach (var enemy in skillTarget)
            {
                enemy.attackTimingAction.Remove(Skill_24_Process);
            }
        };
        // このキャラにバフセット
        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"敵の攻撃を{skill_24_Value1}％カット、{skill_24_Value2}％分のダメージを跳ね返す、最大{skill_24_Count}回発動する 効果累積不可",
            turn = skillData.Duration,
            EndAction = action
        };
        SetBuffData(buff, true);
    }
    void Skill_24_Process(CharacterScript character) // 敵が攻撃したとき
    {
        if (skill_24_Count == 0) return;

        skill_24_Count--;
        AttackAction(character, character.damage * skill_24_Value2 * 0.01f);
        character.damage -= Mathf.RoundToInt(character.damage * skill_24_Value1 * 0.01f); // 敵が与えるダメージを消す
    }

    // ---------------------
    // ハイシールド
    // ---------------------
    async Task Skill_25(Targets target) // ID: 5
    {
        foreach (var character in target.characterTarget) ef.Buff(character.AnimPos);

        await Task.Delay(100);

        playerController.Draw(skillData.Value2); // ドロー
        int vl = Mathf.RoundToInt(maxHp * skillData.Value1 * 0.01f);
        foreach (var character in target.characterTarget) character.AddSilde(vl); // シールド
    }
}
