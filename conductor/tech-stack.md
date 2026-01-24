# 技術スタック

## コア技術
- **言語:** C#
- **プラットフォーム:** .NET 8.0
- **UI フレームワーク:** Avalonia UI
- **UI パターン:** ReactiveUI (MVVM)
- **レイアウト管理:** Dock.Avalonia

## アーキテクチャ
- **設計思想:** Domain-Driven Design (DDD) および オニオンアーキテクチャ
- **UI状態管理:** ステートパターンによる操作状態の管理に加え、`DrawingSession` 集約による不変な描画履歴（Undo/Redo）管理を導入
- **構造:** クリーンアーキテクチャに準拠したマルチプロジェクト構成
  - `Eede.Core / Eede.Domain`: ドメインロジック、エンティティ、値オブジェクト（`DrawingTool` 等）
  - `Eede.Application`: ユースケース、アプリケーションサービス（`ICanvasService` 等）
  - `Eede.Infrastructure`: 技術詳細（`IPictureRepository` 実装、ファイルI/O）
  - `Eede.Presentation`: UI (Avalonia), ViewModels

## テスト
- **フレームワーク:** NUnit
- **カバレッジ計測:** coverlet.collector

## その他ライブラリ
- **Fody (ReactiveUI.Fody):** INotifyPropertyChanged の自動実装
- **System.Text.Json:** JSON シリアライズ
