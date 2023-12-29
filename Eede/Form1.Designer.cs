namespace Eede
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            Domain.DrawStyles.FreeCurve freeCurve1 = new Domain.DrawStyles.FreeCurve();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripButton_newFile = new System.Windows.Forms.ToolStripButton();
            toolStripButton_openFile = new System.Windows.Forms.ToolStripButton();
            toolStripButton_saveFile = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButton9 = new System.Windows.Forms.ToolStripButton();
            toolStripButton10 = new System.Windows.Forms.ToolStripButton();
            toolStripButton11 = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButton12 = new System.Windows.Forms.ToolStripButton();
            toolStripButton13 = new System.Windows.Forms.ToolStripButton();
            toolStripButton14 = new System.Windows.Forms.ToolStripButton();
            toolStripButton15 = new System.Windows.Forms.ToolStripButton();
            toolStripButton16 = new System.Windows.Forms.ToolStripButton();
            toolStripButton17 = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            panel1 = new System.Windows.Forms.Panel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            paintableBox1 = new Ui.DrawableBox();
            pictureActionMenu1 = new Ui.AvaloniaWrapper.Navigation.PictureActionMenu();
            toolStrip3 = new System.Windows.Forms.ToolStrip();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            alphaTransferButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            colorPicker1 = new Ui.ColorPicker();
            splitter1 = new System.Windows.Forms.Splitter();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            toolStrip3.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButton_newFile, toolStripButton_openFile, toolStripButton_saveFile, toolStripSeparator1, toolStripButtonUndo, toolStripButtonRedo, toolStripSeparator2, toolStripButton6, toolStripButton7, toolStripButton8, toolStripSeparator3, toolStripButton9, toolStripButton10, toolStripButton11, toolStripSeparator4, toolStripButton12, toolStripButton13, toolStripButton14, toolStripButton15, toolStripButton16, toolStripButton17, toolStripSeparator5, toolStripButton2 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(1065, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            toolStrip1.ItemClicked += toolStrip1_ItemClicked;
            // 
            // toolStripButton_newFile
            // 
            toolStripButton_newFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton_newFile.Image = Properties.Resources.NewFile_6276;
            toolStripButton_newFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton_newFile.Name = "toolStripButton_newFile";
            toolStripButton_newFile.Size = new System.Drawing.Size(23, 22);
            toolStripButton_newFile.Text = "新規作成";
            toolStripButton_newFile.Click += toolStripButton1_Click;
            // 
            // toolStripButton_openFile
            // 
            toolStripButton_openFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton_openFile.Image = Properties.Resources.Open_6529;
            toolStripButton_openFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton_openFile.Name = "toolStripButton_openFile";
            toolStripButton_openFile.Size = new System.Drawing.Size(23, 22);
            toolStripButton_openFile.Text = "開く";
            toolStripButton_openFile.Click += toolStripButton_openFile_Click;
            // 
            // toolStripButton_saveFile
            // 
            toolStripButton_saveFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton_saveFile.Enabled = false;
            toolStripButton_saveFile.Image = Properties.Resources.Save_6530;
            toolStripButton_saveFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton_saveFile.Name = "toolStripButton_saveFile";
            toolStripButton_saveFile.Size = new System.Drawing.Size(23, 22);
            toolStripButton_saveFile.Text = "上書き保存(&S)";
            toolStripButton_saveFile.Click += toolStripButton3_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonUndo
            // 
            toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonUndo.Enabled = false;
            toolStripButtonUndo.Image = Properties.Resources.Undo_16x;
            toolStripButtonUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonUndo.Name = "toolStripButtonUndo";
            toolStripButtonUndo.Size = new System.Drawing.Size(23, 22);
            toolStripButtonUndo.Text = "toolStripButton4";
            toolStripButtonUndo.Click += toolStripButtonUndo_Click;
            // 
            // toolStripButtonRedo
            // 
            toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonRedo.Enabled = false;
            toolStripButtonRedo.Image = Properties.Resources.Redo_16x;
            toolStripButtonRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonRedo.Name = "toolStripButtonRedo";
            toolStripButtonRedo.Size = new System.Drawing.Size(23, 22);
            toolStripButtonRedo.Text = "toolStripButton5";
            toolStripButtonRedo.Click += toolStripButtonRedo_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton6
            // 
            toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton6.Image = Properties.Resources.Cut_6523;
            toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new System.Drawing.Size(23, 22);
            toolStripButton6.Text = "toolStripButton6";
            // 
            // toolStripButton7
            // 
            toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton7.Image = Properties.Resources.Copy_6524;
            toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton7.Name = "toolStripButton7";
            toolStripButton7.Size = new System.Drawing.Size(23, 22);
            toolStripButton7.Text = "toolStripButton7";
            // 
            // toolStripButton8
            // 
            toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton8.Image = Properties.Resources.Paste_6520;
            toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton8.Name = "toolStripButton8";
            toolStripButton8.Size = new System.Drawing.Size(23, 22);
            toolStripButton8.Text = "toolStripButton8";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton9
            // 
            toolStripButton9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton9.Image = Properties.Resources.viewcolor;
            toolStripButton9.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton9.Name = "toolStripButton9";
            toolStripButton9.Size = new System.Drawing.Size(23, 22);
            toolStripButton9.Text = "toolStripButton9";
            toolStripButton9.Click += toolStripButton9_Click;
            // 
            // toolStripButton10
            // 
            toolStripButton10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton10.Image = Properties.Resources.viewalpha;
            toolStripButton10.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton10.Name = "toolStripButton10";
            toolStripButton10.Size = new System.Drawing.Size(23, 22);
            toolStripButton10.Text = "toolStripButton10";
            toolStripButton10.Click += toolStripButton10_Click;
            // 
            // toolStripButton11
            // 
            toolStripButton11.Checked = true;
            toolStripButton11.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButton11.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton11.Image = Properties.Resources.viewmix;
            toolStripButton11.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton11.Name = "toolStripButton11";
            toolStripButton11.Size = new System.Drawing.Size(23, 22);
            toolStripButton11.Text = "toolStripButton11";
            toolStripButton11.Click += toolStripButton11_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton12
            // 
            toolStripButton12.Checked = true;
            toolStripButton12.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButton12.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton12.Image = Properties.Resources.mag1;
            toolStripButton12.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton12.Name = "toolStripButton12";
            toolStripButton12.Size = new System.Drawing.Size(23, 22);
            toolStripButton12.Text = "x1";
            toolStripButton12.Click += toolStripButton12_Click;
            // 
            // toolStripButton13
            // 
            toolStripButton13.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton13.Image = Properties.Resources.mag2;
            toolStripButton13.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton13.Name = "toolStripButton13";
            toolStripButton13.Size = new System.Drawing.Size(23, 22);
            toolStripButton13.Text = "x2";
            toolStripButton13.Click += toolStripButton13_Click;
            // 
            // toolStripButton14
            // 
            toolStripButton14.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton14.Image = Properties.Resources.mag4;
            toolStripButton14.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton14.Name = "toolStripButton14";
            toolStripButton14.Size = new System.Drawing.Size(23, 22);
            toolStripButton14.Text = "x4";
            toolStripButton14.Click += toolStripButton14_Click;
            // 
            // toolStripButton15
            // 
            toolStripButton15.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton15.Image = Properties.Resources.mag6;
            toolStripButton15.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton15.Name = "toolStripButton15";
            toolStripButton15.Size = new System.Drawing.Size(23, 22);
            toolStripButton15.Text = "x6";
            toolStripButton15.Click += toolStripButton15_Click;
            // 
            // toolStripButton16
            // 
            toolStripButton16.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton16.Image = Properties.Resources.mag8;
            toolStripButton16.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton16.Name = "toolStripButton16";
            toolStripButton16.Size = new System.Drawing.Size(23, 22);
            toolStripButton16.Text = "x8";
            toolStripButton16.Click += toolStripButton16_Click;
            // 
            // toolStripButton17
            // 
            toolStripButton17.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton17.Image = Properties.Resources.mag12;
            toolStripButton17.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton17.Name = "toolStripButton17";
            toolStripButton17.Size = new System.Drawing.Size(23, 22);
            toolStripButton17.Text = "x12";
            toolStripButton17.Click += toolStripButton17_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Properties.Resources.tool_size;
            toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new System.Drawing.Size(23, 22);
            toolStripButton2.Text = "カーソルサイズ設定";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(splitContainer1);
            panel1.Dock = System.Windows.Forms.DockStyle.Left;
            panel1.Location = new System.Drawing.Point(0, 25);
            panel1.Margin = new System.Windows.Forms.Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(658, 641);
            panel1.TabIndex = 2;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(toolStrip3);
            splitContainer1.Panel2.Controls.Add(colorPicker1);
            splitContainer1.Size = new System.Drawing.Size(658, 641);
            splitContainer1.SplitterDistance = 430;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(paintableBox1);
            splitContainer2.Panel2.Controls.Add(pictureActionMenu1);
            splitContainer2.Size = new System.Drawing.Size(658, 430);
            splitContainer2.SplitterDistance = 110;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // paintableBox1
            // 
            paintableBox1.AutoScroll = true;
            paintableBox1.BackgroundImage = (System.Drawing.Image)resources.GetObject("paintableBox1.BackgroundImage");
            paintableBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            paintableBox1.DrawStyle = freeCurve1;
            paintableBox1.Location = new System.Drawing.Point(0, 0);
            paintableBox1.Margin = new System.Windows.Forms.Padding(5);
            paintableBox1.Name = "paintableBox1";
            paintableBox1.PenSize = 1;
            paintableBox1.Size = new System.Drawing.Size(519, 430);
            paintableBox1.TabIndex = 1;
            paintableBox1.ColorChanged += paintableBox1_ColorChanged;
            paintableBox1.Drew += paintableBox1_Drew;
            // 
            // pictureActionMenu1
            // 
            pictureActionMenu1.Command = null;
            pictureActionMenu1.Dock = System.Windows.Forms.DockStyle.Right;
            pictureActionMenu1.Location = new System.Drawing.Point(519, 0);
            pictureActionMenu1.Name = "pictureActionMenu1";
            pictureActionMenu1.Size = new System.Drawing.Size(24, 430);
            pictureActionMenu1.TabIndex = 2;
            // 
            // toolStrip3
            // 
            toolStrip3.Dock = System.Windows.Forms.DockStyle.Right;
            toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripSeparator6, alphaTransferButton, toolStripSeparator7 });
            toolStrip3.Location = new System.Drawing.Point(634, 0);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new System.Drawing.Size(24, 206);
            toolStrip3.TabIndex = 1;
            toolStrip3.Text = "toolStrip3";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(21, 6);
            // 
            // alphaTransferButton
            // 
            alphaTransferButton.CheckOnClick = true;
            alphaTransferButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            alphaTransferButton.Image = Properties.Resources.tool_enable_alpha;
            alphaTransferButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            alphaTransferButton.Name = "alphaTransferButton";
            alphaTransferButton.Size = new System.Drawing.Size(21, 20);
            alphaTransferButton.Text = "toolStripButton1";
            alphaTransferButton.ToolTipText = "アルファ値を適用する";
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(21, 6);
            // 
            // colorPicker1
            // 
            colorPicker1.Location = new System.Drawing.Point(7, 2);
            colorPicker1.Margin = new System.Windows.Forms.Padding(5);
            colorPicker1.Name = "colorPicker1";
            colorPicker1.Size = new System.Drawing.Size(106, 220);
            colorPicker1.TabIndex = 0;
            colorPicker1.ColorChanged += colorPicker1_ColorChanged;
            // 
            // splitter1
            // 
            splitter1.Location = new System.Drawing.Point(658, 25);
            splitter1.Margin = new System.Windows.Forms.Padding(4);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(4, 641);
            splitter1.TabIndex = 3;
            splitter1.TabStop = false;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.Filter = "PNG|*.png";
            saveFileDialog1.Title = "名前を付けて保存";
            // 
            // openFileDialog1
            // 
            openFileDialog1.Filter = "All Files|*.bmp;*.png|bmp|*.bmp|png|*.png";
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1065, 666);
            Controls.Add(splitter1);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            IsMdiContainer = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "Form1";
            Text = "Eede";
            DragDrop += Form1_DragDrop;
            DragEnter += Form1_DragEnter;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_newFile;
        private System.Windows.Forms.ToolStripButton toolStripButton_openFile;
        private System.Windows.Forms.ToolStripButton toolStripButton_saveFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonUndo;
        private System.Windows.Forms.ToolStripButton toolStripButtonRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripButton toolStripButton8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton9;
        private System.Windows.Forms.ToolStripButton toolStripButton10;
        private System.Windows.Forms.ToolStripButton toolStripButton11;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolStripButton12;
        private System.Windows.Forms.ToolStripButton toolStripButton13;
        private System.Windows.Forms.ToolStripButton toolStripButton14;
        private System.Windows.Forms.ToolStripButton toolStripButton15;
        private System.Windows.Forms.ToolStripButton toolStripButton16;
        private System.Windows.Forms.ToolStripButton toolStripButton17;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private Ui.ColorPicker colorPicker1;
        private Ui.DrawableBox paintableBox1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton alphaTransferButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private Ui.AvaloniaWrapper.Navigation.PictureActionMenu pictureActionMenu1;
    }
}

