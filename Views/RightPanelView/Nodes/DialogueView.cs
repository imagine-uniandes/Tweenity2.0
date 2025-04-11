using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;
using System;

namespace Views.RightPanel
{
    public class DialogueView : TweenityNodeView
    {
        private ListView _responseList;
        private const int MaxResponseLength = 39;

        public DialogueView(DialogueNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Dialogue Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            });

            var dialogueModel = (DialogueNodeModel)_model;

            // Dialogue Text
            Add(new Label("Question"));
            var dialogueText = new TextField { value = dialogueModel.DialogueText, multiline = true };
            dialogueText.RegisterValueChangedCallback(evt =>
            {
                dialogueModel.DialogueText = evt.newValue;
                controller.GraphView.RefreshNodeVisual(dialogueModel.NodeID);
            });
            Add(dialogueText);

            // Responses
            Add(new Label("Responses"));

            var addButton = new Button(() =>
            {
                dialogueModel.AddResponse("New Response");
                _responseList.Rebuild();
                controller.GraphView.RefreshNodeVisual(dialogueModel.NodeID);
            })
            { text = "+ Add Response" };
            Add(addButton);

            _responseList = new ListView(dialogueModel.OutgoingPaths, 70, () =>
            {
                var container = new VisualElement
                {
                    style = {
                        flexDirection = FlexDirection.Column,
                        marginBottom = 6,
                        backgroundColor = new Color(0, 0, 0, 0)
                    }
                };

                var responseField = new TextField
                {
                    multiline = true,
                    style = {
                        whiteSpace = WhiteSpace.Normal,
                        flexGrow = 0,
                        overflow = Overflow.Visible,
                        minHeight = 20,
                        marginBottom = 4
                    }
                };

                responseField.RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    int lines = evt.newValue.Split('\n').Length;
                    responseField.style.height = Mathf.Max(20, lines * 18);
                });

                var buttonRow = new VisualElement
                {
                    style = {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween
                    }
                };

                var connectButton = new Button
                {
                    style = {
                        flexGrow = 1
                    }
                };

                buttonRow.Add(connectButton);
                container.Add(responseField);
                container.Add(buttonRow);

                return container;
            },
            (element, i) =>
            {
                var container = element as VisualElement;
                var responseField = container.ElementAt(0) as TextField;
                var buttonRow = container.ElementAt(1) as VisualElement;
                var connectButton = buttonRow.ElementAt(0) as Button;

                var path = dialogueModel.OutgoingPaths[i];
                responseField.value = path.Label;

                responseField.RegisterValueChangedCallback(evt =>
                {
                    var clamped = evt.newValue.Length > MaxResponseLength
                        ? evt.newValue.Substring(0, MaxResponseLength)
                        : evt.newValue;
                    dialogueModel.UpdateResponse(i, clamped);
                    responseField.SetValueWithoutNotify(clamped);
                    controller.GraphView.RefreshNodeVisual(dialogueModel.NodeID);
                });

                if (!string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var targetTitle = controller.GetNode(path.TargetNodeID)?.Title ?? "(Unknown)";
                    connectButton.text = $"→ {targetTitle}";
                    connectButton.SetEnabled(false);
                }
                else
                {
                    connectButton.text = "Connect";
                    connectButton.SetEnabled(true);
                    connectButton.clickable = new Clickable(() =>
                    {
                        controller.StartConnectionFrom(dialogueModel.NodeID, targetNodeId =>
                        {
                            dialogueModel.ConnectResponseTo(i, targetNodeId);
                            controller.GraphView.RenderConnections();
                            _responseList.Rebuild(); // 👈 Refresh after assigning
                        });
                    });
                }
            });

            _responseList.selectionType = SelectionType.None;
            _responseList.style.borderBottomWidth = 0;
            _responseList.style.backgroundColor = new Color(0, 0, 0, 0);

            Add(_responseList);
        }
    }
}
