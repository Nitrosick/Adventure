using UnityEngine;

public class LogUI : MonoBehaviour {
  public static LogUI Instance;
  private Transform container;
  [SerializeField] private LogItem logPrefab;

  void Awake() {
    Instance = this;
    container = transform.Find("Log/Panel");
    if (container == null) Debug.LogError("Log UI components initialization error");
  }

  public void Add(string message) {
    LogItem item = Instantiate(logPrefab, container);
    item.Init(message);
  }

  public void Add(Reward reward) {
    if (reward.experience > 0) Add($"+{reward.experience} XP");
    if (reward.fame > 0) Add($"+{reward.fame} Fame");
    if (reward.fame < 0) Add($"<color=#F61010>{reward.fame} Fame</color>");
    if (reward.reputation > 0) Add($"+{reward.reputation} Reputation");
    if (reward.reputation < 0) Add($"<color=#F61010>{reward.reputation} Reputation</color>");
    if (reward.statPoints > 0) Add($"<color=#EFBF0D>+{reward.statPoints} Stat points</color>");
    if (reward.abilityPoints > 0) Add($"<color=#EFBF0D>+{reward.abilityPoints} Ability points</color>");
    if (reward.projectiles > 0) Add($"+{reward.projectiles} Projectiles");

    for (int i = 0; i < reward.resources.Length; i++) {
      if (reward.resources[i] > 0) {
        string text = MapUI.Instance.resTooltips[i];
        Add($"+{reward.resources[i]} {text}");
      }
    }

    foreach (Equipment item in reward.equipment) {
      // TODO: Кол-во предметов в награде
      Add($"{item.itemName} x1");
    }

    foreach (Item item in reward.items) {
      // TODO: Кол-во предметов в награде
      Add($"{item.itemName} x1");
    }
  }
}
