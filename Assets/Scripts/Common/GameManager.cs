using UnityEditor.Animations;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public static GameManager I;

  [Header("Materials")]
  public Material transparentMaterial;
  public Material stoneMaterial;
  public Material goldMaterial;

  [Header("Slots")]
  public GameObject slotEmpty;
  public GameObject slotMenu;
  public GameObject slotWithCount;
  public GameObject slotWithHealth;
  public GameObject slotWithPrice;
  public GameObject slotBuff;
  public GameObject slotQuest;
  public GameObject slotQuestEmpty;
  public GameObject slotSquadOverwhelmed;
  public GameObject slotQueue;
  public GameObject slotRecipe;
  public GameObject slotChain;

  [Header("Sprites and Icons")]
  public Sprite villagersSprite;
  public Sprite[] resourceSprites;
  public GameObject effectIcon;

  [Header("Animations")]
  public AnimatorController fistsAnimController;

  void Awake() {
    I = this;
  }
}
