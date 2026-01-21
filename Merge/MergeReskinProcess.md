# Merge 换皮流程整理（基于 Dog 主题提交）

> 目的：把“merge assets and addressable configs”提交里体现出来的换皮步骤，整理成可复用的清单，后续新增主题可按此对照。

## 1. 主题注册与活动入口

- **新增主题枚举值**：在 `MergeTheme` 中追加新主题编号，确保与策划表里的 `Theme` 对应。`Dog` 主题对应枚举值 `7`，用于数据表和运行时匹配。 
  【F:Merge/Scripts/Base/Merge/MergeTheme.cs†L1-L15】
- **新增主题活动类**：创建对应的 `MergeActivity_Theme`，定义 Addressable 组名与入口 Prefab，并在 `Initialize` 中加载主题专属数据表。Dog 主题对应 `MergeActivity_Dog`。 
  【F:Merge/Scripts/Base/Activity/MergeActivity_Dog.cs†L1-L47】

## 2. 数据表准备与活动周期配置

- **补齐主题数据表文件**：活动类里加载的表需要在 `Merge/Data` 下准备同名数据（例如 `PropData_Dog`、`MergeAdditionalOutput_Dog`、`MergeGenerateBubble_Dog` 等）。Dog 主题就是通过 `MergeActivity_Dog` 的 `LoadDataTable` 列表进行绑定。 
  【F:Merge/Scripts/Base/Activity/MergeActivity_Dog.cs†L18-L47】
- **配置活动周期**：在 `MergeScheduleData` 中增加活动周期记录，确保新主题能被调度并带上 `Theme`、`MaxPropId`、`TileId` 等字段。Dog 主题示例是第 20 期。 
  【F:Merge/Data/MergeScheduleData.txt†L1-L23】

## 3. Addressable 分组与资源声明

- **新增 Addressable 组**：为新主题建立独立的 Addressable 组（例如 `Merge_Dog`），把菜单、道具、特效、图集等资源集中管理。 
  【F:AddressableAssetsData/AssetGroups/Merge_Dog.asset†L1-L120】
- **同步 Schema 配置**：新组需要对应的 `BundledAssetGroupSchema` 与 `ContentUpdateGroupSchema`，保持与现有主题的打包/更新策略一致。 
  【F:AddressableAssetsData/AssetGroups/Schemas/Merge_Dog_BundledAssetGroupSchema.asset†L1-L12】
  【F:AddressableAssetsData/AssetGroups/Schemas/Merge_Dog_ContentUpdateGroupSchema.asset†L1-L12】
- **更新 Addressable 设置与分组顺序**：新增组后需同步 `AddressableAssetSettings` 与分组排序配置，保证新组出现在工程配置内（截图里这两项被修改）。 
  【F:AddressableAssetsData/AddressableAssetSettings.asset†L1-L20】
  【F:AddressableAssetsData/AddressableAssetGroupSortSettings.asset†L1-L20】

## 4. 主题资源与 Prefab 组织

- **主题 Prefab 集合**：按主题拆分 `Merge/Prefabs_Theme`，包含主界面、菜单、道具、特效等 Prefab。Dog 主题示例：`MergeMainMenu_Dog` 等资源都集中在 `Merge/Prefabs_Dog`。 
  【F:Merge/Prefabs_Dog/Menu/MergeMainMenu_Dog.prefab†L1-L6】
- **主题贴图与素材**：主题贴图放在 `Merge/Sprites_Theme`，供图集和 Prefab 引用。Dog 主题示例：`Merge/Sprites_Dog` 下的贴图资源。 
  【F:Merge/Sprites_Dog/NoAtlas/All.png.meta†L1-L8】
- **图集与贴图分组**：按功能拆分 `Merge/Sprites_Theme/Atlas` 与子目录（Menu/Prop/Game/Guide 等），并确保对应贴图与 `.meta` 跟随提交；Dog 主题示例在 `Merge/Sprites_Dog` 下按目录维护贴图资源。 
  【F:Merge/Sprites_Dog/Atlas.meta†L1-L8】
  【F:Merge/Sprites_Dog/Menu.meta†L1-L8】

## 5. 地图与入口 UI 资源

- **LevelTag 资源注册**：地图层的 Merge 入口标签需要加入 `MapUI` Addressable 组，确保活动入口可被加载。Dog 主题对应 `LevelTag_Merge_Dog`。 
  【F:AddressableAssetsData/AssetGroups/MapUI.asset†L80-L110】
- **入口 Prefab 落地**：地图入口 Prefab 本体需要落在对应路径（示例为 `LevelTag_Merge_Dog.prefab`），并被 Addressable 组引用。 
  【F:TileMatch/Res/Prefab/UI/Map/LevelPlayMenu/LevelTag_Merge_Dog.prefab†L1-L10】

## 6. 主题专属数据与玩家存档（如有）

- **主题进度/奖励存档**：如果主题有独立的气泡/装饰等进度，需要在 `PlayerDataComponent` 中新增持久化键值（如 Dog 的气泡奖励与装饰阶段）。 
  【F:Merge/Scripts/Base/PlayerData/PlayerDataComponent.cs†L470-L614】

## 7. 主题相关的全局配置/支持文件（可选但常见）

- **公共配置与事件类型**：换皮引入新流程时，可能需要同步流程类型或通用事件定义，确保玩法流程与 UI 能联动（截图里 ProcessType 与 CommonEventArgs 有变更记录）。 
  【F:GameMain/Scripts/Framework/Process/ProcessType.cs†L1-L40】
  【F:GameMain/Scripts/Runtime/Event/CommonEventArgs.cs†L1-L40】
- **全局 Merge 管理器与 Theme 映射**：如需扩展主题列表或数据表行为，关注 `MergeManager` 与 `MergeTheme` 的改动。 
  【F:Merge/Scripts/Base/MergeManager.cs†L1-L120】
  【F:Merge/Scripts/Base/Merge/MergeTheme.cs†L1-L15】

---

## 快速检查清单（执行顺序建议）

1. **创建主题枚举与活动类**（`MergeTheme` + `MergeActivity_Theme`）。
2. **准备主题数据表**，并在活动类里加载。
3. **新增 Addressable 组与 Schema**，声明所有 Prefab/图集/素材。
4. **放置 Prefab 与贴图资源**（`Merge/Prefabs_Theme`、`Merge/Sprites_Theme`）。
5. **补地图/入口 UI 的 Addressable 注册**（`LevelTag_Merge_Theme`）。
6. **补主题专属存档字段**（如装饰阶段、气泡奖励时间）。

以上步骤均可以 Dog 主题的实现作为模板进行替换与复用。
