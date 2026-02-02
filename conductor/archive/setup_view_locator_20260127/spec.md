# Specification: ViewLocator導入とDI統合の深化 (Architecture Refinement)

## 1. Overview
現状の「手動での ViewModel 生成」は、依存関係が増えるたびに `App.axaml.cs` が肥大化する典型的なレガシー予備軍です。
本トラックでは、Avalonia の標準メカニズム（`ViewLocator`）と `Dock.Avalonia` のカスタムファクトリを統合し、「依存関係の解決」と「UIの構成」を完全に分離します。これにより、コードの変更耐性とテスト容易性を劇的に向上させます。

## 2. Functional Requirements (Architecture Focus)
- **ViewModelBase の薄い導入 (Clean Design)**:
    - `ReactiveObject` を継承しつつ、プロパティ変更通知などの「UIの関心事」に特化した `ViewModelBase` を作成。
    - **極意:** 基底クラスにビジネスロジックや DI コンテナへの参照（Service Locator）を決して含めない。
- **ViewLocator による名前解決の自動化**:
    - 名前空間 `Eede.Presentation.ViewModels.*` から `Eede.Presentation.Views.*` へのマッピングを自動化。
    - **レガシー改善ポイント:** View のインスタンス化も `IServiceProvider` を経由させ、View 自体が Service を必要とする特殊なケースにも対応可能にする。
- **Dock.Avalonia の Dependency Injection 統合**:
    - `Dock.Model.Avalonia.Factory` をオーバーライドした `InjectableDockFactory` を実装。
    - ドック内の各ツール（パレット、アニメーション等）が要求する依存関係を、DI コンテナがコンストラクタ経由で自動注入する仕組みを構築。
- **App.axaml.cs の「純粋なエントリーポイント」化**:
    - サービス登録（ConfigureServices）と初期画面の解決のみに責務を限定。
    - `MainWindow` の初期化を XAML での解決に委ねる。

## 3. Non-Functional Requirements & Principles
- **依存性逆転の原則 (DIP) の徹底**: View 側は ViewModel の具体的な生成方法を知らず、DI コンテナのみがそれを知る。
- **テスト可能性の確保**: すべての ViewModel がコンストラクタ注入のみで動作するため、ユニットテスト時に任意のモックを差し込める状態を維持する。
- **副作用の排除**: ViewLocator 導入前後で、既存のバインディングや画面遷移の挙動が一切変わらないこと。

## 4. Acceptance Criteria (Verification Protocol)
- [ ] **Dependency Check**: `App.axaml.cs` に `new ...ViewModel()` という記述が一つも残っていないこと。
- [ ] **Dock Injection**: ドック内の ViewModel（例: `AnimationViewModel`）が、正しく DI コンテナから注入された Service を利用できていること。
- [ ] **Naming Convention**: フォルダ構成（ViewModels/Views）に不整合がある場合に、ViewLocator が適切な例外を投げ、開発者に構成ミスを知らせること。
- [ ] **Regression**: アプリケーション起動から、画像編集、アニメーション再生までの主要フローが正常に動作すること。

## 5. Out of Scope
- ドメイン層（Core/Domain）のロジック変更。
- 現状 DI 化されていないサードパーティ製ライブラリの内部動作の変更。
