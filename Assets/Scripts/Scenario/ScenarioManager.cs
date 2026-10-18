using MEGroup.Test.Scenario.Dto;
using MEGroup.Test.Scenario.Enums;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MEGroup.Test.Scenario
{
    public class ScenarioManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI taskText;

        [Header("Scenario Steps")]
        [Tooltip("Массив шагов сценария. Выполняются строго по порядку.")]
        [SerializeField]
        private List<ScenarioStep> steps = new()
        {
            new (StepType.PickObject, "Возьмите объект A со стола."),
            new (StepType.PlaceObject, "Подойдите к объекту B и установите в него объект A."),
            new (StepType.ToggleLever, "Подойдите к рычагу и переведите его в положение «Включено».")
        };

        [Header("Completion Event")]
        [Tooltip("Срабатывает, когда все шаги выполнены")]
        public UnityEvent onScenarioCompleted;

        private int currentStepIndex = 0;
        private bool isCompleted = false;

        public int CurrentStepIndex => currentStepIndex;
        public bool IsCompleted => isCompleted;
        public IReadOnlyList<ScenarioStep> Steps => steps;

        [ContextMenu("Reset to Default Scenario")]
        public void ResetToDefault()
        {
            steps = new List<ScenarioStep>
            {
                new (StepType.PickObject, "Возьмите объект A со стола."),
                new (StepType.PlaceObject, "Подойдите к объекту B и установите в него объект A."),
                new (StepType.ToggleLever, "Подойдите к рычагу и переведите его в положение «Включено».")
            };
        }

        private void Awake()
        {
            SubscribeToAllSteps();
        }

        private void Start()
        {
            UpdateUI();
            UpdateHighlight();
        }

        private void SubscribeToAllSteps()
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];

                if (step.TargetComponent == null)
                {
                    Debug.LogWarning($"[ScenarioManager] Шаг {i} ({step.Type}): не назначен TargetComponent в Inspector. Пропускаю.");
                    continue;
                }

                int stepIndex = i;

                switch (step.Type)
                {
                    case StepType.PickObject:
                        if (step.TargetComponent is XRGrabInteractable grab)
                        {
                            grab.selectEntered.AddListener(args => OnPickStepCompleted(stepIndex));
                        }
                        else
                        {
                            Debug.LogError($"[ScenarioManager] Шаг {i} имеет тип PickObject, но TargetComponent не является XRGrabInteractable! (Текущий: {step.TargetComponent.GetType().Name})");
                        }
                        break;

                    case StepType.PlaceObject:
                        if (step.TargetComponent is XRSocketInteractor socket)
                        {
                            socket.selectEntered.AddListener(args => OnSocketSelectEntered(stepIndex, args));
                        }
                        else
                        {
                            Debug.LogError($"[ScenarioManager] Шаг {i} имеет тип PlaceObject, но TargetComponent не является XRSocketInteractor! (Текущий: {step.TargetComponent.GetType().Name})");
                        }
                        break;

                    case StepType.ToggleLever:
                        var lever = step.TargetComponent.GetComponentInParent<LeverController>();
                        if (lever != null)
                        {
                            lever.OnLeverToggled.AddListener(() => OnLeverToggled(stepIndex));
                        }
                        else
                        {
                            Debug.LogError($"[ScenarioManager] Шаг {i} имеет тип ToggleLever, но на {step.TargetComponent.name} или его родителях нет компонента LeverController!");
                        }
                        break;
                }
            }
        }

        private void OnPickStepCompleted(int stepIndex)
        {
            if (isCompleted || stepIndex != currentStepIndex) return;
            AdvanceToNextStep();
        }

        private void OnSocketSelectEntered(int stepIndex, SelectEnterEventArgs args)
        {
            if (isCompleted || stepIndex != currentStepIndex) return;

            var step = steps[stepIndex];

            if (step.RequiredObject != null)
            {
                if (args.interactableObject is XRGrabInteractable grabbed && grabbed == step.RequiredObject)
                {
                    AdvanceToNextStep();
                }
            }
            else
            {
                AdvanceToNextStep();
            }
        }

        private void OnLeverToggled(int stepIndex)
        {
            if (isCompleted || stepIndex != currentStepIndex) return;

            var step = steps[stepIndex];
            var lever = step.TargetComponent.GetComponentInParent<LeverController>();

            if (lever != null && lever.IsOn)
            {
                AdvanceToNextStep();
            }
        }

        private void AdvanceToNextStep()
        {
            currentStepIndex++;

            if (currentStepIndex >= steps.Count)
            {
                isCompleted = true;
                if (taskText != null)
                    taskText.text = "Тренировка завершена.";

                UpdateHighlight();
                onScenarioCompleted?.Invoke();
                return;
            }

            UpdateUI();
            UpdateHighlight();
        }

        private void UpdateUI()
        {
            if (taskText != null && currentStepIndex < steps.Count)
                taskText.text = steps[currentStepIndex].TaskText;
        }

        private void UpdateHighlight()
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step.Outline != null)
                {
                    step.Outline.enabled = (i == currentStepIndex && !isCompleted);
                }
            }
        }

        public void RestartScenario()
        {
            currentStepIndex = 0;
            isCompleted = false;

            foreach (var step in steps)
            {
                if (step.TargetComponent == null) continue;

                step.TargetComponent
                    .GetComponentInParent<LeverController>()
                    ?.ResetLever();
            }

            UpdateUI();
            UpdateHighlight();
        }
    }
}