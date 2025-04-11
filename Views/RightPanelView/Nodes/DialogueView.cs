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

            _responseList = new ListView(dialogueModel.OutgoingPaths, 30, () =>
            {
                var container = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                };

                var textField = new TextField { style = { flexGrow = 1, marginRight = 4 } };
                var connectButton = new Button() { text = "Connect" };

                container.Add(textField);
                container.Add(connectButton);
                return container;
            },
            (e, i) =>
            {
                var container = e as VisualElement;
                var textField = container.ElementAt(0) as TextField;
                var connectButton = container.ElementAt(1) as Button;

                textField.value = dialogueModel.OutgoingPaths[i].Label;
                textField.RegisterValueChangedCallback(evt =>
                {
                    dialogueModel.UpdateResponse(i, evt.newValue);
                    controller.GraphView.RefreshNodeVisual(dialogueModel.NodeID);
                });

                connectButton.clickable = new Clickable(() =>
                {
                    controller.StartConnectionFrom(dialogueModel.NodeID, (targetNodeId) =>
                    {
                        dialogueModel.ConnectResponseTo(i, targetNodeId);
                        controller.GraphView.RenderConnections();
                        controller.GraphView.RefreshNodeVisual(dialogueModel.NodeID);
                    });
                });
            });

            Add(_responseList);
        }
    }
}
