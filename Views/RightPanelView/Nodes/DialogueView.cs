using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

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
                _controller.UpdateDialogueText(dialogueModel, evt.newValue);
            });
            Add(dialogueText);

            // Responses
            Add(new Label("Responses"));

            var addButton = new Button(() =>
            {
                _controller.AddDialogueResponse(dialogueModel);
                _responseList.Rebuild();
            })
            { text = "+ Add Response" };
            Add(addButton);

            _responseList = new ListView(dialogueModel.Responses, 30, () =>
            {
                var container = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 4 }
                };

                var textField = new TextField { style = { flexGrow = 1, marginRight = 4 } };
                var connectButton = new Button(() => { /* TODO: Handle connect */ }) { text = "Connect" };

                container.Add(textField);
                container.Add(connectButton);

                return container;
            },
            (e, i) =>
            {
                var container = e as VisualElement;
                var textField = container.ElementAt(0) as TextField;

                textField.value = dialogueModel.Responses[i];
                textField.RegisterValueChangedCallback(evt =>
                {
                    _controller.UpdateDialogueResponse(dialogueModel, i, evt.newValue);
                });
            });

            Add(_responseList);
        }
    }
}
