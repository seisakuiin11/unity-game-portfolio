using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class BossSuraimu : EnemyScript
{

    protected override void SetSkillStanby()
    {
        // スキル 処理
        action = new Func<Targets, Task>[1, 3] {
            { Attack, BressAttack, AllInpactAttack }, // 通常状態
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

    const int Skill02_Value = 60;
    const int Skill02_Count = 10;
    AttackData SetDefultAttack02()
    {
        return new AttackData()
        {
            Name = "ブレスラッシュ",
            Select = SelectType.AllyAll,
            Text = () => $"{Skill02_Count}回ランダムな敵に攻撃力{Skill02_Value}％のダメージを与える"
        };
    }
    async Task BressAttack(Targets target) // ID: 1
    {
        waitTime = 600;
        await Task.Delay(100);

        for (int i = 0; i < Skill02_Count; i++)
        {
            await Task.Delay(100);

            // もし敵全員死んでいたら
            if (target.canSelectTarget.Count == 0) break;

            int num = UnityEngine.Random.Range(0, target.canSelectTarget.Count);
            AttackAction(target.canSelectTarget[num], Mathf.RoundToInt(at * Skill02_Value * 0.01f));

            // エフェクト
            ef.Smash(target.canSelectTarget[num].AnimPos);
            
            // 対象が死んだら、選択対象リストから消す
            if (target.canSelectTarget[num].dedFlag) target.canSelectTarget.RemoveAt(num);
        }
    }

    const int Skill03_Value = 100;
    const int Skill03_Count = 3;
    AttackData SetDefultAttack03()
    {
        return new AttackData()
        {
            Name = "マジックミサイル",
            Select = SelectType.AllyAll,
            Text = () => $"敵全体に攻撃力{Skill03_Value}％のダメージを{Skill03_Count}回与える"
        };
    }
    async Task AllInpactAttack(Targets target)
    {
        waitTime = 400;
        await Task.Delay(100);

        for (int i = 0; i < Skill03_Count; i++)
        {
            await Task.Delay(200);

            AttackAction(target.characterTarget, Mathf.RoundToInt(at * Skill03_Value * 0.01f));

            // エフェクト
            foreach (var chara in target.characterTarget) ef.Smash(chara.AnimPos);
        }
    }
}
