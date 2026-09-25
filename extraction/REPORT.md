# Отчёт по выгрузке данных стройки Rust

## Версия / buildid

- `buildid` (Steam depot): **25454815** — из `rustds/steamapps/appmanifest_258550.acf`.
- Все выгрузки лежат в `data/25454815/`.

## Этап 1. Декомпиляция

Декомпилировано `ilspycmd 8.2.0.7535` в `ref/decompiled/<Assembly>/`:

| Сборка | Файлов |
|---|---|
| Assembly-CSharp | 728 |
| Assembly-CSharp-firstpass | 2313 |
| Rust.Data | 443 |
| Rust.Global | 79 |
| Rust.World | 7 |
| Facepunch.System | 16 |
| Facepunch.UnityEngine | 57 |
| Rust.Localization | 6 |

Ошибок декомпиляции нет (только предупреждение ilspycmd об устаревшей версии инструмента — не влияет на результат).

### Проверка обязательных типов

Все типы из чек-листа найдены, кроме одного:

- **`DecaySocket` отсутствует** в этой сборке — такого класса нет вообще (проверено `grep` по всем декомпилированным исходникам). Логика гниения (decay) в этой версии реализована не через PrefabAttribute-сокет, а напрямую в `DecayEntity`, `BuildingGradeDecay`, `DecayPoint`, `ConVar.Decay` — то есть механика есть, но не в виде отдельного `Socket_*`/`ConstructionSocket_*` типа, как предполагал чек-лист.
- Остальные типы (`Construction`, `Construction.Target`, `Construction.Placement`, `ConstructionGrade`, `ConstructionSkin`, `ConstructionPlaceholder`, `Planner`, `Deployer`, `ItemModDeployable`, `Deployable`, все `Socket_*`/`SocketMod_*`/`DeployVolume*`, `StabilityEntity`, `BuildingBlock`, `BuildingGrade`, `BuildingManager`, `BuildingPrivlidge`, `DecayEntity`, `Door`, `SimpleBuildingBlock`, `PrefabAttribute`, `GameManager`, `GameManifest`, `ItemManager`, `ItemDefinition`, `Layers`, `BasePlayer`, ConVar-классы) — присутствуют.

### Логика выбора цели/сокета при прицеливании — что есть на сервере, а что нет

Это оказалось важным и не вполне очевидным моментом:

**Полностью присутствует и является боевой (не заглушка) на сервере:**
`Construction.UpdatePlacement`, `Construction.FindMaleSockets`/`HasMaleSockets`, `Socket_Base.TestTarget`/`DoPlacement`/`CheckSocketMods`/`IsCompatible`/`CanConnect`, `Planner.DoBuild`/`DoPlacement`, `Planner.HasLineOfSight`, `DeployVolume.Check`, `BuildingProximity.Check`, вся цепочка `ConstructionErrors` (LOS, дистанция, deploy-volume, стабильность/proximity, привилегия постройки, дороги, глубокая вода и т.д.) — весь пайплайн валидации размещения полностью серверный и авторитетный. Именно эта логика определяет волл-стаки, бункеры, стоимость, грейд — то, что нужно симулятору.

**Не является полноценным поиском цели, а получает результат от клиента:**
Сервер **не** делает свой рейкаст по миру, чтобы определить "на какой сокет/энтити сейчас наведён игрок" для превью. Вместо этого клиент сам определяет кандидата (arbitraы raycast/UI на его стороне, кода этого в серверной сборке просто нет — он вырезан при компиляции под `UNITY_SERVER`) и присылает результат в RPC-сообщении `CreateBuilding`: `msg.entity` (netID кандидата), `msg.socket` (имя сокета через `StringPool` хэш), `msg.ray/position/normal/rotation`, флаги снаппинга. Сервер (`Planner.DoBuild(CreateBuilding msg)`):
1. находит реальную entity по сети (`BaseNetworkable.serverEntities.Find`),
2. ищет сокет **по имени** через `Planner.FindSocket(name, prefabID)` → `PrefabAttribute.server.FindAll<Socket_Base>(prefabID)` (это просто прямой поиск по имени в списке сокетов префаба — не геометрический поиск "ближайший сокет к точке прицеливания"),
3. трансформирует присланные клиентом локальные ray/position/normal/rotation в мировые координаты через **настоящий** transform найденной entity (то есть подменить сокет на произвольную мировую точку через клиент нельзя — координаты всегда привязаны к реальному transform entity на сервере),
4. и только после этого запускает полную серверную валидацию (`UpdatePlacement` → `FindMaleSockets` → `TestTarget`/`DoPlacement`/`CheckSocketMods`).

