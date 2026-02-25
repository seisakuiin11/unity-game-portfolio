using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AttackerCharacter : CharacterScript
{
    /// <summary>
    /// スキルを配列に格納していく
    /// </summary>
    protected override void SetSkillStanby()
    {
        maxType = 3;

        // 潜在覚醒時の効果
        arousalEffect = new Action[3] {
            ArousalType0, ArousalType1, ArousalType2
        };

        // スキル 処理
        action = new Func<Targets, Task>[3, 6] {
            { Attack, Skill_11,Skill_12, Skill_13, Skill_14, Skill_15 }, // 通常状態
            { Attack, Skill_11,Skill_22, Skill_23, Skill_24, Skill_15 }, // 覚醒：ヒーロー
            { AllAttack, Skill_31,Skill_32, Skill_33, Skill_34, Skill_35 }, // 覚醒：ヒーロー
        };
        // 通常攻撃の選択対象
        defultAttackDatas = new AttackData[3, 1] {
            { SetDefultAttack01() }, // 通常状態
            { SetDefultAttack01() }, // 覚醒：ヒーロー
            { SetDefultAttack03() }, // 覚醒：ダークヒーロー
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
    // 状態：ヒーロー
    // ---------------------
    void ArousalType1()
    {
        // 攻撃力上昇
        var skillValue1 = Mathf.RoundToInt(defultAttack * 0.6f);
        addAttack += skillValue1;   // 攻撃力+

        // 通常攻撃上昇
        var skillValue2 = 40;
        defultAttackMultiplier += skillValue2;  // 通常攻撃上昇

        BuffData buff = new BuffData()
        {
            buffName = "潜在覚醒：ヒーロー",
            buffText = $"攻撃力+{skillValue1}、通常攻撃+{skillValue2}％",
            turn = -1,
            EndAction = (target) => { }
        };
        SetBuffData(buff);
    }
    // ---------------------
    // 状態：ダークヒーロー
    // ---------------------
    void ArousalType2()
    {
        // HP上昇
        var skillValue = Mathf.RoundToInt(defultHP * 0.6f);
        MaxHpUp(skillValue);   // HP+

        BuffData buff = new BuffData()
        {
            buffName = "潜在覚醒：ダークヒーロー",
            buffText = $"HP+{skillValue}、通常攻撃が範囲攻撃になる",
            turn = -1,
            EndAction = (target) => { }
        };
        SetBuffData(buff);
    }

    /*** -------------------- モード.ノーマル ID: 0 ----------------------- ***/

    // ---------------------
    // 通常攻撃
    // ---------------------
    AttackData SetDefultAttack01()
    {
        return new AttackData() {
            Name = "通常攻撃",
            Select = SelectMode.Single | SelectMode.Enemy,
            Text = () => $"敵一体に攻撃力{defultAttackMultiplier}％のダメージを与える"
        };
    }
    async Task Attack(Targets target) // ID: 0
    {
        // 攻撃アニメーション
        var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
        charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        ef.Slash(target.target.AnimPos);

        await Task.Delay(100);

        AttackAction(target.target, at * defultAttackMultiplier * 0.01f);
    }

    // ---------------------
    // スラッシュ
    // ---------------------
    async Task Skill_11(Targets target) // ID: 1
    {
        var vl = skillData.Value1;

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets,Task> ac = async (tg) =>
        {
            // エフェクト
            foreach (var chara in tg.enemyTarget) ef.Slash(chara.AnimPos);
            await Task.Delay(500);
            AttackAction(tg.enemyTarget, at * vl * 0.01f);

            // 攻撃アニメーション
            var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
            charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    // ---------------------
    // ストロング
    // ---------------------
    async Task Skill_12(Targets target) // ID: 2
    {
        // エフェクト
        ef.Buff(target.target.AnimPos);

        await Task.Delay(500);

        var skillTarget = target.target;
        var skillValue1 = Mathf.RoundToInt(defultAttack * skillData.Value1 * 0.01f);
        var skillValue2 = skillData.Value2;
        skillTarget.addAttack += skillValue1;             // 攻撃力+
        skillTarget.dealtDamageMultiplier += skillValue2; // 与ダメージアップ

        // 効果終了処理
        Action<CharacterScript> action = (target)=>{
            target.addAttack -= skillValue1;             // 攻撃力戻す
            target.dealtDamageMultiplier -= skillValue2; // 与ダメージ戻す
        };

        BuffData buff = new BuffData() {
            buffName = skillData.Name,
            buffText = $"攻撃力+{skillValue1}、与ダメージ+{skillValue2}％",
            turn = skillData.Duration,
            EndAction = action
        };
        skillTarget.SetBuffData(buff);
    }

    // ---------------------
    // エール
    // ---------------------
    async Task Skill_13(Targets target) // ID: 3
    {
        await Task.Delay(500);

        playerController.AddMP(skillData.Value1); // EP+
        playerController.Draw(skillData.Value2);  // ドロー
    }

    // ---------------------
    // アタックチャージ
    // ---------------------
    int skill_14_Value;
    async Task Skill_14(Targets target) // ID: 4
    {
        // エフェクト
        foreach(var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        var skillTarget = target.characterTarget;
        skill_14_Value = skillData.Value2;
        playerController.AddMP(skillData.Value1); // EP+

        // 効果 付与
        foreach (var chara in skillTarget)
        {
            if (chara.defultAttackTimingAction.Contains(Skill_14_Process)) continue;

            chara.defultAttackTimingAction.Add(Skill_14_Process);
        }

        // 効果を消す
        Action<CharacterScript> action = (target) => { target.defultAttackTimingAction.Remove(Skill_14_Process); };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"通常攻撃時、EP+{skill_14_Value} 効果累積不可",
            turn = skillData.Duration,
            EndAction = action
        };
        foreach(var chara in skillTarget) chara.SetBuffData(new BuffData(buff), true);
    }
    void Skill_14_Process(CharacterScript character) { playerController.AddMP(skill_14_Value); } // 通常攻撃時EP+

    // ---------------------
    // スタンバイ
    // ---------------------
    async Task Skill_15(Targets target) // ID: 5
    {
        await Task.Delay(500);

        playerController.AddAP(skillData.Value1);
        playerController.Draw(skillData.Value2);
    }

    /*** -------------------- モード.ヒーロー ID: 1 ----------------------- ***/

    // ---------------------
    // スラッシュ・改
    // ---------------------
    // Skill_11

    // ---------------------
    // リーダーシップ
    // ---------------------
    async Task Skill_22(Targets target) // ID: 2
    {
        // エフェクト
        foreach (var chara in target.characterTarget) ef.Buff(chara.AnimPos);

        await Task.Delay(500);

        var skillTargets = target.characterTarget;
        var skillValue1 = Mathf.RoundToInt(defultAttack * skillData.Value1 * 0.01f);
        var skillValue2 = skillData.Value2;

        foreach (var character  in skillTargets)
        {
            character.addAttack += skillValue1; // 攻撃力+
            character.dealtDamageMultiplier += skillValue2; // 与ダメージ+
        }

        // 効果を消す
        Action<CharacterScript> action = (target) => {
            target.addAttack -= skillValue1;
            target.dealtDamageMultiplier -= skillValue2;
        };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"攻撃力+{skillValue1}、与ダメージ+{skillValue2}％",
            turn = skillData.Duration,
            EndAction = action
        };
        foreach (var chara in skillTargets) chara.SetBuffData(new BuffData(buff));
    }

    // ---------------------
    // 修練
    // ---------------------
    async Task Skill_23(Targets target) // ID: 3
    {
        // エフェクト
        ef.Buff(target.target.AnimPos);

        await Task.Delay(500);

        var skillValue1 = Mathf.RoundToInt(defultAttack * skillData.Value2 * 0.01f);
        var skillValue2 = skillData.Value3;
        playerController.Draw(skillData.Value1); // ドロー
        target.target.addAttack += skillValue1; // 攻撃力+
        target.target.defultAttackMultiplier += skillValue2;

        // 効果終了処理
        Action<CharacterScript> action = (target) => {
            target.addAttack -= skillValue1; // 攻撃力戻す
            target.defultAttackMultiplier -= skillValue2; // 通常攻撃倍率戻す
        };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"攻撃力+{skillValue1}、通常攻撃+{skillValue2}％",
            turn = skillData.Duration,
            EndAction = action
        };
        target.target.SetBuffData(buff); // バフ付与
    }

    // ---------------------
    // 乱撃斬
    // ---------------------
    async Task Skill_24(Targets target) // ID: 4
    {
        var skillValue = skillData.Value1;
        var skillValue2 = skillData.Value2;

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            // ランダム攻撃
            for (int i = 0; i < skillValue; i++)
            {
                // もし敵全員死んでいたら
                if (tg.canSelectTarget.Count == 0) break;

                int num = UnityEngine.Random.Range(0, tg.canSelectTarget.Count);
                var skillTarget = tg.canSelectTarget[num];
                // エフェクト
                ef.Slash(skillTarget.AnimPos);

                await Task.Delay(100);

                AttackAction(skillTarget, at * skillValue2 * 0.01f);
                // 対象が死んだら、選択対象リストから消す
                if (skillTarget.dedFlag) target.canSelectTarget.Remove(skillTarget);
            }
        };

        // 攻撃追加
        AddAction(ac);
    }

    // ---------------------
    // スタンバイ・改
    // ---------------------
    // Skill_15

    /*** -------------------- モード.ダークヒーロー ID: 2 ----------------------- ***/
    // ---------------------
    // 通常攻撃
    // ---------------------
    AttackData SetDefultAttack03()
    {
        return new AttackData()
        {
            Name = "通常攻撃",
            Select = SelectMode.All | SelectMode.Enemy,
            Text = () => $"敵全体に攻撃力{defultAttackMultiplier}％のダメージを与える"
        };
    }
    async Task AllAttack(Targets target) // ID: 1
    {
        // エフェクト
        foreach(var chara in target.enemyTarget) ef.Slash(chara.AnimPos);

        await Task.Delay(100);

        AttackAction(target.enemyTarget, at * defultAttackMultiplier * 0.01f);

        // 攻撃アニメーション
        var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
        charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
    }

    // ---------------------
    // スラッシュ・傷
    // ---------------------
    async Task Skill_31(Targets target) // ID: 1
    {
        var skillValue1 = skillData.Value1;
        var skillValue2 = skillData.Value2;
        var skillValue3 = skillData.Value3;

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            // エフェクト
            foreach (var chara in tg.enemyTarget) ef.Slash(chara.AnimPos);

            await Task.Delay(100);

            // HP消費
            int num = Mathf.RoundToInt(nowHp * skillValue1 * 0.01f);
            Damage(num, 0, ValueTextType.DirectHP);

            // 攻撃
            int vl = Mathf.RoundToInt(at * skillValue2 * 0.01f) + Mathf.RoundToInt(maxHp * skillValue3 * 0.01f);
            AttackAction(tg.enemyTarget, vl);

            // 攻撃アニメーション
            var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
            charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    // ---------------------
    // ライフストロング
    // ---------------------
    async Task Skill_32(Targets target) // ID: 2
    {
        // エフェクト
        ef.Buff(AnimPos);

        await Task.Delay(100);

        // HP消費
        int num = Mathf.RoundToInt(nowHp * skillData.Value1 * 0.01f);
        Damage(num, 0, ValueTextType.DirectHP);

        // 最大HPアップ
        var skillValue1 = Mathf.RoundToInt(defultHP * skillData.Value2 * 0.01f);
        MaxHpUp(skillValue1);
        // 攻撃力アップ
        var skillValue2 = Mathf.RoundToInt(maxHp * skillData.Value3 * 0.01f);
        addAttack += skillValue2;

        // 効果終了処理
        Action<CharacterScript> action = (target) => {
            target.MaxHpDown(skillValue1); // 最大HPを戻す
            target.addAttack -= skillValue2; // 攻撃力戻す
        };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"HP+{skillValue1}、攻撃力+{skillValue2}",
            turn = skillData.Duration,
            EndAction = action
        };
        SetBuffData(buff); // 自己バフ付与
    }

    // ---------------------
    // 不屈の精神
    // ---------------------
    async Task Skill_33(Targets target) // ID: 3
    {
        // エフェクト
        ef.Buff(AnimPos);

        await Task.Delay(100);

        // HP消費
        int num = Mathf.RoundToInt(nowHp * skillData.Value1 * 0.01f);
        Damage(num, 0, ValueTextType.DirectHP);

        // 攻撃力アップ
        var skillValue1 = Mathf.RoundToInt(maxHp * skillData.Value2 * 0.01f);
        addAttack += skillValue1;
        // 与ダメージアップ
        var skillValue2 = skillData.Value3;
        dealtDamageMultiplier += skillValue2;

        // 効果終了処理
        Action<CharacterScript> action = (target) => {
            target.addAttack -= skillValue1; // 攻撃力戻す
            target.dealtDamageMultiplier -= skillValue2; // 与ダメージを戻す
        };

        BuffData buff = new BuffData()
        {
            buffName = skillData.Name,
            buffText = $"攻撃力+{skillValue1}、与ダメージ+{skillValue2}%",
            turn = skillData.Duration,
            EndAction = action
        };
        SetBuffData(buff); // 自己バフ付与
    }

    // ---------------------
    // ライフ・インパクト
    // ---------------------
    async Task Skill_34(Targets target) // ID: 4
    {
        var skillValue1 = skillData.Value1;
        var skillValue2 = skillData.Value2;
        var skillValue3 = skillData.Value3;

        await Task.Delay(100);

        // === 攻撃内容 === //
        Func<Targets, Task> ac = async (tg) =>
        {
            // エフェクト
            ef.Slash(tg.target.AnimPos);

            await Task.Delay(100);

            // HP消費
            int num = Mathf.RoundToInt(nowHp * skillValue1 * 0.01f);
            Damage(num, 0, ValueTextType.DirectHP);

            // 攻撃
            int vl = Mathf.RoundToInt(at * skillValue2 * 0.01f) + Mathf.RoundToInt(maxHp * skillValue3 * 0.01f);
            AttackAction(tg.target, vl);

            // 攻撃アニメーション
            var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
            charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        };

        // 攻撃セット
        ChangeAction(skillData, ac);
    }

    // ---------------------
    // リライフ
    // ---------------------
    async Task Skill_35(Targets target) // ID: 5
    {
        await Task.Delay(100);

        int vl = maxHp - nowHp;
        Heal(vl);
    }
}
