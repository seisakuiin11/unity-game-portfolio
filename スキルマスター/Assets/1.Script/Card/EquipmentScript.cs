using InGameData;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class EquipmentScript : SupportItems
{
    protected override void SetSkillStanby()
    {
        Action<Targets>[] action;

        // スキル 処理
        action = new Action<Targets>[5] { Equipment_001, Equipment_001, Equipment_003, Equipment_004, Equipment_004 };
        actions.Add(action); // 選別なし
    }

    /*** -------------------- アイテム ----------------------- ***/

    // ---------------------
    // 普通の鎧
    // ---------------------
    async void Equipment_001(Targets target) // ID: 001
    {
        await Task.Delay(100);

        EquipItem equipItem = new EquipItem();
        equipItem.itemName = itemData.Name;
        var itemValue = itemData.Value1;
        target.target.takeDamageMultiplier -= itemValue;

        // バフ付与
        BuffData buff = new BuffData()
        {
            buffName = itemData.Name,
            buffText = $"被ダメージ-{itemValue}％",
            turn = -1,
            EndAction = (target) => { }
        };
        target.target.SetBuffData(buff);

        // 装備解除処理
        Action<CharacterScript> action = (target) => {
            target.takeDamageMultiplier += itemValue;
            // バフ削除
            target.RemoveBuffData(buff.buffName);
        };

        equipItem.EndAction = action;
        target.target.Equip(equipItem);
    }

    // ---------------------
    // 匠の鎧
    // ---------------------
    // Equipment_001

    // ---------------------
    // 最後の鎧
    // ---------------------
    int equipment_003_Value;
    async void Equipment_003(Targets target) // ID: 001
    {
        await Task.Delay(100);

        EquipItem equipItem = new EquipItem();
        equipItem.itemName = itemData.Name;
        equipment_003_Value = itemData.Value1;

        target.target.damageTimingAction.Add(Equipment_003_Process);

        // バフ付与
        BuffData buff = new BuffData()
        {
            buffName = itemData.Name,
            buffText = $"一度だけダメージを{equipment_003_Value}％カットする",
            turn = -1,
            EndAction = (target) => { }
        };
        target.target.SetBuffData(buff);

        // 装備解除処理
        Action<CharacterScript> action = (target) => {
            target.takeDamageMultiplier += equipment_003_Value;
            // バフ削除
            target.RemoveBuffData(buff.buffName);
        };

        equipItem.EndAction = action;
        target.target.Equip(equipItem);
    }
    void Equipment_003_Process(CharacterScript character)
    {
        character.damage -= Mathf.RoundToInt(character.damage * equipment_003_Value);
        character.Equip(null); // 装備解除
    }

    // ---------------------
    // 普通のリング
    // ---------------------
    async void Equipment_004(Targets target) // ID: 001
    {
        await Task.Delay(100);

        EquipItem equipItem = new EquipItem();
        equipItem.itemName = itemData.Name;
        var itemValue = Mathf.RoundToInt(target.target.defultHP * itemData.Value1 * 0.01f);
        target.target.MaxHpUp(itemValue);

        // バフ付与
        BuffData buff = new BuffData()
        {
            buffName = itemData.Name,
            buffText = $"最大HP+{itemValue}",
            turn = -1,
            EndAction = (target) => { }
        };
        target.target.SetBuffData(buff);

        // 装備解除処理
        Action<CharacterScript> action = (target) => {
            target.MaxHpDown(itemValue);
            // バフ削除
            target.RemoveBuffData(buff.buffName);
        };

        equipItem.EndAction = action;
        target.target.Equip(equipItem);
    }

    // ---------------------
    // 匠のリング
    // ---------------------
    // Equipment_004
}
