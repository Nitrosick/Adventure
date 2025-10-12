using UnityEngine;

public class MapZoneRecruitment : MonoBehaviour {
  public Unit[] recruits;
  public int recruitVillagers;
  public int reputation;
  public bool isRescuing;
  public Requirements requirements;

  public void OpenRecruitmentPanel() {
    RecruitingUI.Open(this);
  }
}
