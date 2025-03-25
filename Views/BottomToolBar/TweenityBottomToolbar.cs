using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Views{
    public static class TweenityBottomToolbar
    {
        public static VisualElement CreateBottomToolbar()
        {
            VisualElement bottomBar = new VisualElement();
            bottomBar.style.flexDirection = FlexDirection.Row;
            bottomBar.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f);
            bottomBar.style.height = 30;
            bottomBar.style.paddingLeft = 5;
            bottomBar.style.paddingRight = 5;
            bottomBar.style.alignItems = Align.Center;
            
            Button debugButton = new Button(() => Debug.Log("Debugging Clicked")) { text = "Debugging" };
            Button selectionButton = new Button(() => Debug.Log("Current Selection")) { text = "Current Selection" };
            
            debugButton.style.flexGrow = 1;
            selectionButton.style.flexGrow = 1;
            
            bottomBar.Add(debugButton);
            bottomBar.Add(selectionButton);
            
            return bottomBar;
        }
    }
}