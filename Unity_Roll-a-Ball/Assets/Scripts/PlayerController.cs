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

    private Color[] playerColors;

    private bool isNextColorByLerp;
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
    void Update()
    {
        if(isNextColorByLerp)
        {
            playerRenderer.material.color = Color.Lerp(playerRenderer.material.color,
                                                       playerColors[colorChangeCount], 0.5f * Time.deltaTime);
        }
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
    }

    private void IncreaseColorValue()
    {
        colorChangeCount++;
        if(colorChangeCount >= playerColors.Length)
        {
            colorChangeCount = 0;
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
    #endregion

    #region Event Handlers
    /// <summary>
    /// Движения игрока.
    /// </summary>
    /// <param name="movementValue">Значение ввода.</param>
    private void OnMove(InputValue movementValue)
    {
        var movementVector = movementValue.Get<Vector2>();

        movementX = movementVector.x;
        movementY = movementVector.y;
    }

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
    /// <param name="colorValue"></param>
    private void OnChangeColor(InputValue colorValue)
    {
        IncreaseColorValue();

        playerRenderer.material.color = playerColors[colorChangeCount];
    }

    /// <summary>
    /// Изменение источника света.
    /// </summary>
    /// <param name="lightSourceValue"></param>
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
    #endregion
}
