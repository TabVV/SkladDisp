using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;

using SavuSocket;
using ScannerAll;
using PDA.OS;
using PDA.Service;
using PDA.BarCode;

using FRACT = System.Decimal;


namespace SkladGP
{
    public partial class MainF : Form
    {
        // ���� ������� ����� � ���������
        private bool b1stEner = true;

        // ������� ������� ������ � �����������
        private int nCurDocFunc = AppC.DT_SHOW;

        // ��� ��������� ������ ����������
        private void EnterInDoc(){
            //FRACT f;

            if (b1stEner == true)
            {
                if (!Only1st())
                    return;
            }
            if (nCurDocFunc == AppC.DT_SHOW)            // � ������ ���������
            {
                StatAllDoc();
                if (xCDoc.drCurRow != null)
                {// ���� ������ ��� �����������
                    //xCDoc.drCurRow["MEST"] = TotMest(NSI.REL2TTN, out f);
                    xCDoc.drCurRow["MEST"] = TotMest(NSI.REL2TTN, null);
                }
                dgDoc.Focus();
            }
        }


        // ������ 1-� ��� ���������� ���������
        private bool Only1st()
        {
            bool
                bContinueApp = true;
            string s;
            DialogResult 
                xDRslt;

            b1stEner = false;
            xNSI.ChgGridStyle(NSI.BD_DOCOUT, NSI.GDOC_INV);
            s = (xSm.sUser == AppC.SUSER)?"Admin":
                (xSm.sUser == AppC.GUEST) ? "�����" : xSm.sUser;
            lInfDocLeft.Text = s;

            xCDoc = new CurDoc(xSm, AppC.DT_ADDNEW);
            //DataView dvMaster = ((DataTable)dgDoc.DataSource).DefaultView;
            //if (dvMaster.Count > 0)
                
            if (xSm.nDocs > 0)
            {// ���� ������ ��� ���������
                RestShowDoc(false);
            }
            if (xSm.RegApp == AppC.REG_MARK)
            {
                //CheckNSIState(false);
                //DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Mark.dll", true);
                xDRslt = CallDllForm(sExeDir + "SGPF-Mark.dll", true, new object[] { this, false });
                if (xDRslt == DialogResult.Abort)
                {
                    bContinueApp = false;
                    this.Close();
                }
            }
            return (bContinueApp);
        }

        // ����� ���������� �� ���� ����������
        private void StatAllDoc()
        {
            // ����� ����������
            int nR = xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Count;

            DataView dvNotUpl = new DataView(xNSI.DT[NSI.BD_DOCOUT].dt,
                                String.Format("(SOURCE<>{0})", NSI.DOCSRC_UPLD), "",
                                DataViewRowState.CurrentRows);

            // ���������� ��� ������
            int nDocNotUpl = dvNotUpl.Count;

            // ��������� ����� ����� (������)
            int nDetIn = xNSI.DT[NSI.BD_DIND].dt.Rows.Count;

            // ��������� ����� ����� (�������/��������������)
            int nDetOut = xNSI.DT[NSI.BD_DOUTD].dt.Rows.Count;

            lInfDocAll.Text = "��� " + nR.ToString() + 
                "(" + nDetIn.ToString() + "/" + nDetOut.ToString() + ")";
            lInfDocRight.Text = "�� " + nDocNotUpl.ToString();
            //tStat_GSt.Text = (xSm.RegApp == AppC.REG_DOC)?"���":"���";
            tStat_GSt.Text = "���";
        }


