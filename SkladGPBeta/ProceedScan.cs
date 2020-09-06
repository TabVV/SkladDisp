using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;

using ScannerAll;
using PDA.OS;
using PDA.BarCode;
using PDA.Service;

using FRACT = System.Decimal;

namespace SkladGP
{
    public class ScanVarGP : ScanVar
    {
        // ����� ���������� ������� ���������
        [Flags]
        public enum BCTyp : int
        {
            UNKNOWN = 0,                // �������������
            SP_OLD_ETIK = 1,                // ����������� �� ������
            SP_NEW_ETIK = 2,                // ����������� �� ������

            SP_SSCC_INT = 4,                // ���������� SSCC
            SP_SSCC_EXT = 8,                // �������� SSCC
            SP_SSCC     = 16,               // ����� SSCC

            //SP_ADR_OBJ      = 16,               // ����� (������)
            SP_ADR_OBJ = 32,               // ����� (������)
            //SP_ADR_STLG = 32,                   // ����� (�������� ��� ��������)
            //SP_ADR_ZONE     = 64,               // ����� (����)

            SNT_GTIN_OLD = 128,              // �����-������
            SNT_GTIN_NEW = 256,              // �����-�����
            CSDR_GTIN = 512,              // ��������-�����
            CSDR_DOC = 1024,             // ��������-��������

            SP_MT_PRDV = 2048,             // ������������ ��� ����������
            SP_MT_PRDVN = 4096,             // ������������ ��� ���������� (�����)

            SP_SSCC_PRT = 8192              // �������� SSCC ������
        }

        private string
            SP_GLN = "4810268";

        private BarcodeScannerEventArgs
            m_SavedArgs;



        //private DataTable dtAI;

        public bool TFullBC(ScanVar x)
        {
            bool ret = false;
            if (x.dicSc.ContainsKey("02") &&
                x.dicSc.ContainsKey("11"))
                ret = true;
            //x.FullData = ret;
            return (ret);
        }


        public ScanVarGP(BarcodeScannerEventArgs e) : this(e, null) { }

        public ScanVarGP(BarcodeScannerEventArgs e, DataTable t)
            : base(t)
        {
            Id = e.nID;
            Dat = e.Data;
            m_SavedArgs = e;
            //dtAI = t;
            WhatBC();
        }

        public BCId Id;
        public string Dat;

        public BCTyp
            bcFlags = BCTyp.UNKNOWN;

        public bool bGoodParse = false;
        public bool bSPOldEtik = false;

        public DateTime ScanDTime
        {
            get { return m_SavedArgs.ScanDTime; }
        }

