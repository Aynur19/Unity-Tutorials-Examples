using UnityEngine;

/// <summary>
/// Поведение снаряда
/// </summary>
public class ProjectileController : MonoBehaviour
{
    /// <summary>
    /// Начальный метод снаряда.
    /// </summary>
    private void Start()
    {
        // Уничтожение снаряда через 5 секунд.
        Destroy(this.gameObject, 5f);
    }

}
