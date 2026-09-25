# Задание для локального агента: выгрузка данных стройки из Rust Dedicated Server

Ты работаешь на ПК пользователя, в папке установленного Rust Dedicated Server (Windows).
Цель — выгрузить в этот репозиторий всё, что нужно, чтобы вне Unity **в точности**
воспроизвести механику строительства и размещения предметов Rust.
Итог коммитишь в этот репозиторий (он приватный) и пушишь.

Главный принцип: **ничего не упрощай и не переписывай своими словами.** Нужны сырые
данные (все поля, все числа, все трансформы) и исходный код как есть. Лучше выгрузить
лишнее, чем потерять поле, от которого зависит край механики (волл-стаки, бункеры).

---

## Этап 0. Окружение

1. Узнай build id сервера (`steamapps/appmanifest_258550.acf` или `RustDedicated_Data/..`, либо команда `version` в консоли сервера). Все выгрузки кладутся в `data/<buildid>/`.
2. Поставь `ilspycmd` (`dotnet tool install -g ilspycmd`).
3. Поставь Oxide **или** Carbon на этот локальный сервер (только локально, для выгрузки).
   Сервер запускай с маленькой картой (`+server.worldsize 1000 +server.level "Procedural Map"`) и `+server.secure 0` — нам не нужен EAC.

## Этап 1. Декомпиляция кода → `ref/decompiled/`

```
ilspycmd -p -o ref/decompiled/Assembly-CSharp RustDedicated_Data/Managed/Assembly-CSharp.dll
```

Также декомпилируй (каждую в свою подпапку), если существуют:
`Assembly-CSharp-firstpass.dll`, `Rust.Data.dll`, `Rust.Global.dll`, `Rust.World.dll`,
`Facepunch.System.dll`, `Facepunch.UnityEngine.dll`, `Rust.Localization.dll`.
Unity*/System*/Mono* не нужны.

Не удаляй ничего из вывода. Проверь, что там есть как минимум эти типы
(если какого-то нет — найди, как он называется сейчас, и напиши в отчёте):

- `Construction`, `Construction.Target`, `Construction.Placement`, `ConstructionGrade`, `ConstructionSkin`, `ConstructionPlaceholder`
- `Planner`, `Deployer`, `ItemModDeployable`, `Deployable`
- `Socket_Base`, `Socket_Specific`, `Socket_Free`, `Socket_Terrain`, `ConstructionSocket`, `ConstructionSocket_*`, `NeighbourSocket`, `StabilitySocket`, `DecaySocket`, …
- все `SocketMod*` (`SocketMod_AreaCheck`, `SocketMod_SphereCheck`, `SocketMod_EntityCheck`, `SocketMod_BuildingBlock`, `SocketMod_TerrainCheck`, `SocketMod_InWater`, `SocketMod_WaterDepth`, `SocketMod_Attraction`, `SocketMod_HotSpot`, …)
- все `DeployVolume*` (`DeployVolumeOBB`, `DeployVolumeSphere`, `DeployVolumeCapsule`, `DeployVolumeEntityBounds`, `DeployVolumeEntityBoundsReverse`, …)
- `StabilityEntity`, `BuildingBlock`, `BuildingGrade`, `BuildingManager`, `BuildingPrivlidge`, `DecayEntity`, `Door`, `SimpleBuildingBlock`, `Wallpaper*`
- `PrefabAttribute`, `GameManager`, `GameManifest`, `ItemManager`, `ItemDefinition`, `Layers`, `ConVar.Stability`, `ConVar.Decay`, `ConVar.Server`
- `BasePlayer` (размеры капсулы игрока), `PlayerWalkMovement`/`AntiHack` (что есть на сервере)

Отдельно напиши в отчёте: какая логика выбора сокета при прицеливании
(поиск цели для превью: `Construction.*Target*`, `Socket_Base.TestTarget`, `Planner.*`)
присутствует в серверной сборке, а какой нет (она может быть только на клиенте).

## Этап 2. Runtime-дамп плагином → `data/<buildid>/`

Статический разбор бандлов не подходит: при загрузке сервер препроцессит префабы и
выносит `PrefabAttribute`-компоненты (сокеты, SocketMod, DeployVolume, Construction и т.п.)
из GameObject'ов в `PrefabAttribute.server`. Поэтому дамп делается **изнутри запущенного
сервера** плагином (Oxide/Carbon) с консольной командой, например `builddump`.
Код плагина положи в `extraction/plugin/`.

### 2.1. Универсальный рефлекшн-сериализатор (обязательно)

