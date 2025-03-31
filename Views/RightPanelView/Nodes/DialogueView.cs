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
            Add(new Label("Dialogue Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var dialogueModel = (DialogueNodeModel)_model;

            // Dialogue Text
            Add(new Label("Dialogue Text"));
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

            _responseList = new ListView(dialogueModel.Responses, 20, () => new Label(), (e, i) =>
            {
                (e as Label).text = dialogueModel.Responses[i];
            });
            Add(_responseList);
        }
    }
}
