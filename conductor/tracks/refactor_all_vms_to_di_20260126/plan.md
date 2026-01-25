# Implementation Plan: 全 ViewModel の DI 移行と DrawingSession 管理の洗練

MainViewModel からサブ ViewModel の生成知識を排除し、IDrawingSessionProvider による状態共有モデルへと移行します。

## 専門家のアドバイス (Legacy Code Refactoring Edition)
- **接合部 (Seam) の作成**: MainViewModel に「サブ ViewModel を受け取るコンストラクタ」を追加し、既存の初期化ロジックと共存させながら段階的に移行する。
- **依存関係の反転 (DIP)**: サブ ViewModel 群を直接注入する前に、まず DrawingSession の「所有権」を ViewModel から Provider へ安全に移動させる。
- **仕様化テストの維持**: 移行中、常に MainViewModel の仕様化テストをパスさせ続け、壊れた瞬間を即座に特定する。

## Phase 0: 追加の仕様化テストと接合部の準備 [checkpoint: f6bbb08]
変更前の挙動をより厳密に固定し、リファクタリング用の「入り口」を作ります。

- [x] Task: サブ ViewModel 間の状態同期（アニメーション ⇔ キャンバス等）を網羅するテストを追加
- [x] Task: `MainViewModel` に、サブ ViewModel 群を引数に取る「オーバーロードされたコンストラクタ」を作成 (接合部の作成)
- [x] Task: 既存のテストが、この新しいコンストラクタ経由でもパスすることを確認
- [x] Task: Conductor - User Manual Verification 'Phase 0' (Protocol in workflow.md)

## Phase 1: IDrawingSessionProvider による状態の「外部化」 [checkpoint: 3198d2b]
アプリの心臓部である `DrawingSession` を、ViewModel の外へ安全に逃がします。

- [x] Task: `IDrawingSessionProvider` 実装。初期化時に「デフォルトのセッション」を生成する責務を持たせる
- [x] Task: `DrawingSessionViewModel` を、Provider を監視して自身のプロパティを更新する「薄い View 用アダプタ」に変更
- [x] Task: `MainViewModel` で保持していたセッション管理ロジックを Provider へ委譲
- [x] Task: テストを実行し、Undo/Redo の挙動が破壊されていないことを確認
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md)

## Phase 2: サブ ViewModel の逐次的な DI 移行
一気に行わず、1つずつ ViewModel を App.axaml.cs での管理に移行します。

- [ ] Task: PaletteContainerViewModel を DI 化し、MainViewModel への注入に切り替える
- [ ] Task: AnimationViewModel を DI 化（ファイルシステム等の依存も整理）
- [ ] Task: DrawableCanvasViewModel を DI 化（これが最難関のため最後に実施）
- [ ] Task: 各ステップで MainViewModel の仕様化テストを実行し、デグレを防止
- [ ] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## Phase 3: 接合部のクリーンアップと DI コンテナの完全統合
仮設のコンストラクタを削除し、設計図（DI登録）を完成させます。

- [ ] Task: MainViewModel の古い（引数の少ない）コンストラクタを削除
- [ ] Task: App.axaml.cs で全ての ViewModel 登録を AddTransient / AddSingleton に整理
- [ ] Task: テストプロジェクトも DI を用いた解決に統一し、保守性を向上
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)
