using TMPro;
using UnityEngine;

public class ItemInfo : MonoBehaviour {
  // Panels
  private static Transform panel;
  private static Transform equipRequirements;
  private static Transform weaponParams;
  private static Transform armorParams;
  private static Transform medicineParams;

  // Fields
  private static TextMeshProUGUI title;
  private static TextMeshProUGUI description;
  private static TextMeshProUGUI price;
  private static TextMeshProUGUI reqStats;
  private static TextMeshProUGUI reqLevel;
  private static TextMeshProUGUI weight;
  private static TextMeshProUGUI weaponDamage;
  private static TextMeshProUGUI weaponDamageType;
  private static TextMeshProUGUI weaponRange;
  private static TextMeshProUGUI armorDefense;
  private static TextMeshProUGUI medIntensity;

  private void Awake() {
    panel = transform.Find("Info/ItemInfoPanel").GetComponent<Transform>();
    equipRequirements = panel.Find("EquipRequirements").GetComponent<Transform>();
    weaponParams = panel.Find("WeaponParams").GetComponent<Transform>();
    armorParams = panel.Find("ArmorParams").GetComponent<Transform>();
    medicineParams = panel.Find("MedicineParams").GetComponent<Transform>();

    title = panel.Find("Name").GetComponent<TextMeshProUGUI>();
    description = panel.Find("Description").GetComponent<TextMeshProUGUI>();
    price = panel.Find("Price/Value").GetComponent<TextMeshProUGUI>();
    reqStats = equipRequirements.Find("Stats/Value").GetComponent<TextMeshProUGUI>();
    reqLevel = equipRequirements.Find("Level/Value").GetComponent<TextMeshProUGUI>();
    weight = equipRequirements.Find("Weight/Value").GetComponent<TextMeshProUGUI>();
    weaponDamage = weaponParams.Find("Damage/Value").GetComponent<TextMeshProUGUI>();
    weaponDamageType = weaponParams.Find("DamageType/Value").GetComponent<TextMeshProUGUI>();
    weaponRange = weaponParams.Find("Range/Value").GetComponent<TextMeshProUGUI>();
    armorDefense = armorParams.Find("Defense/Value").GetComponent<TextMeshProUGUI>();
    medIntensity = medicineParams.Find("Intensity/Value").GetComponent<TextMeshProUGUI>();

    if (
      title == null || description == null || medicineParams == null ||
      medIntensity == null || price == null || equipRequirements == null ||
      reqStats == null || reqLevel == null || weaponParams == null ||
      weaponDamage == null || weaponDamageType == null || weaponRange == null ||
      armorParams == null || armorDefense == null || weight == null
    ) {
      Debug.LogError("Item info components initialization error");
      return;
    }
  }

  public static void Show(Equipment item) {
    panel.gameObject.SetActive(true);
    equipRequirements.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.price.ToString();
    int[] stats = item.requirementStats;
    reqStats.text = string.Format(
      "<color=#F61010>{0}</color> / <color=#81D11F>{1}</color> / <color=#2B8EF3>{2}</color>",
      stats[0], stats[1], stats[2]
    );
    reqLevel.text = item.requirementLevel.ToString();
    weight.text = item.weight.ToString();

    if (item is Weapon weaponItem) {
      weaponParams.gameObject.SetActive(true);
      weaponDamage.text = weaponItem.damage.ToString();
      weaponDamageType.text = weaponItem.damageType.ToString();
      weaponRange.text = weaponItem.range.ToString();
    } else if (item is Armor armorItem) {
      armorParams.gameObject.SetActive(true);
      armorDefense.text = armorItem.defense.ToString();
    }
  }

  public static void Show(Item item) {
    panel.gameObject.SetActive(true);

    title.text = item.itemName;
    description.text = item.description;
    price.text = item.price.ToString();

    if (item is MedicineItem medItem) {
      medicineParams.gameObject.SetActive(true);
      medIntensity.text = medItem.intensity.ToString() + " HP";
    }
  }

  public static void Hide() {
    panel.gameObject.SetActive(false);
    equipRequirements.gameObject.SetActive(false);
    weaponParams.gameObject.SetActive(false);
    armorParams.gameObject.SetActive(false);
    medicineParams.gameObject.SetActive(false);
    title.text = "";
    description.text = "";
    price.text = "";
    reqStats.text = "";
    reqLevel.text = "";
    weight.text = "";
    weaponDamage.text = "";
    weaponDamageType.text = "";
    weaponRange.text = "";
    armorDefense.text = "";
    medIntensity.text = "";
  }
}
