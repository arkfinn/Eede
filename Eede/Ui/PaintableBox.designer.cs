namespace Eede.Ui
{
    partial class PaintableBox
    {
        /// <summary> 
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナで生成されたコード

        /// <summary> 
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.canvas = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).BeginInit();
            this.SuspendLayout();
            // 
            // canvas
            // 
            this.canvas.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.canvas.Location = new System.Drawing.Point(0, 0);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(99, 92);
            this.canvas.TabIndex = 0;
            this.canvas.TabStop = false;
            this.canvas.MouseLeave += new System.EventHandler(this.colorBox_MouseLeave);
            this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.colorBox_MouseMove);
            this.canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.colorBox_MouseDown);
            this.canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.colorBox_Paint);
            this.canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.colorBox_MouseUp);
            this.canvas.MouseEnter += new System.EventHandler(this.colorBox_MouseEnter);
            // 
            // PaintableBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackgroundImage = global::Eede.Properties.Resources.PaintFormBackGround;
            this.Controls.Add(this.canvas);
            this.Name = "PaintableBox";
            this.SizeChanged += new System.EventHandler(this.PaintableBox_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.canvas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox canvas;
    }
}
