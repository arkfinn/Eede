# DockPictureViewModel.cs リファクタリングプラン

## 1. Saveメソッドのリファクタリング

*   `Save` メソッドをアプリケーション層に移動する。
*   `DockPictureViewModel` は、画像の保存処理をアプリケーション層に委譲する。
*   アプリケーション層は、`BitmapFile` を使用して画像の保存処理を行う。

## 2. Initializeメソッドのリファクタリング

*   `Initialize` メソッドをアプリケーション層に移動する。
*   `DockPictureViewModel` は、`BitmapFile` から `Picture` への変換処理をアプリケーション層に委譲する。
*   アプリケーション層は、`PictureBitmapAdapter` を使用して `BitmapFile` から `Picture` への変換処理を行う。

## 3. BringPictureBufferメソッドの見直し

*   `BringPictureBuffer` メソッドの処理内容を確認し、本当に必要な処理なのか判断する。
*   もし、常に新しい `Picture` を作成する必要がない場合は、既存の `Picture` を再利用するように変更する。
*   もし、`BringPictureBuffer` メソッドが不要な場合は、削除する。

## 4. その他

*   `DockPictureViewModel` の責務を明確化し、UIの状態管理に特化させる。


## 1. Saveメソッドのリファクタリング (修正)

*   `Save` メソッドをアプリケーション層に移動するのではなく、アプリケーション層に画像の保存処理を行うユースケースを作成する。
*   `DockPictureViewModel` は、画像の保存処理を行うユースケースを呼び出す。
*   ユースケースは、`BitmapFile` を引数として受け取り、画像の保存処理を行う。
*   `BitmapFile` は、プレゼンテーション層に留める。

## 2. Initializeメソッドのリファクタリング (修正)

*   `Initialize` メソッドをアプリケーション層に移動するのではなく、アプリケーション層に `BitmapFile` から `Picture` への変換処理を行うユースケースを作成する。
*   `DockPictureViewModel` は、`BitmapFile` から `Picture` への変換処理を行うユースケースを呼び出す。
*   ユースケースは、`BitmapFile` を引数として受け取り、`Picture` を返す。
*   `BitmapFile` は、プレゼンテーション層に留める。

## 3. BitmapFileの抽象化

*   `BitmapFile` がAvaloniaに依存しているため、`IBitmapFile` というインターフェースを作成し、`BitmapFile` がそのインターフェースを実装するように変更する。
*   アプリケーション層は、`IBitmapFile` インターフェースを通して `BitmapFile` を操作する。
*   これにより、アプリケーション層はAvaloniaに依存しなくなる。


## 実装ステップ

1.  **IBitmapFileインターフェースの定義:**
    *   `Eede.Presentation` プロジェクトに `IBitmapFile` インターフェースを作成します。
    *   `IBitmapFile` インターフェースは、`BitmapFile` クラスに必要なメソッド（画像の保存、読み込みなど）を定義します。
2.  **BitmapFileクラスの実装変更:**
    *   `BitmapFile` クラスが `IBitmapFile` インターフェースを実装するように変更します。
3.  **ユースケースの作成:**
    *   `Eede.Application` プロジェクトに、画像の保存と読み込みを行うユースケースを作成します。
    *   これらのユースケースは、`IBitmapFile` インターフェースを引数として受け取るようにします。
4.  **DockPictureViewModelの変更:**
    *   `DockPictureViewModel` が、作成したユースケースを使用するように変更します。
    *   `DockPictureViewModel` は、`BitmapFile` クラスを直接使用するのではなく、`IBitmapFile` インターフェースを通して操作するようにします。
5.  **テストの作成:**
    *   作成したユースケースのテストを作成します。
    *   テストは、`IBitmapFile` インターフェースを実装したモックオブジェクトを使用して行うようにします。
6.  **動作確認:**
    *   リファクタリング後のコードが正常に動作することを確認します。
    *   画像の保存、読み込み、表示などの機能が正常に動作することを確認します。
*   アプリケーションロジックは、アプリケーション層に移動する。
