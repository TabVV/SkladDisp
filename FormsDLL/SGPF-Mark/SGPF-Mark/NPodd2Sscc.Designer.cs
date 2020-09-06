namespace SGPF_Mark
{
    partial class NPodd2Sscc
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
            this.tSSCC = new System.Windows.Forms.TextBox();
            this.lLstUch = new System.Windows.Forms.Label();
            this.lHelp = new System.Windows.Forms.Label();
            this.lKMCName = new System.Windows.Forms.Label();
            this.lKrKMC = new System.Windows.Forms.Label();
            this.lPart = new System.Windows.Forms.Label();
            this.lSm = new System.Windows.Forms.Label();
            this.tKrKMC = new System.Windows.Forms.TextBox();
            this.tParty = new System.Windows.Forms.TextBox();
            this.tDTV = new System.Windows.Forms.TextBox();
            this.lDTV = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tVsego = new System.Windows.Forms.TextBox();
            this.tMest = new System.Windows.Forms.TextBox();
            this.chMsg2WMS = new System.Windows.Forms.CheckBox();
            this.lEmk = new System.Windows.Forms.Label();
            this.tEmk = new System.Windows.Forms.TextBox();
            this.tVes = new System.Windows.Forms.TextBox();
            this.chNewDoc = new System.Windows.Forms.CheckBox();
            this.tDevID = new System.Windows.Forms.TextBox();
            this.chWrapp = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tSSCC
            // 
            this.tSSCC.Enabled = false;
            this.tSSCC.Location = new System.Drawing.Point(74, 216);
            this.tSSCC.Multiline = true;
            this.tSSCC.Name = "tSSCC";
            this.tSSCC.Size = new System.Drawing.Size(158, 23);
            this.tSSCC.TabIndex = 5;
            this.tSSCC.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tSSCC.GotFocus += new System.EventHandler(this.SelAllTextF);
            // 
            // lLstUch
            // 
            this.lLstUch.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lLstUch.Location = new System.Drawing.Point(4, 217);
            this.lLstUch.Name = "lLstUch";
            this.lLstUch.Size = new System.Drawing.Size(64, 20);
            this.lLstUch.Text = "SSCC";
            // 
            // lHelp
            // 
            this.lHelp.BackColor = System.Drawing.Color.SkyBlue;
            this.lHelp.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lHelp.Location = new System.Drawing.Point(0, 299);
            this.lHelp.Name = "lHelp";
            this.lHelp.Size = new System.Drawing.Size(240, 20);
            this.lHelp.Text = "№ точки =           <F1>-помощь";
            // 
            // lKMCName
            // 
            this.lKMCName.BackColor = System.Drawing.Color.SkyBlue;
            this.lKMCName.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lKMCName.Location = new System.Drawing.Point(0, 0);
            this.lKMCName.Name = "lKMCName";
            this.lKMCName.Size = new System.Drawing.Size(240, 39);
            this.lKMCName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lKrKMC
            // 
            this.lKrKMC.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lKrKMC.Location = new System.Drawing.Point(4, 46);
            this.lKrKMC.Name = "lKrKMC";
            this.lKrKMC.Size = new System.Drawing.Size(64, 20);
            this.lKrKMC.Text = "Код";
            // 
            // lPart
            // 
            this.lPart.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lPart.Location = new System.Drawing.Point(4, 102);
            this.lPart.Name = "lPart";
            this.lPart.Size = new System.Drawing.Size(64, 20);
            this.lPart.Text = "Партия";
            // 
            // lSm
            // 
            this.lSm.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lSm.Location = new System.Drawing.Point(4, 158);
            this.lSm.Name = "lSm";
            this.lSm.Size = new System.Drawing.Size(64, 20);
            this.lSm.Text = "Мест";
            // 
            // tKrKMC
            // 
            this.tKrKMC.Enabled = false;
            this.tKrKMC.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.tKrKMC.Location = new System.Drawing.Point(74, 45);
            this.tKrKMC.Multiline = true;
            this.tKrKMC.Name = "tKrKMC";
            this.tKrKMC.Size = new System.Drawing.Size(82, 22);
            this.tKrKMC.TabIndex = 0;
            this.tKrKMC.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tKrKMC.Validating += new System.ComponentModel.CancelEventHandler(this.tKrKMC_Validating);
            // 
            // tParty
            // 
            this.tParty.Enabled = false;
            this.tParty.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tParty.Location = new System.Drawing.Point(74, 101);
            this.tParty.Multiline = true;
            this.tParty.Name = "tParty";
            this.tParty.Size = new System.Drawing.Size(82, 22);
            this.tParty.TabIndex = 1;
            this.tParty.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tParty.Validating += new System.ComponentModel.CancelEventHandler(this.tParty_Validating);
            // 
            // tDTV
            // 
            this.tDTV.Enabled = false;
            this.tDTV.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tDTV.Location = new System.Drawing.Point(74, 129);
            this.tDTV.Multiline = true;
            this.tDTV.Name = "tDTV";
            this.tDTV.Size = new System.Drawing.Size(82, 22);
            this.tDTV.TabIndex = 2;
            this.tDTV.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tDTV.Validating += new System.ComponentModel.CancelEventHandler(this.tDTV_Validating);
            // 
            // lDTV
            // 
            this.lDTV.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lDTV.Location = new System.Drawing.Point(4, 130);
            this.lDTV.Name = "lDTV";
            this.lDTV.Size = new System.Drawing.Size(64, 20);
            this.lDTV.Text = "Дата";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(4, 186);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 20);
            this.label2.Text = "Всего";
            // 
            // tVsego
            // 
            this.tVsego.Enabled = false;
            this.tVsego.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tVsego.Location = new System.Drawing.Point(74, 185);
            this.tVsego.Multiline = true;
            this.tVsego.Name = "tVsego";
            this.tVsego.Size = new System.Drawing.Size(82, 22);
            this.tVsego.TabIndex = 4;
            this.tVsego.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tVsego.Validating += new System.ComponentModel.CancelEventHandler(this.tVsego_Validating);
            // 
            // tMest
            // 
            this.tMest.Enabled = false;
            this.tMest.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tMest.Location = new System.Drawing.Point(74, 157);
            this.tMest.Multiline = true;
            this.tMest.Name = "tMest";
            this.tMest.Size = new System.Drawing.Size(82, 22);
            this.tMest.TabIndex = 3;
            this.tMest.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tMest.Validating += new System.ComponentModel.CancelEventHandler(this.tMest_Validating);
            // 
            // chMsg2WMS
            // 
            this.chMsg2WMS.Checked = true;
            this.chMsg2WMS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chMsg2WMS.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.chMsg2WMS.Location = new System.Drawing.Point(7, 275);
            this.chMsg2WMS.Name = "chMsg2WMS";
            this.chMsg2WMS.Size = new System.Drawing.Size(100, 20);
            this.chMsg2WMS.TabIndex = 6;
            this.chMsg2WMS.Text = "Приход";
            // 
            // lEmk
            // 
            this.lEmk.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.lEmk.Location = new System.Drawing.Point(4, 74);
            this.lEmk.Name = "lEmk";
            this.lEmk.Size = new System.Drawing.Size(68, 20);
            this.lEmk.Text = "Емкость";
            // 
            // tEmk
            // 
            this.tEmk.Enabled = false;
            this.tEmk.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tEmk.Location = new System.Drawing.Point(74, 73);
            this.tEmk.Multiline = true;
            this.tEmk.Name = "tEmk";
            this.tEmk.Size = new System.Drawing.Size(82, 22);
            this.tEmk.TabIndex = 17;
            this.tEmk.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tEmk.Validating += new System.ComponentModel.CancelEventHandler(this.tEmk_Validating);
            // 
            // tVes
            // 
            this.tVes.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.tVes.Location = new System.Drawing.Point(159, 185);
            this.tVes.Multiline = true;
            this.tVes.Name = "tVes";
            this.tVes.Size = new System.Drawing.Size(77, 22);
            this.tVes.TabIndex = 27;
            this.tVes.Text = "2548";
            this.tVes.Visible = false;
            this.tVes.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tVes.Validating += new System.ComponentModel.CancelEventHandler(this.tVes_Validating);
            // 
            // chNewDoc
            // 
            this.chNewDoc.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.chNewDoc.Location = new System.Drawing.Point(114, 275);
            this.chNewDoc.Name = "chNewDoc";
            this.chNewDoc.Size = new System.Drawing.Size(115, 20);
            this.chNewDoc.TabIndex = 37;
            this.chNewDoc.Text = "Новый док-т";
            // 
            // tDevID
            // 
            this.tDevID.Location = new System.Drawing.Point(80, 296);
            this.tDevID.Name = "tDevID";
            this.tDevID.Size = new System.Drawing.Size(30, 23);
            this.tDevID.TabIndex = 47;
            this.tDevID.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tDevID.Validating += new System.ComponentModel.CancelEventHandler(this.tDevID_Validating);
            // 
            // chWrapp
            // 
            this.chWrapp.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.chWrapp.Location = new System.Drawing.Point(74, 249);
            this.chWrapp.Name = "chWrapp";
            this.chWrapp.Size = new System.Drawing.Size(131, 20);
            this.chWrapp.TabIndex = 59;
            this.chWrapp.Text = "Застрейчеван";
            this.chWrapp.CheckStateChanged += new System.EventHandler(this.chWrapp_CheckStateChanged);
            // 
            // NPodd2Sscc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(240, 320);
            this.ControlBox = false;
            this.Controls.Add(this.chWrapp);
            this.Controls.Add(this.tDevID);
            this.Controls.Add(this.chNewDoc);
            this.Controls.Add(this.tVes);
            this.Controls.Add(this.tEmk);
            this.Controls.Add(this.lEmk);
            this.Controls.Add(this.chMsg2WMS);
            this.Controls.Add(this.tMest);
            this.Controls.Add(this.tVsego);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lDTV);
            this.Controls.Add(this.tDTV);
            this.Controls.Add(this.tParty);
            this.Controls.Add(this.lSm);
            this.Controls.Add(this.tKrKMC);
            this.Controls.Add(this.lPart);
            this.Controls.Add(this.lKrKMC);
            this.Controls.Add(this.lKMCName);
            this.Controls.Add(this.lHelp);
            this.Controls.Add(this.lLstUch);
            this.Controls.Add(this.tSSCC);
            this.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "NPodd2Sscc";
            this.Text = "          Текущий  поддон";
            this.Activated += new System.EventHandler(this.NPodd2Sscc_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.NPodd2Sscc_Closing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NPodd2Sscc_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NPodd2Sscc_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tSSCC;
        private System.Windows.Forms.Label lLstUch;
        private System.Windows.Forms.Label lHelp;
        private System.Windows.Forms.Label lKMCName;
        private System.Windows.Forms.Label lKrKMC;
        private System.Windows.Forms.Label lPart;
        private System.Windows.Forms.Label lSm;
        private System.Windows.Forms.TextBox tKrKMC;
        private System.Windows.Forms.TextBox tParty;
        private System.Windows.Forms.TextBox tDTV;
        private System.Windows.Forms.Label lDTV;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tVsego;
        private System.Windows.Forms.TextBox tMest;
        private System.Windows.Forms.CheckBox chMsg2WMS;
        private System.Windows.Forms.Label lEmk;
        private System.Windows.Forms.TextBox tEmk;
        private System.Windows.Forms.TextBox tVes;
        private System.Windows.Forms.CheckBox chNewDoc;
        private System.Windows.Forms.TextBox tDevID;
        private System.Windows.Forms.CheckBox chWrapp;
    }
}