Напиши один сериализатор, который для любого объекта выгружает **все поля**
(public и `[SerializeField]` private, включая унаследованные) рекурсивно:
- примитивы, enum (как имя + число), строки;
- `Vector3/Quaternion/Bounds/Matrix4x4/LayerMask` — числами;
- массивы/списки — поэлементно;
- ссылки на `UnityEngine.Object` (GameObject/префаб/ItemDefinition/Material/PhysicMaterial/Mesh) — **не разворачивать**, а писать ссылку: `{ "$ref": тип, "name", "resourcePath"/"prefabID"/"itemid"/"meshId" }`;
- защита от циклов, глубина ≤ 8.

Всё ниже выгружается этим сериализатором, а не ручным перечислением полей —
чтобы не потерять ни одного флага.

### 2.2. Что выгрузить

**`items.json`** — каждый `ItemDefinition` из `ItemManager.itemList`:
itemid, shortname, displayName (англ.), category, все флаги, `steamDlc`/`steamItem`/`isRedirectOf`/`hidden` и прочие признаки платности/скрытости,
все `ItemMod*` (особенно `ItemModDeployable.entityPrefab`, `Planner`/`Deployer` на `heldEntity`),
стоимость/стек. Включи **все** предметы, не только размещаемые, — фильтровать буду я.

**`prefabs/<prefabID>.json`** — для каждого префаба, который может быть поставлен
(все `entityPrefab` из `ItemModDeployable`, все `Construction` из `PrefabAttribute.server`,
все блоки building plan, всё с компонентом `Deployable`/`StabilityEntity`/`DecayEntity`),
а также всё, что используется из `Socket_Specific.female/male`, placeholder'ы и т.п.:
- `path`, `prefabID`, цепочка классов корневой entity;
- **иерархия GameObject целиком**: имя, `layer` (номер+имя), tag, activeSelf, localPosition/localRotation/localScale, и дочерние;
- на каждом узле — все компоненты сериализатором; для коллайдеров обязательно:
  тип (Box/Sphere/Capsule/Mesh/Terrain/Wheel), center/size/radius/height/direction, `isTrigger`, `convex`, `enabled`, `sharedMesh` → ссылка на меш, physicMaterial;
- **все PrefabAttribute этого префаба** из `PrefabAttribute.server` (все типы: `Construction`, все `Socket_*`, `SocketMod*`, `DeployVolume*`, `ConstructionSkin`, `StabilitySocket`, `NeighbourSocket`, …) — каждый с его `worldPosition/worldRotation`/локальными трансформами и всеми полями;
- для `BuildingBlock`/`ConstructionGrade`: все грейды (twig/wood/stone/metal/hqm + скины грейдов), их префабы/меши, HP, стоимости.

**`meshes/<meshName>_<hash>.obj`** — каждый меш, на который ссылается `MeshCollider`
(вершины в локальных координатах меша, без трансформов). Если меш нечитаемый (`isReadable == false`) —
попробуй через `Mesh.AcquireReadOnlyMeshData`, если и так не выходит — запиши в отчёт список.
Отдельно проверь, есть ли на серверных префабах `MeshFilter`/`MeshRenderer` с визуальными мешами;
если есть — выгрузи и их в `meshes_visual/`, если нет — просто отметь это в отчёте.

**`layers.json`** — имена всех 32 слоёв и полная матрица `Physics.GetIgnoreLayerCollision(a,b)`;
плюс значения всех масок из статического класса `Layers` (`Layers.Mask.*`, `Layers.Construction`, …).

**`convars.json`** — все ConVar'ы (имя, значение по умолчанию, описание), в имени/категории которых есть
`stability`, `decay`, `build`, `construct`, `deploy`, `place`, `socket`, `upkeep`, `server.max`, `antihack`.

**`player.json`** — размеры и параметры капсулы `BasePlayer` (радиус, высоты стоя/присед, eye offset), дистанции взаимодействия, дистанция размещения из `Planner`/`Construction`.

**`manifest.json`** — buildid, дата, версия сервера, версия дампера, списки всего, что не удалось выгрузить, с причиной.

Все JSON — с отступами, числа без округления (`R` формат / `G9` для float).

### 2.3. Дополнение после разбора декомпила (обязательно)

По коду видно, что без этого механику 1:1 не собрать:

- **Скины грейдов.** Коллайдеры стройблоков живут не в самом префабе блока, а в префабе скина:
  `ConstructionGrade.skinObject` (→ `ConstructionSkin`). Выгрузи **каждый** такой префаб скина
  (все грейды, все скины грейдов, включая платные: адоб, контейнерный металл, кирпич и т.д.) так же, как 2.2 —
  полная иерархия, коллайдеры, меши, слои.
- **Условные модели.** `ConditionalModel` (+ `ConditionalModelWallpaper`) и все `ModelConditionTest_*` на скинах —
  выгрузи сериализатором все поля, и префабы, которые они спавнят (углы стен, ступеньки фундаментов,
  кромки крыш и т.п. — у них свои коллайдеры).
