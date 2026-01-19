using System.Collections.Generic;
using System.Threading.Tasks;
using InGameData;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] GameDirector director;
    [SerializeField] Vector3[] enemyPos;
    [SerializeField] EnemyScript[] enemyPrefabs;
    [SerializeField] Transform enemyParent;
    [System.Serializable]
    class EnemyID { public List<int> id = new List<int>(); }
    [SerializeField] List<EnemyID> stage;
    [SerializeField] CharacterSprite charaSprite;

    int turnCount;
    int round;
    List<EnemyScript> enemys;

    // Enemy Action Data CPU ------------------------
    enum ActionState
    {
        None,
        Wait,
        Stanby,
        Action
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public List<EnemyScript> Init()
    {
        enemys = new List<EnemyScript>();
        return enemys;
    }

    /// <summary>
    /// ラウンド開始
    /// </summary>
    public void StartRound(out int _nowRound, out int _maxRound)
    {
        int underIndex = 0;
        int[] layerIndex = new int[stage[round].id.Count];
        // エネミー生成
        for (int i = 0; i < stage[round].id.Count; i++)
        {
            EnemyScript enemy = Instantiate(enemyPrefabs[stage[round].id[i]], enemyParent);
            enemy.transform.localPosition = enemyPos[i];
            layerIndex[i] = (int)enemyPos[i].y / -80;
            if (underIndex > layerIndex[i]) underIndex = layerIndex[i];
            enemy.Init(director, Player.Enemy, Instantiate(charaSprite), layerIndex[i], _enemyController: this);
            enemys.Add(enemy);
        }
        // 奥行の調整
        for (int i = 0; i < enemys.Count; i++) enemys[i].transform.SetSiblingIndex(layerIndex[i] - underIndex);

        turnCount = 0;
        round++;
        _nowRound = round;
        _maxRound = stage.Count;
    }

    public void StartTurn()
    {
        turnCount++;

        // ターン経過確認
        foreach (var enemy in enemys) enemy.CheckTurnProgress();

        // ターン経過
        foreach (var enemy in enemys) enemy.RemoveTurn();

        EnemyAction();
    }

    async void EnemyAction()
    {
        await Task.Delay(1600); // 1.6秒待機

        for (int i = 0;i < enemys.Count; i++)
        {
            if (director.endGame) break;

            if (enemys[i].dedFlag) continue;
            await enemys[i].DoThink(turnCount);

            await Task.Delay(1000); // 一秒待機
        }

        if (director.endGame) return;

        director.NextTurn();
    }

    /*** ターゲット選択 ***/
    public void SelectMode(CharacterScript _activePlayer, SelectMode mode)
    {
        // IDをもとにターゲット条件を変更
        // ターゲット条件をもとにターゲット選択
        List<CharacterScript> targets = director.GetTarget(_activePlayer, Player.Enemy, mode);

        IsAction(targets);
    }

    void IsAction(List<CharacterScript> _targets)
    {
        CharacterScript target = _targets[Random.Range(0, _targets.Count)];

        director.IsAction(target); // 選択
        director.IsAction(target); // 実行
    }

    public void EndTurn()
    {
        // ターン経過確認
        foreach (var enemy in enemys) enemy.CheckTurnProgress();
    }

    /// <summary>
    /// 全員倒したか確認
    /// </summary>
    public bool CheckGameClear()
    {
        bool flag = true;
        for (int i = enemys.Count - 1; i >= 0; i--)
        {
            // 死んでない
            if (!enemys[i].dedFlag) { flag = false; continue; }

            // 死んでいたら、削除
            enemys[i].DeadEffect();
            enemys.RemoveAt(i);
        }

        // 次のラウンドへ
        if(flag && round < stage.Count)
        {
            director.NextRound();
            flag = false;
        }

        return flag;
    }
}