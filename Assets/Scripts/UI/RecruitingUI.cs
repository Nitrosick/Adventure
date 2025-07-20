using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitingUI : MonoBehaviour {
  private static Transform window;
  private static Button submit;
  private static Button cancel;
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI description;
  private static Transform villagersPanel;
  private static TextMeshProUGUI villagersCount;
  private static GameObject notEnough;
  private static MapZoneRecruitment mapZone;

  // Requirements
  private static Transform reqPlayerLevel;
  private static TextMeshProUGUI reqPlayerLevelValue;
  private static Transform reqPlayerFame;
  private static TextMeshProUGUI reqPlayerFameValue;
  private static Transform reqGold;
  private static TextMeshProUGUI reqGoldValue;
  private static Transform reqWood;
  private static TextMeshProUGUI reqWoodValue;
  private static Transform reqStone;
  private static TextMeshProUGUI reqStoneValue;
  private static Transform reqMetal;
  private static TextMeshProUGUI reqMetalValue;
  private static Transform reqLeather;
  private static TextMeshProUGUI reqLeatherValue;
  private static Transform reqEquipment;

  private void Awake() {
    Transform Find(string path) => window.Find(path);
    T Get<T>(string path) where T : Component => Find(path).GetComponent<T>();

    window = transform.Find("Recruitment/Panel");
    submit = Get<Button>("Control/Recruit");
    cancel = Get<Button>("Control/Cancel");
    title = Get<TextMeshProUGUI>("Head/Title");
    description = Get<TextMeshProUGUI>("Description");
    villagersPanel = Find("Reward/Villagers");
    villagersCount = Get<TextMeshProUGUI>("Reward/Villagers/Value");
    notEnough = Find("NotEnough").gameObject;

    reqPlayerLevel = Find("Requirements/PlayerLevel");
    reqPlayerLevelValue = Get<TextMeshProUGUI>("Requirements/PlayerLevel/Value");
    reqPlayerFame = Find("Requirements/PlayerFame");
    reqPlayerFameValue = Get<TextMeshProUGUI>("Requirements/PlayerFame/Value");
    reqGold = Find("Requirements/Gold");
    reqGoldValue = Get<TextMeshProUGUI>("Requirements/Gold/Value");
    reqWood = Find("Requirements/Wood");
    reqWoodValue = Get<TextMeshProUGUI>("Requirements/Wood/Value");
    reqStone = Find("Requirements/Stone");
    reqStoneValue = Get<TextMeshProUGUI>("Requirements/Stone/Value");
    reqMetal = Find("Requirements/Metal");
    reqMetalValue = Get<TextMeshProUGUI>("Requirements/Metal/Value");
    reqLeather = Find("Requirements/Leather");
    reqLeatherValue = Get<TextMeshProUGUI>("Requirements/Leather/Value");
    reqEquipment = Find("Requirements/Equipment");

    if (
      window == null || submit == null || cancel == null ||
      title == null || description == null || villagersPanel == null ||
      villagersCount == null || reqPlayerLevel == null || reqPlayerLevelValue == null ||
      reqPlayerFame == null || reqPlayerFameValue == null || reqGold == null ||
      reqGoldValue == null || reqWood == null || reqWoodValue == null ||
      reqStone == null || reqStoneValue == null || reqMetal == null ||
      reqMetalValue == null || reqLeather == null || reqLeatherValue == null ||
      reqEquipment == null || notEnough == null
    ) {
      Debug.LogError("Recruiting UI components initialization error");
    }

    submit.onClick.AddListener(OnSubmit);
    cancel.onClick.AddListener(Close);
  }

  private void OnDestroy() {
    submit.onClick.RemoveListener(OnSubmit);
    cancel.onClick.RemoveListener(Close);
  }

  public static void Open(MapZoneRecruitment zone) {
    if (zone == null || zone.requirements == null) return;
    mapZone = zone;
    title.text = mapZone.zoneName;
    description.text = mapZone.description;

    if (zone.recruitVillagers > 0) {
      villagersCount.text = "x" + zone.recruitVillagers;
      villagersPanel.gameObject.SetActive(true);
    }

    if (!MeetsRequirements(zone.requirements)) {
      notEnough.SetActive(true);
      submit.interactable = false;
    }

    ShowRequirements(zone.requirements);
    window.gameObject.SetActive(true);
    SceneController.ShowBackground();
  }

  private static void Close() {
    window.gameObject.SetActive(false);
    SceneController.HideBackground();
    mapZone = null;
    title.text = "";
    description.text = "";
    villagersPanel.gameObject.SetActive(false);
    notEnough.SetActive(false);
    submit.interactable = true;

    reqPlayerLevel.gameObject.SetActive(false);
    reqPlayerFame.gameObject.SetActive(false);
    reqGold.gameObject.SetActive(false);
    reqWood.gameObject.SetActive(false);
    reqStone.gameObject.SetActive(false);
    reqMetal.gameObject.SetActive(false);
    reqLeather.gameObject.SetActive(false);
    reqEquipment.gameObject.SetActive(false);
  }

  private static void ShowRequirements(Requirements req) {
    static void Check(int value, GameObject obj, TextMeshProUGUI text = null) {
      if (value > 0) {
        obj.SetActive(true);
        if (text != null) text.text = value.ToString();
      }
    }

    Check(req.playerLevel, reqPlayerLevel.gameObject, reqPlayerLevelValue);
    Check(req.playerFame, reqPlayerFame.gameObject, reqPlayerFameValue);
    Check(req.gold, reqGold.gameObject, reqGoldValue);
    Check(req.resources[0], reqWood.gameObject, reqWoodValue);
    Check(req.resources[1], reqStone.gameObject, reqStoneValue);
    Check(req.resources[2], reqMetal.gameObject, reqMetalValue);
    Check(req.resources[3], reqLeather.gameObject, reqLeatherValue);

    if (req.equipment.Length > 0) reqEquipment.gameObject.SetActive(true);
  }

  private static bool MeetsRequirements(Requirements req) {
    Player player = Player.Instance;

    if (req.playerLevel > player.Level) return false;
    if (req.playerFame > player.Fame) return false;
    if (req.gold > player.Gold) return false;

    for (int i = 0; i < req.resources.Length; i++) {
      if (req.resources[i] > player.Resources[i]) return false;
    }

    // FIXME: Проверка на экипировку и предметы
    return true;
  }

  private static void OnSubmit() {
    Player player = Player.Instance;

    if (mapZone.recruitVillagers > 0) {
      player.SetVillagers(mapZone.recruitVillagers);
      MapUI.UpdateResources();
      mapZone.SetCleared();
    }

    // FIXME: Закрыть окно и отключить кнопку взаимодействия
  }
}
