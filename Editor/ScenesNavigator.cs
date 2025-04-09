using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScenesNavigators.Core
{
    public class ScenesNavigator : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _useMultiScene = false;
        private bool _hasToSelectOnProjectView = false;

        private string _sceneToSearch = "";
        private bool _isOptionsActivated;

        private GUIStyle _sceneButtonStyle;
        private GUIStyle _foldoutStyle;

        [MenuItem("Tools/ScenesNavigator")]
        private static void ShowWindow()
        {
            GetWindow(typeof(ScenesNavigator), false, "Scenes Navigator");
        }

        private void OnGUI()
        {
            DrawOptions();

            DrawScenesList();
        }

        private void DrawOptions()
        {
            GUILayout.BeginVertical("Box");

            _foldoutStyle = new GUIStyle(EditorStyles.foldoutHeader);
            _foldoutStyle.fontSize = 10;

            _isOptionsActivated = EditorGUILayout.BeginFoldoutHeaderGroup(_isOptionsActivated, "Settings", _foldoutStyle);

            if (_isOptionsActivated)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("Open Additive");
                _useMultiScene = EditorGUILayout.Toggle("", _useMultiScene);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Select on project");
                _hasToSelectOnProjectView = EditorGUILayout.Toggle("", _hasToSelectOnProjectView);

                GUILayout.EndHorizontal();

                DrawSearchBar();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

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
                return;
            
            GUILayout.BeginVertical();

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

            GUILayout.EndVertical();
        }

        private void DrawScenesList()
        {
            GUILayout.BeginVertical("Box");

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(position.width - 15), 
                GUILayout.MinHeight(1), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));

            DrawAllScenes();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawAllScenes()
        {
            _sceneButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            _sceneButtonStyle.fontSize = 10;

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
            
            if (Event.current.button == 0)
            {
                if (_useMultiScene)
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                else
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(path);
                }
                    
            }
            else if (Event.current.button == 2)
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            if (!_hasToSelectOnProjectView && Event.current.button != 1)
                return;

            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
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