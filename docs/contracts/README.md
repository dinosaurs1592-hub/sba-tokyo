# 実装契約書

Planner フェーズで作成する実装契約書を保存するディレクトリ。

## 命名規則

```
YYYY-MM-DD-{タスク名}.md
```

例: `2026-04-11-jump-audio-feedback.md`

## 役割

- **Planner** が作成し、President の承認を得る
- **Builder** は契約書のスコープを厳守して実装する
- **Evaluator** は成功基準チェックリストを使って検証する

## テンプレート

```markdown
## 目的

<!-- 何のためにこの変更を行うか。1〜3文 -->

## 変更するファイル

- `Assets/Scripts/...`

## 変更しないファイル

- `Assets/Scripts/Core/PlayerState.cs`(前方互換の契約 — CI で検証)

## 成功基準(Evaluator チェックリスト)

- [ ] ビルドエラーなし
- [ ] タスク固有の成功基準 1
- [ ] タスク固有の成功基準 2

## リスク・懸念点

<!-- 実装前に見えている落とし穴 -->
```

## 共通の成功基準(自動検証)

`.github/workflows/validate.yml` が以下を自動でチェックするため、契約書に書く必要なし:

- `PlayerState` enum の既存値保持
- `PrototypeSurfaceType` enum の既存値保持
- `SerializeField` ハードコード検出
- 必須ドキュメント (`CLAUDE.md`, `AGENTS.md`, `docs/specs/prototype_gdd.md`, `docs/plans/prototype_plan.md`) の存在
