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
        nodeTypeDropdown.choices = new List<string> {"noType", "Start", "End", "Random", "MultipleChoice", "Reminder", "Timeout", "Dialogue"};
        nodeTypeDropdown.value = "noType"; // Set default value
        nodeSection.Add(nodeTypeDropdown);

        nodeSection.Add(new Label("Title"));
        TextField titleField = new TextField();
        nodeSection.Add(titleField);
        nodeSection.Add(new Label("Description"));
        TextField descriptionField = new TextField();
        nodeSection.Add(descriptionField);

        // Expected User Action Section
        nodeSection.Add(new Label("Expected User Action"));
        TextField expectedUserActionField = new TextField();
        nodeSection.Add(expectedUserActionField);

        // Simulator Actions Section
        nodeSection.Add(new Label("Simulator Actions"));
        ListView simulatorActionsList = new ListView();
        nodeSection.Add(simulatorActionsList);

        // Timeout & Reminder Settings
        nodeSection.Add(new Label("Reminder Time (seconds)"));
        FloatField reminderTimeField = new FloatField();
        nodeSection.Add(reminderTimeField);

        nodeSection.Add(new Label("Timeout Duration (seconds)"));
        FloatField timeoutDurationField = new FloatField();
        nodeSection.Add(timeoutDurationField);

        // Audio & Dialogue Settings
        nodeSection.Add(new Label("Audio File Name"));
        TextField audioFileField = new TextField();
        nodeSection.Add(audioFileField);

        nodeSection.Add(new Label("Dialogue Text"));
        TextField dialogueTextField = new TextField();
        nodeSection.Add(dialogueTextField);

        // Responses (Connected Nodes)
        nodeSection.Add(new Label("Connected Nodes"));
        ListView connectedNodesList = new ListView();
        nodeSection.Add(connectedNodesList);

        rightPanel.Add(nodeSection);
        return rightPanel;
    }
}