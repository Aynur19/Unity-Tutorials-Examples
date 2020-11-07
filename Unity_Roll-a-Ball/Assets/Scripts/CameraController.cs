using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер камеры.
/// </summary>
public class CameraController : MonoBehaviour
{
    #region Private Serialize Fields
    /// <summary>
    /// Цель для камеры
    /// </summary>
    [SerializeField]
    private Transform lookAtTarget;
    
    /// <summary>
    /// Объект игры - игрок.
    /// </summary>
    [SerializeField]
    private GameObject player;
    #endregion

    #region Private Fields
    /// <summary>
    /// Трехмерный вектор смещения.
    /// </summary>
    private Vector3 offset;

    /// <summary>
    /// Начальный 4-хмерный вектор вращения камеры.
    /// </summary>
    private Quaternion originRotation;

    /// <summary>
    /// Компонет Transform текущей цели.
    /// </summary>
    private Transform currentTargetTransform;

    /// <summary>
    /// Указатель того, что текущая цель камеры - игрок.
    /// </summary>
    private bool isLookAtPlayer;
    #endregion

    #region Camera Life Cycle
    /// <summary>
    /// Начальный метод вызываемый перед обновлением первого кадра.
    /// </summary>
    private void Start()
    {
        ComponentsInitialise();   
    }
    
    /// <summary>
    /// Метод позднего обновления, который вызывается после всех остальных обновлений (FixedUpdate).
    /// </summary>
    private void LateUpdate()
    {
        transform.position = currentTargetTransform.position + offset;
    }
    #endregion

    #region Setters
    /// <summary>
    /// Инициализация компонентов.
    /// </summary>
    private void ComponentsInitialise()
    {
        // Устанавливаем Transform текущей цели.
        currentTargetTransform = player.transform;
        // Расчитываем вектор смещения (константу) относительно начальных положений текущей цели и игрока.
        offset = transform.position - currentTargetTransform.position;
        
        // Получаем вектор поворота.
        originRotation = transform.rotation;
        
        // Указываем, что текущая цель камеры - игрок.
        isLookAtPlayer = true;
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Изменение цели камеры.
    /// </summary>
    /// <param name="lookAtValue"></param>
    private void OnLookAtTarget(InputValue lookAtValue)
    {
        // В зависимости от текущей устанавливае текущий трансформ и трансформа будущей цели.
        if(isLookAtPlayer)
        {
            currentTargetTransform = lookAtTarget;
        }
        else
        {
            currentTargetTransform = player.transform;
        }
        
        // Меняем цель, оставляем поворот камеры в изначальном положении, меняем значение флажка на обратный.
        transform.LookAt(currentTargetTransform);
        transform.rotation = originRotation;
        isLookAtPlayer = !isLookAtPlayer;
    }
    #endregion
}
