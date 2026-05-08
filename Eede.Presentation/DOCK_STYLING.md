# Dock.Avalonia スタイリング仕様

Eede アプリケーションにおける `Dock.Avalonia` (v11) のカスタマイズ方法と、発生しやすい技術的問題についての備忘録です。

## 1. 最重要：InvalidCastException の回避
Avalonia 11 の `FluentTheme` は、システム予約済みのキー（例：`SystemAccentColor`）を「Color 型」として内部で使用します。
**これらのキーを `SolidColorBrush` として再定義・上書きしないでください。** グローバル・局所に関わらず、これを行うとアプリケーション起動時に型不整合によるクラッシュが発生します。

- **誤り**: `<SolidColorBrush x:Key="SystemAccentColor">...</SolidColorBrush>`
- **正解**: `ThemeDictionaries` 内で `<Color x:Key="SystemAccentColor">...</Color>` として定義し、ブラシは独自名でマッピングする。

## 2. ハードコードされた「青色」の克服
`Dock.Avalonia.Themes.Fluent` ライブラリは、一部のブラシ（特にタブ関連）にシステムカラーを無視したハードコードされた 16 進数コードを使用しています。
これらをテーマ色（緑）に変更するには、以下のキーを `App.axaml` の `ThemeDictionaries` で直接上書きする必要があります。

### 対象のリソースキー
| キー名 | 用途 |
| :--- | :--- |
| `DockApplicationAccentBrushLow` | タブやアイテムの選択時背景 |
| `DockApplicationAccentBrushMed` | ホバー時の背景色 |
| `DockApplicationAccentBrushHigh` | 閉じるボタンなどの強調色 |
| `DockTabActiveBackgroundBrush` | アクティブなタブの背景 |
| `DockTabActiveForegroundBrush` | アクティブなタブの文字色 |
| **`DockDocumentTabPointerOverForegroundBrush`** | **Documentタブのホバー時の文字色** |
| `DockTabHoverBackgroundBrush` | ホバー時のタブ背景 |
| `DockTabHoverForegroundBrush` | ホバー時のタブ文字色 (Toolタブ等) |
| `DockTabInactiveHoverForegroundBrush` | 非アクティブ時のホバー文字色 |
| `DockTabActiveIndicatorBrush` | タブ下部のインジケーター（下線） |

## 3. テーマごとの配色設定
Eede では `App.axaml` の `ThemeDictionaries` を通じて、ライト / ダークそれぞれのモードで配色を制御しています。

- **ダークモード**: 文字の視認性を高めるため、ネオングリーン背景では文字色（Foreground）を `Black` に設定しています。
- **ライトモード**: 標準的な緑背景では文字色を `White` に設定しています。

## 4. スタイル優先順位の注意
`Styles.axaml` での `DocumentControl` や `ToolControl` に対するスタイル指定は、リソースの引き上げ（DynamicResource）よりも弱い場合があります。基本的な配色は `App.axaml` の `ThemeDictionaries` で定義したブラシを Dock が自動的に参照するように構築するのが最も安定します。
