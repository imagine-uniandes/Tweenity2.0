using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

public static class TweenityRightPanel
{
    public static VisualElement CreateRightPanel()
    {
        VisualElement rightPanel = new VisualElement();
        rightPanel.style.width = 250;
        rightPanel.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
        rightPanel.style.paddingLeft = 5;
        rightPanel.style.paddingRight = 5;
        rightPanel.style.paddingTop = 5;
        rightPanel.style.paddingBottom = 5;

        // Node details section        
        VisualElement nodeSection = new VisualElement();
        nodeSection.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.85f, 0.85f, 0.85f);
        nodeSection.style.paddingLeft = 5;
        nodeSection.style.paddingRight = 5;
        nodeSection.style.paddingTop = 5;
        nodeSection.style.paddingBottom = 5;
        nodeSection.style.borderTopLeftRadius = 5;
        nodeSection.style.borderTopRightRadius = 5;
        nodeSection.style.borderBottomLeftRadius = 5;
        nodeSection.style.borderBottomRightRadius = 5;

        Label nodeDetailsLabel = new Label("Node Details");
        nodeDetailsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nodeSection.Add(nodeDetailsLabel);

        DropdownField nodeTypeDropdown = new DropdownField("Node Type");
        nodeTypeDropdown.choices = new List<string>(System.Enum.GetNames(typeof(NodeType)));
        nodeTypeDropdown.value = NodeType.NoType.ToString(); // Set default value
        nodeSection.Add(nodeTypeDropdown);

        // Container for dynamic content
        VisualElement dynamicContent = new VisualElement();
        nodeSection.Add(dynamicContent);

        // Function to update the view based on selected node type
        void UpdateNodeView(NodeType selectedType)
        {
            dynamicContent.Clear();
            switch (selectedType)
            {
                case NodeType.NoType:
                    dynamicContent.Add(new Nodes.NoTypeView());
                    break;
                case NodeType.Start:
                    dynamicContent.Add(new Nodes.StartView());
                    break;
                case NodeType.End:
                    dynamicContent.Add(new Nodes.EndView());
                    break;
                case NodeType.Random:
                    dynamicContent.Add(new Nodes.RandomView());
                    break;
                case NodeType.MultipleChoice:
                    dynamicContent.Add(new Nodes.MultipleChoiceView());
                    break;
                case NodeType.Reminder:
                    dynamicContent.Add(new Nodes.ReminderView());
                    break;
                case NodeType.Timeout:
                    dynamicContent.Add(new Nodes.TimeoutView());
                    break;
                case NodeType.Dialogue:
                    dynamicContent.Add(new Nodes.DialogueView());
                    break;
            }
        }

        // Register callback for dropdown changes
        nodeTypeDropdown.RegisterValueChangedCallback(evt =>
        {
            if (System.Enum.TryParse(evt.newValue, out NodeType selectedType))
            {
                UpdateNodeView(selectedType);
            }
        });

        // Initialize default view
        UpdateNodeView(NodeType.NoType);

        rightPanel.Add(nodeSection);
        return rightPanel;
    }
}
