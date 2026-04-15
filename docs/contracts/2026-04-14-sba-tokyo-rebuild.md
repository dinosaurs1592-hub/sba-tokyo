# 設計契約書: SBA Tokyo 本格実装（再構築）

**日付:** 2026-04-14  
**ステータス:** 承認済み

---

## 目的

プロトタイプで検証したコアメカニクスを土台に、`Prototype` prefix を排除した  
クリーンなアーキテクチャで SBA Tokyo を再構築する。

プロトタイプから引き継ぐもの：
- CharacterController ベースの走行・レーン移動・ジャンプ
- サーフェス判定（Ground / Rail / Object / Obstacle）
- ランディングウィンドウ・コンボシステム
- チャンク式レベル生成の概念

---

## 変更方針

### プロトタイプとの決定的な違い

| 問題 | プロトタイプ | 本実装 |
|---|---|---|
| チャンクプレハブ | シーンオブジェクトを Instantiate | `PrefabUtility.SaveAsPrefabAsset()` で実アセット作成 |
| クラス名 | `PrototypeXxx` | プレフィックスなし |
| 名前空間 | `SBATokyo.Prototype.*` | `SBATokyo.Core` / `SBATokyo.Gameplay` / `SBATokyo.UI` |
| シーン | `RunnerPrototype.unity` | `GameScene.unity` |
| シーンビルダー | `PrototypeSceneBuilder` | `SBASceneBuilder` |

---

## ファイル一覧

### 新規作成

| ファイル | 旧対応 |
|---|---|
| `Assets/Scripts/Core/SBAGameManager.cs` | PrototypeGameManager |
| `Assets/Scripts/Core/SBAInputManager.cs` | PrototypeInputManager |
| `Assets/Scripts/Core/PlayerState.cs` | 同名（内容変更なし） |
| `Assets/Scripts/Gameplay/RunnerController.cs` | PrototypeRunnerController |
| `Assets/Scripts/Gameplay/CameraFollow.cs` | PrototypeCameraFollow |
| `Assets/Scripts/Gameplay/SurfaceType.cs` | PrototypeSurfaceType |
| `Assets/Scripts/Gameplay/LandingActionType.cs` | PrototypeLandingActionType |
| `Assets/Scripts/Gameplay/RoadSurface.cs` | PrototypeSurface |
| `Assets/Scripts/Gameplay/Obstacle.cs` | PrototypeObstacle |
| `Assets/Scripts/Gameplay/ChunkSpawner.cs` | 同名（内容変更なし） |
| `Assets/Scripts/Gameplay/ChunkController.cs` | 同名（内容変更なし） |
| `Assets/Scripts/UI/GameHUD.cs` | PrototypeHUD |
| `Assets/Editor/SBASceneBuilder.cs` | PrototypeSceneBuilder（完全書き直し） |

### アーカイブ（削除しない）

既存の `Assets/Scripts/` 以下の `Prototype*.cs` は  
`Assets/Scripts/Archive/` に移動する。

---

## SBASceneBuilder の設計

```
[MenuItem("SBA Tokyo/Build Game Scene")]

1. EnsureFolder("Assets/Prefabs/Chunks")
2. CreateChunkPrefabs() → PrefabUtility.SaveAsPrefabAsset() で 5 種保存
3. シーン新規作成 ("GameScene")
4. GameManager / InputManager / Runner / Camera / HUD を配置
5. ChunkSpawner.chunkPrefabs に Assets/Prefabs/Chunks/*.prefab を代入
6. シーン保存
```

ChunkSpawner は `.prefab` アセットを参照するため、  
Play モードで Instantiate しても正しく複製される。

---

## チャンク仕様（プロトタイプと同一）

| 名前 | 内容 |
|---|---|
| ChunkFlat | 障害物なし |
| ChunkObstacleRun | 3 レーン障害物 |
| ChunkRailSection | 中央レール（Grinding） |
| ChunkObjectPlatform | 段差プラットフォーム |
| ChunkMixed | レール＋両サイド障害物 |

チャンク長: 36f、プールサイズ: 各 3

---

## 成功基準

- [ ] `Build Game Scene` でシーンが生成される
- [ ] `Assets/Prefabs/Chunks/` に 5 種の `.prefab` が保存される
- [ ] Play 開始直後からチャンクが正常にスクロールし続ける
- [ ] 障害物・レール・プラットフォームが機能する
- [ ] `Prototype` prefix のスクリプトが本番コードに含まれない
