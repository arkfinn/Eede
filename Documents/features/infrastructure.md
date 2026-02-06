# Infrastructure: 基盤設計 (Architecture & Infrastructure)

## 1. Overview
Eede は、保守性とテスト容易性を最大化するために「オニオンアーキテクチャ」を採用しています。ドメイン知識を外部のフレームワーク（Avalonia）やライブラリ（Dock.Avalonia）から隔離し、純粋なビジネスロジックをドメイン層に封じ込める設計を徹底しています。

## 2. Data Model

### 2.1 Dependency Container
- **Microsoft.Extensions.DependencyInjection**: アプリケーション全体のサービス、UseCase、および ViewModel の依存関係を管理します。
- **InjectableDockFactory**: `Dock.Avalonia` 標準の Factory を拡張し、DI コンテナから ViewModel を解決するブリッジとして機能します。

### 2.2 View & ViewModel Mapping
- **ViewLocator**: 命名規則（`ViewModels.*ViewModel` → `Views.*View`）に基づき、ViewModel から対応する UI コンポーネントを自動解決します。

## 3. Logic & Behavior

### 3.1 依存性の逆転 (DIP)
- インフラ依存（ファイル入出力、クリップボード、ダイアログ等）は、Application 層で定義されたインターフェース（Port）を通じて行われ、Infrastructure 層でその具体的な実装（Adapter）が提供されます。

### 3.2 サービスの解体 (Service Elimination)
- 肥大化した「Service クラス」を廃止し、以下の構成に再定義しています。
    - **Domain Aggregates**: データの整合性と振る舞いを保持。
    - **UseCases**: ユーザーの意図を表現する単一操作（Add, Replace 等）。
    - **ViewModels**: UI の状態管理と UseCase の呼び出し。

### 3.3 ViewModel の DI 化
- すべての ViewModel はコンストラクタ注入によって依存関係を受け取ります。これにより、ユニットテストにおいてモックへの差し替えが容易になります。

## 4. Constraints
- **UI フレームワークへの非依存**: Domain および Application 層は Avalonia 等の UI ライブラリに直接依存してはなりません。色の表現などは共通の ArgbColor モデルを使用します。
- **命名規則の遵守**: ViewLocator が機能するため、ViewModel と View の命名および名前空間の対応関係を厳格に維持する必要があります。