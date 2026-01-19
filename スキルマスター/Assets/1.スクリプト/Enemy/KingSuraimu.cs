using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;

public class KingSuraimu : EnemyScript
{

    protected override void SetSkillStanby()
    {
        // スキル 処理
        action = new Func<Targets, Task>[1, 3] {
            { Attack, InpactAttack, AllInpactAttack }, // 通常状態
        };
        // スキルの選択対象
        actionOfSelectType = new SelectMode[1, 3] {
            { SelectType.AllySingle, SelectType.AllySingle, SelectType.AllyAll }, // 通常状態
        };
    }

    /*** -------------------- 通常状態(テスト) ID: 0 ----------------------- ***/

    async Task Attack(Targets target) // ID: 0
    {
        waitTime = 100;
        await Task.Delay(100);

        AttackAction(target.target, at);

        // ダメージアニメーション
        var p = charaImg.transform.position.x + (0.5f * (player == Player.Mine ? 1 : -1));
        charaImg.transform.DOMoveX(p, 0.05f).SetLoops(2, LoopType.Yoyo);
        ef.Smash(target.target.AnimPos);
    }

    async Task InpactAttack(Targets target) // ID: 0
    {
        waitTime = 100;
        await Task.Delay(100);

        AttackAction(target.target, at * 1.5f);
        ef.Smash(target.target.AnimPos);
    }

    async Task AllInpactAttack(Targets target)
    {
        waitTime = 200;
        await Task.Delay(200);

        AttackAction(target.characterTarget, at * 1.5f);

        // エフェクト
        foreach (var chara in target.characterTarget) ef.Magic(chara.AnimPos);
    }
}
