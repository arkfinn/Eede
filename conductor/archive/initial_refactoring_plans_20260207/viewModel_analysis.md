# ViewModel 分析結果

## Eede.Presentation/ViewModels/Pages/MainViewModel.cs

*   [ ] アプリケーションロジックやドメインロジックを含んでいる可能性がある
*   [ ] UIの状態管理以外の責務を持っている可能性がある

## Eede.Presentation/ViewModels/DataEntry/DrawableCanvasViewModel.cs

*   [ ] ドメインロジック（描画処理など）を含んでいる可能性がある
*   [ ] UIの状態管理以外の責務を持っている可能性がある

## Eede.Presentation/ViewModels/DataDisplay/DockPictureViewModel.cs

*   [ ] アプリケーションロジック（画像の表示処理など）を含んでいる可能性がある
*   [ ] UIの状態管理以外の責務を持っている可能性がある

## 全体的な課題

*   ViewModelがUIの状態管理だけでなく、アプリケーションロジックやドメインロジックまで含んでいる場合、責務が過多になっている可能性がある。
*   ViewModelの責務を明確化し、UIの状態管理に特化させる必要がある。


## Eede.Presentation/ViewModels/Pages/MainViewModel.cs の詳細分析

*   `MainViewModel` は、複数の `DockPictureViewModel` を管理し、`DrawableCanvasViewModel` と連携しています。また、Undo/Redoのコマンドや、ファイルのLoad/Saveコマンド、パレットの操作コマンドなど、アプリケーション全体の操作を管理しています。
*   これらの責務は、UIの状態管理だけでなく、アプリケーションロジックやドメインロジックまで含んでいる可能性があり、責務が過多になっていると考えられます。
*   特に、`ExecuteLoadPicture`、`ExecuteSavePicture`、`OnPushToDrawArea`、`OnPullFromDrawArea`、`ExecutePictureAction` などのメソッドは、アプリケーション層やドメイン層に移動することを検討する必要があります。

## Eede.Presentation/ViewModels/DataEntry/DrawableCanvasViewModel.cs の詳細分析

*   `DrawableCanvasViewModel` は、描画領域の状態を管理し、描画処理を `DrawableArea` に委譲しています。また、ペンの色や太さ、描画スタイルなどの設定を管理しています。
*   これらの責務は、UIの状態管理だけでなく、ドメインロジック（描画処理）まで含んでいる可能性があり、責務が過多になっていると考えられます。
*   特に、`ExecuteDrawBeginAction`、`ExecutePonterRightButtonPressedAction`、`ExecuteDrawingAction`、`ExecuteDrawEndAction` などのメソッドは、ドメイン層に移動することを検討する必要があります。

## Eede.Presentation/ViewModels/DataDisplay/DockPictureViewModel.cs の詳細分析

*   `DockPictureViewModel` は、個々の画像の表示状態を管理し、画像のLoad/Save処理を `BitmapFile` に委譲しています。また、画像のPush/Pull処理を `PictureEditingUseCase` に委譲しています。
*   これらの責務は、UIの状態管理だけでなく、アプリケーションロジック（画像のLoad/Save処理）まで含んでいる可能性があり、責務が過多になっていると考えられます。
*   特に、`Save` メソッドは、アプリケーション層に移動することを検討する必要があります。
*   アプリケーションロジックやドメインロジックは、アプリケーション層やドメイン層に移動する必要がある。
