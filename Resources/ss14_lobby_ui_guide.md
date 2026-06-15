# SS14 Lobby UI — Полное руководство для разработчика форка

> Документация для разработчиков форков на базе **Corvax / Space Station 14**  
> Актуально для upstream SS14 + Corvax специфики

---

## 1. UI-фреймворк: что используется в SS14

SS14 работает на собственном движке **RobustToolbox** (написан на C#). UI-система в нём построена на **XAML**, но не стандартном WPF/Avalonia — это кастомная реализация поверх движка.

### Как это устроено технически

- **XamlIL** — компилятор, преобразующий `.xaml` файлы в IL-код (.NET) прямо во время сборки. Он позаимствован из Avalonia UI, но адаптирован под Robust Toolbox.
- **Стиль** задаётся через **StyleNano** (`Content.Client/Stylesheets/StyleNano.cs`) — это C#-класс, который программно определяет правила оформления контролов (аналог CSS, но в коде).
- **Namespaces в XAML**: стандартный `xmlns="https://spacestation14.io"` для движковых типов; кастомные namespace подключаются через `xmlns:foo="clr-namespace:..."`.

```xml
<Control xmlns="https://spacestation14.io"
         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
         xmlns:lobby="clr-namespace:Content.Client.Lobby.UI">
    <!-- контролы -->
</Control>
```

### Иерархия файлов для каждого UI-элемента

Каждый UI-компонент состоит из **двух файлов** (паттерн code-behind):

| Файл | Назначение |
|------|-----------|
| `SomethingGui.xaml` | Декларативная разметка: лэйаут, контролы, свойства |
| `SomethingGui.xaml.cs` | C#-логика: обработчики событий, связь с данными |

---

## 2. Файлы, отвечающие за UI лобби

### Клиентская сторона (Content.Client)

```
Content.Client/
├── Lobby/
│   ├── LobbyState.cs                  ← Главный контроллер состояния лобби
│   └── UI/
│       ├── LobbyGui.xaml              ← XAML-разметка лобби
│       ├── LobbyGui.xaml.cs           ← Code-behind для LobbyGui
│       └── LobbyCharacterPreviewPanel ← Панель предпросмотра персонажа
│
└── GameTicking/
    └── Managers/
        ├── ClientGameTicker.cs        ← Получает события от сервера, уведомляет UI
        └── TitleWindowManager.cs      ← Управляет заголовком окна
```

### Серверная сторона (Content.Server)

```
Content.Server/
└── GameTicking/
    ├── GameTicker.cs                  ← Основной Game Ticker
    ├── GameTicker.Lobby.cs            ← Логика лобби (отправка событий клиентам)
    ├── GameTicker.RoundFlow.cs        ← Управление раундами
    └── GameTicker.Player.cs           ← Управление игроками в лобби
```

### Shared (Content.Shared)

```
Content.Shared/
├── CCVar/
│   └── CCVars.cs                      ← Console Variables (настройки лобби)
│
└── GameTicking/
    ├── SharedGameTicker.cs            ← Сетевые события (TickerLobbyStatusEvent и др.)
    └── Prototypes/
        └── LobbyBackgroundPrototype.cs ← Прототип для фоновых изображений
```

### Ресурсы (Resources)

```
Resources/
├── Prototypes/
│   └── Corvax/
│       └── lobbyscreens.yml           ← YAML-прототипы фоновых экранов лобби
│
├── Textures/
│   └── Corvax/
│       └── LobbyScreens/
│           ├── *.png                  ← Сами изображения
│           └── attributions.yml       ← Атрибуция авторов арта
│
└── Locale/
    ├── en-US/lobby/
    │   ├── lobby-gui.ftl              ← Строки UI лобби (английский)
    │   └── lobby-state.ftl            ← Строки состояния лобби
    └── ru-RU/lobby/
        └── ...                        ← Русская локализация (в Corvax)
```

---

## 3. Как изменить или добавить элементы UI лобби

### Шаг 1: Редактирование XAML-разметки

Основной файл — `Content.Client/Lobby/UI/LobbyGui.xaml`. Структура лобби по умолчанию:

```xml
<Control xmlns="https://spacestation14.io"
         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 1. Фоновое изображение -->
    <TextureRect Name="Background"
                 Stretch="Scale"
                 HorizontalExpand="true"
                 VerticalExpand="true" />

    <!-- 2. Верхняя панель: кнопки (Options, Admin Help и т.д.) -->
    <HBoxContainer>
        <Button Name="OptionsButton" />
        <Button Name="LeaveButton" />
    </HBoxContainer>

    <!-- 3. Информация о сервере -->
    <VBoxContainer>
        <Label Name="ServerName" />
        <RichTextLabel Name="InfoBlob" />  <!-- ID раунда, игроки, карта -->
    </VBoxContainer>

    <!-- 4. Персонаж + кнопки действий -->
    <lobby:LobbyCharacterPreviewPanel Name="CharacterPreview" />
    <Button Name="ReadyButton" />
    <Button Name="ObserveButton" />

</Control>
```

**Как добавить новый элемент:**

```xml
<!-- Пример: добавить кастомный баннер вашего сервера -->
<PanelContainer StyleClasses="PanelDark">
    <VBoxContainer>
        <TextureRect Texture="/Textures/YourServer/logo.png"
                     MinSize="200 80"
                     Stretch="KeepAspectCentered" />
        <Label Text="Добро пожаловать на MyServer!" 
               HorizontalAlignment="Center"
               StyleClasses="LabelHeading" />
    </VBoxContainer>
</PanelContainer>
```

### Шаг 2: Подключение логики в Code-Behind

В `LobbyGui.xaml.cs` добавьте ссылки на новые контролы:

```csharp
public sealed partial class LobbyGui : Control
{
    // Ссылки на контролы по имени из XAML (Name="...")
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public Button ReadyButton { get; private set; } = default!;
    public Button ObserveButton { get; private set; } = default!;
    
    // Ваш новый контрол
    public Label ServerBannerLabel { get; private set; } = default!;

    public LobbyGui()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        
        // Получение ссылок на контролы по имени
        ReadyButton = GetChild<Button>("ReadyButton");
        ObserveButton = GetChild<Button>("ObserveButton");
        ServerBannerLabel = GetChild<Label>("ServerBannerLabel");
    }
}
```

### Шаг 3: Управление состоянием из LobbyState.cs

`LobbyState.cs` управляет жизненным циклом лобби и реагирует на события:

```csharp
public sealed class LobbyState : Robust.Client.State.State
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private LobbyGui? _lobby;

    protected override void Startup()
    {
        _lobby = new LobbyGui();
        _uiManager.StateRoot.AddChild(_lobby);

        // Подписка на нажатие кнопки
        _lobby.ReadyButton.OnPressed += OnReadyPressed;
        _lobby.ObserveButton.OnPressed += OnObservePressed;

        // Подписка на сетевые события
        SubscribeNetworkEvent<TickerLobbyStatusEvent>(OnLobbyStatus);
        SubscribeNetworkEvent<TickerLobbyCountdownEvent>(OnLobbyCountdown);
    }

    protected override void Shutdown()
    {
        _lobby?.Dispose();
    }
    
    // Реакция на обновление статуса раунда от сервера
    private void OnLobbyStatus(TickerLobbyStatusEvent ev)
    {
        if (_lobby == null) return;
        
        // Обновить фон лобби
        _lobby.Background.Texture = /* загрузить текстуру из ev.LobbyBackground */;
        
        // Обновить кнопку Ready
        _lobby.ReadyButton.Text = ev.IsGameStarted 
            ? Loc.GetString("lobby-state-join") 
            : Loc.GetString("lobby-state-ready");
    }
}
```

---

## 4. Связь UI с игровым кодом: C# и YAML

### Сетевые события (Server → Client)

Связь между сервером и UI происходит через **NetMessages / NetworkEvents**. Определены в `Content.Shared/GameTicking/SharedGameTicker.cs`:

| Событие | Что передаёт | Кто обрабатывает |
|---------|-------------|-----------------|
| `TickerLobbyStatusEvent` | Статус раунда, ID фона, ready-статус | `ClientGameTicker.cs` |
| `TickerLobbyInfoEvent` | Строка с инфо (игроки, карта, ID раунда) | `ClientGameTicker.cs` |
| `TickerLobbyCountdownEvent` | Время до старта, пауза | `ClientGameTicker.cs` |
| `TickerJobsAvailableEvent` | Доступные слоты профессий | `ClientGameTicker.cs` |

**Диаграмма потока данных:**

```
[GameTicker.Lobby.cs]         [ClientGameTicker.cs]        [LobbyState.cs]
      (Server)                      (Client)                   (UI)
         │                              │                         │
         │──TickerLobbyStatusEvent──►   │                         │
         │                              │──OnLobbyStatusUpdate──► │
         │                              │                         │──обновить LobbyGui
         │                              │                         │
         │──TickerLobbyCountdownEvent──►│                         │
         │                              │──OnLobbyCountdown──────►│
         │                              │                         │──обновить таймер
```

### Как сервер отправляет события

В `GameTicker.Lobby.cs` на сервере:

```csharp
private void SendStatusToAll()
{
    var ev = new TickerLobbyStatusEvent(
        IsGameStarted,
        _lobbyBackground,  // ID прототипа фона из YAML
        _roundStartTime,
        _readyPlayerList
    );
    RaiseNetworkEvent(ev); // отправить всем клиентам
}
```

### Как работают YAML-прототипы лобби

Фоновые экраны лобби задаются через YAML-прототипы (`LobbyBackgroundPrototype`):

```yaml
# Resources/Prototypes/Corvax/lobbyscreens.yml

- type: lobbyBackground
  id: MyServerBackground1
  background: /Textures/MyServer/lobby_bg_1.png   # путь к текстуре
  # Необязательные поля (специфично для Corvax):
  author: "Имя художника"
  title: "Название работы"
```

Сервер случайно выбирает один из зарегистрированных прототипов и передаёт его ID клиентам через `TickerLobbyStatusEvent`. Клиент загружает текстуру и устанавливает её как фон.

### CCVars, влияющие на лобби

Из `Content.Shared/CCVar/CCVars.cs`:

```csharp
// Включить/выключить лобби
public static readonly CVarDef<bool> GameLobbyEnabled = 
    CVarDef.Create("game.lobbyenabled", false, CVar.SERVER);

// Время ожидания в лобби (в секундах)
public static readonly CVarDef<int> GameLobbyDuration = 
    CVarDef.Create("game.lobbyduration", 20, CVar.SERVER);

// Название сервера (реплицируется на клиент)
public static readonly CVarDef<string> ServerLobbyName = 
    CVarDef.Create("server.name", "MyServer", CVar.REPLICATED);
```

---

## 5. Особенности при кардинальном изменении лобби

### 5.1 Стилизация: StyleNano и как её расширить

Стили для UI контролов задаются в `Content.Client/Stylesheets/StyleNano.cs`. Это не CSS-файл, а C#-код. Чтобы добавить стиль для вашего компонента:

```csharp
// В StyleNano.cs или в вашем собственном stylesheet-файле
private static StyleRule MyServerBannerRule()
{
    return new StyleRule(
        new SelectorElement(typeof(PanelContainer), 
                            new[] { "MyServerBanner" }, null, null),
        new[]
        {
            new StyleProperty(PanelContainer.StylePropertyPanel, 
                new StyleBoxFlat 
                { 
                    BackgroundColor = Color.FromHex("#1a1a2e"),
                    BorderColor = Color.FromHex("#e94560"),
                    BorderThickness = new Thickness(2)
                })
        }
    );
}
```

Затем применить стиль в XAML:

```xml
<PanelContainer StyleClasses="MyServerBanner">
    ...
</PanelContainer>
```

### 5.2 Обязательные имена контролов

`LobbyState.cs` обращается к контролам по **именам из XAML**. Если вы переименуете или удалите контрол — нужно обновить все обращения к нему в `LobbyState.cs` и `LobbyGui.xaml.cs`. Критичные контролы:

| Name в XAML | Используется в |
|-------------|---------------|
| `ReadyButton` | `LobbyState.cs` — обработчик нажатия |
| `ObserveButton` | `LobbyState.cs` — обработчик нажатия |
| `InfoBlob` | `LobbyState.cs` — вывод серверной информации |
| `ServerName` | `LobbyState.cs` — название сервера |
| `Background` | `LobbyState.cs` — установка фонового изображения |
| `CharacterPreview` | `LobbyState.cs` — предпросмотр персонажа |

### 5.3 Локализация строк (Fluent)

Все тексты в UI должны идти через систему локализации **Fluent** (`.ftl` файлы), а не хардкодиться:

```csharp
// В C# коде:
_lobby.ReadyButton.Text = Loc.GetString("lobby-ready-button");

// В файле Resources/Locale/ru-RU/lobby/lobby-gui.ftl:
lobby-ready-button = Готов
```

### 5.4 Структура для Corvax-форка

В Corvax принято держать собственный контент в отдельных папках с префиксом `Corvax` или именем вашего форка:

```
Content.Client/
└── YourFork/
    └── Lobby/
        └── UI/
            ├── YourLobbyExtension.xaml       ← доп. элементы лобби
            └── YourLobbyExtension.xaml.cs

Resources/
├── Prototypes/
│   └── YourFork/
│       └── lobbyscreens.yml                  ← ваши фоны
└── Textures/
    └── YourFork/
        └── LobbyScreens/
            └── *.png
```

### 5.5 Chat UI в лобби

Чат в лобби инициализируется через `ChatUIController`, а не в самом `LobbyGui`. Его нельзя просто убрать из XAML — нужно позаботиться о корректном `Startup`/`Shutdown` в `LobbyState.cs`:

```csharp
// LobbyState.cs
private ChatUIController _chatController = default!;

protected override void Startup()
{
    // Чат инициализируется отдельно от LobbyGui
    _chatController = UserInterfaceManager.GetUIController<ChatUIController>();
    _chatController.SetMainChat(true);
    // ...
}

protected override void Shutdown()
{
    _chatController.SetMainChat(false);
    // ...
}
```

### 5.6 Фоновая музыка лобби

Музыка управляется через `ContentAudioSystem.LobbySoundtrackChanged`. `LobbyState.cs` подписывается на это событие, чтобы отобразить название трека в UI:

```csharp
SubscribeLocalEvent<LobbySoundtrackChangedEvent>(OnSoundtrackChanged);

private void OnSoundtrackChanged(LobbySoundtrackChangedEvent ev)
{
    // Обновить label с названием трека, если есть такой элемент в вашем UI
    if (_lobby?.TrackLabel != null)
        _lobby.TrackLabel.Text = ev.TrackTitle;
}
```

---

## Быстрый чеклист для полного рефизайна лобби

- [ ] Отредактировать `LobbyGui.xaml` — новая разметка
- [ ] Обновить `LobbyGui.xaml.cs` — ссылки на все новые контролы
- [ ] Проверить `LobbyState.cs` — все обращения к именованным контролам
- [ ] Добавить стили в `StyleNano.cs` (или отдельный stylesheet)
- [ ] Добавить локализацию в `Resources/Locale/ru-RU/lobby/lobby-gui.ftl`
- [ ] Добавить YAML-прототипы фонов в `Resources/Prototypes/YourFork/lobbyscreens.yml`
- [ ] Положить текстуры в `Resources/Textures/YourFork/LobbyScreens/`
- [ ] Не сломать инициализацию Chat UI в `LobbyState.Startup()`
- [ ] Не сломать обработчики `TickerLobbyStatusEvent` и `TickerLobbyCountdownEvent`

---

## Полезные ссылки

- [Документация по UI — docs.spacestation14.com](https://docs.spacestation14.com/en/robust-toolbox/user-interface.html)
- [UI Survival Guide](https://docs.spacestation14.com/en/ss14-by-example/ui-survival-guide.html)
- [DeepWiki: Lobby UI and Audio (Corvax)](https://deepwiki.com/space-syndicate/space-station-14/12.2-lobby-ui-and-audio)
- [DeepWiki: Corvax Lobby Screens](https://deepwiki.com/space-syndicate/space-station-14/3.3-corvax-content-and-lobby-screens)
