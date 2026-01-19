using InGameData;
using System.Threading.Tasks;
using System;
using UnityEngine.UIElements;
using UnityEngine;

public class WeponScript : SupportItems
{
    protected override void SetSkillStanby()
    {
        Action<Targets>[] action;

        // スキル 処理
        action = new Action<Targets>[4] { Wepon_001, Wepon_002, Wepon_001, Wepon_002 };
        actions.Add(action); // 選別なし
    }

    /*** -------------------- アイテム ----------------------- ***/

    // ---------------------
    // 普通の剣
    // ---------------------
    async void Wepon_001(Targets target) // ID: 001
    {
        await Task.Delay(100);

        EquipItem equipItem = new EquipItem();
        equipItem.itemName = itemData.Name;
        var weponValue = Mathf.RoundToInt(target.target.defultAttack * itemData.Value1 * 0.01f);
        target.target.addAttack += weponValue;

        // バフ付与
        BuffData buff = new BuffData()
        {
            buffName = itemData.Name,
            buffText = $"攻撃力+{weponValue}",
            turn = -1,
            EndAction = (target) => { }
        };
        target.target.SetBuffData(buff);

        // 装備解除
        Action<CharacterScript> action = (target) =>
        {
            target.addAttack -= weponValue;
            // バフ削除
            target.RemoveBuffData(buff.buffName);
        };

        equipItem.EndAction = action;
        target.target.Equip(equipItem);
    }

    // ---------------------
    // 普通の杖
    // ---------------------
    async void Wepon_002(Targets target) // ID: 002
    {
        await Task.Delay(100);

        EquipItem equipItem = new EquipItem();
        equipItem.itemName = itemData.Name;
        var weponValue = Mathf.RoundToInt(target.target.defultMagicAttack * itemData.Value1 * 0.01f);
        target.target.addMagicAttack += weponValue;

        // バフ付与
        BuffData buff = new BuffData()
        {
            buffName = itemData.Name,
            buffText = $"魔法攻撃力+{weponValue}",
            turn = -1,
            EndAction = (target) => { }
        };
        target.target.SetBuffData(buff);

        // 装備解除
        Action<CharacterScript> action = (target) =>
        {
            target.addMagicAttack -= weponValue;
            // バフ削除
            target.RemoveBuffData(buff.buffName);
        };

        equipItem.EndAction = action;
        target.target.Equip(equipItem);
    }

    // ---------------------
    // 匠の剣
    // ---------------------
    // Wepon_001

    // ---------------------
    // 匠の杖
    // ---------------------
    // Wepon_002
}
