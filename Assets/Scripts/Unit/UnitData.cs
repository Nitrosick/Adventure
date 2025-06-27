[System.Serializable]
public class UnitData {
  // FIXME: Добавить все сериализуемые поля
  public int prefabId = 1;
  public float currentHealth;
  public bool isDead;
  public bool inSquad;
  public float strength;
  public float dexterity;
  public float intelligence;
  public int level;

  public Weapon primaryWeapon;
  public Weapon secondaryWeapon;
  public Armor shield;
  public Armor armor;
}
