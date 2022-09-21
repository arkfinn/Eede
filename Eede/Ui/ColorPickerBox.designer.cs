namespace Eede.Ui
{
    partial class ColorPickerBox
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.vGradationSlider1 = new Eede.Ui.GradationSlider();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // vGradationSlider1
            // 
            this.vGradationSlider1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.vGradationSlider1.GradationColor = new System.Drawing.Color[] {
        System.Drawing.Color.Empty,
        System.Drawing.Color.Black};
            this.vGradationSlider1.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.vGradationSlider1.Location = new System.Drawing.Point(4, 19);
            this.vGradationSlider1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.vGradationSlider1.Maximum = 255;
            this.vGradationSlider1.Minimum = 0;
            this.vGradationSlider1.Name = "vGradationSlider1";
            this.vGradationSlider1.Size = new System.Drawing.Size(22, 126);
            this.vGradationSlider1.TabIndex = 0;
            this.vGradationSlider1.Text = "vGradationSlider1";
            this.vGradationSlider1.Value = 255;
            this.vGradationSlider1.ValueChanged += new System.EventHandler(this.vGradationSlider1_ValueChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Font = new System.Drawing.Font("MS UI Gothic", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox1.Location = new System.Drawing.Point(0, 165);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(24, 17);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "0";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button1.Location = new System.Drawing.Point(1, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(20, 21);
            this.button1.TabIndex = 2;
            this.button1.Text = "▲";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button2.Location = new System.Drawing.Point(1, 144);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(20, 21);
            this.button2.TabIndex = 3;
            this.button2.Text = "▼";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ColorPickerBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.vGradationSlider1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ColorPickerBox";
            this.Size = new System.Drawing.Size(27, 188);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GradationSlider vGradationSlider1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
