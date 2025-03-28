using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Controllers;

namespace Views{
    public static class TweenityLeftPanel
    {
        public static VisualElement CreateLeftPanel(GraphController graphController)
        {
            VisualElement leftPanel = new VisualElement();
            leftPanel.style.width = 200;
            leftPanel.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
            leftPanel.style.paddingLeft = 5;
            leftPanel.style.paddingRight = 5;
            leftPanel.style.paddingTop = 5;
            leftPanel.style.paddingBottom = 5;

            // Search Bar Section
            VisualElement searchSection = new VisualElement();
            searchSection.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.85f, 0.85f, 0.85f);
            searchSection.style.paddingLeft = 5;
            searchSection.style.paddingRight = 5;
            searchSection.style.paddingTop = 5;
            searchSection.style.paddingBottom = 5;
            searchSection.style.borderTopLeftRadius = 5;
            searchSection.style.borderTopRightRadius = 5;
            searchSection.style.borderBottomLeftRadius = 5;
            searchSection.style.borderBottomRightRadius = 5;
            searchSection.style.marginBottom = 5;

            Label searchLabel = new Label("Search Nodes");
            searchLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            searchSection.Add(searchLabel);

            TextField searchBar = new TextField();
            searchBar.RegisterValueChangedCallback(evt =>
            {
                graphController.SearchNodes(evt.newValue);
            });
            searchBar.style.flexGrow = 1;
            searchBar.value = "Search for a node...";
            searchBar.style.width = StyleKeyword.Auto;
            searchSection.Add(searchBar);
            
            leftPanel.Add(searchSection);

            // Hierarchy View Section
            VisualElement hierarchySection = new VisualElement();
            hierarchySection.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.85f, 0.85f, 0.85f);
            hierarchySection.style.paddingLeft = 5;
            hierarchySection.style.paddingRight = 5;
            hierarchySection.style.paddingTop = 5;
            hierarchySection.style.paddingBottom = 5;
            hierarchySection.style.borderTopLeftRadius = 5;
            hierarchySection.style.borderTopRightRadius = 5;
            hierarchySection.style.borderBottomLeftRadius = 5;
            hierarchySection.style.borderBottomRightRadius = 5;
            
            Label hierarchyLabel = new Label("Hierarchy View");
            hierarchyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            hierarchySection.Add(hierarchyLabel);
            leftPanel.Add(hierarchySection);
            
            return leftPanel;
        }
    }
}