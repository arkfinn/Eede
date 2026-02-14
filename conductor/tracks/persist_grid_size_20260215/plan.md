# Implementation Plan - ツールバーでのグリッドサイズ設定の永続化 (TDD Edition)

ツールバーのグリッドサイズ設定を JSON ファイルとして永続化し、起動時に復元する。
Red-Green-Refactor サイクルを厳守し、テストによって設計を導き出す。

## Phase 1: Domain & Infrastructure - Settings Data Model & Repository
設定情報のデータ構造と、JSON による永続化の仕組みを TDD で構築する。

- [ ] Task: 振る舞いの定義：\AppSettings\ データモデルの作成（シリアライズ・デシリアライズのテストを先に書く）
    - [ ] 1.1 (Red): JSON へのシリアライズ/デシリアライズを確認する失敗テストを \Eede.Infrastructure.Tests\ に作成
    - [ ] 1.2 (Green): \AppSettings\ モデルを \Eede.Presentation.Settings\ に作成し、テストをパスさせる
- [ ] Task: インフラの定義：\ISettingsRepository\ と \JsonSettingsRepository\ の実装
    - [ ] 2.1 (Red): ファイルが存在しない場合にデフォルト値を返すこと、保存した値が読み込めることを確認する失敗テストを作成
    - [ ] 2.2 (Green): \ISettingsRepository\ インターフェースと \JsonSettingsRepository\（\%AppData%\ 対応）の実装
    - [ ] 2.3 (Refactor): パス計算ロジックやファイルアクセスコードの整理
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Domain & Infrastructure' (Protocol in workflow.md)

## Phase 2: Application Service - Coordination logic
ViewModel と Repository を橋渡しするサービスのロジックを TDD で実装する。

- [ ] Task: \SettingsService\ の実装（初期値の提供と保存の委譲）
    - [ ] 3.1 (Red): \SettingsService\ が Repository を適切に呼び出し、キャッシュやデフォルト値を管理するテストを Mock を用いて作成
    - [ ] 3.2 (Green): \SettingsService\ を \Eede.Presentation.Services\ に実装
    - [ ] 3.3 (Refactor): インターフェースの抽出や責務の整理
- [ ] Task: DI 登録：\App.axaml.cs\ での \ISettingsRepository\ と \SettingsService\ の登録
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Application Service' (Protocol in workflow.md)

## Phase 3: Presentation - ViewModel Integration & Observable Binding
MainViewModel に設定サービスを統合し、リアクティブに設定を同期させる。

- [ ] Task: \MainViewModel\ の初期化時ロードの実装
    - [ ] 4.1 (Red): 初期化時に \SettingsService\ から値を取得し、\MinCursorWidth/Height\ に反映されることを確認するテストを \Eede.Presentation.Tests\ に作成
    - [ ] 4.2 (Green): \MainViewModel\ のコンストラクタでロード処理を実装
- [ ] Task: \MainViewModel\ の変更時保存の実装
    - [ ] 5.1 (Red): \MinCursorWidth/Height\ が変更されたとき、\SettingsService\ の保存メソッドが呼ばれることを確認するテストを作成
    - [ ] 5.2 (Green): 変更を監視（\WhenAnyValue\）して保存を呼び出すロジックを実装
    - [ ] 5.3 (Refactor): Observable の購読管理（Disposable）の整理
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Presentation' (Protocol in workflow.md)

## Phase 4: Integration & Robustness
実環境での統合確認と例外処理のガードを固める。

- [ ] Task: 堅牢性の向上：ファイル書き込み失敗時などに例外で落ちないことを確認するテストの追加
- [ ] Task: 最終確認：実際のアプリを起動し、%AppData% への書き込みと再起動後の復元を確認する
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Integration & Robustness' (Protocol in workflow.md)
