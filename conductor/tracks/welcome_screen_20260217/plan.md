# Implementation Plan - Welcome Screen Enhancement

ドックエリアの「ようこそ」画面をモダンに刷新し、最近使ったファイルやクイックアクションを提供します。

## Phase 1: Infrastructure & Application Layer (Recent Files Logic)
最近編集した画像を保存・追跡するための基盤を整備します。

- [ ] Task: 既存の `ISettingsRepository` に「最近使ったファイル」の保存・取得機能が含まれているか調査する
- [ ] Task: 最近使ったファイルを管理するための Value Object または DTO を `Eede.Domain` または `Eede.Application` に定義する
- [ ] Task: ファイル保存・読み込み成功時に「最近使ったファイル」を更新するロジックを `UseCase` または `ApplicationService` に追加する
- [ ] Task: 最近使ったファイルのリストを永続化する `Infrastructure` 層の実装（既存の設定ファイルへの追記など）を行う
- [ ] Task: Phase Completion Verification and Checkpointing Protocol

## Phase 2: ViewModel Implementation
「ようこそ」画面を表示するための ViewModel を実装します。

- [ ] Task: `WelcomeViewModel` を `Eede.Presentation.ViewModels` に新規作成する
    - [ ] 依存注入（DI）により `ISettingsRepository` や `IPictureIO` 等を受け取る
    - [ ] 最近使ったファイルのリストを `ObservableCollection` で公開する
    - [ ] 「新規作成」「開く」のコマンドを実装する
    - [ ] 外部リンク（GitHub）を開くコマンドを実装する
- [ ] Task: `App.axaml.cs` で `WelcomeViewModel` を DI コンテナに登録する
- [ ] Task: Phase Completion Verification and Checkpointing Protocol

## Phase 3: UI Design & Implementation (WelcomeView)
VS Code 風のモダンなデザインで UI を構築します。

- [ ] Task: `WelcomeView.axaml` を `Eede.Presentation.Views` に新規作成し、`WelcomeViewModel` と紐付ける
- [ ] Task: `WelcomeView` のレイアウトを実装する
    - [ ] 上部へのロゴ画像（またはロゴアイコン）の配置
    - [ ] セクション分け（最近使ったもの、開始、リンク、情報）
    - [ ] `PathIcon` を使用した視認性の高いアイコン配置
    - [ ] マウスオーバー時のハイライト効果（VS Code風）
- [ ] Task: `PictureFrame.axaml` の既存の `<TextBlock Text="説明など"/>` を `WelcomeView` に置き換える
- [ ] Task: 狭いドック幅でも崩れないよう、レスポンシブなスタイル（WrapPanelやScrollViewerの活用）を調整する
- [ ] Task: Phase Completion Verification and Checkpointing Protocol

## Phase 4: Verification & Polishing
- [ ] Task: 実際に画像を作成・保存し、「最近編集した画像」が更新されるか確認する
- [ ] Task: GitHub リンクが正常にブラウザで開くか確認する
- [ ] Task: デザインの微調整（余白、フォントサイズ、色味の統一）
- [ ] Task: Phase Completion Verification and Checkpointing Protocol
- [ ] Task: Conductor - User Manual Verification 'Welcome Screen Enhancement' (Protocol in workflow.md)
