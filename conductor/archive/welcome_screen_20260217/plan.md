# Implementation Plan - Welcome Screen Enhancement

ドックエリアの「ようこそ」画面をモダンに刷新し、最近使ったファイルやクイックアクションを提供します。

## Phase 1: Infrastructure & Application Layer (Recent Files Logic) [checkpoint: 3421446]
最近編集した画像を保存・追跡するための基盤を整備します。

- [x] Task: 既存の `ISettingsRepository` に「最近使ったファイル」の保存・取得機能が含まれているか調査する
- [x] Task: 最近使ったファイルを管理するための Value Object または DTO を `Eede.Domain` または `Eede.Application` に定義する
- [x] Task: ファイル保存・読み込み成功時に「最近使ったファイル」を更新するロジックを `UseCase` または `ApplicationService` に追加する
- [x] Task: 最近使ったファイルのリストを永続化する `Infrastructure` 層の実装（既存の設定ファイルへの追記など）を行う
- [x] Task: Phase Completion Verification and Checkpointing Protocol

## Phase 2: ViewModel Implementation [checkpoint: 8033639]
「ようこそ」画面を表示するための ViewModel を実装します。

- [x] Task: `WelcomeViewModel` を `Eede.Presentation.ViewModels` に新規作成する
    - [x] 依存注入（DI）により `ISettingsRepository` や `IPictureIO` 等を受け取る
    - [x] 最近使ったファイルのリストを `ObservableCollection` で公開する
    - [x] 「新規作成」「開く」のコマンドを実装する
    - [x] 外部リンク（GitHub）を開くコマンドを実装する
- [x] Task: `App.axaml.cs` で `WelcomeViewModel` を DI コンテナに登録する
- [x] Task: Phase Completion Verification and Checkpointing Protocol

## Phase 3: UI Design & Implementation (WelcomeView) [checkpoint: 38279f6]
VS Code 風のモダンなデザインで UI を構築します。

- [x] Task: `WelcomeView.axaml` を `Eede.Presentation.Views` に新規作成し、`WelcomeViewModel` と紐付ける
- [x] Task: `WelcomeView` のレイアウトを実装する
    - [x] 上部へのロゴ画像（またはロゴアイコン）の配置
    - [x] セクション分け（最近使ったもの、開始、リンク、情報）
    - [x] `PathIcon` を使用した視認性の高いアイコン配置
    - [x] マウスオーバー時のハイライト効果（VS Code風）
- [x] Task: `PictureFrame.axaml` の既存の `<TextBlock Text="説明など"/>` を `WelcomeView` に置き換える
- [x] Task: 狭いドック幅でも崩れないよう、レスポンシブなスタイル（WrapPanelやScrollViewerの活用）を調整する
- [x] Task: Phase Completion Verification and Checkpointing Protocol

## Phase 4: Verification & Polishing [checkpoint: 617288f]
- [x] Task: 実際に画像を作成・保存し、「最近編集した画像」が更新されるか確認する
- [x] Task: GitHub リンクが正常にブラウザで開くか確認する
- [x] Task: デザインの微調整（余白、フォントサイズ、色味の統一）
- [x] Task: Phase Completion Verification and Checkpointing Protocol
- [x] Task: Conductor - User Manual Verification 'Welcome Screen Enhancement' (Protocol in workflow.md)

## Phase: Review Fixes
- [x] Task: Apply review suggestions (asynchronous OpenUrlCommand) [6f6e491]
