# Implementation Plan: MainViewModel のクリーン化と DI コンテナの導入

MainViewModel の肥大化を解消し、DIコンテナ（Microsoft.Extensions.DependencyInjection）を導入することで、テスト可能で保守性の高い構造へリファクタリングします。

## 専門家のアドバイス (Expert Consultation)
- efactor-legacy-code: 仕様化テスト（Golden Master）による挙動の固定を優先。
- unit-test-architect: DI導入後は、モックを活用した「古典派」のアプローチでリファクタリング耐性の高いテストを構築。
- efactor-strict-ddd-onion-complete: Presentation層からの具象インフラ依存を排除。

## Phase 0: セーフティネットの構築 (仕様化テスト)
現状の挙動を破壊しないよう、リファクタリング前の動作を固定するテストを作成します。

- [ ] Task: Eede.Presentation.Tests プロジェクトに MainViewModel の仕様化テストを作成
    - ファイルロード、ツール切り替え、Push/Pull の一連の流れをテストで再現
- [ ] Task: 現状のテストが全てパスすることを確認
- [ ] Task: Conductor - User Manual Verification 'Phase 0' (Protocol in workflow.md)

## Phase 1: DI コンテナの導入とインフラ依存の排除 (優先度1)
Microsoft.Extensions.DependencyInjection を導入し、ファイルI/Oの依存を抽象化します。

- [ ] Task: Microsoft.Extensions.DependencyInjection パッケージの追加
- [ ] Task: IPictureRepository や IStorageService などの依存をコンストラクタ注入に変更
- [ ] Task: App.axaml.cs で DI コンテナをセットアップし、インスタンス管理を委譲
- [ ] Task: Phase 0 のテストを DI 経由の解決に変更し、挙動が変わらないことを確認
- [ ] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md)

## Phase 2: ツール生成ロジックの Factory 化 (優先度2) [checkpoint: c13c221]
`IDrawStyle` の生成ロジックを `MainViewModel` から分離します。

- [x] Task: `IDrawStyleFactory` インターフェースと実装クラスの作成
- [x] Task: `MainViewModel.ExecuteUpdateDrawStyle` を Factory へ移譲
- [x] Task: ツール切り替え機能の単体テストを追加
- [x] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## Phase 3: Push/Pull ロジックの UseCase 移譲 (優先度3) [checkpoint: 0ed57be]
ViewModel 内に記述されているデータの加工・転送ロジックを Application 層へ移動します。

- [x] Task: `OnPushToDrawArea` および `OnPullFromDrawArea` のロジックを `PictureEditingUseCase` または新規 UseCase へ整理
- [x] Task: ViewModel は UseCase を呼び出すだけの薄い実装に変更
- [x] Task: 全てのテスト（Phase 0 を含む）を実行し、最終的な整合性を確認
- [x] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)
