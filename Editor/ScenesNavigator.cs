using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScenesNavigators.Core
{
    public class ScenesNavigator : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Vector2 _searchScrollPosition;
        private string _sceneToSearch = "";
        private GUIStyle _sceneButtonStyle;
        private Queue<string> _lastestScenes = new Queue<string>();

        [MenuItem("Tools/ScenesNavigator")]
        private static void ShowWindow()
        {
            GetWindow(typeof(ScenesNavigator), false, "Scenes Navigator");
        }

        private void OnGUI()
        {
            DrawOptions();
            DrawScenesList();
            DrawLatestScenes();
        }

        private void DrawLatestScenes()
        {
            if(_lastestScenes == null || _lastestScenes.Count == 0)
                return;
            
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Latest opened scenes");
            
            if(GUILayout.Button(EditorGUIUtility.IconContent("Cancel"), GUILayout.Width(30)))
                _lastestScenes.Clear();
            
            GUILayout.EndHorizontal();
            foreach (var scene in _lastestScenes)
            {
                GUILayout.Space(5);
                DrawSceneButton(scene);
            }
            
            GUILayout.EndVertical();
        }

        private void DrawOptions()
        {
            GUILayout.BeginVertical("Box");
            
            DrawSearchBar();

            GUILayout.EndVertical();
        }

        private void DrawSearchBar()
        {
            _sceneButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            _sceneButtonStyle.fontSize = 10;

            GUILayout.BeginHorizontal();

            _sceneToSearch = GUILayout.TextField(_sceneToSearch, new GUIStyle("ToolbarSearchTextField"));

            if (_sceneToSearch != "")
            {
                
                if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_mac_close_h"), EditorStyles.iconButton, 
                        GUILayout.Width(18), GUILayout.Height(18)))
                    _sceneToSearch = "";
            }

            GUILayout.EndHorizontal();

            if (_sceneToSearch == "")
            {
                _searchScrollPosition = Vector2.zero;
                return;
            }
            
            GUILayout.BeginVertical();

            _searchScrollPosition = GUILayout.BeginScrollView(_searchScrollPosition, false, false, GUILayout.Width(position.width - 15), 
                GUILayout.MinHeight(1), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));
            
            foreach (var scene in EditorBuildSettings.scenes)
            {
                string sceneName = GetSceneName(scene.path);
                sceneName = sceneName.ToLower();
                bool exists = sceneName.Contains(_sceneToSearch.ToLower());
                if (!exists) 
                    continue;

                GUILayout.Space(5);

                DrawSceneButton(scene.path);
            }
            
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawScenesList()
        {
            GUILayout.BeginVertical("Box");

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(position.width - 15), 
                GUILayout.MinHeight(1), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));

            DrawAllScenes();

            if (GUILayout.Button("Open all scenes"))
            {
                foreach (var scene in EditorBuildSettings.scenes)
                    EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawAllScenes()
        {
            _sceneButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            _sceneButtonStyle.fontSize = 10;
            _sceneButtonStyle.alignment = TextAnchor.MiddleLeft;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                GUILayout.Space(5);

                DrawSceneButton(scene.path);
            }
        }

        private void DrawSceneButton(string path)
        {
            if (!GUILayout.Button(GetSceneName(path), _sceneButtonStyle)) 
                return;

            switch (Event.current.button)
            {
                case 0:
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(path);
                    AddSceneToLatestOpened(path);
                    break;
                case 2:
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    AddSceneToLatestOpened(path);
                    break;
                case 1:
                {
                    Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                    break;
                }
            }
        }

        private void AddSceneToLatestOpened(string scenePath)
        {
            if (_lastestScenes.Contains(scenePath))
                return;
            
            _lastestScenes.Enqueue(scenePath);
            if (_lastestScenes.Count > 2)
                _lastestScenes.Dequeue();
        }

        private string GetSceneName(string _path)
        {
            char[] path = _path.ToCharArray();
            string sceneName = "";

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/')
                    break;
                sceneName += path[i];
            }

            path = sceneName.ToCharArray();
            sceneName = "";

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                    break;
                sceneName += path[i];
            }

            return sceneName;
        }
    }
}