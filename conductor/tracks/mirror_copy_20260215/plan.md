# 鏡面反転コピー機能 実装計画 (plan.md)

## 概要
作業エリアまたは選択範囲の半分を、もう半分に鏡面反転コピーする機能（左→右、上→下）を実装する。

## フェーズ 1: ドメインロジックの実装 (TDD)
鏡面反転コピーのコアロジックを `Eede.Domain` に実装します。

- [ ] Task: `PictureActions` 列挙型に `MirrorCopyRight` と `MirrorCopyBottom` を追加
- [ ] Task: 鏡面反転コピーの基底クラスまたは共通インターフェースの設計（必要に応じて）
- [ ] Task: `MirrorCopyRightAction` のテスト作成（正常系、奇数サイズ系）
- [ ] Task: `MirrorCopyRightAction` の実装
- [ ] Task: `MirrorCopyBottomAction` のテスト作成（正常系、奇数サイズ系）
- [ ] Task: `MirrorCopyBottomAction` の実装
- [ ] Task: `GeometricTransformationFactory`（または相当するファクトリ）への登録
- [ ] Task: Conductor - User Manual Verification 'フェーズ 1: ドメインロジックの実装 (TDD)' (Protocol in workflow.md)

## フェーズ 2: アプリケーション層の統合
ドメインロジックを `Eede.Application` のユースケースから呼び出せるようにします。

- [ ] Task: `TransformImageUseCase`（または関連する UseCase）の対応確認と修正
- [ ] Task: ユースケースレベルでのユニットテストの追加
- [ ] Task: Conductor - User Manual Verification 'フェーズ 2: アプリケーション層の統合' (Protocol in workflow.md)

## フェーズ 3: UI 実装とアイコンの配置
ツールバーにボタンを追加し、機能をユーザーに公開します。

- [ ] Task: `MirrorCopyRight.axaml` および `MirrorCopyBottom.axaml` アイコンリソースの利用設定確認
- [ ] Task: `PictureActionMenu.axaml` に MirrorCopyRight ボタンを追加
- [ ] Task: `PictureActionMenu.axaml` に MirrorCopyBottom ボタンを追加
- [ ] Task: ツールチップおよびアクセシビリティ対応（日本語対応）
- [ ] Task: Conductor - User Manual Verification 'フェーズ 3: UI 実装とアイコンの配置' (Protocol in workflow.md)

## フェーズ 4: 結合テストと最終確認
全体を通した動作確認を行います。

- [ ] Task: 選択範囲あり/なしでの統合テスト（Headless テスト）の追加
- [ ] Task: 巨大画像での動作パフォーマンス確認
- [ ] Task: Conductor - User Manual Verification 'フェーズ 4: 結合テストと最終確認' (Protocol in workflow.md)
