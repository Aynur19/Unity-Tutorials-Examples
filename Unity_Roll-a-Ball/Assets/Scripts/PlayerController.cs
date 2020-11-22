using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Контроллер игрока.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Private Serialize Fields
    /// <summary>
    /// Физическая обёртка снаряда.
    /// </summary>
    [SerializeField]
    private GameObject rocketPrefab;
    
    /// <summary>
    /// Игровой объект - другой мяч.
    /// </summary>
    [SerializeField] 
    private GameObject otherBall;
    
    /// <summary>
    /// Солнечный свет.
    /// </summary>
    [SerializeField]
    private Light sun;

    /// <summary>
    /// Свет от центральной лампы.
    /// </summary>
    [SerializeField]
    private Light centralLamp;
    
    /// <summary>
    /// Скорость перемещения.
    /// </summary>
    [SerializeField]
    private float speed;

    /// <summary>
    /// Компонент UI-текст вывода счета.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI countText;

    /// <summary>
    /// Компонент UI-текст вывода сообщения о выигрыше.
    /// </summary>
    [SerializeField]
    private GameObject winTextObject;
    
    /// <summary>
    /// Минимальная интенсивность света.
    /// </summary>
    [SerializeField] 
    private float minLightIntensity;
    
    /// <summary>
    /// Максимальная интенсивность света.
    /// </summary>
    [SerializeField] 
    private float maxLightIntensity;
    #endregion

    #region Private Fields
    /// <summary>
    /// Объект для симуляции физики игрока.
    /// </summary>
    private Rigidbody playerRigidbody;
    
    /// <summary>
    /// Счетчик касаний игрока с "PickUp".
    /// </summary>
    private int count;

    /// <summary>
    /// Направление передвижения игрока.
    /// </summary>
    private Vector2 moveDirection;

    /// <summary>
    /// Компонент отрисовки игрока.
    /// </summary>
    private Renderer playerRenderer;

    /// <summary>
    /// Счетчик смены цвета игрока.
    /// </summary>
    private int colorChangeCount;

    /// <summary>
    /// Обычный (начальный) цвет игрока.
    /// </summary>
    private Color originPlayerColor;

    /// <summary>
    /// Массив цветов игрока.
    /// </summary>
    private Color[] playerColors;

    /// <summary>
    /// Указатель того, что цвет игрока меняется через мето Color.Lerp().
    /// </summary>
    private bool isNextColorByLerp;

    /// <summary>
    /// Кортеж:
    /// 1. Указатель того, то включен режим изменения интенсивность света.
    /// 2. Указатель того, то интенсивность надо повышать, если true, иначе уменьшать. 
    /// </summary>
    private (bool, bool) isLightIntensityChangingMode;

    private DefaultInputActions inputActions; 
    #endregion

    #region Player Life Cycle
    /// <summary>
    /// Метод пробуждения игрового объекта (компонента).
    /// Вызывается перед методом Start() даже если скрипт выключен.
    /// </summary>
    private void Awake()
    {
        this.inputActions = new DefaultInputActions();
        this.inputActions.Player.Jump.performed += context => Jump();
        this.inputActions.Player.Move.performed += OnMove;
        this.inputActions.Player.ChangeDirectionalLightIntensity.performed += context => ChangeDirectionalLightIntensity();
        this.inputActions.Player.ChangeColorLerp.performed += context => ChangeColorLerp();
        this.inputActions.Player.ChangeColor.performed += context => ChangeColor();
        this.inputActions.Player.ChangeLightSource.performed += context => ChangeLightSource();
        this.inputActions.Player.DestroyOtherBall.performed += context => DestroyOtherBall();
        this.inputActions.Player.Fire.performed += context => Fire();
    }

    /// <summary>
    /// Включение компонентов игрока.
    /// </summary>
    private void OnEnable()
    {
        this.inputActions.Player.Enable();
    }

    /// <summary>
    /// Начальный метод игрока.
    /// </summary>
    private void Start()
    {
        ComponentsInitialise();
        SetCountText();
    }
    
    /// <summary>
    /// Обновление для физического движка.
    /// </summary>
    private void FixedUpdate()
    {
        Move(this.moveDirection);
    }
    
    /// <summary>
    /// Метод обновления кадра.
    /// Вызывается перед самым выводом следующего фрейма.
    /// </summary>
    private void Update()
    {
        // Плавное изменение цвета игрока.
        SetObjectMaterialColorByLerp(this.playerRenderer, this.isNextColorByLerp);

        // Плавное изменение интенсивности солнечного света.
        SetLightIntensityBySmoothDamp(this.sun, this.isLightIntensityChangingMode);
    }

    /// <summary>
    /// Выключение компонентов игрока.
    /// </summary>
    private void OnDisable()
    {
        this.inputActions.Player.Disable();
    }
    #endregion

    #region Mechanics
    /// <summary>
    /// Передвижение игрока.
    /// </summary>
    /// <param name="direction">Направление движения.</param>
    private void Move(Vector2 direction)
    {
        var currentMoveDirection = new Vector3(direction.x, 0, direction.y);
        this.playerRigidbody.AddForce(currentMoveDirection * (this.speed));
    }
    
    /// <summary>
    /// Огонь шариками.
    /// </summary>
    private void Fire()
    {

        var rocketInstance = Instantiate(rocketPrefab, playerRenderer.transform.position, 
                                         Quaternion.Euler(new Vector3(0, 0, 0)));
        rocketInstance.GetComponentInChildren<Rigidbody>().AddForce(new Vector3(0, 10, 0), ForceMode.Impulse);
    }
    
    /// <summary>
    /// Прыжок.
    /// </summary>
    private void Jump()
    {
        this.playerRigidbody.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
    }
    
    /// <summary>
    /// Плавное изменение интенсивности света - Directional Light через метод Math.SmoothDamp().
    /// </summary>
    private void ChangeDirectionalLightIntensity()
    {
        this.isLightIntensityChangingMode.Item1 = !this.isLightIntensityChangingMode.Item1;

        if(this.isLightIntensityChangingMode.Item1)
        {
            this.isLightIntensityChangingMode.Item2 = !this.isLightIntensityChangingMode.Item2;
        }
    }
    
    /// <summary>
    /// Плавное изменение цвета игрока
    /// </summary>
    private void ChangeColorLerp()
    {
        this.isNextColorByLerp = !this.isNextColorByLerp;

        if(this.isNextColorByLerp)
        {
            IncreaseColorValue();
        }
    }
    
    /// <summary>
    /// Изменение цвета игрока.
    /// </summary>
    private void ChangeColor()
    {
        IncreaseColorValue();
        this.isNextColorByLerp = false;
        this.playerRenderer.material.color = this.playerColors[colorChangeCount];
    }
    
    /// <summary>
    /// Изменение источника света.
    /// </summary>
    private void ChangeLightSource()
    {
        this.sun.enabled = !this.sun.enabled;
        this.centralLamp.enabled = !this.centralLamp.enabled;
    }
    
    /// <summary>
    /// Обработчик команды удаления другого мяча через 3 секунды после ввода команды.
    /// </summary>
    private void DestroyOtherBall()
    {
        Destroy(otherBall, 3f);    
    }
    #endregion

    #region Setters
    /// <summary>
    /// Инициализация компонентов.
    /// </summary>
    private void ComponentsInitialise()
    {
        this.playerColors = new Color[]
        {
            Color.black,
            Color.blue,
            Color.cyan,
            Color.gray,
            Color.green,
            Color.magenta,
            Color.red,
            Color.white,
            Color.yellow,
        };
        this.playerRigidbody = GetComponent<Rigidbody>();
        this.playerRenderer = GetComponent<Renderer>();
        this.count = 0;
        
        this.colorChangeCount = 0;
        this.originPlayerColor = this.playerRenderer.material.color;
        this.winTextObject.SetActive(false);
        this.centralLamp.enabled = false;
        this.isNextColorByLerp = false;
        this.isLightIntensityChangingMode = (false, false);
    }

    /// <summary>
    /// Увеличение счетика изменений цвета игрока.
    /// </summary>
    private void IncreaseColorValue()
    {
        this.colorChangeCount++;
        if(this.colorChangeCount >= this.playerColors.Length)
        {
            this.colorChangeCount = 0;
        }
    }

    /// <summary>
    /// Плавное изменение цвета объекта через метод Color.Lerp().
    /// </summary>
    /// <param name="objectRenderer">Компонент отрисовки объекта.</param>
    /// <param name="isLerpActivate">Флажок включенности функции Color.Lerp().</param>
    private void SetObjectMaterialColorByLerp(Renderer objectRenderer, bool isLerpActivate)
    {
        if(isLerpActivate)
        {
            this.playerRenderer.material.color = Color.Lerp(this.playerRenderer.material.color,
                                                            this.playerColors[colorChangeCount], 
                                                            0.5f * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Установка текста в UI.
    /// </summary>
    private void SetCountText()
    {
        this.countText.text = $"Count: {this.count}";
        if(this.count >= 12)
        {
            this.winTextObject.SetActive(true);
        }
    }

    /// <summary>
    /// Плавное изменение изтенсивности света.
    /// </summary>
    /// <param name="currentLight">Источник света.</param>
    /// <param name="isLightIntensityChange">Указатель включенности и направления изменения интенсивности света.</param>
    private void SetLightIntensityBySmoothDamp(Light currentLight, (bool, bool) isLightIntensityChange)
    {
        if(isLightIntensityChange.Item1)
        {
            var intensityVelocity = 0.0f;
            if(isLightIntensityChangingMode.Item2)
            {
                currentLight.intensity = Mathf.SmoothDamp(currentLight.intensity, this.maxLightIntensity, 
                                                          ref intensityVelocity, 0.3f);
            }
            else
            {
                currentLight.intensity = Mathf.SmoothDamp(currentLight.intensity, this.minLightIntensity, 
                                                          ref intensityVelocity, 0.3f);
            }
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Движения игрока.
    /// </summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        var control = context.control;
        var button = control as ButtonControl;
        
        if(button != null && button.isPressed)
        {
            this.moveDirection = context.ReadValue<Vector2>();
        }
        else
        {
            this.moveDirection.x = 0;
            this.moveDirection.y = 0;
        }
        //this.moveDirection = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Обработчик касания колайдера объекта с другим колайдером объекта.
    /// </summary>
    /// <param name="other">Колайдер другого объекта.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Проверка тега.
        if(other.gameObject.CompareTag("PickUp"))
        {
            // Отключение других игровых объектов c нужным тегом.
            other.gameObject.SetActive(false);
            this.count++;
            SetCountText();
        }
    }
    #endregion
}
