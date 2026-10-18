using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MEGroup.Test.Scenario.Dto
{
    [Serializable]
    public class ScenarioStep
    {
        [Tooltip("Òèï äåéñòâèÿ íà ıòîì øàãå")]
        [SerializeField] private Enums.StepType type;

        [Tooltip("Öåëåâîé êîìïîíåíò (íàçíà÷àåòñÿ â Inspector)")]
        [SerializeField] private Component targetComponent;

        [Tooltip("Òîëüêî äëÿ PlaceObject: êàêîé îáúåêò äîëæåí áûòü ïîìåù¸í â ñîêåò.")]
        [SerializeField] private XRGrabInteractable requiredObject;

        [Tooltip("Òåêñò çàäàíèÿ, îòîáğàæàåìûé â UI")]
        [TextArea(2, 4)]
        [SerializeField] private string taskText;

        [Tooltip("Êîìïîíåíò Quick Outline äëÿ ïîäñâåòêè (íàçíà÷àåòñÿ â Inspector)")]
        [SerializeField] private Outline outline;

        public ScenarioStep(Enums.StepType type, string taskText)
        {
            this.type = type;
            this.taskText = taskText;
        }

        public Enums.StepType Type => type;
        public Component TargetComponent => targetComponent;
        public XRGrabInteractable RequiredObject => requiredObject;
        public string TaskText => taskText;
        public Outline Outline => outline;
    }
}