- **`BuildingProximity`** — все атрибуты этого типа (на нём держится запрет волл-стаков 1.49 м).
- **`ColliderInfo`** на каждом узле с коллайдером — поле `flags` (числом и списком имён флагов).
- **Теги.** Для каждого узла: `tag` (особенно `DeployVolumeIgnore`) и кастомные теги
  (`GameObjectEx.HasCustomTag` / `GameObjectTag` — все значения, какие есть на узле).
- **`EntityListScriptableObject`** из `DeployVolume.entityGroups` — развернуть в список prefabID/путей.
- **`BaseEntity.bounds`** корневой entity (из него строится OBB для `BuildingProximity`, `TestPlacingThroughRock` и т.п.).
- **`StabilityEntity.grounded`**, `BuildingBlock.blockDefinition` → путь Construction.
- `worldPosition`/`worldRotation`/`localPosition`/`localRotation` у каждого PrefabAttribute (это то, что использует код,
  а не transform GameObject'а).
- Для `Socket_Base`: `selectSize`, `selectCenter`, `socketName`, `checkOccupiedSockets`, и список `socketMods` с их полями.
- `layers.json`: плюс все числовые маски-литералы, которые встречаются в коде стройки — их я разберу сам,
  но нужны имена всех 32 слоёв.

### 2.4. Доработки дампера после первого прогона (обязательно)

1. **`items.json`: нет `ItemModDeployable`.** Это не `ItemMod`, а отдельный `MonoBehaviour` на GameObject'е
   предмета, поэтому в `itemMods` его нет. Добавь каждому предмету поле `itemModDeployable` — сериализацию
   `def.GetComponent<ItemModDeployable>()` (как минимум `entityPrefab` с `resourcePath`/`resourceID`, все остальные поля тоже).
   Заодно выгрузи в `extraComponents` все прочие компоненты с GameObject'а предмета, которых нет в `itemMods`.
2. **`$ref` с `$alreadyDumpedElsewhereInThisFile` должен нести `instanceID`.** Сейчас у таких ссылок `instanceID: 0`,
   потому что чтение `.name` падает раньше. `GetInstanceID()` в Unity берётся из managed-кэша и работает даже для
   уничтоженного native-объекта — вызывай его до и независимо от `.name`. Тогда любую ссылку можно разрешить
   по `instanceID` у развёрнутого объекта.
3. **Ссылки на `BaseEntity` (например `DeployVolume.entityList`) должны нести `prefabID` и `resourcePath`
   этого префаба**, а не только имя GameObject'а: игра сравнивает именно `prefabID`.
4. **Поправь `REPORT.md`:** «пустой слот 0» в `Construction.grades` не уничтожен. Это твиг-грейд, он развёрнут
   в поле `defaultGrade` того же файла, а в `grades[0]` лежит ссылка на него с тем же багом из п. 2.

После правок перезапусти `builddump` и замени `data/25454815/` целиком.

## Этап 3. Эталонные постройки (golden tests) → `data/<buildid>/golden/`

Добавь в плагин команду `basedump <radius>` для админа: выгружает все entity в радиусе от игрока
(prefabID, позиция, поворот, грейд/скин, родитель/сокет, к каким entity прикреплено,
**текущая стабильность** (`cachedStability` и связанные поля), HP, флаги).
Пользователь вручную построит на локальном сервере эталоны (волл-стаки, бункеры, стабилити-бункеры)
и сохранит их этой командой — по ним я проверю, что симулятор даёт ровно те же числа.
Также добавь `baseload <file>` для обратной загрузки такого файла на сервер (чтобы потом проверять
постройки из симулятора в реальной игре).

## Этап 4 (потом, по отдельной команде). Удалённые/старые предметы

Для предметов, которых нет в текущей сборке: через DepotDownloader скачать старые сборки
Rust Dedicated Server (app 258550, депот и manifest id — по истории на SteamDB),
прогнать тот же дампер и сложить в `data/<старый buildid>/`. Сначала составь список
отсутствующих предметов (сравни `items.json` разных сборок) и согласуй с пользователем, какие сборки качать.

## Визуал (не обязательно сейчас)

Код клиента обфусцирован (IL2CPP), но **ассеты клиента — обычные Unity-бандлы** (`Rust/Bundles`).
Иконки предметов и визуальные меши/текстуры можно вытащить из клиента AssetRipper/AssetStudio.
Не делай этого без запроса пользователя — объём большой.

## Ограничения по git

- Файлы > 90 МБ не коммитить (лимит GitHub 100 МБ); если такие получаются — разбей или сожми и напиши в отчёте.
- Если суммарно `data/` + `ref/` > 1 ГБ — остановись и спроси пользователя.
- Ветка: та, на которую указал пользователь. Сообщение коммита — что выгружено и из какого buildid.

## Отчёт

В конце создай `extraction/REPORT.md`: версия/buildid, что выгружено (количества: предметов, префабов,
мешей), что не удалось и почему, ответы на вопросы из этапа 1, любые странности.
