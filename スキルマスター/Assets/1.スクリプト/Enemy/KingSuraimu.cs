using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class KingSuraimu : EnemyScript
{

    protected override void SetSkillStanby()
    {
        // スキル 処理
        action = new Func<Targets, Task>[1, 3] {
            { Attack, InpactAttack, AllInpactAttack }, // 通常状態
        };
        // スキルの選択対象
        defultAttackDatas = new AttackData[1, 3] {
            { SetDefultAttack01(), SetDefultAttack02(), SetDefultAttack03() }, // 通常状態
        };
    }

    /*** -------------------- 通常状態(テスト) ID: 0 ----------------------- ***/

    const int Skill01_Value = 100;
    AttackData SetDefultAttack01()
    {
        return new AttackData()
        {
            Name = "タックル",
            Select = SelectType.AllySingle,
            Text = () => $"敵一体に攻撃力{Skill01_Value}％のダメージを与える"
        };
    }
    async Task Attack(Targets target) // ID: 0
    {
        waitTime = 100;
        await Task.Delay(100);

        AttackAction(target.target, Mathf.RoundToInt(at * Skill01_Value * 0.01f));

        // ダメージアニメーション
        var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
        charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        ef.Smash(target.target.AnimPos);
    }

    const int Skill02_Value = 150;
    AttackData SetDefultAttack02()
    {
        return new AttackData()
        {
            Name = "強タックル",
            Select = SelectType.AllySingle,
            Text = () => $"敵一体に攻撃力{Skill02_Value}％のダメージを与える"
        };
    }
    async Task InpactAttack(Targets target) // ID: 0
    {
        waitTime = 100;
        await Task.Delay(100);

        AttackAction(target.target, Mathf.RoundToInt(at * Skill02_Value * 0.01f));
        ef.Smash(target.target.AnimPos);
    }

    const int Skill03_Value = 150;
    AttackData SetDefultAttack03()
    {
        return new AttackData()
        {
            Name = "インパクトタックル",
            Select = SelectType.AllyAll,
            Text = () => $"敵全体に攻撃力{Skill03_Value}％のダメージを与える"
        };
    }
    async Task AllInpactAttack(Targets target)
    {
        waitTime = 200;
        await Task.Delay(200);

        AttackAction(target.characterTarget, Mathf.RoundToInt(at * Skill03_Value * 0.01f));

        // エフェクト
        foreach (var chara in target.characterTarget) ef.Smash(chara.AnimPos);
    }
}
