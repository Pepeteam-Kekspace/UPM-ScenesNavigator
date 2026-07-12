using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScenesNavigators.Core
{
    public class SceneRowView : VisualElement
    {
        private const string RowClass = "scene-row";
        private const string OpenClass = "scene-row--open";
        private const string MissingClass = "scene-row--missing";
        private const string NameClass = "scene-row__name";
        private const string ActionsClass = "scene-row__actions";
        private const string ActionButtonClass = "scene-row__action-button";

        private readonly string _scenePath;
        private readonly string _sceneName;
        private bool _isVisible = true;

        public string ScenePath => _scenePath;
        public Action<string> OnOpenClicked { get; set; }
        public Action<string> OnAdditiveClicked { get; set; }
        public Action<string> OnPingClicked { get; set; }

        public SceneRowView(string scenePath)
        {
            _scenePath = scenePath;
            _sceneName = Path.GetFileNameWithoutExtension(scenePath);

            AddToClassList(RowClass);
            CreateNameLabel();
            CreateActionButtons();
            RegisterCallback<PointerDownEvent>(HandleRowPointerDown);

            if (!HasSceneAsset(_scenePath))
                SetAsMissing();
        }

        public void MarkAsOpen()
        {
            AddToClassList(OpenClass);
        }

        public void MarkAsClosed()
        {
            RemoveFromClassList(OpenClass);
        }

        public void Show()
        {
            style.display = DisplayStyle.Flex;
            _isVisible = true;
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
            _isVisible = false;
        }

        public bool IsVisible()
        {
            return _isVisible;
        }

        public bool HasNameContaining(string text)
        {
            return _sceneName.ToLower().Contains(text.ToLower());
        }

        private void CreateNameLabel()
        {
            Label nameLabel = new Label(_sceneName);
            nameLabel.AddToClassList(NameClass);
            Add(nameLabel);
        }

        private void CreateActionButtons()
        {
            VisualElement actionsContainer = new VisualElement();
            actionsContainer.AddToClassList(ActionsClass);
            actionsContainer.Add(CreateActionButtonWithIcon("Toolbar Plus", "Open additive", InvokeAdditiveClicked));
            actionsContainer.Add(CreateActionButtonWithIcon("ViewToolZoom", "Ping asset in Project", InvokePingClicked));
            Add(actionsContainer);
        }

        private Button CreateActionButtonWithIcon(string iconName, string buttonTooltip, Action clickHandler)
        {
            Button actionButton = new Button(clickHandler);
            actionButton.AddToClassList(ActionButtonClass);
            actionButton.tooltip = buttonTooltip;
            actionButton.RegisterCallback<PointerDownEvent>(StopEventPropagation);

            Image buttonIcon = new Image();
            buttonIcon.image = GetThemedIconWithName(iconName);
            actionButton.Add(buttonIcon);

            return actionButton;
        }

        private void InvokeAdditiveClicked()
        {
            OnAdditiveClicked?.Invoke(_scenePath);
        }

        private void InvokePingClicked()
        {
            OnPingClicked?.Invoke(_scenePath);
        }

        private void StopEventPropagation(PointerDownEvent pointerDownEvent)
        {
            pointerDownEvent.StopPropagation();
        }

        private void HandleRowPointerDown(PointerDownEvent pointerDownEvent)
        {
            if (pointerDownEvent.button == 0)
                OnOpenClicked?.Invoke(_scenePath);

            if (pointerDownEvent.button == 1)
                OnPingClicked?.Invoke(_scenePath);

            if (pointerDownEvent.button == 2)
                OnAdditiveClicked?.Invoke(_scenePath);
        }

        private void SetAsMissing()
        {
            AddToClassList(MissingClass);
            SetEnabled(false);
        }

        private static bool HasSceneAsset(string scenePath)
        {
            return !ReferenceEquals(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), null);
        }

        private static Texture GetThemedIconWithName(string iconName)
        {
            if (!EditorGUIUtility.isProSkin)
                return EditorGUIUtility.IconContent(iconName).image;

            Texture darkThemeIcon = EditorGUIUtility.IconContent("d_" + iconName).image;
            if (!ReferenceEquals(darkThemeIcon, null))
                return darkThemeIcon;

            return EditorGUIUtility.IconContent(iconName).image;
        }
    }
}
