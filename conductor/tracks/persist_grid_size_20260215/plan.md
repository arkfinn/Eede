# Implementation Plan - ツールバーでのグリッドサイズ設定の永続化 (TDD Edition) [checkpoint: 6632ee0]

ツールバーのグリッドサイズ設定を JSON ファイルとして永続化し、起動時に復元する。
Red-Green-Refactor サイクルを厳守し、テストによって設計を導き出す。
※Service クラスの使用を避け、UseCase パターンを採用する。

## Phase 1: Domain & Infrastructure - Settings Data Model & Repository [checkpoint: 6632ee0]
設定情報のデータ構造と、JSON による永続化の仕組みを TDD で構築する。

- [x] Task: 振る舞いの定義：\AppSettings\ データモデルの作成（シリアライズ・デシリアライズのテストを先に書く） [6632ee0]
    - [x] 1.1 (Red): JSON へのシリアライズ/デシリアライズを確認する失敗テストを \Eede.Infrastructure.Tests\ に作成 [6632ee0]
    - [x] 1.2 (Green): \AppSettings\ モデルを \Eede.Presentation.Settings\ に作成し、テストをパスさせる [6632ee0]
- [x] Task: インフラの定義：\ISettingsRepository\ と \JsonSettingsRepository\ の実装 [6632ee0]
    - [x] 2.1 (Red): ファイルが存在しない場合にデフォルト値を返すこと、保存した値が読み込めることを確認する失敗テストを作成 [6632ee0]
    - [x] 2.2 (Green): \ISettingsRepository\ インターフェースと \JsonSettingsRepository\（\%AppData%\ 対応）の実装 [6632ee0]
    - [x] 2.3 (Refactor): パス計算ロジックやファイルアクセスコードの整理 [6632ee0]
- [x] Task: Conductor - User Manual Verification 'Phase 1: Domain & Infrastructure' (Protocol in workflow.md) [6632ee0]

## Phase 2: Application UseCase - Coordination logic [checkpoint: 6632ee0]
ViewModel と Repository を橋渡しする UseCase のロジックを TDD で実装する。

- [x] Task: \LoadSettingsUseCase\ と \SaveSettingsUseCase\ の実装 [6632ee0]
    - [x] 3.1 (Red): UseCase が Repository を適切に呼び出すテストを作成 [6632ee0]
    - [x] 3.2 (Green): UseCase を \Eede.Application.UseCase.Settings\ に実装 [6632ee0]
- [x] Task: DI 登録：\App.axaml.cs\ での \ISettingsRepository\ と UseCase の登録 [6632ee0]
- [x] Task: Conductor - User Manual Verification 'Phase 2: Application UseCase' (Protocol in workflow.md) [6632ee0]

## Phase 3: Presentation - ViewModel Integration & Observable Binding [checkpoint: 6632ee0]
MainViewModel に UseCase を統合し、リアクティブに設定を同期させる。

- [x] Task: \MainViewModel\ の初期化時ロードの実装 [6632ee0]
    - [x] 4.1 (Red): 初期化時に UseCase から値を取得し、\MinCursorWidth/Height\ に反映されることを確認するテストを \Eede.Presentation.Tests\ に作成 [6632ee0]
    - [x] 4.2 (Green): \MainViewModel\ のコンストラクタでロード処理を実装 [6632ee0]
- [x] Task: \MainViewModel\ の変更時保存の実装 [6632ee0]
    - [x] 5.1 (Red): \MinCursorWidth/Height\ が変更されたとき、SaveUseCase が呼ばれることを確認するテストを作成 [6632ee0]
    - [x] 5.2 (Green): 変更を監視（\WhenAnyValue\）して保存を呼び出すロジックを実装 [6632ee0]
    - [x] 5.3 (Refactor): Observable の購読管理（Disposable）の整理 [6632ee0]
- [x] Task: Conductor - User Manual Verification 'Phase 3: Presentation' (Protocol in workflow.md) [6632ee0]

## Phase 4: Integration & Robustness [checkpoint: 6632ee0]
実環境での統合確認と例外処理のガードを固める。

- [x] Task: 堅牢性の向上：ファイル読み書き失敗時などに例外で落ちないことを確認 [6632ee0]
- [x] Task: 最終確認：実際のアプリを起動し、%AppData% への書き込みと再起動後の復元を確認する [6632ee0]
- [x] Task: Conductor - User Manual Verification 'Phase 4: Integration & Robustness' (Protocol in workflow.md) [6632ee0]
