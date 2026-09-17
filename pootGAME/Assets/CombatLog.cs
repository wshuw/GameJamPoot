using UnityEngine;

// Централизованный лог боевой системы. Пока нет моделек/анимаций — это
// единственный способ понять, что вообще происходит в бою. Всё идёт через
// Debug.Log с префиксом [Combat] и именем актёра — в консоли Unity можно
// вбить в поиск "[Combat]" (все бои) или имя конкретного объекта (только он).
public static class CombatLog
{
    private const bool Enabled = true; // один выключатель на всё, если станет слишком шумно

    // Переход состояния конечного автомата (Idle -> Windup и т.п.)
    public static void State(string actor, string from, string to, string reason = null)
    {
        if (!Enabled) return;
        string reasonPart = string.IsNullOrEmpty(reason) ? "" : $"  [{reason}]";
        Debug.Log($"[Combat][{actor}] СОСТОЯНИЕ: {from} -> {to}{reasonPart}");
    }

    // Решения ИИ (бросок кубика на защиту и т.п.)
    public static void Decision(string actor, string message)
    {
        if (!Enabled) return;
        Debug.Log($"[Combat][{actor}] РЕШЕНИЕ: {message}");
    }

    // Нанесённый/полученный урон по ХП
    public static void Damage(string actor, string message)
    {
        if (!Enabled) return;
        Debug.Log($"[Combat][{actor}] УРОН: {message}");
    }

    // Изменения стамины
    public static void Stamina(string actor, string message)
    {
        if (!Enabled) return;
        Debug.Log($"[Combat][{actor}] СТАМИНА: {message}");
    }

    // Всё остальное (восприятие, патруль, хитбоксы, ввод)
    public static void Info(string actor, string message)
    {
        if (!Enabled) return;
        Debug.Log($"[Combat][{actor}] {message}");
    }
}