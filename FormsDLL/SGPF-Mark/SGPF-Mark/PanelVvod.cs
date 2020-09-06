using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

using ScannerAll;
using PDA.OS;
using PDA.Service;

using FRACT = System.Decimal;


namespace SkladGP
{
    public partial class MainF : Form
    {

        // ������� ��������� ������
        private int nCurVvodState = AppC.DT_SHOW;

        // ������� �������� (DataRow), ������������ �� ������
        private DataRow drShownDoc = null;

        // ������� ������� - ������ ��� ���������
        private bool bShowTTN = true;

        // ������ ������� ��� ��������� ?
        private bool bZVKPresent = false;


        // ������� ������ � ������� ��������� �����
        private DataRow drDet = null;

        // ������� ����-������
        private PSC_Types.ScDat scCur;

        // ������� ������� ��������� ��� ��������������
        //private List<AppC.EditEnable> aEdVvod;
        private AppC.EditListC aEdVvod;

        // ���� ���������� �����
        //private bool bQuitEdVvod = false;

        // ������� ������� �������� � ������� ���������
        //private int nCurEditCommand = -1;

        // ������� ������������� ��������� ����������� ������������
        private bool bLastScan = false;

        // ������ ��������� �������� (���������� ������������ ����� �����������)
        private int nOldMest = 0;
        private FRACT fOldVsego, fOldVes;

        // ������ ��������� �������� (���������� ������������ ����� ���������������)
        private int nDefMest = 0;
        private FRACT fDefEmk, fDefVsego, fDefVes = 0;

        // ������ ��������� �������� (���������� ������������ ����� �����������)
        private bool bAskEmk = false, 
            bAskKrk = false;
        private int nOldKrk = 0;
        private FRACT fOldEmk = 0M;

        private int nOldKrkEmkNoSuch = 0;

        // ������� �� ������� ����
        private void EnterInScan()
        {
            if ((xCDoc != null) && (xCDoc.nTypOp == AppC.TYPOP_MOVE))
            {// ��� �������� ����������� - ������ ��������
                lEAN.Text = " ��";
                lDateIzg.Text = "  �";
            }
            else
            {
                lEAN.Text = "EAN";
                lDateIzg.Text = "����";
            }
            ShowRegVvod();

            // ��� ���������� (��� ��� ������)?
            DataTable dtD = ((DataTable)this.dgDet.DataSource);
            bShowTTN = (dtD == xNSI.DT[NSI.BD_DOUTD].dt) ? true : false;

            if (drShownDoc != xCDoc.drCurRow)
            {// �������� ��������
                NewDoc(dtD);
            }
            lDocInf.Text = CurDocInf(xCDoc.xDocP);
            if ((xCDoc.xDocP.nTypD == AppC.TYPD_OPR) && (bShowTTN == false))
            {
                ChgDetTable(null, NSI.BD_DOUTD);
            }
            if ((xCDoc.drCurRow != null) &&
               ((xCDoc.nTypOp == AppC.TYPOP_KMPL) ||
                (xCDoc.nTypOp == AppC.TYPOP_OTGR)))
            {
                if (xCDoc.xNPs.Current <= 0)
                    xCDoc.xNPs.TryNext(true, true);
                SetEasyEdit(AppC.REG_SWITCH.SW_SET);
            }
            else
            {// ��� ���������� ������� ���������
                if (bShowTTN == false)
                    xNSI.ChgGridStyle(NSI.BD_DIND, NSI.GDET_ZVK);
                if (xCDoc.xNPs.Current <= 0)
                    xCDoc.xNPs.TryNext(true, false);
            }
            tCurrPoddon.Text = xCDoc.xNPs.Current.ToString();
            dgDet.Focus();
        }

        // ������������ ������ � ����� �� �������� ��������� (��������� ������)
        private string CurDocInf(DocPars xP)
        {
            string 
                //sTypDoc = DocPars.TypName(ref xP.nTypD) + ": ",
                sTypDoc = DocPars.TypDName(ref xP.nTypD) + ": ",

                sData = xP.dDatDoc.ToString("dd.MM"),
                sSmena = (xP.sSmena == "") ? "" : " ��:" + xP.sSmena,
                sUch = (xP.nUch == AppC.EMPTY_INT) ? "" : " ��: " + xP.nUch.ToString(),
                sEksName = (xP.nEks == AppC.EMPTY_INT)? "" :
                    xP.nEks.ToString() + " " + xP.sEks.Substring(0, Math.Min(20, xP.sEks.Length)) + "\n",
                sPolName = xP.sPol.Substring(0, Math.Min(20, xP.sPol.Length));

            switch (xP.nTypD)
            {
                case AppC.TYPD_SAM:
                    // ��� ����������
                    sTypDoc += String.Format("{0} {1} �{2} �� {3}", sPolName, sSmena, xP.sNomDoc, sData);
                    break;
                case AppC.TYPD_CVYV:
                    // ��� ������������
                    sTypDoc += String.Format("{0} {1} �{2} �� {3}", sPolName, sSmena, xP.sNomDoc, sData);
                    break;
                case AppC.TYPD_VPER:
                    // ��� ����������� �����������
                    sTypDoc += String.Format("{0} {1} �{2} �� {3}", sPolName, sSmena, xP.sNomDoc, sData);
                    break;
                case AppC.TYPD_SVOD:
                    // ��� �����
                    sTypDoc += String.Format("{0} {1}{2}{3}", sEksName, sData, sSmena, sUch);
                    break;
                case AppC.TYPD_PRIH:
                    // ��� ����������
                    sTypDoc += String.Format("� {0} �� {1} {2}", xP.sNomDoc, sData, sEksName);
                    break;
                case AppC.TYPD_BRK:
                case AppC.TYPD_INV:
                    // ��� ��������������
                    sTypDoc += String.Format("�� {0} {1} {2} �{3}", sData, sUch, sSmena, xP.sNomDoc);
                    break;
            }

            if ((xCDoc.nTypOp == AppC.TYPOP_KMPL) ||
                (xCDoc.nTypOp == AppC.TYPOP_OTGR))
            {
                sTypDoc = sData + sSmena;
                sTypDoc += " ��. " + xCDoc.sLstUchNoms;
                sTypDoc += " ������� " + xCDoc.xNPs.RangeN();
            }

            if (xCDoc.nTypOp == AppC.TYPOP_MARK)
                sTypDoc = "���������� " + sData + sSmena;

            if (xCDoc.nTypOp == AppC.TYPOP_PRMK)
            {
                sTypDoc = "������� " + sData + sSmena;
            }
            if (xCDoc.nTypOp == AppC.TYPOP_MOVE)
            {
                sTypDoc = "����������� " + sData + sSmena + "\n";
                if (xSm.xAdrFix1 != null)
                {
                    sTypDoc += "���� " + xSm.xAdrFix1.sName;
                }
            }

            return (sTypDoc);
        }

        // �������� ��� ����� ���������
        // dtD - ������� � ����� ��������� �����
        private void NewDoc(DataTable dtD)
        {
            string sF = "";
                
            bZVKPresent = false;
            if (xCDoc.drCurRow != null)
            {
                sF = xCDoc.drCurRow["SYSN"].ToString();
                DataRow[] childRows = xCDoc.drCurRow.GetChildRows(NSI.REL2ZVK);
                if (childRows.Length > 0)
                    bZVKPresent = true;

                //xPars.parVvodSHTNewRec = false;
                //xPars.parVvodVESNewRec = false;

                //if ((int)xCDoc.drCurRow["TD"] == AppC.TYPD_SVOD)
                //    xPars.parVvodVESNewRec = true;
            }

            dtD.DefaultView.RowFilter = String.Format("(SYSN={0})", sF);

            drShownDoc = xCDoc.drCurRow;

            //ShowDocInVvod();
            ChangeDetRow(true);
            ShowStatDoc();
            ShowRegVvod();

            //xCDoc.xNPs = new PoddonList();
            //if (xCDoc.drCurRow != null)
            //{
            //    if ((int)xCDoc.drCurRow["TYPOP"] == AppC.TYPOP_KMPL)
            //    {
            //        FillPoddonlLst((int)drShownDoc["SYSN"]);
            //    }
            //}
        }

        // ����� ����� ��������� ����� ��� ������� ���������
        private void ShowRegVvod()
        {
            string sN = "���[";
            //sN += (xPars.parVvodVESNewRec == true)?"�":"\x3A3";
            //sN += (xPars.aParsTypes[1].bAddNewRow == true) ? "�" : "\x3A3";
            sN += (xPars.aDocPars[xCDoc.xDocP.nTypD].bSumVes == false) ? "�" : "\x3A3";

            sN += (AppPars.bVesNeedConfirm == true) ? "]�����" : "]�����";
            tVvod_VESReg.Text = sN;

            //sN = "��[";
            //sN += (xPars.parVvodSHTNewRec == true) ? "�" : "\x3A3" + "]";
            //tVvod_SHTReg.Text = sN;
        }

        // ����� ���.���������� ����� ������������ ��� ������������
        private void ShowDopInfKMPL(object x)
        {
            string sN = "";
            if (((int)xCDoc.drCurRow["TYPOP"] == AppC.TYPOP_KMPL) &&
                (xScrDet.CurReg == ScrMode.SCRMODES.FULLMAX) )
            {
                try
                {
                    sN = scCur.dDataIzg.ToString("dd.MM");
                    sN = String.Format("� {0} {1}", scCur.nParty, sN);
                }
                catch { }
                tVvod_VESReg.Text = sN;
                lVvodStat_vv.Text = x.ToString();
            }
        }




        // ���������� ��������� ����� � ���/������
        private void ShowStatDoc()
        {
            string 
                sS = @"0(0�)";
            if (xCDoc.drCurRow != null)
            {
                int nM = 0;
                DataRow[] childRows = xCDoc.drCurRow.GetChildRows((bShowTTN) ? NSI.REL2TTN : NSI.REL2ZVK);
                foreach(DataRow dr in childRows)
                    nM += (int)dr["KOLM"];
                sS = String.Format("{0}({1}�)", childRows.Length, nM);
                //sSpec = IsSpecZVK(childRows);
                //if (sSpec != "")
                //{
                //    lSpecCond_vv.Text = sSpec;
                //    lSpecCond_vv.Visible = true;
                //}
                //else
                //{
                //    lSpecCond_vv.Visible = false;
                //}
            }
            lVvodStat_vv.Text = sS;
        }

        // ��������� �������� ����������� ������� ������ (� ������)
        // ���������� ������� ��� ���������, ��� ������� ����������� ������
        //private string IsSpecZVK(DataRow[] drA)
        //{
        //    int nSpec = 0;
        //    string nRet = "";

        //    foreach (DataRow dr in drA)
        //    {
        //        if (dr["NP"] != System.DBNull.Value)
        //        {
        //            if ((int)dr["NP"] > 0)
        //            {
        //                int nS = (int)dr["READYZ"];
        //                if ((NSI.READINESS)((int)dr["READYZ"]) != NSI.READINESS.FULL_READY)
        //                {
        //                    try
        //                    {
        //                        nRet = ((int)dr["KRKMC"]).ToString();
        //                    }
        //                    catch
        //                    {
        //                        nRet = "��� ����";
        //                    }
        //                    nSpec++;
        //                }
        //            }
        //        }
        //    }
        //    if (nSpec > 0)
        //    {
        //        if (nSpec == 1)
        //            nRet += " ������";
        //        else
        //            nRet = nSpec.ToString() + " �������";
        //    }
        //    return (nRet);
        //}





        // ��������� ����� ����� [+ ����� ��������������]
        private void SetDetFields(bool bClearInf)
        {
            this.tKMC.Text = (scCur.nKrKMC == AppC.EMPTY_INT) ? "" : scCur.nKrKMC.ToString();

            this.tNameSc.Text = scCur.sN;

            if (xCDoc.nTypOp == AppC.TYPOP_MOVE)
            {
                tEAN.Text = scCur.xOp.GetSrc(true);
                tDatMC.Text = scCur.xOp.GetDst(true);
            }
            else
            {
                this.tEAN.Text = scCur.sEAN;
                this.tDatMC.Text = scCur.sDataIzg;
            }

            this.tParty.Text = scCur.nParty;
            this.tMest.Text = (scCur.nMest == AppC.EMPTY_INT) ? "" : scCur.nMest.ToString();
            this.tEmk.Text = (scCur.fEmk == 0) ? "" : scCur.fEmk.ToString();
            this.tVsego.Text = (scCur.fVsego == 0) ? "" : scCur.fVsego.ToString();

            if (bClearInf == true)
            {
                lMst_alr.Text = "";
                lEdn_alr.Text = "";
                lOst_vv.Text = "";
                lOstVsego_vv.Text = "";
                lSpecCond_vv.Visible = false;
                //lVvodStat_vv.Text = "";
            }
            else
            {// �������������� ���� ������������
            }


        }


        // ���������� ����� ������
        private void dgDet_CurrentCellChanged(object sender, EventArgs e)
        {
            ChangeDetRow(false);


        }

