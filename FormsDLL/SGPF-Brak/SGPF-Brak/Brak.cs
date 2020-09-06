using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using PDA.Service;
using PDA.OS;
using SkladGP;
using ScannerAll;

using FRACT = System.Decimal;

namespace SGPF_Brak
{
    public partial class Brak : Form
    {
        private MainF xMainF;
        private NSI xNSI;

        // режим работы (приемка или инвентаризация)
        private int nDocType = 0;

        private DataRow drParent;
        private BindingSource bsB;
        private DataView dvB;

        private bool bEditMode = false;
           
        //private AppC.EditList aEd;
        private AppC.EditListC aEd;

        // не обрабатывать введенный символ
        private bool bSkipKey = false;

        private int nMaxMest;
        private FRACT fMaxEd, fEmk;
        private int nTotM = 0;
        private FRACT fTotE = 0;

        public Brak()
        {
            InitializeComponent();
        }

        private void AfterConstruct(MainF xMF)
        {
            xMainF = xMF;
            xNSI = xMF.xNSI;
            nDocType = (int)(xMF.xDLLAPars[0]);
            //DataRow drP = (DataRow)(xMF.xDLLAPars[1]);
            //drParent = drP;

            // BD_DOUTD
            drParent = (DataRow)(xMF.xDLLAPars[1]);

            //
            dvB = xNSI.dsM.Relations[NSI.REL2BRK].ChildTable.DefaultView;
            dvB.RowFilter = String.Format("SYSN={0} AND ID={1}", drParent["SYSN"], drParent["ID"]);

            lHeadB.Text = "Брак (" + drParent["KRKMC"].ToString() + ")";
            tNameProdB.Text = (string)drParent["SNM"];

            // Мест и единиц по накладной
            nMaxMest = (int)drParent["KOLM"];
            fMaxEd = (FRACT)drParent["KOLE"];
            fEmk = (FRACT)drParent["EMK"];

            EvalTotal();

            lMaxM.Text = nMaxMest.ToString();
            lMaxE.Text = fMaxEd.ToString();

            SetBindBrak(drParent);

            // Настройки грида
            dgBrak.SuspendLayout();
            dgBrak.DataSource = bsB;
            BrakStyle(dgBrak);
            dgBrak.ResumeLayout();

            // Включить TouchScreen
            xMainF.xBCScanner.TouchScr(true);

            if (bsB.Count == 0)
            {
                CreateNew();
                if (nDocType == AppC.TYPD_BRK)
                {
                    // Мест
                    tKolMB.Text = lMaxM.Text;
                    tKolMB.DataBindings[0].WriteValue();
                    // Единиц
                    tKolEB.Text = lMaxE.Text;
                    tKolEB.DataBindings[0].WriteValue();
                }
                BeginEditB();
            }
            else
            {
                dgBrak.Enabled = true;
                dgBrak.Focus();
                bsB.ResetBindings(false);
            }
        }

        // Привязка к данным
        private void SetBindBrak(DataRow drP)
        {
            bsB = new BindingSource();
            bsB.DataSource = dvB;

            // Мест
            tKolMB.DataBindings.Add("Text", bsB, "KOLM");
            // Единиц
            tKolEB.DataBindings.Add("Text", bsB, "KOLE");
            // Причина
            tKrkB.DataBindings.Add("Text", bsB, "KRK");

            xNSI.DT[NSI.NS_PRPR].dt.DefaultView.Sort = "KRK";
            cmbReasons.DataSource = xNSI.DT[NSI.NS_PRPR].dt;
            cmbReasons.DisplayMember = "SNM";
            cmbReasons.ValueMember = "KRK";

            //cmbReasons.DataBindings.Add("SelectedValue", tKrkB, "Text");
        }

        private void EvalTotal()
        {
            // Всего брака
            nTotM = 0;
            fTotE = 0;
            for (int i = 0; i < dvB.Count; i++)
            {
                nTotM += (int)dvB[i].Row["KOLM"];
                fTotE += (FRACT)dvB[i].Row["KOLE"];
            }
        }

        // Добавление новой записи
        private void CreateNew()
        {
            DataRowView drv = (DataRowView)bsB.AddNew();
            drv.Row["SYSN"] = drParent["SYSN"];
            drv.Row["ID"] = drParent["ID"];
            bsB.ResetBindings(false);
        }

