using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScenesNavigators.Core
{
    public class RecentScenesRepository
    {
        private const int MaxScenes = 5;
        private const char PathsSeparator = '|';

        private readonly string _preferencesKey;

        public RecentScenesRepository()
        {
            _preferencesKey = "ScenesNavigator.RecentScenes." + Application.dataPath.GetHashCode();
        }

        public void Add(string scenePath)
        {
            List<string> scenePaths = GetAll();
            scenePaths.Remove(scenePath);
            scenePaths.Insert(0, scenePath);

            if (scenePaths.Count > MaxScenes)
                scenePaths.RemoveRange(MaxScenes, scenePaths.Count - MaxScenes);

            EditorPrefs.SetString(_preferencesKey, string.Join(PathsSeparator.ToString(), scenePaths));
        }

        public List<string> GetAll()
        {
            string storedPaths = EditorPrefs.GetString(_preferencesKey, "");

            List<string> scenePaths = new List<string>();
            foreach (string scenePath in storedPaths.Split(PathsSeparator))
            {
                if (HasSceneAsset(scenePath))
                    scenePaths.Add(scenePath);
            }

            return scenePaths;
        }

        private static bool HasSceneAsset(string scenePath)
        {
            return !ReferenceEquals(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), null);
        }

        public void Clear()
        {
            EditorPrefs.DeleteKey(_preferencesKey);
        }
    }
}
