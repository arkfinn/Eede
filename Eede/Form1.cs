using Eede.Actions;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Common.Pictures.Actions;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Pictures.Actions;
using Eede.Domain.Scales;
using Eede.Domain.Systems;
using Eede.Infrastructure.Pictures;
using Eede.Settings;
using Eede.Ui;
using Reactive.Bindings;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace Eede
{
    internal partial class Form1 : Form, IViewFor<Form1ViewModel>
    {
        public Form1ViewModel ViewModel
        {
            get; set;
        } = new Form1ViewModel();

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as Form1ViewModel; }
        }

        public Form1()
        {
            InitializeComponent();

            Bind(() => paintableBox1.Magnification, () => ViewModel.Magnification.Value);

            Bind(() => paintableBox1.PenColor, () => ViewModel.PenColor.Value);
            Bind(() => colorPicker1.NowColor, () => ViewModel.PenColor.Value);


            // 暫定
            drawStyleMenu1 = new();
            drawStyleMenu1.Dock = DockStyle.Top;
            splitContainer2.Panel1.Controls.Add(drawStyleMenu1);
            drawStyleMenu1.Element.DrawStyleChanged += (value) =>
            {
                ViewModel.DrawStyle = value;
                paintableBox1.DrawStyle = value;
            };

            // pictureActionMenu1.Command = ViewModel.PictureActionCommand;
            ReactiveCommand<PictureActions> pictureActionCommand = new();
            pictureActionCommand.Subscribe(new Action<PictureActions>(actionType =>
            {
                Picture nowPicture = paintableBox1.GetImage();
                switch (actionType)
                {
                    case PictureActions.ShiftUp:
                        Picture updatedPicture = new ShiftUpAction(nowPicture).Execute();
                        PullPictureAction action = new(p => paintableBox1.SetupPicture(p), nowPicture, updatedPicture);
                        action.Do();
                        AddUndoItem(action);
                        break;
                    case PictureActions.ShiftDown:
                        break;
                    case PictureActions.ShiftLeft:
                        break;
                    case PictureActions.ShiftRight:
                        break;
                    default:
                        break;
                }
            }));
            pictureActionMenu1.Command = pictureActionCommand;

            penWidthSelector1 = new();
            penWidthSelector1.Dock = DockStyle.Bottom;
            splitContainer2.Panel1.Controls.Add(penWidthSelector1);
            penWidthSelector1.Element.PenWidthChanged += (width) =>
            {
                ViewModel.PenSize = width;
                paintableBox1.PenSize = width;
            };

            toolStripButton14.PerformClick();
        }
        private Ui.AvaloniaWrapper.Navigation.DrawStyleMenu drawStyleMenu1;
        private Ui.AvaloniaWrapper.DataDisplay.PenWidthSelector penWidthSelector1;


        private static void Bind<T, U>(Expression<Func<T>> targetValue, Expression<Func<U>> modelValue)
        {
            static Tuple<object, string> ResolveLambda<V>(Expression<Func<V>> expression)
            {
                var lambda = expression as LambdaExpression ?? throw new ArgumentException("lambda");
                var property = lambda.Body as MemberExpression ?? throw new ArgumentException("property");
                var parent = property.Expression;
                // (Source, Name)
                return new Tuple<object, string>(Expression.Lambda(parent).Compile().DynamicInvoke(), property.Member.Name);
            }
            var tuple1 = ResolveLambda(targetValue);
            var tuple2 = ResolveLambda(modelValue);
            var control = tuple1.Item1 as Control ?? throw new ArgumentException("control");
            control.DataBindings.Add(new Binding(tuple1.Item2, tuple2.Item1, tuple2.Item2, true, DataSourceUpdateMode.OnPropertyChanged));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CreateNewPicture();
        }

        private void CreateNewPicture()
        {
            using SizeSelectDialog dialog = new();
            _ = dialog.ShowDialog();
            if (dialog.DialogResult != DialogResult.OK)
            {
                return;
            }
            System.Drawing.Size size = dialog.PictureSize;
            AddChildWindow(new CreatePictureUseCase().Execute(size));
        }

        private void OpenPictureFromDialog()
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            OpenPicture(new FilePath(openFileDialog1.FileName));
        }

        private void OpenPicture(FilePath filename)
        {
            try
            {
                Picture picture = new PictureFileReader(filename).Read();
                AddChildWindow(new PictureFile(filename, picture));
            }
            catch (Exception)
            {
                _ = MessageBox.Show("ファイルの読み込みに失敗しました");
            }
        }

        private void AddChildWindow(PictureFile file)
        {
            PictureWindow form = new(file.FilePath, file.Picture, paintableBox1)
            {
                MdiParent = this
            };
            form.FormClosed += new FormClosedEventHandler(ChildFormClosed);
            form.PicturePulled += new EventHandler<PicturePulledEventArgs>(ChildFormPicturePulled);
            form.PicturePushed += new EventHandler<PicturePushedEventArgs>(ChildFormPicturePushed);
            form.Show();
            toolStripButton_saveFile.Enabled = true;
        }

        private void ChildFormClosed(object sender, FormClosedEventArgs e)
        {
            if (MdiChildren.Length > 1)
            {
                return;
            }

            toolStripButton_saveFile.Enabled = false;
        }

        private void ChildFormPicturePulled(object sender, PicturePulledEventArgs e)
        {
            Picture picture = e.CutOutImage();
            PullPictureAction action = new(p => paintableBox1.SetupPicture(p), paintableBox1.GetImage(), picture);
            action.Do();
            AddUndoItem(action);
        }

        private void ChildFormPicturePushed(object sender, PicturePushedEventArgs e)
        {
            if (sender is PictureWindow)
            {
                PictureWindow window = sender as PictureWindow;
                Picture src = paintableBox1.GetImage();

                PushPictureAction action = new(window, e.Picture, src, PrepareImageBlender(), e.Position);
                action.Do();
                AddUndoItem(action);

            }
        }

        private IImageBlender PrepareImageBlender()
        {
            return alphaTransferButton.Checked ? new AlphaImageBlender() : new DirectImageBlender();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild is not PictureWindow child)
            {
                return;
            }

            if (child.IsEmptyFileName() && !RenameChildFile(child))
            {
                return;
            }

            child.Save();
        }

        private bool RenameChildFile(PictureWindow child)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return false;
            }
            child.Rename(new FilePath(saveFileDialog1.FileName));
            return true;
        }

        private void toolStripButton_openFile_Click(object sender, EventArgs e)
        {
            OpenPictureFromDialog();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string filename in files)
            {
                OpenPicture(new FilePath(filename));
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            ViewModel.Magnification.Value = new Magnification(1);
            toolStripButton12.Checked = true;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            ViewModel.Magnification.Value = new Magnification(2);
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = true;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            ViewModel.Magnification.Value = new Magnification(4);
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = true;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            ViewModel.Magnification.Value = new Magnification(6);
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = true;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            ViewModel.Magnification.Value = new Magnification(8);
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = true;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            ViewModel.Magnification.Value = new Magnification(12);
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = true;
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            paintableBox1.ChangeImageTransfer(new RGBToneImageTransfer());
            paintableBox1.ChangeImageBlender(new RGBOnlyImageBlender());
            toolStripButton9.Checked = true;
            toolStripButton10.Checked = false;
            toolStripButton11.Checked = false;
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            paintableBox1.ChangeImageTransfer(new AlphaToneImageTransfer());
            paintableBox1.ChangeImageBlender(new AlphaOnlyImageBlender());
            toolStripButton9.Checked = false;
            toolStripButton10.Checked = true;
            toolStripButton11.Checked = false;
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            paintableBox1.ChangeImageTransfer(new DirectImageTransfer());
            paintableBox1.ChangeImageBlender(new DirectImageBlender());
            toolStripButton9.Checked = false;
            toolStripButton10.Checked = false;
            toolStripButton11.Checked = true;
        }
        private void colorPicker1_ColorChanged(object sender, EventArgs e)
        {
            ViewModel.PenColor.Value = colorPicker1.NowColor;
        }

        private void paintableBox1_ColorChanged(object sender, EventArgs e)
        {
            ViewModel.PenColor.Value = paintableBox1.PenColor;
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            using BoxSizeSettingDialog dialog = new();
            dialog.SetBoxSize(GlobalSetting.Instance().BoxSize);
            _ = dialog.ShowDialog();
            if (dialog.DialogResult != DialogResult.OK)
            {
                return;
            }
            GlobalSetting.Instance().BoxSize = dialog.GetInputBoxSize();
        }

        #region Undo

        private UndoSystem UndoSystem = new();


        private void AddUndoItem(IUndoItem item)
        {
            UndoSystem = UndoSystem.Add(item);
            UpdateUndoButtonEnabled();
        }

        private void UpdateUndoButtonEnabled()
        {
            toolStripButtonUndo.Enabled = UndoSystem.CanUndo();
            toolStripButtonRedo.Enabled = UndoSystem.CanRedo();
        }

        private void toolStripButtonUndo_Click(object sender, EventArgs e)
        {
            UndoSystem = UndoSystem.Undo();
            UpdateUndoButtonEnabled();
        }

        private void toolStripButtonRedo_Click(object sender, EventArgs e)
        {
            UndoSystem = UndoSystem.Redo();
            UpdateUndoButtonEnabled();
        }

        #endregion Undo

        private void paintableBox1_Drew(object sender, Application.Drawings.DrawEventArgs e)
        {
            DrawAction action = new(paintableBox1, e.PreviousPicture, e.NowPicture);
            AddUndoItem(action);
        }

    }
}