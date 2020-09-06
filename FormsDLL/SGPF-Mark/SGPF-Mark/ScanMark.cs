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

    public partial class MainF : Form
    {


        public enum ADR_TYPE : int
        {
            UNKNOWN = 0,
            OBJECT = 1,                    // ������
            STELLAGE = 2,                    // �������
            ZONE = 4                     // ����
        }

        // ���������� �� ������ ��� ��������
        public class AddrInfo
        {
            // ���������: ��-������, ���-����� ��������, ��-�����, �-����, �-����� �������
            private string m_FullAddr = "";           // ����� ������-����

            public string sMesto = "";           // ����� ������-����
            public string sCanal = "";           // ����� ������-����
            public string sYarus = "";           // ����� ������-����

            public string sName = "";           // ������������ ������-����
            public string sCat = "";            // ��������� ������-����

            public bool bFixed = false;         // ����� ������������

            public ADR_TYPE nType = ADR_TYPE.UNKNOWN;         // ��� ������

            public DateTime
                dtScan = DateTime.Now;

            private MainF xS = null;
            private ExprDll.RUN xR = null;
            //private ExprDll.Action xExA = null;

            public AddrInfo() { }

            public AddrInfo(string sA) : this(sA, "", null) { }

            public AddrInfo(string sA, string sN, MainF x)
            {
                Addr = sA;
                sName = sN;
                xS = x;
                if (x is MainF)
                    xR = xS.xGExpr.run;


                //if (xS.xExpDic.ContainsKey("APPBLK"))
                //{
                //    xExA = xS.xExpDic["APPBLK"].ExprVal.run.FindFunc("NameAdr");
                //    xR = xS.xExpDic["APPBLK"].ExprVal.run;
                //}
            }


            // ������ ������
            public string Addr
            {
                get { return m_FullAddr; }
                set
                {
                    try
                    {
                        m_FullAddr = value;
                        sMesto = m_FullAddr.Substring(2, 3);
                        sCanal = m_FullAddr.Substring(5, 2);
                        sYarus = m_FullAddr.Substring(7, 1);
                        nType = (m_FullAddr.Substring(5, 4) == "0000") ? ADR_TYPE.ZONE : ADR_TYPE.OBJECT;
                    }
                    catch
                    {
                        sMesto = sCanal = sYarus = "";
                        nType = ADR_TYPE.UNKNOWN;
                    }
                }
            }

            private string x;
            // ���������� ����������� ������
            public string AddrShow
            {
                get
                {
                    if (nType == ADR_TYPE.UNKNOWN)
                        x = "";
                    else
                    {
                        if (nType == ADR_TYPE.OBJECT)
                        {
                            if (xR != null)
                            {
                                //x = (string)xR.ExecFunc("NameAdr", new object[] { xS.xSm.nSklad, Addr }, xS.xExpDic["APPBLK"].ActionVal);

                                x = (string)xR.ExecFunc("NameAdr", new object[] { xS.xSm.nSklad, Addr });


                                //x = (string)xS.xExpDic["APPBLK"].ExprVal.run.ExecFunc("NameAdr", new object[] { xS.xSm.nSklad, Addr }, xS.xExpDic["APPBLK"].ActionVal);
                            }
                            else
                                x = String.Format("{0}-{1}.{2}", sMesto, sCanal, sYarus);
                        }
                        else
                        {
                            if ((nType == ADR_TYPE.ZONE) && (sName.Length > 0))
                                x = sName;
                            else
                                x = m_FullAddr;
                        }
                    }
                    return x;
                }
            }

        }








        ScanVarGP xScan, xScanPrev = null;

        private bool bInScanProceed = false;

        // ��������� ������������ ������������
        private void OnScan(object sender, BarcodeScannerEventArgs e)
        {
            bool 
                bRet = AppC.RC_CANCELB,
                bEasyEd,
                bDupScan;
            int nRet = AppC.RC_CANCEL;
            string sErr = "";

            // �������� ��������� ������������
            bInScanProceed = true;
            if (e.nID != BCId.NoData)
            {
                try
                {
                    PSC_Types.ScDat sc = new PSC_Types.ScDat(e, xCDoc.xOper);
                    //sc.xSV.dgTest = new TestBCFull(sc.TFullBC);
                    xScan = new ScanVarGP(e, xNSI.DT["NS_AI"].dt);
                    bDupScan = ((xScanPrev != null) && (xScanPrev.Dat == xScan.Dat)) ? true : false;

                    //nTypBCode = TypeBC(ref sc);
                    sc.sN = e.Data + "-???";

                    #region ��������� �����
                    do
                    {
                        switch (tcMain.SelectedIndex)
                        {
                            case PG_DOC:
                                ProceedScanDoc(xScan, ref sc);
                                nRet = AppC.RC_OK;
                                break;
                            case PG_SCAN:
                                if (bDupScan)
                                {// ������������� �������� ������ ���������
                                    if (xCDoc.nTypOp == AppC.TYPOP_PRMK)
                                    {
                                        SetOverOPR(true);
                                        xScan = null;
                                        break;
                                    }
                                }

                                if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_ZONE) == ScanVarGP.BCTyp.SP_ADR_ZONE) ||
                                     ((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_STLG) == ScanVarGP.BCTyp.SP_ADR_STLG) ||
                                     ((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) == ScanVarGP.BCTyp.SP_ADR_OBJ))
                                {
                                    int nRetAdr = ProceedAdr(xScan, ref sc);
                                    break;
                                }

                                //if (nSpecAdrWait > 0)
                                //{// ���������� ������� �������� ������
                                //    nSpecAdrWait = 0;
                                //    xFPan.HideP();
                                //    if (xSm.xAdrFix1 != null)
                                //    {
                                //        sErr = String.Format("������������� {0}\n ����� �������...", xSm.xAdrFix1.Addr);
                                //        Srv.ErrorMsg(sErr, true);
                                //        xSm.xAdrFix1 = null;
                                //        lDocInf.Text = CurDocInf(xCDoc.xDocP);
                                //        break;
                                //    }
                                //}

                                if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) == ScanVarGP.BCTyp.SP_SSCC_EXT) ||
                                    ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_INT) == ScanVarGP.BCTyp.SP_SSCC_INT))
                                {
                                    int nRetSSCC = ProceedSSCC(xScan, ref sc);
                                    if (nRetSSCC == AppC.RC_WARN)
                                    {
                                        bRet = true;
                                    }
                                    else
                                    {
                                        ChkOPR(true);
                                        xScan = null;
                                        break;
                                    }
                                }
                                else
                                {// ������ ���� ���������
                                    if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_OLD_ETIK) == ScanVarGP.BCTyp.SP_OLD_ETIK) ||
                                         (xScan.Id != BCId.Code128))
                                    {// ������ �������� ��� EAN13
                                        bRet = TranslSCode(ref sc);
                                    }
                                    else
                                    {// ����� ��������
                                        //if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_NEW_ETIK) == ScanVarGP.BCTyp.SP_NEW_ETIK))
                                        //    bRet = NewTranslSCode(ref sc);
                                        //else
                                        //{
                                        //    if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDV) == ScanVarGP.BCTyp.SP_MT_PRDV))
                                        //        bRet = TranslMT(ref sc);
                                        //    else if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDVN) == ScanVarGP.BCTyp.SP_MT_PRDVN))
                                        //        bRet = TranslMTNew(ref sc);

                                        //}

                                        if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDV) == ScanVarGP.BCTyp.SP_MT_PRDV))
                                            bRet = TranslMT(ref sc);
                                        else if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDVN) == ScanVarGP.BCTyp.SP_MT_PRDVN))
                                            bRet = TranslMTNew(ref sc);
                                        else
                                            // ������ �������� ����������� �������� �� AI
                                            bRet = NewTranslSCode(ref sc);
                                    }
                                }

                                if (!bRet)
                                    throw new Exception("������ ������������!");

                                if (xPars.WarnNewScan == true)
                                {// �������������� ��� ������������� �����
                                    if ((bInEasyEditWait && !bDupScan) || (bEditMode == true))
                                    {
                                        Srv.ErrorMsg("��������� ����!", true);
                                        break;
                                    }
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

                                nRet = ProceedProd(xScan, ref sc, bDupScan, bEasyEd);
                                if (ChkOPR(true) != AppC.RC_OK)
                                {
                                    break;
                                }
                                break;
                        }
                        xScanPrev = xScan;
                    } while (false);
                    #endregion
                }
                catch (Exception ex)
                {
                    string sE = String.Format("{0}({1}){2}", xScan.Id.ToString(), xScan.Dat.Length, xScan.Dat);
                    if (tcMain.SelectedIndex == PG_SCAN)
                        tNameSc.Text = sE;
                    Srv.ErrorMsg(sE + "\n" + ex.Message, "������ ������������", true);
                }
            }
            // ��������� ������������ ��������
            bInScanProceed = false;
            ResetTimerReLogon(true);
        }

        // �������� ����������� ������ ����� ����� ���������
        private int ChkOPR(bool bAfterScan)
        {
            int 
                nRet = AppC.RC_OK;


            if ((xCDoc.nTypOp == AppC.TYPOP_MOVE) && (xPars.OpChkAdr))
            {
                if (xCDoc.xOper.IsFillSrc())
                {
                    ServerExchange xSE = new ServerExchange(this);

                    xCUpLoad = new CurUpLoad(xPars);
                    xDP = xCUpLoad.xLP;

                    xCUpLoad.bOnlyCurRow = true;
                    xCUpLoad.drForUpl = drDet;
                    xCUpLoad.sCurUplCommand = AppC.COM_CKCELL;
                    string sL = UpLoadDoc(xSE, ref nRet);

                    if ((xSE.ServerRet != AppC.EMPTY_INT) &&
                        (xSE.ServerRet != AppC.RC_OK))
                    {// �������� �������� �� ������ �� ������� (�������������� ������)
                        Srv.ErrorMsg(sL, "������ ����������", true);
                        AddrInfo xA = xCDoc.xOper.xAdrSrc;
                        xCDoc.xOper = new CurOper();
                        xCDoc.xOper.xAdrSrc = xA;
                        drDet.Delete();
                    }
                    else
                        nRet = AppC.RC_OK;
                }
            }
            return (nRet);
        }





        private int SetOverOPR(bool bAfterScan)
        {
            int nRet = AppC.RC_OK;
            ServerExchange xSE = new ServerExchange(this);

            if (bEditMode == false)
            {
                if ((drDet != null) && (bShowTTN))
                {
                    if ((AppC.OPR_STATE)drDet["STATE"] != AppC.OPR_STATE.OPR_UPL)
                    {
                        if (bAfterScan)
                        {
                            //if ((scCur.sKMC == (string)drDet["EAN13"]) &&
                            if ((scCur.sKMC == (string)drDet["KMC"]) &&
                                (scCur.nParty == (string)drDet["NP"]) &&
                                (scCur.dDataIzg.ToString("yyyyMMdd") == (string)drDet["DVR"]))
                                bAfterScan = false;
                        }
                        if (!bAfterScan)
                        {// �������� �� ��������
                            drDet["STATE"] = AppC.OPR_STATE.OPR_OVER;
                            xCUpLoad = new CurUpLoad(xPars);
                            xDP = xCUpLoad.xLP;

                            xCUpLoad.bOnlyCurRow = true;
                            xCUpLoad.drForUpl = drDet;
                            //xFPan = new FuncPanel(this, this.pnLoadDocG);
                            //EditOverBeforeUpLoad(AppC.RC_OK, 0);

                            if (xPars.OpAutoUpl)
                            {// ����-�������� ��������
                                string sL = UpLoadDoc(xSE, ref nRet);
                                if (xSE.ServerRet == AppC.RC_OK)
                                    xCDoc.xOper = new CurOper();

                                if (nRet != AppC.RC_OK)
                                {
                                    if (nRet == AppC.RC_HALFOK)
                                    {
                                        Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                                        MessageBox.Show(sL, "��������������!");
                                    }
                                    else
                                        Srv.ErrorMsg(sL, true);
                                }

                                if ((xSE.ServerRet != AppC.EMPTY_INT) &&
                                    (xSE.ServerRet != AppC.RC_OK))
                                {// �������� �������� �� ������ �� ������� (�������������� ������)
                                if (xSE.ServerRet == 99)
                                    CompareAddrs(xCDoc.xOper.xAdrDst.Addr, String.Format("---{0}-----------����� ��������", xSE.ServerRet), true);
                                    xCDoc.xOper.xAdrDst = null;
                                    tDatMC.Text = "";
                                }

                            }
                            else
                                xCDoc.xOper = new CurOper();
                            xCUpLoad = null;
                        }
                    }
                }
            }
            return (nRet);
        }

        //private int AddGroupDet(ref PSC_Types.ScDat scD, int nRLoad)


        private int AddGroupDet(int nRLoad)
        {
            int nRet = AppC.RC_OK,
                nRec;

            if (nRLoad == AppC.RC_MANYEAN)
            {// ���������� ������ ����� (���������������� ������)
                if (xCDoc.nTypOp == AppC.TYPOP_DOCUM)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    nRec = xCLoad.dtZ.Rows.Count;
                    for (int i = 0; i < nRec; i++)
                    {
                        PSC_Types.ScDat scMD = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.EAN13, 
                            xCLoad.dtZ.Rows[i]["EAN13"].ToString()));
                        SetVirtScan(xCLoad.dtZ.Rows[i], ref scMD, false);
                        TryEvalNewZVKTTN(ref scMD, false);
                        AddDet1(ref scMD);
                    }
                    nRet = AppC.RC_OK;
                    Cursor.Current = Cursors.Default;
                }
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
                ret = ConvertSSCC2Lst(xSE, xSc, ref scD, true);
                if (ret == AppC.RC_OK)
                {// ��� ���� ���, ������ ������� ��������
                    ret = AppC.RC_WARN;
                }
                else
                {
                    AddGroupDet(ret);
                    // � ����� ������ ��������� ����� �����������
                    ret = AppC.RC_OK;
                }
            }
            return ret;
        }

        private int nSpecAdrWait = 0;
        //private int ProceedSpecAdr(PSC_Types.ScDat scD)
        //{
        //    int
        //        nRet = AppC.RC_OK;
        //    string
        //        sH,
        //        sErr;

        //    if (nSpecAdrWait > 0)
        //    {// ���������� ������� ��������/������ ������
        //        nSpecAdrWait = 0;
        //        xFPan.HideP();
        //        // ������ ������� �� �����������
        //        ehCurrFunc -= Keys4FixAddr;

        //        if (xSm.xAdrFix1 != null)
        //        {
        //            sErr = "������������� ����� �������...";
        //            xSm.xAdrFix1 = null;
        //        }
        //        else
        //        {
        //            xSm.xAdrFix1 = new AddrInfo(scD.sN);
        //            xSm.xAdrFix1.sName = xNSI.AdrName(scD.sN);
        //            xSm.xAdrFix1.nType = (xScan.bcFlags == ScanVarGP.BCTyp.SP_ADR_OBJ) ? ADR_TYPE.OBJECT : ADR_TYPE.ZONE;
        //            sErr = "����� ������������...";
        //        }
        //        sH = xSm.xAdrFix1.sName;
        //        Srv.ErrorMsg(sErr, sH, true);
        //        lDocInf.Text = CurDocInf(xCDoc.xDocP);
        //    }
        //    return (nRet);
        //}


        // ��������� ����������� ������
        private int ProceedAdr(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        {
            int 
                ret = AppC.RC_OK,
                nUseAdr = 0; // 1-From, 2-To

            // �������� ������
            scD.sN = xSc.Dat.Substring(2);

            if (xCDoc.nTypOp == AppC.TYPOP_DOCUM)
            {
                DialogResult dRez = MessageBox.Show(
                    "����� �� ����� (Enter)?\n(ESC)- �������� ������", "���������� ������",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                //DialogResult dRez = (xCDoc.xDocP.nTypD != AppC.TYPD_INV) ? DialogResult.OK :
                //    MessageBox.Show("����� �� ����� (Enter)?\n(ESC)- �������� ������", "���������� ������",
                //    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                string sTypeSPR = (dRez == DialogResult.OK)?"TXT":"ROW";
                ConvertAdr2Lst(scD.sN, sTypeSPR);
            }

            if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_ZONE) == ScanVarGP.BCTyp.SP_ADR_ZONE) ||
                 ((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) == ScanVarGP.BCTyp.SP_ADR_OBJ))
                {// ����� ���� ��� �������
                if (nSpecAdrWait > 0)
                {// ���������� ������� �������� ������
                    nSpecAdrWait = 0;
                    xFPan.HideP();
                    // ������ ������� �� �����������
                    ehCurrFunc -= Keys4FixAddr;

                    xSm.xAdrFix1 = new AddrInfo(scD.sN);
                    xSm.xAdrFix1.sName = xNSI.AdrName(scD.sN);
                    xSm.xAdrFix1.nType = (xScan.bcFlags == ScanVarGP.BCTyp.SP_ADR_OBJ) ? ADR_TYPE.OBJECT : ADR_TYPE.ZONE;
                    Srv.ErrorMsg("����� ������������...", xSm.xAdrFix1.sName, true);
                    lDocInf.Text = CurDocInf(xCDoc.xDocP);
                    return (ret);
                }
            }

            if (xSm.xAdrFix1 != null)
            {// ������������ �����, ������ ��� ����
                if (!xCDoc.xOper.bObjOperScanned)
                {// ������ ��� �� ������������, ������ ������ ����� �����������
                    nUseAdr = 1;
                    xCDoc.xOper.xAdrDst = xSm.xAdrFix1;
                }
                else
                {// ������ ��� ������������, ������ ������ ����� ����������
                    nUseAdr = 2;
                    xCDoc.xOper.xAdrSrc = xSm.xAdrFix1;
                }
            }
            else
            {// ������������� ������� ���� �� ����

                if (!xCDoc.xOper.IsFillSrc())
                    nUseAdr = 1;
                else
                {
                    if (xCDoc.nTypOp == AppC.TYPOP_DOCUM)
                        nUseAdr = 1;
                    else
                    {
                        if (!xCDoc.xOper.IsFillDst() && !xCDoc.xOper.bObjOperScanned)
                        {
                            DialogResult drQ = MessageBox.Show("�������� \"��������\" (Enter)?\n(ESC) - ������",
                                "����� �����!",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (drQ == DialogResult.OK)
                                nUseAdr = 1;
                            else
                                nUseAdr = 0;
                        }
                        else
                            nUseAdr = 2;
                    }
                }
            }

            if (nUseAdr > 0)
            {// ��������������� ������� ������� ���������������  ��� ������� ����������� ��� ����������
                AddrInfo xA = new AddrInfo(scD.sN, xNSI.AdrName(scD.sN), this);

                if (nUseAdr == 1)
                {// ��� ��������
                    if (xCDoc.xOper.GetDst(false) == xA.Addr)
                        xA = null;
                    else
                        xCDoc.xOper.xAdrSrc = xA;
                }
                else
                {// ��� ��������
                    if (xCDoc.xOper.GetSrc(false) == xA.Addr)
                        xA = null;
                    else
                        xCDoc.xOper.xAdrDst = xA;
                }
                if (xA == null)
                    Srv.ErrorMsg("������ ���������...", scD.sN, true);
                else
                {
                    if (xCDoc.nTypOp == AppC.TYPOP_MOVE)
                    {
                        tEAN.Text = xCDoc.xOper.GetSrc(true);
                        tDatMC.Text = xCDoc.xOper.GetDst(true);
                        IsOperReady(true);
                    }
                }
            }

            return (ret);
        }

        private bool IsOperReady(bool bProceedAddr)
        {
            bool 
                bRet = xCDoc.xOper.IsFillAll();
            if (bRet)
            {
                drDet["TIMEOV"] = xCDoc.xOper.xAdrDst.dtScan;
                drDet["ADRFROM"] = xCDoc.xOper.xAdrSrc.Addr;
                drDet["ADRTO"] = xCDoc.xOper.xAdrDst.Addr;
                if (bProceedAddr)
                    CompareAddrs(xCDoc.xOper.xAdrDst.Addr, "", false);
                SetOverOPR(false);
            }
            return (bRet);
        }

        private void CompareAddrs(string sA, string sReason, bool bForceWrite)
        {
            string
                sFProt = "",
                sS = xScan.Dat.Substring(2);

            if ((sS != sA) || (bForceWrite))
            {
                sFProt = 
                    (System.IO.Directory.Exists(@"\FlashDisk\Progs"))?@"\FlashDisk\Progs\AdrErr" :
                    (System.IO.Directory.Exists(@"\BACKUP\Progs")) ? @"\BACKUP\Progs" :
                    (System.IO.Directory.Exists(@"\Flash\Progs")) ? @"\Flash\Progs\AdrErr" : @"\Temp\AdrErr";
                System.IO.StreamWriter swT = System.IO.File.AppendText(sFProt);
                swT.WriteLine(String.Format("{3} Scanned***{0}***---��������***{1}***===={2}", xScan.Dat, sA, sReason, DateTime.Now.ToString("dd.MM.yy hh:mm:ss")));
                swT.Close();
            }
        }


        // ��������� SSCC ��� �����
        //int ProceedSSCC(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        //{
        //    int ret = AppC.RC_OK;
        //    bool bMaySet = true;
        //    DataRow dr;
        //    RowObj xR;
        //    DialogResult dRez;

        //    if ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) == ScanVarGP.BCTyp.SP_SSCC_EXT)
        //    {// ������� SSCC
        //        switch (xCDoc.nTypOp)
        //        {
        //            case AppC.TYPOP_PRMK:
        //            case AppC.TYPOP_MARK:
        //                if (drDet != null)
        //                {
        //                    xR = new RowObj(drDet);

        //                    if (xR.IsSSCC)
        //                    {
        //                        dRez = MessageBox.Show(
        //                            String.Format("SSCC={0}\n�������� (Enter)?\n(ESC)-�������� SSCC", xR.sSSCC),
        //                            "��� ����������!", MessageBoxButtons.OKCancel,
        //                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        //                        bMaySet = (dRez == DialogResult.OK) ? false : true;
        //                    }
        //                    //if ((AppC.OPR_STATE)drDet["STATE"] == AppC.OPR_STATE.OPR_UPL)
        //                    //{// ��� ���������
        //                    //    dRez = MessageBox.Show(
        //                    //        String.Format("SSCC={0}\n�������� (Enter)?\n(ESC)-���������� SSCC", xR.SName),
        //                    //        "��� ����������!", MessageBoxButtons.OKCancel,
        //                    //        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        //                    //    bMaySet = (dRez == DialogResult.OK) ? false : true;
        //                    //}
        //                    //else
        //                    //{
        //                    //}
        //                    if (bMaySet)
        //                    {
        //                        drDet["SSCC"] = xSc.Dat;
        //                        SetOverOPR(false);
        //                        //xScanPrev = null;
        //                    }
        //                }
        //                break;
        //            case AppC.TYPOP_MOVE:
        //                // �������� �����������
        //                if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() &&
        //                    (xSm.xAdrFix1 == null))
        //                {
        //                    Srv.ErrorMsg("����� �� ������!", true);
        //                    break;
        //                }

        //                ret = ConvertSSCC2Lst(xSc, ref scD, false);
        //                if (ret == AppC.RC_OK)
        //                {
        //                    if (xCLoad.dtZ.Rows.Count == 1)
        //                    {// ���������� ������
        //                        dr = xNSI.AddDet(scD, xCDoc, null);
        //                        if (dr != null)
        //                        {
        //                            dr["SSCC"] = xSc.Dat;
        //                            xCDoc.xOper.bObjOperScanned = true;
        //                        }
        //                    }
        //                    else
        //                    {// ������� ������ ���� ����� �������
        //                        dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
        //                    }
        //                }
        //                else
        //                {// �������� ����������� SSCC �� ������� �� �������
        //                    dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
        //                    ret = AppC.RC_OK;
        //                }
        //                if (dr != null)
        //                    drDet = dr;
        //                IsOperReady(true);
        //                break;
        //            case AppC.TYPOP_DOCUM:
        //            case AppC.TYPOP_KMPL:
        //                //
        //                ret = ConvertSSCC2Lst(xSc, ref scD, true);
        //                if (ret == AppC.RC_OK)
        //                {
        //                    if (xCLoad.dtZ.Rows.Count == 1)
        //                    {// ������������ ������� ��������� �����
        //                        ret = AppC.RC_WARN;
        //                    }
        //                }
        //                else
        //                {
        //                    ret = AddGroupDet(ref scD, ret);
        //                    // � ����� ������ ��������� ����� �����������
        //                    ret = AppC.RC_OK;
        //                }
        //                break;
        //            case AppC.TYPOP_OTGR:
        //                ret = SSCC4OTG(xSc, ref scD, ScanVarGP.BCTyp.SP_SSCC_EXT);
        //                //ret = FindSSCCInZVK(xSc, ref scD);
        //                //if (ret == AppC.RC_OK)
        //                //{// ������������� ������������� � ������
        //                //    dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT);
        //                //    if (dr != null)
        //                //        drDet = dr;
        //                //}
        //                //else
        //                //{
        //                //    ret = AppC.RC_OK;
        //                //}
        //                break;
        //        }
        //    }
        //    else
        //    {// ���������� SSCC (������� ������)
        //        switch (xCDoc.nTypOp)
        //        {
        //            case AppC.TYPOP_MARK:
        //                // ����� ���������� �������� �������
        //                dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_INT, "");
        //                if (dr != null)
        //                    drDet = dr;
        //                break;
        //            case AppC.TYPOP_DOCUM:
        //            case AppC.TYPOP_KMPL:
        //                ret = ConvertSSCC2Lst(xSc, ref scD, true);
        //                if (ret == AppC.RC_OK)
        //                {
        //                    if (xCLoad.dtZ.Rows.Count == 1)
        //                    {// ������������ ������� ��������� �����
        //                        ret = AppC.RC_WARN;
        //                    }
        //                }
        //                break;
        //            case AppC.TYPOP_OTGR:
        //                ret = SSCC4OTG(xSc, ref scD, ScanVarGP.BCTyp.SP_SSCC_INT);
        //                break;
        //        }
        //        xScanPrev = xScan;
        //    }
        //    return (ret);
        //}

        // ��������� SSCC ��� �����
        int ProceedSSCC(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        {
            int 
                ret = AppC.RC_OK;
            bool
                bExt,
                bMaySet = true;
            DataRow dr = null;
            RowObj xR;
            DialogResult dRez;
            ServerExchange xSE = new ServerExchange(this);


            // ������� SSCC
            bExt = ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) == ScanVarGP.BCTyp.SP_SSCC_EXT) ? true : false;

            switch (xCDoc.nTypOp)
            {
                case AppC.TYPOP_PRMK:
                case AppC.TYPOP_MARK:
                    if (!bExt && (xCDoc.nTypOp == AppC.TYPOP_MARK))
                    {
                        // ����� ���������� �������� �������
                        dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_INT, "");
                        if (dr != null)
                            drDet = dr;
                        break;
                    }
                    if (drDet != null)
                    {
                        xR = new RowObj(drDet);
                        if (xR.IsSSCC)
                        {
                            dRez = MessageBox.Show(
                                String.Format("SSCC={0}\n�������� (Enter)?\n(ESC)-�������� SSCC", xR.sSSCC),
                                "��� ����������!", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            bMaySet = (dRez == DialogResult.OK) ? false : true;
                        }
                        if (bMaySet)
                        {
                            drDet["SSCC"] = xSc.Dat;
                            SetOverOPR(false);
                        }
                    }
                    break;
                case AppC.TYPOP_MOVE:
                    // �������� �����������
                    if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() &&
                        (xSm.xAdrFix1 == null))
                    {
                        Srv.ErrorMsg("����� �� ������!", true);
                        break;
                    }
                    ret = ConvertSSCC2Lst(xSE, xSc, ref scD, true);
                    if (ret == AppC.RC_OK)
                    {
                        if (xCLoad.dtZ.Rows.Count == 1)
                        {// ���������� ������
                            //dr = xNSI.AddDet(scD, xCDoc, null);
                            AddDet1(ref scD, out dr);
                            if (dr != null)
                            {
                                dr["SSCC"] = xSc.Dat;
                                xCDoc.xOper.bObjOperScanned = true;
                            }
                        }
                        else
                        {// ������� ������ ���� ����� �������
                            dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
                        }
                    }
                    else
                    {// �������� ����������� SSCC �� ������� �� �������
                        if ((xSE.ServerRet == AppC.EMPTY_INT) ||
                            (xSE.ServerRet == AppC.RC_OK))
                        {// �� ��� �� ������ �� �������, ��������, ������� ������
                            dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
                            ret = AppC.RC_OK;
                        }
                    }
                    if (dr != null)
                        drDet = dr;
                    IsOperReady(false);
                    break;
                case AppC.TYPOP_DOCUM:
                case AppC.TYPOP_KMPL:
                    //
                    ret = ConvertSSCC2Lst(xSE, xSc, ref scD, true);
                    if (ret == AppC.RC_OK)
                    {
                        if (xCLoad.dtZ.Rows.Count == 1)
                        {// ������������ ������� ��������� �����
                            ret = AppC.RC_WARN;
                        }
                    }
                    else
                    {
                        ret = AddGroupDet(ret);
                        // � ����� ������ ��������� ����� �����������
                        ret = AppC.RC_OK;
                    }
                    break;
                case AppC.TYPOP_OTGR:
                    ret = SSCC4OTG(xSE, xSc, ref scD, (bExt) ? ScanVarGP.BCTyp.SP_SSCC_EXT :
                        ScanVarGP.BCTyp.SP_SSCC_INT);
                    break;
            }

            return (ret);
        }

        // ���������� � ������ ��� ���������������� �������
        private DataRow AddDetSSCC(ScanVarGP xSc, int nId, ScanVarGP.BCTyp xT, string sN)
        {
            DateTime
                dtCr;
            DataRow ret = null,
                dr;
            try
            {
                dr = xNSI.DT[NSI.BD_DOUTD].dt.NewRow();

                dr["SYSN"] = nId;
                dr["KOLM"] = 1;
                dr["KOLE"] = 0;

                dtCr = DateTime.Now;
                if (xCDoc.nTypOp == AppC.TYPOP_MOVE)
                {
                    if (xCDoc.xOper.IsFillSrc())
                        dtCr = xCDoc.xOper.xAdrSrc.dtScan;
                }
                    
                dr["TIMECR"] = dtCr;
                dr["NPODD"] = int.Parse(xSc.Dat.Substring(12, 7));

                if (sN == "")
                {
                    if (xT == ScanVarGP.BCTyp.SP_SSCC_EXT)
                    {
                        dr["SSCC"] = xSc.Dat;
                        sN = "������.";
                    }
                    else
                    {
                        dr["SSCCINT"] = xSc.Dat;
                        sN = "������.";
                    }
                    sN = String.Format("{1} ������ �{0}", dr["NPODD"], sN);
                }
                dr["SNM"] = sN;

                // ��� PrimaryKey
                dr["KRKMC"] = int.Parse(xSc.Dat.Substring(2, 1));
                dr["EMK"]   = 0;
                dr["NP"]    = "";

                dr["ADRFROM"] = (xCDoc.xOper.IsFillSrc()) ? xCDoc.xOper.xAdrSrc.Addr : "";
                dr["ADRTO"] = (xCDoc.xOper.IsFillDst()) ? xCDoc.xOper.xAdrDst.Addr : "";

                xNSI.DT[NSI.BD_DOUTD].dt.Rows.Add(dr);
                AfterAddScan(this, new EventArgs());
                if (bShowTTN)
                    SetCurRow(dgDet, "ID", (int)dr["ID"]);
                ret = dr;
            }
            catch //(Exception e)
            {
                ret = null;
            }
            return (ret);
        }


        // ���������� ��������� ScDat �� ������ ������������ �����-����
        // (��������� ��� ��)
        private bool TranslSCode(ref PSC_Types.ScDat s)
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
                                    //bFind = xNSI.GetMCData(s.sKMCFull, ref s, 0);
                                    //sTypDoc.sDataIzg = sTypDoc.dDataIzg.ToString("dd.MM.yy");
                                    sS = sS.Substring(6);
                                    break;
                                case "30":                          // ���������� ���� �� �������
                                    s.nMestPal = int.Parse(sS.Substring(0, 4));
                                    s.nTypVes = AppC.TYP_PALET;
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
                            bFind = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
                            if (s.bVes == true)
                            {
                                s.nTara = "";
                                s.fVes = FRACT.Parse(sVsego) / 1000;
                                s.fEmk = 0;
                            }
                            else
                            {// ��� �������
                                s.nTara = sVsego.Substring(0, 2);
                                s.fVes = 0;
                                s.fEmk = FRACT.Parse(sVsego.Substring(2, 4));
                                s.fEmk_s = s.fEmk;
                            }


                            n = (s.tTyp == AppC.TYP_TARA.TARA_PODDON) ? s.nMestPal : (int)s.fEmk;
                            //sP = (s.sEAN.Length >= 12) ? s.sEAN.Substring(0, 12) : "";
                            if (!SetKMCOnGTIN(ref s, s.sGTIN, n, xScan))
                            {
                                bFind = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
                            }
                            ret = CompareEmk(ref s, n);
                        }
                    }
                    else
                    {
                        if (sS.Length > 13)
                        {
                            s.sGTIN = sS;
                            s.sEAN = Srv.CheckSumModul10(sS.Substring(1, 12));
                            sS = s.sEAN;
                        }

                        sIdPrim = sS.Substring(0, 1);
                        if (sIdPrim == "2")     // ������� ��������� ��� ���������� ���
                        {
                            bFind = xNSI.IsAlien(sS, ref s);
                            if (!bFind)
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
                                    s.nTypVes = AppC.TYP_VES_1ED;
                                }
                                bFind = xNSI.GetMCData("", ref s, s.nKrKMC);
                                s.bVes = true;
                            }
                            else
                            {
                                s.bAlienMC = true;
                                s.fVes = FRACT.Parse(sS.Substring(7, 5)) / 1000;
                            }
                        }
                        else
                        {
                            ret = xNSI.GetMCDataOnEAN(sS, ref s, true);
                        }
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
        private bool NewTranslSCode(ref PSC_Types.ScDat s)
        {
            string 
                sP;
            int 
                n = 0;
            bool 
                ret = true;

                try
                {
                    if (xScan.Id == ScannerAll.BCId.Code128)
                    {
                        if (xScan.dicSc.ContainsKey("01"))
                        {
                            s.sEAN = Srv.CheckSumModul10( xScan.dicSc["01"].Dat.Substring(1,12) );
                            s.sGTIN = xScan.dicSc["01"].Dat;
                            s.tTyp = AppC.TYP_TARA.TARA_TRANSP;
                        }
                        if (xScan.dicSc.ContainsKey("02"))
                        {
                            s.sEAN = Srv.CheckSumModul10( xScan.dicSc["02"].Dat.Substring(1,12) );
                            s.sGTIN = xScan.dicSc["02"].Dat;
                            s.tTyp = AppC.TYP_TARA.TARA_PODDON;
                        }

                        if (xScan.dicSc.ContainsKey("23"))
                        {
                            if (s.tTyp == AppC.TYP_TARA.TARA_PODDON)
                                s.nNomPodd = (int)(long)(xScan.dicSc["23"].xV);
                            else
                                s.nNomMesta = (int)(long)(xScan.dicSc["23"].xV);
                        }

                        if (xScan.dicSc.ContainsKey("10"))
                        {
                            s.nParty = xScan.dicSc["10"].Dat;
                            while (s.nParty.Length > 0)
                            {
                                if (s.nParty.StartsWith("0"))
                                    s.nParty = s.nParty.Substring(1);
                                else
                                    break;
                            }
                            //s.nParty = int.Parse(xScan.dicSc["10"].Dat).ToString();
                        }

                        if (xScan.dicSc.ContainsKey("11"))
                        {
                            //sP = xScan.dicSc["11"].Dat;
                            //sTypDoc.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);

                            s.dDataIzg = (DateTime)(xScan.dicSc["11"].xV);
                            s.sDataIzg = s.dDataIzg.ToString("dd.MM.yy");
                        }
                        //bL2Nsi = xNSI.GetMCData(s.sKMCFull, ref s, 0);

                        // ���������� � ����� ��� ��������
                        if (xScan.dicSc.ContainsKey("37"))
                        {
                            n = (int)(long)(xScan.dicSc["37"].xV);
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

                        if (!SetKMCOnGTIN(ref s, s.sGTIN, n, xScan))
                                xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);

                        ret = CompareEmk(ref s, n);

                        if (xScan.dicSc.ContainsKey("310"))
                        {// ������� �����
                            //s.nTara = 0;
                            s.fVes = (FRACT)(xScan.dicSc["310"].xV);
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
            if (s.tTyp == AppC.TYP_TARA.TARA_TRANSP)
            {// �� ������ ������ �������� ����� ����������� ������ �������
                if ((s.drSEMK != null) && (n > 0))
                {// ������� ���������� ������� �� �����������
                    if (s.fEmk != n)
                    {
                        string
                            sP = String.Format("������������ ��������!\n� ��������� - {0}\n� ����������� - {1}\n�������� ������������(Enter)?\n(ESC)-���������� {0}", n, s.fEmk);
                        DialogResult dr = MessageBox.Show(sP, String.Format("������������:{0} <> {1}", n, s.fEmk),
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dr == DialogResult.OK)
                            ret = false;
                        else
                            s.fEmk = s.fEmk_s = n;
                    }
                }
            }
            return (ret);
        }













        private bool TranslMT(ref PSC_Types.ScDat s)
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
        private bool TranslMTNew(ref PSC_Types.ScDat s)
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
            s.sEAN = Srv.CheckSumModul10("20" + sS.Substring(0, 10));
            bFind = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
            sS = sS.Substring(10);

            // SysN ��������� (����������)
            s.nNPredMT = int.Parse(sS.Substring(0, 9)) * (-1);
            sS = sS.Substring(9);

            // ���� ��������(������������) (������)
            sP = sS.Substring(0, 6);
            s.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);
            s.sDataIzg = sP.Substring(4, 2) + "." + sP.Substring(2, 2) + "." +
                sP.Substring(0, 2);

            sS = sS.Substring(6);

            // �������/���������� ������
            s.fVsego = Srv.Str2VarDec(sS.Substring(0, 7));
            s.fVes = s.fVsego;
            s.fEmk = s.fVes;
            s.fEmk_s = s.fEmk;
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
        private bool TrySetEmk(DataTable dtM, DataTable dtD, ref PSC_Types.ScDat sc, FRACT fVesU)
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

                DataRelation myRelation = dtM.ChildRelations["KMC2Emk"];
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

                if ((sc.drSEMK == null) || bEditMode)
                {// �� ITF/����������� �������� �� ����������
                    sc.fEmk = fEmk;
                    sc.fEmk_s = sc.fEmk;
                    sc.nTara = sTara;
                    sc.nKolSht = nSht;
                    sc.nMestPal = nEP;
                }

                if (bTryComp == true)
                {
                    if (bNot1Sht == false)
                    {// 1 �����
                        if (sc.nTypVes != AppC.TYP_VES_PAL)
                        {
                            sc.fEmk = TrySetEmkByZVK(AppC.TYP_VES_1ED, ref sc, 0);
                            if (sc.fEmk != 0)
                            {
                                sc.nTypVes = AppC.TYP_VES_TUP;
                                sc.nMest = 1;
                                fEmk = sc.fEmk;
                            }
                            else
                            {
                                sc.nTypVes = AppC.TYP_VES_1ED;
                                sc.nMest = 0;
                            }
                        }
                    }
                    else if (( (fEmk != 0) && (fDiff_Start < 40) ) ||
                        ( (fEmk == 0) && (fDiff_Start == MAXDIFF) && bNot1Pal ))

                    //else if ((fEmk != 0) && (fDiff_Start < 40)) 
                    



                    {
                        if (sc.nTypVes != AppC.TYP_PALET)
                        {
                            sc.nTypVes = AppC.TYP_VES_TUP;
                            sc.nMest = 1;
                            sc.fEmk = TrySetEmkByZVK(AppC.TYP_VES_TUP, ref sc, fEmk);
                            if (sc.fEmk != fEmk)
                            {// ���-�� ����������
                                if ((fVesU / sc.fEmk) > 1.3M)
                                {
                                    sc.nTypVes = AppC.TYP_VES_PAL;
                                    sc.nMest = -1;
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    else if (bNot1Pal == false)
                    {
                        sc.nTypVes = AppC.TYP_VES_PAL;
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

        private FRACT TrySetEmkByZVK(int nTypeVes, ref PSC_Types.ScDat sc, FRACT fCurE)
        {
            FRACT fEZ,
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
                    if (nTypeVes == AppC.TYP_VES_1ED)
                        // ��������������� ��������
                        sMsg = String.Format("��� ���� {0:N1}(Enter)?\n(ESC) - ��������������� ����", fEZ);
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

        // �������� (���������) ������� � ������ �� ����������� ��������
        //private int CheckEmk_(DataTable dtM, ref PSC_Types.ScDat sc)
        //{
        //    int
        //        nRet = AppC.RC_OK,
        //        nEmkPod_Def = 0,
        //        nEmkPod = 0;
        //    FRACT
        //        fCE,
        //        fEmk_Def = 0,
        //        fEmk = 0;

        //    if ((sc.bFindNSI == true) && (sc.drMC != null))
        //    {// ����� ������� �� ���� ��������� � �������� ���������� ����
        //        DataRelation myRelation = dtM.ChildRelations["KMC2Emk"];
        //        DataRow[] childRows = sc.drMC.GetChildRows(myRelation);
        //        if (childRows.Length == 1)
        //        {// ��������� ������, ������ ���� �������
        //            fEmk = (FRACT)childRows[0]["EMK"];
        //            nEmkPod = (int)childRows[0]["EMKPOD"];
        //        }
        //        else
        //        {
        //            foreach (DataRow chRow in childRows)
        //            {
        //                fCE = (FRACT)chRow["EMK"];
        //                nEmkPod = (int)chRow["EMKPOD"];

        //                if ((int)chRow["PR"] > 0)
        //                {// ������� �� ���������
        //                    fEmk_Def = fCE;
        //                    nEmkPod_Def = nEmkPod;
        //                }

        //                if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
        //                {
        //                    if (nEmkPod == sc.nMestPal)
        //                    {
        //                        fEmk = fCE;
        //                        if ((int)chRow["PR"] > 0)
        //                        {// ������� �� ���������
        //                            fEmk_Def = fCE;
        //                            nEmkPod_Def = (int)chRow["EMKPOD"];
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (fCE != 0)
        //                    {// ������� �������
        //                        if (fCE == sc.fEmk)
        //                        {// ������� �������
        //                            fEmk = fCE;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            if (fEmk == 0)
        //            {// ���������� ������� ����� �� �������
        //                if (fEmk_Def != 0)
        //                {
        //                    if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
        //                    {// ��������� ������ ��, ��� �� ���������
        //                        sc.fEmk_s = fEmk_Def;
        //                    }
        //                    else
        //                    {
        //                        fEmk = fEmk_Def;
        //                        nEmkPod = nEmkPod_Def;
        //                    }
        //                }
        //            }
        //        }
        //        if (fEmk != 0)
        //        {
        //            if ((sc.fEmk == 0) || (bEditMode == true))
        //            {
        //                sc.fEmk = fEmk;
        //                sc.fEmk_s = sc.fEmk;
        //            }
        //            else
        //            {
        //                if (fEmk != sc.fEmk)
        //                {
        //                    DialogResult dr = MessageBox.Show("�������� ������������(Enter)?\n(ESC)-���������� �������",
        //                        "������������ ��������",
        //                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        //                    if (dr == DialogResult.OK)
        //                        nRet = AppC.RC_CANCEL;
        //                }
        //            }
        //        }
        //    }
        //    if (sc.nMestPal == 0)
        //        sc.nMestPal = nEmkPod;
        //    return (nRet);
        //}

        public StrAndInt[] GetEmk4KMC(ref PSC_Types.ScDat sc, string sF, bool bOrigOnly, out int nDefaultEmk)
        {
            bool
                bOrigEmk;
            int
                jMax = 0;
            DataView 
                dv;
            FRACT
                fCurEmk;
            StrAndInt[]
                sa,
                siTmp;

            sa = new StrAndInt[0];
            nDefaultEmk = -1;
            if (IsTara("", sc.nKrKMC))
                return (sa);
            try
            {
                dv = new DataView(xNSI.DT[NSI.NS_SEMK].dt, sF, "EMK", DataViewRowState.CurrentRows);
                if (dv.Count > 0)
                {
                    siTmp = new StrAndInt[dv.Count];
                    for (int i = 0; i < dv.Count; i++)
                    {
                        fCurEmk = (FRACT)dv[i].Row["EMK"];
                        bOrigEmk = true;
                        if (bOrigOnly)
                        {// ��������� ������ ������������� �������
                            for (int j = 0; j < jMax; j++)
                            {
                                if (siTmp[j].DecDat == fCurEmk)
                                {
                                    bOrigEmk = false;
                                    break;
                                }
                            }
                        }
                        if (bOrigEmk)
                        {
                            //siTmp[jMax] = new StrAndInt(jMax.ToString(), (int)dv[i].Row["EMKPOD"]);
                            siTmp[jMax] = new StrAndInt(jMax.ToString(), (int)dv[i].Row["EMKPOD"],
                                (string)dv[i].Row["KTARA"], (string)dv[i].Row["GTIN"],
                                (int)dv[i].Row["KRK"], (int)dv[i].Row["PR"]);
                            siTmp[jMax].DecDat = fCurEmk;

                            if ((int)dv[i].Row["PR"] > 0)
                                nDefaultEmk = jMax;
                            jMax++;
                        }
                    }
                    sa = new StrAndInt[jMax];
                    for (int j = 0; j < jMax; j++)
                        sa[j] = siTmp[j];
                    if (nDefaultEmk < 0)
                        nDefaultEmk = 0;
                }
            }
            catch (Exception e)
            {
                jMax = e.Message.Length;
            }
            return (sa);
        }

        // �������� (���������) ������� � ������ �� ����������� ��������
        //private int CheckEmk(ref PSC_Types.ScDat sc)
        //{
        //    int
        //        nRet = AppC.RC_OK,
        //        nDefEmk = 0;
        //    string
        //        sF = "";
        //    FRACT
        //        fEmk = 0;
        //    StrAndInt[]
        //        sa = new StrAndInt[0];

        //    if ((sc.bFindNSI == true) && (sc.drMC != null))
        //    {// ����� ������� �� ���� ���������
        //        if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
        //        {//��� ��������

        //            sF = String.Format("(KMC='{0}')", sc.sKMC);
        //            if (sc.nMestPal > 0)
        //                sF = String.Format(sF + "AND(EMKPOD={0})", sc.nMestPal);

        //            sa = GetEmk4KMC(ref sc, sF, true, out nDefEmk);
        //            if (sa.Length <= 1)
        //            {
        //                if (sa.Length == 1)
        //                {
        //                    fEmk = sa[0].DecDat;
        //                }
        //                else
        //                {//����� ������� �� ����������, �������� ������������� �������
        //                    sa = GetEmk4KMC(ref sc, String.Format("(KMC='{0}')", sc.sKMC), true, out nDefEmk);
        //                    if (sa.Length == 1)
        //                    {
        //                        fEmk = sa[0].DecDat;
        //                    }
        //                }
        //            }
        //            else
        //            {//�������� ��������, �.�. ��������� ��������� ������� � ����� ����������� ���� �� �������
        //            }
        //        }
        //        else if ((sc.tTyp != AppC.TYP_TARA.TARA_POTREB) || (sc.nRecSrc == (int)NSI.SRCDET.HANDS))
        //        {// ��� ������

        //            sF = String.Format("(KMC='{0}')", sc.sKMC);
        //            if (sc.fEmk > 0)
        //                sF = String.Format(sF + "AND(EMK={0})", sc.fEmk);

        //            //sa = GetEmk4KMC(ref sc, sF, false, out nDefEmk);
        //            sa = GetEmk4KMC(ref sc, sF, true, out nDefEmk);
        //            if (sa.Length >= 1)
        //            {// ��� ������, ����� ����� ����, �������� ��� ������ �������
        //                if (sa.Length == 1)
        //                {
        //                    fEmk = sa[0].DecDat;
        //                }
        //            }
        //            else
        //            {
        //                if (sc.nRecSrc == (int)NSI.SRCDET.SCAN)
        //                {
        //                    sF = String.Format("��� �={0}", sc.fEmk);
        //                    DialogResult dr = MessageBox.Show("�������� ������������(Enter)?\n(ESC)-���������� �������", sF,
        //                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        //                    if (dr == DialogResult.OK)
        //                        nRet = AppC.RC_CANCEL;
        //                    else
        //                        fEmk = sc.fEmk;
        //                }
        //            }
        //            if (nRet == AppC.RC_OK)
        //                sc.fEmk_s = sc.fEmk;
        //        }
        //        if (fEmk > 0)
        //        {
        //            sc.fEmk = fEmk;
        //            sc.fEmk_s = fEmk;
        //            if ((sc.tTyp == AppC.TYP_TARA.TARA_PODDON) && (sc.nMestPal == 0))
        //            {
        //                if (sa.Length == 1)
        //                    sc.nMestPal = sa[0].IntCode;
        //                else
        //                {
        //                    if ((sa.Length > 1) && (nDefEmk < sa.Length))
        //                        sc.nMestPal = sa[nDefEmk].IntCode;
        //                }
        //            }
        //        }
        //    }
        //    if (sa.Length > 1)
        //        sc.xEmks = new Srv.Collect4Show<StrAndInt>(sa);
        //    return (nRet);
        //}









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
        private bool SetKMCOnGTIN(ref PSC_Types.ScDat sc, string sEAN12, int nEFromBC, ScanVarGP xSc)
        {
            bool
                ret = AppC.RC_CANCELB;
            string
                sF = "";
            DataView 
                dvMC,
                dv;

            if (sc.sGTIN.Length > 0)
            {
                sF = String.Format("(GTIN LIKE '%{0}%')", sEAN12);
                dv = new DataView(xNSI.DT[NSI.NS_SEMK].dt, sF, "EMK", DataViewRowState.CurrentRows);
                if (dv.Count > 0)
                {// ������� ������������ 
                    if (dv.Count > 1)
                    {// � �� ���������
                        if (nEFromBC > 0)
                        {// ������� � ���� �������
                            if (sc.tTyp == AppC.TYP_TARA.TARA_TRANSP)
                            {
                                if (xSc.dicSc.ContainsKey("310"))
                                {// ������� �����
                                    sF += String.Format("AND(KRK={0})", nEFromBC);
                                }
                                else
                                {// ������� �����
                                    sF += String.Format("AND(EMK={0})", nEFromBC);
                                }
                            }
                            else if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                            {
                                //if (int.Parse(sc.sGTIN.Substring(0, 1)) > 0)
                                //{
                                //}
                                sF = String.Format("(GTIN='{0}')AND(EMK>0)", sc.sGTIN);
                            }
                        }
                        else
                        {
                            sF = String.Format("(GTIN='{0}')AND(EMK>0)", sc.sGTIN);
                        }

                        dv = new DataView(xNSI.DT[NSI.NS_SEMK].dt, sF, "EMK", DataViewRowState.CurrentRows);
                    }
                    if (dv.Count > 0)
                    {
                        sc.drSEMK = dv[0].Row;
                        sc.fEmk = (FRACT)sc.drSEMK["EMK"];
                        if ((sc.tTyp == AppC.TYP_TARA.TARA_PODDON) && (sc.nMestPal > 0))
                        { }
                        else
                            sc.nMestPal = (int)sc.drSEMK["EMKPOD"];
                        //sc.nTara = (sc.drSEMK["KT"] is string) ? (string)sc.drSEMK["KT"] : "";
                        sc.nTara = (sc.drSEMK["KTARA"] is string) ? (string)sc.drSEMK["KTARA"] : "";
                        sc.nKolSht = (sc.drSEMK["KRK"] is int) ? (int)sc.drSEMK["KRK"] : 0;
                        sc.sKMC = (sc.drSEMK["KMC"] is string) ? (string)sc.drSEMK["KMC"] : "";
                        sc.bEmkByITF = true;
                        //dvMC = new DataView(xNSI.DT[NSI.NS_MC].dt, String.Format("(KMC='{0}')", sc.drSEMK["KMC"]), "", DataViewRowState.CurrentRows);
                        //ret = sc.GetFromNSI(sc.s, dvMC[0].Row, xNSI.DT[NSI.NS_MC].dt);
                        ret = sc.GetFromNSI(sc.s, xNSI.DT[NSI.NS_MC].dt.Rows.Find(new object[] { sc.sKMC }), xNSI.DT[NSI.NS_MC].dt);
                        if (ret)
                        {
                            int nL = 0;
                            sc.xEmks = new Srv.Collect4Show<StrAndInt>(GetEmk4KMC(ref sc, String.Format("(KMC='{0}')", sc.sKMC), true, out nL));
                        }
                    }
                }
            }

            return (ret);
        }



    }
}
