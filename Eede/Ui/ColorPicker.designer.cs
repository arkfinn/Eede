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
            button1 = new System.Windows.Forms.Button();
            pickerB = new ColorPickerBox();
            pickerG = new ColorPickerBox();
            pickerR = new ColorPickerBox();
            pickerA = new ColorPickerBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button1.Location = new System.Drawing.Point(0, 201);
            button1.Margin = new System.Windows.Forms.Padding(4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(99, 29);
            button1.TabIndex = 4;
            button1.Text = "A   R   G   B";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // pickerB
            // 
            pickerB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            pickerB.GradationColor = new System.Drawing.Color[] { System.Drawing.Color.Empty, System.Drawing.Color.Black };
            pickerB.Location = new System.Drawing.Point(75, 0);
            pickerB.Margin = new System.Windows.Forms.Padding(5);
            pickerB.Maximum = 255;
            pickerB.Name = "pickerB";
            pickerB.Size = new System.Drawing.Size(27, 201);
            pickerB.TabIndex = 2;
            pickerB.Value = 0;
            pickerB.ValueChanged += picker_ValueChanged;
            // 
            // pickerG
            // 
            pickerG.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            pickerG.GradationColor = new System.Drawing.Color[] { System.Drawing.Color.Empty, System.Drawing.Color.Black };
            pickerG.Location = new System.Drawing.Point(50, 0);
            pickerG.Margin = new System.Windows.Forms.Padding(5);
            pickerG.Maximum = 255;
            pickerG.Name = "pickerG";
            pickerG.Size = new System.Drawing.Size(27, 201);
            pickerG.TabIndex = 1;
            pickerG.Value = 0;
            pickerG.ValueChanged += picker_ValueChanged;
            // 
            // pickerR
            // 
            pickerR.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            pickerR.GradationColor = new System.Drawing.Color[] { System.Drawing.Color.Empty, System.Drawing.Color.Black };
            pickerR.Location = new System.Drawing.Point(25, 0);
            pickerR.Margin = new System.Windows.Forms.Padding(5);
            pickerR.Maximum = 255;
            pickerR.Name = "pickerR";
            pickerR.Size = new System.Drawing.Size(27, 201);
            pickerR.TabIndex = 0;
            pickerR.Value = 0;
            pickerR.ValueChanged += picker_ValueChanged;
            // 
            // pickerA
            // 
            pickerA.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            pickerA.GradationColor = new System.Drawing.Color[] { System.Drawing.Color.Empty, System.Drawing.Color.Black };
            pickerA.Location = new System.Drawing.Point(0, 0);
            pickerA.Margin = new System.Windows.Forms.Padding(5);
            pickerA.Maximum = 255;
            pickerA.Name = "pickerA";
            pickerA.Size = new System.Drawing.Size(27, 201);
            pickerA.TabIndex = 3;
            pickerA.Value = 255;
            pickerA.ValueChanged += picker_ValueChanged;
            // 
            // ColorPicker
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(button1);
            Controls.Add(pickerB);
            Controls.Add(pickerG);
            Controls.Add(pickerR);
            Controls.Add(pickerA);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "ColorPicker";
            Size = new System.Drawing.Size(106, 233);
            Load += ColorPicker_Load;
            ResumeLayout(false);
        }

        #endregion

        private ColorPickerBox pickerR;
        private ColorPickerBox pickerG;
        private ColorPickerBox pickerB;
        private ColorPickerBox pickerA;
        private System.Windows.Forms.Button button1;
    }
}
