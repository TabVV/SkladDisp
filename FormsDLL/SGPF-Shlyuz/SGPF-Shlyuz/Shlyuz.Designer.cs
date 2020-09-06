namespace SGPF_Shlyuz
{
    partial class Shlyuz
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
            this.tShlAddr = new System.Windows.Forms.TextBox();
            this.dgShlyuz = new System.Windows.Forms.DataGrid();
            this.tNDoc = new System.Windows.Forms.TextBox();
            this.lHeadP = new System.Windows.Forms.Label();
            this.lReasB = new System.Windows.Forms.Label();
            this.tAvto = new System.Windows.Forms.TextBox();
            this.lMestB = new System.Windows.Forms.Label();
            this.lEdB = new System.Windows.Forms.Label();
            this.tSm = new System.Windows.Forms.TextBox();
            this.lSm = new System.Windows.Forms.Label();
            this.tOut = new System.Windows.Forms.TextBox();
            this.tIn = new System.Windows.Forms.TextBox();
            this.lDTCome = new System.Windows.Forms.Label();
            this.lDTOut = new System.Windows.Forms.Label();
            this.lNPropusk = new System.Windows.Forms.Label();
            this.tPropusk = new System.Windows.Forms.TextBox();
            this.lShNomPP = new System.Windows.Forms.Label();
            this.tShNomPP = new System.Windows.Forms.TextBox();
            this.lShlName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tShlAddr
            // 
            this.tShlAddr.Enabled = false;
            this.tShlAddr.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            this.tShlAddr.Location = new System.Drawing.Point(100, 45);
            this.tShlAddr.Multiline = true;
            this.tShlAddr.Name = "tShlAddr";
            this.tShlAddr.Size = new System.Drawing.Size(88, 23);
            this.tShlAddr.TabIndex = 0;
            this.tShlAddr.Text = "1234567890";
            this.tShlAddr.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // dgShlyuz
            // 
            this.dgShlyuz.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.dgShlyuz.Location = new System.Drawing.Point(5, 197);
            this.dgShlyuz.Name = "dgShlyuz";
            this.dgShlyuz.RowHeadersVisible = false;
            this.dgShlyuz.Size = new System.Drawing.Size(230, 89);
            this.dgShlyuz.TabIndex = 4;
            this.dgShlyuz.LostFocus += new System.EventHandler(this.dgShlyuz_LostFocus);
            this.dgShlyuz.GotFocus += new System.EventHandler(this.dgShlyuz_GotFocus);
            // 
            // tNDoc
            // 
            this.tNDoc.Enabled = false;
            this.tNDoc.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.tNDoc.Location = new System.Drawing.Point(63, 107);
            this.tNDoc.Multiline = true;
            this.tNDoc.Name = "tNDoc";
            this.tNDoc.Size = new System.Drawing.Size(71, 23);
            this.tNDoc.TabIndex = 3;
            this.tNDoc.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // lHeadP
            // 
            this.lHeadP.BackColor = System.Drawing.Color.PaleGreen;
            this.lHeadP.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lHeadP.Location = new System.Drawing.Point(0, 0);
            this.lHeadP.Name = "lHeadP";
            this.lHeadP.Size = new System.Drawing.Size(240, 22);
            this.lHeadP.Text = ">=>   œ–»¡€“»≈   <=<";
            this.lHeadP.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lReasB
            // 
            this.lReasB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.lReasB.Location = new System.Drawing.Point(5, 142);
            this.lReasB.Name = "lReasB";
            this.lReasB.Size = new System.Drawing.Size(105, 20);
            this.lReasB.Text = "π ‡‚ÚÓ";
            // 
            // tAvto
            // 
            this.tAvto.Enabled = false;
            this.tAvto.Location = new System.Drawing.Point(5, 166);
            this.tAvto.Multiline = true;
            this.tAvto.Name = "tAvto";
            this.tAvto.Size = new System.Drawing.Size(75, 23);
            this.tAvto.TabIndex = 2;
            this.tAvto.TextChanged += new System.EventHandler(this.tAvto_TextChanged);
            this.tAvto.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // lMestB
            // 
            this.lMestB.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lMestB.Location = new System.Drawing.Point(100, 25);
            this.lMestB.Name = "lMestB";
            this.lMestB.Size = new System.Drawing.Size(59, 20);
            this.lMestB.Text = "ÿÎ˛Á";
            // 
            // lEdB
            // 
            this.lEdB.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lEdB.Location = new System.Drawing.Point(3, 108);
            this.lEdB.Name = "lEdB";
            this.lEdB.Size = new System.Drawing.Size(56, 20);
            this.lEdB.Text = "π œÀ";
            this.lEdB.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tSm
            // 
            this.tSm.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.tSm.Location = new System.Drawing.Point(196, 107);
            this.tSm.Multiline = true;
            this.tSm.Name = "tSm";
            this.tSm.Size = new System.Drawing.Size(36, 23);
            this.tSm.TabIndex = 1;
            this.tSm.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // lSm
            // 
            this.lSm.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lSm.Location = new System.Drawing.Point(136, 108);
            this.lSm.Name = "lSm";
            this.lSm.Size = new System.Drawing.Size(54, 20);
            this.lSm.Text = "—ÏÂÌ‡";
            // 
            // tOut
            // 
            this.tOut.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tOut.Enabled = false;
            this.tOut.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.tOut.Location = new System.Drawing.Point(104, 170);
            this.tOut.Multiline = true;
            this.tOut.Name = "tOut";
            this.tOut.Size = new System.Drawing.Size(129, 20);
            this.tOut.TabIndex = 8;
            this.tOut.Text = "”·˚Î";
            // 
            // tIn
            // 
            this.tIn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tIn.Enabled = false;
            this.tIn.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.tIn.Location = new System.Drawing.Point(104, 144);
            this.tIn.Multiline = true;
            this.tIn.Name = "tIn";
            this.tIn.Size = new System.Drawing.Size(129, 20);
            this.tIn.TabIndex = 9;
            this.tIn.Text = "œË·˚Î";
            // 
            // lDTCome
            // 
            this.lDTCome.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lDTCome.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lDTCome.Location = new System.Drawing.Point(0, 291);
            this.lDTCome.Name = "lDTCome";
            this.lDTCome.Size = new System.Drawing.Size(120, 22);
            this.lDTCome.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lDTOut
            // 
            this.lDTOut.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lDTOut.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lDTOut.Location = new System.Drawing.Point(120, 291);
            this.lDTOut.Name = "lDTOut";
            this.lDTOut.Size = new System.Drawing.Size(120, 22);
            this.lDTOut.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lNPropusk
            // 
            this.lNPropusk.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lNPropusk.Location = new System.Drawing.Point(10, 25);
            this.lNPropusk.Name = "lNPropusk";
            this.lNPropusk.Size = new System.Drawing.Size(72, 20);
            this.lNPropusk.Text = "œÓÔÛÒÍ";
            this.lNPropusk.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tPropusk
            // 
            this.tPropusk.Enabled = false;
            this.tPropusk.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            this.tPropusk.Location = new System.Drawing.Point(10, 45);
            this.tPropusk.Multiline = true;
            this.tPropusk.Name = "tPropusk";
            this.tPropusk.Size = new System.Drawing.Size(80, 23);
            this.tPropusk.TabIndex = 17;
            this.tPropusk.Text = "8899338";
            // 
            // lShNomPP
            // 
            this.lShNomPP.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lShNomPP.Location = new System.Drawing.Point(197, 25);
            this.lShNomPP.Name = "lShNomPP";
            this.lShNomPP.Size = new System.Drawing.Size(33, 20);
            this.lShNomPP.Text = "π";
            // 
            // tShNomPP
            // 
            this.tShNomPP.Enabled = false;
            this.tShNomPP.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.tShNomPP.Location = new System.Drawing.Point(196, 45);
            this.tShNomPP.Multiline = true;
            this.tShNomPP.Name = "tShNomPP";
            this.tShNomPP.Size = new System.Drawing.Size(36, 23);
            this.tShNomPP.TabIndex = 28;
            // 
            // lShlName
            // 
            this.lShlName.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lShlName.Location = new System.Drawing.Point(100, 75);
            this.lShlName.Name = "lShlName";
            this.lShlName.Size = new System.Drawing.Size(126, 20);
            // 
            // Shlyuz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(240, 320);
            this.ControlBox = false;
            this.Controls.Add(this.lShlName);
            this.Controls.Add(this.tShNomPP);
            this.Controls.Add(this.lShNomPP);
            this.Controls.Add(this.tPropusk);
            this.Controls.Add(this.lNPropusk);
            this.Controls.Add(this.lDTOut);
            this.Controls.Add(this.lDTCome);
            this.Controls.Add(this.tIn);
            this.Controls.Add(this.tOut);
            this.Controls.Add(this.lSm);
            this.Controls.Add(this.tSm);
            this.Controls.Add(this.lEdB);
            this.Controls.Add(this.lMestB);
            this.Controls.Add(this.tAvto);
            this.Controls.Add(this.lReasB);
            this.Controls.Add(this.lHeadP);
            this.Controls.Add(this.tNDoc);
            this.Controls.Add(this.dgShlyuz);
            this.Controls.Add(this.tShlAddr);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "Shlyuz";
            this.Activated += new System.EventHandler(this.Shlyuz_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Shlyuz_Closing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Shlyuz_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Shlyuz_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tShlAddr;
        private System.Windows.Forms.DataGrid dgShlyuz;
        private System.Windows.Forms.TextBox tNDoc;
        private System.Windows.Forms.Label lHeadP;
        private System.Windows.Forms.Label lReasB;
        private System.Windows.Forms.TextBox tAvto;
        private System.Windows.Forms.Label lMestB;
        private System.Windows.Forms.Label lEdB;
        private System.Windows.Forms.TextBox tSm;
        private System.Windows.Forms.Label lSm;
        private System.Windows.Forms.TextBox tOut;
        private System.Windows.Forms.TextBox tIn;
        private System.Windows.Forms.Label lDTCome;
        private System.Windows.Forms.Label lDTOut;
        private System.Windows.Forms.Label lNPropusk;
        private System.Windows.Forms.TextBox tPropusk;
        private System.Windows.Forms.Label lShNomPP;
        private System.Windows.Forms.TextBox tShNomPP;
        private System.Windows.Forms.Label lShlName;
    }
}