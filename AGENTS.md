# AGENTS.md

Claude Code がこのリポジトリで作業する際の運用ルールです。

## ハーネス設計

Anthropic の公式研究に基づく **Planner → Builder → Evaluator** パターンを採用します。

エージェントは自分の成果を自画自賛しがちです。独立した Evaluator を設けることで、この認知バイアスを排除します。

---

## シングルエージェント vs マルチエージェント

**シングルエージェントで対応する（デフォルト）：**
- 1〜2ファイルの修正
- 調査・説明・提案のみ

**マルチエージェント（サブエージェント）を使う：**
- 複数ファイルにまたがる実装
- 大規模な調査（Explore サブエージェントに委譲してメインコンテキストを保護）
- 独立した検証が必要な場合

マルチエージェントはトークンを 3〜10 倍消費します。必要な時だけ使います。

---

## 役割定義

### Planner（計画フェーズ）

**担当：** Claude Code（`EnterPlanMode` を使用）

- 作業を開始する前に `docs/contracts/` に実装契約書を作成する
- 変更するファイルと変更しないファイルを明示する
- Evaluator が使う **測定可能な成功基準** を定義する
- President（ユーザー）の承認を得てから Builder フェーズに移行する

### Builder（実装フェーズ）

**担当：** Claude Code（メインエージェント）

- 契約書のスコープを厳守する。スコープ外は実装しない
- `SerializeField` の値をコードにハードコードしない（Inspector で管理）
- 前方互換の契約を守る（後述）
- 完了後、Evaluator チェックリストを添付して PR を作成する

### Evaluator（検証フェーズ）

**担当：** Claude Code（サブエージェントまたは明示的なレビューステップ）

- 「良さそう」という主観的判断をしない
- 契約書の各成功基準を **○ / ✗** で判定する
- 未検証の項目は正直に「未確認」と報告する
- 合格しない基準があれば Builder に差し戻す

---

## 実装契約書フォーマット

`docs/contracts/YYYY-MM-DD-{タスク名}.md` に保存。

```markdown
## 目的

## 変更するファイル
- `Assets/Scripts/...`

## 変更しないファイル
- `Assets/Scripts/Core/PlayerState.cs`（前方互換の契約）

## 成功基準（Evaluator チェックリスト）
- [ ] ビルドエラーなし
- [ ] 既存の SerializeField 値が変わっていない
- [ ] PlayerState / PrototypeSurfaceType の既存値を変更していない
- [ ] （タスク固有の基準）

## リスク・懸念点
```

---

## 提案ラベル

コード以外の変更提案には必ずラベルをつける：

- `Optional` — なくても動く改善
- `Recommended` — 品質・保守性の向上
- `Urgent` — バグ・仕様違反・前方互換の破壊

---

## 前方互換の契約

以下は **変更・削除禁止**。フルゲームへの拡張の土台です。

- `PlayerState` enum の既存値（`Running`, `Airborne`, `Grinding`, `LandingWindow`, `GameOver`）
- `PrototypeSurfaceType` enum の既存値（`Ground`, `Object`, `Rail`, `Obstacle`）

---

## GitHub ワークフロー

```
main（保護ブランチ）
  └── feat/{機能名}
  └── fix/{バグ名}
  └── docs/{ドキュメント名}
```

1. 作業は必ずブランチを切って行う（`main` への直接 push 禁止）
2. 実装完了後に PR を作成し、Evaluator チェックリストを PR 本文に添付する
3. コミットは Conventional Commits 形式（`feat:`, `fix:`, `docs:` 等）
4. PR は President（ユーザー）がマージする

---

## ドキュメントマップ

| ファイル | 役割 |
|---|---|
| `AGENTS.md` | 運用ルール（本ファイル） |
| `CLAUDE.md` | Claude Code 向け技術ガイド |
| `docs/specs/` | ゲーム仕様・要件 |
| `docs/plans/` | マイルストーン計画 |
| `docs/contracts/` | 実装契約書（Planner が作成） |
| `docs/reports/` | QA レポート（Evaluator が作成） |
