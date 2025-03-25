using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class DialogueView : VisualElement
    {
        private ListView _responseList;
        private DialogueNodeModel _model;
        private GraphController _controller;

        public DialogueView(DialogueNodeModel model, GraphController controller)
        {
            _model = model;
            _controller = controller;

            style.paddingLeft = 5;
            style.paddingRight = 5;
            style.paddingTop = 5;
            style.paddingBottom = 5;

            Label header = new Label("Dialogue Node Details");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(header);

            Add(new Label("Dialogue Text"));
            var dialogueText = new TextField { value = _model.DialogueText };
            dialogueText.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateDialogueText(_model, evt.newValue);
            });
            Add(dialogueText);

            Add(new Label("Responses"));
            var addButton = new Button(() =>
            {
                _controller.AddDialogueResponse(_model);
                _responseList.Rebuild();
            })
            { text = "+ Add Response" };
            Add(addButton);

            _responseList = new ListView(_model.Responses, 20, () => new Label(), (e, i) =>
            {
                (e as Label).text = _model.Responses[i];
            });
            Add(_responseList);
        }
    }
}
