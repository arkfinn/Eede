# 実装プラン：Velopackによる自動アップデート機能の導入（TDD Edition）

#### フェーズ 1: ドメインとユースケースの構築 (TDD)
- [ ] Task: アップデート状態を表すドメインモデル（値オブジェクト）の定義
- [ ] Task: `IUpdateService` インターフェースの設計
- [ ] Task: **[Red]** `CheckUpdateUseCase` のテスト作成（Mockを使用した異常系・正常系の定義）
- [ ] Task: **[Green]** `CheckUpdateUseCase` の最小実装
- [ ] Task: **[Refactor]** ユースケースのリファクタリング
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: ドメインとユースケース' (Protocol in workflow.md)

#### フェーズ 2: ViewModel への統合と状態遷移の検証 (TDD)
- [ ] Task: **[Red]** `WelcomeViewModel` におけるアップデート自動確認のテスト作成
- [ ] Task: **[Green]** `WelcomeViewModel` へのロジック実装
- [ ] Task: **[Red]** 「再起動して適用」コマンドの実行テスト
- [ ] Task: **[Green]** `WelcomeViewModel` へのコマンド実装
- [ ] Task: **[Refactor]** ViewModel のリファクタリング
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: ViewModel 統合' (Protocol in workflow.md)

#### フェーズ 3: インフラストラクチャ（Velopack 境界）の実装
- [ ] Task: `VelopackUpdateService` の実実装 (`Eede.Infrastructure`)
- [ ] Task: DI コンテナへの登録と結合確認
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: インフラ統合' (Protocol in workflow.md)

#### フェーズ 4: UI 実装と最終検証 (UX 仕上げ)
- [ ] Task: `WelcomeView` (ようこそ画面) の XAML 実装とバインディング
- [ ] Task: `MainView` (メインエディタ) へのアップデート通知インジケーターの実装
- [ ] Task: ヘルプメニューへの「アップデートを確認」の追加
- [ ] Task: **[Red/Green]** 手動アップデート確認の ViewModel テストと実装
- [ ] Task: Conductor - User Manual Verification 'フェーズ 4: UI 仕上げ' (Protocol in workflow.md)
