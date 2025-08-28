using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestsDatabase", menuName = "GameObjects/Quests/Database")]
public class QuestsDatabase : ScriptableObject {
  public List<Quest> quests;
}
