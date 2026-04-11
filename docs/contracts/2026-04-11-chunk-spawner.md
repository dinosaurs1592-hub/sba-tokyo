# 実装契約書: チャンク式レベル生成（M8）

**日付:** 2026-04-11
**担当:** Builder
**ステータス:** 承認待ち

---

## 目的

`ObstacleSpawner` を廃止し、`ChunkSpawner` に置き換える。
チャンクは事前設計されたパターンを持つプレハブ単位で管理され、
レール・オブジェクト面・障害物パターンをゲームデザイン意図通りに配置できるようにする。

---

## 変更するファイル

| ファイル | 変更内容 |
|---|---|
| `Assets/Scripts/Gameplay/ChunkSpawner.cs` | **新規作成** |
| `Assets/Scripts/Gameplay/ChunkController.cs` | **新規作成**（チャンク個別の初期化・リセット） |
| `Assets/Scripts/Gameplay/ObstacleSpawner.cs` | 削除（ChunkSpawner に完全置き換え） |
| `Assets/Editor/PrototypeSceneBuilder.cs` | ObstacleSpawner 参照を ChunkSpawner に更新 |

---

## 変更しないファイル

- `Assets/Scripts/Core/PrototypeGameManager.cs`（`RunReset` イベントはそのまま流用）
- `Assets/Scripts/Core/PlayerState.cs`
- `Assets/Scripts/Gameplay/PrototypeSurfaceType.cs`
- `Assets/Scripts/Gameplay/PrototypeLandingActionType.cs`
- `Assets/Scripts/Gameplay/PrototypeRunnerController.cs`
- `Assets/Scripts/Gameplay/PrototypeSurface.cs`
- `Assets/Scripts/Gameplay/PrototypeObstacle.cs`（チャンク内で引き続き使用）

---

## 設計方針

### ChunkSpawner

```
[SerializeField] GameObject[] chunkPrefabs   // チャンクプレハブ一覧
[SerializeField] int poolSizePerChunk = 3    // チャンクごとのプール数
[SerializeField] float chunkLength = 36f     // チャンク1個の Z 長さ
[SerializeField] float spawnDistanceAhead    // プレイヤー前方の生成距離
[SerializeField] PrototypeGameManager gameManager
[SerializeField] Transform runner

- Start() でプールを生成
- Update() でプレイヤー Z 位置に応じてチャンクを繰り出し・回収
- gameManager.RunReset を購読してリセット
- gameManager.DifficultyLevel（M7 で追加予定）を参照して難度別チャンク選択（M7 まではランダム）
```

### ChunkController

```
- チャンク内の障害物・サーフェスオブジェクトを管理
- Activate(float startZ) で位置を設定して有効化
- Deactivate() で無効化・プールに返却
```

### チャンクプレハブ（5種、Unity Editor で作成）

| プレハブ名 | 内容 | Z長 |
|---|---|---|
| `ChunkFlat` | 障害物なし、Ground のみ | 36 |
| `ChunkObstacleRun` | 3レーンに障害物パターン（1〜2個） | 36 |
| `ChunkRailSection` | 中央レーンにレール面（PrototypeSurface = Rail） | 36 |
| `ChunkObjectPlatform` | 中央に段差プラットフォーム（PrototypeSurface = Object） | 36 |
| `ChunkMixed` | レール + 両サイド障害物 | 36 |

---

## 成功基準（Evaluator チェックリスト）

- [ ] プレイヤーが進むとチャンクが途切れなく繋がる
- [ ] `RunReset` でチャンクが全て回収・リセットされる
- [ ] `ChunkRailSection` でプレイヤーが `Grinding` 状態になる
- [ ] `ChunkObjectPlatform` のプラットフォームにジャンプで乗れる
- [ ] `PrototypeGameManager`・`PrototypeRunnerController` を変更していない
- [ ] `PlayerState` / `PrototypeSurfaceType` の既存値を変更していない
- [ ] `ObstacleSpawner` が残っていない（削除済み）

---

## リスク・懸念点

- チャンクプレハブは Unity Editor で手動作成が必要（スクリプトでは自動生成不可）
- `PrototypeSceneBuilder.cs` のシーン再生成コマンドを使えば Editor 上で確認可能
- チャンク長 36f は `spacing = 12f`（旧 ObstacleSpawner）の3倍。調整が必要な場合は Inspector で変更
