# Eede アプリケーション アーキテクチャ概要

## 1. システム概要

Eede は、高機能な画像編集ツールとして設計されたデスクトップアプリケーションです。Avalonia UI フレームワークを使用し、クロスプラットフォーム対応のユーザーインターフェースを提供します。

## 2. アーキテクチャの特徴

本システムは、クリーンアーキテクチャの原則にしたがって設計されており、以下の主要なレイヤーで構成されています：

### 2.1 Domain Layer (Eede.Domain)

- ビジネスロジックの中核
- エンティティとバリューオブジェクト
- ドメインサービス
- 主要なコンポーネント：
  - Colors: カラーパレットと色管理
  - Drawings: 描画バッファーと描画ロジック
  - Pictures: 画像処理の中核機能

### 2.2 Application Layer (Eede.Application)

- ユースケースの実装
- ドメインレイヤーの操作を統合
- 主要なコンポーネント：
  - PaintLayers: レイヤー管理システム
  - UseCase: アプリケーションのユースケース実装

### 2.3 Infrastructure Layer (Eede.Infrastructure)

- 外部サービスとの統合
- データ永続化の実装
- ファイル操作の実装

### 2.4 Presentation Layer (Eede.Presentation)

- Avalonia UI ベースのユーザーインターフェース
- ViewModel と View の実装
- ユーザー入力の処理

## 3. 主要なデータフロー

1. ユーザーインタラクション → Presentation Layer
2. ユースケース実行 → Application Layer
3. ビジネスロジック処理 → Domain Layer
4. データ永続化 → Infrastructure Layer

## 4. 依存関係の方向

内側のレイヤー（Domain）は外側のレイヤーについて何も知りません：

- Domain ← Application ← Infrastructure/Presentation

## 5. クロスカッティングコンサーン

- ロギング
- エラーハンドリング
- パフォーマンス監視
- セキュリティ

## 6. テスト戦略

各レイヤーは独自のテストプロジェクトを持ちます：

- Eede.Domain.Tests
- Eede.Application.Tests
- Eede.Infrastructure.Tests

## 7. 設計原則

- SOLID 原則の遵守
- 依存性の逆転
- 関心の分離
- インターフェース分離

## 8. 今後の展開

- パフォーマンス最適化
- 新機能の追加
- クロスプラットフォーム対応の強化

## 9. メンテナンス

このドキュメントは定期的に更新され、最新のアーキテクチャ変更を反映します。
変更履歴は[CHANGELOG.md](../CHANGELOG.md)を参照してください。
