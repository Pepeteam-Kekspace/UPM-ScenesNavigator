using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ScenesNavigators.Core
{
    public class ScenesNavigator : EditorWindow
    {
        private const string StyleSheetPath = "Packages/com.lalo.scenesnavigator/Editor/ScenesNavigator.uss";

        private readonly List<SceneRowView> _sceneRows = new List<SceneRowView>();
        private readonly List<SceneRowView> _recentRows = new List<SceneRowView>();
        private RecentScenesRepository _recentScenesRepository;
        private ToolbarSearchField _searchField;
        private VisualElement _recentSection;
        private VisualElement _recentRowsContainer;
        private ScrollView _scenesScrollView;

        [MenuItem("Tools/ScenesNavigator")]
        private static void ShowWindow()
        {
            GetWindow(typeof(ScenesNavigator), false, "Scenes");
        }

        private void CreateGUI()
        {
            _recentScenesRepository = new RecentScenesRepository();

            LoadStyleSheet();
            CreateToolbar();
            CreateRecentSection();
            CreateScenesSection();
            RebuildAllRows();
            SubscribeToEditorEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEditorEvents();
        }

        private void LoadStyleSheet()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (ReferenceEquals(styleSheet, null))
                return;

            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            _searchField = new ToolbarSearchField();
            _searchField.AddToClassList("toolbar__search");
            _searchField.RegisterValueChangedCallback(HandleSearchChanged);
            toolbar.Add(_searchField);

            ToolbarButton openAllButton = new ToolbarButton(OpenAllScenes);
            openAllButton.text = "Open All";
            toolbar.Add(openAllButton);

            rootVisualElement.Add(toolbar);
        }

        private void CreateRecentSection()
        {
            _recentSection = new VisualElement();
            _recentSection.AddToClassList("section");

            VisualElement header = CreateSectionHeader("RECENT");
            Button clearButton = new Button(ClearRecentScenes);
            clearButton.AddToClassList("section__clear-button");
            clearButton.text = "✕";
            clearButton.tooltip = "Clear recent scenes";
            header.Add(clearButton);
            _recentSection.Add(header);

            _recentRowsContainer = new VisualElement();
            _recentSection.Add(_recentRowsContainer);

            rootVisualElement.Add(_recentSection);
        }

        private void CreateScenesSection()
        {
            VisualElement scenesSection = new VisualElement();
            scenesSection.AddToClassList("section");
            scenesSection.AddToClassList("section--scenes");
            
            _scenesScrollView = new ScrollView();
            _scenesScrollView.AddToClassList("section__scroll");
            scenesSection.Add(_scenesScrollView);

            rootVisualElement.Add(scenesSection);
        }

        private VisualElement CreateSectionHeader(string title)
        {
            VisualElement header = new VisualElement();
            header.AddToClassList("section__header");

            Label titleLabel = new Label(title);
            titleLabel.AddToClassList("section__title");
            header.Add(titleLabel);

            return header;
        }

        private void SubscribeToEditorEvents()
        {
            EditorBuildSettings.sceneListChanged += RebuildAllRows;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorSceneManager.sceneClosed += HandleSceneClosed;
        }

        private void UnsubscribeFromEditorEvents()
        {
            EditorBuildSettings.sceneListChanged -= RebuildAllRows;
            EditorSceneManager.sceneOpened -= HandleSceneOpened;
            EditorSceneManager.sceneClosed -= HandleSceneClosed;
        }

        private void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RefreshOpenHighlights();
        }

        private void HandleSceneClosed(Scene scene)
        {
            RefreshOpenHighlights();
        }

        private void HandleSearchChanged(ChangeEvent<string> changeEvent)
        {
            ApplySearchFilter();
        }

        private void RebuildAllRows()
        {
            RebuildSceneRows();
            RebuildRecentRows();
        }

        private void RebuildSceneRows()
        {
            _sceneRows.Clear();
            _scenesScrollView.Clear();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                SceneRowView sceneRow = CreateSceneRow(scene.path);
                _sceneRows.Add(sceneRow);
                _scenesScrollView.Add(sceneRow);
            }

            if (_sceneRows.Count == 0)
                _scenesScrollView.Add(CreateEmptyListLabel());

            ApplySearchFilter();
            RefreshOpenHighlights();
        }

        private void RebuildRecentRows()
        {
            _recentRows.Clear();
            _recentRowsContainer.Clear();

            foreach (string scenePath in _recentScenesRepository.GetAll())
            {
                SceneRowView recentRow = CreateSceneRow(scenePath);
                _recentRows.Add(recentRow);
                _recentRowsContainer.Add(recentRow);
            }

            ApplySearchFilter();
            RefreshOpenHighlights();
        }

        private SceneRowView CreateSceneRow(string scenePath)
        {
            SceneRowView sceneRow = new SceneRowView(scenePath);
            sceneRow.OnOpenClicked = OpenSceneSingle;
            sceneRow.OnAdditiveClicked = OpenSceneAdditive;
            sceneRow.OnPingClicked = PingSceneAsset;
            return sceneRow;
        }

        private Label CreateEmptyListLabel()
        {
            Label emptyLabel = new Label("No scenes in Build Settings");
            emptyLabel.AddToClassList("section__empty-message");
            return emptyLabel;
        }

        private void ApplySearchFilter()
        {
            string searchText = _searchField.value;

            foreach (SceneRowView sceneRow in _sceneRows)
                ApplyFilterToRow(sceneRow, searchText);

            foreach (SceneRowView recentRow in _recentRows)
                ApplyFilterToRow(recentRow, searchText);

            UpdateRecentSectionVisibility();
        }

        private void ApplyFilterToRow(SceneRowView sceneRow, string searchText)
        {
            if (sceneRow.HasNameContaining(searchText))
            {
                sceneRow.Show();
                return;
            }

            sceneRow.Hide();
        }

        private void UpdateRecentSectionVisibility()
        {
            foreach (SceneRowView recentRow in _recentRows)
            {
                if (recentRow.IsVisible())
                {
                    _recentSection.style.display = DisplayStyle.Flex;
                    return;
                }
            }

            _recentSection.style.display = DisplayStyle.None;
        }

        private void RefreshOpenHighlights()
        {
            foreach (SceneRowView sceneRow in _sceneRows)
                RefreshRowOpenHighlight(sceneRow);

            foreach (SceneRowView recentRow in _recentRows)
                RefreshRowOpenHighlight(recentRow);
        }

        private void RefreshRowOpenHighlight(SceneRowView sceneRow)
        {
            if (IsSceneOpen(sceneRow.ScenePath))
            {
                sceneRow.MarkAsOpen();
                return;
            }

            sceneRow.MarkAsClosed();
        }

        private bool IsSceneOpen(string scenePath)
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                if (SceneManager.GetSceneAt(sceneIndex).path == scenePath)
                    return true;
            }

            return false;
        }

        private void OpenSceneSingle(string scenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(scenePath);
            _recentScenesRepository.Add(scenePath);
            RebuildRecentRows();
        }

        private void OpenSceneAdditive(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            _recentScenesRepository.Add(scenePath);
            RebuildRecentRows();
        }

        private void PingSceneAsset(string scenePath)
        {
            Object sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
            Selection.activeObject = sceneAsset;
            EditorGUIUtility.PingObject(sceneAsset);
        }

        private void OpenAllScenes()
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
        }

        private void ClearRecentScenes()
        {
            _recentScenesRepository.Clear();
            RebuildRecentRows();
        }
    }
}
