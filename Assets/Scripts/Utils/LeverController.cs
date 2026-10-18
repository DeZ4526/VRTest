using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MEGroup.Test
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class LeverController : MonoBehaviour
    {
        [Header("Pivot — что именно вращаем")]
        [Tooltip("Если не назначен, вращается сам объект с компонентом.")]
        [SerializeField] private Transform pivot;

        [Header("Положения рычага")]
        [Tooltip("Ось вращения в локальных координатах pivot.")]
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;

        [Tooltip("Угол положения «Выключено», относительно исходной ориентации pivot.")]
        [SerializeField] private float offAngle = 0f;

        [Tooltip("Угол положения «Включено», относительно исходной ориентации pivot.")]
        [SerializeField] private float onAngle = 45f;

        [Header("Анимация")]
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Состояние")]
        [SerializeField] private bool isOn;
        public bool IsOn => isOn;

        [Header("События")]
        [SerializeField] private UnityEvent onLeverToggled = new UnityEvent();
        public UnityEvent OnLeverToggled => onLeverToggled;

        private XRSimpleInteractable _interactable;
        private Quaternion _restRotation;
        private Quaternion _offRotation;
        private Quaternion _onRotation;
        private Quaternion _targetRotation;

        private void Awake()
        {
            _interactable = GetComponent<XRSimpleInteractable>();

            if (pivot == null)
            {
                pivot = transform;
                Debug.LogWarning($"{name}: pivot не назначен — вращаю сам объект.", this);
            }

            _restRotation = pivot.localRotation;

            _offRotation = _restRotation * Quaternion.AngleAxis(offAngle, rotationAxis);
            _onRotation = _restRotation * Quaternion.AngleAxis(onAngle, rotationAxis);

            pivot.localRotation = isOn ? _onRotation : _offRotation;
            _targetRotation = pivot.localRotation;

            if (_interactable != null)
                _interactable.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnDestroy()
        {
            if (_interactable != null)
                _interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        private void Update()
        {
            if (pivot == null) return;

            if (rotationSpeed <= 0f)
            {
                pivot.localRotation = _targetRotation;
                return;
            }

            float t = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
            pivot.localRotation = Quaternion.Slerp(pivot.localRotation, _targetRotation, t);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Toggle();
        }

        public void Toggle() => SetOn(!isOn);

        public void SetOn(bool value)
        {
            if (isOn == value) return;

            isOn = value;
            _targetRotation = isOn ? _onRotation : _offRotation;
            onLeverToggled?.Invoke();
        }

        /// <summary>Для RestartScenario: мгновенно возвращаем в «Выключено».</summary>
        public void ResetLever()
        {
            isOn = false;
            _targetRotation = _offRotation;

            if (pivot != null)
                pivot.localRotation = _offRotation;
        }
    }
}