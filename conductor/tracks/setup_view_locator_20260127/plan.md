# Implementation Plan: ViewLocator導入とDI統合の深化

この計画は、既存の依存関係を壊すことなく、Avalonia と Dock.Avalonia の ViewModel 解決メカニズムを DI コンテナ（Microsoft.Extensions.DependencyInjection）へ安全に移行するためのものです。

## Phase 1: 基礎構造の構築 (Infrastructure)
ViewLocator と基底クラスを導入し、解決の仕組みを準備します。

- [x] Task: `ViewModelBase` クラスの作成
    - `Eede.Presentation.ViewModels` 名前空間に作成
    - `ReactiveObject` を継承 [d4f6c7a]
- [x] Task: `ViewLocator` クラスの実装
    - `IDataTemplate` を継承し、`ViewModelBase` を受けて対応する `View` を名前解決する
    - View の生成には `App.Services` (IServiceProvider) を使用する [7b2a1f0]
- [x] Task: `App.axaml` への `ViewLocator` 登録
    - `DataTemplates` セクションに登録 [9a1b2c3]
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md) [a4b5c6d]

## Phase 2: ViewModel の整理と基底クラス適用 (Refactoring)
既存の全 ViewModel を新しい基底クラスに対応させます。

- [x] Task: 既存 ViewModel への `ViewModelBase` 継承の適用
    - `MainViewModel`, `PaletteContainerViewModel`, `AnimationViewModel`, `DrawableCanvasViewModel` 等 [b2c3d4e]
- [x] Task: `App.axaml.cs` の `ConfigureServices` の見直し
    - すべての ViewModel が `AddTransient` または `AddSingleton` で登録されていることを確認 [c3d4e5f]
- [x] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md) [d4e5f6a]

## Phase 3: Dock.Avalonia の DI 統合 (Dock Integration)
ドック内のパネル生成を DI コンテナ経由に差し替えます。

- [x] Task: `InjectableDockFactory` の実装
    - `Dock.Model.Avalonia.Factory` を継承 [g7h8i9j]
    - `IDocument`, `ITool` 等の生成時に DI コンテナから ViewModel を解決するようにオーバーライド
- [x] Task: `ConfigureServices` でのカスタム Factory 登録 [k0l1m2n]
- [x] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md) [o3p4q5r]

## Phase 4: エントリーポイントのクリーンアップ (Cleanup)
`App.axaml.cs` の手動生成コードを排除します。

- [x] Task: `MainWindow` / `MainView` の DataContext 設定の自動化
    - `App.axaml.cs` での `Services.GetRequiredService<MainViewModel>()` による明示的代入を排除し、XAML 側で DataContext を解決できるか検証（必要に応じて View のコンストラクタ注入を検討） [e5f6a7b]
- [x] Task: 動作確認とリグレッショナテスト
    - 起動、ドックの配置、各種ツールの動作確認 [f6a7b8c]
- [x] Task: Conductor - User Manual Verification 'Phase 4' (Protocol in workflow.md) [a8b9c0d]
