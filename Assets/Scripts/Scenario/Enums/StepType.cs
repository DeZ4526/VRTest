namespace MEGroup.Test.Scenario.Enums
{
    /// <summary>
    /// Тип действия, которое нужно выполнить на шаге.
    /// </summary>
    public enum StepType
    {
        /// <summary>Взять объект (требуется XRGrabInteractable)</summary>
        PickObject,
        /// <summary>Установить объект в сокет (требуется XRSocketInteractor)</summary>
        PlaceObject,
        /// <summary>Переключить рычаг (требуется XRBaseInteractable + LeverController)</summary>
        ToggleLever
    }
}