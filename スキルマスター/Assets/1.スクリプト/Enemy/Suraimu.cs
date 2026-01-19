using DG.Tweening;
using InGameData;
using System;
using System.Threading.Tasks;

public class Suraimu : EnemyScript
{

    protected override void SetSkillStanby()
    {
        // スキル 処理
        action = new Func<Targets, Task>[1, 1] {
            { Attack }, // 通常状態
        };
        // スキルの選択対象
        actionOfSelectType = new SelectMode[1, 1] {
            { SelectType.AllySingle }, // 通常状態
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
}
