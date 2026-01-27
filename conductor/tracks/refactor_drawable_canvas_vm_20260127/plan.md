# Implementation Plan: DrawableCanvasViewModel の責務分離とドメイン知識の返却

## Phase 0: セーフティネットの構築と現状確認
リファクタリングの影響を最小限に抑えるため、既存の挙動を保護するテストを整備します。

- [x] Task: 既存の `DrawableCanvasViewModel` に対する仕様化テストの追加 [7e84a2b]
    - 自由線描画、範囲選択、座標変換の現状の挙動をテストコードで固定する。
- [ ] Task: Conductor - User Manual Verification 'Phase 0' (Protocol in workflow.md)

## Phase 1: 空間 Value Object の抽出と座標変換の隠蔽 [checkpoint: 92be9e1]
「空間の型による分離」パターンを適用し、手動の座標計算を排除します。

- [x] Task: `CanvasCoordinate` (ピクセル単位) と `DisplayCoordinate` (UI単位) Value Object の作成 [3c6a01d]
    - `Eede.Domain.ImageEditing` または `SharedKernel` に配置。
    - `Magnification` を受け取って相互変換するメソッドを実装。
- [x] Task: ViewModel 内の座標計算を新 VO へ移行 [5e3789b]
    - `DrawableCanvasViewModel` 内の `Magnification.Magnify` 等の直接呼び出しを VO のメソッドに置き換える。
- [x] Task: Conductor - User Manual Verification 'Phase 1' (Protocol in workflow.md)

## Phase 2: 編集ユースケースの抽出 (Application層)
手続き的な編集コマンドを UseCase へ移動し、1クラス1メソッド化します。

- [ ] Task: `CopySelectionUseCase` の実装とテスト
    - `Eede.Application.Pictures.UseCase` に配置。
- [ ] Task: `CutSelectionUseCase` の実装とテスト
- [ ] Task: `PasteFromClipboardUseCase` の実装とテスト
- [ ] Task: ViewModel のコマンドから各 UseCase を呼び出すように変更
- [ ] Task: Conductor - User Manual Verification 'Phase 2' (Protocol in workflow.md)

## Phase 3: 相互作用ロジックのドメイン返却 (Domain層)
マウスイベントと状態遷移をドメイン集約へ移動します。

- [ ] Task: `CanvasInteractionSession` 集約の設計と実装
    - `_selectionState` の遷移ロジックをこの集約内に閉じ込める。
    - 入力: `CanvasCoordinate`, `PointerEvent` -> 出力: `UpdatedPicture`, `NextSession`, `CursorType`.
- [ ] Task: `DrawableCanvasViewModel` のイベントハンドラのリファクタリング
    - イベントを `CanvasInteractionSession` に委譲し、結果をプロパティに反映するだけにする。
- [ ] Task: `UpdateImage` メソッドの整理
    - プレビュー合成ロジックを整理し、必要に応じて Domain モデル側へ寄せる。
- [ ] Task: Conductor - User Manual Verification 'Phase 3' (Protocol in workflow.md)