        // выделение всего поля при входе (текстовые поля)
        private void SelAllTextF(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        // стили таблицы детальных строк заявок
        private void BrakStyle(DataGrid dg)
        {
            ServClass.DGTBoxColorColumn sC;
            System.Drawing.Color colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = NSI.BD_SPMC;

            sC = new ServClass.DGTBoxColorColumn(dg);
            sC.MappingName = "SNM";
            sC.HeaderText = "      Причина брака";
            sC.TableInd = NSI.BD_SPMC;
            sC.Width = 130;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg);
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.TableInd = NSI.BD_SPMC;
            sC.MappingName = "KOLM";
            sC.HeaderText = "Мест";
            sC.Width = 32;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg);
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.TableInd = NSI.BD_SPMC;
            sC.MappingName = "KOLE";
            sC.HeaderText = "Единиц";
            sC.Width = 60;
            ts.GridColumnStyles.Add(sC);
            /*

            sC = new ServClass.DGTBoxColorColumn(dg);
            sC.MappingName = "KRK";
            sC.HeaderText = "Код";
            sC.TableInd = NSI.BD_SPMC;
            sC.Width = 26;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg);
            sC.MappingName = "KPR";
            sC.HeaderText = "Код";
            sC.TableInd = NSI.BD_SPMC;
            sC.Width = 120;
            ts.GridColumnStyles.Add(sC);




                        c = new DataGridTextBoxColumn();
                        c.MappingName = "EMK";
                        c.HeaderText = "Емк";
                        c.Width = 35;
                        ts.GridColumnStyles.Add(c);

                        c = new DataGridTextBoxColumn();
                        c.MappingName = "NP";
                        c.HeaderText = "№ Пт";
                        c.Width = 40;
                        c.NullText = "";
                        ts.GridColumnStyles.Add(c);

                        c = new DataGridTextBoxColumn();
                        c.MappingName = "KRKT";
                        c.HeaderText = "Тара";
                        c.Width = 35;
                        c.NullText = "";
                        ts.GridColumnStyles.Add(c);
             */

            dg.TableStyles.Add(ts);
        }

