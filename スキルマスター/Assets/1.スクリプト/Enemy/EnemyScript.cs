using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyScript : CharacterScript
{
    [Header("ターン経過スキル発動(小さい順)"), SerializeField]
    int[] skillTurns;

    protected int waitTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async virtual Task DoThink(int turnCount)
    {
        int _skillID = 0;
        // 使用するスキルを決める
        for (int i = skillTurns.Length - 1; i >= 0; i--)
        {
            if (turnCount % skillTurns[i] != 0) continue;

            _skillID = i;
            break;
        }

        skillID = _skillID;
        SetSkillID(skillID);
        enemyController.SelectMode(this, defultAttackDatas[typeID, skillID].Select);

        await Task.Delay(waitTime);
        // 二回行動　連続行動がメイン(仮)　別キャラまたぐのはやらないかも
    }

    /// <summary> 死亡エフェクト(Destroy込) </summary>
    public async void DeadEffect()
    {
        await Task.Delay(200);
        Destroy(charaImg.gameObject);
        Destroy(gameObject);
    }

    protected override void SetSkillStanby()
    {
        // スキル 処理
        action = new Func<Targets, Task>[1, 2] {
            { Attack, AllAttack }, // 通常状態
        };
        // スキルの選択対象
        defultAttackDatas = new AttackData[1, 2] {
            { SetDefultAttack01(), SetDefultAttack02() }, // 通常状態
        };
    }

    /*** -------------------- 通常状態(テスト) ID: 0 ----------------------- ***/
    AttackData SetDefultAttack01()
    {
        return new AttackData()
        {
            Name = "通常攻撃",
            Select = SelectMode.Single | SelectMode.Ally,
            Text = () => $"敵一体に攻撃力{defultAttackMultiplier}％のダメージを与える"
        };
    }
    async Task Attack(Targets target) // ID: 0
    {
        await Task.Delay(100);

        AttackAction(target.target, at);
    }
    AttackData SetDefultAttack02()
    {
        return new AttackData()
        {
            Name = "薙ぎ払い",
            Select = SelectMode.All | SelectMode.Ally,
            Text = () => $"敵全体に攻撃力{defultAttackMultiplier}％のダメージを与える"
        };
    }
    async Task AllAttack(Targets target) // ID: 1
    {
        await Task.Delay(100);

        AttackAction(target.characterTarget, at);
    }
}
