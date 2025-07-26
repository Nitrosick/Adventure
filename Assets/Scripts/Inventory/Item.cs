using UnityEngine;

public abstract class Item : ScriptableObject {
  public string id;
  public string itemName;
  [TextArea] public string description;
  public bool usable;
  public bool disposable;
  public Rarity rarity;
  public Sprite icon;
  public int price;
}
