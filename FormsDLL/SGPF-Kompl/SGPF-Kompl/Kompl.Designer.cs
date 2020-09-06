namespace SGPF_Kompl
{
    partial class Kompl
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
            this.dgZkz = new System.Windows.Forms.DataGrid();
            this.lHeadKompl = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tLstUch = new System.Windows.Forms.TextBox();
            this.lLstUch = new System.Windows.Forms.Label();
            this.tPolName = new System.Windows.Forms.TextBox();
            this.lHelpU = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // dgZkz
            // 
            this.dgZkz.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.dgZkz.Location = new System.Drawing.Point(1, 22);
            this.dgZkz.Name = "dgZkz";
            this.dgZkz.RowHeadersVisible = false;
            this.dgZkz.Size = new System.Drawing.Size(237, 196);
            this.dgZkz.TabIndex = 4;
            // 
            // lHeadKompl
            // 
            this.lHeadKompl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lHeadKompl.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            this.lHeadKompl.Location = new System.Drawing.Point(0, 0);
            this.lHeadKompl.Name = "lHeadKompl";
            this.lHeadKompl.Size = new System.Drawing.Size(238, 22);
            this.lHeadKompl.Text = "Комплектация поддонов";
            this.lHeadKompl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.LightSkyBlue;
            this.label2.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(0, 299);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(238, 20);
            this.label2.Text = "ENTER - Загрузка и выход";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tLstUch
            // 
            this.tLstUch.Enabled = false;
            this.tLstUch.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.tLstUch.Location = new System.Drawing.Point(68, 244);
            this.tLstUch.Multiline = true;
            this.tLstUch.Name = "tLstUch";
            this.tLstUch.Size = new System.Drawing.Size(168, 23);
            this.tLstUch.TabIndex = 13;
            this.tLstUch.Text = "0";
            // 
            // lLstUch
            // 
            this.lLstUch.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lLstUch.Location = new System.Drawing.Point(3, 248);
            this.lLstUch.Name = "lLstUch";
            this.lLstUch.Size = new System.Drawing.Size(60, 20);
            this.lLstUch.Text = "Участки";
            // 
            // tPolName
            // 
            this.tPolName.BackColor = System.Drawing.Color.PaleTurquoise;
            this.tPolName.Location = new System.Drawing.Point(0, 219);
            this.tPolName.Multiline = true;
            this.tPolName.Name = "tPolName";
            this.tPolName.Size = new System.Drawing.Size(238, 22);
            this.tPolName.TabIndex = 17;
            this.tPolName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lHelpU
            // 
            this.lHelpU.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lHelpU.Location = new System.Drawing.Point(3, 270);
            this.lHelpU.Name = "lHelpU";
            this.lHelpU.Size = new System.Drawing.Size(219, 20);
            this.lHelpU.Text = "для загрузки (F4 - изменить)";
            // 
            // Kompl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 320);
            this.ControlBox = false;
            this.Controls.Add(this.lHelpU);
            this.Controls.Add(this.tPolName);
            this.Controls.Add(this.lLstUch);
            this.Controls.Add(this.tLstUch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lHeadKompl);
            this.Controls.Add(this.dgZkz);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "Kompl";
            this.Text = "Kompl";
            this.Activated += new System.EventHandler(this.Kompl_Activated);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Kompl_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Kompl_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid dgZkz;
        private System.Windows.Forms.Label lHeadKompl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tLstUch;
        private System.Windows.Forms.Label lLstUch;
        private System.Windows.Forms.TextBox tPolName;
        private System.Windows.Forms.Label lHelpU;
    }
}