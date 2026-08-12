// Filename: LevelData.cs
// Folder: Assets/Scripts/Data/
// Purpose: ScriptableObject describing a single level (Phase 7).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.Data
{
    /// <summary>
    /// Data asset for one playable level scene.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData_", menuName = "Bounder Trail/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string levelId = "level_01";
        [SerializeField] private string displayName = "New Level";
        [SerializeField] private string sceneName = "Gameplay";

        [Header("Meta")]
        [SerializeField] private int buildIndex = 0;
        [TextArea(2, 4)]
        [SerializeField] private string designerNotes;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public int BuildIndex => buildIndex;
        public string DesignerNotes => designerNotes;
    }
}
