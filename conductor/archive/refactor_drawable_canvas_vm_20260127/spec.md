# 概要
`DrawableCanvasViewModel` は現在、表示計算、入力ハンドリング、ドメイン操作の3つの責務が混在し、肥大化（God Class化）している。本トラックでは、レガシーコード改善の観点から「Humble Object」パターンを適用し、表示用計算ロジックを分離・純粋化することで、テスト可能性の向上とコードの可読性改善を図る。

# 機能要件
1. **表示計算ロジックの抽出**:
   - `Magnification` に基づく `Thickness` や `Size` の計算ロジックを `CanvasViewCalculator`（仮称）などの純粋なC#クラスへ移動する。
   - `SelectingArea` から `SelectingThickness` / `SelectingSize` を求める計算を分離する。
   - `PreviewPosition` / `PreviewPixels` から `PreviewThickness` / `PreviewSize` 等を求める計算を分離する。
2. **Avalonia依存の最小化**:
   - ViewModel本体が直接 `Thickness` を計算して保持するのではなく、計算クラスを通じて提供、またはView側での変換を検討する。
3. **副作用の整理**:
   - `WhenAnyValue` による複雑な計算フローを整理し、入力に対する計算結果が予測可能な状態にする。

# 非機能要件
- **テスト可能性**: 抽出された計算ロジックに対し、Avaloniaを起動せずに実行可能な単体テストを実装する。
- **後方互換性**: 既存の描画機能、選択機能の挙動を変更しない（リファクタリングに徹する）。

# 受入基準
- [ ] 表示用計算ロジックが独立したクラスに抽出されている。
- [ ] `DrawableCanvasViewModel` から `Thickness` 計算などのロジックが消去されている。
- [ ] 新設された計算クラスに対して単体テストがパスしている。
- [ ] 既存のキャンバス表示（ズーム、選択範囲表示、プレビュー表示）が正しく動作し続けている。

# アウトオブスコープ
- マウス操作ロジック（`ISelectionState` 周り）の抜本的な再設計（次フェーズ以降とする）。
- ドメイン層（`Picture`, `DrawingBuffer`）のロジック変更。
