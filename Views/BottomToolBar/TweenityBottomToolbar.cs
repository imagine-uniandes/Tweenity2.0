using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Controllers;
using Views.MiddlePanel;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Views
{
    public static class TweenityBottomToolbar
    {
        public static VisualElement CreateBottomToolbar(GraphController graphController)
        {
            var bottomBar = new VisualElement();
            bottomBar.style.flexDirection = FlexDirection.Row;
            bottomBar.style.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.15f, 0.15f, 0.15f)
                : new Color(0.65f, 0.65f, 0.65f);
            bottomBar.style.height = 40;
            bottomBar.style.paddingLeft = 8;
            bottomBar.style.paddingRight = 8;
            bottomBar.style.alignItems = Align.Center;

            var leftContainer = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexStart,
                    flexGrow = 1,
                    flexBasis = new Length(50, LengthUnit.Percent)
                }
            };

            var centerButton = new Button(() =>
            {
                var selectedNode = graphController.GraphView?.selection
                    .OfType<TweenityNode>()
                    .FirstOrDefault();

                if (selectedNode != null)
                {
                    graphController.GraphView.CenterOnNode(selectedNode.NodeID);
                }
                else
                {
                    Debug.Log("⚠️ No node is currently selected.");
                }
            })
            {
                text = "Center On Selection"
            };

            centerButton.style.flexGrow = 1;
            centerButton.style.minHeight = 22;
            centerButton.style.marginRight = 6;

            leftContainer.Add(centerButton);
            bottomBar.Add(leftContainer);

            var rightContainer = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexEnd,
                    flexGrow = 1,
                    flexBasis = new Length(50, LengthUnit.Percent)
                }
            };

            var searchField = new TextField
            {
                style = {
                    flexGrow = 1,
                    marginRight = 4,
                    minWidth = 180,
                    minHeight = 22
                }
            };
            searchField.label = "Find Node";

            static string ToNormalizedLower(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) return "";
                var normalized = input.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder();
                foreach (var c in normalized)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        sb.Append(c);
                }
                return sb.ToString().ToLowerInvariant().Replace(" ", "");
            }

            void ExecuteSearch(string query)
            {
                var cleanedQuery = ToNormalizedLower(query);

                if (string.IsNullOrEmpty(cleanedQuery)) return;

                var match = graphController.Graph.Nodes
                    .FirstOrDefault(n => ToNormalizedLower(n.Title).Contains(cleanedQuery));

                if (match != null)
                {
                    graphController.GraphView.CenterOnNode(match.NodeID);
                }
                else
                {
                    Debug.Log($"🔍 No match found for: {query}");
                }
            }

            var input = searchField.Q("unity-text-input");
            input.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    ExecuteSearch(searchField.value);
                    evt.StopPropagation();
                }
            });

            var searchButton = new Button(() =>
            {
                ExecuteSearch(searchField.value);
            });

            var searchIcon = new StyleBackground(EditorGUIUtility.IconContent("Search Icon").image as Texture2D);
            searchButton.style.width = 24;
            searchButton.style.height = 24;
            searchButton.style.backgroundImage = searchIcon;
            searchButton.style.backgroundColor = Color.clear;
            searchButton.style.borderBottomWidth = 0;
            searchButton.style.borderTopWidth = 0;
            searchButton.style.borderLeftWidth = 0;
            searchButton.style.borderRightWidth = 0;
            searchButton.style.marginLeft = 2;
            searchButton.style.marginRight = 2;

            rightContainer.Add(searchField);
            rightContainer.Add(searchButton);
            bottomBar.Add(rightContainer);

            return bottomBar;
        }
    }
}
