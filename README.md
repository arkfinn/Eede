# Eede - Excellent Expert Dot Editor
[![Release](https://github.com/arkfinn/Eede/actions/workflows/dotnet-desktop-release.yml/badge.svg)](https://github.com/arkfinn/Eede/actions/workflows/dotnet-desktop-release.yml)

## 概要
Eede(Excellent Expart Dot Editor)は、32ビットRGBAのドット絵を描くための専用ツールです。

##  必要条件

### 動作環境
- Windows
- .NET 8.0  以上

### 開発環境
- Visual Studio 2022  以上

##  Eedeのインストールと実行

[![Download](https://img.shields.io/badge/Download-Windows-blue?logo=github)](https://github.com/arkfinn/Eede/releases/latest/download/eede.zip)

1. ダウンロードしたファイルを適当な場所に解凍してください。
2. Eede.exeを実行してください。

##  コントリビューション

プロジェクトへの貢献をお待ちしております。以下の手順でプルリクエストを送信してください。

1.  フォークしてリポジトリをクローンします。
2.  新しいブランチを作成します。
3.  変更を加え、コミットします。
4.  プルリクエストを作成します。

##  ライセンス

このプロジェクトはMITライセンスの下で提供されています。詳細は `LICENSE` ファイルを参照してください。

##  連絡先

プロジェクトに関する質問やフィードバックは、Issuesセクションまたはメールでお知らせください。

##  今後の改善点

### バグ
- 一度でもDocumentをCloseした後に他のDocumentを移動するとDockが表示されなくなる不具合
- Documentを開いた際にCursorAreaが必ず32x32になる不具合
- 右から左・下から上に動かした場合の範囲選択の動作が時々おかしい不具合
- 無限アンドゥできるためにメモリをいつか使い果たす仕様

### デザイン

- アイコンの作成

### 機能

- パレット機能
    - 前回パレットの自動保存・読み込み
- グリッド表示を実装
- 他のペンタイプの実装
    - 円（塗りつぶしあり・なし）
    - 四角（塗りつぶしあり・なし）
- 原寸表示ウィンドウの実装
- 選択範囲の実装
    - 切り取り
    - コピー
    - 貼り付け
    - 移動
- 描画エリア自動保存を追加
- 子ウィンドウの拡大縮小機能
- 自動アンチエイリアスの実装
- アニメーション確認ウィンドウ

### 対応環境

- クロスプラットフォーム対応


