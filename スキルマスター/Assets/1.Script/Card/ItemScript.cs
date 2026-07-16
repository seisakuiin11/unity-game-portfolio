using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ItemScript : SupportItems
{
    protected override void SetSkillStanby()
    {
        Action<Targets>[] action;

        // スキル 処理
        action = new Action<Targets>[5] { Item_001, Item_002, Item_003, Item_004, Item_005 };
        actions.Add(action);
    }

    /*** -------------------- アイテム ----------------------- ***/

    // ---------------------
    // 回復ポーション
    // ---------------------
    async void Item_001(Targets target) // ID: 001
    {
        await Task.Delay(100);

        var value = Mathf.RoundToInt(target.target.maxHp * itemData.Value1 * 0.01f);
        target.target.Heal(value);
    }

    // ---------------------
    // オール回復ポーション
    // ---------------------
    async void Item_002(Targets target) // ID: 002
    {
        await Task.Delay(100);

        foreach (var character in target.characterTarget) character.Heal(Mathf.RoundToInt(character.maxHp * itemData.Value1 * 0.01f));
    }

    // ---------------------
    // マナポーション
    // ---------------------
    async void Item_003(Targets target) // ID: 002
    {
        await Task.Delay(100);

        Debug.LogWarning("まだできてないよ");
    }

    // ---------------------
    // バフポーション
    // ---------------------
    async void Item_004(Targets target) // ID: 002
    {
        await Task.Delay(100);

        Debug.LogWarning("まだできてないよ");
    }

    // ---------------------
    // バリアポーション
    // ---------------------
    async void Item_005(Targets target) // ID: 002
    {
        await Task.Delay(100);

        Debug.LogWarning("まだできてないよ");
    }
}
