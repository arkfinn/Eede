namespace Eede.Ui
{
    partial class ColorPicker
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
            this.button1 = new System.Windows.Forms.Button();
            this.pickerB = new Eede.Ui.ColorPickerBox();
            this.pickerG = new Eede.Ui.ColorPickerBox();
            this.pickerR = new Eede.Ui.ColorPickerBox();
            this.pickerA = new Eede.Ui.ColorPickerBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(0, 201);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 29);
            this.button1.TabIndex = 4;
            this.button1.Text = "A   R   G   B";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pickerB
            // 
            this.pickerB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pickerB.GradationColor = new System.Drawing.Color[] {
        System.Drawing.Color.Empty,
        System.Drawing.Color.Black};
            this.pickerB.Location = new System.Drawing.Point(74, 0);
            this.pickerB.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pickerB.Maximum = 255;
            this.pickerB.Name = "pickerB";
            this.pickerB.Size = new System.Drawing.Size(27, 201);
            this.pickerB.TabIndex = 2;
            this.pickerB.Value = 0;
            this.pickerB.ValueChanged += new System.EventHandler(this.picker_ValueChanged);
            // 
            // pickerG
            // 
            this.pickerG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pickerG.GradationColor = new System.Drawing.Color[] {
        System.Drawing.Color.Empty,
        System.Drawing.Color.Black};
            this.pickerG.Location = new System.Drawing.Point(49, 0);
            this.pickerG.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pickerG.Maximum = 255;
            this.pickerG.Name = "pickerG";
            this.pickerG.Size = new System.Drawing.Size(27, 201);
            this.pickerG.TabIndex = 1;
            this.pickerG.Value = 0;
            this.pickerG.ValueChanged += new System.EventHandler(this.picker_ValueChanged);
            // 
            // pickerR
            // 
            this.pickerR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pickerR.GradationColor = new System.Drawing.Color[] {
        System.Drawing.Color.Empty,
        System.Drawing.Color.Black};
            this.pickerR.Location = new System.Drawing.Point(24, 0);
            this.pickerR.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pickerR.Maximum = 255;
            this.pickerR.Name = "pickerR";
            this.pickerR.Size = new System.Drawing.Size(27, 201);
            this.pickerR.TabIndex = 0;
            this.pickerR.Value = 0;
            this.pickerR.ValueChanged += new System.EventHandler(this.picker_ValueChanged);
            // 
            // pickerA
            // 
            this.pickerA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pickerA.GradationColor = new System.Drawing.Color[] {
        System.Drawing.Color.Empty,
        System.Drawing.Color.Black};
            this.pickerA.Location = new System.Drawing.Point(0, 0);
            this.pickerA.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pickerA.Maximum = 255;
            this.pickerA.Name = "pickerA";
            this.pickerA.Size = new System.Drawing.Size(27, 201);
            this.pickerA.TabIndex = 3;
            this.pickerA.Value = 255;
            this.pickerA.ValueChanged += new System.EventHandler(this.picker_ValueChanged);
            // 
            // ColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pickerB);
            this.Controls.Add(this.pickerG);
            this.Controls.Add(this.pickerR);
            this.Controls.Add(this.pickerA);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ColorPicker";
            this.Size = new System.Drawing.Size(106, 233);
            this.Load += new System.EventHandler(this.ColorPicker_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ColorPickerBox pickerR;
        private ColorPickerBox pickerG;
        private ColorPickerBox pickerB;
        private ColorPickerBox pickerA;
        private System.Windows.Forms.Button button1;
    }
}
