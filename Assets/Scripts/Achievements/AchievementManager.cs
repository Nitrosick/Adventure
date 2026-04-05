using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AchievementManager {
  public static List<AchievementInstance> achievementsList = new();

  public static void Init() {
    achievementsList.Clear();

    Achievement[] achievementsData = Resources.LoadAll<Achievement>("Achievements");
    achievementsList = achievementsData
      .OrderBy(a => int.Parse(a.id[2..]))
      .Select(a => new AchievementInstance(a))
      .ToList();

    AchievementData[] savedData = StateManager.achievements;
    foreach (AchievementData ach in savedData) {
      AchievementInstance achievementIns = achievementsList.FirstOrDefault(a => a.data.id == ach.id);
      if (achievementIns == null) continue;
      achievementIns.completed = ach.completed;
      achievementIns.progress = ach.progress;
    }

    foreach (AchievementInstance ach in achievementsList) {
      if (ach.progress == 0) ach.progress = ach.data.defaultProgress;
    }
  }

  public static void UpdateAchievement(string id, float value, bool inBattle) {
    AchievementInstance ach = achievementsList.FirstOrDefault(a => a.data.id == id);
    if (ach == null || ach.completed) return;
    ach.progress += value;
    if (ach.progress < 0) ach.progress = 0;

    if (ach.progress >= ach.data.objectiveCount) {
      ach.progress = ach.data.objectiveCount;
      ach.completed = true;
      ach.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

      if (inBattle) {
        BattleManager.Instance.Reward.Add(ach.data.reward);
      }
      else {
        Player.Instance.CollectReward(ach.data.reward);
        _ = Toast.Show("success", "Achievement recieved");
      }
    }
    StateManager.WriteAchievementsData(achievementsList.ToArray());
  }
}
