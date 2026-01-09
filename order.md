以下は一般的な観点から定義された選択範囲の移動機能です。
これを元に本プロジェクトに適した実装を検討してください。

## 1. 機能の目的とスコープ定義（前提）

### 目的

* ユーザーが選択したピクセル領域を、直感的なドラッグ操作でキャンバス上の別位置へ移動できること

### スコープ

* 対象は **ラスタ画像**
* 単一選択領域（矩形／自由形状は抽象化）
* レイヤーは「1レイヤー前提」または「アクティブレイヤーのみ対象」

---

## 2. 概念モデル（ドメイン定義）

### 主要概念

| 概念             | 説明                         |
| ---------------- | ---------------------------- |
| Selection        | 選択領域の形状・マスク       |
| SelectionContent | 選択領域内のピクセルデータ   |
| DragState        | ドラッグ中かどうか、開始位置 |
| PreviewLayer     | ドラッグ中に表示される仮描画 |
| Commit / Cancel  | 操作確定 or 破棄             |

---

## 3. 状態遷移（重要）

### 状態一覧

```text
Idle
 └─ SelectionExists
      └─ Dragging
           ├─ Commit
           └─ Cancel
```

### 状態遷移条件

| トリガー          | 遷移                       |
| ----------------- | -------------------------- |
| 範囲選択完了      | Idle → SelectionExists     |
| 選択内でMouseDown | SelectionExists → Dragging |
| MouseUp           | Dragging → Commit          |
| ESC / 右クリック  | Dragging → Cancel          |

---

## 4. 操作フロー詳細（UIイベント単位）

### ① MouseDown（選択内）

* ヒットテスト：

  * **「選択マスク内か」**をピクセル精度 or 境界精度で判定
* 成功時：

  * ドラッグ開始点を記録
  * 選択範囲のピクセルを **一時バッファへコピー**
  * 元の位置は「消去しない」（※重要）

```text
dragStartPos = mousePos
originalSelectionPos = selection.bounds
selectionPixels = extractPixels(selection)
```

---

### ② MouseMove（ドラッグ中）

#### 処理方針（ベストプラクティス）

* **キャンバスを直接書き換えない**
* プレビュー専用レイヤーに描画

#### 実装要点

* 移動量 = currentMousePos - dragStartPos
* 仮表示は「半透明」「境界線付き」が一般的

```text
delta = mousePos - dragStartPos
previewPosition = originalSelectionPos + delta
```

---

### ③ MouseUp（確定）

#### 処理手順

1. 元の選択領域を消去
2. 新しい位置にピクセルを書き込み
3. 選択範囲を新位置へ移動
4. 履歴（Undo）に1操作として登録

```text
clearPixels(originalSelectionArea)
drawPixels(selectionPixels, newPosition)
updateSelection(newPosition)
```

---

### ④ Cancel（ESC等）

* プレビュー破棄
* 元画像は **一切変更しない**
* 状態を SelectionExists に戻す

---

## 5. Undo / Redo 設計（必須）

### ベストプラクティス

* 「移動」は **1つのコマンド**
* 差分は以下を保持

```text
before:
 - originalPosition
 - originalPixels
after:
 - newPosition
 - movedPixels
```

👉 Command Pattern / Memento Pattern が定番

---

## 6. 選択範囲の扱い（重要設計ポイント）

### よくある設計ミス

* 選択＝矩形と決め打ち
* ピクセルとマスクを分離しない

### 推奨モデル

```text
Selection {
  mask: Bitmask / AlphaMask
  bounds: Rect
}
```

* ピクセル操作は必ず **maskを掛ける**
* 自由選択／矩形選択を同一ロジックで扱える

---

## 7. UXベストプラクティス

### 視覚フィードバック

* ドラッグ中：

  * 透明度を含めて移動する（DirectimageTransferを利用）
  * 元の位置に「ゴースト」を残さない
* カーソル変更：

  * 移動可能時：Moveカーソル

### 操作補助

* Shift：水平／垂直固定
* Alt：コピー移動（元を消さない）


---

## 9. AIに渡すための「実装指示用 要約」

以下のようにまとめると、AIが誤実装しにくくなります。

> * 選択領域はマスクとして保持する
> * ドラッグ開始時に選択ピクセルを一時バッファへコピー
> * ドラッグ中はキャンバスを書き換えず、プレビュー用レイヤーに描画
> * MouseUpで元領域を消去し、新位置へ描画して1Undoとして確定
> * ESCでキャンセル可能
> * Undo/Redoは移動操作を1コマンドとして扱う

---
