using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LethalToolkit
{
    [FilePath("Project/LethalToolkit/lethalToolkitManager.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class LethalToolkitManager : ScriptableSingleton<LethalToolkitManager>
    {
        public static LethalToolkitManager Instance => instance;

        private LethalToolkitSettings _scriptableManagerSettings;
        [SerializeField]
        public LethalToolkitSettings LethalToolkitSettings
        {
            get
            {
                if (_scriptableManagerSettings == null)
                {
                    IEnumerable<ScriptableObject> scriptableObjects;
                    scriptableObjects = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject")
                    .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                    .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(x));
                    foreach (ScriptableObject item in scriptableObjects)
                        if (item.GetType() == typeof(LethalToolkitSettings))
                            _scriptableManagerSettings = ((LethalToolkitSettings)item);
                }
                return (_scriptableManagerSettings);
            }
        }

        public void OpenScene(string scenePath, OpenSceneMode sceneMode)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("Trying to open " + scenePath);
                EditorSceneManager.OpenScene(scenePath, sceneMode);
            }
        }
    }

    public static class LethalToolkitMenuItems
    {
        public static LethalToolkitManager lethalToolkitManager => LethalToolkitManager.Instance;
        public static LethalToolkitSettings lethalToolkitSettings => lethalToolkitManager.LethalToolkitSettings;

        [MenuItem("LethalToolkit/Open LethalToolkit Settings")]
        static void SelectLethalToolkitSettings()
        {
            Selection.activeObject = lethalToolkitSettings;
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/01. Experimentation", false, 10)]
        static void LoadLevelExperimentation()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level1Experimentation.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/02. Assurance", false, 10)]
        static void LoadLevelAssurance()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level2Assurance.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/03. Vow", false, 10)]
        static void LoadLevelVow()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level3Vow.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/04. March", false, 10)]
        static void LoadLevelMarch()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level4March.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/05. Offense", false, 10)]
        static void LoadLevelOffense()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level7Offense.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/05. Rend", false, 10)]
        static void LoadLevelRend()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level5Rend.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/06. Dine", false, 10)]
        static void LoadLevelDine()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level6Dine.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }

        [MenuItem("LethalToolkit/Scenes/Vanilla Moons/07. Titan", false, 10)]
        static void LoadLevelTitan()
        {
            string path = lethalToolkitSettings.vanillaScenesFolderDir;
            path += "Level8Titan.unity";
            lethalToolkitManager.OpenScene(path, OpenSceneMode.Single);
        }
    }
}
