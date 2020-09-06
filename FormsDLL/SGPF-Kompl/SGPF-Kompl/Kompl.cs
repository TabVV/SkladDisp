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

using FRACT = System.Decimal;

namespace SGPF_Kompl
{
    public partial class Kompl : Form
    {
        private MainF xMF;
        private NSI xNSI;
        private FuncDic xFuncs;
        private int nFuncCh;

        private bool bEditMode = false;
        private AppC.EditListC aEd;

        // не обрабатывать введенный символ
        private bool bSkipKey = false;

        public Kompl()
        {
            InitializeComponent();
        }

        //public Kompl(MainF x, FuncDic xF, int nFCh)
        private void AfterConstruct(MainF x)
        {
            xMF = x;
            xNSI = xMF.xNSI;
            xFuncs = x.xFuncs;
            nFuncCh = (int)x.xDLLPars;

            this.lHeadKompl.Text = (nFuncCh == AppC.F_LOADKPL) ? "Комплектация поддонов" : "Отгрузка поддонов";

            dgZkz.SuspendLayout();
            KomplStyle(dgZkz);
            dgZkz.DataSource = xNSI.DT[NSI.BD_KMPL].dt;
            dgZkz.ResumeLayout();

            Binding bi = new Binding("Text", xNSI.DT[NSI.BD_KMPL].dt, "PP_NAME");


            //bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
            //bi.DataSourceUpdateMode = DataSourceUpdateMode.Never;
            //bi.FormattingEnabled = true;
            //bi.Format += new ConvertEventHandler(bi_Format);

            bi.DataSourceUpdateMode = DataSourceUpdateMode.Never;

            tPolName.DataBindings.Add(bi);

            tLstUch.Text = xMF.xSm.LstUchKompl;

            dgZkz.Focus();
        }

        private void Kompl_KeyDown(object sender, KeyEventArgs e)
        {
            int nDig = 0,
                nFunc = 0;
            bool ret = true;

            bSkipKey = false;
            nFunc = xFuncs.TryGetFunc(e);

            if (bEditMode == false)
            {//в режиме просмотра
                switch (nFunc)
                {
                    case AppC.F_LOAD_DOC:
                        xMF.xCLoad.drPars4Load = ((DataRowView)((tPolName.DataBindings[0]).BindingManagerBase.Current)).Row;
                        //DataRow ddrr = ((DataRowView)((tPolName.DataBindings[0]).BindingManagerBase.Current)).Row;
                        //xMF.xCLoad.dr1st = ddrr;
                        this.Close();
                        break;
                    case AppC.F_CHG_REC:
                        // перед редактированием все очищается
                        xMF.xSm.Uch2Lst(0, true);
                        tLstUch.Text = xMF.xSm.LstUchKompl;

                        BeginEditB();
                        break;
                    default:
                        ret = false;
                        break;
                }
                if (!ret)
                {
                    switch (e.KeyValue)
                    {
                        case W32.VK_ENTER:
                            xMF.xCLoad.drPars4Load = ((DataRowView)((tPolName.DataBindings[0]).BindingManagerBase.Current)).Row;
                            this.Close();
                            break;
                        case W32.VK_ESC:
                            this.Close();
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
            }
            else
            {// для режима редактирования
                switch (nFunc)
                {
                    case AppC.F_DEL_REC:
                        xMF.xSm.Uch2Lst(0, true);
                        tLstUch.Text = xMF.xSm.LstUchKompl;
                        break;
                    default:
                        ret = false;
                        break;
                }
                if (!ret)
                {// функции не вызывались
                    ret = true;
                    //if ((e.KeyValue >= W32.VK_D1) && (e.KeyValue <= W32.VK_D9))

                    if (Srv.IsDigKey(e, ref nDig))
                    {
                        //int i = (e.KeyValue == W32.VK_D1) ? 1 : (e.KeyValue == W32.VK_D2) ? 2 : (e.KeyValue == W32.VK_D3) ? 3 :
                        //        (e.KeyValue == W32.VK_D4) ? 4 : (e.KeyValue == W32.VK_D5) ? 5 : (e.KeyValue == W32.VK_D6) ? 6 :
                        //        (e.KeyValue == W32.VK_D7) ? 7 : (e.KeyValue == W32.VK_D8) ? 8 : 9;
                        if (nDig == 9)
                        {
                            xMF.xSm.Uch2Lst(99, true);
                        }
                        else
                        {
                            if (xMF.xSm.LstUchKompl == "99")
                                xMF.xSm.Uch2Lst(nDig, true);
                            else
                                xMF.xSm.Uch2Lst(nDig);
                        }
                        tLstUch.Text = xMF.xSm.LstUchKompl;
                    }
                    else
                    {
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
            }




            e.Handled = ret;
            bSkipKey = ret;

        }

        private void Kompl_KeyPress(object sender, KeyPressEventArgs e)
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
            aEd = new AppC.EditListC(new AppC.VerifyEditFields(VerifyB));
            aEd.AddC(tLstUch);
            tLstUch.Text = xMF.xSm.LstUchKompl;

            bEditMode = true;
            aEd.SetCur(aEd[0]);
        }

        // Корректность введенного
        private AppC.VerRet VerifyB()
        {
            AppC.VerRet v;

            v.nRet = AppC.RC_OK;
            v.cWhereFocus = null;
            return (v);
        }

        // Завершение редактирования
        public void EndEditB(bool bSave)
        {
            bEditMode = false;
            if (bSave)
            {
            }
            else
            {
            }
            aEd.EditIsOver();
            dgZkz.Focus();
        }

        private void SelAllTextF(object sender, EventArgs e)
        {

        }

        private void KomplStyle(DataGrid dg)
        {
            ServClass.DGTBoxColorColumn sC;
            System.Drawing.Color colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = NSI.BD_KMPL;

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_KMPL);
            sC.MappingName = "EXPR_DT";
            sC.HeaderText = "Дата";
            sC.Width = 33;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_KMPL);
            sC.MappingName = "KSMEN";
            sC.HeaderText = "Смена";
            sC.Width = 32;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_KMPL);
            sC.MappingName = "TD";
            sC.HeaderText = "Тип";
            sC.Width = 25;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_KMPL);
            sC.MappingName = "NOMD";
            sC.HeaderText = "Заказ";
            sC.Width = 35;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_KMPL);
            sC.MappingName = "NUCH";
            sC.HeaderText = "Участки";
            sC.Width = 95;
            ts.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);
        }

        private void Kompl_Activated(object sender, EventArgs e)
        {
            if (this.Tag != null)
            {
                AfterConstruct((MainF)this.Tag);
                this.Tag = null;
            }
        }




    }
}