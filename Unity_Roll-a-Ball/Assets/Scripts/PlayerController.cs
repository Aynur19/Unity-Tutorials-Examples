using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Контроллер игрока.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Private Serialize Fields
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
    [SerializeField] private float minLightIntensity;
    
    /// <summary>
    /// Максимальная интенсивность света.
    /// </summary>
    [SerializeField] private float maxLightIntensity;
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
    /// Движение игрока по X.
    /// </summary>
    private float movementX;
    
    /// <summary>
    /// Движение игрока по Y.
    /// </summary>
    private float movementY;

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
    #endregion

    #region Player Life Cycle
    /// <summary>
    /// Метод пробуждения игрового объекта (компонента).
    /// Вызывается перед методом Start() даже если скрипт выключен.
    /// </summary>
    private void Awake()
    {
        //Debug.Log("Awake called.");
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
        var movement = new Vector3(movementX,0.0f,movementY);
        
        playerRigidbody.AddForce(movement * speed);
    }
    
    /// <summary>
    /// Метод обновления кадра.
    /// Вызывается перед самым выводом следующего фрейма.
    /// </summary>
    private void Update()
    {
        // Плавное изменение цвета игрока.
        SetObjectMaterialColorByLerp(playerRenderer, isNextColorByLerp);

        // Плавное изменение интенсивности солнечного света.
        SetLightIntensityBySmoothDamp(sun, isLightIntensityChangingMode);
    }
    #endregion

    #region Setters
    /// <summary>
    /// Инициализация компонентов.
    /// </summary>
    private void ComponentsInitialise()
    {
        playerColors = new Color[]
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
        playerRigidbody = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        count = 0;
        
        colorChangeCount = 0;
        originPlayerColor = playerRenderer.material.color;
        winTextObject.SetActive(false);
        centralLamp.enabled = false;
        isNextColorByLerp = false;
        isLightIntensityChangingMode = (false, false);
    }

    /// <summary>
    /// Увеличение счетика изменений цвета игрока.
    /// </summary>
    private void IncreaseColorValue()
    {
        colorChangeCount++;
        if(colorChangeCount >= playerColors.Length)
        {
            colorChangeCount = 0;
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
            playerRenderer.material.color = Color.Lerp(playerRenderer.material.color,
                                                       playerColors[colorChangeCount], 0.5f * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Установка текста в UI.
    /// </summary>
    private void SetCountText()
    {
        countText.text = $"Count: {count}";

        if(count >= 12)
        {
            winTextObject.SetActive(true);
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
            float intensityVelocity = 0.0f;
            if(isLightIntensityChangingMode.Item2)
            {
                currentLight.intensity = Mathf.SmoothDamp(currentLight.intensity, maxLightIntensity, 
                                                          ref intensityVelocity, 0.3f);
            }
            else
            {
                currentLight.intensity = Mathf.SmoothDamp(currentLight.intensity, minLightIntensity, 
                                                          ref intensityVelocity, 0.3f);
            }
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Движения игрока.
    /// </summary>
    /// <param name="movementValue">Ввод пользователя. Клавиши 'WSDA'.</param>
    private void OnMove(InputValue movementValue)
    {
        var movementVector = movementValue.Get<Vector2>();

        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    /// <summary>
    /// Плавное изменение интенсивности света - Directional Light через метод Math.SmoothDamp().
    /// </summary>
    /// <param name="inputValue">Ввод пользователя. Клавиша 'I'.</param>
    private void OnChangeDirectionalLightIntensity(InputValue inputValue)
    {
        isLightIntensityChangingMode.Item1 = !isLightIntensityChangingMode.Item1;

        if(isLightIntensityChangingMode.Item1)
        {
            isLightIntensityChangingMode.Item2 = !isLightIntensityChangingMode.Item2;
        }
    }

    /// <summary>
    /// Плавное изменение цвета игрока
    /// </summary>
    /// <param name="colorValue">Ввод пользователя. Клавиша 'J'.</param>
    private void OnChangeColorLerp(InputValue colorValue)
    {
        isNextColorByLerp = !isNextColorByLerp;

        if(isNextColorByLerp)
        {
            IncreaseColorValue();
        }
    }

    /// <summary>
    /// Изменение цвета игрока.
    /// </summary>
    /// <param name="colorValue">Ввод пользователя. Клавиша 'R'.</param>
    private void OnChangeColor(InputValue colorValue)
    {
        IncreaseColorValue();
        isNextColorByLerp = false;
        playerRenderer.material.color = playerColors[colorChangeCount];
    }

    /// <summary>
    /// Изменение источника света.
    /// </summary>
    /// <param name="lightSourceValue">Ввод пользователя. Клавиша 'N'.</param>
    private void OnChangeLightSource(InputValue lightSourceValue)
    {
        sun.enabled = !sun.enabled;
        centralLamp.enabled = !centralLamp.enabled;
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
            count++;
            SetCountText();
        }
    }

    /// <summary>
    /// Обработчик команды удаления другого мяча через 3 секунды после ввода команды.
    /// </summary>
    /// <param name="inputValue">Ввод пользователя. Клавиша 'Backspace'.</param>
    private void OnDestroyOtherBall(InputValue inputValue)
    {
        Destroy(otherBall, 3f);    
    }
    #endregion
}