Вывод для симулятора: "провалидировать данное конкретное размещение (entity+сокет+грейд) и посчитать результат (успех/провал, стоимость, стабильность и т.п.)" — воспроизводимо полностью по серверному коду. А вот "автоматически определить, какой сокет клиент увидел бы под прицелом" — этой логики на сервере нет и её придётся реализовывать отдельно (эвристикой), она не нужна для проверки корректности уже заданного размещения, но нужна, если симулятор должен угадывать намерение игрока по одному лучу мыши.

## Этап 2/3. Runtime-дамп и golden-тесты

Плагин: `extraction/plugin/ConstructionDump.cs` (Oxide, команды `builddump`, `basedump <radius>`, `baseload <file>`).
Запускался на локальном сервере под отдельным identity `extraction` (маленькая процедурная карта,
`server.secure 0`), реальный мир пользователя (`my_serv`) не трогался.

### Итоговые числа (`data/25454815/manifest.json`)

- **1259** `ItemDefinition` в `items.json`.
- **3730** префабов в `prefabs/<prefabID>.json` (включая все скины грейдов и все префабы,
  которые спавнят `ConditionalModel`/`ConditionalModelWallpaper`, найденные BFS-обходом всех
  встретившихся `GameObjectRef`, а не только явно перечисленные в 2.2 категории).
- **2212** меша коллайдеров в `meshes/`, **180** визуальных мешей в `meshes_visual/`
  (визуальные меши на серверных префабах **есть** не только null-случай — 180 штук всё же
  осталось на некоторых серверных объектах).
- `layers.json` — все 32 имени слоя + полная матрица `Physics.GetIgnoreLayerCollision` + маски `Layers.Mask.*`.
- `convars.json` — **219** конваров по ключевым словам из задания (antihack 128, decay 57, server 20, stability 9, остальное — construct/ai/deepsea/render).
- `player.json` — капсула `BasePlayer` (радиус 0.5, высоты 1.8/1.1, eye offset 1.5) + дистанция размещения по умолчанию.
- `data/` + `ref/` суммарно ≈ 486 МБ (лимит 1 ГБ не превышен), самый большой файл ≈22 МБ (лимит 90 МБ не превышен).

### Раздел 2.3 (грейд-скины, условные модели и т.д.) — выполнено