        // ����� ������������ ���������
        // bReRead = true - �������������� ������ ������
        private int ChangeDetRow(bool bReRead)
        {
            int ret = 0;
            DataView dvDetail = ((DataTable)this.dgDet.DataSource).DefaultView;
            ret = dvDetail.Count;

            if (ret >= 1)
            {
                DataRowView drv = dvDetail[this.dgDet.CurrentRowIndex];
                if ((drDet != drv.Row) || (bReRead == true))
                {// ����� ������
                    if (bInEasyEditWait == true)
                    {
                        ZVKeyDown(AppC.F_OVERREG, null, ref ehCurrFunc);
                    }
                    //string sTypDoc = GetGridCurrentStyle(dgDet);
                    drDet = drv.Row;
                    //drvInDetGrid = drv;

                    //if (nCurVvodState == AppC.DT_SHOW)

                    if (!bEditMode)
                    {
                        scCur = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.NoData, ""));
                        xNSI.InitCurProd(ref scCur, drDet);
                        SetDetFields(true);
                    }
                    else
                        SetDetFields(false);
                }
            }
            else
            {
                drDet = null;
                scCur = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.NoData, ""));
                SetDetFields(true);
            }

            bLastScan = false;
            //SetDetFields((drDet == null));
            //SetDetFields(true);

            return (ret);
        }


        string GetGridCurrentStyle(DataGrid dataGrid)
        {
            CurrencyManager currencyManager = (CurrencyManager)BindingContext[dataGrid.DataSource];
            IList iList = currencyManager.List;
            if (iList is ITypedList)
            {
                ITypedList iTypedList = (ITypedList)currencyManager.List;
                return iTypedList.GetListName(null);
            }
            else
                return iList.GetType().Name;
        }
        private void SetDopFieldsForEnter(bool bAfterAdd)
        { SetDopFieldsForEnter(bAfterAdd, false); }

        // ��������� ����� ������ ������ - ��� �������
        // bAfterAdd = true - ����� ����� ���������� ��������������� ���������
        private void SetDopFieldsForEnter(bool bAfterAdd, bool bMainRefresh)
        {
            int nMa = scCur.nKolM_alr + scCur.nKolM_alrT;
            FRACT fVa = scCur.fKolE_alr + scCur.fKolE_alrT;
            bool bShowZvk = false;

            if (bAfterAdd == true)
            {// ����� �������������� � ���������� �������� ������ "��� �������"
                nMa += scCur.nMest;

                if (scCur.fEmk == 0)
                {// ������ ��������� �������
                    if (fVa == 0)
                        // ��������� ��������� �� ���������, ��������� ����� (���� ��� ��)
                        fVa = scCur.fVsego;
                    else
                        // ��������� ��������� ���������, ��������� �� ���������� (���� ��� ��)
                        fVa += scCur.fVsego;
                }
                else
                {// ��������� �����
                    if (fVa == 0)
                        // ��������� ��������� �� ���������, ��������� ����� (���� ��� ��)
                        fVa = scCur.fVsego + scCur.fMKol_alr;
                    else
                    {// ��������� ��������� ���������, ����������� ������
                    }
                }
            }
            else
            {// ���� ��� �����, ���� ������� �����
                if (fVa == 0)
                {// ��������� ��������� �� ���������, ��������� ����� (���� ��� ��)
                    fVa = scCur.fMKol_alr + scCur.fMKol_alrT;
                }
            }

            if (bZVKPresent == true)
            {
                if (xCDoc.xDocP.nTypD != AppC.EMPTY_INT)
                {
                    bShowZvk = xPars.aDocPars[xCDoc.xDocP.nTypD].bShowFromZ;
                }
            }


            if (bShowZvk == true)
            {
                lOst_vv.Text = scCur.nKolM_zvk.ToString();
                lOstVsego_vv.Text = scCur.fKolE_zvk.ToString();
            }
            else
            {
                lOst_vv.Text = "";
                lOstVsego_vv.Text = "";
            }

            lMst_alr.Text = nMa.ToString();
            lEdn_alr.Text = fVa.ToString();

            if (bMainRefresh)
            {
                tMest.Text = (scCur.nMest == AppC.EMPTY_INT) ? "" : scCur.nMest.ToString();
                tEmk.Text = (scCur.fEmk == 0) ? "" : scCur.fEmk.ToString();
                tVsego.Text = (scCur.fVsego == 0) ? "" : scCur.fVsego.ToString();
            }
        }



        // ��������� ������ �� ������ ������������/�����
        private bool Vvod_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;
            CurrencyManager cmDet;

            if ((nFunc <= 0) && (bEditMode == false) &&
                (e.KeyValue == W32.VK_ESC) && (e.Modifiers == Keys.None))
                nFunc = AppC.F_MAINPAGE;

            if (nFunc > 0)
            {
                if (xScrDet.CurReg != 0)
                {// ����� ����� �� ��������������
                    if ((nFunc != AppC.F_CHG_SORT) && (nFunc != AppC.F_CHGSCR)
                     && (nFunc != AppC.F_HELP)
                     && (nFunc != AppC.F_KMCINF)
                     && (nFunc != AppC.F_CELLINF)
                     && (nFunc != AppC.F_SETPODD)
                     && (nFunc != AppC.F_GOFIRST)
                     && (nFunc != AppC.F_GOLAST)
                     && (nFunc != AppC.F_NEXTDOC) && (nFunc != AppC.F_PREVDOC)
                     && (nFunc != AppC.F_QUIT) && (nFunc != AppC.F_SAMEKMC)
                     && (nFunc != AppC.F_EASYEDIT) && (nFunc != AppC.F_CHG_GSTYLE)
                     && (nFunc != AppC.F_DEL_REC) && (nFunc != AppC.F_FLTVYP))
                    {
                        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                        xScrDet.NextReg(AppC.REG_SWITCH.SW_CLEAR);
                    }
                }

                switch (nFunc)
                {
                    case AppC.F_CELLINF:
                        if (xCDoc.xOper.IsFillSrc())
                        {
                            ConvertAdr2Lst(xCDoc.xOper.xAdrSrc.Addr, "TXT");
                        }
                        ret = true;
                        break;
                    case AppC.F_KMCINF:
                        GetKMCInf(nFunc);
                        ret = true;
                        break;
                }



                if (bEditMode == false)
                {// ������� ������ ��� ������ ���������
                    //if (xScrDet.CurReg != 0)
                    //{// ����� ����� �� ��������������
                    //    if ((nFunc != AppC.F_CHG_SORT) && (nFunc != AppC.F_CHGSCR)
                    //     && (nFunc != AppC.F_HELP)
                    //     && (nFunc != AppC.F_KMCINF)
                    //     && (nFunc != AppC.F_CELLINF)
                    //     && (nFunc != AppC.F_SETPODD)
                    //     && (nFunc != AppC.F_GOFIRST)
                    //     && (nFunc != AppC.F_GOLAST)
                    //     && (nFunc != AppC.F_NEXTDOC) && (nFunc != AppC.F_PREVDOC)
                    //     && (nFunc != AppC.F_QUIT) && (nFunc != AppC.F_SAMEKMC)
                    //     && (nFunc != AppC.F_EASYEDIT) && (nFunc != AppC.F_CHG_GSTYLE)
                    //     && (nFunc != AppC.F_DEL_REC) && (nFunc != AppC.F_FLTVYP))
                    //    {
                    //        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                    //        xScrDet.NextReg(AppC.REG_SWITCH.SW_CLEAR);
                    //        //return (true);
                    //    }
                    //}

                    switch (nFunc)
                    {
                        case AppC.F_MAINPAGE:
                            tcMain.SelectedIndex = PG_DOC;
                            ret = true;
                            break;
                        case AppC.F_ADD_REC:
                            scCur = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.NoData, ""));
                            AddOrChangeDet(nFunc);
                            ret = true;
                            break;
                        case AppC.F_CHG_REC:
                            AddOrChangeDet(nFunc);
                            ret = true;
                            break;
                        case AppC.F_DEL_ALLREC:
                        case AppC.F_DEL_REC:
                            DelDetDoc(nFunc);
                            ShowStatDoc();
                            ret = true;
                            break;
                        case AppC.F_CHG_SORT:
                            string sNS = "";
                            string sS = xNSI.SortDet(bShowTTN, xNSI, ref sNS);
                            lSortInf.Text = sNS;
                            DataView dv = ((DataTable)this.dgDet.DataSource).DefaultView;
                            dv.Sort = sS;
                            ChangeDetRow(true);
                            ret = true;
                            break;
                        case AppC.F_NEXTDOC:
                        case AppC.F_PREVDOC:
                            SetNextPrevDoc(nFunc);
                            ret = true;
                            break;
                        case AppC.F_GOFIRST:
                        case AppC.F_GOLAST:
                            cmDet = (CurrencyManager)BindingContext[dgDet.DataSource];
                            if (cmDet.Count > 0)
                            {
                                cmDet.Position = (nFunc == AppC.F_GOFIRST) ? 0 : cmDet.Count - 1;
                                ChangeDetRow(true);
                            }
                            ret = true;
                            break;
                        case AppC.F_DEBUG:
                            if ((DataTable)dgDet.DataSource == xNSI.DT[NSI.BD_DIND].dt)
                            {
                                DataRow drZ = ((DataTable)dgDet.DataSource).DefaultView[dgDet.CurrentRowIndex].Row;
                                drZ["READYZ"] = NSI.READINESS.FULL_READY;
                            }
                            ret = true;
                            break;
                        case AppC.F_CHG_GSTYLE:
                            // ������������ ���������/������
                            ChgDetTable(null, "");
                            ret = true;
                            break;
                        case AppC.F_TOT_MEST:
                            // ����� ���� �� ���������/������
                            //ShowTotMest();
                            if (drDet != null)
                                ShowTotMestAll((int)drDet["KRKMC"], (string)drDet["NP"]);
                            else
                                ShowTotMest();
                                ret = true;
                            break;
                        case AppC.F_CTRLDOC:
                            // �������� �������� ���������
                            if (drShownDoc != null)
                            {
                                Cursor.Current = Cursors.WaitCursor;
                                List<string> lstCtrl = new List<string>();
                                ControlDocZVK(null, lstCtrl);
                                Cursor.Current = Cursors.Default;
                                xHelpS.ShowInfo(lstCtrl, ref ehCurrFunc);
                            }
                            ret = true;
                            break;
                        case AppC.F_SAMEKMC:
                            // ������� �� ����� �� ��� � ������� ���������
                            GoSameKMC();
                            ret = true;
                            break;
                        case AppC.F_CHGSCR:
                            // ����� ������
                            xScrDet.NextReg(AppC.REG_SWITCH.SW_NEXT);
                            ret = true;
                            break;
                        case AppC.F_FLTVYP:
                            // ��������� ������� - ������ �������������
                            SetDetFlt(AppC.REG_SWITCH.SW_NEXT);
                            ret = true;
                            break;
                        case AppC.F_EASYEDIT:
                            // ����� ����������� �����
                            SetEasyEdit(AppC.REG_SWITCH.SW_NEXT);
                            ret = true;
                            break;
                        case AppC.F_ZVK2TTN:
                            // ������� � ���
                            ZVK2TTN();
                            ret = true;
                            break;
                        case AppC.F_BRAKED:
                            if (bShowTTN && (drDet != null))
                            {
                                xDLLAPars = new object[2]{xCDoc.xDocP.nTypD, drDet};
                                DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Brak.dll", true);
                                dgDet.Focus();
                            }
                            ret = true;
                            break;
                        case AppC.F_OPROVER:
                            SetOverOPR(false);
                            ret = true;
                            break;
                        case AppC.F_SETPODD:
                            TryNextPoddon(null);
                            ret = true;
                            break;
                        case AppC.F_SETPODDCUR:
                            if (drDet != null)
                            {
                                TryNextPoddon(new DataRow[] { drDet });
                            }
                            ret = true;
                            break;
                        case AppC.F_PRNDOC:
                            PrintEtikPoddon("", "", null);
                            ret = true;
                            break;
                        case AppC.F_SETPRN:
                            SetCurPrinter(true);
                            ret = true;
                            break;
                        case AppC.F_PODD:
                            CallFrmPars();
                            ret = true;
                            break;
                        case AppC.F_EXLDPALL:
                            if (drDet != null)
                            {
                                int nP = (int)drDet["NPODDZ"];
                                if (nP <= 0)
                                {
                                    drDet["NPODDZ"] = (nP == 0) ? -2 : 0;
                                }
                                else
                                    Srv.ErrorMsg(String.Format("��� � ������� {0}!", nP), true);
                            }
                            ret = true;
                            break;
                    }
                }
                else
                {// ��������� � ��� ��������������
                    switch (nFunc)
                    {
                        case AppC.F_PODD:
                            if (tMest.Focused == true)
                            {
                                int nM = int.Parse(tMest.Text);
                                nM = scCur.nMestPal * nM;
                                tMest.Text = nM.ToString();
                            }
                            ret = true;
                            break;
                    }
                }

            }
            else
            {
                if (bEditMode == true)
                {
                    switch (e.KeyValue)
                    {
                        case W32.VK_ESC:
                            //nCurEditCommand = AppC.CC_CANCEL;
                            ret = true;
                            EditEndDet(AppC.CC_CANCEL);
                            break;
                        case W32.VK_UP:
                        case W32.VK_DOWN:
                            aEdVvod.TryNext((e.KeyValue == W32.VK_UP) ? AppC.CC_PREV : AppC.CC_NEXT);
                            ret = true;
                            break;

                        case W32.VK_LEFT:
                        case W32.VK_RIGHT:
                            if ((aEdVvod.Current == tMest) && (scCur.xEmks.Count > 0))
                            {
                                if (scCur.xEmks.Count > 1)
                                {
                                    StrAndInt xS = null;
                                    if (scCur.xEmks.Current == null)
                                        xS = scCur.xEmks.MoveEx(Srv.Collect4Show<StrAndInt>.DIR_MOVE.FORWARD);
                                    else
                                    {
                                        if (e.KeyValue == W32.VK_LEFT)
                                            xS = scCur.xEmks.MoveEx(Srv.Collect4Show<StrAndInt>.DIR_MOVE.BACK);
                                        else
                                            xS = scCur.xEmks.MoveEx(Srv.Collect4Show<StrAndInt>.DIR_MOVE.FORWARD);
                                    }
                                    //if (scCur.xEmks.Current != null)
                                    //{
                                        scCur.fEmk = xS.DecDat;
                                        scCur.nTara = xS.SNameAdd1;
                                        scCur.nKolSht = xS.IntCodeAdd1;
                                        scCur.fVsego = scCur.nMest * scCur.fEmk;

                                        tEmk.Text = scCur.fEmk.ToString();
                                        tVsego.Text = scCur.fVsego.ToString();
                                    //}
                                    ret = true;
                                }
                            }
                            break;


                        case W32.VK_ENTER:
                            ret = true;
                            if (aEdVvod.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                                //if (bQuitEdVvod == true)
                                EditEndDet(AppC.CC_NEXTOVER);
                            break;
                        case W32.VK_TAB:
                            aEdVvod.TryNext((e.Shift == true) ? AppC.CC_PREV : AppC.CC_NEXT);
                            ret = true;
                            break;
                    }
                }
                else
                {// ��� ������ ���������
                    switch (e.KeyValue)
                    {
                        case W32.VK_ENTER:
                            if (xScrDet.CurReg == ScrMode.SCRMODES.FULLMAX)
                            {
                                if ((xCDoc.drCurRow != null) &&
                                    ((int)xCDoc.drCurRow["TYPOP"] == AppC.TYPOP_KMPL))
                                {
                                    SetDetFlt(NSI.BD_DOUTD, AppC.REG_SWITCH.SW_NEXT);
                                }
                            }
                            ret = true;
                            break;
                        case W32.VK_TAB:
                            CallFrmPars();
                            ret = true;
                            break;
                    }
                }
            }
            e.Handled = bSkipChar = ret;

            return (ret);
        }


        private void TryNextPoddon(DataRow[] drP)
        {
            int
                nOldP = xCDoc.xNPs.Current,
                nOperType = (int)xCDoc.drCurRow["TYPOP"];

                DialogResult 
                    drz = CallDllForm(sExeDir + "SGPF-PdSSCC.dll", true);
                if ((drz == DialogResult.OK) &&
                    (xCDoc.sSSCC.Length == 20))
                {
                    if (nOperType == AppC.TYPOP_KMPL)
                    {
                        if (xCDoc.xNPs.Current != nOldP)
                        {
                            lDocInf.Text = CurDocInf(xCDoc.xDocP);
                            if (xSm.FilterTTN == NSI.FILTRDET.NPODD)
                            {
                                //SetDetFlt(NSI.BD_DOUTD, AppC.REG_SWITCH.SW_SET);
                                //if (!bShowTTN)
                                SetDetFlt(NSI.BD_DOUTD, AppC.REG_SWITCH.SW_SET);
                            }
                            tCurrPoddon.Text = xCDoc.xNPs.Current.ToString();
                        }
                    }
                    else
                    {
                        PrintEtikPoddon(String.Format("PAR=(SSCC={0});", xCDoc.sSSCC), xCDoc.sSSCC, drP);
                    }
                }
        }



        // �������� ����
        private void tKMC_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if ((sT.Length > 0) && bEditMode)
            {
                try
                {
                    int nM = int.Parse(sT);
                    PSC_Types.ScDat sTmp = new PSC_Types.ScDat(new ScannerAll.BarcodeScannerEventArgs(BCId.Unknown,""));
                    if (true == xNSI.GetMCData("", ref sTmp, nM))
                    {
                        scCur = sTmp;
                        scCur.nRecSrc = (int)NSI.SRCDET.HANDS;
                        if (scCur.bVes)
                            TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt, ref scCur, 0);
                        else
                        {
                            //CheckEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt, ref scCur);

                            // ������� ��� ����������� ��� �������� �� ������� ����?
                            //CheckEmk(ref scCur);

                        }
                        SetDetFields(false);
                    }
                    else
                    {
                        e.Cancel = true;
                        Srv.ErrorMsg("��� � �����������!", "��� " + nM.ToString(), true);
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                scCur.nKrKMC = 0;
        }

        // �������� EAN
        private void tEAN_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    e.Cancel = !xNSI.GetMCDataOnEAN(sT, ref scCur, true);
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            scCur.sEAN = sT;
        }

        // �������� ��������� ������
        private void tParty_Validating(object sender, CancelEventArgs e)
        {
            string
                sT = tParty.Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    if (sT != scCur.nParty)
                    {
                        IsKeyFieldChanged(tParty, scCur.fEmk, sT);
                        scCur.nParty = sT;
                    }
                }
                catch { e.Cancel = true; }
            }
            else
                scCur.nParty = "";
        }

        // �������� ����
        private void tDatMC_Validating(object sender, CancelEventArgs e)
        {
            string sD = ((TextBox)sender).Text.Trim();
            if (sD.Length > 0)
            {
                try
                {
                    sD = Srv.SimpleDateTime(sD, Smena.DateDef);
                    DateTime d = DateTime.ParseExact(sD, "dd.MM.yy", null);
                    scCur.dDataIzg = d;
                    scCur.sDataIzg = sD;
                    ((TextBox)sender).Text = sD;
                    TryEvalNewZVKTTN(ref scCur, true);
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                scCur.dDataIzg = DateTime.MinValue;
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }



        private bool bMestChanged = false;
        // ��������� ��������� ����
        private void tMest_TextChanged(object sender, EventArgs e)
        {
            bMestChanged = true;
        }

        // �������� ��������� ����
        private void tMest_Validating(object sender, CancelEventArgs e)
        {
            string s, 
                sErr = "";
            int i, nDif,
                nM = 0;
            bool bGoodData = true;

            // �� ���������� �������������� ����� ��������
            if (!bEditMode)
                return;

            s = tMest.Text.Trim();
            if (s.Length > 0)
            {
                if (scCur.bAlienMC && !scCur.bNewAlienPInf && !tParty.Enabled && (s.Length >= 2))
                {// ������� �������� ��������� ������ � ������
                    if (s.Substring(0,2) == "00")
                    {
                        tParty.Enabled=false;
                        aEdVvod.TryNext(AppC.CC_PREV);
                        scCur.bNewAlienPInf = true;
                        tMest.Text = scCur.nMest.ToString();
                        bMestChanged = false;
                        return;
                    }
                }
                try
                {
                    nM = int.Parse(s);
                    if (nM < 0)
                        e.Cancel = true;
                }
                catch
                {
                    e.Cancel = true;
                }
            }

            if (e.Cancel != true)
            {
                if (scCur.bVes == true)
                {
                    bGoodData = (MestValid(ref nM) == AppC.RC_OK) ? true : false;
                    // ��������� ���������, ����� ����� ��������������, ����
                    // ��� �� ��������������� ������� ��������
                }
                else
                {// ������� ���������
                    if (nM == 0)
                    {// ������� ���������� � ����������, �� ����� - ���������� ��������
                        scCur.fEmk = 0;
                        tEmk.Text = "0";
                        tVsego.Enabled = true;
                        tEmk.Enabled = false;
                    }
                    else
                    {// ������������ ��������

                        // ������ ������� ������� ������� �� ��������� (����� ������������ � ��� ������ �����)
                        if ((xPars.aParsTypes[AppC.PRODTYPE_SHT].bMAX_Kol_EQ_Poddon) &&
                            ((nCurVvodState == AppC.F_ADD_SCAN) || (nCurVvodState == AppC.F_ADD_REC)))
                        {
                            if (xCDoc.xDocP.nTypD != AppC.TYPD_INV)
                            {
                                if (scCur.nMestPal > 0)
                                {// ������� �� ������ �������
                                    i = scCur.nMestPal;
                                }
                                else
                                {// ������� �� ������ �����������, �� ������ 200 �� ������ (���� ���)
                                    i = 200;
                                }
                                nDif = nM - i;
                                bGoodData = (nDif <= 0) ? true : false;
                                sErr = String.Format("����������({0}) �������({1})!", nDif, i);
                            }
                        }
                        if (bGoodData)
                        {// �������� �� ������������ ������
                            if (bZVKPresent)
                            {
                                if (nM > scCur.nMest)
                                {
                                    DialogResult dr = MessageBox.Show("�������� ���� (Enter)?\n(ESC) - ���������� ����",
                                        "���������� ������!",
                                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                    if (dr == DialogResult.OK)
                                        bGoodData = false;
                                }
                            }
                        }
                        else
                        {
                            Srv.ErrorMsg(sErr, true);
                        }
                        if (bGoodData)
                        {
                            // ������� �������� �� �������� ����� � ������ ?
                            if (scCur.fEmk == 0)
                            {
                                if ((scCur.tTyp == AppC.TYP_TARA.TARA_PODDON) && (!tEmk.Enabled) && (scCur.fVsego == 0))
                                {
                                    if (scCur.xEmks.Count > 1)
                                    {// ������ ������� �� ������
                                    }
                                }
                                else
                                {
                                    // ������� ����� ������� �� �����������
                                    FRACT dEmk = Math.Max(scCur.fEmk, scCur.fEmk_s);
                                    if (dEmk > 0)
                                    {// ���� ���� ������� - ��������� �����
                                        scCur.fEmk = dEmk;
                                        tEmk.Text = scCur.fEmk.ToString();
                                        scCur.fVsego = nM * scCur.fEmk;
                                        tVsego.Text = scCur.fVsego.ToString();
                                        tEmk.Enabled = false;
                                        tVsego.Enabled = false;
                                    }
                                    else
                                    {// ���� ��� - ������ ������
                                        tEmk.Enabled = true;
                                    }
                                }
                            }
                            else
                            {
                                if ((scCur.tTyp == AppC.TYP_TARA.TARA_PODDON) && (tEmk.Enabled) && (scCur.fVsego == 0))
                                {
                                }
                                else
                                {
                                    scCur.fVsego = nM * scCur.fEmk;
                                    tVsego.Text = scCur.fVsego.ToString();
                                    tEmk.Enabled = false;
                                    tVsego.Enabled = false;
                                }
                            }
                        }
                    }
                }
                if (bGoodData)
                {
                    scCur.nMest = nM;
                    //e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
                }
                else
                {
                    ((TextBox)sender).SelectAll();
                    e.Cancel = true;
                }
            }
        }


        // �������� ���� ��� ��������
        // nM - ��������� ����������
        // �����:
        // - RC_OK - ������� � ����������
        // - RC_CANCEL - �������� � ��������������
        // - RC_EDITEND - �������������� ��������, ���������� �����������
        private int MestValid(ref int nM)
        {
            int nRet = AppC.RC_OK;
            bool bDopInf = false;

            switch (nCurVvodState)
            {
                case AppC.F_ADD_SCAN:
                    switch (scCur.nTypVes)
                    {
                        case AppC.TYP_VES_PAL:
                            // ����� ���� �� �������
                            if (nM != 0)
                            {
                                if (scCur.fEmk == 0)
                                {
                                    bDopInf = TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt,
                                        ref scCur, scCur.fVes / nM);
                                    if ((bDopInf == true) && (scCur.nTypVes == AppC.TYP_VES_TUP))
                                    {// ������������ �������
                                        IsKeyFieldChanged(tEmk, scCur.fEmk, scCur.nParty);
                                        tEmk.Text = scCur.fEmk.ToString();
                                    }
                                    else
                                    {
                                        Srv.ErrorMsg("������� �� ����������!");
                                        nRet = AppC.RC_CANCEL;
                                    }
                                }

                            }
                            else
                                nRet = AppC.RC_CANCEL;
                            break;
                        case AppC.TYP_VES_1ED:
                            // ��������� ���������, ��� ��������� ���� (1) ��� ��� (0)
                            nM = (nM > 0) ? 1 : 0;
                            break;
                        case AppC.TYP_VES_TUP:
                        case AppC.TYP_PALET:
                            if ((nM != scCur.nMest) || (bMestChanged == true))
                            {// ��� - ������������� �� �������
                                scCur.fVsego = nM * scCur.fEmk;
                                tVsego.Text = scCur.fVsego.ToString();
                            }
                            break;
                    }
                    break;
                case AppC.F_CHG_REC:
                    //if ((bLastScan == true) && (nM == 0) && (nOldMest != nM))
                    //{// ������ ���������� ������������
                    //    //scCur.nMest = nM;
                    //    return (AppC.RC_OK);
                    //}
                    if (scCur.fEmk == 0)
                    {// ��� ��������� ����� ������ ���������/�������� ����
                        nM = (nM > 0) ? 1 : 0;
                    }
                    else
                    {
                        switch (scCur.nTypVes)
                        {
                            case AppC.TYP_VES_PAL:
                                break;
                            case AppC.TYP_VES_TUP:
                                if (((nM != scCur.nMest) || (bMestChanged == true)))
                                {// ��� �������������� ��� - ������������� �� �������
                                    if ((xCDoc != null) && (xCDoc.xDocP.nTypD == AppC.TYPD_INV))
                                    {
                                        scCur.fVsego = nM * scCur.fEmk;
                                        tVsego.Text = scCur.fVsego.ToString();
                                    }
                                }
                                else
                                    nRet = AppC.RC_CANCEL;
                                break;
                        }
                    }
                    break;
            }
            return (nRet);
        }

        // �������� �������
        private void tEmk_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    FRACT nM = FRACT.Parse(sT);
                    if (nM != scCur.fEmk)
                    {
                        IsKeyFieldChanged(tEmk, nM, scCur.nParty);
                        scCur.fEmk = nM;
                        // 
                        if (scCur.bVes == true)
                            TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt, ref scCur, 0);

                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                scCur.fEmk = 0;
            if (e.Cancel != true)
            {
                scCur.fVsego = scCur.nMest * scCur.fEmk;
                tVsego.Text = scCur.fVsego.ToString();
                if ((scCur.bVes == true) || (scCur.nMest == 0))
                    tVsego.Enabled = true;

                //e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
            }

        }

        // �������� ���������� ������ (�����)
        private void tVsego_Validating(object sender, CancelEventArgs e)
        {
            if (tVsego.Text.Trim().Length > 0)
            {
                try
                {
                    FRACT nM = FRACT.Parse(tVsego.Text);
                    if (nM > 0)
                    {
                        scCur.fVsego = nM;
                    }
                    else
                        e.Cancel = true;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                scCur.fVsego = 0;
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }

        private void IsKeyFieldChanged(TextBox tB, FRACT fVal, string nVal)
        {
            bool bNeedEval = false;     // �������� ����� ?
            if ((nCurVvodState == AppC.F_CHG_REC) || (bShowTTN == false))
            {
                return;
            }

            if (tB == tEmk)
            {// ������� ��������� � ������
                bNeedEval = true;
                EvalEnteredVals(ref scCur, scCur.sKMC, fVal, scCur.nParty, null, 0, 0);
            }
            else if (tB == tParty)
            {// ������� ����� ������
                bNeedEval = true;
                EvalEnteredVals(ref scCur, scCur.sKMC, scCur.fEmk, nVal, null, 0, 0);
            }

            if (bNeedEval == true)
            {
                SetDopFieldsForEnter(false);
            }
        }

        /// --- ������� ������ � ���������� �������� --- 
        ///

        // --- ���������� ����� ��� ��������� ������
        // nReg - ��������� �����
        private void AddOrChangeDet(int nReg)
        {
            bool bMayEdit = false;

            DataTable dtChg = ((DataTable)this.dgDet.DataSource);

            switch (nReg)
            {
                case AppC.F_ADD_REC:
                    //if (bShowTTN == true)
                    //{
                    //    bMayEdit = true;
                    //    scCur.nRecSrc = (int)NSI.SRCDET.HANDS;
                    //}

                    bMayEdit = true;
                    scCur.nRecSrc = (int)NSI.SRCDET.HANDS;
                    break;
                case AppC.F_ADD_SCAN:
                    bMayEdit = true;
                    break;
                case AppC.F_CHG_REC:
                    if ((dtChg.DefaultView.Count > 0))
                    {// ������ ��� ���, ������ ������
                        if ((xSm.sUser == AppC.SUSER) || (dtChg.TableName == NSI.BD_DOUTD))
                        {
                            bMayEdit = true;
                            if (scCur.bVes == true)
                            {
                                if ((scCur.fEmk != 0) && (scCur.nMest == 1))        // ������������ �� ��������������
                                    bMayEdit = false;
                            }
                        }
                    }
                    break;
            }
            if (bMayEdit == true)
                EditBeginDet(nReg, new AppC.VerifyEditFields(VerifyVvod));
        }

        class PartyInf
        {
            public string nParty;
            public DateTime dV;
            public PartyInf(string nP, DateTime d)
            {
                nParty = nP;
                dV = d;
            }
        }

        Dictionary<string, PartyInf> dicAlienP = new Dictionary<string,PartyInf>();

        private bool PInfReady()
        {
            bool ret = false,
                bFind = true;
            string sK = scCur.sKMC + scCur.sIntKod;
            try
            {
                PartyInf xPi = dicAlienP[sK];
                scCur.nParty = xPi.nParty;
                scCur.dDataIzg = xPi.dV;
                scCur.sDataIzg = xPi.dV.ToString("dd.MM.yy");
            }
            catch
            {
                bFind = false;
            }
            scCur.bNewAlienPInf = !bFind;
            if ((!bFind) || (scCur.nTypVes == AppC.TYP_VES_PAL))
            {
                ret = true;
            }
            return (ret);
        }

        /// ���� � ����� ��������/������������� ��������� ������ **********************
        /// - ��������� ����� ��������������
        /// - ��������� �����
        private void EditBeginDet(int nReg, AppC.VerifyEditFields dgV)
        {
            bool 
                bFlag;
            int 
                nProdType = (scCur.bVes == true) ? AppC.PRODTYPE_VES : AppC.PRODTYPE_SHT;

            nCurVvodState = nReg;
            SetEditMode(true);

            aEdVvod = new AppC.EditListC(dgV);
            aEdVvod.Fict4Next = tVvod_SHTReg;

            foreach (AppPars.FieldDef fd in xPars.aFields)
            {
                // ����� ������
                bFlag = 
                    (nReg == AppC.F_CHG_REC) ? fd.aVes[nProdType].bEdit :
                    (nReg == AppC.F_ADD_SCAN) ? fd.aVes[nProdType].bScan : 
                    fd.aVes[nProdType].bVvod;

                // � ����� ����� �������� � ������ � ���� ��������� ����� �� ����
                if ((scCur.bAlienMC) && (fd.sFieldName == "tParty"))
                {
                    bFlag = (scCur.bNewAlienPInf || (scCur.nTypVes == AppC.TYP_VES_PAL)) ? true : false;
                }

                if (scCur.bVes)
                {
                    if (fd.sFieldName == "tDatMC")
                    {
                        // ��� ����� ���-13 ���� ������� �� ����, � � ����� ����� ������ �������
                        if ((scCur.dDataIzg == DateTime.MinValue) &&
                            (!bFlag) && 
                            (scCur.nParty.Length == 0) &&
                            (nReg == AppC.F_ADD_SCAN))
                            bFlag = true;
                    }
                }
                if (scCur.bFindNSI && (scCur.ci == BCId.EAN13))
                {
                    if (((fd.sFieldName == "tDatMC") && ((scCur.dDataIzg == DateTime.MinValue))) ||
                        ((fd.sFieldName == "tParty") && (scCur.nParty.Length == 0)))
                    {
                            bFlag = true;
                    }
                }

                aEdVvod.AddC(FieldByName(fd.sFieldName), bFlag);
            }

            if (scCur.bVes)
            {
                //if (scCur.fEmk == 0)
                //    tEmk.Enabled = true;
                if (scCur.fVes == 0)
                    tVsego.Enabled = true;
            }
            if (scCur.tTyp == AppC.TYP_TARA.TARA_PODDON)
            {
                if ((scCur.fEmk == 0) && (scCur.fEmk_s != 0))
                {// ���������� ������� ������� ����� �� �������
                    scCur.fEmk = scCur.fEmk_s;
                    tEmk.Text = scCur.fEmk.ToString();
                    aEdVvod.SetAvail(tEmk, true);
                }
            }

            aEdVvod.WhichSetCur();

            nDefMest = scCur.nMest;
            fDefEmk = scCur.fEmk;
            fDefVsego = scCur.fVsego;

            bMestChanged = false;
        }

        // ������� ������� �� ��� �����
        private Control FieldByName(string s)
        {
            return ((s == "tKMC") ? this.tKMC :
                (s == "tParty") ? this.tParty :
                (s == "tEAN") ? this.tEAN :
                (s == "tDatMC") ? this.tDatMC :
                (s == "tMest") ? this.tMest :
                (s == "tEmk") ? this.tEmk : this.tVsego);
        }

        private bool IsTara(string sEAN, int nKrKMC)
        {
            bool
                ret = false;

            if (string.IsNullOrEmpty(sEAN))
            {// ����� �� ��������
                if (((nKrKMC >= 1) && (nKrKMC <= 8)) ||
                      (nKrKMC == 41))
                {
                    ret = true;
                }
            }
            else
            {// ����� �� EAN
                switch(sEAN)
                {
                    case "4100000000041":
                    case "2010050100023":
                    case "2010050100207":
                    case "2010050100313":
                    case "2010050100405":
                    case "2010050100436":
                    case "2010050100474":
                    case "2010050200174":
                    case "2010050300089":
                        ret = true;
                        break;
                }
            }
            return (ret);
        }


        // �������� ��������� ������ �� ������������
        private AppC.VerRet VerifyVvod()
        {
            int 
                nRet = AppC.RC_OK;
            string 
                sSaved = aEdVvod.Current.Text;

            AppC.VerRet v;

            #region �������� ������������ ��������� �����
            do
            {
                if ((scCur.nRecSrc == (int)NSI.SRCDET.HANDS) && (xCDoc.bConfScan))
                {
                    nRet = TestProdBySrv(ref scCur);
                    if (nRet != AppC.RC_OK)
                    {
                        nRet = AppC.RC_CANCEL;
                        EditEndDet(AppC.CC_CANCEL);
                        break;
                    }

                }

                //switch (nCurVvodState)
                //{
                //    case AppC.F_ADD_SCAN:

                //        if (scCur.bVes == true)
                //        {// ������� ���������
                //        }
                //        else
                //        {// ������� ���������
                //        }
                //        break;
                //    case AppC.F_CHG_REC:
                //        // �������������� �������� - ������� ���������� ���������
                //        if (bShowTTN == true)
                //        {
                //        }
                //        break;
                //}

                if (scCur.fVsego <= 0)
                {
                    Srv.ErrorMsg("��������� ����������!");
                    nRet = AppC.RC_CANCEL;
                }

                if (scCur.nParty.Length == 0)
                {
                    if (!IsTara("", scCur.nKrKMC))
                    {
                        if (scCur.nNPredMT == 0)
                        {// ��� �� ��������, ��� ���������
                            Srv.ErrorMsg("������ �� �������!");
                            nRet = AppC.RC_CANCEL;
                            break;
                        }
                    }
                }

                if (scCur.dDataIzg == DateTime.MinValue)
                {
                    if ((scCur.ci != BCId.EAN13) || (bEditMode == false))
                    {
                        if (!IsTara("", scCur.nKrKMC))
                        {
                            Srv.ErrorMsg("���� ��������� ?!");
                            nRet = AppC.RC_CANCEL;
                            break;
                        }
                    }
                }
            } while (false);
            #endregion

            //if (nRet == AppC.RC_OK)
            //    bQuitEdVvod = true;

            v.nRet = nRet;
            v.cWhereFocus = null;
            aEdVvod.Current.Text = sSaved;
            return (v);

        }

        // ��������� �����/��������������
        private void EditEndDet(int nReg)
        {
            bool bReRead = false;

            if (nReg == AppC.CC_NEXTOVER)
            {// �������� ��������� �����
                switch (nCurVvodState)
                {
                    case AppC.F_ADD_REC:
                    case AppC.F_ADD_SCAN:
                        // ������������ ������ ������������ ������ ���
                        bReRead= !AddDet1(ref scCur);

                        if (bShowTTN == true)
                            SetDopFieldsForEnter(true);

                        break;
                    case AppC.F_CHG_REC:
                        SaveDetChange(1);
                        if ((bShowTTN == true) && (bLastScan == true))
                            SetDopFieldsForEnter(false);
                        break;
                }
                if (scCur.bAlienMC && scCur.bNewAlienPInf)
                {
                    string sK = scCur.sKMC + scCur.sIntKod;
                    if (dicAlienP.ContainsKey(sK))
                    {
                        dicAlienP[sK].nParty = scCur.nParty;
                        dicAlienP[sK].dV = scCur.dDataIzg;
                    }
                    else
                        dicAlienP.Add(sK, new PartyInf(scCur.nParty, scCur.dDataIzg));
                }
            }
            else
                bReRead = true;

            SetEditMode(false);
            //for (int i = 0; i < aEdVvod.Count; i++)
            //{
            //    aEdVvod[i].Enabled = false;
            //}

            if (bReRead == true)
                ChangeDetRow(true);

            ShowStatDoc();


            if ((nCurVvodState == AppC.F_ADD_SCAN) && (nReg == AppC.CC_NEXTOVER))
                bLastScan = true;

            nCurVvodState = AppC.DT_SHOW;
            if (xCDoc.bTmpEdit)
            {
                SetEasyEdit(AppC.REG_SWITCH.SW_SET);
                xCDoc.bTmpEdit = false;
            }
            aEdVvod.EditIsOver(dgDet);
            //dgDet.Focus();
        }

        // ���������� ������ � ��������� ������ ���
        //private bool AddDet1(ref PSC_Types.ScDat scForAdd)
        //{
        //    bool bNewRec = false;

        //    // ��������� ����� ��� ��������� � ������������
        //    DataRow dr = null;
        //    if (xCDoc.xDocP.nTypD != AppC.TYPD_OPR)
        //        dr = WhatRegAdd(ref scForAdd);

        //    if (dr == null)
        //        bNewRec = true;

        //    nOldMest = scForAdd.nMest;
        //    fOldVsego = scForAdd.fVsego;
        //    fOldVes = scForAdd.fVes;


        //    if (scForAdd.nRecSrc == (int)NSI.SRCDET.HANDS)
        //        scForAdd.dtScan = DateTime.Now;
        //    dr = xNSI.AddDet(scForAdd, xCDoc, dr);

        //    EvalZVKState(ref scForAdd);

        //    if (bShowTTN == true)
        //    {// ������ �� �����������/�����������������
        //        if (drDet == null)
        //            drDet = dr;

        //        int nOldRec = GetRecNoInGrid(dr);
        //        if (nOldRec != -1)
        //            dgDet.CurrentRowIndex = nOldRec;
        //    }

        //    ShowStatDoc();

        //    AfterAddScan(this, new EventArgs());
        //    return (bNewRec);
        //}

        // ���������� ������ � ��������� ������ ���
        private bool AddDet1(ref PSC_Types.ScDat scForAdd)
        { 
            DataRow d = null;
            return( AddDet1(ref scForAdd, out d) );
        }

        private bool AddDet1(ref PSC_Types.ScDat scForAdd, out DataRow dr)
        {
            bool bNewRec = false;

            // ��������� ����� ��� ��������� � ������������
            //dr = null;
            //if (xCDoc.xDocP.nTypD != AppC.TYPD_OPR)
            dr = WhatRegAdd(ref scForAdd);

            if (dr == null)
                bNewRec = true;

            nOldMest = scForAdd.nMest;
            fOldVsego = scForAdd.fVsego;
            fOldVes = scForAdd.fVes;


            if (scForAdd.nRecSrc == (int)NSI.SRCDET.HANDS)
                scForAdd.dtScan = DateTime.Now;
            dr = xNSI.AddDet(scForAdd, xCDoc, dr);

            //EvalZVKState(ref scForAdd);
            EvalZVKStateNew(ref scForAdd);

            if (bShowTTN == true)
            {// ������ �� �����������/�����������������
                if (drDet == null)
                    drDet = dr;

                int nOldRec = GetRecNoInGrid(dr);
                if (nOldRec != -1)
                    dgDet.CurrentRowIndex = nOldRec;
            }

            ShowStatDoc();

            AfterAddScan(this, new EventArgs());
            return (bNewRec);
        }

















        private int GetRecNoInGrid(DataRow dr)
        {
            int nPos = -1;
            DataView dv = ((DataTable)dgDet.DataSource).DefaultView;
            for (int i = 0; i < dv.Count; i++)
            {
                if (dv[i].Row == dr)
                {
                    nPos = i;
                    break;
                }
            }

            return(nPos);
        }

        // ����������� ������ ���������� ��������������� ��������: ����� ������ ��� ����������
        private DataRow WhatRegAdd(ref PSC_Types.ScDat sc)
        {
            int nDocType = xCDoc.xDocP.nTypD;
            DataRow ret = null;

            if ( 
                //((xPars.parVvodVESNewRec == true) && (sc.bVes == true)) ||
                //((xPars.aParsTypes[1].bAddNewRow == true) && (sc.bVes == true)) ||
                ((xPars.aDocPars[nDocType].bSumVes == false) && (sc.bVes == true)) ||
                (nDocType == AppC.TYPD_OPR) ||
                (nDocType == AppC.TYPD_BRK) ||
                (xCDoc.nTypOp == AppC.TYPOP_KMPL))
                ret = null; 
            else
            {
                if (sc.fEmk == 0)
                    ret = sc.drEd;
                else
                    ret = sc.drMest;
                if (ret != null)
                {
                    if (xCDoc.nTypOp == AppC.TYPOP_DOCUM)
                    {
                        if ((int)ret["NPODDZ"] > 0)
                        {// � �������������� ������� �� ����������
                            ret = null;
                        }
                    }
                }
            }

            return (ret);
        }

        // ��������� ������� ������ ����� �����/�������������
        private void EvalZVKStateNew(ref PSC_Types.ScDat sc)
        {
            int 
                nAddMest = 0,
                nMz = 0;
            FRACT 
                fAddEd = 0,
                fVz = 0;

            if (bZVKPresent == true)
            {// ������ �������

                #region ��� �� �������� ������ ����� ����������
                do
                {

                    if (sc.fEmk > 0)
                    {// ��� ��������� ��������� �����
                        nMz = sc.nMest;
                        if (sc.drTotKey != null)
                        {// ��� ����� ���� ����� ������� ���������� ������
                            nMz = (int)sc.drTotKey["KOLM"] - (sc.nKolM_alrT + nMz);
                            if (nMz <= 0)
                                // ������ �����������
                                sc.drTotKey["READYZ"] = NSI.READINESS.FULL_READY;
                            if (nMz >= 0)
                                // ������ ������������ ������
                                break;
                            nMz = Math.Abs(nMz);
                        }
                        else
                            nAddMest = sc.nKolM_alrT;

                        if (sc.drPartKey != null)
                        {// ������� �������, ����� ������ �����
                            nMz = (int)sc.drPartKey["KOLM"] - (sc.nKolM_alr + nAddMest + nMz);
                            if (nMz <= 0)
                                // ������ �����������
                                sc.drPartKey["READYZ"] = NSI.READINESS.FULL_READY;
                        }
                    }
                    else
                    {// ��� ��������� ��������� �������
                        fVz = sc.fVsego;
                        if (sc.drTotKeyE != null)
                        {// ��� ����� ������ ����� ������� ���������� ������
                            fVz = (FRACT)sc.drTotKeyE["KOLE"] - (sc.fKolE_alrT + fVz);
                            if (fVz <= 0)
                                // ������ �����������
                                sc.drTotKeyE["READYZ"] = NSI.READINESS.FULL_READY;
                            if (fVz >= 0)
                                // ������ ������������ ������
                                break;
                            fVz = Math.Abs(fVz);
                        }
                        else
                            fAddEd = sc.fKolE_alrT;
                        if (sc.drPartKeyE != null)
                        {// ��� ����� ������ ����� �� � �������
                            fVz = (FRACT)sc.drPartKeyE["KOLE"] - (sc.fKolE_alr + fAddEd + fVz);
                            if (fVz <= 0)
                                // ������ �����������
                                sc.drPartKeyE["READYZ"] = NSI.READINESS.FULL_READY;
                        }
                    }
                } while (false);

                #endregion

            }
        }









        // ���������� �������������
        private int SaveDetChange(int nReg)
        {
            int nRet = AppC.RC_OK;
            int nM;
            FRACT fV, fVess;

            try
            {
                if (scCur.bFindNSI == true)
                {
                    drDet["KRKMC"] = (int)scCur.nKrKMC;
                    drDet["SNM"] = scCur.sN;
                    drDet["EAN13"] = (string)scCur.sEAN;
                }

                if ((nOldMest != scCur.nMest) || (fOldVsego != scCur.fVsego))
                    ClearZVKState(scCur.sKMC);

                if (bLastScan == true)
                {// ������������� ��������� ��������� ������
                    if (((scCur.nMest == 0) && (nOldMest != scCur.nMest)) ||
                        ((scCur.nMest == 0) && (scCur.fVsego == 0.0M)))
                    {// ������ ���������� ������������
                        scCur.fVsego = 0;
                        scCur.fVes = 0;
                    }
                    nM = ((int)drDet["KOLM"] - nOldMest) + scCur.nMest;
                    nOldMest = scCur.nMest;
                    fV = ((FRACT)drDet["KOLE"] - fOldVsego) + scCur.fVsego;
                    fOldVsego = scCur.fVsego;
                    fVess = ((FRACT)drDet["VES"] - fOldVes) + scCur.fVes;
                    fOldVes = scCur.fVes;
                }
                else
                {
                    nM = scCur.nMest;
                    fV = scCur.fVsego;
                    fVess = scCur.fVes;
                }

                drDet["NP"] = scCur.nParty;
                drDet["EMK"] = scCur.fEmk;

                drDet["KOLM"] = nM;
                drDet["KOLE"] = fV;
            }
            catch
            {
                MessageBox.Show("Err Updating!");
            }

            return (nRet);

        }

        private void ZVKStyle()
        {
        }


        // --- ����� �������
        private void ChgDetTable(DataRow drNew, string sNeededTable)
        {
            string sRf = xCDoc.DefDetFilter();
            NSI.TABLESORT ts = NSI.TABLESORT.NO;
            DataGridCell dgCur = dgDet.CurrentCell;

            dgDet.SuspendLayout();
            if (((bShowTTN == false) && (sNeededTable == "")) ||
                (sNeededTable == NSI.BD_DOUTD) )
            {// ������� - ������, ��������������� ���
                dgDet.DataSource = xNSI.DT[NSI.BD_DOUTD].dt;
                tVvodReg.Text = "���";
                bShowTTN = true;
                ts = xNSI.DT[NSI.BD_DOUTD].TSort;
                if ((int)xCDoc.drCurRow["TYPOP"] == AppC.TYPOP_KMPL)
                {
                    if (xCDoc.xNPs.Current > 0)
                    {
                        if (xNSI.DT[NSI.BD_DOUTD].sTFilt != "")
                            sRf += xNSI.DT[NSI.BD_DOUTD].sTFilt;
                    }
                }
            }
            else
            {// ������ ������� - ���, ��������������� �����
                dgDet.DataSource = xNSI.DT[NSI.BD_DIND].dt;
                tVvodReg.Text = "������";
                bShowTTN = false;
                ts = xNSI.DT[NSI.BD_DIND].TSort;
                sRf += xNSI.DT[NSI.BD_DIND].sTFilt;
                if ((xCDoc.nTypOp == AppC.TYPOP_KMPL) || 
                    (xCDoc.nTypOp == AppC.TYPOP_OTGR))
                {
                    xNSI.ChgGridStyle(NSI.BD_DIND, NSI.GDET_ZVK_KMPL);
                    if (xCDoc.xNPs.Current > 0)
                    {
                        if (xNSI.DT[NSI.BD_DOUTD].sTFilt != "")
                            sRf += xNSI.DT[NSI.BD_DOUTD].sTFilt;
                    }
                }
                else
                {
                    xNSI.ChgGridStyle(NSI.BD_DIND, NSI.GDET_ZVK);
                }

            }

            ((DataTable)dgDet.DataSource).DefaultView.RowFilter = sRf;
            AdjustCurRow(dgCur, drNew);

            xNSI.SortName(bShowTTN, ref sRf);
            lSortInf.Text = sRf;
            dgDet.ResumeLayout();
            ShowStatDoc();
        }

        // ���������������� �� ����� DataRow � �����
        private void AdjustCurRow(DataGridCell dgOldCell, DataRow drNew)
        {
            bool bRowChanged = false;
            int nI = dgDet.CurrentRowIndex;
            DataGridCell dgCur = dgDet.CurrentCell;

            CurrencyManager cmDet = (CurrencyManager)BindingContext[dgDet.DataSource];
            int nOldPos = cmDet.Position;

            if (drNew != null)
            {
                nOldPos = GetRecNoInGrid(drNew);
            }

            if (((dgDet.VisibleRowCount > 0)&& ((nOldPos) >= dgDet.VisibleRowCount)) || (drNew != null))
            {
                //dgDet.CurrentRowIndex = 0;
                if (nOldPos > -1)
                {
                    dgDet.CurrentRowIndex = nOldPos;
                    bRowChanged = true;
                }
                else
                    Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
            }
            if (bRowChanged == false)
            {
                //if (dgOldCell.Equals(dgCur) == true)
                    ChangeDetRow(true);
            }
        }

        // --- ����� �������� ���������
        private void SetNextPrevDoc(int nFunc)
        {
            bool bChanged = false;
            CurrencyManager cmDoc = (CurrencyManager)BindingContext[dgDoc.DataSource];
            if (cmDoc.Count > 0)
            {
                if (nFunc == AppC.F_PREVDOC)
                {
                    if (cmDoc.Position > 0)
                    {
                        cmDoc.Position--;
                        bChanged = true;
                    }
                }
                else
                {
                    if (cmDoc.Position < cmDoc.Count - 1)
                    {
                        cmDoc.Position++;
                        bChanged = true;
                    }
                }
                if (bChanged == true)
                {
                    xCDoc.drCurRow = ((DataRowView)cmDoc.Current).Row;
                    xNSI.InitCurDoc(xCDoc, xSm);
                    SetParFields(xCDoc.xDocP);

                    NewDoc((DataTable)this.dgDet.DataSource);
                    lDocInf.Text = CurDocInf(xCDoc.xDocP);
                }
            }
        }

        // --- �������� ��������� ������
        private void DelDetDoc(int nFunc)
        {
            DataTable dtDel = ((DataTable)this.dgDet.DataSource);
            //!!! if (dtDel == xNSI.DT[NSI.BD_DOUTD].dt)
                
            if (dtDel == xNSI.DT[NSI.BD_DOUTD].dt)
                {
                DataView dvDetail = dtDel.DefaultView;
                int ret = dvDetail.Count;
                if (ret >= 1)
                {
                    if (nFunc == AppC.F_DEL_REC)
                    {
                        ClearZVKState(dvDetail[this.dgDet.CurrentRowIndex].Row["KMC"].ToString());
                        dtDel.Rows.Remove(dvDetail[this.dgDet.CurrentRowIndex].Row);
                    }
                    else
                    {
                        DialogResult dr = MessageBox.Show("�������� �������� ���� (Enter)?\r\n(ESC) - ��� ������� ��� ��������",
                            "��������� ��� ������!",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dr != DialogResult.OK)
                        {
                            DataRow[] drMDetZ = xCDoc.drCurRow.GetChildRows(xNSI.dsM.Relations[NSI.REL2TTN]);
                            foreach (DataRow drDel in drMDetZ)
                            {
                                xNSI.dsM.Tables[NSI.BD_DOUTD].Rows.Remove(drDel);
                            }
                            ClearZVKState("");
                        }
                    }
                    ChangeDetRow(false);
                    xCDoc.xOper = new CurOper();
                }
            }
        }

        // ����� ������� ������
        private void ClearZVKState(string sKMC)
        {
            // ������ - SYSN + EAN13
            string sRf = ((DataTable)dgDet.DataSource).DefaultView.RowFilter;
            if (sKMC != "")
                sRf += String.Format("AND(KMC='{0}')", sKMC);

            // ��� ��������� � ������ ����� �� ������
            DataView dv = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "", DataViewRowState.CurrentRows);
            for (int i = 0; i < dv.Count; i++)
            {
                dv[i].Row["READYZ"] = NSI.READINESS.NO;
            }
        }


        private void ShowTotMest()
        {
            FRACT fV = 0;
            try
            {
                string s = "���� �� ��� - " + TotMest(NSI.REL2TTN, out fV).ToString() + "\n";
                string sV = "������� ��� - " + fV.ToString() + "\n";

                s += "���� �� ������ - " + TotMest(NSI.REL2ZVK, out fV).ToString() + "\n" +
                      sV +
                     "������� ������ - " + fV.ToString();

                MessageBox.Show(s);
            }
            catch { }
        }

        // ����� ���� �� ������ ��������� (������ ��� ���)
        private int TotMest(string sRel, out FRACT fTotVes)
        {
            int nMTTN = 0;

            fTotVes = 0;
            try
            {
                DataRow[] chR = xCDoc.drCurRow.GetChildRows(sRel);
                foreach (DataRow dr in chR)
                {
                    nMTTN += (int)dr["KOLM"];
                    if ((int)dr["SRP"] > 0)
                    {
                        fTotVes += (FRACT)dr["KOLE"];
                    }
                }
            }
            catch { }
            return (nMTTN);
        }



        private void ShowTotMestAll(int nK, string nP)
        {
            int nM,
                nMK, nMKP;
            FRACT fV = 0,
                fVK = 0, fVKP;
            try
            {

                nM = TotMestAll(NSI.REL2TTN, nK, nP, out fV, 
                    out nMK, out fVK,
                    out nMKP, out fVKP);

                string sKP = String.Format("{0} �.� {1} ���� {2}\n               ��. {3}", nK, nP, nMKP, fVKP);
                string sK = String.Format("{0}        ���� {1}\n               ��. {2}", nK, nMK, fVK);
                string sM = String.Format("����� ���� {0} \n����� ��� {1}", nM, fV);

                string s = sKP + "\n" + sK + "\n" + sM + "\n" + "===== ������ =====" + "\n";


                nM = TotMestAll(NSI.REL2ZVK, nK, nP, out fV,
                    out nMK, out fVK,
                    out nMKP, out fVKP);

                sKP = String.Format("{0} �.� {1} ���� {2}\n               ��. {3}", nK, nP, nMKP, fVKP);
                sK = String.Format("{0}        ���� {1}\n               ��. {2}", nK, nMK, fVK);
                sM = String.Format("����� ���� {0} \n����� ��� {1}", nM, fV);

                s += sKP + "\n" + sK + "\n" + sM;

                MessageBox.Show(s);
            }
            catch { }
        }

        private int TotMestAll(string sRel, int nKrKMC, string nParty, out FRACT fTotVes,
                        out int nMKrKMC, out FRACT fTotVesKrKMC,
                        out int nMKrKMCP, out FRACT fTotVesKrKMCP)
        {
            int nMTTN = 0;
            fTotVes = 0;

                        
            nMKrKMC = nMKrKMCP = 0;
            fTotVesKrKMC = fTotVesKrKMCP = 0;

            try
            {
                DataRow[] chR = xCDoc.drCurRow.GetChildRows(sRel);
                foreach (DataRow dr in chR)
                {
                    nMTTN += (int)dr["KOLM"];
                    if ((int)dr["SRP"] > 0)
                    {
                        fTotVes += (FRACT)dr["KOLE"];
                    }
                    if (nKrKMC == (int)dr["KRKMC"])
                    {
                        nMKrKMC += (int)dr["KOLM"];
                        fTotVesKrKMC += (FRACT)dr["KOLE"];
                        if (nParty == (string)dr["NP"])
                        {
                            nMKrKMCP += (int)dr["KOLM"];
                            fTotVesKrKMCP += (FRACT)dr["KOLE"];
                        }
                    }
                }
            }
            catch { }
            return (nMTTN);
        }






        // �������� ����� ��������� �� ������������ ������
        //private void ControlDocZVK_Old(DataRow drD, List<string> lstProt)
        //{
        //    int i = 0, 
        //        iStart,
        //        iCur,
        //        iTMax, iZMax,
        //        nDokState = AppC.RC_OK,
        //        nRet;

        //    //TimeSpan tsDiff;
        //    //int t1 = Environment.TickCount, t2, t3, td1, td2, tc = 0, tc1 = 0, tc2 = 0;
        //    //t2 = t1;

        //    if (drD == null)
        //        drD = xCDoc.drCurRow;

        //    string sRf = String.Format("(SYSN={0})", drD["SYSN"]);


        //    PSC_Types.ScDat sc = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
        //    lstProt.Add(HeadLineCtrl(drD));

        //    // ���� ��� ��������� �������� ����������
        //    drD["DIFF"] = NSI.DOCCTRL.UNKNOWN;

        //    // ��� ��������� �� ������ �� ���������
        //    DataView dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "KRKMC", DataViewRowState.CurrentRows);
        //    iZMax = dvZ.Count;
        //    if (iZMax <= 0)
        //    {
        //        nDokState = AppC.RC_CANCEL;
        //        lstProt.Add("*** ������ �����������! ***");
        //    }

        //    // ��� ��������� �� ��� �� ���������
        //    DataView dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KRKMC,EMK DESC", DataViewRowState.CurrentRows);
        //    iTMax = dvT.Count;
        //    if (iTMax <= 0)
        //    {
        //        nDokState = AppC.RC_CANCEL;
        //        lstProt.Add("*** ��� �����������! ***");
        //    }
        //    dvZ.BeginInit();
        //    dvT.BeginInit();

        //    if (nDokState == AppC.RC_OK)
        //    {
        //        foreach (DataRowView dr in dvZ)
        //        {// ����� ���� ��������
        //            dr["READYZ"] = NSI.READINESS.NO;
        //        }
                
        //        lstProt.Add("<->----- ��� ------<->");
                  
        //        while(i < iTMax)
        //        {
        //                sc.sKMCFull = (string)dvT[i]["EAN13"];
        //                if (sc.sKMCFull.Length <= 0)
        //                {
        //                    i++;
        //                    continue;
        //                }
        //                sc.nKrKMC = (int)dvT[i]["KRKMC"];

        //                sc.bVes = ((int)(dvT[i]["SRP"]) > 0) ? true : false;

        //                sc.fEmk = (FRACT)dvT[i]["EMK"];
        //                sc.nParty = (int)dvT[i]["NP"];
        //                sc.dDataIzg = DateTime.ParseExact((string)dvT[i]["DVR"], "yyyyMMdd", null);
        //                sc.nTara = (int)dvT[i]["KRKT"];

        //                //td1 = Environment.TickCount;

        //                iStart = dvZ.Find(sc.nKrKMC);
        //                if (iStart != -1)
        //                    nRet = EvalZVKMest(ref sc, dvZ, iStart, iZMax);
        //                else
        //                    nRet = AppC.RC_NOEAN;

        //                //tc += (Environment.TickCount - td1);

        //                iCur = -1;
        //                if (nRet == AppC.RC_OK)
        //                {// ���� ��� �������� ��� ����� �������
        //                    //td1 = Environment.TickCount;

        //                    EvalEnteredVals(ref sc, sc.sKMCFull, sc.fEmk, sc.nParty, dvT, i, iTMax);

        //                    //td2 = Environment.TickCount;
        //                    //tc1 += (td2 - td1);


        //                    iCur = i;
        //                    nRet = EvalDiffZVK(ref sc, dvZ, dvT, lstProt, iStart, iZMax, ref iCur, iTMax);

        //                    //tc2 += (Environment.TickCount - td2);

        //                    if (nDokState != AppC.RC_CANCEL)
        //                    {
        //                        if (nRet != AppC.RC_OK)
        //                            nDokState = nRet;
        //                    }
        //                }
        //                else if (nRet == AppC.RC_NOEAN)
        //                {// ��� �����������
        //                    nDokState = AppC.RC_CANCEL;
        //                    lstProt.Add(String.Format("_��� {0}:��� � ������", sc.nKrKMC));
        //                    iCur = SetTTNState(dvT, sc.nKrKMC, -100, NSI.DESTINPROD.USER, i, iTMax);
        //                }
        //                else if (nRet == AppC.RC_NOEANEMK)
        //                {// ������� �����������
        //                    nDokState = AppC.RC_CANCEL;
        //                    lstProt.Add(String.Format("_{0} ������� {1}:��� � ������", sc.nKrKMC, sc.fEmk));
        //                    iCur = SetTTNState(dvT, sc.nKrKMC, sc.fEmk, NSI.DESTINPROD.USER, i, iTMax);
        //                }
        //                if (iCur != -1)
        //                    i = iCur;

        //            i++;
        //        }

        //        //t2 = Environment.TickCount;

        //        lstProt.Add("<->---- ������ ----<->");
        //        for (i = 0; i < dvZ.Count; i++)
        //        {
        //            if ((NSI.READINESS)dvZ[i]["READYZ"] == NSI.READINESS.NO)
        //            {
        //                nDokState = AppC.RC_CANCEL;
        //                try
        //                {
        //                    if ((FRACT)dvZ[i]["EMK"] > 0)
        //                        lstProt.Add(String.Format("_{0}:��� �����-{1} �",
        //                            (int)dvZ[i]["KRKMC"], (int)dvZ[i]["KOLM"]));
        //                    else
        //                        lstProt.Add(String.Format("_{0}:��� �����-{1} ��",
        //                            (int)dvZ[i]["KRKMC"], (FRACT)dvZ[i]["KOLE"]));
        //                }
        //                catch
        //                {
        //                }
        //            }
        //        }
        //    }

        //    if (nDokState == AppC.RC_CANCEL)
        //    {
        //        drD["DIFF"] = NSI.DOCCTRL.ERRS;
        //        lstProt.Add("!!!===! ������ �������� !===!!!");
        //    }
        //    else if (nDokState == AppC.RC_WARN)
        //    {
        //        drD["DIFF"] = NSI.DOCCTRL.WARNS;
        //        lstProt.Add("== ��������� - �������������� ==");
        //    }
        //    else if (nDokState == AppC.RC_OK)
        //    {
        //        drD["DIFF"] = NSI.DOCCTRL.OK;
        //        lstProt.Add("=== ��������� - ��� ������ ===");
        //    }

        //    dvT.EndInit();
        //    dvZ.EndInit();

        //    //t3 = Environment.TickCount;
        //    //tsDiff = new TimeSpan(0, 0, 0, 0, t3 - t1);

        //    //lstProt.Add(String.Format("����� - {0}, ������ - {1}, ZVK-{2}, TTN-{3}, Diff-{4}",
        //    //    tsDiff.TotalSeconds,
        //    //    new TimeSpan(0, 0, 0, 0, t3 - t2).TotalSeconds,
        //    //    new TimeSpan(0, 0, 0, 0, tc).TotalSeconds,
        //    //    new TimeSpan(0, 0, 0, 0, tc1).TotalSeconds,
        //    //    new TimeSpan(0, 0, 0, 0, tc2).TotalSeconds));
        //    //MessageBox.Show(new TimeSpan(0, 0, 0, 0, tss).TotalSeconds.ToString());
        //}



        private string HeadLineCtrl(DataRow dr)
        {
            int 
                nT = AppC.EMPTY_INT;
            string 
                s = "",
                sData = "",
                sSmena = " ��:",
                sEks = " ���: ",
                sPol = " ���: ";

            try
            {
                nT = (int)dr["TD"];
                //s = DocPars.TypName(ref nT) + ":";
                s = DocPars.TypDName(ref nT) + ":";

                sData = (string)dr["EXPR_DT"];
                sSmena += (string)dr["KSMEN"];
                sEks += dr["KEKS"].ToString();
                sPol += dr["KRKPP"].ToString();
            }
            catch
            {
            }

            s += sData;

            switch(nT){
                case AppC.TYPD_SVOD:
                    s += sEks + sSmena;
                    break;
                case AppC.TYPD_CVYV:
                    s += sEks + sSmena + sPol ;
                    break;
                case AppC.TYPD_SAM:
                    s += sSmena + sPol;
                    break;
                case AppC.TYPD_VPER:
                    s += sSmena + sPol;
                    break;
            }
            return (s);
        }

        // ������� ����� ������� � ���������
        private int EvalDiffZVK(ref PSC_Types.ScDat sc, DataView dvZ, DataView dvT, List<string> lstProt,
            int iZ, int iZMax, ref int iT, int iTMax)
        {
            bool bNeedSetZVK = false;
            int nRet = AppC.RC_OK;
            int nM = 0;
            FRACT fV = 0;
            NSI.READINESS rpEmk = NSI.READINESS.NO;

            if (sc.fEmk > 0)
            {
                if (sc.nKolM_zvk > 0)
                {
                    bNeedSetZVK = true;
                    nM = sc.nKolM_zvk - (sc.nKolM_alr + sc.nKolM_alrT);
                    if (nM > 0)
                    {// ����-�� ��� ��������, �� ������ ������ �� ���������
                        nRet = AppC.RC_CANCEL;
                        rpEmk = NSI.READINESS.PART_READY;
                        lstProt.Add(String.Format("_{0}:���������-{1} �",
                            sc.nKrKMC, nM));
                    }
                    else
                    {
                        if (nM < 0)
                        {// ������� �� ������, ���������
                            nRet = AppC.RC_WARN;
                            lstProt.Add(String.Format(" {0}:������ {1} �",
                                sc.nKrKMC, Math.Abs(nM)));
                        }
                        rpEmk = NSI.READINESS.FULL_READY;
                    }
                }
                else
                {
                    nRet = AppC.RC_CANCEL;
                    rpEmk = NSI.READINESS.PART_READY;
                    lstProt.Add(String.Format("_{0}:��� � ������-{1} �",
                        sc.nKrKMC, (sc.nKolM_alr + sc.nKolM_alrT)));
                }

                // ��������� ��������� � ��������
                try
                {
                    iT = SetTTNState(dvT, sc.nKrKMC, sc.fEmk, NSI.DESTINPROD.PARTZ, iT, iTMax);
                    if (bNeedSetZVK == true)
                    {
                        SetZVKState(dvZ, sc.nKrKMC, sc.fEmk, rpEmk, iZ, iZMax);
                        //while ((iZ < iZMax) && ((int)dvZ[iZ]["KRKMC"] == sc.nKrKMC))
                        //{
                        //    if (sc.fEmk == (FRACT)dvZ[iZ]["EMK"])
                        //        dvZ[iZ]["READYZ"] = rpEmk;
                        //    iZ++;
                        //}
                    }
                }
                catch { }


            }
            else
            {
                if ((sc.fKolE_zvk > 0) || ((sc.fKolE_alr + sc.fKolE_alrT) > 0))
                {// ���� ��� ������ ��� �����
                    if (sc.fKolE_zvk > 0)
                    {
                        bNeedSetZVK = true;
                        fV = sc.fKolE_zvk - (sc.fKolE_alr + sc.fKolE_alrT);

                        if (fV > 0)
                        {// ����-�� ��� ��������
                            nRet = AppC.RC_CANCEL;
                            rpEmk = NSI.READINESS.PART_READY;
                            lstProt.Add(String.Format("_{0}:���������-{1} ��",
                                sc.nKrKMC, fV));
                        }
                        else
                        {
                            if (fV < 0)
                            {// ������� �� ���������, ���������
                                nRet = AppC.RC_WARN;
                                lstProt.Add(String.Format(" {0}:������{1} ��",
                                    sc.nKrKMC, Math.Abs(fV)));
                            }
                            rpEmk = NSI.READINESS.FULL_READY;
                        }
                    }
                    else
                    {
                        nRet = AppC.RC_CANCEL;
                        rpEmk = NSI.READINESS.PART_READY;
                        lstProt.Add(String.Format("_{0}:��� � ������-{1} ��",
                            sc.nKrKMC, (sc.fKolE_alr + sc.fKolE_alrT)));
                    }

                    try
                    {
                        iT = SetTTNState(dvT, sc.nKrKMC, sc.fEmk, NSI.DESTINPROD.PARTZ, iT, iTMax);
                        if (bNeedSetZVK == true)
                        {
                            SetZVKState(dvZ, sc.nKrKMC, sc.fEmk, rpEmk, iZ, iZMax);
                            //while ((iZ < iZMax) && ((int)dvZ[iZ]["KRKMC"] == sc.nKrKMC))
                            //{
                            //    if (sc.fEmk == (FRACT)dvZ[iZ]["EMK"])
                            //        dvZ[iZ]["READYZ"] = rpEmk;
                            //    iZ++;
                            //}
                        }
                    }
                    catch { }
                }
            }

            return (nRet);
        }

        private void SetZVKState(DataView dv, int nK, FRACT fE, NSI.READINESS rpE, int i, int nZMax)
        {
            while ((i < nZMax) && ((int)dv[i]["KRKMC"] == nK))
            {
                if (fE == (FRACT)dv[i]["EMK"])
                    dv[i]["READYZ"] = rpE;
                i++;
            }
        }

        // ��������� ��������
        // - �� ����� ���� (��� RC_NOEAN), fE = -100
        // - �� ����� ���� � ������ ������� (��� RC_NOEAN)
        private int SetTTNState(DataView dv, int nK, FRACT fE, NSI.DESTINPROD dSt, int i, int iMax)
        {
            //int tss1 = Environment.TickCount;
            int nLastI = -1;
            while ((i < iMax) && ((int)dv[i]["KRKMC"] == nK)) 
            {
                if ((fE == -100) || (fE == (FRACT)dv[i]["EMK"]))
                {
                    dv[i]["DEST"] = dSt;
                    nLastI = i;
                }
                i++;
            }
            //tss += (Environment.TickCount - tss1);
            return (nLastI);
        }
        //int tss = 0;





        // ������� �� ����� �� ��� � ������� ���������
        private void GoSameKMC()
        {
            DataRow drNew = null;
            if (drDet != null)
            {// ���� ��� ������
                try
                {
                    object[] xF = new object[] { (int)drDet["SYSN"], (int)drDet["KRKMC"] };

                    DataView dvEn = (bShowTTN == true) ? new DataView(xNSI.DT[NSI.BD_DIND].dt) :
                        new DataView(xNSI.DT[NSI.BD_DOUTD].dt);
                    dvEn.Sort = "SYSN,KRKMC";
                    int nR = dvEn.Find(xF);
                    if (nR > -1)
                        drNew = dvEn[nR].Row;
                    else
                    {
                        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                    }
                }
                catch { }
            }
            ChgDetTable(drNew, "");
        }

        // ������� �� ����� � �����������
        private void ZVK2TTN()
        {
            DataRow drNew = null;
            if ((drDet != null) && (bZVKPresent == true) && (bShowTTN == false))
            {// ���� ��� ������
                try
                {
                    // ���������� ����� �� �����
                    object[] xF = new object[] { (int)drDet["SYSN"], (int)drDet["KRKMC"], (FRACT)drDet["EMK"], 
                    (string)drDet["NP"]};

                    DataView dvEn = new DataView(xNSI.DT[NSI.BD_DOUTD].dt);
                    dvEn.Sort = "SYSN,KRKMC,EMK,NP";
                    int nR = dvEn.Find(xF);

                    if (nR > -1)
                    {// ��� ���� ����� ���
                        drNew = dvEn[nR].Row;
                        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                        ChgDetTable(drNew, "");
                    }
                    else
                    {
                        DataRow drN = xNSI.DT[NSI.BD_DOUTD].dt.NewRow();
                        drN["SYSN"] = drDet["SYSN"];
                        drN["KRKMC"] = drDet["KRKMC"];
                        drN["SNM"] = drDet["SNM"];
                        drN["KOLM"] = drDet["KOLM"];
                        drN["KOLE"] = drDet["KOLE"];
                        drN["EMK"] = drDet["EMK"];
                        drN["NP"] = drDet["NP"];
                        drN["DVR"] = drDet["DVR"];
                        drN["EAN13"] = drDet["EAN13"];
                        drN["SRP"] = drDet["SRP"];
                        drN["KMC"] = drDet["KMC"];
                        drN["DEST"] = (int)NSI.DESTINPROD.USER;
                        drN["SRC"] = (int)NSI.SRCDET.FROMZ;
                        drN["TIMECR"] = DateTime.Now;

                        xNSI.DT[NSI.BD_DOUTD].dt.Rows.Add(drN);
                        Srv.PlayMelody(W32.MB_4HIGH_FLY);
                        MessageBox.Show("����������...");
                    }
                }
                catch { }
            }
        }






        // �������� �������������� � �������� ������
        // ����������� ����� ��������� �����
        class ScrMode
        {
            // ������ �����������
            public enum SCRMODES : int
            {
                NORMAL      = 0,                        // 4-��������� �����������
                FULLMAX     = 1                         // ���� ����� ���������
            }

            private Point[] xLoc;
            private Size[] xSize;
            private Control[] xParent;
            private int[] nTabI;
            
            private SCRMODES nCur = SCRMODES.NORMAL;
            private int nMaxReg = 2;

            // ����
            private Control xCtrl;

            public ScrMode(Control xC)
            {
                xCtrl = xC;
                xLoc  = new Point[] { 
                    new Point(xC.Location.X, xC.Location.Y), 
                    new Point(0, 0) };

                xSize = new Size[]{ 
                    new Size(xC.Size.Width, xC.Size.Height),
                    new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height - 24) };

                xParent = new Control[] { 
                    xC.Parent, 
                    xC.TopLevelControl };

                nTabI = new int[] { xC.TabIndex, 0 };
                nCur = SCRMODES.NORMAL;
            }

            // ������� �����
            public SCRMODES CurReg
            {
                get { return (nCur); }
            }


            // ������������ �� ��������� �����
            public void NextReg(AppC.REG_SWITCH rgSW)
            {
                if (rgSW == AppC.REG_SWITCH.SW_NEXT)
                {
                    nCur++;
                    if ((int)nCur == nMaxReg)
                        nCur = 0;
                }
                else
                    nCur = (rgSW == AppC.REG_SWITCH.SW_SET) ? SCRMODES.FULLMAX : SCRMODES.NORMAL;

                xCtrl.SuspendLayout();
                xCtrl.Parent = xParent[(int)nCur];
                xCtrl.TabIndex = nTabI[(int)nCur];
                xCtrl.Location = xLoc[(int)nCur];
                xCtrl.Size = xSize[(int)nCur];
                if (nCur != SCRMODES.NORMAL)
                    xCtrl.BringToFront();
                
                xCtrl.ResumeLayout();
                xCtrl.Focus();
            }

            //public void SetDefault()
            //{
            //    SetDefault(false);
            //}
            //public void SetDefault(bool bFocus)
            //{
            //    nCur = 0;
            //    xCtrl.SuspendLayout();
            //    xCtrl.Parent = xParent[(int)nCur];
            //    xCtrl.TabIndex = nTabI[(int)nCur];
            //    xCtrl.Location = xLoc[(int)nCur];
            //    xCtrl.Size = xSize[(int)nCur];
            //    xCtrl.ResumeLayout();
            //    if (bFocus == true)
            //        xCtrl.Focus();
            //}
        }

        // ����������� ������ �����
        private int IsGeneralEdit(ref PSC_Types.ScDat sc)
        {
            int nRet = AppC.RC_OK;
            int nM = 0;
            FRACT fV = 0;

            if (xScrDet.CurReg == ScrMode.SCRMODES.FULLMAX)
            {// ������������� �����
                if (bZVKPresent == true)
                {// ������ �������
                    if (bShowTTN == false)
                    {// ������� - ������
                        nRet = AppC.RC_CANCEL;
                        if (((sc.nMest > 0) || (sc.fVsego > 0)) && (sc.nDest != NSI.DESTINPROD.USER))
                        {
                            DataRow dr = null;
                            if (sc.nMest > 0)
                            {
                                if (sc.drTotKey != null)
                                {// ��� ����� ���� ����� ������� ���������� ������
                                    nM = (int)sc.drTotKey["KOLM"] - (sc.nKolM_alrT + sc.nMest);
                                    if (nM >= 0)
                                    {
                                        if ((nM == 0) || (sc.bVes == true))
                                            dr = sc.drTotKey;
                                    }
                                }
                                if ((sc.drPartKey != null) && (dr == null))
                                {// ��� ����� ���� ����� �������
                                    nM = (int)sc.drPartKey["KOLM"] - (sc.nKolM_alr + sc.nMest);
                                    if (nM >= 0)
                                    {
                                        if ((nM == 0) || (sc.bVes == true))
                                            dr = sc.drPartKey;
                                    }
                                }
                                if (dr != null)
                                {// ����� ��������� �����
                                    if (sc.bVes == false)
                                        sc.fVsego = sc.nMest * sc.fEmk;
                                }
                            }
                            else
                            {
                                if (sc.drTotKeyE != null)
                                {// ��� ����� ������ ����� ������� ���������� ������
                                    fV = (FRACT)sc.drTotKeyE["KOLE"] - (sc.fKolE_alrT + sc.fVsego);
                                    if (fV >= 0)
                                    {
                                        if ((fV == 0) || (sc.bVes == true))
                                            dr = sc.drTotKeyE;
                                    }
                                }

                                if ((sc.drPartKeyE != null) && (dr == null))
                                {// ��� ����� ������ ����� �������
                                    fV = (FRACT)sc.drPartKeyE["KOLE"] - (sc.fKolE_alr + sc.fVsego);
                                    if (fV >= 0)
                                    {
                                        if ((fV == 0) || (sc.bVes == true))
                                            dr = sc.drPartKeyE;
                                    }
                                }
                                if (dr != null)
                                {// ����� ��������� ��������
                                    sc.fEmk = 0;
                                }
                            }
                            if (dr != null)
                            {
                                nRet = AppC.RC_ALLREADY;
                                PSC_Types.ScDat scOld = sc;
                                drEasyEdit = dr;
                                dgDet.CurrentRowIndex = GetRecNoInGrid(dr);

                                scCur = scOld;

                                if (bInEasyEditWait == false)
                                {
                                    bInEasyEditWait = true;
                                    ehCurrFunc += new Srv.CurrFuncKeyHandler(ZVKeyDown);
                                }
                                dgDet.Invalidate();
                            }
                        }
                    }
                    else
                        nRet = AppC.RC_BADTABLE;
                }
                else
                    nRet = AppC.RC_ZVKONLY;
            }
            return (nRet);
        }

        public static DataRow drEasyEdit = null;
        public static bool bInEasyEditWait = false;

        //private bool ZVKeyDown(int nFunc, KeyEventArgs e, ref PSC_Types.ScDat scN)
        //{
        //    if (scN.sTypDoc == scCur.sTypDoc)
        //    {
        //        nFunc = AppC.F_ZVK2TTN;
        //        //scCur = scN;
        //    }
        //    return (ZVKeyDown(nFunc, null));
        //}


        private bool ZVKeyDown(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            bool bKeyHandled = true,
                bWriteData = false,
                bCloseEdit = true;
            int 
                nFunc = (int)nF,
                nNum = 0;

            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case AppC.F_ZVK2TTN:
                        // ������������� ����� ������
                        bWriteData = true;
                        break;
                    case AppC.F_HELP:
                        bCloseEdit = false;
                        bKeyHandled = false;
                        break;
                    case AppC.F_PODD:
                        xCDoc.bTmpEdit = true;
                        bKeyHandled = false;
                        break;
                }
            }
            else
            {
                if (Srv.IsDigKey(e, ref nNum))
                {
                    bKeyHandled = false;
                    xCDoc.bTmpEdit = true;
                }
                else
                {
                    switch (e.KeyValue)
                    {
                        case W32.VK_RIGHT:
                        case W32.VK_LEFT:
                            bCloseEdit = false;
                            break;
                        case W32.VK_ENTER:
                            // ���� �� ��� ������?
                            if (VerifyVvod().nRet == AppC.RC_CANCEL)
                                xCDoc.bTmpEdit = true;
                            else
                                bWriteData = true;
                            break;
                    }
                }
            }
            if (bCloseEdit == true)
            {
                drEasyEdit = null;
                if (bWriteData == true)
                {
                    Srv.PlayMelody(W32.MB_1MIDDL_HAND);
                    int nOldPos = dgDet.CurrentRowIndex;
                    bInEasyEditWait = false;

                    AddDet1(ref scCur);
                    int nNewPos = dgDet.CurrentRowIndex;
                    if (nOldPos != nNewPos)
                    {
                        if (dgDet.VisibleRowCount > 0)
                        {
                            //nOldPos = ((nOldPos + 1) >= dgDet.VisibleRowCount) ? dgDet.VisibleRowCount - 1 : nOldPos;
                            //dgDet.CurrentRowIndex = nOldPos;
                            //bRowChanged = true;
                        }
                    }
                    //if (bRowChanged == false)
                    //{
                    //    //if (dgOldCell.Equals(dgCur) == true)
                    //    ChangeDetRow(true);
                    //}


                }
                else
                {
                    Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                    Srv.PlayMelody(W32.MB_4HIGH_FLY);
                }
                ehCurrFunc -= ZVKeyDown;
                bInEasyEditWait = false;
                dgDet.Invalidate();
                if (xCDoc.bTmpEdit)
                {// ������� � ������� �����
                    PSC_Types.ScDat scS = scCur;
                    SetEasyEdit(AppC.REG_SWITCH.SW_CLEAR);
                    scCur = scS;
                    SetDetFields(false);
                    SetDopFieldsForEnter(false);
                    AddOrChangeDet(AppC.F_ADD_SCAN);
                    if (nFunc != AppC.F_PODD)
                        W32.SimulKey(e.KeyValue, e.KeyValue);
                }

            }
            return (bKeyHandled);
        }

        private void SetDetFlt(AppC.REG_SWITCH rg)
        {
            string s = ((bShowTTN == false)&&(bZVKPresent))?NSI.BD_DIND : NSI.BD_DOUTD;
            SetDetFlt(s, rg);
        }

        // ������ ��� ������� ��������� �� �������
        private void SetDetFlt(string sT, AppC.REG_SWITCH bForceSet)
        {
            string
                sFP = "";
            DataTable dt = ((DataTable)dgDet.DataSource);

            // ����������� ������������ �� ��������� ����� (���������/�����)
            dgDet.SuspendLayout();
            int nCurPoddon = 0;
            try
            {
                nCurPoddon = xCDoc.xNPs.Current;
            }
            catch { nCurPoddon = 0; }
            if (nCurPoddon > 0)
                sFP = String.Format("AND(NPODDZ={0})", nCurPoddon);

            //sF = DefDetFilter();
            if ( ((xNSI.DT[sT].sTFilt == "") && (bForceSet == AppC.REG_SWITCH.SW_NEXT)) ||
                (bForceSet == AppC.REG_SWITCH.SW_SET))

            //if ((((xNSI.DT[NSI.BD_DOUTD].sTFilt == "")||
            //     ((sT == NSI.BD_DIND) && (xCDoc.FilterZVK == NSI.FILTRDET.UNFILTERED))) && 
            //    (bForceSet == AppC.REG_SWITCH.SW_NEXT)) ||
            //    (bForceSet == AppC.REG_SWITCH.SW_SET))
            {// ����������� ���������
                if (sT == NSI.BD_DOUTD)
                {// ��������� ��� ��� ������ � ��� ������
                    xNSI.DT[NSI.BD_DOUTD].sTFilt = xNSI.DT[NSI.BD_DIND].sTFilt = sFP;
                    xSm.FilterTTN = NSI.FILTRDET.NPODD;
                    if (xSm.FilterZVK == NSI.FILTRDET.READYZ)
                        xNSI.DT[NSI.BD_DIND].sTFilt += String.Format("AND(READYZ<>{0})", (int)NSI.READINESS.FULL_READY);
                }
                if (sT == NSI.BD_DIND)
                {
                    xNSI.DT[sT].sTFilt = xNSI.DT[NSI.BD_DOUTD].sTFilt + 
                        String.Format("AND(READYZ<>{0})", (int)NSI.READINESS.FULL_READY);
                    xSm.FilterZVK = NSI.FILTRDET.READYZ;
                }
                //if ((sT == NSI.BD_DOUTD) ||
                //    (((DataTable)dgDet.DataSource).TableName == sT))
                //    ((DataTable)dgDet.DataSource).DefaultView.RowFilter = sF;
            }
            else
            {// ����������� �����

                if (sT == NSI.BD_DOUTD)
                {// ����� ��� ��� � ��� ������
                    xNSI.DT[NSI.BD_DOUTD].sTFilt = xNSI.DT[NSI.BD_DIND].sTFilt = "";
                    xSm.FilterTTN = NSI.FILTRDET.UNFILTERED;
                    if (xSm.FilterZVK == NSI.FILTRDET.READYZ)
                        xNSI.DT[NSI.BD_DIND].sTFilt =
                            String.Format("AND(READYZ<>{0})", (int)NSI.READINESS.FULL_READY);
                }
                if (sT == NSI.BD_DIND)
                {
                    xNSI.DT[sT].sTFilt = "";
                    if (xSm.FilterTTN == NSI.FILTRDET.NPODD)
                        xNSI.DT[sT].sTFilt += sFP;
                    xSm.FilterZVK = NSI.FILTRDET.UNFILTERED;
                }
            }
            if ((sT == NSI.BD_DOUTD) || (dt.TableName == sT))
                dt.DefaultView.RowFilter = xCDoc.DefDetFilter() + xNSI.DT[dt.TableName].sTFilt;
            xNSI.SortName(bShowTTN, ref sFP);
            lSortInf.Text = sFP;
            dgDet.ResumeLayout();
        }











        // ���������/����� ������ ����������� �����
        private void SetEasyEdit(AppC.REG_SWITCH rgSW)
        {
            if (((xScrDet.CurReg == 0) && (rgSW == AppC.REG_SWITCH.SW_NEXT)) ||
                (rgSW == AppC.REG_SWITCH.SW_SET))
                {// ���� ������� �����, ������������ � �������������
                if (bZVKPresent == true)
                {// ������ �������
                    if (bShowTTN == true)
                        ChgDetTable(null, "");
                    //SetFltVyp(true);
                    SetDetFlt(NSI.BD_DIND, AppC.REG_SWITCH.SW_SET);
                    if (xScrDet.CurReg == 0)
                        xScrDet.NextReg(AppC.REG_SWITCH.SW_SET);
                }
            }
            else
            {// ������������� �����, ������������ � �������
                if (bShowTTN == false)
                    ChgDetTable(null, "");
                //SetFltVyp(false);
                SetDetFlt(NSI.BD_DOUTD, AppC.REG_SWITCH.SW_CLEAR);
                xScrDet.NextReg(AppC.REG_SWITCH.SW_CLEAR);
                ShowRegVvod();
            }
        }



        #region NOT_USED
        /*
         * 
        // ����� �������� �������� � ������
        private int SearchActControl(out int iP, out int iN, out int iFst, out int iL)
        {
            bool bSearchNext = false;
            int iPrev = -1, iNext = -1, iTmp = -1;
            int iFirst = -1, iLast = -1;
            int ret = -1;
            for (int i = 0; i < aEdVvod.Count; i++)
            {
                if (aEdVvod[i].xControl.Enabled == true)
                {// ������ ��� ���������
                    if (iFirst == -1)
                        iFirst = i;
                    if (aEdVvod[i].xControl.Focused == true)  // ���� ������� �������� �������
                    {
                        ret = i;
                        iPrev = iTmp;
                        bSearchNext = true;
                    }
                    else
                    {
                        if (bSearchNext == true)
                        {
                            bSearchNext = false;
                            iNext = i;
                        }
                        iTmp = i;
                    }
                    iLast = i;
                }
            }
            iP = iPrev;
            iN = iNext;
            iFst = iFirst;
            iL = iLast;
            return (ret);
        }


        // ����������� ����� Controls ��������� ����������
        private bool SetNextControl(bool bNext)
        {
            int nPrev, nNext, nFirst, nLast;
            bool ret = true;                                                            // ���� ����-�� ������� ��������
            int i = SearchActControl(out nPrev, out nNext, out nFirst, out nLast);      // ������� ������

            if (bNext == false)
            {// ������� �� ����������
                if (nPrev >= 0)
                    aEdVvod[nPrev].xControl.Focus();
                else
                {
                    if (nLast > i)
                        aEdVvod[nLast].xControl.Focus();
                    else
                        ret = false;        // � ��� ���� ��� - ������ ����
                }
            }
            else
            {// ������� �� ���������
                if (nNext >= 0)
                    aEdVvod[nNext].xControl.Focus();
                else
                {// ����� �� ���������
                    if (nFirst >= 0)
                        aEdVvod[nFirst].xControl.Focus();
                    else
                        ret = false;        // � ��� ���� ��� - ������ ����
                }
            }
            return (ret);
        }

        private void EditEndDet()
        {
            RestShowVvod(false);
        }

         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */
        #endregion



    }
}
