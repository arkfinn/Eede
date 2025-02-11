# Eede ビジネスロジックフロー図

## 1. 画像編集の基本フロー

```mermaid
graph TB
    Start[開始] --> Load[画像ロード]
    Load --> Initialize[編集環境の初期化]
    Initialize --> EditLoop[編集ループ]
    EditLoop --> |ユーザー操作| Operation{操作種別}
    Operation --> |描画| Drawing[描画処理]
    Operation --> |レイヤー操作| Layer[レイヤー処理]
    Operation --> |色選択| Color[色処理]
    Drawing --> Update[画面更新]
    Layer --> Update
    Color --> Update
    Update --> |継続| EditLoop
    Update --> |終了| Save[保存処理]
    Save --> End[終了]
```

## 2. 描画処理フロー

```mermaid
graph TB
    Input[マウス/ペン入力] --> Style{描画スタイル}
    Style --> |フリーハンド| Free[FreeCurve処理]
    Style --> |直線| Line[Line処理]
    Style --> |塗りつぶし| Fill[Fill処理]
    Free --> |位置計算| Position[Position計算]
    Line --> |位置計算| Position
    Fill --> |領域計算| Region[Region計算]
    Position --> Buffer[描画バッファー更新]
    Region --> Buffer
    Buffer --> Blend[画像ブレンド処理]
    Blend --> Display[画面表示]
```

## 3. レイヤー管理フロー

```mermaid
graph TB
    Layer[レイヤー操作] --> Type{操作種別}
    Type --> |新規作成| Create[レイヤー作成]
    Type --> |選択| Select[レイヤー選択]
    Type --> |合成モード変更| Mode[合成モード設定]
    Create --> Update[状態更新]
    Select --> Update
    Mode --> Update
    Update --> Composite[レイヤー合成]
    Composite --> Display[画面表示]
```

## 4. アンドゥ/リドゥ処理フロー

```mermaid
graph TB
    Action[編集操作] --> History[履歴スタック追加]
    History --> |アンドゥ| Undo[状態復元]
    History --> |リドゥ| Redo[操作再適用]
    Undo --> Update[画面更新]
    Redo --> Update
```

## 5. 画像保存フロー

```mermaid
graph TB
    Save[保存処理開始] --> Format{フォーマット選択}
    Format --> |PNG| PNG[PNG エンコード]
    Format --> |BMP| BMP[BMP エンコード]
    PNG --> Quality[品質設定]
    BMP --> Direct[直接保存]
    Quality --> Write[ファイル書き込み]
    Direct --> Write
    Write --> Complete[完了]
```

## 6. カラーパレット操作フロー

```mermaid
graph TB
    Color[カラー操作] --> Source{ソース選択}
    Source --> |パレット| Palette[パレット選択]
    Source --> |カラーピッカー| Picker[色選択UI]
    Source --> |スポイト| Dropper[画像から取得]
    Palette --> Convert[色空間変換]
    Picker --> Convert
    Dropper --> Convert
    Convert --> Apply[描画色設定]
```

## 7. データの依存関係

```mermaid
graph LR
    UI[ユーザーインターフェース] --> VM[ViewModel]
    VM --> UC[UseCase]
    UC --> Domain[ドメインモデル]
    Domain --> Infra[インフラストラクチャ]
```

## 8. エラーハンドリングフロー

```mermaid
graph TB
    Operation[操作実行] --> Try{try処理}
    Try --> |成功| Success[処理完了]
    Try --> |失敗| Catch[例外捕捉]
    Catch --> Type{例外種別}
    Type --> |I/O| IO[I/Oエラー処理]
    Type --> |メモリ| Memory[メモリエラー処理]
    Type --> |その他| Other[その他エラー処理]
    IO --> Recover[リカバリー処理]
    Memory --> Recover
    Other --> Recover
    Recover --> Complete[完了]
```

## 9. パフォーマンス最適化フロー

```mermaid
graph TB
    Start[処理開始] --> Size{データサイズ}
    Size --> |小| Direct[直接処理]
    Size --> |大| Async[非同期処理]
    Direct --> Complete[完了]
    Async --> Thread[スレッドプール]
    Thread --> Progress[進捗更新]
    Progress --> |継続| Thread
    Progress --> |完了| Complete
```

## 注意事項

1. 各フローは簡略化されており、実際の実装ではより詳細な処理が含まれます。
2. エラーハンドリングはすべてのフローに組み込まれていますが、図の簡略化のため省略されている場合があります。
3. パフォーマンス最適化は、とくに大きな画像を扱う際に重要となります。
4. ユーザー操作に対するレスポンス性を確保するため、重い処理は可能な限り非同期で実行されます。

## 更新履歴

このドキュメントは、新機能の追加や処理フローの変更に応じて更新されます。更新の際は、関連するテストケースの見直しも必要です。
