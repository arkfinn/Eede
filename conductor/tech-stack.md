# 技術スタック

## コア技術
- **言語:** C#
- **プラットフォーム:** .NET 8.0
- **UI フレームワーク:** Avalonia UI
- **UI パターン:** ReactiveUI (MVVM)
- **レイアウト管理:** Dock.Avalonia

## アーキテクチャ
- **設計思想:** Domain-Driven Design (DDD)
- **構造:** クリーンアーキテクチャに準拠したマルチプロジェクト構成
  - `Eede.Core / Eede.Domain`: ドメインロジック、エンティティ、値オブジェクト
  - `Eede.Application`: ユースケース、アプリケーションサービス
  - `Eede.Infrastructure`: 外部ライブラリ依存、ファイルI/O
  - `Eede.Presentation`: UI (Avalonia), ViewModels

## テスト
- **フレームワーク:** NUnit
- **カバレッジ計測:** coverlet.collector

## その他ライブラリ
- **Fody (ReactiveUI.Fody):** INotifyPropertyChanged の自動実装
- **System.Text.Json:** JSON シリアライズ