Реализовано через один общий механизм: универсальный сериализатор обходит **любую** ссылку типа
`GameObjectRef`, встреченную где угодно (не только явно перечисленные поля), и докидывает найденный
prefabID в очередь на дамп. Это само по себе покрыло скины грейдов (`ConstructionGrade.skinObject`)
и префабы, которые спавнят `ConditionalModel`/`ModelConditionTest_*` (углы стен, ступеньки и т.п.) —
без отдельного кода под каждый случай. Проверено вручную на `foundation.prefab`: 13 реальных грейдов
(Wood/Stone×5/Metal×2/TopTier×2 — по количеству скинов) с HP, стоимостью, `physicMaterial`,
`damageProtecton` и корректно резолвящимся `skinObject` → отдельно продампленный `foundation.wood.prefab`
и т.д. `BuildingProximity`, `ColliderInfo.flags` (число + имена), теги узлов, `EntityListScriptableObject`,
`BaseEntity.bounds`, world/local-трансформы каждого `PrefabAttribute" — всё присутствует.

### Найденные и исправленные баги дампера (важно для доверия к данным)

1. **Циклическая раздутость через глобальные синглтоны.** `PrefabAttribute.gameManager` и
   `.prefabAttribute` — обратные ссылки на `GameManager` и весь реестр `PrefabAttribute.server`.
   Без явного исключения сериализатор разворачивал всю базу префабов заново из каждого атрибута.
2. **`ScriptableObject` как класс-мешок.** Слепое разворачивание любого `ScriptableObject` заодно
   инлайнило `SoundDefinition`/`AmbienceDefinition` (тысячи общих аудио-ассетов). Сужено до allowlist:
   `BuildingGrade`, `ProtectionProperties`, `EntityListScriptableObject`.
3. **Комбинаторный взрыв на графах.** Даже после (1)+(2) `cargoshiptest.prefab` (граф AI-путей
   `BasePathNode`, ~276k узлов) раздувался за счёт повторного разворачивания объектов, на которые
   ссылаются много раз. Исправлено множеством `ExpandedOnce` — каждый уникальный объект разворачивается
   один раз за файл, повторные ссылки становятся лёгким `$ref`.
   Вместе (1)+(2)+(3) уменьшили `data/` с нерабочих **4.8 ГБ / 27094 ошибок** (один файл — 816 МБ)
   до **442 МБ / 37 ошибок**.
4. **`UnityEngine.Object.name` на уничтоженном native-объекте кидает `NullReferenceException`**
   (а не даёт привычный Unity fake-null) — часть `PrefabAttribute`-компонентов на момент дампа
   уже уничтожены на нативной стороне (сервер вызывает `NominateForDeletion` при препроцессинге).
   Обычные managed-поля читаются нормально в любом случае; только чтение `.name` через try/catch
   → `"<destroyed native object>"`.
5. **Атрибуты конваров без суффикса `Attribute`.** Классы вроде `ServerVar`/`ReplicatedVar`
   (в недекомпилированной `Facepunch.Console.dll`) называются без суффикса `...Attribute` — проверка
   `EndsWith("VarAttribute")` не находила ни одного, `convars.json` был пуст. Исправлено на
   `EndsWith("Var")`; заодно оказалось, что `Help` — публичное **поле**, а не property.

### Известные, не исправляемые пробелы (ожидаемо, не баг)

- **29 UI-only префабов** (`ui.dialog.*`, `ui.map.*`, диалоги меню) — `GameManager.server.FindPrefab`
  возвращает null: у них нет серверного GameObject'а, это чисто клиентские префабы, ссылки на них
  встречаются только как записи в `ItemManager`/меню. Ожидаемо, отмечено в `manifest.json.failures`.
- **2 нечитаемых меша** (`jungle_ruins_c_root_HLOD_mesh`, `jungle_ruins_d_root_HLOD_mesh`) —
  `isReadable == false`, `Mesh.AcquireReadOnlyMeshData` тоже не сработал (HLOD-меши, судя по всему,
  собираются рантайм-запечёнными без CPU-читаемых данных). Список зафиксирован в `manifest.json`.
- **`player.json`**: реальная `CapsuleColliderInfo` игрока не снята (на момент дампа на сервере не
  было подключённого игрока) — задампана только статическая геймплейная капсула из констант.
  Если нужны именно рантайм-значения коллайдера — надо перезапускать `builddump`, когда игрок
  на сервере (я этого не делал, чтобы не тратить лишний прогон; скажи, если нужно).
- **Один "пустой" слот грейда на каждом `Construction`** (индекс 0 в `Construction.grades` —
  видимо зарезервированный/неиспользуемый слот под `BuildingGrade.Enum.None`) — это по-настоящему
  уничтоженная Unity-ссылка **в самой игре**, не артефакт дампера: `Construction.AttributeSetup`
  в реальном коде игры явно проверяет `if (!(constructionGrade == null))` при переборе `grades`,
  то есть игра сама трактует такой слот как "нет грейда с этим индексом" и пропускает его. В дампе
  это видно как `"$ref"` с `"$error": "Object reference not set to an instance of an object."` —
  корректно игнорировать этот элемент массива `grades`, как это делает сам симулятор Rust.

### Golden-тесты (Этап 3)

`basedump <radius>` и `baseload <file>` реализованы и прошли санити-чек через RCON (корректные ответы
без исключений на пустой карте — "0 сущностей" ожидаемо, построек ещё нет). Сбор реальных эталонов
(волл-стаки, бункеры, стабилити-бункеры) — ручной шаг: нужно зайти на локальный сервер (`server.identity
"extraction"`, `secure 0`), построить нужные конструкции и выполнить `basedump <radius>` рядом с ними;
файлы лягут в `data/25454815/golden/`. Этот шаг ещё не выполнялся.
