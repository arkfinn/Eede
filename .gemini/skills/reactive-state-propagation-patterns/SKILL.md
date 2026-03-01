---
name: Reactive State Propagation Patterns
description: ViewModel の状態変更を、動的に生成される子要素や View 内部の座標判定処理に確実に伝播・同期させるための設計パターン。
---

# Skill: Reactive State Propagation Patterns

## Description
このスキルは、親 ViewModel から動的に生成される子 ViewModel への状態継承、および ViewModel から View 内部フィールド（座標計算や HitTest 用のキャッシュなど）への同期を確実に実装するためのパターンを定義します。

## Key Patterns / Strategies

### 1. Explicit State Inheritance for Dynamic Children
`ObservableCollection` 等で動的に追加された子 ViewModel (例: ドキュメントタブ) は、生成時の「現在の状態」を親から引き継ぐ必要があります。`CollectionChanged` ハンドラなどで明示的にプロパティをセットします。

```csharp
private void SetupChildViewModel(ChildViewModel vm)
{
    // 初期値として現在の親の状態をコピー
    vm.CurrentCursorSize = this.GlobalCursorSize;
    
    // 以降の変更を監視・バインド
    _ = this.WhenAnyValue(x => x.GlobalCursorSize)
           .BindTo(vm, x => x.CurrentCursorSize);
}
```

### 2. View-Internal Field Synchronization
View (UserControl) がパフォーマンスや座標計算のために、ViewModel のプロパティを内部フィールド（例: `_cursorSize`）に保持（キャッシュ）している場合、初期化時（`DataContextChanged`）だけでなく、常にその変更を購読して同期させる必要があります。

```csharp
private void OnDataContextChanged(object? sender, EventArgs e)
{
    _viewModel = DataContext as MyViewModel;
    if (_viewModel == null) return;

    // 初期値の読み込み
    _cursorSize = _viewModel.CursorSize;

    // 動的な変更の監視
    _viewModel.WhenAnyValue(x => x.CursorSize)
        .Subscribe(size =>
        {
            _cursorSize = size;
            UpdateVisuals(); // 描画や判定の更新
        });
}
```

### 3. Verification through Behavior Tests
再発防止のため、以下の 2 つの側面から回帰テストを作成します：
- **ViewModel レベル**: 親のプロパティを変更した後に子を追加し、子がその変更を初期値として持っているか検証する。
- **View レベル**: ViewModel のプロパティを動的に変更した際、View の内部フィールドや描画状態が（ユーザー操作なしで）即座に更新されるかをリフレクション等で検証する。