        // ��������� ������������ �� ������ ����������
        private void ProceedScanDoc(ScanVarGP xSc, ref PSC_Types.ScDat s)
        {
            int
                nRet = AppC.RC_OK;
            string 
                sH,
                sPar,
                sErr = "";
            CurLoad 
                ret;
            ServerExchange 
                xSE = new ServerExchange(this);


            if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC_INT) > 0)
            {
                xCLoad = new CurLoad(AppC.UPL_ALL);
                xCLoad.sSSCC = xSc.Dat;
                xCLoad.drPars4Load = xNSI.DT[NSI.BD_KMPL].dt.NewRow();
                LoadKomplLst(xCLoad.drPars4Load, AppC.F_LOADKPL);
            }
            else
            {
                ret = TransDocCode(ref s, xSE);
                if (ret != null)
                {
                    xCLoad = ret;

                    if (xSE.FullCOM2Srv.Length == 0)
                    {
                        if (bInLoad == true)
                        {
                            W32.keybd_event(W32.VK_ESC, W32.VK_ESC, 0, 0);
                            W32.keybd_event(W32.VK_ESC, W32.VK_ESC, 0, 0);
                            W32.keybd_event(W32.VK_ESC, W32.VK_ESC, W32.KEYEVENTF_KEYUP, 0);
                            W32.keybd_event(W32.VK_ESC, W32.VK_ESC, W32.KEYEVENTF_KEYUP, 0);
                        }
                        else
                        {
                            LoadDocFromServer(AppC.F_INITRUN, new KeyEventArgs(Keys.Enter), ref ehCurrFunc);
                        }
                    }
                    else
                    {
                        AutoSaveDat();
                        LoadFromSrv dgL = new LoadFromSrv(DocFromSrv);
                        xCLoad.nCommand = AppC.F_LOAD_DOC;
                        xCLoad.sComLoad = AppC.COM_ZZVK;
                        sErr = xSE.ExchgSrv(AppC.COM_ZZVK, "", "", dgL, null, ref nRet);

                        MessageBox.Show("�������� �������� - " + sErr, "��� - " + nRet.ToString());
                        PosOnLoaded(nRet);
                        Back2Main();
                    }
                }
                else
                {
                    xCUpLoad = new CurUpLoad();

                    if (xSE.FullCOM2Srv.Length == 0)
                    {
                        sPar = String.Format("BC={0};BCT={1}", xSc.Dat, xSc.Id.ToString().ToUpper());
                        sErr = xSE.ExchgSrv(AppC.COM_UNKBC, sPar, "", null, null, ref nRet);
                    }
                    else
                    {
                        AutoSaveDat();
                        LoadFromSrv dgL = new LoadFromSrv(DocFromSrv);
                        xCLoad.nCommand = AppC.F_LOAD_DOC;
                        xCLoad.sComLoad = AppC.COM_ZZVK;
                        sErr = xSE.ExchgSrv(AppC.COM_ZZVK, "", "", dgL, null, ref nRet);

                        MessageBox.Show("�������� �������� - " + sErr, "��� - " + nRet.ToString());
                        PosOnLoaded(nRet);
                        Back2Main();
                    }

                    if (nRet != AppC.RC_OK)
                    {
                        nRet = xSE.ServerRet;
                        if (nRet != AppC.RC_NEEDPARS)
                        {
                        }
                        Srv.PlayMelody(W32.MB_4HIGH_FLY);
                        sH = "������!";
                    }
                    else
                    {
                        sH = "��� ���������";
                    }
                    Srv.ErrorMsg(sErr, sH, false);
                }
            }
        }



        // ��������� ������������ �� ������ ����������
        private CurLoad TransDocCode(ref PSC_Types.ScDat s, ServerExchange xSE)
        {
            string 
                sEks, sPol;
            bool 
                ret = false;
            CurLoad 
                xL = null;

            if (xScrDoc.CurReg == 0)
            {
                string sS = s.s;
                int i, nLen;
                xL = new CurLoad(AppC.UPL_FLT, Doc4Chk);

                ret = true;
                try
                {
                    if (s.ci == ScannerAll.BCId.Code128)
                    {
                        nLen = sS.Length;
                        switch (nLen)
                        {
                            case 14:                            // � ���������
                                ret = false;
                                if (sS.Substring(0, 3) == "821")
                                {// ��� ����� - 821...
                                    i = int.Parse(sS.Substring(7, 7));
                                    if (i > 0)
                                    {
                                        xL.xLP.sNomDoc = i.ToString();
                                        ret = true;
                                    }
                                    else
                                        xL.xLP.sNomDoc = "";
                                }
                                xL.xLP.nTypD = AppC.TYPD_SAM;
                                break;
                            case 26:                            // �������� ��� ���������
                                if (sS.Substring(0, 2) == "91")
                                {
                                    xSE.FullCOM2Srv = String.Format("COM={0};KSK={1};MAC={2};KP={3};BC={4};",
                                        AppC.COM_ZZVK,
                                        xSm.nSklad,
                                        xSm.MACAdr,
                                        xSm.sUser,
                                        sS
                                        );
                                }
                                break;
                            case 36:                            // �������� �� �������� � �.�.
                                if (sS.Substring(0, 2) == "50")
                                {
                                    // ��������� ���� �� ��������� (����� - 34)
                                    // ��� ������ - 50 (2)
                                    // ���� - ������ (6)
                                    // ����� - (4)
                                    // ����� - (3)
                                    // ������� - (3)
                                    // ���������� - (4)
                                    // ���������� - (4)
                                    // ��� ��������� - (2)
                                    // � ��������� - (8)
                                    sS = sS.Substring(2);
                                    xL.xLP.dDatDoc = DateTime.ParseExact(sS.Substring(0, 6), "yyMMdd", null);
                                    // ����� �����
                                    i = int.Parse(sS.Substring(6, 1));
                                    if (i > 0)
                                        xL.xLP.sSmena = sS.Substring(7, i);
                                    else
                                        xL.xLP.sSmena = "";

                                    i = int.Parse(sS.Substring(10, 3));
                                    if (i > 0)
                                        xL.xLP.nSklad = i;
                                    else
                                        xL.xLP.nSklad = AppC.EMPTY_INT;

                                    i = int.Parse(sS.Substring(13, 3));
                                    if (i > 0)
                                        xL.xLP.nUch = i;
                                    else
                                        xL.xLP.nUch = AppC.EMPTY_INT;

                                    sEks = sS.Substring(16, 4);
                                    i = int.Parse(sS.Substring(16, 4));
                                    if (i > 0)
                                        xL.xLP.nEks = i;
                                    else
                                        xL.xLP.nEks = AppC.EMPTY_INT;

                                    sPol = sS.Substring(20, 4);
                                    i = int.Parse(sS.Substring(20, 4));
                                    if (i > 0)
                                        xL.xLP.nPol = i;
                                    else
                                        xL.xLP.nPol = AppC.EMPTY_INT;

                                    i = int.Parse(sS.Substring(24, 2));
                                    if (i >= 0)
                                        xL.xLP.nTypD = i;
                                    else
                                        xL.xLP.nTypD = AppC.EMPTY_INT;

                                    if ((xL.xLP.nTypD == 0)||(xL.xLP.nTypD == 1))
                                    {// ��� ���������� ����������� ��� (17.07.18 - � ��� ������������ ����)
                                        xL.xLP.nEks = AppC.EMPTY_INT;
                                        i = int.Parse(sEks + sPol);
                                        if (i > 0)
                                            xL.xLP.nPol = i;
                                        else
                                            xL.xLP.nPol = AppC.EMPTY_INT;
                                    }

                                    i = int.Parse(sS.Substring(26, 8));
                                    if (i > 0)
                                    {
                                        xL.xLP.sNomDoc = i.ToString();
                                        ret = true;
                                    }
                                    else
                                        xL.xLP.sNomDoc = "";
                                }
                                else
                                    ret = false;
                                break;
                            default:
                                ret = false;
                                break;
                        }
                    }
                }
                catch
                {
                    ret = false;
                }

            }
            if (ret == false)
            {
                xL = null;
            }
            return (xL);
        }



        // ��������� ������� � ������ �� ������
        private bool Doc_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;
            string sMsg;

            if (nFunc > 0)
            {
                if (bEditMode == false)
                {
                    switch (nFunc)
                    {
                        case AppC.F_ADD_REC:            // ���������� �����
                            AddOrChangeDoc(AppC.F_ADD_REC);
                            ret = true;
                            break;
                        case AppC.F_CHG_REC:            // �������������
                            if (xCDoc.drCurRow != null)
                            {

                                AddOrChangeDoc(AppC.F_CHG_REC);
                            }
                            ret = true;
                            break;
                        case AppC.F_DEL_ALLREC:         // �������� ����
                        case AppC.F_DEL_REC:            // ��� ������
                            DelDoc(nFunc);
                            StatAllDoc();
                            ret = true;
                            break;
                        case AppC.F_TOT_MEST:
                            // ����� ���� �� ���������/������
                            ShowTotMest();
                            ret = true;
                            break;
                        case AppC.F_CTRLDOC:
                            // �������� �������� ���������
                            ControlDocs(AppC.F_INITREG, null, ref ehCurrFunc);
                            ret = true;
                            break;
                        case AppC.F_GOFIRST:
                        case AppC.F_GOLAST:
                            //CurrencyManager cmDoc = (CurrencyManager)BindingContext[dgDoc.DataSource];
                            //if (cmDoc.Count > 0)
                            //    cmDoc.Position = (nFunc == AppC.F_GOFIRST) ? 0 : cmDoc.Count - 1;
                            Go1stLast(dgDoc, nFunc);
                            ret = true;
                            break;
                        //case AppC.F_CHGSCR:
                        //    // ����� ������
                        //    xScrDoc.NextReg(AppC.REG_SWITCH.SW_NEXT);
                        //    ret = true;
                        //    break;
                        case AppC.F_FLTVYP:
                            // ��������� ������� �� �����������
                            xPars.bHideUploaded = !xPars.bHideUploaded;
                            FiltForDocs(xPars.bHideUploaded, xNSI.DT[NSI.BD_DOCOUT]);
                            ret = true;
                            break;
                        //case AppC.F_CHG_GSTYLE:
                        case AppC.F_LOADKPL:
                            xCLoad = new CurLoad(AppC.UPL_FLT);
                            if (LoadKomplLst(null, AppC.F_LOADKPL))
                            {
                                xCLoad.drPars4Load = null;
                                xDLLPars = AppC.F_LOADKPL;
                                DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Kompl.dll", false);
                                if (xCLoad.drPars4Load != null)
                                {
                                    xCLoad.sSSCC = "";
                                    LoadKomplLst(xCLoad.drPars4Load, AppC.F_LOADKPL);
                                }
                            }
                            ret = true;
                            break;
                        case AppC.F_LOADOTG:
                            xCLoad = new CurLoad(AppC.UPL_FLT);
                            if (LoadKomplLst(null, AppC.F_LOADOTG))
                            {
                                xCLoad.drPars4Load = null;
                                xDLLPars = AppC.F_LOADOTG;
                                DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Kompl.dll", false);

                                if (xCLoad.drPars4Load != null)
                                {
                                    LoadKomplLst(xCLoad.drPars4Load, AppC.F_LOADOTG);
                                }
                            }
                            ret = true;
                            break;

                        case AppC.F_LOAD4CHK:
                            Doc4Chk = true;
                            Srv.PlayMelody(W32.MB_1MIDDL_HAND);
                            Srv.ErrorMsg("�������� ��� ��������!", "����� ��������", false);
                            ret = true;
                            break;


                        case AppC.F_TMPMOV:
                            SetTempMove();
                            ret = true;
                            break;

                        case AppC.F_CHG_LIST:
                            xNSI.ChgGridStyle(NSI.BD_DOCOUT, ((xNSI.DT[NSI.BD_DOCOUT].nGrdStyle == NSI.GDOC_VNT) ? NSI.GDOC_INV : NSI.GDOC_VNT));
                            ret = true;
                            break;

                    }
                }
            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_ENTER:
                        if (nCurDocFunc == AppC.DT_SHOW)
                        {
                            if (xCDoc.drCurRow != null)
                            {
                                tcMain.SelectedIndex = PG_SCAN;
                                ret = true;
                            }
                        }
                            break;
                }
            }
            e.Handled |= ret;
            return (ret);

        }

        // ��������� �� ������ �������� ����, ������������ � �����������
        private AppC.VALNSI IsControlByNSI(int nDocType, string sVal, string sNSIName)
        {
            AppC.VALNSI
                bRet = AppC.VALNSI.NO_NSI;
            if (xNSI.DT.ContainsKey(sNSIName))
            {
                if (xNSI.DT[sNSIName].dt.Rows.Count > 0)
                {
                    bRet = AppC.VALNSI.ANY_AVAIL;
                    switch (nDocType)
                    {
                        case AppC.TYPD_SAM:
                            switch (sNSIName)
                            {
                                case NSI.NS_EKS:
                                    break;
                            }
                            break;
                    }
                }
            }
            return (bRet);
        }

        // �������� ���������� ����� �������
        private bool VerifyPars(DocPars xP, int nF, ref object xErr)
        {
            bool ret = false;
            string sE = "";

            // ��� ���� ����� ����������
            if ((xP.nTypD != AppC.EMPTY_INT) && (xP.dDatDoc != DateTime.MinValue))
            {// ������� ������
                if (xP.nSklad != AppC.EMPTY_INT)
                {// ����� �������

                    switch(nF)
                    {
                        case AppC.F_ADD_REC:
                        case AppC.F_CHG_REC:
                            if (Math.Abs((Smena.DateDef - xP.dDatDoc).Days) > xPars.Days2Save)
                            {
                                DialogResult drQ = MessageBox.Show("�������� (Enter)?\n(ESC) - ��������",
                                    "��������� ����!",
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                if (drQ == DialogResult.OK)
                                {
                                    ret = false;
                                    sE = "�������� ���� ��� ��������� �����";
                                    xErr = DocPars.tDate;
                                    break;
                                }
                            }
                            if (xP.nTypD != AppC.TYPD_INV)
                            {// ���� �� �������������� - ��������� ������

                                switch (xP.nTypD)
                                {
                                    case AppC.TYPD_BRK:         // ��� ����� ����������
                                        if (xP.sNomDoc.Length > 0)
                                            ret = true;
                                        else
                                        {
                                            sE = "����� ���� �� ������!";
                                            xErr = tNom_p;
                                        }
                                        break;
                                    case AppC.TYPD_OPR:        // ��������������� �����������
                                        //if (xP.nPol != AppC.EMPTY_INT)
                                        //    ret = true;
                                        //else
                                        //{
                                        //    sE = "��� ����������� �� ������!";
                                        //    xErr = DocPars.tKPol;
                                        //    ret = true;
                                        //}
                                        if (xP.TypOper > 0)
                                            ret = true;
                                        else
                                        {
                                            sE = "��� ����������� �� ������!";
                                            xErr = DocPars.tKPol;
                                            ret = true;
                                        }

                                        break;
                                    case AppC.TYPD_SVOD:        // ��� ������
                                        if (xP.nEks != AppC.EMPTY_INT)
                                            ret = true;
                                        else
                                        {
                                            sE = "���������� �� ������!";
                                            xErr = DocPars.tKEks;
                                        }
                                        break;
                                    case AppC.TYPD_CVYV:        // �����������
                                        //if ((xP.nEks != AppC.EMPTY_INT) && (xP.nPol != AppC.EMPTY_INT)) 
                                        //    ret = true;
                                        //else
                                        //{
                                        //    sE = "���������� ��� ���������� �� ������!";
                                        //    xErr = DocPars.tKEks;
                                        //}
                                        if (xP.nEks != AppC.EMPTY_INT)
                                        {
                                            ret = true;
                                            if (xP.nPol == AppC.EMPTY_INT)
                                                sE = "���������� �� ������!";
                                        }
                                        else
                                        {
                                            sE = "���������� �� ������!";
                                            xErr = DocPars.tKEks;
                                        }
                                        break;
                                    case AppC.TYPD_SAM:         // ���������
                                    case AppC.TYPD_VPER:        // ���������� �����������
                                        if (xP.nPol != AppC.EMPTY_INT)
                                        {
                                            if (xP.nTypD == AppC.TYPD_SAM)
                                                ret = true;
                                            else
                                            {// ���������� �����������
                                                if (xP.sNomDoc != "")
                                                    ret = true;
                                                else
                                                {
                                                    sE = "� ��������� �� ������!";
                                                    xErr = tNom_p;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ret = true;
                                            sE = "���������� �� ������!";
                                        }
                                        break;
                                    case AppC.TYPD_PRIH:         // ��������� �����
                                        ret = true;
                                        if (xP.sSmena == "")
                                        {
                                            if (IsControlByNSI(xP.nTypD, "", NSI.NS_SMEN) > AppC.VALNSI.ANY_AVAIL)
                                            {
                                                ret = false;
                                                sE = "����� �� �������!";
                                                xErr = tSm_p;
                                            }
                                        }
                                        else if (xP.sNomDoc == "")
                                        {
                                            ret = false;
                                            sE = "� ��������� �� ������!";
                                            xErr = tNom_p;
                                        }
                                        break;
                                    default:
                                        ret = true;
                                        break;
                                }
                            }
                            else
                            {//��� ��������������
                                ret = true;
                                if (xP.sSmena == "")
                                {
                                    if (IsControlByNSI(xP.nTypD, "", NSI.NS_SMEN) > AppC.VALNSI.ANY_AVAIL)
                                    {
                                        sE = "����� �� �������!";
                                        xErr = tSm_p;
                                    }
                                }
                            }
                            break;
                        case AppC.F_UPLD_DOC:
                        case AppC.F_LOAD_DOC:
                            if (xP.sSmena != "")
                                ret = true;
                            else
                            {
                                if (IsControlByNSI(xP.nTypD, "", NSI.NS_SMEN) > AppC.VALNSI.ANY_AVAIL)
                                {// ������ - ������!
                                    if (xP.nTypD == AppC.TYPD_VPER)
                                    {// ���������� �����������
                                        if (xP.sNomDoc.Length > 0)
                                        {
                                            ret = true;
                                            break;
                                        }
                                    }
                                    sE = "����� �� �������!";
                                    xErr = tSm_p;
                                }
                                else
                                    ret = true;
                            }
                            break;
                }
                }
                else
                {
                    sE = "����� �� ������!";
                    //xErr = DocPars.tKSkl;
                    xErr = tKSkl_p;
                }
            }
            else
            {
                sE = "��������� ��� ��� ����!";
                xErr = DocPars.tDate;
            }
            if ((ret == false) || (sE.Length > 0))
            {
                Srv.ErrorMsg(sE);
            }
            return (ret);
        }


        // ������� � ����� ���������
        private void RestShowDoc(bool bGoodBefore)
        {
            tStat_Reg.Text = "��������";
            nCurDocFunc = AppC.DT_SHOW;
            if (bGoodBefore == false)
            {// ���������� �������� ���������, ���������� ������ (���� ����)
                DataView dvMaster = ((DataTable)dgDoc.DataSource).DefaultView;

                if (dvMaster.Count > 0)
                {// ���� ������ ��� ���������
                    xCDoc.drCurRow = dvMaster[dgDoc.CurrentRowIndex].Row;
                    xNSI.InitCurDoc(xCDoc, xSm);
                }
                else
                    xCDoc.xDocP = new DocPars(AppC.DT_ADDNEW);
                SetParFields(xCDoc.xDocP);
            }
            dgDoc.Focus();
        }



        /// *** ������� ������ � �����������
        /// 

        // ���������� ����� ��� ��������� ������
        // nReg - ��������� �����
        private void AddOrChangeDoc(int nFunc)
        {
            CTRL1ST 
                FirstC = CTRL1ST.START_EMPTY;

            if (nFunc == AppC.F_ADD_REC)
            {// ���� � ����� ���������� ����� ������
                xCDoc = new CurDoc(xSm, AppC.DT_ADDNEW);
                tStat_Reg.Text = "�����";
                //if (xSm.RegApp == AppC.REG_DOC)
                //    FirstC = CTRL1ST.START_EMPTY;
            }
            else
            {// ���� � ����� ������������� ������
                tStat_Reg.Text = "����-��";
            }
            EditPars(nFunc, xCDoc.xDocP, FirstC, VerifyDoc, EditFieldsIsOver);
        }

        // �������� ��������� ��������
        private AppC.VerRet VerifyDoc()
        {
            AppC.VerRet v;
            v.nRet = AppC.RC_OK;
            object xErr = null;
            bool bRet = VerifyPars(xCDoc.xDocP, nCurFunc, ref xErr);
            if (bRet != true)
                v.nRet = AppC.RC_CANCEL;
            //else
            //{
            //    //bQuitEdPars = true;
            //    if (xCDoc.xDocP.nTypD == AppC.TYPD_OPR)
            //    {
            //        xCDoc.nTypOp = xCDoc.xDocP.nPol;
            //    }

            //}
            v.cWhereFocus = (Control)xErr;
            return (v);
        }

        private void EditFieldsIsOver(int RC, int nF)
        {
            bool bRet = false;          // ���������� ������
            if (RC == AppC.RC_OK)
            {
                switch (nF)
                {
                    case AppC.F_ADD_REC:
                        bRet = xNSI.AddDocRec(xCDoc);
                        // ���� ����������� �� �����, ���� ���-�� ������ �� ���
                        //CurrencyManager cmDoc = (CurrencyManager)BindingContext[dgDoc.DataSource];
                        //cmDoc.Position = cmDoc.Count - 1;

                        xSm.DocType = xCDoc.xDocP.nTypD;

                        SetCurRow(dgDoc, "SYSN", xCDoc.nId);
                        break;
                    case AppC.F_CHG_REC:
                        bRet = xNSI.UpdateDocRec(xCDoc.drCurRow, xCDoc);
                        break;
                }
            }
            RestShowDoc(bRet);
        }


        // �������� ��������� (��)
        private void DelDoc(int nReg)
        {
            if (xCDoc.drCurRow != null)
            {
                if (nReg == AppC.F_DEL_REC)
                {// �������� ���������
                    xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Remove( xCDoc.drCurRow );

                    DataView dvMaster = ((DataTable)dgDoc.DataSource).DefaultView;
                    if (dvMaster.Count > 0)
                        xCDoc.drCurRow = dvMaster[dgDoc.CurrentRowIndex].Row;
                    else
                        xCDoc.drCurRow = null;
                }
                else
                {
                    DialogResult dr = MessageBox.Show("�������� �������� ���� (Enter)?\n(ESC) - ��� ������� ��� ��������",
                        "��������� ��� ������!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (dr != DialogResult.OK)
                    {
                        xNSI.DT[NSI.BD_SSCC].dt.Rows.Clear();
                        xNSI.DT[NSI.BD_SPMC].dt.Rows.Clear();
                        xNSI.DT[NSI.BD_DIND].dt.Rows.Clear();
                        xNSI.DT[NSI.BD_DOUTD].dt.Rows.Clear();
                        xNSI.DT[NSI.BD_PICT].dt.Rows.Clear();
                        xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Clear();
                        xCDoc.drCurRow = null;
                    }
                }
                RestShowDoc(false);

                //if (xCDoc.drCurRow != null)
                //{
                //    RestShowDoc(false);
                //}
                //else
                //{// ��-��������, ������� ���������, ���� �� ������
                //    //AddOrChangeDoc(AppC.DT_ADDNEW);
                //}
            }
        }

        private void ControlAllDoc(List<string> lstProt)
        {
            DataView dvD = ((DataTable)dgDoc.DataSource).DefaultView;
            for (int i = 0; i < dvD.Count; i++)
            {
                ControlDocZVK(dvD[i].Row, lstProt);
            }
        }


        // ����� ����� Grid-���������
        //private void ChgDocGridStyle(int nReg)
        //{
        //    //MessageBox.Show("Changing...");
        //    xNSI.ChgGridStyle(NSI.NS_DOCOUT, NSI.GDOC_NEXT);
        //}


        // ���������� ����� ������
        private void dgDoc_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            DataView dvMaster = ((DataTable)dg.DataSource).DefaultView;
            DataRow dr = dvMaster[dg.CurrentRowIndex].Row;
            if (xCDoc.drCurRow != dr)
            {// ��������� ������
                xCDoc = new CurDoc(xSm);
                xCDoc.drCurRow = dr;
                xNSI.InitCurDoc(xCDoc, xSm);
                SetParFields(xCDoc.xDocP);
                if ((xSm.FilterTTN & NSI.FILTRDET.SSCC) > 0)
                {
                    xSm.FilterTTN = NSI.FILTRDET.UNFILTERED;
                    xNSI.DT[NSI.BD_DOUTD].sTFilt = "";
                }
                //if ((int)dr["TYPOP"] == AppC.TYPOP_KMPL)
                //{
                //    xNSI.ChgGridStyle(NSI.BD_DIND, 1);
                //}
                //else
                //{
                //    xNSI.ChgGridStyle(NSI.BD_DIND, 0);
                //}
            }
        }

        //public void FillPoddonlLst_(int nId)
        //{
        //    string 
        //        sRf = DefDetFilter();
        //    DataTable dt = xNSI.DT[NSI.BD_DIND].dt,
        //        dtD = xNSI.DT[NSI.BD_DOUTD].dt;

        //    // ������ ������� �������� �� ������
        //    DataView dv = new DataView(dt, sRf, "", DataViewRowState.CurrentRows);
        //    DataTable dtN = dv.ToTable(true, "NPODDZ");

        //    // ������ ������� �������� �� ���������
        //    DataView dv1 = new DataView(dtD, sRf, "", DataViewRowState.CurrentRows);
        //    DataTable dtN1 = dv.ToTable(true, "NPODDZ");

        //    //xCDoc.lstNomsFromZkz = new List<int>();
        //    //xCDoc.lstNomsFromZkz.Clear();

        //    DataTable ddtt = (dtN1.Rows.Count > dtN.Rows.Count) ? dtN1 : dtN;
        //    //xCDoc.sLstNoms = "";
        //    xCDoc.xNPs.Clear();
        //    foreach (DataRow dr in ddtt.Rows)
        //    {
        //        //xCDoc.lstNomsFromZkz.Add((int)dr["NPODDZ"]);
        //        xCDoc.xNPs.Add((int)dr["NPODDZ"], new PoddonInfo());
        //    }


        //}

        public void SetSSCCForPoddon(string sSSCC, DataView dv, int nP)
        {
            string
                sF;
            sF = (sSSCC.Substring(2, 1) == "1") ? "SSCC" : "SSCCINT";
            foreach (DataRowView drv in dv)
            {
                (drv.Row[sF]) = sSSCC;
                (drv.Row["SSCC"]) = sSSCC;
            }
            //MessageBox.Show(String.Format("������ {0} ����������� ({1}) �������", nP, dv.Count));

            //xCDoc.xNPs.TryNextPoddon(true);
            tCurrPoddon.Text = xCDoc.xNPs.Current.ToString();
        }

        public bool StoreSSCC(ScanVarGP xSc, int nPoddonN, bool bNeedWrite, out DataView dv)
        {
            int n = 0;
            string 
                s, sRf,
                sF,
                sD = xSc.Dat;
            bool 
                //bIsExt,
                bRet = AppC.RC_CANCELB;
            DataView dvZ;
            DialogResult dRez;

            if (sD.Substring(2, 1) == "1")
            {
                //bIsExt = true;
                sF = "SSCC";
            }
            else
            {
                //bIsExt = false;
                sF = "SSCCINT";
            }

            /// 14.02.18
            sF = "SSCC";

            dv = null;
            if (xCDoc.drCurRow == null)
            {
                return (bRet);
            }
            try
            {
                if ((int)xCDoc.drCurRow["TYPOP"] == AppC.TYPOP_KMPL)
                {
                    if (nPoddonN > 0)
                    {
                        //string sRf = xCDoc.DefDetFilter() + String.Format(" AND (SSCC='{0}')", sSSCC);
                        //dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "", DataViewRowState.CurrentRows);
                        //n = dv.Count;
                        if ( IsUsedSSCC(sD) )
                        {
                            dRez = MessageBox.Show(
                                String.Format("SSCC={0}\n�������� (Enter)?\n(ESC)-���������� SSCC", sD),
                                "��� �������������!", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            n = (dRez == DialogResult.OK) ? 1 : 0;
                        }
                        if (n == 0)
                        {// ����� SSCC ��� �� �������������
                            sRf = xCDoc.DefDetFilter() + String.Format(" AND (NPODDZ={0})", nPoddonN);
                            dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "", DataViewRowState.CurrentRows);
                            if (dv.Count > 0)
                            {

                                foreach (DataRowView drv in dv)
                                {
                                    if ((drv.Row[sF]) != System.DBNull.Value)
                                    {
                                        s = (drv.Row[sF]).ToString();
                                        if ((s.Length > 0) && (s != sD))
                                        {
                                            dRez = MessageBox.Show(
                                            String.Format("SSCC={0} ��� ����������\n�������� (Enter)?\n(ESC)-���������� SSCC", drv.Row["SSCC"]),
                                            String.Format("������ {0}", nPoddonN),
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                            n = (dRez == DialogResult.OK) ? 1 : 0;
                                            break;
                                        }
                                    }
                                }
                                if (n == 0)
                                {// ������ ��� �� ���������
                                    // ������� ������ �� �����������
                                    sRf += String.Format("AND(READYZ<>{0})", (int)NSI.READINESS.FULL_READY);
                                    dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "", DataViewRowState.CurrentRows);
                                    n = dvZ.Count;
                                    if (n > 0)
                                    {// �� ��� ������ �������
                                        if (!xCDoc.bFreeKMPL)
                                        {
                                            dRez = MessageBox.Show(
                                                "������ �� ���������!\n�������� (Enter)?\r\n(ESC)-���������� SSCC",
                                                String.Format("������ {0}", nPoddonN),
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                            n = (dRez == DialogResult.OK) ? 1 : 0;
                                        }
                                        else
                                            n = 0;
                                    }
                                    if (n == 0)
                                    {
                                        bRet = AppC.RC_OKB;
                                        if (bNeedWrite)
                                            SetSSCCForPoddon(xSc.Dat, dv, nPoddonN);

                                        //foreach (DataRowView drv in dv)
                                        //{
                                        //    (drv.Row[sF]) = sSSCC;
                                        //}
                                        //MessageBox.Show(String.Format("������ {0} ����������� ({1}) �������",
                                        //    xCDoc.xNPs.Current, dv.Count));

                                        //xCDoc.xNPs.TryNextPoddon(true);
                                        //tCurrPoddon.Text = xCDoc.xNPs.Current.ToString();
                                    }
                                }
                            }
                            else
                                Srv.ErrorMsg("��� ���������������!");
                        }
                        else
                            Srv.ErrorMsg("SSCC ������� ��� �������������!");
                    }
                    else
                        Srv.ErrorMsg("� ������� �� ����������!");
                }
                else
                    Srv.ErrorMsg("������ ��� ������������!");





            }
            catch (Exception e)
            {
                Srv.ErrorMsg("������ ��� ������������!");
            }




            return (bRet);
        }

        //// ������ ����� ��� ���
        //public static bool IsDigKey(KeyEventArgs e, ref int nNum)
        //{
        //    bool bRet = AppC.RC_CANCELB;
        //    if ((e.KeyValue >= W32.VK_D1) && (e.KeyValue <= W32.VK_D9))
        //    {
        //        nNum = (e.KeyValue == W32.VK_D1) ? 1 : (e.KeyValue == W32.VK_D2) ? 2 : (e.KeyValue == W32.VK_D3) ? 3 :
        //                (e.KeyValue == W32.VK_D4) ? 4 : (e.KeyValue == W32.VK_D5) ? 5 : (e.KeyValue == W32.VK_D6) ? 6 :
        //                (e.KeyValue == W32.VK_D7) ? 7 : (e.KeyValue == W32.VK_D8) ? 8 : 9;
        //        bRet = AppC.RC_OKB;
        //    }
        //    return (bRet);
        //}

    }
}
