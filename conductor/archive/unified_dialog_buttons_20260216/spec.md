# 概要: ダイアログボタンの統一と共通コンポーネントの導入

アプリケーション内のダイアログや設定画面におけるボタン配置（肯定/否定/キャンセル）を統一し、UXの一貫性を向上させます。

## 背景
現在、ボタンの並び順やスタイルが各画面でバラバラであり、ユーザーに一貫した操作感を提供できていません。これを解決するため、標準的な配置ルールを強制する共通コンポーネント `DialogButtons` と、重要度を示すスタイル定義を導入します。

## 機能要件

### 1. 共通スタイルの定義
- `App.axaml` または関連するスタイルファイルに `primary` クラスを追加。
- **Primary Button (`Classes="primary"`)**: アクセントカラー（`SystemControlHighlightListAccentLowBrush` 等）で塗りつぶし、文字色を白、太字にする。推奨されるアクションに使用。
- **Default Button**: 標準の枠線あり、またはグレーのスタイル。

### 2. 共通コンポーネント `DialogButtons` の作成
- **配置場所**: `Eede.Presentation/Controls/DialogButtons.axaml`
- **レイアウト**: 右寄せの `StackPanel` (Horizontal)。
- **順序**: [キャンセル] -> [Secondary (任意)] -> [Primary (必須級)]。
- **StyledProperty**:
    - `PrimaryText` (string, default: "OK"), `PrimaryCommand` (ICommand), `IsPrimaryVisible` (bool, default: true)
    - `SecondaryText` (string, default: null), `SecondaryCommand` (ICommand), `IsSecondaryVisible` (bool, default: false)
    - `CancelText` (string, default: "キャンセル"), `CancelCommand` (ICommand), `IsCancelVisible` (bool, default: true)
- **機能**: `IsEnabled` の連動（Commandの `CanExecute` 状態に応じてボタンが無効化される）。

### 3. 既存画面のリファクタリング
以下の画面のボタン配置を `DialogButtons` コンポーネントに置き換える。
- 画像リサイズダイアログ
- 設定画面 (AppSettings)
- 新規作成ダイアログ

## 非機能要件
- **保守性**: コンポーネントを差し替えるだけで、将来的に全画面のボタン配置ルールを一括変更可能にすること。
- **一貫性**: 「右側ほど強いアクション」という視覚的ヒエラルキーの徹底。

## 受け入れ基準
- `DialogButtons` コンポーネントを使用して、指定された画面のボタンが正しく表示・動作すること。
- `Primary` ボタンがアクセントカラーで強調されていること。
- コマンドが無効な場合、対応するボタンも無効化されること。

## 対象外
- モーダル以外のメイン画面上の特殊なツールボタンの統一。
- 独自の複雑なボタンレイアウトを必要とする画面の強制的な置き換え。
