---
name: ddd-knowledge-repatriation
description: 手続き的なサービスに吸い出されたドメイン知識を、Value Object や Entity へ「返却」し、ドメインモデルを豊かにする（貧血症の改善）設計パターン。
---

# Skill: DDD Knowledge Repatriation (ドメイン知識の返却)

## Description
「ドメインモデル貧血症」の典型的な症状である、状態を持たない手続き的なサービスクラスを解体し、本来その知識を持つべきオブジェクトにロジックを再配置する手法。特に、計算、スケーリング、変換、不変条件のチェックなどをドメインオブジェクト自身に行わせることで、型安全性とカプセル化を最大化します。

## Key Patterns / Strategies

### 1. 空間の型による分離 (Spatial Type Separation)
異なる文脈（例：UI表示上の座標とドメインピクセル座標）を別々の型（Value Object）として定義し、その相互変換ロジックを型自身に持たせるパターン。

```csharp
// UI空間の座標
public readonly record struct DisplayCoordinate(int X, int Y) {
    // 変換知識をVO自身が持つ
    public CanvasCoordinate ToCanvas(Magnification mag) {
        return new CanvasCoordinate(mag.Minify(X), mag.Minify(Y));
    }
}

// ドメイン空間の座標
public readonly record struct CanvasCoordinate(int X, int Y);
```

### 2. 「便利屋」サービスの解体プロセス
1.  **引数の観察**: サービスメソッドが頻繁に特定のVOを引数に取っている場合、そのメソッドはそのVOのメソッドであるべきです。
2.  **ロジックの移動**: サービス内の計算式をVOへ移動し、サービスは単にVOのメソッドを呼ぶだけにします。
3.  **サービスの消滅**: サービスが「指示（UseCase）」以外の責任を持たなくなったら、そのサービスを廃止し、Application層のUseCaseへ昇格させるか消去します。

### 3. 不変条件の強制 (State Guard in Aggregates)
集約（Aggregate）のメソッドは、データ更新だけでなく「オブジェクトが常に正しい状態にあること」を保証します。

```csharp
public DrawingSession Push(Picture nextPicture) {
    // 単なるデータ比較だけでなく、状態（描画中かどうか）のリセットも「確定」の責務
    if (nextPicture == Buffer.Previous && !IsDrawing()) return this;
    return new DrawingSession(new DrawingBuffer(nextPicture), ...);
}
```
