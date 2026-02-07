# Implementation Plan: ゼロクローン変換

## フェーズ 1: 現状の振る舞い固定（仕様化テスト）
リファクタリング前の正しい挙動をテストコードで記録し、デグレードを防止します。

- [~] Task: 仕様化テストの作成
    - [ ] `Picture` クラスのピクセルアクセス（PickColor, CutOut）の正確性を検証するテストを `Eede.Domain.Tests` に追加。
    - [ ] 既存の `IBitmapAdapter` の動作を検証するテストを `Eede.Presentation.Tests` に追加。

## フェーズ 2: Picture ドメインモデルのリファクタリング
... (省略)