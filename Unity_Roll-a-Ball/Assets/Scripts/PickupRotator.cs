using UnityEngine;

/// <summary>
/// Класс вращения кубика.
/// </summary>
public class PickupRotator : MonoBehaviour
{
    /// <summary>
    /// Обновление объекта игры перед выводом нового кадра.
    /// </summary>
    private void Update()
    {
        // Поворот данного объекта на указанный вектор.
        transform.Rotate(new Vector3(15,30,45) * Time.deltaTime);        
    }
}
