# Project Consolidation: プロジェクト構成の統合と再定義

## 1. Overview
現在のオニオンアーキテクチャに基づいた多層プロジェクト構成（9プロジェクト）を、保守コストの削減と開発スピードの向上のため、3つの主要プロジェクト（Core, Test, Desktop）に統合します。物理的な境界（Project Reference）を論理的な境界（Namespace & Linting）へ移行し、「モジュラーモノリス」に近い柔軟な構造を目指します。

## 2. Proposed Structure

### 2.1 Eede.Core (統合コアプロジェクト)
以下のレイヤーを単一のプロジェクト内にフォルダとして保持します。
- **Domain**: 純粋なドメインモデル、値オブジェクト、ドメインイベント。
- **Application**: UseCase、インターフェース定義（Port）、アプリケーションロジック。
- **Infrastructure**: DB/ファイル入出力、外部API、共通サービスの実装（Adapter）。
- **Presentation**: ViewModel、共有View、ViewLocator、コンバーター等。

### 2.2 Eede.Tests (統合テストプロジェクト)
すべてのユニットテスト、インテグレーションテストを集約します。
- **ArchTests**: アーキテクチャの境界（例：DomainがInfrastructureを参照していないか）を検証するテスト。
- 各レイヤー（Domain, Application...）に対応するサブフォルダでのテストコード。

### 2.3 Eede.Desktop / Eede.[Platform] (ホストプロジェクト)
マルチプラットフォーム展開の際のエントリポイントです。
- **Eede.Desktop**: Avaloniaの起動ロジック、プラットフォーム固有の構成（App.config, manifest等）。
- 今後、`Eede.Mobile` や `Eede.Web` 等が追加される際の雛形となります。

## 3. Implementation Plan

### 3.1 Phase 1: 境界検証の自動化 (ArchUnit.NETの導入)
プロジェクト統合前に、名前空間に基づいた依存関係ルールをコードとして定義します。
- `NetArchTest.eNet` を導入。
- 「Domain名前空間は、他の一切の名前空間に依存してはならない」等のルールを記述したテストを作成。

### 3.2 Phase 2: Coreプロジェクトの構築と移行
1. 新規プロジェクト `Eede.Core` を作成。
2. 以下の順序でコードを移動し、名前空間の不整合を修正。
   - Domain → Application → Infrastructure → Presentation
3. 各レイヤーが使用していた NuGet パッケージを `Eede.Core` に集約。

### 3.3 Phase 3: テストとホストの再編
1. `Eede.Tests` プロジェクトに全テストを統合。
2. `Eede.Presentation.Desktop` を `Eede.Desktop` にリネームし、参照先を `Eede.Core` のみに更新。
3. 古い `.csproj` ファイルおよびソリューション上のプロジェクト参照を削除。

## 4. Constraints & Risk Mitigation

### 4.1 カプセル化の保護
- **リスク**: `internal` 修飾子がレイヤーを越えて筒抜けになる。
- **対策**: 
  - 本当に隠蔽が必要なクラスには `file` スコープ修飾子（C# 11以降）を検討。
  - ArchTest により「Presentation層から Infrastructure層のクラス（特定の接尾辞を持つもの等）への直接アクセス」を禁止する。

### 4.2 NuGet 依存関係の露出
- **リスク**: Domain層から Infrastructure層のライブラリ（SQLite等）が見えてしまう。
- **対策**: 
  - コード上での使用は ArchTest でブロック可能。
  - インテリセンスに現れる不純物を許容するか、あるいは Infrastructure のみ独立したプロジェクトとして残すか（Core と Infrastructure の2分割）は、移行中の認知負荷を見て最終判断する。

### 4.3 マルチプラットフォームへの備え
- `Eede.Core` は `netstandard2.1` または `net8.0`（共通）をターゲットとし、特定のプラットフォーム（Windows API等）に依存するコードは必ずインターフェース化し、各プラットフォームプロジェクトまたは Infrastructure フォルダ内の条件付きコンパイルで処理します。
