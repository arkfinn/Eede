# Implementation Plan: Legacy-Aware Codebase Refactoring

## Phase 1: Assessment and Safety Lockdown
リファクタリングの安全性を確保するための準備と、ターゲットの精密な分析を行います。

- [x] Task: Gitステータスの確認（クリーンな状態であることの保証） 885218b
- [ ] Task: 巨大クラスおよび設計上の問題があるクラスの特定
    - ターゲット候補: `Eede.Presentation` 内の ViewModel (500行超を優先)
- [ ] Task: **【安全性】ターゲットに対する仕様化テスト（Golden Master）の作成**
    - 既存の挙動（バグを含む）をブラックボックスとして記録し、リファクタリングによる予期せぬ変化を検知可能にする
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Assessment and Safety Lockdown' (Protocol in workflow.md)

## Phase 2: Domain Discovery and Extraction
ViewModel等の巨大クラスから、ビジネスロジックとドメイン知識を分離します。

- [ ] Task: ドメイン探索: クラス内のロジックを「純粋な計算」と「I/O・状態変更」に分類
- [ ] Task: Value Object / Entity の抽出
    - 意味のある型を定義し、プリミティブ執着（Primitive Obsession）を排除
- [ ] Task: ロジックの Domain/Application 層への移動
    - `refactor-ddd-onion-complete` に基づき、依存関係を整理
- [ ] Task: 古典派アプローチによる新規コンポーネントのユニットテスト作成
    - 実装詳細ではなく振る舞いをテストし、リファクタリング耐性を最大化する
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Domain Discovery and Extraction' (Protocol in workflow.md)

## Phase 3: Structural Refinement (Presentation & Infrastructure)
Presentation層を薄く保ち、層間の結合度を下げます。

- [ ] Task: `refactor-five-lines-strategy` を用いた ViewModel のメソッド整理
- [ ] Task: 依存性逆転の原則（DIP）の適用
    - Infrastructure層への依存をインターフェース経由に抽象化
- [ ] Task: 仕様化テスト（Phase 1で作成）の実行と、リファクタリング後の整合性確認
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Structural Refinement' (Protocol in workflow.md)

## Phase 4: Verification and Consolidation
最終的な品質確認と、得られた知見のドキュメント化を行います。

- [ ] Task: 全テストスイートの実行とカバレッジ（目標80%）の確認
- [ ] Task: 不要になった古い実装の完全削除
- [ ] Task: プロジェクト全体のビルドと手動動作確認
- [ ] Task: /learn プロトコルの実行と、設計原則（ADR等）の更新
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Verification and Consolidation' (Protocol in workflow.md)
