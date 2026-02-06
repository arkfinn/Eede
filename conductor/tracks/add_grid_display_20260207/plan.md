# 実装計画: 作業エリアへのグリッド表示機能の追加 (TDDアプローチ)

## 第1フェーズ: ViewModel の状態管理と操作
グリッドの表示状態（ON/OFF）を管理するロジックを、テスト先行で実装します。

- [x] Task: `MainViewModel` のグリッド表示フラグのテスト作成と実装 (17a309b)
    - [x] `IsShowPixelGrid`, `IsShowCursorGrid` の初期値と変更を検証するテストを `MainViewModelTests` に追加
    - [x] テストをパスさせるためのプロパティ実装
- [x] Task: ツールバー（`MainView.axaml`）の UI 実装 (17a309b)
    - [x] 2つのトグルボタンを配置し、ViewModel とバインド
- [x] Task: Conductor - User Manual Verification '第1フェーズ: 基盤の構築と表示状態の管理' (Protocol in workflow.md) (17a309b)

## 第2フェーズ: グリッド描画ロジックの実装
描画に必要な計算ロジックを、ピュアなロジックとして抽出または検証可能な形で実装します。

- [x] Task: グリッド描画判定ロジックのテストと実装 (17a309b)
    - [x] 「倍率x4以上で1pxグリッドを表示する」判定ロジックのテスト作成
    - [x] 判定ロジックの実装（ViewModel または専用のヘルパー）
- [x] Task: `DrawableCanvas` への描画処理の追加 (17a309b)
    - [x] 1pxグリッドの実装（ダイナミック・グレー実線）
    - [x] カーソルグリッドの実装（カーソルサイズ・ダイナミック・グレー破線）
- [x] Task: （達人のこだわり）背景色に応じたダイナミック・グレーの算出ロジックのテストと実装 (17a309b)
    - [x] 背景の明度を考慮した、視認性の高いグレーを算出するロジックを検証するテストの作成
    - [x] 算出ロジックの実装
- [x] Task: Conductor - User Manual Verification '第2フェーズ: グリッド描画ロジックの実装' (Protocol in workflow.md) (17a309b)

## 第3フェーズ: 統合確認とリファクタリング
実際の描画結果の確認と、コードの整理を行います。

- [x] Task: 動作確認とパフォーマンス検証 (17a309b)
    - [x] 大規模画像や高倍率時の描画負荷が許容範囲内であることを確認
- [x] Task: コードのリファクタリング (17a309b)
    - [x] 重複ロジックの整理、可読性の向上
- [x] Task: Conductor - User Manual Verification '第3フェーズ: 仕上げと視認性の調整' (Protocol in workflow.md) (17a309b)