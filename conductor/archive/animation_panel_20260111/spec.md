# Specification: Animation Panel & Sequencer Implementation

## 1. Overview
マルチタブ形式のドット絵エディタ「Eede」において、作成したドット絵のアニメーションを確認・構成するための「アニメーションパネル」および「シーケンサー」を実装する。
本機能は、描画データ（画像）と動きの定義（パターン）を分離し、異なる画像に対して同一の動きを即座に適用・プレビューできることを最大の特徴とする。

## 2. Functional Requirements

### 2.1 UI Layout & Docking (Dock.Avalonia)
*   **Strict Layout Enforcement:**
    *   **Animation Settings & Preview:** 右側のドックエリアに配置。
    *   **Sequencer (Timeline):** 下部のドックエリアに配置。
    *   デフォルトレイアウト設定を変更し、初回起動時またはリセット時にこの配置になるよう構成する。

### 2.2 Animation Library & Data Management
*   **Global Pattern Storage:**
    *   アニメーションパターン（フレーム順序、表示時間など）はアプリケーション全体の設定として保存する。
    *   どのタブ（画像）を開いても、共通の「走り」「待機」などのパターンリストにアクセス可能とする。
*   **Import/Export:**
    *   作成したパターンをJSON/XML形式で外部ファイルとして保存・読み込み可能にする。
*   **Pattern Structure:**
    *   各パターンは以下の情報を持つ：
        *   `Name` (例: "Run", "Idle")
        *   `GridSettings` (Cell Width, Height, Offset, Padding) - パターンごとにグリッド設定を保持する。
        *   `Frames` (Cell Index, Duration) のリスト

### 2.3 Interaction & Editing
*   **Animation Edit State (Mode):**
    *   専用の「アニメーション編集モード」ボタン（またはトグル）を実装。
    *   このモード中は、メインキャンバスのクリック操作が「描画」ではなく「クリックしたセルのシーケンスへの追加」として機能する。
*   **Sequencer Panel:**
    *   登録されたフレームを横並びで表示。
    *   ドラッグ＆ドロップによる順序の入れ替えが可能。
    *   各フレームの表示時間（Duration）を編集可能。
    *   選択したフレームの削除機能。

### 2.4 Real-time Preview
*   **Floating/Docked Preview:**
    *   現在アクティブなタブの画像データと、現在選択中のアニメーションパターンを組み合わせて再生する。
    *   再生コントロール：再生/停止、ループON/OFF、再生速度変更（倍率）。
    *   タブを切り替えた際、プレビュー対象の画像のみを即座に切り替え、再生状態（パターン、現在のフレーム位置）は維持する。

## 3. Technical Implementation

### 3.1 Architecture
*   **State Pattern:**
    *   `Eede.Application.Common.SelectionStates` 名前空間に、新たに `AnimationEditingState` (仮) を追加し、キャンバス上のクリックイベントをハンドリングする。
*   **Data Binding:**
    *   ViewModel (`AnimationViewModel`) と View のバインディングにより、シーケンサーの変更を即座にプレビューに反映させる。

## 4. Acceptance Criteria
*   [ ] 右側に設定/プレビュー、下部にシーケンサーが表示されるデフォルトレイアウトが機能していること。
*   [ ] 「アニメーション編集モード」に切り替え、キャンバス上のセルをクリックしてフレームを追加できること。
*   [ ] 追加したフレームをシーケンサー上で並べ替えられること。
*   [ ] タブAで作成したパターンを維持したままタブBに切り替え、タブBの画像で同じアニメーションがプレビューされること。
*   [ ] パターンをJSONとしてエクスポートし、別の環境（またはリセット後）でインポートして復元できること。
*   [ ] パターンごとに異なるグリッド設定（例：32x32用パターンと16x16用パターン）が正しく切り替わること。

## 5. Out of Scope
*   GIF/APNGとしての画像ファイル書き出し（今回はプレビューとパターン保存まで）。
*   オニオンスキン機能。