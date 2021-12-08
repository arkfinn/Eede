using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Settings;
using Eede.Ui;
using System;
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
            using (var dialog = new SizeSelectDialog())
            {
                dialog.ShowDialog();
                if (dialog.DialogResult != DialogResult.OK)
                {
                    return;
                }
                var form = new PictureWindow(dialog.PictureSize, paintableBox1);
                AddChildWindow(form);
            }
        }

        private void OpenPictureFromDialog()
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var filename = new FilePath(openFileDialog1.FileName);
            OpenPicture(filename);
        }

        private void OpenPicture(FilePath filename)
        {
            try
            {
                AddChildWindow(new PictureWindow(filename, paintableBox1));
            }
            catch (Exception)
            {
                MessageBox.Show("ファイルの読み込みに失敗しました");
            }
        }

        private void AddChildWindow(PictureWindow form)
        {
            form.MdiParent = this;
            form.FormClosed += new FormClosedEventHandler(ChildFormClosed);
            form.PicturePulled += new EventHandler<PicturePulledEventArgs>(ChildFormPicturePulled);
            form.PicturePushed = ChildFormPicturePushed;
            form.Show();
            toolStripButton_saveFile.Enabled = true;
        }

        private void ChildFormClosed(object sender, FormClosedEventArgs e)
        {
            if (MdiChildren.Length > 1) return;
            toolStripButton_saveFile.Enabled = false;
        }

        private void ChildFormPicturePulled(object sender, PicturePulledEventArgs e)
        {
            paintableBox1.SetupImage(e.CutOutImage());
        }

        private Picture ChildFormPicturePushed(PicturePushedEventArgs e)
        {
            var src = paintableBox1.GetImage();
            // TODO:アルファ値ゼロの色情報が消える問題あり
            //e.Bitmap.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            //e.Bitmap.DrawImage(src, new Rectangle(e.Position, src.Size));
            var blender = new DirectImageBlender();
            return e.Picture.Blend(blender, src, e.Position);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (!(ActiveMdiChild is PictureWindow child)) return;
            if (child.IsEmptyFileName() && !RenameChildFile(child)) return;

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
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (var filename in files)
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
            paintableBox1.SetPenColor(colorPicker1.GetArgb());
        }

        private void penSizeSetter1_PenSizeChanged(object sender, EventArgs e)
        {
            paintableBox1.SetPenSize(penSizeSetter1.PenSize);
        }

        private void paintableBox1_ColorChanged(object sender, EventArgs e)
        {
            colorPicker1.SetColor(paintableBox1.GetPenColor());
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            using (var dialog = new BoxSizeSettingDialog())
            {
                dialog.SetBoxSize(GlobalSetting.Instance().BoxSize);
                dialog.ShowDialog();
                if (dialog.DialogResult != DialogResult.OK)
                {
                    return;
                }
                GlobalSetting.Instance().BoxSize = dialog.GetInputBoxSize();
            }
        }
    }
}