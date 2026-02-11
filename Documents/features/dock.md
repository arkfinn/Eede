# Feature: ドッキングレイアウト (Docking Layout)

## 1. Overview
`Dock.Avalonia` ライブラリを使用して、マルチウィンドウおよびマルチタブ形式の柔軟な UI レイアウトを実現します。ユーザーは自由にドックエリアを分割・移動・結合することができ、作業効率を最大化できます。

## 2. Data Model

### 2.1 Dockable ViewModel
- **DockPictureViewModel**: 各画像タブに対応する ViewModel。`IDockable` を通じて `Dock.Avalonia` に管理されます。
- **AnimationDockViewModel**: アニメーションプレビューおよびシーケンサーを保持する Tool ウィンドウ。

### 2.2 InjectableDockFactory
- `IFactory` を継承したカスタムファクトリ。
- DI コンテナから ViewModel を生成し、ドックシステムへ提供する責務を負います。

## 3. Logic & Behavior

### 3.1 動的ドキュメント生成
- 新規画像の作成や既存ファイルの読み込み時、`PictureDock`（`DocumentDock` の拡張）が `PictureDocument` を生成し、現在のレイアウト構造に動的に追加します。

### 3.2 グローバル・フォーカス同期
- **課題**: ドックが分割されると、単一の XAML インスタンスへのバインドでは全体の「現在のアクティブ項目」を追跡できなくなります。
- **解決策**: `InjectableDockFactory.SetFocusedDockable` をインターセプトし、レイアウト内のどこでフォーカスが発生しても `ActiveDockableChanged` イベントを発火させる仕組みを導入しました。
- **再帰防止 (Loop Protection)**:
    - **送信側ガード**: `InjectableDockFactory` 内部で、前回の値と同一の場合はイベントをスキップします。
    - **受信側ガード**: `PictureFrame` (View) のプロパティ更新時に、現在の値と異なる場合のみ代入を行います。これにより、双方向バインドやイベント連鎖による `StackOverflowException` を完全に遮断します。

## 4. Constraints
- **Factory 依存**: 全てのドッキング操作は `InjectableDockFactory` を通じて行われる必要があります。手動で `IDockable` を追加すると、DI 解決やフォーカス同期が正しく機能しない可能性があります。
- **名前解決**: ドックの永続化（保存・復元）を行う場合、各 ViewModel に一意な `Id` が設定されている必要があります。