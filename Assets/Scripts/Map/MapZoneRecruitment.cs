using UnityEngine;

public class MapZoneRecruitment : MapZone {
  [Header("Reqruitment")]
  public Unit[] recruits;
  public int recruitVillagers;
  public Requirements requirements;

  public void OpenRecruitmentPanel() {
    RecruitingUI.Open(this);
  }

  public override void SetCleared() {
    base.SetCleared();

    if (events.Count < 1) {
      // FIXME: Гасить свет в домах
    }
  }
}
