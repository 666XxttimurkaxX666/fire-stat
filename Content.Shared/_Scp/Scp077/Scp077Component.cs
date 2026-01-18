using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Scp.Scp077;

/// <summary>
/// Компонент для SCP-077 (Череп из проклятой гробницы).
/// Требует чтения ритуала в унисон двумя игроками.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class Scp077Component : Component
{
    /// <summary>
    /// Текущее оставшееся время до выброса газа (в секундах).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float CurrentTimer;

    /// <summary>
    /// Интервал таймера по умолчанию (1200 секунд = 20 минут).
    /// </summary>
    [DataField("timerInterval")]
    public float TimerInterval = 1200f;

    /// <summary>
    /// Активная фраза ритуала, которую должны произнести игроки.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public string ActivePhrase = "";

    /// <summary>
    /// Таймер для контроля длительности синего свечения глаз.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float BlueGlowTimer = 0f;

    /// <summary>
    /// Словарь для отслеживания попыток ритуала. 
    /// Хранит EntityUid игрока и время, когда он произнес фразу.
    /// </summary>
    public Dictionary<EntityUid, TimeSpan> RitualAttempts = new();

    /// <summary>
    /// Список доступных имен рун для генерации фразы.
    /// </summary>
    [DataField("runeNames")]
    public List<string> RuneNames = new() 
    { 
        "Луун", "Ареш", "Гхал", "Гхап", "Лрр", "Щзац", "Шзац", "Орх", "Иммо" 
    };

    /// <summary>
    /// Список символов, соответствующих именам рун (для Examine).
    /// </summary>
    [DataField("runeSymbols")]
    public List<string> RuneSymbols = new() 
    { 
        "0-0", "VvV", "X_X", "~W~", "*Z*", "Y=Y", "S+S", "<M>", "L&L" 
    };

    /// <summary>
    /// Урон, наносимый игроку при ошибке в ритуале.
    /// </summary>
    [DataField("failDamage")]
    public float FailDamage = 50f;
}