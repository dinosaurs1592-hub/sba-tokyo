# AGENTS.md

Claude Code がこのリポジトリで作業する際の運用ルール。

> **禁止事項は `~/.claude/settings.json` に、CI 検証項目は `.github/workflows/validate.yml` に集約している。本ファイルはプロセス設計のみを扱う。**

---

## ハーネス: Planner → Builder → Evaluator

エージェントは自分の成果を自画自賛しがち。独立した Evaluator を置くことで認知バイアスを排除する。

| 役割 | 担当 | 責務 |
|---|---|---|
| **Planner** | Claude(`EnterPlanMode`) | `docs/contracts/` に契約書作成、成功基準定義、President 承認 |
| **Builder** | Claude(メインエージェント) | 契約スコープ厳守、PR 作成 |
| **Evaluator** | Claude(サブエージェント) | 成功基準を ○/✗ 判定、未検証は「未確認」と正直に報告 |

---

## チーム編成(モデル別役割)

| モデル | 役割 | 使い所 |
|---|---|---|
| **Opus**(司令塔) | Planner / 重要 Builder / 最終判断 | 設計、複雑な実装、コードレビュー |
| **Sonnet** | 標準 Builder | Task サブエージェントで通常実装・リファクタ |
| **Gemini Flash** | 調査 / 要約 | アセット検索、ログ要約、`ccr code` 経由 |
| **DeepSeek**(将来) | Evaluator / think 系 | 独立検証、MCP 経由で追加予定 |

---

## 実装契約書

`docs/contracts/YYYY-MM-DD-{タスク名}.md` に保存。

テンプレートと共通成功基準は [`docs/contracts/README.md`](docs/contracts/README.md) を参照。

必須項目:目的 / 変更するファイル / 変更しないファイル / 成功基準(Evaluator チェックリスト)/ リスク。

---

## 提案ラベル

コード以外の提案には必ず付与:

- `Optional` — なくても動く改善
- `Recommended` — 品質・保守性の向上
- `Urgent` — バグ・仕様違反・前方互換の破壊

---

## GitHub ワークフロー

```
main(保護)
  └── feat/{機能名}   fix/{バグ名}   docs/{ドキュメント名}
```

1. 作業はブランチを切る(`main` 直 push は settings.json で拒否される)
2. コミットは Conventional Commits(`feat:` / `fix:` / `docs:` / `wip:`)
3. PR 作成 → Evaluator チェックリストを本文に添付
4. **マージは Claude が自動実行**:CI green を確認 → `gh pr merge --auto --squash --delete-branch`
5. President から「マージ待って」の明示があった場合のみ待機

---

## ドキュメントマップ

| ファイル | 役割 |
|---|---|
| `AGENTS.md` | 本ファイル(プロセス) |
| `CLAUDE.md` | 技術ガイド(アーキテクチャ・入力・チューニング) |
| `~/.claude/settings.json` | 禁止事項・許可コマンド |
| `.github/workflows/validate.yml` | CI 検証項目 |
| `docs/specs/` | ゲーム仕様 |
| `docs/plans/` | ロードマップ |
| `docs/contracts/` | 実装契約書 |
| `docs/reports/` | QA レポート |
