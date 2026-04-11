# SBA Tokyo Prototype GDD

> ステータス: **フェーズ1完了**（M1〜M4 実装済み）

## 目的

3レーンランナーの最小構成でコアループの体感を検証する。

---

## 実装済み機能

### 当初スコープ内

- 自動前進移動
- 3レーン切り替え（横スワイプ / A・D キー）
- 下スワイプ プッシュ入力（S キー）
- スピード減衰（1秒放置後、毎秒 -2.0）
- 障害物衝突 → ゲームオーバー
- 距離・スピード HUD
- `PlayerState` による状態管理
- `PrototypeSurfaceType` による面の型定義

### 当初スコープ外だったが実装済み（M4 で先行実装）

以下はフェーズ2の基盤として意図的に実装された。

| 機能 | 実装クラス | 備考 |
|---|---|---|
| ジャンプ | `PrototypeRunnerController` | `jumpVelocity = 8f` |
| 着地ウィンドウ判定 | `PrototypeGameManager.RegisterLandingResult()` | 0.15s / Perfect 0.08s |
| アクションバッファ | `PrototypeRunnerController.BufferLandingAction()` | 先行入力を保持 |
| コンボ・スコア | `PrototypeGameManager` | 通常 +100 / Perfect +200 |
| レールグラインド | `UpdateSurfaceState()` | レール上で速度減衰停止 |
| レールトリック | `PrototypeRunnerController.RailTrick()` | +150 点 |
| 面別着地ルール | `RegisterLandingResult()` の switch 式 | Ground / Object / Rail |
| Perfect 着地速度ボーナス | `PrototypeGameManager` | +1.5f |

---

## チューニングベースライン

| パラメータ | 値 | Inspector |
|---|---|---|
| 開始スピード | 8.0 | `startSpeed` |
| 最高スピード | 20.0 | `maxSpeed` |
| プッシュ量 | 2.0 | `pushAmount` |
| 減衰開始遅延 | 1.0 s | `idleDecayDelay` |
| 減衰量/秒 | 2.0 | `speedDecayPerSecond` |
| 着地ウィンドウ | 0.15 s | `landingWindowDuration` |
| Perfect ウィンドウ | 0.08 s | `perfectLandingWindow` |
| Perfect 速度ボーナス | 1.5 | `speedBonus` |
| レールトリックスコア | 150 | `railTrickScore` |
| レーン幅 | 3.0 | `laneWidth` |

---

## フェーズ1 検証基準

プレイテストで確認すべき体感目標：

- [ ] 速度圧力を即座に理解できる（プッシュしないと死ぬ感覚）
- [ ] レーン切り替えが直感的に機能する
- [ ] 障害物回避に反射的な判断が生まれる
- [ ] 着地ウィンドウの存在にプレイヤーが気づける（現状 HUD のみ）

> ⚠️ 着地ウィンドウの視覚フィードバックが不足しており、現状では4番目の基準を満たしにくい。フェーズ2の最優先タスク。

---

## 前方互換の契約

以下は **変更・削除禁止**：

- `PlayerState` の既存値（`Running`, `Airborne`, `Grinding`, `LandingWindow`, `GameOver`）
- `PrototypeSurfaceType` の既存値（`Ground`, `Object`, `Rail`, `Obstacle`）
- `PrototypeLandingActionType` の既存値（`None`, `Tap`, `Swipe`, `Jump`, `RailTrick`）
