using Eede.Actions;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.DrawStyles;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Systems;
using Eede.Infrastructure.Pictures;
using Eede.Settings;
using Eede.Ui;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eede
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            toolStripButton14.PerformClick();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void splitContainer1_Panel2_Paint_1(object sender, PaintEventArgs e)
        {
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
            Size size = dialog.PictureSize;
            // TODO: pictureがDisposeされないが、別の修正でそもそもpictureからIDisposableを取る
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
            PullPictureAction action = new(paintableBox1, picture);
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
            paintableBox1.Magnification = 1;
            toolStripButton12.Checked = true;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            paintableBox1.Magnification = 2;
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = true;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            paintableBox1.Magnification = 4;
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = true;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            paintableBox1.Magnification = 6;
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = true;
            toolStripButton16.Checked = false;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            paintableBox1.Magnification = 8;
            toolStripButton12.Checked = false;
            toolStripButton13.Checked = false;
            toolStripButton14.Checked = false;
            toolStripButton15.Checked = false;
            toolStripButton16.Checked = true;
            toolStripButton17.Checked = false;
        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            paintableBox1.Magnification = 12;
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
            paintableBox1.PenColor = colorPicker1.GetArgb();
        }

        private void penSizeSetter1_PenSizeChanged(object sender, EventArgs e)
        {
            paintableBox1.PenSize = penSizeSetter1.PenSize;
        }

        private void paintableBox1_ColorChanged(object sender, EventArgs e)
        {
            colorPicker1.SetColor(paintableBox1.PenColor);
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

        private void freeCurveToolButton_Click(object sender, EventArgs e)
        {
            paintableBox1.DrawStyle = new FreeCurve();
            freeCurveToolButton.Checked = true;
            lineToolButton.Checked = false;
            fillToolButton.Checked = false;
        }

        private void lineToolButton_Click(object sender, EventArgs e)
        {
            paintableBox1.DrawStyle = new Line();
            freeCurveToolButton.Checked = false;
            lineToolButton.Checked = true;
            fillToolButton.Checked = false;

        }

        private void fillToolButton_Click(object sender, EventArgs e)
        {
            paintableBox1.DrawStyle = new Fill();
            freeCurveToolButton.Checked = false;
            lineToolButton.Checked = false;
            fillToolButton.Checked = true;

        }
    }
}