        // Обработка клавиш
        private void Brak_KeyDown(object sender, KeyEventArgs e)
        {
            int nFunc = 0;
            bool ret = true;

            bSkipKey = false;
            nFunc = xMainF.xFuncs.TryGetFunc(e);
            if (nFunc > 0)
            {
                if (bEditMode == false)
                {// только в режиме просмотра
                    switch (nFunc)
                    {
                        case AppC.F_ADD_REC:
                            CreateNew();
                            BeginEditB();
                            break;
                        case AppC.F_DEL_REC:
                            if (bsB.Count > 0)
                            {
                                bsB.RemoveCurrent();
                                bsB.ResetBindings(false);
                            }
                            break;
                        case AppC.F_CHG_REC:
                            if (bsB.Count > 0)
                            {
                                BeginEditB();
                            }
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else
                {
                    switch (nFunc)
                    {
                        case AppC.F_QUIT:
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
            }
            else
            {
                if (bEditMode == false)
                {// в режиме просмотра
                    switch (e.KeyValue)
                    {
                        case W32.VK_ESC:
                            this.Close();
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else
                {// в режиме редактирования
                    switch (e.KeyValue)
                    {
                        case W32.VK_UP:
                            aEd.TryNext(AppC.CC_PREV);
                            break;
                        case W32.VK_DOWN:
                            aEd.TryNext(AppC.CC_NEXT);
                            break;
                        case W32.VK_ENTER:
                            if (aEd.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                                EndEditB(true);
                            break;
                        case W32.VK_ESC:
                            EndEditB(false);
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
            }
            e.Handled = ret;
            bSkipKey = ret;

        }

        private void Brak_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipKey == true)
            {
                bSkipKey = false;
                e.Handled = true;
            }
        }

        // Начало редактирования
        public void BeginEditB()
        {
            bool bFlag;

            aEd = new AppC.EditListC(new AppC.VerifyEditFields(VerifyB));
            aEd.AddC(tKrkB);

            aEd.AddC(cmbReasons);

            if (nDocType != AppC.TYPD_BRK)
            {
                bFlag = (nMaxMest > 0) ? true : false;
                aEd.AddC(tKolMB, bFlag);
                aEd.AddC(tKolEB);
            }

            bEditMode = true;
            aEd.SetCur(tKrkB);
            //aEd.SetCur(0);
        }

        // Корректность введенного
        private AppC.VerRet VerifyB()
        {
            AppC.VerRet v;

            tKrkB.DataBindings[0].WriteValue();
            tKolMB.DataBindings[0].WriteValue();
            tKolEB.DataBindings[0].WriteValue();
            EvalTotal();

            DataRow dr = ((DataRowView)bsB.Current).Row;
            DataRow drReas = xNSI.DT[NSI.NS_PRPR].dt.Rows.Find(dr["KPR"]);

            v.nRet = AppC.RC_CANCEL;
            v.cWhereFocus = null;
            if (drReas != null)
            {
                if (fTotE > fMaxEd)
                {
                    Srv.ErrorMsg("Превышение количества!", true);
                    tKolEB.Text = fTotE.ToString();
                    tKolEB.SelectAll();
                    v.cWhereFocus = (tKolMB.Enabled) ? tKolMB : tKolEB;
                }
                else
                {
                    if ((fTotE != 0) || (nTotM != 0))
                        v.nRet = AppC.RC_OK;
                    else
                        Srv.ErrorMsg("Нулевые количества!", true);
                }
            }
            else
            {
                Srv.ErrorMsg("Причина не указана!", true);
            }

            return (v);
        }

        // Завершение редактирования
        public void EndEditB(bool bSave)
        {
            bEditMode = false;
            if (bSave)
            {
                bsB.EndEdit();
            }
            else
            {
                try
                {
                    if (bsB.Current != null)
                        ((DataRowView)bsB.Current).CancelEdit();
                    bsB.CancelEdit();
                }
                catch { }
            }

            bsB.ResetBindings(false);
            aEd.EditIsOver(this);

            if (dgBrak.Enabled)
                dgBrak.Focus();
            else
            {
                this.Close();
            }
        }

        private void cmbReasons_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if ((cmb.SelectedValue != null) && (bEditMode == true))
            {
                DataRowView drv = (DataRowView)cmb.SelectedItem;
                tKrkB.Text = cmb.SelectedValue.ToString();
                tKrkB.DataBindings[0].WriteValue();
                UpdateFullKodPr(drv.Row["KPR"].ToString(), drv.Row["SNM"].ToString());
            }
        }

        private void UpdateFullKodPr(string sFullKod, string sName)
        {
            if (bsB.Current != null)
            {
                ((DataRowView)bsB.Current).Row["KPR"] = sFullKod;
                ((DataRowView)bsB.Current).Row["SNM"] = sName;
            }
        }

        private void tKrkB_Validated(object sender, EventArgs e)
        {
            int i = 0;
            string s = ((TextBox)sender).Text.Trim();
            if (s.Length > 0)
            {
                DataView dvR = ((DataTable)cmbReasons.DataSource).DefaultView;
                for (i = 0; i < dvR.Count; i++)
                    if (dvR[i].Row["KRK"].ToString() == s)
                    {// краткий код обнаружен
                        if (cmbReasons.SelectedIndex == i)
                            cmbReasons.SelectedIndex = i - 1;
                        cmbReasons.SelectedIndex = i;
                        //UpdateFullKodPr(dvR[i].Row["KPR"].ToString(), dvR[i].Row["SNM"].ToString());

                        if (nDocType == AppC.TYPD_BRK)
                        {// для актов забраковки на этом все заканчивается
                            EndEditB(true);
                            this.Close();
                        }
                        //aEd.TryNext(AppC.CC_NEXT);
                        break;
                    }
                if (i >= dvR.Count)
                {
                    Srv.ErrorMsg("Не найден!", "Код " + s, true);
                    //((TextBox)sender).Text = "";
                }
            }
        }

        private void tKolMB_Validated(object sender, EventArgs e)
        {
            try
            {
                if (tKolMB.Enabled)
                {// это не окончание режима редактирования
                    int nM = int.Parse(tKolMB.Text);
                    tKolEB.Text = ((FRACT)(nM * fEmk)).ToString();
                }
            }
            catch { }
        }

        private void Brak_Closing(object sender, CancelEventArgs e)
        {
            // Отключить TouchScreen
            xMainF.xBCScanner.TouchScr(false);
        }

        private void Brak_Activated(object sender, EventArgs e)
        {
            if (this.Tag != null)
            {
                AfterConstruct((MainF)this.Tag);
                this.Tag = null;
            }
        }




    }
}