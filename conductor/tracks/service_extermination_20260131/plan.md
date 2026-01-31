# 実装計画: Serviceクラスの撲滅とドメイン知識の返却

## 概要
`AnimationService`、`ICanvasBackgroundService`、およびインフラ系サービスの解体と再配置を行い、DDDに基づくクリーンなアーキテクチャを実現します。

## フェーズ 1: Safety Net & Infrastructure Ports (準備と基盤整備)
リファクタリングの安全性を確保するためのテスト整備と、インフラ系サービスの整理を行います。

- [x] Task: 現状の `AnimationService` と `CanvasBackgroundService` の挙動を固定する仕様化テストの作成 [72b1a2a]
- [x] Task: `Eede.Application/Infrastructure` ディレクトリの作成と `IClipboard`, `IFileStorage` への名称変更・移動
- [x] Task: `IClipboard` 等をラップする UseCase（`CopyImageUseCase` 等）の作成と ViewModel からの呼び出し置換
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md) [checkpoint: b6b1ee7]

## フェーズ 2: Animation Service の解体 (ドメイン知識の返却)
`AnimationService` をドメイン集約と UseCase に分割します。

- [x] Task: ドメイン層への `AnimationPatterns` 集約（ファーストクラスコレクション）の導入とテスト [checkpoint: b6b1ee7]
- [x] Task: `AddAnimationPatternUseCase`, `RemoveAnimationPatternUseCase` 等の実装
- [x] Task: ViewModel で保持していた `IAnimationService` を UseCase と新しい状態管理に差し替え
- [x] Task: `AnimationService` クラスの削除
- [ ] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## フェーズ 3: Canvas Background の整理 (Humble ViewModel 化)
描画ロジックを View へ移動し、設定を Value Object 化します。

- [ ] Task: ドメイン層への `CanvasBackgroundStyle` Value Object の導入
- [ ] Task: `BackgroundCalculator` の抽出とユニットテスト
- [ ] Task: Presentation 層（View/Painter）への背景描画ロジックの移動
- [ ] Task: `ICanvasBackgroundService` の廃止と Application 層からの `System.Drawing` 依存の削除
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)

## フェーズ 4: 仕上げとクリーンアップ
残ったサービスの整理と、ディレクトリ構造の最終調整を行います。

- [ ] Task: `Eede.Application/Services` ディレクトリの完全削除
- [ ] Task: 全プロジェクトのビルド確認とユニットテストの実行、カバレッジ確認
- [ ] Task: Conductor - User Manual Verification 'Phase 4' (Protocol in workflow.md)
