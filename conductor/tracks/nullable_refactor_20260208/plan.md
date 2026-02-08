# Implementation Plan - Nullable Disable and Behavioral Refactoring

## Phase 1: Preparation and Characterization Testing (描画・操作系) [checkpoint: 61b132a]
現在の挙動を保護するためのテストを作成します。

- [x] Task: 描画ユースケース (`DrawActionUseCase`) の振る舞いテスト作成 [72c3fa2]
    - [x] 既存のペン・図形描画の挙動を固定するテストコードの実装
    - [x] テストの実行と全パスの確認
- [x] Task: キャンバス操作 (`TransferImageToCanvasUseCase`, `TransformImageUseCase`) の振る舞いテスト作成 [8fc04ad]
    - [x] 画像の転送（Push/Pull）および変形操作の挙動を固定するテストコードの実装
    - [x] テストの実行と全パスの確認
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md) [61b132a]

## Phase 2: Characterization Testing (入出力・編集系) [checkpoint: a062f29]
残りの主要なユースケースをテストで保護します。

- [x] Task: ファイル入出力 (`LoadPictureUseCase`, `SavePictureUseCase`) の振る舞いテスト作成 [fe29fac]
    - [x] 画像の読み込み・保存処理の挙動を固定するテストコードの実装
- [x] Task: 選択・クリップボード (`CopySelectionUseCase`, `PasteFromClipboardUseCase`) の振る舞いテスト作成 [25ce88d]
    - [x] 選択範囲のコピー・ペースト挙動を固定するテストコードの実装
- [x] Task: 全テストの実行と現時点でのステータス確認 [25ce88d]
- [x] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md) [a062f29]

## Phase 3: Project Configuration Update
テストによる保護が完了した後、Nullable 設定を変更します。

- [ ] Task: 全 csproj ファイル（Tests以外）の Nullable 設定を disable に更新
    - [ ] `Eede.Domain`, `Eede.Application`, `Eede.Infrastructure`, `Eede.Presentation`, `Eede.Presentation.Desktop` の各プロジェクトファイルを編集
- [ ] Task: ビルドおよび全テストの再実行
    - [ ] Nullable 無効化によってコンパイルエラーやテスト失敗が発生しないことを確認
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)

## Phase 4: Final Verification and Cleanup
- [ ] Task: 全プロジェクトのクリーンビルドと全テスト実行の最終確認
- [ ] Task: Conductor - User Manual Verification 'Phase 4' (Protocol in workflow.md)
