# Eede - Excellent Expert Dot Editor
[![.NET Core Desktop CI](https://github.com/arkfinn/Eede/actions/workflows/dotnet-desktop-ci.yml/badge.svg?branch=master)](https://github.com/arkfinn/Eede/actions/workflows/dotnet-desktop-ci.yml)
![](https://img.shields.io/github/license/arkfinn/Eede)
![](https://img.shields.io/github/v/release/arkfinn/Eede)

## 概要

Eede(Excellent Expert Dot Editor)は、32 ビット RGBA のドット絵を描くための専用ツールです。

## 必要条件

### 動作環境

- Windows
- .NET 8.0 以上

### 開発環境

- Visual Studio 2022 以上

## Eede のインストールと実行

[![Download](https://img.shields.io/badge/Download-Windows-blue?logo=github)](https://github.com/arkfinn/Eede/releases/latest/download/EedeSetup.exe)

1. [最新の Setup.exe](https://github.com/arkfinn/Eede/releases/latest/download/EedeSetup.exe) をダウンロードして実行してください。
2. アプリケーションは自動的にインストールされ、デスクトップにショートカットが作成されます。
3. アップデートがある場合は、起動時に自動的に確認・適用されます。

## 開発者向け情報

### リリース手順 (Velopack)

本プロジェクトは Velopack を使用して自動アップデートを配信しています。新しいバージョンをリリースする手順は以下の通りです：

1.  **タグの作成とプッシュ**:
    ```bash
    git tag v1.x.x
    git push origin v1.x.x
    ```
2.  **自動ビルドとパッケージング**:
    GitHub Actions がタグのプッシュを検知し、自動的に `vpk pack` を実行してリリースアセットを生成します。
3.  **GitHub Release の公開**:
    生成されたアセット（`.nupkg`, `RELEASES`, `Setup.exe`）が GitHub Release にドラフトまたは公開状態でアップロードされます。

※ 手動でパッケージングを行う場合は、`vpk` CLI ツールを使用してください。

## コントリビューション

プロジェクトへの貢献をお待ちしております。以下の手順で開発に参加してください。

1. 新機能の追加やバグ修正を行う前に、まず Issue を作成して提案内容を共有してください。
2. フォークしてリポジトリをクローンします。
3. 新しいブランチを作成します。
4. 変更を加え、コミットします。
5. プルリクエストを作成します。

プルリクエストの作成時には、関連する Issue 番号を本文に含めてください。

## ライセンス

このプロジェクトは MIT ライセンスの下で提供されています。詳細は `LICENSE` ファイルを参照してください。

## 連絡先

プロジェクトに関する質問やフィードバックは、Issues セクションまたはメールでお知らせください。

## 今後の改善点

### 既知のバグ

[Issues](https://github.com/arkfinn/Eede/issues)を参照ください。

### 機能

- パレット機能
  - 前回パレットの自動保存・読み込み
- 原寸表示ウィンドウの実装
- 描画エリア自動保存を追加
- 自動アンチエイリアスの実装

### 対応環境

- クロスプラットフォーム対応（予定）
