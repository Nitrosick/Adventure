using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBrushSet", menuName = "Brushes/Tile Brush Set")]
public class TileBrushSet : ScriptableObject {
  public string brushSetName;
  public List<GameObject> prefabs;
}
