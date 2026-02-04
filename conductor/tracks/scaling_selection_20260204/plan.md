# Implementation Plan: 選択範囲の拡大縮小 (Scaling Selection)

## フェーズ 1: ドメインモデルの定義と純粋なロジックの実装 (DDD Core)
UIや描画ライブラリに依存しない純粋なドメインモデルとして、変形操作と制約を定義・実装します。

- [x] Task: 選択範囲変形モデル (ResizingSelection Value Object) の作成 [14d0416]
    - [ ] 設計: 元の矩形、変形方向（ハンドル位置）、ドラッグ量をカプセル化。
    - [ ] ロジック実装: Resize(Vector dragDistance, bool keepAspectRatio) の実装。
    - [ ] 不変条件: 最小サイズ (1px) 制約の強制。
    - [ ] テスト: 純粋なC#クラスとしての徹底的な単体テスト。
- [x] Task: 画像変形サービスの抽象化 (IImageResampler) [0c060d5]
    - [ ] インターフェース定義: ドメイン層に変形ロジックの意図のみを定義。
    - [ ] インフラ実装: ニアレストネイバー法による具体的な画像処理の実装。

## フェーズ 2: インタラクションの状態管理とアプリケーション層 (Interaction & App)
ユーザーのドラッグ操作をドメインモデルへのメッセージに変換し、状態遷移を管理します。

- [x] Task: インタラクション集約 (CanvasInteractionSession) の拡張 [e3a9865]
    - [ ] Stateパターン: ResizingState を導入して状態遷移を厳格に管理する。
    - [ ] ハンドル判定: マウス座標がどのハンドルにあるかを判定するドメインロジックの組み込み。
- [x] Task: プレビュー情報の更新 (SelectionPreviewInfo) [e3a9865]
    - [ ] DrawingSession 内で変形中の状態を保持し、不変性を保ちながら更新する仕組みの実装。

## フェーズ 3: プレゼンテーションと視覚的フィードバック (Presentation)
ドメインの状態を View に反映させます。

- [x] Task: ハンドルとプレビューの描画 [bebda75]
    - [ ] Humble ViewModel: ドメイン層から算出されたハンドルの表示座標をそのままViewに公開する。
    - [ ] カーソル制御: ドメインが返すハンドルタイプに基づいてカーソル形状をバインディングする。
- [x] Task: 確定と永続化 [bebda75]
    - [ ] UseCase実装: ConfirmSelectionScaleUseCase を作成し、キャンバス統合と Undo 履歴への記録を実装。
- [x] Task: Conductor - User Manual Verification '拡大縮小機能の基本動作' (Protocol in workflow.md) [bebda75]

## フェーズ 4: 仕上げと洗練
- [x] Task: エッジケースの検証（極小サイズでの挙動など） [bebda75]
- [x] Task: リファクタリング（ドメイン知識の漏洩チェック・知識の返還） [bebda75]
- [x] Task: Conductor - User Manual Verification '最終確認' (Protocol in workflow.md) [bebda75]

## フェーズ 5: ユーザーフィードバック対応
- [x] Task: 選択状態の維持と画質劣化防止 [420c52a]
    - [x] SelectionPreviewState でもハンドルを表示・操作可能にする
    - [x] SelectionPreviewState に元画像を保持させ、画質劣化を防ぐ
- [x] Task: 極小サイズでの操作性改善 [420c52a]
    - [x] SelectionHandleDetector に極小サイズ用のハンドル縮小ロジックを追加
    - [x] 既存テストコードの修正（ドラッグ位置の適正化）

## バグ修正
- [x] Task: バグ修正 - リサイズ確定時に元に戻る問題を修正 [21d856b]
    - [x] InteractionCoordinator.PointerLeftButtonReleased で ResizingState 終了時にもセッションを更新するように修正 [21d856b]
- [~] Task: バグ調査 - 範囲外クリックで確定できない問題
    - [ ] 再現テスト Move_Then_Click_Outside_Commits の作成と実行
