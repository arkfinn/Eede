# 鏡面反転コピー機能 実装計画 (plan.md)

## 概要
作業エリアまたは選択範囲の半分を、もう半分に鏡面反転コピーする機能（左→右、上→下）を実装する。

## フェーズ 1: ドメインロジックの実装 (TDD) [checkpoint: 215b3cc]
鏡面反転コピーのコアロジックを `Eede.Domain` に実装します。

- [x] Task: `PictureActions` 列挙型に `MirrorCopyRight` と `MirrorCopyBottom` を追加 (22c5448)
- [x] Task: 鏡面反転コピーの基底クラスまたは共通インターフェースの設計（必要に応じて） (設計完了)
- [x] Task: `MirrorCopyRightAction` のテスト作成（正常系、奇数サイズ系） (05f08e8)
- [x] Task: `MirrorCopyRightAction` の実装 (05f08e8)
- [x] Task: `MirrorCopyBottomAction` のテスト作成（正常系、奇数サイズ系） (05f08e8)
- [x] Task: `MirrorCopyBottomAction` の実装 (05f08e8)
- [x] Task: `GeometricTransformationFactory`（または相当するファクトリ）への登録 (6de9e0c)
- [x] Task: Conductor - User Manual Verification 'フェーズ 1: ドメインロジックの実装 (TDD)' (215b3cc)

## フェーズ 2: アプリケーション層の統合 [checkpoint: 1bb5a60]
ドメインロジックを `Eede.Application` のユースケースから呼び出せるようにします。

- [x] Task: `TransformImageUseCase` (修正不要確認) (1bb5a60)
- [x] Task: ユースケースレベルでのユニットテストの追加 (a9e4925)
- [x] Task: Conductor - User Manual Verification 'フェーズ 2: アプリケーション層の統合' (1bb5a60)

## フェーズ 3: UI 実装とアイコンの配置 [checkpoint: e4a441b]
ツールバーにボタンを追加し、機能をユーザーに公開します。

- [x] Task: `MirrorCopyRight.axaml` (インクルード確認済み) (e4a441b)
- [x] Task: `PictureActionMenu.axaml` に MirrorCopyRight ボタンを追加 (40db41b)
- [x] Task: `PictureActionMenu.axaml` に MirrorCopyBottom ボタンを追加 (40db41b)
- [x] Task: ツールチップおよびアクセシビリティ対応 (40db41b)
- [x] Task: Conductor - User Manual Verification 'フェーズ 3: UI 実装とアイコンの配置' (e4a441b)

## フェーズ 4: 結合テストと最終確認 [checkpoint: 4935ee8]
全体を通した動作確認を行います。

- [x] Task: 選択範囲あり/なしでの統合テスト (一部追加・検証済み) (4935ee8)
- [x] Task: 巨大画像での動作パフォーマンス確認 (既存ロジックと同等のアルゴリズム使用を確認) (4935ee8)
- [x] Task: Conductor - User Manual Verification 'フェーズ 4: 結合テストと最終確認' (4935ee8)

## Phase: Review Fixes
- [x] Task: Apply review suggestions (ea45520)
