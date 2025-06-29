using UnityEngine;

[CreateAssetMenu(menuName = "Effect")]
public class Effect : ScriptableObject {
  public string effectName;
  public int duration;
  public float damage;
  public bool cancelMove;
  public bool cancelAttack;
  public bool isStackable;
  public bool isNegative;
  [TextArea] public string description;

  public DamageType damageType;
  public GameObject icon;
  public Sprite uiIcon;
  public Color uiIconColor;
}
