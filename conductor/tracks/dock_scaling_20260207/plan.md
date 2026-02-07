# 実装計画: ドックエリアの拡大縮小機能 (TDD Style)

ドックエリアの各タブにおいて、画像の表示倍率を個別に変更できる機能を実装します。t_wada氏のTDDアプローチに基づき、テストによる仕様の明確化と、小さなサイクルでの実装・リファクタリングを繰り返します。

## フェーズ 1: ViewModel のスケーリングロジック実装 (TDD Cycle)
UIに依存しない純粋なロジック部分から着手し、確実な土台を作ります。

- [ ] Task: `DockPictureViewModel` の倍率初期化テスト (Red)
- [ ] Task: 初期化の実装 (Green)
- [ ] Task: 倍率変更メソッドのテスト (Red)
- [ ] Task: 倍率変更の実装 (Green)
- [ ] Task: ステップによる拡大縮小テスト (Red)
- [ ] Task: ステップロジックの実装 (Green)
- [ ] Task: リファクタリング (Refactor)
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: ViewModel のスケーリングロジック実装 (TDD Cycle)' (Protocol in workflow.md)

## フェーズ 2: UIバインディングとコマンドの結合 (TDD Cycle)
ViewModelとViewのつなぎこみをテスト駆動で行います。

- [ ] Task: コマンド実行のテスト (Red)
- [ ] Task: コマンドの実装 (Green)
- [ ] Task: `PictureContainer.axaml` の実装
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: UIバインディングとコマンドの結合 (TDD Cycle)' (Protocol in workflow.md)

## フェーズ 3: 描画サイズと座標変換の対応 (TDD Cycle)
View (Code-behind) のロジック変更も可能な限りテスト可能な形で切り出しながら進めます。

- [ ] Task: 表示サイズ計算のテスト (Red)
- [ ] Task: 表示サイズ更新の実装 (Green)
- [ ] Task: 入力座標変換のテスト (Red)
- [ ] Task: 座標変換の実装 (Green)
- [ ] Task: カーソル・プレビュー表示位置のテスト (Red)
- [ ] Task: 表示位置更新の実装 (Green)
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: 描画サイズと座標変換の対応 (TDD Cycle)' (Protocol in workflow.md)

## フェーズ 4: 統合確認と品質向上
- [ ] Task: 複数タブの独立性検証
- [ ] Task: リファクタリング (Refactor)
- [ ] Task: Conductor - User Manual Verification 'フェーズ 4: 統合確認と品質向上' (Protocol in workflow.md)
