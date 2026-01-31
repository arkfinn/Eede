# Specification: DockPictureViewModel Refactoring (DDD & Safety Focused)

## Overview
`DockPictureViewModel` に集中しているアプリケーションロジックとインフラ依存を整理し、関心の分離を徹底します。DDDの原則に基づきドメイン知識をモデルへ返却し、安全なリファクタリング手法を用いて既存機能を破壊せずに構造を改善します。

## Goals
1.  **安全性**: 仕様化テストによる既存挙動の保護。
2.  **アーキテクチャの適正化**: `IPictureRepository`（ドメイン層）の導入によるDIPの実現と、Application層（UseCase）へのロジック移動。
3.  **ドメインモデルの深化**: 貧血症の解消（ロジックをエンティティ/VOへ移動）。
4.  **可読性**: 「5行ルール」の適用による徹底した簡素化。

## Success Criteria
-   `DockPictureViewModel` が UI 状態管理に特化している。
-   保存・読み込みロジックが `Eede.Application` の UseCase に集約されている。
-   ドメインルールが `Eede.Domain` の適切なオブジェクトにカプセル化されている。
-   全ての新設コードにテストが存在し、回帰テストをパスする。