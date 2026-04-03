using UnityEngine;

/// <summary>
/// Интерфейс для вражеского ИИ
/// Определяет базовые методы для всех типов вражеского поведения
/// </summary>
public interface IEnemyAI
{
    // Сброс состояния ИИ к начальным настройкам
    void ResetAI();

    // Флаг обнаружения игрока врагом
    bool CanSeePlayer { get; }
}