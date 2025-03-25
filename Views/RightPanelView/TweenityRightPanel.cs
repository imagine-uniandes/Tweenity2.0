using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace Views
{
    public static class TweenityRightPanel
    {
        public static VisualElement CreateRightPanel()
        {
            VisualElement rightPanel = new VisualElement();
            rightPanel.name = "RightPanel";
            rightPanel.style.width = 250;
            rightPanel.style.backgroundColor = EditorGUIUtility.isProSkin ?
                new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
            rightPanel.style.paddingLeft = 5;
            rightPanel.style.paddingRight = 5;
            rightPanel.style.paddingTop = 5;
            rightPanel.style.paddingBottom = 5;
            rightPanel.style.flexDirection = FlexDirection.Column;

            return rightPanel;
        }
    }
}
