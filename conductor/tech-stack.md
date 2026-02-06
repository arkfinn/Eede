# 技術スタック

## コア技術
- **言語:** C#
- **プラットフォーム:** .NET 8.0
- **UI フレームワーク:** Avalonia UI
- **UI パターン:** ReactiveUI (MVVM)
- **レイアウト管理:** Dock.Avalonia

## アーキテクチャ
- **設計思想:** Domain-Driven Design (DDD) および オニオンアーキテクチャ
- **UI状態管理:** ステートパターンによる操作状態の管理に加え、`DrawingSession` 集約による不変な描画履歴（Undo/Redo）管理を導入。履歴管理にはポリモーフィックな `IHistoryItem` モデルを採用し、作業エリアの描画だけでなくドックエリアへの画像転送（Pull）などの異なる種類の操作も単一の時系列スタックとして一元管理する。また、マウス等の入力イベントに伴う状態遷移は `CanvasInteractionSession` 集約にカプセル化し、ドメイン知識の Presentation 層への漏出を防ぐ。さらに、移動や貼り付け操作時には `DrawingSession` が `SelectionPreviewType`（CutAndMove または Paste）を持つ `SelectionPreviewInfo` を保持することで、キャンバスを汚染せずに用途に応じた非破壊的なプレビューと微調整を可能にしている。
- **ViewModel 設計:** 肥大化した ViewModel に対しては責務の再配置（Repatriation）を適用し、UI 型に依存しない計算ロジックを Domain 層（Value Object, Entity）や Application 層（UseCase）へ移動することで、テスト可能性と保守性を向上させる。
- **空間の分離:** UI 表示上の座標 (`DisplayCoordinate`) とピクセル単位のドメイン座標 (`CanvasCoordinate`) を型として厳格に分離し、倍率（Magnification）に基づく変換ロジックを VO 内に閉じ込める。
- **構造:** オニオンアーキテクチャに準拠したマルチプロジェクト構成
  - `Eede.Core / Eede.Domain`: ドメインロジック、集約（`DrawingSession`）、値オブジェクト（`DrawingTool`, `DisplayCoordinate`, `CanvasCoordinate` 等）
  - `Eede.Application`: ユースケース（`DrawActionUseCase`, `SavePictureUseCase`, `LoadPictureUseCase`, `TransformImageUseCase`, `TransferImageToCanvasUseCase`, `TransferImageFromCanvasUseCase` 等）、アプリケーションサービス
  - `Eede.Infrastructure`: 技術詳細（`IPictureRepository` 実装、ファイルI/O）
  - `Eede.Presentation`: UI (Avalonia), ViewModels, アダプター（`PictureRepository` 実装）
- **依存関係管理:** `Microsoft.Extensions.DependencyInjection` を採用し、`App.axaml.cs` でサービスと ViewModel を一括登録。コンストラクタ注入による疎結合な設計を徹底。

## テスト
- **フレームワーク:** NUnit
- **モッキング:** Moq
- **UIテスト:** Avalonia.Headless.NUnit (ヘッドレス環境でのUIコンポーネントテスト)
- **Reactive Extensionsテスト:** ReactiveUI.Testing, Microsoft.Reactive.Testing (TestSchedulerを用いた時間依存ロジックのテスト)
- **カバレッジ計測:** coverlet.collector

## その他ライブラリ
- **Microsoft.Extensions.DependencyInjection:** DI コンテナによる依存関係管理
  - `ViewLocator` および `ServiceResolveExtension` を介して、XAML 側で ViewModel を自動解決する仕組みを導入
- **Fody (ReactiveUI.Fody):** INotifyPropertyChanged の自動実装
- **System.Text.Json:** JSON シリアライズ
