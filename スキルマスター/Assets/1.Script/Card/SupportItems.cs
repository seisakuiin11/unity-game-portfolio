using InGameData;
using CommonData;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SupportItems
{
    protected CardData itemData;
    protected int charID;   // 選択対象の条件
    protected int skillID;  // 種別
    protected List<Action<Targets>[]> actions;

    public void Init()
    {
        actions = new List<Action<Targets>[]>();

        SetSkillStanby();
    }

    // スキルIDを取得
    public void SetSkillID(int _charID, int id, CardData _data)
    {
        charID = _charID;
        skillID = id; // ID取得
        itemData = _data;
    }

    // ターゲットを取得して、スキルIDをもとにアクションを行う
    public void Action(Targets target)
    {
        actions[charID][skillID](target);
    }

    /// <summary>
    /// スキルを配列に格納していく
    /// </summary>
    protected virtual void SetSkillStanby()
    {
        Action<Targets>[] action;

        // スキル 処理
        action = new Action<Targets>[2] { Kaihuku, AllKaihuku };
        actions.Add(action);
        action = new Action<Targets>[2] { Kaihuku, AllKaihuku };
        actions.Add(action);
    }

    /*** -------------------- アイテム ----------------------- ***/

    // ---------------------
    // 回復
    // ---------------------
    void Kaihuku(Targets target) // ID: 001
    {
        target.target.Heal((int)(itemData.Value1 * 0.01f));
    }

    // ---------------------
    // 全員回復
    // ---------------------
    void AllKaihuku(Targets target) // ID: 002
    {
        foreach (var character in target.characterTarget) character.Heal((int)(itemData.Value1 * 0.01f));
    }
}

/// <summary>
/// 装備アイテム
/// </summary>
public class EquipItem
{
    public string itemName;
    public Action<CharacterScript> EndAction;
}