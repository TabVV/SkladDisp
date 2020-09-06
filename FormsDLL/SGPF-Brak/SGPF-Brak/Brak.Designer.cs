namespace SGPF_Brak
{
    partial class Brak
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
            this.tKolMB = new System.Windows.Forms.TextBox();
            this.dgBrak = new System.Windows.Forms.DataGrid();
            this.tKolEB = new System.Windows.Forms.TextBox();
            this.lHeadB = new System.Windows.Forms.Label();
            this.lReasB = new System.Windows.Forms.Label();
            this.tKrkB = new System.Windows.Forms.TextBox();
            this.cmbReasons = new System.Windows.Forms.ComboBox();
            this.lMestB = new System.Windows.Forms.Label();
            this.lEdB = new System.Windows.Forms.Label();
            this.tNameProdB = new System.Windows.Forms.TextBox();
            this.lMaxM = new System.Windows.Forms.Label();
            this.lMaxE = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tKolMB
            // 
            this.tKolMB.Enabled = false;
            this.tKolMB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.tKolMB.Location = new System.Drawing.Point(17, 143);
            this.tKolMB.Multiline = true;
            this.tKolMB.Name = "tKolMB";
            this.tKolMB.Size = new System.Drawing.Size(50, 23);
            this.tKolMB.TabIndex = 2;
            this.tKolMB.Text = "0";
            this.tKolMB.Validated += new System.EventHandler(this.tKolMB_Validated);
            this.tKolMB.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // dgBrak
            // 
            this.dgBrak.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.dgBrak.Enabled = false;
            this.dgBrak.Location = new System.Drawing.Point(5, 201);
            this.dgBrak.Name = "dgBrak";
            this.dgBrak.RowHeadersVisible = false;
            this.dgBrak.Size = new System.Drawing.Size(230, 78);
            this.dgBrak.TabIndex = 4;
            // 
            // tKolEB
            // 
            this.tKolEB.Enabled = false;
            this.tKolEB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.tKolEB.Location = new System.Drawing.Point(120, 143);
            this.tKolEB.Multiline = true;
            this.tKolEB.Name = "tKolEB";
            this.tKolEB.Size = new System.Drawing.Size(89, 23);
            this.tKolEB.TabIndex = 3;
            this.tKolEB.Text = "0";
            this.tKolEB.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // lHeadB
            // 
            this.lHeadB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lHeadB.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            this.lHeadB.Location = new System.Drawing.Point(0, 0);
            this.lHeadB.Name = "lHeadB";
            this.lHeadB.Size = new System.Drawing.Size(240, 22);
            this.lHeadB.Text = "Брак продукции";
            this.lHeadB.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lReasB
            // 
            this.lReasB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lReasB.Location = new System.Drawing.Point(17, 59);
            this.lReasB.Name = "lReasB";
            this.lReasB.Size = new System.Drawing.Size(136, 20);
            this.lReasB.Text = "Причина брака";
            // 
            // tKrkB
            // 
            this.tKrkB.Enabled = false;
            this.tKrkB.Location = new System.Drawing.Point(159, 58);
            this.tKrkB.Multiline = true;
            this.tKrkB.Name = "tKrkB";
            this.tKrkB.Size = new System.Drawing.Size(50, 23);
            this.tKrkB.TabIndex = 0;
            this.tKrkB.Validated += new System.EventHandler(this.tKrkB_Validated);
            this.tKrkB.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // cmbReasons
            // 
            this.cmbReasons.Enabled = false;
            this.cmbReasons.Location = new System.Drawing.Point(17, 89);
            this.cmbReasons.Name = "cmbReasons";
            this.cmbReasons.Size = new System.Drawing.Size(192, 23);
            this.cmbReasons.TabIndex = 1;
            this.cmbReasons.SelectedIndexChanged += new System.EventHandler(this.cmbReasons_SelectedIndexChanged);
            // 
            // lMestB
            // 
            this.lMestB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lMestB.Location = new System.Drawing.Point(17, 117);
            this.lMestB.Name = "lMestB";
            this.lMestB.Size = new System.Drawing.Size(50, 20);
            this.lMestB.Text = "Мест";
            // 
            // lEdB
            // 
            this.lEdB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lEdB.Location = new System.Drawing.Point(120, 117);
            this.lEdB.Name = "lEdB";
            this.lEdB.Size = new System.Drawing.Size(100, 20);
            this.lEdB.Text = "Единиц";
            // 
            // tNameProdB
            // 
            this.tNameProdB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tNameProdB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tNameProdB.Location = new System.Drawing.Point(0, 24);
            this.tNameProdB.Name = "tNameProdB";
            this.tNameProdB.Size = new System.Drawing.Size(240, 26);
            this.tNameProdB.TabIndex = 14;
            this.tNameProdB.WordWrap = false;
            // 
            // lMaxM
            // 
            this.lMaxM.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lMaxM.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.lMaxM.Location = new System.Drawing.Point(17, 170);
            this.lMaxM.Name = "lMaxM";
            this.lMaxM.Size = new System.Drawing.Size(50, 20);
            this.lMaxM.Text = "0";
            // 
            // lMaxE
            // 
            this.lMaxE.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lMaxE.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.lMaxE.Location = new System.Drawing.Point(123, 170);
            this.lMaxE.Name = "lMaxE";
            this.lMaxE.Size = new System.Drawing.Size(89, 20);
            this.lMaxE.Text = "0";
            // 
            // Brak
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(240, 320);
            this.ControlBox = false;
            this.Controls.Add(this.lMaxE);
            this.Controls.Add(this.lMaxM);
            this.Controls.Add(this.tNameProdB);
            this.Controls.Add(this.lEdB);
            this.Controls.Add(this.lMestB);
            this.Controls.Add(this.cmbReasons);
            this.Controls.Add(this.tKrkB);
            this.Controls.Add(this.lReasB);
            this.Controls.Add(this.lHeadB);
            this.Controls.Add(this.tKolEB);
            this.Controls.Add(this.dgBrak);
            this.Controls.Add(this.tKolMB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "Brak";
            this.Activated += new System.EventHandler(this.Brak_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Brak_Closing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Brak_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Brak_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tKolMB;
        private System.Windows.Forms.DataGrid dgBrak;
        private System.Windows.Forms.TextBox tKolEB;
        private System.Windows.Forms.Label lHeadB;
        private System.Windows.Forms.Label lReasB;
        private System.Windows.Forms.TextBox tKrkB;
        private System.Windows.Forms.ComboBox cmbReasons;
        private System.Windows.Forms.Label lMestB;
        private System.Windows.Forms.Label lEdB;
        private System.Windows.Forms.TextBox tNameProdB;
        private System.Windows.Forms.Label lMaxM;
        private System.Windows.Forms.Label lMaxE;
    }
}