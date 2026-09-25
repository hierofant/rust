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

_(в процессе — дописывается по мере готовности)_
