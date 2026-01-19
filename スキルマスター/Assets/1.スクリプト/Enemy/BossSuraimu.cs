using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;

public class BossSuraimu : EnemyScript
{

    protected override void SetSkillStanby()
    {
        // スキル 処理
        action = new Func<Targets, Task>[1, 3] {
            { Attack, BressAttack, AllInpactAttack }, // 通常状態
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

    async Task BressAttack(Targets target) // ID: 1
    {
        waitTime = 600;
        await Task.Delay(100);

        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(50);

            // もし敵全員死んでいたら
            if (target.canSelectTarget.Count == 0) break;

            int num = UnityEngine.Random.Range(0, target.canSelectTarget.Count);
            AttackAction(target.canSelectTarget[num], at * 0.5f);

            // エフェクト
            ef.Slash(target.canSelectTarget[num].AnimPos);
            
            // 対象が死んだら、選択対象リストから消す
            if (target.canSelectTarget[num].dedFlag) target.canSelectTarget.RemoveAt(num);
        }
    }

    async Task AllInpactAttack(Targets target)
    {
        waitTime = 400;
        await Task.Delay(100);

        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(100);

            AttackAction(target.characterTarget, at);

            // エフェクト
            foreach (var chara in target.characterTarget) ef.Magic(chara.AnimPos);
        }
    }
}
