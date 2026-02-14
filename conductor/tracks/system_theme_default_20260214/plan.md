# Implementation Plan: システム設定に合わせたテーマ選択の初期値設定

## 概要
設定画面のテーマ選択において、未設定時の初期値をシステムの設定（Light/Dark）に合わせる実装を行います。

## フェーズ 1: 基盤調査とテスト作成
- [x] Task: システムテーマ取得方法の調査
    - Avaloniaの `Application.Current.ActualThemeVariant` または `PlatformSettings.GetColorValues()` の挙動を確認する
- [x] Task: 設定クラスの現状確認
    - `Eede.Presentation` 内の `Settings` クラス（またはそれに準ずるクラス）の初期値設定ロジックを確認する
- [x] Task: 失敗するテストの作成（Red）
    - 設定ファイルが存在しない場合に、システム設定に基づいたテーマが ViewModel に反映されることを検証するテストを書く

## フェーズ 2: 実装 (Green)
- [x] Task: システムテーマ判定ロジックの実装
    - `Settings` の初期化時, または ViewModel の初期化時にシステムテーマを取得するサービスを導入する
- [x] Task: ViewModel の初期値設定ロジックの修正
    - 保存された設定がない場合にのみシステムテーマを採用する条件分岐を追加する
- [x] Task: 設定画面（View）とのバインド確認
    - ラジオボタン等の選択状態が正しく反映されることを確認する

## フェーズ 3: リファクタリングと検証
- [x] Task: テストのパス確認とリファクタリング（Refactor）
- [x] Task: 既存のテーマ切り替え機能への影響確認
- [x] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)

## フェーズ 4: 完了処理
- [x] Task: チェックポイント作成