        public void WhatBC()
        {
            int nAI = 0;
            string
                sPart,
                s;

            if ((Id == BCId.Code128) || true)
            {
                try
                {
                    try
                    {
                        base.ScanParse(Dat);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.IndexOf("��") == 0)
                            throw new Exception(e.Message, e);
                        if (Dat.StartsWith("99") && (Dat.Length == 12))
                        {
                            base.dicSc.Clear();
                            base.dicSc.Add("99", new OneFieldBC("ADR", Dat.Substring(2), Dat.Substring(2), "C", "ADR"));
                        }
                    }
                    nAI = base.dicSc.Count;
                    if ((nAI == 1) && base.dicSc.ContainsKey("11") && (base.dicSc["11"].xV == null))
                        // ������ ������ �������� ��� ����������
                        nAI = 0;
                    if (nAI > 0)
                    {
                        switch (nAI)
                        {
                            case 1:
                            case 2:
                                if ((nAI == 1) && Dat.StartsWith("9"))
                                {// �������� �������� ?
                                    if (base.dicSc.ContainsKey("99") ||         // ��������-���
                                        base.dicSc.ContainsKey("91"))           // HB-������
                                    {
                                        bcFlags = BCTyp.SP_ADR_OBJ;
                                    }
                                    else if (base.dicSc.ContainsKey("959"))
                                    {// SSCC_PARTY
                                        bcFlags = BCTyp.SP_SSCC_PRT;
                                    }
                                    break;
                                }

                                if (Dat.Length == 38)
                                {// ����� ������ �� ������ ���
                                    if (Dat.StartsWith("02"))
                                    {
                                        s = Dat.Substring(16);
                                        if ((s.Length == 22) &&
                                            (s.Substring(6, 2) == "11") &&
                                            (s.Substring(14, 2) == "37"))
                                            bcFlags |= BCTyp.SP_OLD_ETIK;
                                    }
                                }
                                else
                                {
                                    if ((Dat.Length == 20) && (nAI == 1) && (base.dicSc.ContainsKey("00")))
                                        //&& (Dat.Substring(3).StartsWith(SP_GLN)))
                                    {
                                        bcFlags = BCTyp.SP_SSCC;
                                        if (Dat.Substring(2).StartsWith("1"))
                                            // ������� SSCC
                                            bcFlags |= BCTyp.SP_SSCC_EXT;
                                        else
                                            bcFlags |= BCTyp.SP_SSCC_INT;
                                    }
                                }
                                break;
                            case 5:
                                if ((base.dicSc.ContainsKey("02")) || (base.dicSc.ContainsKey("01")))
                                {
                                    if (Dat.Length > 38)
                                    {
                                        if ((base.dicSc.ContainsKey("11")) &&
                                            (base.dicSc.ContainsKey("23")) &&
                                            (base.dicSc.ContainsKey("37")) &&
                                            (base.dicSc.ContainsKey("10")))
                                        {
                                            bcFlags |= BCTyp.SP_NEW_ETIK;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        sPart = Dat.Substring(0, 2);
                        switch (Id)
                        {
                            case BCId.Code128:
                                switch (sPart)
                                {
                                    case "11":
                                        if (Dat.Length == 38)
                                        {
                                            bcFlags |= BCTyp.SP_MT_PRDV;
                                        }
                                        break;
                                    case "52":
                                        if (Dat.Length == 40)
                                        {
                                            bcFlags |= BCTyp.SP_MT_PRDVN;
                                        }
                                        break;
                                    case "53":
                                        if (Dat.Length == 34)
                                        {
                                            bcFlags |= BCTyp.SP_MT_PRDVN;
                                        }
                                        break;
                                }
                                break;
                        }
                    }

                }
                catch// (Exception e)
                {
                    bGoodParse = false;
                }
            }
            //return (AppC.TYP_BC_OLD);
            //nRet = AppC.TYP_BC_NEW;
            // �������� ������������ ������������
        }
    }

    public partial class MainF : Form
    {

        /// ��������� ������������ � ��������
        private void SpecScan(ScanVarGP xSc)
        {
            string
                s;

            xCLoad = null;
            switch (nSpecAdrWait)
            {

                case AppC.F_CHKSSCC:
                case AppC.F_CNTSSCC:
                    if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0)
                    {
                        xCDoc.sSSCC = xSc.Dat;
                        xFPan.UpdateReg(xCDoc.sSSCC);
                        s = "Enter-�� �����, Tab-�������� ������";
                        xFPan.UpdateHelp(s);
                    }
                    break;
                case AppC.F_GENSCAN:
                    xFPan.UpdateReg(xSc.Dat);
                    xFPan.UpdateHelp(String.Format("���-{0} �����={1} AI={2}", xSc.Id.ToString(), xSc.Dat.Length, xSc.dicSc.Count));
                    break;
                case AppC.F_REFILL:
                    if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0)
                    {
                        xSm.xAdrForSpec = new AddrInfo(xSc, xSm.nSklad);
                        xFPan.UpdateReg(xSm.xAdrForSpec.AddrShow);
                        xFPan.UpdateHelp("Enter - ���������");
                    }
                    break;
                case AppC.F_SETADRZONE:
                    // ������� �������� ������
                    if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0)
                    {// ����� ���� ��� �������
                        xSm.xAdrForSpec = new AddrInfo(xSc, xSm.nSklad);
                        xFPan.UpdateReg(String.Format("{0:20}...", xSm.xAdrForSpec.AddrShow));
                        xFPan.UpdateHelp("Enter - ������������� �����");
                    }
                    break;
                case AppC.F_CELLINF:
                    // ������� ��������� ����������� ������
                    if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0)
                    {// ����� ���� ��� �������
                        xSm.xAdrForSpec = new AddrInfo(xSc, xSm.nSklad);
                        xFPan.UpdateReg(xSm.xAdrForSpec.AddrShow);
                        s = (xCDoc.xDocP.nTypD == AppC.TYPD_OPR)?
                            "Enter-�� �����":
                            "Enter-�� �����, Tab-�������� ������";
                        xFPan.UpdateHelp(s);
                    }
                    break;
                case AppC.F_CLRCELL:
                    // ������� ����������� ������
                    if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0)
                    {// ����� ���� ��� �������
                        xSm.xAdrForSpec = new AddrInfo(xSc, xSm.nSklad);
                        xFPan.UpdateReg(xSm.xAdrForSpec.AddrShow);
                        xFPan.UpdateHelp("Enter - �������� �����   ESC - �����");
                    }
                    break;
            }
        }


        ScanVarGP 
            xScan, xScanPrev = null;

        private bool 
            bInScanProceed = false;


        private bool IsDoc4Check()
        {
            bool ChkDoc = false;
            try
            {
                ChkDoc = ((int)xCDoc.drCurRow["SSCCONLY"] > 0) ? true : false;
            }
            catch
            {
                ChkDoc = false;
            }

            return (ChkDoc);
        }

        // ��������� ������������ ������������
        private void OnScan(object sender, BarcodeScannerEventArgs e)
        {
            bool 
                bRet = AppC.RC_CANCELB,
                bNewDetAdded = false,
                bNewBarcode = false,
                bEasyEd,
                bDupScan;
            int 
                t1,t2=0,
                nRet = AppC.RC_CANCEL;
            string
                sP,
                sErr = "",
                sP1;

            if (bInScanProceed)
                return;

            // �������� ��������� ������������
            bInScanProceed = true;

            // ������ ��� �������������� ���������
            ClearAttentionInfo();

            if (e.nID != BCId.NoData)
            {
                try
                {
                    xScan = new ScanVarGP(e, xNSI.DT["NS_AI"].dt);
                    bDupScan = ((xScanPrev != null) && (xScanPrev.Dat == xScan.Dat)) ? true : false;

                    #region ��������� �����
                    do
                    {
                        if (nSpecAdrWait > 0)
                        {
                            SpecScan(xScan);
                            break;
                        }

                        if (xPars.WarnNewScan == true)
                        {// �������������� ��� ������������� �����
                            if ((bInEasyEditWait && !bDupScan) || (bEditMode == true))
                            {
                                Srv.ErrorMsg("��������� ����!", true);
                                break;
                            }
                        }

                        PSC_Types.ScDat sc = new PSC_Types.ScDat(e, xCDoc.xOper, xScan);
                        sc.sN = e.Data + "-???";

                        switch (tcMain.SelectedIndex)
                        {
                            case PG_DOC:
                                ProceedScanDoc(xScan, ref sc);
                                nRet = AppC.RC_OK;
                                break;
                            case PG_SCAN:
                                if (IsDoc4Check())
                                    break;
                                if (bDupScan)
                                {// ������������� �������� ������ ���������
                                    if (xCDoc.xDocP.TypOper == AppC.TYPOP_PRMK)
                                    {
                                        if ((bShowTTN) && (drDet != null))
                                        {
                                            SetOverOPR(true, drDet);
                                            xScan = null;
                                        }
                                        else
                                            Srv.ErrorMsg("���!");
                                        break;
                                    }
                                }

                                if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0) ||
                                    ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0)) 
                                {// ��������� ������
                                    //nRet = ProceedAdr(xScan, ref sc);
                                    nRet = ProceedAdrNew(xScan, ref sc);

                                    if (nRet != AppC.RC_CONTINUE)
                                        break;
                                    bRet = true;
                                }

                                if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0) ||
                                    ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_PRT) > 0))
                                {// ��������� SSCC

                                    //if ((xCDoc.xDocP.TypOper != AppC.TYPOP_DOCUM) || (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.AVAIL))
                                    //if ((xCDoc.xDocP.TypOper != AppC.TYPOP_KMPL) || (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.AVAIL))
                                    //{
                                        int nRetSSCC = ProceedSSCC(xScan, ref sc);
                                        if (nRetSSCC == AppC.RC_WARN)
                                        {
                                            bRet = true;
                                        }
                                        else
                                        {
                                            //ChkOPR(true);
                                            //xScan = null;
                                            break;
                                        }
                                    //}
                                    //else
                                      //  break;
                                }
                                else
                                {// ������ ���� ���������
                                    if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_OLD_ETIK) == ScanVarGP.BCTyp.SP_OLD_ETIK) ||
                                         (xScan.Id != BCId.Code128))
                                    {// ������ �������� ��� EAN13
                                        bRet = TranslSCode(ref sc, ref sErr);
                                    }
                                    else
                                    {// ����� ��������
                                        if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDV) == ScanVarGP.BCTyp.SP_MT_PRDV))
                                            bRet = TranslMT(ref sc);
                                        else if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDVN) == ScanVarGP.BCTyp.SP_MT_PRDVN))
                                            bRet = TranslMTNew(ref sc);
                                        else
                                        {
                                            // ������ �������� ����������� �������� �� AI
                                            bNewBarcode = true;
                                            t1 = Environment.TickCount;
                                            bRet = NewTranslSCode(ref sc, xScan);
                                            t2 = Environment.TickCount;
                                            sP = Srv.TimeDiff(t1, t2, 3);
                                        }
                                    }
                                }


                                if (!bRet)
                                {
                                    if (sErr.Length == 0)
                                        sErr = (!sc.bFindNSI) ? String.Format("��� �� ������!\nGTIN14={0}\nEAN={1}", sc.sGTIN, sc.sEAN) : "";
                                    throw new Exception(sErr);
                                }

                                bEasyEd = IsEasyEdit();
                                if (bEasyEd)
                                {// ��� ������ ����������� �����
                                    if ((bDupScan) && (bInEasyEditWait == true))
                                    {
                                        ZVKeyDown(AppC.F_ZVK2TTN, null, ref ehCurrFunc);
                                        break;
                                    }
                                    ZVKeyDown(AppC.F_OVERREG, null, ref ehCurrFunc);
                                }

                                bNewDetAdded = ProceedProd(ref sc, bDupScan, bEasyEd, bNewBarcode);
                                t1 = Environment.TickCount;
                                sP1 = Srv.TimeDiff(t2, t1, 3);

                                if ((sc.nRecSrc == (int)NSI.SRCDET.FROMADR)
                                    && (!bEasyEd))
                                    ShowOperState(xCDoc.xOper, sc.nMest);

                                ChkOPR(ref sc, bNewDetAdded);
                                break;
                            case PG_SSCC:
                                ProceedScanSSCC(xScan, ref sc);
                                nRet = AppC.RC_OK;
                                break;
                        }
                        xScanPrev = xScan;
                    } while (false);
                    #endregion
                }
                catch (Exception ex)
                {
                    string 
                        sPrt,
                        sE = String.Format("{0}({1}){2}", xScan.Id.ToString(), xScan.Dat.Length, xScan.Dat);
                    if (tcMain.SelectedIndex == PG_SCAN)
                        tNameSc.Text = sE;
                    sPrt = sE + "\n" + ex.Message;
                    WriteProt(DateTime.Now.ToString("dd.MM.yy HH:mm:ss - ") + sPrt + "\n");
                    Srv.ErrorMsg(sPrt, "������ ������������", true);
                }
            }
            // ��������� ������������ ��������
            bInScanProceed = false;
            ResetTimerReLogon(true);
        }

        private void WriteProt(string s)
        {
            if (swProt != null)
            {
                swProt.WriteLine(s);
            }
        }

        // �������� ����������� ������ ����� ����� ���������
        private int ChkOPR(ref PSC_Types.ScDat sc, bool bNewDetAdded)
        {
            int 
                nRet = AppC.RC_OK;
            AddrInfo 
                xA = null;

            if ((xCDoc.xDocP.TypOper != AppC.TYPOP_DOCUM)
                && (xCDoc.xDocP.TypOper != AppC.TYPOP_KMPL))
            {
                if (bShowTTN)
                {
                    if ((xSm.xAdrFix1 != null) && (!xCDoc.xOper.IsFillSrc()))
                    {// ������������ �����
                        xA = xSm.xAdrFix1;
                        xCDoc.xOper.SetOperSrc(xA, xCDoc.xDocP.nTypD, true);
                    }

                    if (xPars.OpChkAdr)
                    {
                        if (xCDoc.xOper.IsFillSrc() &&
                            (sc.nRecSrc != (int)NSI.SRCDET.FROMADR))
                        {// �������� ����������� ������
                            ServerExchange xSE = new ServerExchange(this);

                            xCUpLoad = new CurUpLoad(xPars);
                            xDP = xCUpLoad.xLP;
                            xCUpLoad.bOnlyCurRow = true;
                            if (bNewDetAdded)
                                xCUpLoad.drForUpl = drDet;
                            else
                                xCUpLoad.drForUpl = xNSI.AddDet(sc, xCDoc, null, false);

                            xCUpLoad.sCurUplCommand = AppC.COM_CKCELL;
                            string sL = UpLoadDoc(xSE, ref nRet);

                            //if ((xSE.ServerRet != AppC.EMPTY_INT) &&
                            //    (xSE.ServerRet != AppC.RC_OK))
                            //{// �������� �������� �� ������ �� ������� (�������������� ������)
                            //    Srv.ErrorMsg(sL, "������ ����������", true);
                            //    xA = xCDoc.xOper.xAdrSrc;
                            //    xCDoc.xOper = new CurOper(false);
                            //    if (bNewDetAdded)
                            //        drDet.Delete();
                            //}


                            if ( TestProdBySrv(xSE, nRet) != AppC.RC_OK )
                            {// �������� �������� �� ������ �� ������� (�������������� ������)
                                //Srv.ErrorMsg(sL, "������ ����������", true);
                                xA = xCDoc.xOper.xAdrSrc;
                                xCDoc.xOper = new CurOper(false);
                                if (bNewDetAdded)
                                    drDet.Delete();
                            }




                            //nRet = IsOperReady(drDet);
                            //??? nRet = IsOperReady();
                        }
                    }
                    if (xA != null)
                        xCDoc.xOper.SetOperSrc(xA, xCDoc.xDocP.nTypD, true);
                }
                else
                    Srv.ErrorMsg("���!", true);
            }
            return (nRet);
        }


        private int AddGroupDet(int nRLoad, int nRowSource, string sAddInf)
        {
            return(AddGroupDet(nRLoad, nRowSource, sAddInf, true));
        }


        private int AddGroupDet(int nRLoad, int nRowSource, string sAddInf, bool bIsNeedControl)
        {
            bool
                bRet;
            int 
                nRezCtrl,
                nRet = AppC.RC_OK,
                nRec;
            string
                sK = "",
                sE = "";
            DataRow
                d = null;
                        
            PSC_Types.ScDat 
                scMD;
            CurOper
                xTMPOper;
            List<int>
                lstNewR = new List<int>();


            if (nRLoad == AppC.RC_MANYEAN)
            {// ���������� ������ ����� (���������������� ������)
                if (xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    xTMPOper = xCDoc.xOper;
                    nRec = xCLoad.dtZ.Rows.Count;
                    try
                    {
                        for (int i = 0; i < nRec; i++)
                        {
                            xCDoc.xOper = xTMPOper;
                            sE = xCLoad.dtZ.Rows[i]["EAN13"].ToString();
                            scMD = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.EAN13, sE));
                            if (SetVirtScan(xCLoad.dtZ.Rows[i], ref scMD, false, false))
                            {
                                scMD.nRecSrc = nRowSource;
                                scCur = scMD;
                                if (nRowSource == (int)NSI.SRCDET.SSCCT)
                                {
                                    scCur.sSSCC = sAddInf;
                                }
                                //TryEvalNewZVKTTN(ref scMD, false);
                                //AddDet1(ref scMD, out d);
                                if (bIsNeedControl)
                                    TryEvalNewZVKTTN(ref scCur, false);
                                AddDet1(ref scCur, out d);
                                lstNewR.Add((int)d["ID"]);
                            }
                            else
                            {
                                nRet = AppC.RC_NOEAN;
                                sK = xCLoad.dtZ.Rows[i]["KMC"].ToString();
                                break;
                            }
                        }
                        // ���� ����� ��������� � �������
                        if (nRowSource == (int)NSI.SRCDET.FROMADR_BUTTON)
                        {
                            int nSQU = 0;
                            foreach (DataRow drsc in xCLoad.dsZ.Tables[NSI.BD_SSCC].Rows)
                            {
                                try
                                {
                                    nSQU = (int)drsc["MONO"];
                                }
                                catch
                                {
                                    nSQU = 1;
                                }
                                AddSSCC2SSCCTable((string)drsc["SSCC"], 0, xCDoc, nSQU, 1, 1);
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                    xInf = new List<string>();
                    nRezCtrl = AppC.RC_OK;
                    if (bZVKPresent && bIsNeedControl)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        try
                        {
                            nRezCtrl = ControlDocZVK(null, xInf, sAddInf);
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                    }

                    if (nRezCtrl != AppC.RC_OK)
                    {
                        //xHelpS.ShowInfo(lstCtrl, ref ehCurrFunc);
                        DialogResult dr = MessageBox.Show("�������� ���������� (Enter)?\n(ESC) - �������� �����������\n(SHIFT-F1 - ��������)",
                            "�������������� ������!",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        //if (dr == DialogResult.OK)
                        //    bGoodData = false;

                        if (dr == DialogResult.OK)
                        {
                            DataRow[] drMDetZ = xCDoc.drCurRow.GetChildRows(xNSI.dsM.Relations[NSI.REL2TTN]);
                            foreach (DataRow drDel in drMDetZ)
                            {
                                if (lstNewR.Contains((int)drDel["ID"]))
                                    xNSI.dsM.Tables[NSI.BD_DOUTD].Rows.Remove(drDel);
                            }
                            ClearZVKState("", null);
                        }
                    }

                    xCDoc.xOper = new CurOper(true);
                    //xCDoc.xOper.SetOperObj(null, xCDoc.xDocP.nTypD, true);    

                    if (nRet != AppC.RC_OK)
                    {
                        Srv.ErrorMsg(String.Format("�� ������ KMC={0}\nEAN={1}", sK, sE), sAddInf, true);
                    }
                }
                //Back2Main();
            }
            else
            {// ���������� ��������� ������������ ����������
                nRet = AppC.RC_OK;
            }
            return (nRet);
        }

        private int SSCC4OTG(ServerExchange xSE, ScanVarGP xSc, ref PSC_Types.ScDat scD, ScanVarGP.BCTyp xT)
        {
            int ret = AppC.RC_OK;
            DataRow dr;

            ret = FindSSCCInZVK(xSc, ref scD);
            if (ret == AppC.RC_OK)
            {// ������������� ������������� � ������
                dr = AddDetSSCC(xSc, xCDoc.nId, xT, "");
                if (dr != null)
                    drDet = dr;
            }
            else
            {// � ������ ����, ���� ����� �� ������
                ret = ConvertSSCC2Lst(xSE, xSc.Dat, ref scD, false);
                if (ret == AppC.RC_OK)
                {// ��� ���� ���, ������ ������� ��������
                    ret = AppC.RC_WARN;
                }
                else
                {
                    AddGroupDet(ret, (int)NSI.SRCDET.SSCCT, xSc.Dat);
                    // � ����� ������ ��������� ����� �����������
                    ret = AppC.RC_OK;
                }
            }
            return ret;
        }

        private int nSpecAdrWait = 0;











        private int EvalGroupDetStat(DataTable dtZ, out int nTotMest, out FRACT fTotEd)
        {
            int
                nRet = AppC.RC_OK;

            nTotMest = 0;
            fTotEd = 0;
            for (int i = 0; i < xCLoad.dtZ.Rows.Count; i++)
            {
                try
                {
                    nTotMest += (int)dtZ.Rows[i]["KOLM"];
                }
                catch
                {
                }

                try
                {
                    fTotEd += (FRACT)dtZ.Rows[i]["KOLE"];
                }
                catch
                {
                }
            }

            return (nRet);
        }

        // ���������� ��������� ScDat �� ������ ������������ �����-����
        // (��������� ��� ��)
        public bool TranslSCode(ref PSC_Types.ScDat s, ref string sErr)
        {
            bool
                bFind = false,     // ����� �� ������������ MC �� ����������� (����)
                ret = true;
            int
                n;
            string 
                sIdPrim,
                sP,
                sVsego = "",
                sS = s.s;

                try
                {
                    //if (s.ci != ScannerAll.BCId.EAN13)
                    if (sS.Length > 14)
                    {
                        while (sS.Length > 0)
                        {
                            sIdPrim = sS.Substring(0, 2);
                            sS = sS.Substring(2);
                            switch (sIdPrim)
                            {
                                case "01":                          // ���������� ����� ������
                                case "02":
                                    s.sEAN = Srv.CheckSumModul10(sS.Substring(1, 12));
                                    s.sGTIN = sS.Substring(0, 14);
                                    sS = sS.Substring(14);
                                    break;
                                case "10":                          // ����� ������
                                    s.nParty = int.Parse(sS.Substring(0, 4)).ToString();
                                    sS = sS.Substring(4);
                                    break;
                                case "11":                          // ���� ������������ (������)
                                    sP = sS.Substring(0, 6);
                                    s.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);
                                    s.sDataIzg = s.dDataIzg.ToString("dd.MM.yy");
                                    sS = sS.Substring(6);
                                    break;
                                case "30":                          // ���������� ���� �� �������
                                    s.nMestPal = int.Parse(sS.Substring(0, 4));
                                    s.tTyp = AppC.TYP_TARA.TARA_PODDON;
                                    sS = sS.Substring(4);
                                    break;
                                case "37":                          // ���������� �������
                                    sVsego = sS.Substring(0,6);
                                    sS = sS.Substring(6);
                                    break;
                                default:
                                    sS = "";
                                    ret = false;
                                    break;
                            }
                        }
                        if (ret)
                        {
                            //if (!s.sGTIN.StartsWith("0"))
                            //{// GTIN ����� �����
                            //    bFind = SetKMCOnGTIN_N(ref s, s.sGTIN);
                            //}
                            ret = SetKMCOnGTIN_N(ref s, s.sGTIN);
                            if (!ret)
                                ret = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
                            if (ret)
                            {
                                if (s.bVes == true)
                                    s.fVes = FRACT.Parse(sVsego) / 1000;
                                else
                                {// ������� �����, - ������� �� ���������
                                    n = int.Parse(sVsego.Substring(2, 4));
                                    if ((int)((StrAndInt)s.xEmks.Current).DecDat != n)
                                    {
                                        bFind = false;
                                        for (int j = 0; j < s.xEmks.Count; j++)
                                        {
                                            s.xEmks.CurrIndex = j;
                                            if (((StrAndInt)s.xEmks.Current).DecDat == n)
                                            {
                                                bFind = true;
                                                break;
                                            }
                                        }
                                        if (!bFind)
                                        {
                                            s.tTyp = AppC.TYP_TARA.TARA_TRANSP;
                                            CompareEmk(ref s, n);
                                        }
                                        else
                                        {
                                            s.fEmk_s = s.fEmk = n;
                                            s.nKolSht = ((StrAndInt)s.xEmks.Current).IntCodeAdd1;
                                            s.nMestPal = ((StrAndInt)s.xEmks.Current).IntCode;
                                        }
                                    }
                                    else
                                    {
                                        s.fEmk_s = s.fEmk = n;
                                        s.nKolSht = ((StrAndInt)s.xEmks.Current).IntCodeAdd1;
                                        s.nMestPal = ((StrAndInt)s.xEmks.Current).IntCode;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {// tckb <= 14
                        if (sS.Length == 14)
                        {// ��� ITF
                            s.sGTIN = sS;
                            s.sEAN = Srv.CheckSumModul10(sS.Substring(1, 12));
                            ret = SetKMCOnGTIN_N(ref s, s.sGTIN);
                            if (!ret)
                                sS = s.sEAN;
                            else
                                sS = "";
                        }
                        else
                        {// ��� EAN13 ��� ....
                            sIdPrim = sS.Substring(0, 1);
                            if (sIdPrim == "2")     // ������� ��������� ��� ���������� ���
                            {
                                if (!xNSI.IsAlien(sS, ref s))
                                {
                                    sS = sS.Substring(1);
                                    s.fVes = FRACT.Parse(sS.Substring(5, 6)) / 1000;
                                    if (sS.Substring(0, 1) != "9")
                                    {// �� ������������ ������� (���� ��� ������)
                                        s.nParty = int.Parse(sS.Substring(0, 3)).ToString();
                                        s.nKrKMC = int.Parse(sS.Substring(3, 2));
                                    }
                                    else
                                    {// �� ��������� ������� ������� ���������
                                        if (sS.Substring(4, 1) == "6")
                                            s.nKrKMC = 52;
                                        else
                                            s.nKrKMC = 23;
                                        s.tTyp = AppC.TYP_TARA.TARA_POTREB;
                                    }
                                    ret = xNSI.GetMCData("", ref s, s.nKrKMC, false);
                                    s.bVes = true;
                                }
                                else
                                {
                                    ret = true;
                                    s.bAlienMC = true;
                                    s.fVes = FRACT.Parse(sS.Substring(7, 5)) / 1000;
                                }
                                sS = "";
                            }
                            else
                            {
                                s.sEAN = sS;
                                s.sGTIN = "0" + sS;
                            }
                        }
                        if (sS.Length > 0)     // 
                            ret = xNSI.GetMCDataOnEAN(sS, ref s, true);
                    }
                }
                catch
                {
                    ret = false;
                    sS = "";
                }
            return (ret);
        }

        // ���������� ��������� ScDat �� ������ ������������ �����-����
        // (��������� ��� ��)
        public bool NewTranslSCode(ref PSC_Types.ScDat s, ScanVarGP xScanGP)
        {
            string 
                sP;
            int 
                t1,t2,
                n = 0;
            bool 
                bR,
                ret = true;

                try
                {
                    if (xScanGP.Id == ScannerAll.BCId.Code128)
                    {
                        if (xScanGP.dicSc.ContainsKey("01"))
                        {
                            s.sEAN = Srv.CheckSumModul10( xScanGP.dicSc["01"].Dat.Substring(1,12) );
                            s.sGTIN = xScanGP.dicSc["01"].Dat;
                            s.tTyp = AppC.TYP_TARA.TARA_TRANSP;
                        }
                        if (xScanGP.dicSc.ContainsKey("02"))
                        {
                            s.sEAN = Srv.CheckSumModul10( xScanGP.dicSc["02"].Dat.Substring(1,12) );
                            s.sGTIN = xScanGP.dicSc["02"].Dat;
                            s.tTyp = AppC.TYP_TARA.TARA_PODDON;
                        }

                        if (xScanGP.dicSc.ContainsKey("23"))
                        {
                            if (s.tTyp == AppC.TYP_TARA.TARA_PODDON)
                                s.nNomPodd = (int)(long)(xScanGP.dicSc["23"].xV);
                            else
                                s.nNomMesta = (int)(long)(xScanGP.dicSc["23"].xV);
                        }

                        // �������� �����
                        if (xScanGP.dicSc.ContainsKey("21"))
                        {
                            if (s.tTyp == AppC.TYP_TARA.TARA_PODDON)
                            {
                                try
                                {
                                    s.nNomPodd = int.Parse((string)(xScanGP.dicSc["21"].xV));
                                }
                                catch { }
                            }
                            else
                            {
                                try
                                {
                                    s.nNomMesta = int.Parse((string)(xScanGP.dicSc["21"].xV));
                                }
                                catch { }
                            }
                        }


                        if (xScanGP.dicSc.ContainsKey("10"))
                        {
                            s.nParty = xScanGP.dicSc["10"].Dat;
                            while (s.nParty.Length > 0)
                            {
                                if (s.nParty.StartsWith("0"))
                                    s.nParty = s.nParty.Substring(1);
                                else
                                    break;
                            }
                            //s.nParty = int.Parse(xScanGP.dicSc["10"].Dat).ToString();
                        }

                        if (xScanGP.dicSc.ContainsKey("11"))
                        {
                            //sP = xScanGP.dicSc["11"].Dat;
                            //sTypDoc.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);

                            s.dDataIzg = (DateTime)(xScanGP.dicSc["11"].xV);
                            s.sDataIzg = s.dDataIzg.ToString("dd.MM.yy");
                        }
                        //bL2Nsi = xNSI.GetMCData(s.sKMCFull, ref s, 0);

                        // ���������� � ����� ��� ��������
                        if (xScanGP.dicSc.ContainsKey("37"))
                        {
                            n = (int)(long)(xScanGP.dicSc["37"].xV);
                            if (s.tTyp == AppC.TYP_TARA.TARA_PODDON)
                            {
                                s.nMestPal = n;
                            }
                            else
                            {
                                if (s.bVes)
                                    s.nKolSht = n;
                                else
                                    s.fEmk = n;
                            }
                        }
                        //sP = (s.sEAN.Length >= 12) ? s.sEAN.Substring(0, 12) : "";

                        //if (!SetKMCOnGTIN(ref s, s.sGTIN, n, xScanGP))
                        //    xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);

                        t1 = Environment.TickCount;
                        //bR = SetKMCOnGTIN(ref s, s.sGTIN, n, xScanGP);
                        bR = SetKMCOnGTIN_N(ref s, s.sGTIN);
                        t2 = Environment.TickCount;
                        if (!bR)
                            xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
                        sP = Srv.TimeDiff(t1, t2, 3);

                        ret = CompareEmk(ref s, n);

                        if (xScanGP.dicSc.ContainsKey("310"))
                        {// ������� �����
                            //s.nTara = 0;
                            s.fVes = (FRACT)(xScanGP.dicSc["310"].xV);
                        }

                    }
                }
                catch
                {
                    ret = false;
                    //sTypDoc.sN = sS + "-???";
                }
            return (ret);
        }

        private bool CompareEmk(ref PSC_Types.ScDat s, int n)
        {
            bool
                ret = true;
            int
                nNSI = 0;
            if (s.tTyp == AppC.TYP_TARA.TARA_TRANSP)
            {// �� ������ ������ �������� ����� ����������� ������ �������
                //if ((s.drSEMK != null) && (n > 0))
                if ((s.xEmks.Count > 0) && (n > 0))
                {// ������� ���������� ������� �� �����������
                    if (s.bVes)
                    {
                        nNSI = (int)((StrAndInt)s.xEmks.Current).IntCodeAdd1;
                        if (s.nKolSht != n)
                        {
                            string
                                sP = String.Format("������������ ��������!\n� ��������� - {0}\n� ����������� - {1}\n����������� {0}(Enter)?\n(ESC)-������� {1}", n, nNSI);
                            DialogResult dr = MessageBox.Show(sP, String.Format("������������:{0} <> {1}", n, s.nKolSht),
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.OK)
                                s.nKolSht = n;
                            else
                                s.nKolSht = nNSI;
                        }
                    }
                    else
                    {
                        nNSI = (int)((StrAndInt)s.xEmks.Current).DecDat;
                        if (nNSI != n)
                        {
                            string
                                sP = String.Format("������������ ��������!\n� ��������� - {0}\n� ����������� - {1}\n����������� {0}(Enter)?\n(ESC)-������� {1}", n, nNSI);
                            DialogResult dr = MessageBox.Show(sP, String.Format("������������:{0} <> {1}", n, nNSI),
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.OK)
                                s.fEmk = s.fEmk_s = n;
                            else
                                s.fEmk = s.fEmk_s = nNSI;
                        }
                    }
                }
            }
            return (ret);
        }













        public bool TranslMT(ref PSC_Types.ScDat s)
        {
            bool 
                ret = false;

            try
            {
                if (xScan.Id == ScannerAll.BCId.Code128)
                {
                    s.tTyp = AppC.TYP_TARA.TARA_PODDON;

                    s.nNPredMT = int.Parse(xScan.Dat.Substring(2, 9));

                    s.sEAN = Srv.CheckSumModul10("20" + xScan.Dat.Substring(11, 10));

                    s.nParty = int.Parse(xScan.Dat.Substring(21, 4)).ToString();

                    s.dDataIzg = DateTime.ParseExact( xScan.Dat.Substring(25, 6), "yyMMdd", null);
                        
                    s.sDataIzg = s.dDataIzg.ToString("dd.MM.yy");
                    
                    s.fEmk = (FRACT)(int.Parse(xScan.Dat.Substring(31, 7))) / 1000;

                    ret = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
                    if (s.bVes)
                        s.fVes = s.fEmk;
                }
            }
            catch
            {
                ret = false;
                //sTypDoc.sN = sS + "-???";
            }
            return (ret);
        }


        //private bool TranslMTNew(ref PSC_Types.ScDat s, ScanVarGP xSc)
        // ���������� ��������� ScDat �� ������ ������������ �����-����
        // ����� ������
        public bool TranslMTNew(ref PSC_Types.ScDat s)
        {
            bool
                bPoddon = false,
                bFind = false,          // ����� �� ������������ MC �� ����������� (����)
                ret = false;
            string
                sTaraType,
                sP,
                sS = s.s;

            sTaraType = sS.Substring(0, 2);
            if (sTaraType == "52")
            {// ��� ��������
                bPoddon = true;
                s.tTyp = AppC.TYP_TARA.TARA_PODDON;
            }
            else if (sTaraType == "53")
            {// ��� ������ ����
                bPoddon = false;
                s.tTyp = AppC.TYP_TARA.TARA_TRANSP;
            }
            sS = sS.Substring(2);

            // ��� ���������
            s.sKMC = sS.Substring(0, 10);
            s.sEAN = Srv.CheckSumModul10("20" + sS.Substring(0, 10));
            bFind = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
            sS = sS.Substring(10);

            // SysN ��������� (����������) - ��� �� � ����� ������
            s.nParty = sS.Substring(0, 9);
            // SysN ��������� (����������)
            s.nNPredMT = int.Parse(s.nParty) * (-1);
            s.nParty = s.nParty.Substring(4, 5);

            sS = sS.Substring(9);

            // ���� ��������(������������) (������)
            sP = sS.Substring(0, 6);
            s.dDataGodn =
            s.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);
            s.sDataIzg = sP.Substring(4, 2) + "." + sP.Substring(2, 2) + "." +
                sP.Substring(0, 2);

            sS = sS.Substring(6);

            // �������/���������� ������
            s.fVsego = Srv.Str2VarDec(sS.Substring(0, 7));
            s.fEmk = s.fEmk_s = s.fVsego;

            if (s.bVes)
                s.fVes = s.fVsego;

            sS = sS.Substring(7);

            if (bPoddon)
            {
                // � �������
                s.nNomPodd = int.Parse(sS.Substring(0, 3));
                s.nMestPal = int.Parse(sS.Substring(3, 3));
                s.nMest = s.nMestPal;
                if (!s.bVes)
                {
                    s.fVsego = s.fEmk * s.nMestPal;
                }
            }
            else
            {
            }

            sS = "";
            ret = true;

            return (ret);
        }










        // ����������� ������� � ������ ��� �������� ������ �� ����������� ��������
        // ��������� �������� - 1 ������������ �������� (���� �����)
        //                      2 ��������������� ���� (���� �����)
        //                      3 ������ (��������� ����)
        // ����� ���� �����������:
        // - �������
        // - ����
        // - ���������� ����
        // - ��� ��������
        public bool TrySetEmk(DataTable dtM, DataTable dtD, ref PSC_Types.ScDat sc, FRACT fVesU)
        {
            const int MAXDIFF = 1000000;
            bool ret = false;

            bool bTryComp = false,       // ���� ���-�� ��������� ��� ����������� ���� ��������?
                bNot1Sht = true,         // ��� �� ��������� �������
                bNot1Pal = true;         // ��� �� �������

            int 
                //nPrPl = 0,               // � ������. ����.
                nVesVar = xPars.aParsTypes[AppC.PRODTYPE_VES].nDefEmkVar,
                nEP = 0,
                nSht = 0;
            string
                sTara = "";

            FRACT fEmk = 0;
            FRACT 
                fCE,
                fSignDiff,
                fDiff,
                fDiffPercent = 0,
                fDiff1Ed = 1000,
                fDiff_Start = MAXDIFF;

            if ((sc.bFindNSI == true) && (sc.drMC != null))
            {// ����� ��� ������ ������� �� ���� ��������� � �������� ���������� ����

                DataRelation myRelation = dtM.ChildRelations[NSI.REL2EMK];
                DataRow[] childRows = sc.drMC.GetChildRows(myRelation);

                //if (sc.nParty.Length == 4)
                //    nPrPl = sc.nParty.Substring(0, 1);

                foreach (DataRow chRow in childRows)
                {
                    fCE = (FRACT)chRow["EMK"];
                    if ( fCE != 0)
                    {// ������� �������
                        if (fVesU > 0)
                        {// � ��� �������
                            bTryComp = true;
                            fSignDiff = fVesU - fCE;
                            fDiff = Math.Abs(fSignDiff);
                            fDiffPercent = fDiff * 100 / fCE;

                            //bNot1Sht |= (fDiffPercent < 40) || (fDiffPercent > 100);

                            //bNot1Pal |= (fDiffPercent < 200);

                            if (fDiffPercent <= nVesVar)
                            {// ������ �� 1 ������ �����, ������������ ������ ���������� � �������� 40%
                                if (fDiff < fDiff_Start)
                                {
                                    bNot1Sht = true;
                                    bNot1Pal = true;
                                    fDiff_Start = fDiff;
                                    fEmk = fCE;
                                    //sTara = (string)chRow["KT"];
                                    sTara = (string)chRow["KTARA"];
                                    nSht = (int)chRow["KRK"];
                                    nEP = (int)chRow["EMKPOD"];
                                }
                            }
                            else
                            {// ��� ����� ��� ������
                                if (fVesU < fCE)
                                {// ���� �� ������
                                    bNot1Pal = true;
                                    nSht = (int)chRow["KRK"];
                                    nEP = (int)chRow["EMKPOD"];
                                    if (nSht > 0)
                                    {
                                        fDiff1Ed = Math.Abs(fVesU - (fCE / nSht));
                                        fDiff1Ed = fDiff1Ed * 100 / fVesU;
                                        if (fDiff1Ed <= 20)
                                        {
                                            bNot1Sht = false;
                                            fEmk = 0;
                                            break;
                                        }
                                    }

                                }
                                else
                                {// ��� �������� ������� ������������ �� ����� ����
                                    // ��� ������� ������� (���������� ��)
                                    bNot1Sht = true;
                                    bNot1Pal = false;
                                }
                            }


                        }
                        else
                        {// ��� �� ������, ����������� ���� � ����/����
                            if (sc.fEmk > 0)
                            {// ��� �������
                                if ((sc.fEmk == (FRACT)chRow["EMK"]) || true)
                                {// ��������� �������
                                    if (((int)chRow["PR"] > 0) || (fEmk == 0))
                                    {// ���� �� ������������ ��� ����� �� ��������
                                        fEmk = (FRACT)chRow["EMK"];
                                        //sTara = (string)chRow["KT"];
                                        sTara = (string)chRow["KTARA"];
                                        nSht = (int)chRow["KRK"];
                                        nEP = (int)chRow["EMKPOD"];
                                        //if ((int)chRow["PR"] == nPrPl)
                                        //    break;
                                        if ((int)chRow["PR"] > 0)
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    if (childRows.Length == 1)
                    {// ��������� ������, ������ ���� �������
                        fEmk = (FRACT)chRow["EMK"];
                        //sTara = (string)chRow["KT"];
                        sTara = (string)chRow["KTARA"];
                        nSht = (int)chRow["KRK"];
                        nEP = (int)chRow["EMKPOD"];
                    }
                }

                if ((sc.xEmks.Count <= 0) || bEditMode)
                {// �� ITF/����������� �������� �� ����������
                    sc.fEmk_s = sc.fEmk = fEmk;
                    sc.nTara = sTara;
                    sc.nKolSht = nSht;
                    sc.nMestPal = nEP;
                }

                if (bTryComp == true)
                {// �������� ���-�� ���������
                    if (bNot1Sht == false)
                    {// 1 �����
                        if (sc.tTyp != AppC.TYP_TARA.TARA_PODDON)
                        {
                            sc.fEmk = TrySetEmkByZVK(AppC.TYP_TARA.TARA_POTREB, ref sc, 0);
                            if (sc.fEmk != 0)
                            {
                                sc.tTyp = AppC.TYP_TARA.TARA_TRANSP;
                                sc.nMest = 1;
                                fEmk = sc.fEmk;
                            }
                            else
                            {
                                sc.tTyp = AppC.TYP_TARA.TARA_POTREB;
                                sc.nMest = 0;
                            }
                        }
                    }
                    else if (
                        ( (fEmk != 0) && (fDiff_Start < 40) ) ||
                        ( (fEmk == 0) && (fDiff_Start == MAXDIFF) && bNot1Pal ))
                    {// ���� ��� ������
                        if (sc.tTyp != AppC.TYP_TARA.TARA_PODDON)
                        {// ��� �����
                            //sc.nTypVes = AppC.TYP_VES_TUP;
                            sc.tTyp = AppC.TYP_TARA.TARA_TRANSP;
                            sc.nMest = 1;
                            sc.fEmk = TrySetEmkByZVK(AppC.TYP_TARA.TARA_TRANSP, ref sc, fEmk);
                            if (sc.fEmk != fEmk)
                            {// ���-�� ����������
                                if ((fVesU / sc.fEmk) > 1.3M)
                                {
                                    //sc.nTypVes = AppC.TYP_VES_PAL;
                                    sc.tTyp = AppC.TYP_TARA.TARA_PODDON;
                                    if (sc.nMestPal == 0)
                                        sc.nMest = -1;
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                        {// ��� �������
                            sc.fEmk = sc.fEmk_s = fEmk;
                            if (sc.nMestPal == 0)
                                sc.nMest = -1;
                        }
                    }
                    else if (bNot1Pal == false)
                    {// ��� �������
                        sc.tTyp = AppC.TYP_TARA.TARA_PODDON;
                        if (sc.nMestPal == 0)
                            sc.nMest = -1;
                    }

                }
                ret = (fEmk > 0) ? true : false;
            }
            else
            {// ���������� �����������, �� ��������� - ???
            }

            return (ret);
        }

        private FRACT TrySetEmkByZVK(AppC.TYP_TARA tTara, ref PSC_Types.ScDat sc, FRACT fCurE)
        {
            FRACT 
                fEZ,
                fE = fCurE;
            if (bZVKPresent)
            {
                string sF = FilterKompl(xCDoc.nId, sc.sKMC, (xCDoc.xNPs.Current > 0)?true:false);
                sF += " AND(EMK>0)";
                DataView dv = new DataView(xNSI.DT[NSI.BD_DIND].dt,
                        sF, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);
                if (dv.Count == 1)
                {
                    string sMsg = "";
                    fEZ = (FRACT)dv[0].Row["EMK"];
                    if (tTara == AppC.TYP_TARA.TARA_POTREB)
                        // ��������������� ��������
                        sMsg = String.Format("(ESC) - ��������������� ����\n(Enter) - ��� ���� \n �������� {0:N1}", fEZ);
                    else if (fEZ != fCurE)
                        sMsg = String.Format("(ENT) - ����� {0:N1} �� ������ ?\n(ESC) - ������������ {1:N1}", fEZ, fCurE);
                    if (sMsg.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show(sMsg, "������� ����������!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                        if (dr == DialogResult.OK)
                            fE = fEZ;
                    }
                }
            }
            return (fE);
        }



        private int EvalEnteredVals(ref PSC_Types.ScDat sc, string sKMCCode, FRACT fE, string nP,
            DataView dvEn, int i, int nMaxR)
        {
            int 
                ret = 0,
                nM = 0,
                nMest = 0;
            string
                nParty = "",
                sDVyr;

            FRACT fEm = 0,
                fVsego = 0,
                fV = 0;

            NSI.DESTINPROD desProd;
            DataRow dr;

            sc.fKolE_alr = 0;
            sc.nKolM_alr = 0;
            sc.fMKol_alr = 0;

            sc.fKolE_alrT = 0;          // ��� ������� ������ ������� ���� (���� = 0)
            sc.nKolM_alrT = 0;          // ��� ������� ���� ������� ����
            sc.fMKol_alrT = 0;

            sc.drEd = null;
            sc.drMest = null;


            // ������ - SYSN + EAN13
            if (dvEn == null)
            {
                if (sc.sFilt4View.Length == 0)
                    sc.sFilt4View = FilterKompl(xCDoc.nId, sKMCCode, false);

                dvEn = new DataView(xNSI.DT[NSI.BD_DOUTD].dt,
                    sc.sFilt4View, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);
                nMaxR = dvEn.Count;
                i = 0;
            }

            while ((i < nMaxR) && ((string)dvEn[i].Row["KMC"] == sKMCCode))
            {
                dr = dvEn[i].Row;
                nM = (int)dr["KOLM"];
                fV = ((int)dr["SRP"] > 0) ? 1 : (FRACT)dr["KOLE"];
                fEm = (FRACT)dr["EMK"];
                nParty = (string)dr["NP"];
                sDVyr = (string)dr["DVR"];

                desProd = (NSI.DESTINPROD)(dr["DEST"]);

                if (fEm == 0)
                {// ��� �������
                    if (desProd == NSI.DESTINPROD.TOTALZ)
                    {// ������� ��� ���������� ������ �� ������
                        sc.fKolE_alrT += fV;
                    }
                    else
                    {// ������� �� ����� ������
                        sc.fKolE_alr += fV;
                    }
                    if (nParty == nP)
                    {// ���� ������� ������ � ���� ���������
                        if (sDVyr == sc.dDataIzg.ToString("yyyyMMdd"))
                        {// ���� ����������� - ������ ����
                            sc.drEd = dr;
                        }
                    }
                }
                else
                {// �����
                    nMest += nM;
                    fVsego += fV;
                    if (fEm == fE)
                    {
                        if (desProd == NSI.DESTINPROD.TOTALZ)
                        {
                            sc.nKolM_alrT += nM;
                            sc.fMKol_alrT += fV;
                        }
                        else
                        {// ��������� ����� �����
                            sc.nKolM_alr += nM;
                            sc.fMKol_alr += fV;
                        }
                        if (nParty == nP)
                        {
                            if (sDVyr == sc.dDataIzg.ToString("yyyyMMdd"))
                            {// ���� ����������� - ������ ����
                                sc.drMest = dr;
                            }
                        }
                    }
                }

                i++;

                // ����� � ������ �����
                ret++;
            }

            if (fE == 0)
            {// ������� ����������
                sc.nKolM_alr = nMest;
                sc.fMKol_alr = fVsego;
            }
            return (ret);
        }






        // �������� (���������) KMC �� GTIN
        //private bool SetKMCOnGTIN_N(ref PSC_Types.ScDat sc, string sGTIN, int nEFromBC, ScanVarGP xSc)
        private bool SetKMCOnGTIN_N(ref PSC_Types.ScDat sc, string sGTIN)
        {
            bool
                ret = AppC.RC_CANCELB;
            DataRow
                drMC,
                drGTIN = null;
            DataRow[]
                draGTIN = null;

            if (sc.sGTIN.Length > 0)
            {
                //drGTIN = xNSI.DT[NSI.NS_SEMK].dt.Rows.Find(new object[] { sGTIN });

                //xNSI.DT[NSI.NS_SEMK].SetAddSort("GTIN");
                draGTIN = xNSI.DT[NSI.NS_SEMK].dt.Select(String.Format("GTIN='{0}'", sGTIN));
                if (draGTIN.Length > 0)
                {
                    drGTIN = draGTIN[0];
                }
                if (drGTIN != null)
                {// ��� ���������� drSEMK
                    sc.bEmkByITF = true;
                    sc.fEmk = (FRACT)drGTIN["EMK"];
                    if ((sc.tTyp == AppC.TYP_TARA.TARA_PODDON) && (sc.nMestPal > 0))
                    { }
                    else
                        sc.nMestPal = (int)drGTIN["EMKPOD"];
                    //sc.nTara = (sc.drSEMK["KT"] is string) ? (string)sc.drSEMK["KT"] : "";
                    sc.nTara = (drGTIN["KTARA"] is string) ? (string)drGTIN["KTARA"] : "";
                    sc.nKolSht = (drGTIN["KRK"] is int) ? (int)drGTIN["KRK"] : 0;
                    sc.sKMC = (drGTIN["KMC"] is string) ? (string)drGTIN["KMC"] : "";

                    drMC = xNSI.DT[NSI.NS_MC].dt.Rows.Find(new object[] { sc.sKMC });
                    ret = sc.GetFromNSI(sc.s, drMC, xNSI.DT[NSI.NS_MC].dt);
                }




            }
            return (ret);
        }







        /// �������� ����� ��������� �� ������������ ������
        //private void ControlDocZVK_Old(DataRow drD, List<string> lstProt)
        //{
        //    int 
        //        i = 0,
        //        nM,
        //        iStart,
        //        iCur,
        //        iTMax, iZMax,
        //        nDokState = AppC.RC_OK,
        //        nRet;
        //    string 
        //        s1,
        //        s2,
        //        sFlt;
        //    FRACT
        //        fE,
        //        fV = 0;
        //    object
        //        xProt;

        //    DataRow 
        //        drC;
        //    RowObj 
        //        xR;

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
        //    DataView 
        //        //dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "KMC", DataViewRowState.CurrentRows);
        //        dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "KRKMC", DataViewRowState.CurrentRows);

        //    iZMax = dvZ.Count;
        //    if (iZMax <= 0)
        //    {
        //        nDokState = AppC.RC_CANCEL;
        //        lstProt.Add("*** ������ �����������! ***");
        //    }

        //    // ��� ��������� �� ��� �� ���������
        //    DataView 
        //        //dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KMC,EMK DESC", DataViewRowState.CurrentRows);
        //    dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KRKMC,EMK DESC", DataViewRowState.CurrentRows);
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
        //        foreach (DataRowView dr in dvT)
        //        {// ����� ���� ���������� �����
        //            dr["DEST"] = NSI.DESTINPROD.UNKNOWN;
        //        }


        //        lstProt.Add("<->----- ��� ------<->");

        //        while (i < iTMax)
        //        {

        //            if ((int)dvT[i]["DEST"] != (int)NSI.DESTINPROD.UNKNOWN)
        //            {// ������ ��������� ��� ����������
        //                i++;
        //                continue;
        //            }

        //            drC = dvT[i].Row;
        //            // ��� �� ������ � ������?
        //            xR = new RowObj(drC);

        //            if (xR.AllFlags == (int)AppC.OBJ_IN_DROW.OBJ_NONE)
        //            {
        //                lstProt.Add("��� ���������/SSCC");
        //                i++;
        //                continue;
        //            }
        //            if (!xR.IsEAN)
        //            {// ���� �� SSCC
        //                sFlt = "";
        //                if (xR.IsSSCCINT)
        //                    sFlt += String.Format("AND(SSCCINT='{0}')", xR.sSSCCINT);
        //                if (xR.IsSSCC)
        //                    sFlt += String.Format("AND(SSCC='{0}')", xR.sSSCC);

        //                DataView dvZSC = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf + sFlt, "SSCC,SSCCINT",
        //                    DataViewRowState.CurrentRows);
        //                if (dvZSC.Count > 0)
        //                    dvZSC[0].Row["READYZ"] = NSI.READINESS.FULL_READY;
        //                else
        //                {// SSCC �� ������
        //                    lstProt.Add(String.Format("����.{0} {1}:��� ������", xR.sSSCCINT, xR.sSSCC));
        //                }
        //                i++;
        //                continue;
        //            }

        //            sc.sEAN = (string)drC["EAN13"];
        //            sc.sKMC = (string)drC["KMC"];
        //            sc.nKrKMC = (int)drC["KRKMC"];

        //            sc.bVes = ((int)(drC["SRP"]) > 0) ? true : false;

        //            sc.fEmk = (FRACT)drC["EMK"];
        //            sc.nParty = (string)drC["NP"];
        //            sc.dDataIzg = DateTime.ParseExact((string)drC["DVR"], "yyyyMMdd", null);
        //            //sc.nTara = (string)drC["KRKT"];
        //            sc.nTara = (string)drC["KTARA"];

        //            sc.nRecSrc = (int)NSI.SRCDET.CR4CTRL;

        //            //td1 = Environment.TickCount;

        //            iStart = dvZ.Find(sc.nKrKMC);
        //            //iStart = dvZ.Find(sc.sKMC);
        //            if (iStart != -1)
        //                nRet = EvalZVKMest(ref sc, dvZ, iStart, iZMax);
        //                //nRet = LookAtZVK(ref sc, dvZ, iStart, iZMax);
        //            else
        //                nRet = AppC.RC_NOEAN;

        //            //tc += (Environment.TickCount - td1);

        //            iCur = -1;
        //            if (nRet == AppC.RC_OK)
        //            {// ���� ��� �������� ��� ����� �������
        //                //td1 = Environment.TickCount;

        //                //EvalEnteredVals(ref sc, sc.sKMC, sc.fEmk, sc.nParty, dvT, i, iTMax);

        //                EvalEnteredKol(dvT, i, ref sc, sc.sKMC, sc.fEmk, out nM, out fV, true);

        //                //td2 = Environment.TickCount;
        //                //tc1 += (td2 - td1);

        //                iCur = i;
        //                nRet = EvalDiffZVK(ref sc, dvZ, dvT, lstProt, iStart, iZMax, ref iCur, iTMax, nM, fV);

        //                //tc2 += (Environment.TickCount - td2);

        //                if (nDokState != AppC.RC_CANCEL)
        //                {
        //                    if (nRet != AppC.RC_OK)
        //                        nDokState = nRet;
        //                }
        //            }
        //            else
        //            {
        //                switch(nRet)
        //                {

        //                    case AppC.RC_NOEAN:
        //                        // ��� �����������
        //                        s1 = "";
        //                        xProt = "";
        //                        fE = -100;
        //                        break;
        //                    case AppC.RC_NOEANEMK:
        //                        // ������� �����������
        //                        s1 = "���.";
        //                        xProt = sc.fEmk;
        //                        fE = sc.fEmk;
        //                        break;
        //                    case AppC.RC_BADPARTY:
        //                        // ��� ������
        //                        s1 = "����.";
        //                        xProt = sc.nParty;
        //                        fE = sc.fEmk;
        //                        break;
        //                    default:
        //                        s1 = String.Format("���={0}", sc.fEmk);
        //                        xProt = String.Format("����-{0}", sc.nParty);
        //                        fE = sc.fEmk;
        //                        break;
        //                }
        //                nDokState = AppC.RC_CANCEL;

        //                lstProt.Add(String.Format("_{0} {1} {2}:��� ������", sc.nKrKMC, s1, xProt));
        //                iCur = SetTTNState(dvT, ref sc, fE, NSI.DESTINPROD.USER, i, iTMax);
        //            }
        //            if (iCur != -1)
        //                i = iCur;

        //            i++;
        //        }

        //        //t2 = Environment.TickCount;

        //        lstProt.Add("<->---- ������ ----<->");
        //        for (i = 0; i < dvZ.Count; i++)
        //        {
        //            if ((NSI.READINESS)dvZ[i]["READYZ"] != NSI.READINESS.FULL_READY)
        //            {
        //                nDokState = AppC.RC_CANCEL;
        //                drC = dvZ[i].Row;
        //                xR = new RowObj(drC);
        //                try
        //                {
        //                    if (xR.IsEAN)
        //                    {
        //                        s1 = ((NSI.READINESS)dvZ[i]["READYZ"] == NSI.READINESS.NO)?"��� �����":"��������";
        //                        if ((FRACT)drC["EMK"] > 0)
        //                            lstProt.Add(String.Format("_{0}:{2}-{1} �", (int)drC["KRKMC"], (int)drC["KOLM"], s1));
        //                        else
        //                            lstProt.Add(String.Format("_{0}:{2}-{1} ��", (int)drC["KRKMC"], (FRACT)drC["KOLE"], s1));
        //                    }
        //                    else
        //                    {
        //                        if (xR.IsSSCCINT || xR.IsSSCC)
        //                        {
        //                            lstProt.Add(String.Format("����.{0} {1}:��� �����", xR.sSSCCINT, xR.sSSCC));
        //                        }
        //                    }
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












        ///// �������� ����� ��������� �� ������������ ������
        //private void ControlDocZVK_Old(DataRow drD, List<string> lstProt)
        //{
        //    bool
        //        bIsKMPL;
        //    int
        //        i = 0,
        //        nM,
        //        iStart,
        //        iCur,
        //        iCurSaved,
        //        iTMax, iZMax,
        //        nDokState = AppC.RC_OK,
        //        nRet;
        //    string
        //        s1,
        //        s2,
        //        sOldKMC,
        //        sFlt;
        //    FRACT
        //        fE,
        //        fV = 0;
        //    object
        //        xProt;

        //    DataRow
        //        drC;
        //    RowObj
        //        xR;

        //    //TimeSpan tsDiff;
        //    //int t1 = Environment.TickCount, t2, t3, td1, td2, tc = 0, tc1 = 0, tc2 = 0;
        //    //t2 = t1;

        //    if (drD == null)
        //        drD = xCDoc.drCurRow;

        //    bIsKMPL = (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL) ? true : false;

        //    string sRf = String.Format("(SYSN={0})", drD["SYSN"]);

        //    PSC_Types.ScDat sc = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
        //    lstProt.Add(HeadLineCtrl(drD));

        //    // ���� ��� ��������� �������� ����������
        //    drD["DIFF"] = NSI.DOCCTRL.UNKNOWN;

        //    // ��� ��������� �� ������ �� ���������
        //    DataView
        //        //dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "KMC", DataViewRowState.CurrentRows);
        //        dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "KRKMC", DataViewRowState.CurrentRows);

        //    iZMax = dvZ.Count;
        //    if (iZMax <= 0)
        //    {
        //        nDokState = AppC.RC_CANCEL;
        //        lstProt.Add("*** ������ �����������! ***");
        //    }
        //    else
        //        bZVKPresent = true;

        //    // ��� ��������� �� ��� �� ���������
        //    DataView
        //        //dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KMC,EMK DESC", DataViewRowState.CurrentRows);
        //    dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KRKMC,EMK DESC", DataViewRowState.CurrentRows);
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
        //        foreach (DataRowView dr in dvT)
        //        {// ����� ���� ���������� �����
        //            dr["DEST"] = NSI.DESTINPROD.UNKNOWN;
        //            dr["NPP_ZVK"] = -1;
        //        }


        //        lstProt.Add("<->----- ��� ------<->");
        //        sOldKMC = "";
        //        while (i < iTMax)
        //        {

        //            if ((int)dvT[i]["DEST"] != (int)NSI.DESTINPROD.UNKNOWN)
        //            {// ������ ��������� ��� ����������
        //                i++;
        //                continue;
        //            }

        //            drC = dvT[i].Row;
        //            // ��� �� ������ � ������?
        //            xR = new RowObj(drC);

        //            if (xR.AllFlags == (int)AppC.OBJ_IN_DROW.OBJ_NONE)
        //            {
        //                lstProt.Add("��� ���������/SSCC");
        //                i++;
        //                continue;
        //            }
        //            if (!xR.IsEAN)
        //            {// ���� �� SSCC
        //                sFlt = "";
        //                if (xR.IsSSCCINT)
        //                    sFlt += String.Format("AND(SSCCINT='{0}')", xR.sSSCCINT);
        //                if (xR.IsSSCC)
        //                    sFlt += String.Format("AND(SSCC='{0}')", xR.sSSCC);

        //                DataView dvZSC = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf + sFlt, "SSCC,SSCCINT",
        //                    DataViewRowState.CurrentRows);
        //                if (dvZSC.Count > 0)
        //                    dvZSC[0].Row["READYZ"] = NSI.READINESS.FULL_READY;
        //                else
        //                {// SSCC �� ������
        //                    lstProt.Add(String.Format("����.{0} {1}:��� ������", xR.sSSCCINT, xR.sSSCC));
        //                }
        //                i++;
        //                continue;
        //            }

        //            sc.sEAN = (string)drC["EAN13"];
        //            sc.sKMC = (string)drC["KMC"];
        //            sc.nKrKMC = (int)drC["KRKMC"];

        //            sc.bVes = ((int)(drC["SRP"]) > 0) ? true : false;

        //            sc.fEmk = (FRACT)drC["EMK"];
        //            sc.nParty = (string)drC["NP"];
        //            sc.dDataIzg = DateTime.ParseExact((string)drC["DVR"], "yyyyMMdd", null);
        //            //sc.nTara = (string)drC["KRKT"];
        //            sc.nTara = (string)drC["KTARA"];

        //            sc.nRecSrc = (int)NSI.SRCDET.CR4CTRL;

        //            //td1 = Environment.TickCount;

        //            iStart = dvZ.Find(sc.nKrKMC);
        //            //iStart = dvZ.Find(sc.sKMC);
        //            if (iStart != -1)
        //                nRet = EvalZVKMest(ref sc, dvZ, iStart, iZMax);
        //            //nRet = LookAtZVK(ref sc, dvZ, iStart, iZMax);
        //            else
        //                nRet = AppC.RC_NOEAN;

        //            //tc += (Environment.TickCount - td1);

        //            iCur = -1;
        //            if (nRet == AppC.RC_OK)
        //            {// ���� ��� �������� ��� ����� �������
        //                //td1 = Environment.TickCount;

        //                if ((bIsKMPL) || true)
        //                {
        //                    sc.nMest = (int)drC["KOLM"];
        //                    sc.fVsego = (FRACT)drC["KOLE"];
        //                    EvalZVKStateNew(ref sc, drC);

        //                    if (sOldKMC != sc.sKMC)
        //                    {// ����� ����
        //                        iCurSaved = i;

        //                        //EvalEnteredKol(dvT, i, ref sc, sc.sKMC, sc.fEmk, out nM, out fV, true);

        //                        iCur = i;

        //                        //nRet = EvalDiffZVK(ref sc, dvZ, dvT, lstProt, iStart, iZMax, ref iCur, iTMax, nM, fV);

        //                        //if (nDokState != AppC.RC_CANCEL)
        //                        //{
        //                        //    if (nRet != AppC.RC_OK)
        //                        //        nDokState = nRet;
        //                        //}
        //                        sOldKMC = sc.sKMC;
        //                    }
        //                }
        //                else
        //                {
        //                    EvalEnteredKol(dvT, i, ref sc, sc.sKMC, sc.fEmk, out nM, out fV, true);

        //                    //td2 = Environment.TickCount;
        //                    //tc1 += (td2 - td1);

        //                    iCur = i;
        //                    nRet = EvalDiffZVK(ref sc, dvZ, dvT, lstProt, iStart, iZMax, ref iCur, iTMax, nM, fV);

        //                    //tc2 += (Environment.TickCount - td2);

        //                    if (nDokState != AppC.RC_CANCEL)
        //                    {
        //                        if (nRet != AppC.RC_OK)
        //                            nDokState = nRet;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                switch (nRet)
        //                {

        //                    case AppC.RC_NOEAN:
        //                        // ��� �����������
        //                        s1 = "";
        //                        xProt = "";
        //                        fE = -100;
        //                        break;
        //                    case AppC.RC_NOEANEMK:
        //                        // ������� �����������
        //                        s1 = "���.";
        //                        xProt = sc.fEmk;
        //                        fE = sc.fEmk;
        //                        break;
        //                    case AppC.RC_BADPARTY:
        //                        // ��� ������
        //                        s1 = "����.";
        //                        xProt = sc.nParty;
        //                        fE = sc.fEmk;
        //                        break;
        //                    default:
        //                        s1 = String.Format("���={0}", sc.fEmk);
        //                        xProt = String.Format("����-{0}", sc.nParty);
        //                        fE = sc.fEmk;
        //                        break;
        //                }
        //                nDokState = AppC.RC_CANCEL;

        //                lstProt.Add(String.Format("_{0} {1} {2}:��� ������", sc.nKrKMC, s1, xProt));
        //                iCur = SetTTNState(dvT, ref sc, fE, NSI.DESTINPROD.USER, i, iTMax);
        //            }
        //            if (iCur != -1)
        //                i = iCur;

        //            i++;
        //        }

        //        //t2 = Environment.TickCount;

        //        lstProt.Add("<->---- ������ ----<->");
        //        for (i = 0; i < dvZ.Count; i++)
        //        {
        //            if ((NSI.READINESS)dvZ[i]["READYZ"] != NSI.READINESS.FULL_READY)
        //            {
        //                nDokState = AppC.RC_CANCEL;
        //                drC = dvZ[i].Row;
        //                xR = new RowObj(drC);
        //                try
        //                {
        //                    if (xR.IsEAN)
        //                    {
        //                        s1 = ((NSI.READINESS)dvZ[i]["READYZ"] == NSI.READINESS.NO) ? "��� �����" : "��������";
        //                        if ((FRACT)drC["EMK"] > 0)
        //                            lstProt.Add(String.Format("_{0}:{2}-{1} �", (int)drC["KRKMC"], (int)drC["KOLM"], s1));
        //                        else
        //                            lstProt.Add(String.Format("_{0}:{2}-{1} ��", (int)drC["KRKMC"], (FRACT)drC["KOLE"], s1));
        //                    }
        //                    else
        //                    {
        //                        if (xR.IsSSCCINT || xR.IsSSCC)
        //                        {
        //                            lstProt.Add(String.Format("����.{0} {1}:��� �����", xR.sSSCCINT, xR.sSSCC));
        //                        }
        //                    }
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















        // ������� ����� ������� � ���������
        private int EvalDiffZVK(ref PSC_Types.ScDat sc, DataView dvZ, DataView dvT, List<string> lstProt,
            int iZ, int iZMax, ref int iT, int iTMax, int nMAll, FRACT fEAll)
        {
            bool 
                bNeedSetZVK = false;
            int 
                nRet = AppC.RC_OK,
                nM = 0;
            FRACT 
                fV = 0;
            NSI.READINESS 
                rpEmk = NSI.READINESS.NO;

            if (sc.fEmk > 0)
            {
                if (sc.nKolM_zvk > 0)
                {
                    bNeedSetZVK = true;


                    //nM = sc.nKolM_zvk - sc.nKolM_alrT;
                    //if ( (sc.nKolM_alr > 0) && (sc.nKolM_alrT == 0))
                    //    nM = sc.nKolM_zvk - sc.nKolM_alr;

                    if ((int)sc.lstAvailInZVK[0]["COND"] == (int)NSI.SPECCOND.NO)
                        nM = sc.nKolM_zvk - nMAll;
                    else
                    {
                        nM = sc.nKolM_zvk - sc.nKolM_alrT;
                        if ((sc.nKolM_alr > 0) && (sc.nKolM_alrT == 0))
                            nM = sc.nKolM_zvk - sc.nKolM_alr;
                    }

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
                    iT = SetTTNState(dvT, ref sc, sc.fEmk, NSI.DESTINPROD.PARTZ, iT, iTMax);
                    if (bNeedSetZVK == true)
                    {
                        SetZVKState(dvZ, ref sc, rpEmk, iZ, iZMax);
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
                        //fV = sc.fKolE_zvk - sc.fKolE_alrT;
                        //if ((sc.fKolE_alr > 0) && (sc.fKolE_alrT == 0))
                        //    fV = sc.fKolE_zvk - sc.fKolE_alr;

                        if ((int)sc.lstAvailInZVK[0]["COND"] == (int)NSI.SPECCOND.NO)
                            fV = sc.fKolE_zvk - fEAll;
                        else
                        {
                            fV = sc.fKolE_zvk - sc.fKolE_alrT;
                            if ((sc.fKolE_alr > 0) && (sc.fKolE_alrT == 0))
                                fV = sc.fKolE_zvk - sc.fKolE_alr;
                        }
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
                                lstProt.Add(String.Format(" {0}:������ {1} ��",
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
                        iT = SetTTNState(dvT, ref sc, sc.fEmk, NSI.DESTINPROD.PARTZ, iT, iTMax);
                        if (bNeedSetZVK == true)
                        {
                            SetZVKState(dvZ, ref sc, rpEmk, iZ, iZMax);
                        }
                    }
                    catch { }
                }
            }

            return (nRet);
        }


        private void SetZVKState(DataView dv, ref PSC_Types.ScDat sc, NSI.READINESS rpE, int i, int nZMax)
        {
            if (sc.lstAvailInZVK.Count > 0)
            {
                foreach (DataRow drl in sc.lstAvailInZVK)
                {
                    drl["READYZ"] = rpE;
                }
            }
            else
            {
                while ((i < nZMax) && ((string)dv[i]["KMC"] == sc.sKMC))
                {
                    if (sc.fEmk == (FRACT)dv[i]["EMK"])
                        dv[i]["READYZ"] = rpE;
                    i++;
                }
            }
        }

        // ��������� ��������
        // - �� ����� ���� (��� RC_NOEAN), fE = -100
        // - �� ����� ���� � ������ ������� (��� RC_NOEAN)
        private int SetTTNState(DataView dv, ref PSC_Types.ScDat sc, FRACT fE, NSI.DESTINPROD dSt, int i, int iMax)
        {
            //int tss1 = Environment.TickCount;
            int 
                nLastI = i;

            if ((fE >= 0) && (dSt != NSI.DESTINPROD.USER))
            {
                dv[i]["DEST"] = dSt;
                nLastI = i;
            }
            else
            {
                while ((i < iMax) && ((string)dv[i]["KMC"] == sc.sKMC))
                {
                    if ((fE < 0) || (fE == (FRACT)dv[i]["EMK"]))
                    {
                        dv[i]["DEST"] = dSt;
                        nLastI = i;
                    }
                    i++;
                }
            }
            //tss += (Environment.TickCount - tss1);
            return (nLastI);
        }
        //int tss = 0;













        // ������� ��������
        private int JoinEd(DataRow drD, DataTable dt, List<string> lstProt)
        {
            int
                nRet = 0,
                i = 0;
            string
                sRf,
                sBf;

            DataView
                dvBox,
                dvT;
            System.Collections.Generic.List<object[]>
                x4Del = new List<object[]>();


            sRf = String.Format("(SYSN={0})AND(KOLM=0)AND(EMK=0)", drD["SYSN"]);

            // ��� ��������� �� ��� �� ���������
            i = 0;
            dvT = new DataView(dt, sRf, "KMC", DataViewRowState.CurrentRows);
            for (i = 0; i < dvT.Count; i++)
                x4Del.Add(new object[] { dvT[i]["KMC"], dvT[i]["NP"], dvT[i]["DVR"], dvT[i]["KRKMC"], dvT[i]["KOLE"], dvT[i].Row });

            for (i = 0; i < x4Del.Count; i++)
            {
                sBf = String.Format("(SYSN={0})AND(KMC='{1}')AND(NP='{2}')AND(DVR='{3}')AND(KOLM>0)AND(EMK>0)",
                    drD["SYSN"], x4Del[i][0], x4Del[i][1], x4Del[i][2]);
                dvBox = new DataView(dt, sBf, "KMC", DataViewRowState.CurrentRows);
                if (dvBox.Count > 0)
                {
                    nRet++;
                    lstProt.Add(String.Format("{0,-4} ����.-{1,-4} ��.={2}", x4Del[i][3], x4Del[i][1], x4Del[i][4]));
                    dvBox[0]["KOLE"] = (FRACT)dvBox[0]["KOLE"] + (FRACT)x4Del[i][4];
                    dt.Rows.Remove((DataRow)x4Del[i][5]);
                }
            }
            lstProt.Insert(0, String.Format("����� �����������/�������:{0}", nRet));
            return (nRet);
        }










    }
}
