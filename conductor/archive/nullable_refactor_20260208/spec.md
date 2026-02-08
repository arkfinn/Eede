# Specification - Nullable Disable and Behavioral Refactoring

## Overview
現在のプロジェクトにおいて、中途半端に有効化されている、あるいは明示されていない <Nullable> 設定を、テストプロジェクトを除いて一括で <Nullable>disable</Nullable> に統一する。
この変更に伴う予期せぬ挙動の変化を防ぐため、リファクタリングの前に「振る舞いテスト（Characterization Tests）」を `Eede.Application` のユースケースを中心に作成し、コードの安全性を確保する。

## Goals
- すべての製品プロジェクト（Tests以外）で <Nullable>disable</Nullable> を明示的に設定する。
- 重要なビジネスロジック（UseCase）に対して、現状の挙動を保証するテストコードを作成する。
- 警告のノイズを減らし、ドメインモデルの洗練（ガード節の導入や不変条件の強化）に集中できる土壌を作る。

## Functional Requirements
### 1. 振る舞いテストの作成
- `Eede.Application.Tests` 内に、以下の領域のユースケースを対象としたテストを順次作成する：
  - **描画系:** `DrawActionUseCase`
  - **キャンバス操作系:** `TransferImageToCanvasUseCase`, `TransformImageUseCase`
  - **ファイル入出力系:** `LoadPictureUseCase`, `SavePictureUseCase`
  - **選択・コピー系:** `CopySelectionUseCase`, `PasteFromClipboardUseCase`

### 2. プロジェクト設定の統一（テスト成功後）
- 振る舞いテストの成功を確認した後、以下のプロジェクトファイルに <Nullable>disable</Nullable> を追加または更新する：
  - `Eede.Domain.csproj`
  - `Eede.Application.csproj`
  - `Eede.Infrastructure.csproj`
  - `Eede.Presentation.csproj`
  - `Eede.Presentation.Desktop.csproj`

## Non-Functional Requirements
- **テストの網羅性:** 既存の正常系および主要な異常系の挙動をキャプチャし、リファクタリング前後で結果が変わらないことを確認する。
- **保守性:** 実装詳細（privateメソッド等）ではなく、公開API（UseCaseのExecuteメソッド等）の振る舞いをテストする。

## Acceptance Criteria
- すべての対象 `.csproj` で <Nullable>disable</Nullable> が設定されている。
- 新設された振る舞いテストがすべてパスしている。
- 既存のすべてのテスト（`Eede.Domain.Tests` など）がパスしている。

## Out of Scope
- Nullable無効化以外の大きな機能追加や大規模なアーキテクチャ変更。
- テストプロジェクト自体の Nullable 設定の変更。
