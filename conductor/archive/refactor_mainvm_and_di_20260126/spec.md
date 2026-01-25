# Track Specification: MainViewModel のクリーン化と DI コンテナの導入

## 1. 概要
MainViewModel に集中している責務を整理し、Microsoft.Extensions.DependencyInjection を導入して依存関係を疎結合にします。これにより、テスト容易性と保守性を向上させます。

## 2. 背景・目的
- MainViewModel がインフラ層の具象クラス（PictureRepository 等）に直接依存しており、単体テストが困難。
- ViewModel がツール生成やデータ連携などのアプリケーションロジックを多く抱え、肥大化（God Object化）している。
- App.axaml.cs での手動による依存関係の組み上げが限界に近づいている。

## 3. ゴール (Acceptance Criteria)
- [ ] Microsoft.Extensions.DependencyInjection が導入され、各 ViewModel やサービスが DI コンテナ経由で解決されている。
- [ ] MainViewModel から具象クラスへの 
ew による依存が排除され、インターフェース経由での注入に変更されている。
- [ ] IDrawStyle の生成ロジックが IDrawStyleFactory に移譲されている。
- [ ] 画像の Push/Pull ロジックが Application 層の適切な UseCase に整理されている。
- [ ] リファクタリング前後で既存の機能（ロード、セーブ、描画ツール切り替え、Push/Pull）の挙動が変わっていないことがテストで証明されている。

## 4. 機能要件
### 4.1. 仕様化テストの構築 (Phase 0)
- 現状の MainViewModel に対して、ファイルロード/セーブ、ツール切り替え、Push/Pull の基本動作を確認するテストを作成する。

### 4.2. DI コンテナの導入
- Microsoft.Extensions.DependencyInjection をプロジェクトに追加。
- App.axaml.cs でサービスと ViewModel の登録・解決を行う。

### 4.3. 依存関係の抽象化と移譲
- **優先度1:** ファイル I/O の抽象化 (IPictureRepository 等を注入)。
- **優先度2:** IDrawStyleFactory の作成と、MainViewModel からのツール生成ロジックの分離。
- **優先度3:** Push/Pull 処理の Application 層への再配置。

## 5. 非機能要件
- **単体テストカバレッジ:** 今回変更を加えるロジックについて 80% 以上を維持。
- **設計原則:** DDD、オニオンアーキテクチャ、SOLID原則を遵守。

## 6. アウトオブスコープ
- UI（.axaml）のデザイン変更。
- DrawableCanvasViewModel の内部ロジックの詳細なリファクタリング（本トラックでは連携部分に留める）。
