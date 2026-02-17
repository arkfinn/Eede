# 実装プラン：Velopackによる自動アップデート機能の導入（TDD Edition）

#### フェーズ 1: ドメインとユースケースの構築 (TDD) [checkpoint: 351e92c]
- [x] Task: アップデート状態を表すドメインモデル（値オブジェクト）の定義 [e5607f0]
- [x] Task: `IUpdateService` インターフェースの設計 [e5607f0]
- [x] Task: **[Red]** `CheckUpdateUseCase` のテスト作成（Mockを使用した異常系・正常系の定義） [c119288]
- [x] Task: **[Green]** `CheckUpdateUseCase` の最小実装 [c119288]
- [x] Task: **[Refactor]** ユースケースのリファクタリング [c119288]
- [x] Task: Conductor - User Manual Verification 'フェーズ 1: ドメインとユースケース' (Protocol in workflow.md) [351e92c]

#### フェーズ 2: ViewModel への統合と状態遷移の検証 (TDD) [checkpoint: 72f577a]
- [x] Task: **[Red]** `WelcomeViewModel` におけるアップデート自動確認のテスト作成 [72f577a]
- [x] Task: **[Green]** `WelcomeViewModel` へのロジック実装 [72f577a]
- [x] Task: **[Red]** 「再起動して適用」コマンドの実行テスト [72f577a]
- [x] Task: **[Green]** `WelcomeViewModel` へのコマンド実装 [72f577a]
- [x] Task: **[Refactor]** ViewModel のリファクタリング [72f577a]
- [x] Task: Conductor - User Manual Verification 'フェーズ 2: ViewModel 統合' (Protocol in workflow.md) [72f577a]

#### フェーズ 3: インフラストラクチャ（Velopack 境界）の実装
- [~] Task: `VelopackUpdateService` の実実装 (`Eede.Infrastructure`)
- [ ] Task: DI コンテナへの登録と結合確認
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: インフラ統合' (Protocol in workflow.md)

#### フェーズ 4: UI 実装と最終検証 (UX 仕上げ)
- [ ] Task: `WelcomeView` (ようこそ画面) の XAML 実装とバインディング
- [ ] Task: `MainView` (メインエディタ) へのアップデート通知インジケーターの実装
- [ ] Task: ヘルプメニューへの「アップデートを確認」の追加
- [ ] Task: **[Red/Green]** 手動アップデート確認の ViewModel テストと実装
- [ ] Task: Conductor - User Manual Verification 'フェーズ 4: UI 仕上げ' (Protocol in workflow.md)
