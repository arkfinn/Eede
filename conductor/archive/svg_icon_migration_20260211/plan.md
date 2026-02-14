# 実装計画: SVGアイコンコンポーネント基盤の構築と「透過ボタン」の移行

## フェーズ 1: アイコン基盤の構築とスタイル定義
SVG アイコンを PathIcon で統一的に扱うための共通スタイルと、リソース管理の仕組みを構築する。

- [ ] Task: .icon クラスを持つ PathIcon 用の共通スタイルを Styles/ 以下に作成・定義する
    - [ ] 16x16 のサイズ固定、Foreground 継承の設定を含む
- [ ] Task: アイコンリソースを集約するための Resources/Icons.axaml (集約用) を作成する
- [ ] Task: `App.axaml` で新規作成したスタイルとリソースファイルを読み込む設定を追加する
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: アイコン基盤の構築とスタイル定義' (Protocol in workflow.md)

## フェーズ 2: 「透過ボタン (BlurOn)」のリソース化とコンポーネント化
個別のアイコンを独立したファイルとして定義し、集約ファイルに登録する。

- [ ] Task: Resources/Icons/BlurOn.axaml を作成し、指定された SVG パスを StreamGeometry として定義する
- [ ] Task: Resources/Icons.axaml に BlurOn.axaml を MergedDictionaries として追加する
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: 「透過ボタン (BlurOn)」のリソース化とコンポーネント化' (Protocol in workflow.md)

## フェーズ 3: UI への適用と最終検証
既存のビットマップ表示箇所を新しい SVG アイコンに差し替え、動作を確認する。

- [ ] Task: MainView 等で透過ボタンを表示している箇所を特定し、表示を PathIcon (BlurOnGeometry 使用) に書き換える
- [ ] Task: 透過ボタンの表示が正しく SVG 化され、色の変化（親の Foreground 継承）やサイズが 16x16 であることを確認する
- [ ] Task: 全プロジェクトのビルドと、既存テストの実行によりデグレードがないことを確認する
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: UI 適用と最終検証' (Protocol in workflow.md)





