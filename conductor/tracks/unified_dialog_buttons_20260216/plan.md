# 実装計画: ダイアログボタンの統一と共通コンポーネントの導入

## フェーズ 1: 基盤整備とスタイル定義
共通コンポーネントの土台となるスタイルと、テスト環境を準備します。

- [x] Task: `App.axaml` への `primary` ボタンスタイルの追加
- [x] Task: `Eede.Presentation/Controls` フォルダの作成
- [x] Task: フェーズ1の完了確認（ビルド確認）
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: 基盤整備とスタイル定義' (Protocol in workflow.md)

## フェーズ 2: DialogButtons コンポーネントの開発 (TDD)
コンポーネントの作成と、その動作を検証するヘッドレステストを実装します。

- [x] Task: `DialogButtons` の表示・コマンド連動を検証するユニットテストの作成 (`Eede.Presentation.Tests`)
- [x] Task: `DialogButtons.axaml` および `.axaml.cs` の新規作成とプロパティ定義
- [x] Task: テストがパスすることの確認（Red -> Green）
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: DialogButtons コンポーネントの開発' (Protocol in workflow.md)

## フェーズ 3: 既存画面のリファクタリング
作成したコンポーネントを使用して、既存のダイアログや設定画面を修正します。

- [x] Task: 新規作成ダイアログ (`NewPictureWindow` 等) のリファクタリング
- [x] Task: 画像リサイズダイアログ (`ResizeWindow` 等) のリファクタリング
- [x] Task: 設定画面 (`SettingsWindow` 等) のリファクタリング
- [x] Task: 全体の動作確認と UI の崩れがないかのチェック
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: 既存画面のリファクタリング' (Protocol in workflow.md)
