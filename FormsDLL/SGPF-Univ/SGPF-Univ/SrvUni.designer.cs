namespace SGPF_Univ
{
    partial class PrintBlank
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lBlankCmb = new System.Windows.Forms.Label();
            this.lbFuncs = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lBlankCmb
            // 
            this.lBlankCmb.BackColor = System.Drawing.Color.LightSkyBlue;
            this.lBlankCmb.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lBlankCmb.Location = new System.Drawing.Point(0, 3);
            this.lBlankCmb.Name = "lBlankCmb";
            this.lBlankCmb.Size = new System.Drawing.Size(240, 20);
            this.lBlankCmb.Text = "<< Сделайте выбор >>";
            this.lBlankCmb.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbFuncs
            // 
            this.lbFuncs.BackColor = System.Drawing.Color.Azure;
            this.lbFuncs.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lbFuncs.Items.Add("Печать ");
            this.lbFuncs.Items.Add("Создание");
            this.lbFuncs.Items.Add("Корректировка");
            this.lbFuncs.Items.Add("Задание");
            this.lbFuncs.Items.Add("Инвентаризация");
            this.lbFuncs.Location = new System.Drawing.Point(0, 27);
            this.lbFuncs.Name = "lbFuncs";
            this.lbFuncs.Size = new System.Drawing.Size(240, 211);
            this.lbFuncs.TabIndex = 5;
            this.lbFuncs.Tag = "Par";
            // 
            // PrintBlank
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(240, 320);
            this.ControlBox = false;
            this.Controls.Add(this.lbFuncs);
            this.Controls.Add(this.lBlankCmb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "PrintBlank";
            this.Load += new System.EventHandler(this.PrintBlank_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.PrintBlank_Closing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PrintBlank_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintBlank_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lBlankCmb;
        private System.Windows.Forms.ListBox lbFuncs;
    }
}