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

        // ����������� ���������� �� ��������� ���������� ������ (����������� �� ������� - �����)
        // � ��������������� �� ���������� �� ���
        public class OneEANStat
        {
            public class Sum4Cond
            {
                public NSI.SPECCOND tCond;
                public FRACT fEmk;
            }

            public Dictionary<Sum4Cond, FRACT> dicZVK;
            public Dictionary<Sum4Cond, FRACT> dicTTN;
        }

        // Event delegate and handler
        public delegate void ScanProceededEventHandler(object sender, EventArgs e);
        public event ScanProceededEventHandler AfterAddScan;

        private void OnPoddonReady(object sender, EventArgs e)
        {
            if (xCDoc != null)
            {
                switch (xCDoc.xDocP.TypOper)
                {
                    case AppC.TYPOP_KMPL:
                        // �������������� ����� �������� ������� ��� ������������
                        try
                        {
                            string sRf = xCDoc.DefDetFilter() + String.Format(" AND (NPODDZ={0}) AND (READYZ<>{1})", xCDoc.xNPs.Current, (int)NSI.READINESS.FULL_READY);
                            DataView dv = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "", DataViewRowState.CurrentRows);
                            if (dv.Count == 0)
                                TryNextPoddon(null);
                        }
                        catch (Exception ex)
                        {
                            Srv.ErrorMsg(ex.Message, true);
                        }
                        break;
                    case AppC.TYPOP_MOVE:
                        //if ((xCDoc.xOper != null) 
                        //    //&& (xCDoc.xOper.bObjOperScanned == false))
                        //    && ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) == 0)
                        //{// ������ �������� ����������
                        //    // ???
                        //    //xCDoc.xOper.bObjOperScanned = true;
                        //}
                        //break;
                    case AppC.TYPOP_DOCUM:
                        if (xCDoc.xDocP.nTypD == AppC.TYPD_BRK)
                        {
                            if (bShowTTN && (drDet != null))
                            {
                                xDLLAPars = new object[2] { xCDoc.xDocP.nTypD, drDet };
                                DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Brak.dll", true);
                                dgDet.Focus();
                            }
                        }
                        break;
                }
            }
        }


        private int TestProdBySrv(ref PSC_Types.ScDat sc)
        {
            ServerExchange 
                xSE = new ServerExchange(this);
            int
                nRet = AppC.RC_OK;

            if (PSC_Types.IsTara(sc.sEAN, sc.nKrKMC))
                return(nRet);

            nRet = ServConfScan(ref sc, xSE);

            return(TestProdBySrv(xSE, nRet));
        }


        private int TestProdBySrv(ServerExchange xSE, int nRet)
        {
            string
                sH = "�������� ���������!",
                sMess = "";
            bool
                b4biddScan = true;
                            
            if (nRet != AppC.RC_OK)
            {
                if (xSE.ServerRet != AppC.EMPTY_INT)
                {// ����� �� ������� �������� �������
                    try
                    {
                        sMess = xSE.ServerAnswer["MSG"];
                    }
                    catch
                    {
                        sMess = "������������ ���������!";
                    }

                    if (xSE.ServerRet == AppC.RC_HALFOK)
                    {// ������ ������ ����������
                        if (sMess.Length == 0)
                            sMess = "������������ ���������!";
                        sMess += "\n(OK-�����)\n(ESC-���������)";

                        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                        DialogResult drQ = MessageBox.Show(sMess, sH,
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (drQ != DialogResult.OK)
                        {
                            b4biddScan = false;
                            nRet = AppC.RC_OK;
                        }
                        else
                        {// Enter - ����� �� ������������, �������� ������ �� ����
                            sMess = "";
                        }
                    }
                    if (b4biddScan)
                    {
                        if (sMess.Length > 0)
                            Srv.ErrorMsg(sMess, sH, true);
                    }
                }
                else
                {
                    sMess = "������ ����������!";
                    sMess += "\n(OK-�����)\n(ESC-���������� ����)";

                    DialogResult drQ = MessageBox.Show(sMess, "������!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (drQ != DialogResult.OK)
                    {
                        b4biddScan = false;
                        nRet = AppC.RC_OK;
                    }
                    else
                    {// Enter - ����� �� ������������, �������� ������ �� ����
                    }

                }
            }





            return (nRet);
        }

        //private int ProceedProd(ScanVarGP xSc, ref PSC_Types.ScDat sc, bool bDupScan, bool bEasyEd, bool bNewBC)



        /// ��������� �� �����-�� �� ����� ��������
        private bool ProceedProd(ref PSC_Types.ScDat sc, bool bDupScan, bool bEasyEd, bool bNewBC)
        {
            int 
                nRet = AppC.RC_CANCEL;
            bool
                bNewDetAdded = false,
                bDopValuesNeed = true;
            DataRow
                drNewProd = null;


            #region ��������� ����� ���������
            do
            {
                if (!CanSetOperObj())
                    break;

                xCDoc.bConfScan = (ConfScanOrNot(xCDoc.drCurRow, xPars.ConfScan) > 0) ? true : false;

                if (!bDupScan)
                {// ��������� ���� ���������, ���������� ��� ������
                    if (xCDoc.xDocP.TypOper == AppC.TYPOP_MOVE)
                    {
                        if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() && 
                            (xSm.xAdrFix1 == null) )


                        //bool g = !xCDoc.xOper.IsFillSrc();
                        //g &= !xCDoc.xOper.IsFillDst();
                        //g &= (xSm.xAdrFix1 == null);
                        //if (g)
                        {
                            Srv.ErrorMsg("����� �� ������!", true);
                            break;
                        }
                    }
                }
                else
                {// ��������� ���� ���� �� ��������� - ���, ��� �������, ������������� �����
                    if ((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) && (!bEasyEd))
                    {
                        if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                        {
                            if (sc.nRecSrc == (int)NSI.SRCDET.SCAN)
                            {
                                Srv.ErrorMsg("��� �� ������!", true);
                                break;
                            }
                        }
                    }
                }

                if (xCDoc.drCurRow != null)
                {
                    scCur = sc;
                    if (!sc.bFindNSI)
                        Srv.ErrorMsg("��� �� ������! �������� ���!", true);

                    bDopValuesNeed = true;
                    nRet = AppC.RC_OK;
                    if (scCur.bVes == true)
                    {
                        scCur.fVsego = scCur.fVes;
                        //FRACT fE = scCur.fVes;

                        // 11.07.14 ����� � �������
                        if (scCur.nRecSrc != (int)NSI.SRCDET.SSCCT)
                        {
                            if (!xScan.dicSc.ContainsKey("37") && (xScan.dicSc.Count == 4))
                            {
                                scCur.tTyp = AppC.TYP_TARA.TARA_POTREB;
                                scCur.fEmk = scCur.fEmk_s = 0;
                            }


                            if ((!bNewBC) || (!scCur.bEmkByITF))
                            {
                                // ����� ��-�������
                                if ((scCur.tTyp == AppC.TYP_TARA.TARA_PODDON) && (scCur.nMestPal > 0))
                                {
                                    PSC_Types.ScDat
                                        scTmp = scCur;
                                    bool
                                        bSetEmk = TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt,
                                                ref scTmp, scTmp.fVes / scTmp.nMestPal);
                                    //if ((bSetEmk == true) && (scTmp.nTypVes == AppC.TYP_VES_TUP))
                                    if (bSetEmk == true)
                                    {
 
                                        //if (scTmp.tTyp == AppC.TYP_TARA.TARA_TRANSP))
                                        scCur.fEmk = scCur.fEmk_s = scTmp.fEmk;
                                        //scCur.nMest = scCur.nMestPal;
                                    }
                                }
                                else
                                    TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt, ref scCur, scCur.fVes);
                            }
                        }
                        if ((xCDoc.xDocP.nTypD == AppC.TYPD_OPR) && (scCur.tTyp == AppC.TYP_TARA.TARA_PODDON))
                        {
                            bDopValuesNeed = false;
                            //sc.nMest = sc.nMestPal;
                            scCur.nMest = scCur.nMestPal;
                        }
                        if (AppPars.bVesNeedConfirm == false)
                        {// ������������� ����� ���������
                            //if ((scCur.nTypVes == AppC.TYP_VES_TUP) ||
                            //    (scCur.nTypVes == AppC.TYP_PALET) ||
                            //    ((scCur.nTypVes == AppC.TYP_VES_1ED) && (scCur.nParty.Length > 0)))
                            if ((scCur.tTyp != AppC.TYP_TARA.UNKNOWN) ||
                                ((scCur.tTyp == AppC.TYP_TARA.TARA_POTREB) && (scCur.nParty.Length > 0)))
                            {
                                bDopValuesNeed = false;
                            }
                        }
                        if ((scCur.fVes == fDefVes) && (scCur.fVes > 0))
                        {
                            DialogResult dr = MessageBox.Show("�������� ���� (Enter)?\r\n(ESC) - ���������� ����",
                                String.Format("��� �� ���-{0}!", scCur.fVes), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.OK)
                                nRet = AppC.RC_CANCEL;
                        }
                        fDefVes = scCur.fVes;
                        if (scCur.ci == BCId.EAN13)
                        {
                            if (xCDoc.xDocP.nTypD == AppC.TYPD_ZKZ)
                            {// � ������ ����������� ������ ����� !!!
                                nRet = AppC.RC_CANCEL;
                                Srv.ErrorMsg("��� ��� ����������!", true);
                            }
                        }
                    }
                    else
                    {
                        if ((scCur.tTyp == AppC.TYP_TARA.TARA_PODDON))
                        {
                            if (AppPars.bVesNeedConfirm == false)
                                bDopValuesNeed = false;

                        }
                        else
                        {
                            nRet = AppC.RC_OK;
                        }
                    }

                    if ((AppC.RC_OK == nRet) &&
                        (AppC.RC_OK == EvalZVKMest(ref scCur, null, 0, 0)))
                    {

                            nRet = Prep4Ed(ref scCur, ref bDopValuesNeed, 0);

                        //VerifyMestByPoddon(ref scCur);

                        SetDopFieldsForEnter(false);
                        if (nRet == AppC.RC_OK)
                        {
                            PInfReady();
                            SetDetFields(false);
                            ShowDopInfKMPL(scCur.nKolM_alr + scCur.nKolM_alrT);

                            if ((bDopValuesNeed == false) && (bEditMode))
                            {// �������������� ���� ����������, �� ���� �� ��� ������?
                                bDopValuesNeed = (VerifyVvod().nRet == AppC.RC_CANCEL) ? true : false;
                                //int tR = VerifyVvod().nRet;
                            }

                            if (xCDoc.bConfScan)
                            {// �������� ������� ������������� ����� ��� ������ ���������
                                //if ((sc.nRecSrc != (int)NSI.SRCDET.FROMADR) &&
                                //    (sc.nRecSrc != (int)NSI.SRCDET.SSCCT))
                                //    {// ���������� � ������� �� ���������
                                //    if (TestProdBySrv(ref sc) != AppC.RC_OK)
                                //        break;
                                //}

                                // ���������� � ������� ����! ���������
                                if (TestProdBySrv(ref scCur) != AppC.RC_OK)
                                    break;
                            }

                            if ((bDopValuesNeed == true))
                            {// ����� ��������������
                                int nR = IsGeneralEdit(ref scCur);
                                if (nR == AppC.RC_OK)
                                    AddOrChangeDet(AppC.F_ADD_SCAN);
                                else if ((nR == AppC.RC_CANCEL) || (nR == AppC.RC_NOTALLDATA))
                                {
                                    //Srv.ErrorMsg("���������������!\r\n��������� � ������� �����!");
                                    ZVKeyDown(AppC.F_PODD, null, ref ehCurrFunc);
                                }
                                else if (nR == AppC.RC_BADTABLE)
                                {
                                    //Srv.ErrorMsg("������������ � ������...\r\n��������� ������������");
                                    ChgDetTable(null, "");
                                }
                                else if (nR == AppC.RC_ZVKONLY)
                                {
                                    Srv.ErrorMsg("������ � �������!");
                                }
                            }
                            else
                            {
                                bNewDetAdded = AddDet1(ref scCur, out drNewProd);
                                SetDopFieldsForEnter(true);
                            }
                        }
                        else
                        {// ����� �� ������������, ���� �� ������ ����������
                            ChangeDetRow(true);
                        }
                    }
                    else
                    {// ����� �� ������������, ���� �� ������ ����������
                        ChangeDetRow(true);
                    }

                }
            } while (false);
            #endregion

            return (bNewDetAdded);
        }
        
        // ���������� ���� �� ������
        private int EvalZVKMest(ref PSC_Types.ScDat sc, DataView dvZ, int nCurR, int nMaxR)
        {
            bool 
                bNeedAsk = (IsEasyEdit()) ? false : true;
            int 
                nRet = AppC.RC_OK;
            string 
                sH = "",
                sDopMsg = "",
                sErr = "";

            sc.nDest = NSI.DESTINPROD.GENCASE;
            if (bZVKPresent)
            {
                nRet = LookAtZVK(ref sc, dvZ, nCurR, nMaxR);
                if (nRet != AppC.RC_OK)
                {
                    sH = String.Format("{0} / {1}", sc.nKrKMC, sc.fEmk);
                    switch (nRet)
                    {
                        case AppC.RC_SAMEVES:
                            sErr = String.Format("��� �� ���-{0}!", sc.fVes);
                            if (sc.nKrKMC == nOldKrk)
                                bNeedAsk = true;
                            break;
                        case AppC.RC_NOEAN:
                            sErr = sc.nKrKMC.ToString() + "-��� � ������!";
                            if (sc.nKrKMC == nOldKrk)
                            {// ���������� ��� �� ���� ���������� ?
                                if (bAskKrk == true)
                                    bNeedAsk = false;
                            }
                            //if (IsEasyEdit())
                            //{
                            //}
                            break;
                        case AppC.RC_BADPARTY:
                            sErr = String.Format("������ {0} ��� � ������!", sc.nParty);
                            bNeedAsk = true;
                            break;
                        case AppC.RC_BADDATEXCT:
                        case AppC.RC_BADDATE:
                            //DataRow drZ;
                            //string dV, dG, eM;
                            //try
                            //{
                            //    drZ = sc.lstAvailInZVK[sc.nCurAvail];
                            //    dV = (DateTime.ParseExact((string)drZ["DVR"], "yyyyMMdd", null)).ToString("dd.MM.yy");
                            //    eM = drZ["EMK"].ToString();
                            //}
                            //catch 
                            //{
                            //    dV = "";
                            //    eM = "";
                            //}
                            //sErr = String.Format("���(����):{0}\n���(����):{1}", dV,sc.sDataIzg);
                            //sH = String.Format("{0} / {1}", sc.nKrKMC, sc.fEmk);
                            sErr = sc.sErr;
                            bNeedAsk = true;
                            break;
                        case AppC.RC_NOEANEMK:
                            sErr = sc.nKrKMC.ToString() + "/" + sc.fEmk.ToString() + "-��� � ������!";
                            if ((sc.nKrKMC == nOldKrk) && (sc.fEmk == nOldKrkEmkNoSuch))
                                if (bAskEmk == true)
                                    bNeedAsk = true;
                            break;
                        case AppC.RC_BADPODD:
                            if (!xCDoc.bFreeKMPL)
                            {
                                sErr = sc.sErr;
                                bNeedAsk = true;
                            }
                            else { bNeedAsk = false; }
                            break;
                        case AppC.RC_UNVISPOD:
                            sErr = "������� ������ �� �������!";
                            break;
                        case AppC.RC_NOAUTO:
                            sErr = "������ �� ������������!";
                            bNeedAsk = true;
                            break;
                        case AppC.RC_ALREADY:
                            sErr = "������ ��� ���������!";
                            bNeedAsk = true;
                            break;
                        default:
                            sErr = "������������� ������!";
                            break;
                    }

                    bNeedAsk = (bInScanProceed || bEditMode) ? bNeedAsk : false;
                    if (bNeedAsk && ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) 
                        || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR) 
                        || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR_BUTTON)))
                    {
                        if (nRet != AppC.RC_NOEAN)
                            bNeedAsk = false;
                    }

                    if (bNeedAsk == true)
                    {
                        bAskEmk = false;
                        bAskKrk = false;

                        //DataRow drZ = sc.lstAvailInZVK[sc.nCurAvail];
                        //int nCondition = (int)drZ["COND"];

                        //if (((nCondition & (int)NSI.SPECCOND.DATE_SET_EXACT) > 0) ||
                        //    ((nCondition & (int)NSI.SPECCOND.PARTY_SET) > 0))

                        if ((nRet == AppC.RC_BADPARTY) ||
                            (nRet == AppC.RC_BADDATEXCT)) 
                        {
                            if (xPars.BadPartyForbidd)
                            {
                                if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
                                {
                                    bNeedAsk = false;
                                    sErr = "��������� ������\n ������/����!!!";
                                }
                            }
                        }
                    }

                    if (bNeedAsk == true)
                    {
                        DialogResult
                            dr = MessageBox.Show(sErr + ((sDopMsg.Length > 0)?"\n" + sDopMsg:"") + "\n\n�������� ���� (Enter)?\n(ESC) - ���������� ����",
                            sErr,
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                        if (dr == DialogResult.OK)
                            nRet = AppC.RC_CANCEL;
                        else
                        {
                            sc.nDest = NSI.DESTINPROD.USER;
                            if (nRet == AppC.RC_NOEANEMK)
                            {
                                bAskEmk = true;
                                nOldKrk = sc.nKrKMC;
                                fOldEmk = sc.fEmk;
                            }
                            if (nRet == AppC.RC_NOEAN)
                            {
                                bAskKrk = true;
                                nOldKrk = sc.nKrKMC;
                            }
                            if (nRet == AppC.RC_BADPODD)
                                sc.nDest = NSI.DESTINPROD.GENCASE;
                            nRet = AppC.RC_OK;

                            //if (bInEasyEditWait == true)
                            //{
                            //    SetFltVyp(false);
                            //    ZVKeyDown(0, new KeyEventArgs(Keys.Cancel));
                            //}
                        }
                    }
                    else
                    {// ������� �� �����, ������ ��������� 
                        if (bInScanProceed || bEditMode)
                        {// ��������� ������������
                            if (xScrDet.CurReg != ScrMode.SCRMODES.FULLMAX)
                                nRet = AppC.RC_OK;
                            if (sErr != "")
                                Srv.ErrorMsg(sErr, sH, true);
                            if ((sc.drTotKey != null) || (sc.drTotKeyE != null))
                                sc.nDest = NSI.DESTINPROD.TOTALZ;
                            if  (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
                                nRet = AppC.RC_CANCEL;
                        }
                    }
                }
                else
                {
                    bAskEmk = false;
                    bAskKrk = false;
                    if ((sc.drTotKey != null) || (sc.drTotKeyE != null))
                        sc.nDest = NSI.DESTINPROD.TOTALZ;
                }
            }

            return (nRet);
        }

        private bool IsEasyEdit()
        {
            return (
            ((xScrDet.CurReg == ScrMode.SCRMODES.FULLMAX) &&
                (dgDet.DataSource == xNSI.DT[NSI.BD_DIND].dt)) ? true : false
                );
        }

        // fCurEmk - ������� � ������ ������
        private bool WhatTotKeyF_Emk(DataRow dr, ref PSC_Types.ScDat sc, FRACT fCurEmk)
        {
            bool bSet = false;
            if (fCurEmk == 0)
            {// � ������ - �������
                if ((sc.fEmk == 0) || bEditMode)
                {// ��� �������� ������� ������ �������� �����
                    if (sc.drTotKeyE == null)
                    {
                        sc.drTotKeyE = dr;
                    }
                    bSet = true;
                }
            }
            else
            {
                if (sc.fEmk == fCurEmk)
                {
                    if (sc.drTotKey == null)
                    {
                        sc.drTotKey = dr;
                    }
                    bSet = true;
                }
            }
            return (bSet);
        }

        private bool WhatPrtKeyF_Emk(DataRow dr, ref PSC_Types.ScDat sc, FRACT fCurEmk)
        {
            bool 
                bSet = false;

            if (fCurEmk == 0)
            {
                //if ((sc.fEmk == 0) || bEditMode)
                //{// ��� �������� ������� ������ �������� �����
                //    if (sc.drPartKeyE == null)
                //    {
                //        sc.drPartKeyE = dr;
                //    }
                //    bSet = true;
                //}
                if ((sc.fEmk == 0) || (sc.nRecSrc != (int)NSI.SRCDET.CR4CTRL))
                {// ��� �������� ������� ������ �������� �����
                    bSet = true;
                    if (sc.drPartKeyE == null)
                        sc.drPartKeyE = dr;
                }
            }
            else
            {
                if (sc.fEmk == fCurEmk)
                {
                    if (sc.drPartKey == null)
                    {
                        sc.drPartKey = dr;
                    }
                    bSet = true;
                }
            }
            return (bSet);
        }


        // ������ � �������� ��� ��������� �� ������ � ���
        // ��� ����������� ������� � ��� �������������
        private string FilterKompl(int nSys, string sKMC, bool bUsePoddon)
        {
            int nCurPoddon = 0;
            string ret = "(SYSN={0})AND(KMC='{1}')";

            if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
            {// ��� ������������ ����� ������������ ����������� �� �������

                //if (xSm.FilterTTN == NSI.FILTRDET.NPODD)


                if (bUsePoddon)
                {// ������ �� ������� ����������
                    try
                    {
                        nCurPoddon = xCDoc.xNPs.Current;
                    }
                    catch { nCurPoddon = 0; }
                    if (nCurPoddon > 0)
                        ret += "AND(NPODDZ={2})";
                }
            }
            ret = String.Format(ret, nSys, sKMC, nCurPoddon);
            return (ret);
        }

        // ������� � ������
        // ��� ��������:
        // RC_NOEAN - ���� ��� � ������, ������� ������ �� �����������
        // RC_NOEANEMK - ��������� ������ ���� ��� � ������, ���� � ����� �������� ����
        //               ��������� nKolM_zvk - ����� �����-�� ����
        // RC_OK - ���� ��� �������� (fKolE_zvk != 0, ����� �������� drPartKeyE != null drTotKeyE != null) 
        //         ��� ����� (nKolM_zvk != 0, ����� �������� drPartKey != null drTotKey != null)

        private int LookAtZVK(ref PSC_Types.ScDat sc, DataView dv, int i, int nMaxR)
        {
            bool
                bEvalInPall;
            DataRow
                dr = null;
            int 
                nRFind = AppC.RC_ALREADY,
                nRet = AppC.RC_OK,
                nTR,
                nM = 0,
                nMestEmk = 0,
                nMest = 0,
                nState;
            string
                nParty = "";

            FRACT fCurEmk;

            sc.ZeroZEvals();
            if ((xCDoc.xNPs.Current > 0) && ((xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL) || (xCDoc.xDocP.TypOper == AppC.TYPOP_OTGR)))
                bEvalInPall = true;
            else
                bEvalInPall = false;

            // ������ - SYSN + KMC
            if (bInScanProceed || bEditMode)
            {// ����� ��� ����� ������ ����� ������������
                nMaxR = 0;
                if (bEvalInPall) 
                {// �� �������� �������
                    sc.sFilt4View = FilterKompl(xCDoc.nId, sc.sKMC, true);
                    dv = new DataView(xNSI.DT[NSI.BD_DIND].dt,
                        sc.sFilt4View, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);
                    nMaxR = dv.Count;
                    if (nMaxR > 0)
                    {// ���� ���� ���������� - �������� � ��������, ����� - ���� ��������
                        i = 0;
                        for (int j = 0; j < nMaxR; j++)
                        {
                            if ((int)dv[j].Row["READYZ"] != (int)NSI.READINESS.FULL_READY)
                            {
                                i = nMaxR;
                                break;
                            }
                        }
                        nMaxR = i;
                    }
                }
                if (nMaxR == 0)
                {
                    sc.sFilt4View = FilterKompl(xCDoc.nId, sc.sKMC, false);
                    dv = new DataView(xNSI.DT[NSI.BD_DIND].dt,
                        sc.sFilt4View, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);
                    if (bEvalInPall)
                    {// �� �������� ������� ������ �� �������
                        if (dv.Count > 0)
                        {// � ��� ��� ������ �������
                            if (xSm.FilterTTN == NSI.FILTRDET.NPODD)
                            {// ������ ������� �� �������� �� ������
                                return (AppC.RC_UNVISPOD);
                            }
                            else
                            {// ������ ������� �� ������, �� �������� ����
                                nRet = AppC.RC_BADPODD;
                            }
                        }
                        else
                        {// �� ������ �������� ����� ������ ���
                            return (AppC.RC_NOEAN);
                        }
                    }
                    nMaxR = dv.Count;
                }
                i = 0;
            }
            else
                sc.sFilt4View = FilterKompl(xCDoc.nId, sc.sKMC, false);

            while ((i < nMaxR) && ((string)dv[i].Row["KMC"] == sc.sKMC))
            {
                dr = dv[i].Row;
                fCurEmk = (FRACT)dr["EMK"];
                nParty = (string)dr["NP"];
                nState = (int)dr["READYZ"];

                if (fCurEmk == 0)
                {// ������� �� ������� (�������� ��������� ������� ���������)
                    sc.fKolE_zvk += (FRACT)dr["KOLE"];
                }
                else
                {// ������� ������������ � ������, ��������� �����
                    nM = (int)dr["KOLM"];
                    nMest += nM;
                    if (fCurEmk == sc.fEmk)
                    {// � ����-������ ������� ����� �� � ���������
                        nMestEmk += nM;
                    }
                }

                // ���-�� �� ������ ����������� ���� �������������?
                nTR = FindRowsInZVK(dr, ref sc, nParty, fCurEmk, nRet, bEvalInPall);
                if (nTR == AppC.RC_OK)
                    sc.lstAvailInZVK.Add(dr);
                else
                {
                    //if ((nRFind == AppC.RC_ALLREADY) && (fCurEmk == sc.fEmk) && (nParty == sc.nParty) )
                    //    break;
                    if (nTR != AppC.RC_ALREADY)
                        nRFind = nTR;
                }

                i++;
            } // �������� ����


            if (nMaxR > 0)
            {// �����-�� ������ ���-���� ����
                if (sc.fEmk == 0)
                {// ������� �� ��������� ����-������ ���������� �� �������
                    sc.nKolM_zvk = nMest;
                }
                else
                {
                    sc.nKolM_zvk = nMestEmk;
                }

                if (sc.lstAvailInZVK.Count > 0)
                {
                    sc.nCurAvail = 0;
                    sc.nKolM_zvk = 0;
                    foreach (DataRow drl in sc.lstAvailInZVK)
                    {
                        sc.nKolM_zvk += (int)drl["KOLM"];
                        if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
                            break;                                          // �������� ������ � ����� �������
                    }
                    nRet = AppC.RC_OK;
                }
                else
                {// ������ ���������� �������� - ������ �� �����
                    nRet = nRFind;
                }

                if (nRet == AppC.RC_OK)
                {// ���� ��� ��� � �������
                    if ((nMest != 0) && (nMestEmk == 0) && (sc.fEmk != 0))
                    {// 
                        nRet = AppC.RC_NOEANEMK;
                    }

                }
            }
            else
                nRet = AppC.RC_NOEAN;

            return (nRet);
        }


        /// �������� �� ��� ��������������� ��������� � ������?
        /// dr - ������ ������
        //private bool CmpFromTTN2ZVK_(int nCond, FRACT fEmk, DateTime dDat, string nP,
        //    FRACT fEmk_TTN, DateTime dDat_TTN, string sParty_TTN)
        //{
        //    bool bMayUsed = false;

        //    #region ������������� ������ ������ � ������� ������ � ������ ���
        //    do
        //    {
        //        if (fEmk == fEmk_TTN)
        //        {
        //            if (nCond != (int)NSI.SPECCOND.NO)
        //            {// ���� ����������� ��� ���� ��� ������
        //                if (nCond == (int)NSI.SPECCOND.PARTY_SET)
        //                {// ������� ���������� ������ �� xx/yy/zz
        //                    if ((nP == sParty_TTN) && (dDat == dDat_TTN))
        //                    {// ������ ���������� �����
        //                        bMayUsed = true;
        //                    }
        //                    break;
        //                }
        //                if (dDat_TTN >= dDat)
        //                {// �� ����� �������� ��������
        //                    bMayUsed = true;
        //                    break;
        //                }
        //            }
        //            else
        //            {// ����� ������, ����������� ����� ������� ��� �����
        //                bMayUsed = true;
        //                break;
        //            }
        //        }
        //    } while (false);

        //    #endregion

        //    //if (!bMayUsed)
        //    //{
        //    //    int k = 12 * 6;
        //    //    bMayUsed = false;
        //    //}

        //    return (bMayUsed);
        //}


        // �������� �� ��� ��������������� ��������� � ������?
        // dr - ������ ������
        private bool CmpFromTTN2ZVK(ref PSC_Types.ScDat sc, FRACT fEmk_TTN, DateTime dDat_TTN, string sParty_TTN)
        {
            bool 
                bTryNext = false,
                bMayUsed = false;
            string
                sParty_ZVK;
            DateTime
                dDat_ZVK = DateTime.MinValue;
            FRACT 
                fEmk_ZVK;
            DataRow
                drZ = null;
            int
                iZ = 0,
                nCond;

            #region ������������� ������ ������ � ������� ������ � ������ ���
            do
            {
                drZ = sc.lstAvailInZVK[iZ];
                nCond = (int)drZ["COND"];
                fEmk_ZVK = (FRACT)drZ["EMK"];
                sParty_ZVK = (string)drZ["NP"];

                if ((nCond & (int)NSI.SPECCOND.DATE_SET) > 0)
                    dDat_ZVK = (drZ["DVR"] is string) ? DateTime.ParseExact((string)drZ["DVR"], "yyyyMMdd", null) : DateTime.MinValue;


                if ((fEmk_ZVK == fEmk_TTN) || (fEmk_ZVK == 0))
                {
                    if (nCond != (int)NSI.SPECCOND.NO)
                    {// ���� ����������� ��� ���� ��� ������
                        if (nCond == (int)NSI.SPECCOND.PARTY_SET)
                        {// ������� ���������� ������ �� xx/yy/zz
                            if ((sParty_ZVK == sParty_TTN) && (dDat_ZVK == dDat_TTN))
                            {// ������ ���������� �����
                                bMayUsed = true;
                            }
                            break;
                        }
                        if (dDat_TTN >= dDat_ZVK)
                        {// �� ����� �������� ��������
                            bMayUsed = true;
                            break;
                        }
                    }
                    else
                    {// ����� ������, ����������� ����� ������� ��� �����
                        bMayUsed = true;
                        break;
                    }
                }
                if (++iZ >= sc.lstAvailInZVK.Count)
                    bTryNext = false;
            } while (!bMayUsed && bTryNext);

            #endregion

            if (bMayUsed)
                // ����� �� break - ������!!!
                sc.nCurAvail = iZ;
            //{
            //    int k = 12 * 6;
            //    bMayUsed = false;
            //}
            return (bMayUsed);
        }




        // ��������� ������ ���
        // ������� ��� �������������
        private int EvalEnteredKol(DataView dvEn, int i, ref PSC_Types.ScDat sc, string sKMCCode, FRACT fE, 
            out int nM_A, out FRACT fE_A, bool bDocControl)
        {
            //int ret = AppC.RC_OK;
            bool 
                bInEasy,
                bTry2FindSimilar,
                bUsingZVK = false,
                bSame = false;
            string
                nParty_ZVK = "",
                nParty = "";

            int 
                ret = 0,
                nM = 0,
                nMaxR = 0,
                nCond = (int)NSI.SPECCOND.NO,

                // ������������� ���� � ������ ����������� �����
                nKolM_alrT = 0,
                // ������������� ���� � ���������� ����������� �����
                nKolM_alr = 0;

            DateTime
                dDVyr;
                //dDVyr_ZVK = DateTime.Now;

            FRACT 
                fEm = 0,
                fEmk_ZVK = 0,

                fV = 0,
            
                // ������������� ������ � ������ ����������� �����
                fKolE_alrT = 0,
                // ������������� ������ � ���������� ����������� �����
                fKolE_alr = 0;

            //NSI.DESTINPROD 
            //    desProd;
            DataRow
                dr,
                drZ = null;

            // ����� ���� �� ���� �������������
            nM_A = 0;
            // ����� ������ �� ���� �������������
            fE_A = 0;

            // ���� ������ ������ �� ���������
            sc.drEd = sc.drMest = null;

            bInEasy = IsEasyEdit();

            if ((sc.lstAvailInZVK.Count > 0) && (sc.nCurAvail >= 0))
            {// ���������� ������ ��� ������� ����� (�������� � ����� �� ��������� �����)
                bUsingZVK = true;
                drZ = sc.lstAvailInZVK[sc.nCurAvail];
                nCond = (int)drZ["COND"];
                fEmk_ZVK = (FRACT)drZ["EMK"];
                //if (nCond != (int)NSI.SPECCOND.NO)
                //    dDVyr_ZVK = DateTime.ParseExact((string)drZ["DVR"], "yyyyMMdd", null);

                //if ((nCond & (int)NSI.SPECCOND.DATE_V_SET) > 0)
                //    dDVyr_ZVK = DateTime.ParseExact((string)drZ["DVR"], "yyyyMMdd", null);

                nParty_ZVK = (string)drZ["NP"];
            }

            // ������ - SYSN + KMC [+ � �������]
            nMaxR = dvEn.Count;
            while ((i < nMaxR) && ((string)dvEn[i].Row["KMC"] == sKMCCode))
            {
                dr = dvEn[i].Row;
                nM = (int)dr["KOLM"];
                fV = ((int)dr["SRP"] > 0) ? 1 : (FRACT)dr["KOLE"];
                fEm = (FRACT)dr["EMK"];
                nParty = (string)dr["NP"];
                dDVyr = DateTime.ParseExact((string)dr["DVR"], "yyyyMMdd", null);
                //desProd = (NSI.DESTINPROD)(dr["DEST"]);

                // ��� ���� ����� ����� �� � ������ ���?
                bSame = ((nParty == sc.nParty) && (dDVyr.Date == sc.dDataIzg.Date)) ? true : false;

                // �� � ��������� ������� ����� �� ��� �� �����������
                //if (bInScanProceed || bEditMode)
                //{
                //    if ((bInEasy) || (!bSame))
                //    //if (bInEasy)
                //    {
                //        bTry2FindSimilar = CmpFromTTN2ZVK(nCond, fEmk_ZVK, dDVyr_ZVK, nParty_ZVK, fEm, dDVyr, nParty);
                //    }
                //}

                
                if (bUsingZVK)
                {// �������� ������� ������ �� ��� � ������ ����������?
                    //bTry2FindSimilar = CmpFromTTN2ZVK(nCond, fEmk_ZVK, dDVyr_ZVK, nParty_ZVK, fEm, dDVyr, nParty);
                    bTry2FindSimilar = CmpFromTTN2ZVK(ref sc, fEm, dDVyr, nParty);
                }
                else
                    bTry2FindSimilar = true;

                if (bTry2FindSimilar)
                {
                    if (fEm == 0)
                    {// ��������������� �������

                        fE_A += fV;
                        if (bSame)
                        {
                            fKolE_alrT += fV;
                            if (bDocControl)
                                dr["DEST"] = NSI.DESTINPROD.TOTALZ;
                            if (sc.drEd == null)
                            {
                                if ((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) &&
                                    ((int)dr["NPODDZ"] == 0))
                                {// � �������������� ������� �� ���������
                                    sc.drEd = dr;
                                }
                            }
                        }
                        else
                        {// ������� �� ����� ������
                            fKolE_alr += fV;
                            if (bDocControl)
                                dr["DEST"] = NSI.DESTINPROD.PARTZ;
                        }
                    }
                    else
                    {// ��������������� �����
                        if (fEm == fE)
                        {
                            nM_A += nM;
                            if (bSame)
                            {
                                nKolM_alrT += nM;
                                if (bDocControl)
                                    dr["DEST"] = NSI.DESTINPROD.TOTALZ;

                                if (sc.drMest == null)
                                {
                                    if ((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) &&
                                        ((int)dr["NPODDZ"] == 0))
                                    {// � �������������� ������� �� ���������
                                        sc.drMest = dr;
                                    }
                                }
                            }
                            else
                            {// ��������� ����� �����
                                nKolM_alr += nM;
                                if (bDocControl)
                                    dr["DEST"] = NSI.DESTINPROD.PARTZ;
                            }
                        }
                    }
                }
                i++;
            }

            sc.nKolM_alrT = nKolM_alrT;
            sc.fKolE_alrT = fKolE_alrT;

            sc.nKolM_alr = nKolM_alr;
            sc.fKolE_alr = fKolE_alr;

            return (ret);

        }




        private DataRow PrevKol(ref PSC_Types.ScDat sc, ref int nAlrM, ref FRACT fAlrE)
        {

            int 
                nIDZvk;
            string 
                sF;
            DataRow
                drZ = null;
            DataView
                dv;

            nAlrM = 0;
            fAlrE = 0;

            try
            {
                drZ = sc.lstAvailInZVK[sc.nCurAvail];
                nIDZvk = (int)(drZ["NPP"]);

                sF = String.Format("{0} AND (NPP_ZVK={1})", sc.sFilt4View, nIDZvk);
                dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt,
                    sF, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);

                foreach (DataRowView drv in dv)
                {
                    nAlrM += (int)(drv.Row["KOLM"]);
                    fAlrE += (FRACT)(drv.Row["KOLE"]);
                }

            }
            catch
            {
                drZ = null;
                nIDZvk = -1;
            }


            sc.nMAlr_NPP = nAlrM;
            sc.fVAlr_NPP = fAlrE;

            return(drZ);
        }
                
        // ������ ����������� ��� ����� ����������
        // nNonCondUsing = -1 - �� ������������ ������, ���� ���� ����
        // nNonCondUsing =  1 - ������������ ������, ���� ���� �� ���������
        private int Prep4Ed(ref PSC_Types.ScDat sc, ref bool bWillBeEdit, int nUnCondUsing)
        {
            int
                nM = 0,                 // ��������� ���������� ����, ��� ��������������� � ���
                nMEd = 0,               // ���������� ���������� ���� ��� ��������������/�������������
                nRet = AppC.RC_OK;
            bool
                bUseZVK = false;        // � ������� ���-�� �� ��������, ��� �� ���������

            string
                sErr;

            FRACT
                fVEd = 0,               // ���������� ���������� ������ ��� ��������������/�������������
                fV = 0;                 // ��������� ���������� ������ , ��� ��������������� � ���


            DataRow 
                drZ;
            DataView 
                dv4Sum = null;

            if (xCDoc.xDocP.nTypD == AppC.TYPD_OPR)
            {// �������� �������������� ��������

                if ((xCDoc.xDocP.TypOper == AppC.TYPOP_PRMK) ||
                    (xCDoc.xDocP.TypOper == AppC.TYPOP_MOVE) ||
                    (xCDoc.xDocP.TypOper == AppC.TYPOP_MARK))
                {
                    if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                    {
                        bWillBeEdit = false;
                        if ((sc.nRecSrc == (int)NSI.SRCDET.FROMADR) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR_BUTTON) ||
                            (sc.nRecSrc == (int)NSI.SRCDET.SSCCT)) { }
                        else
                            sc.nMest = sc.nMestPal;
                        if (sc.bVes == true)
                        {
                            sc.fVsego = sc.fVes;
                        }
                        else
                        {
                            sc.fVsego = sc.nMest * sc.fEmk;
                        }
                    }
                    else
                    {
                        sc.nMest = sc.nMestPal;
                        if (sc.bVes == true)
                        {
                            //sc.nMest = (sc.nTypVes == AppC.TYP_VES_PAL) ? -1 :
                            //            (sc.nTypVes == AppC.TYP_VES_TUP) ? 1 : 0;
                            sc.nMest = (sc.tTyp == AppC.TYP_TARA.TARA_PODDON) ? -1 :
                                        (sc.tTyp == AppC.TYP_TARA.TARA_TRANSP) ? 1 : 0;
                            sc.fVsego = sc.fVes;
                        }
                        else
                            sc.fVsego = ((sc.nMest == 0) ? 1 : sc.nMest) * sc.fEmk;
                    }
                    if (sc.fVsego == 0)
                        bWillBeEdit = true;
                }
                return (nRet);
            }

            if (sc.sFilt4View.Length == 0)
                sc.sFilt4View = FilterKompl(xCDoc.nId, sc.sKMC, false);

            dv4Sum = new DataView(xNSI.DT[NSI.BD_DOUTD].dt,
                sc.sFilt4View, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);

            EvalEnteredKol(dv4Sum, 0, ref sc, sc.sKMC, sc.fEmk, out nM, out fV, false);

            if (bZVKPresent)
            {// ������ �������
                if (sc.nCurAvail >= 0)
                {// � ������ ������� ����������

                    drZ = sc.lstAvailInZVK[sc.nCurAvail];

                    if (((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) 
                        || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL))
                        && (xCDoc.xDocP.nTypD != AppC.EMPTY_INT))
                    {// ��� ��������� ��������� ����� ���������� �� ������?
                        if (xPars.aDocPars[xCDoc.xDocP.nTypD].bShowFromZ)
                        {// ��, ����� �������� ������� ������������
                            if (nUnCondUsing != -1)
                                bUseZVK = true;
                        }
                        else
                        {
                            if (nUnCondUsing == 1)
                                bUseZVK = true;
                        }
                    }
                    if (bUseZVK)
                    {// ������� ������������

                        //if (IsEasyEdit())
                        //{// � ���������� �������� � ����� ������� ������

                        if ( IsEasyEdit()
                            || (xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM)
                            || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL) )
                        {// � ���������� ��������,..27-02-18..,  � ����� ������� ������

                            nMEd = (int)drZ["KOLM"];
                            fVEd = (FRACT)drZ["KOLE"];

                            //if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
                            //{
                            //    PrevKol(ref sc, ref nM, ref fV);
                            //}
                                
                            PrevKol(ref sc, ref nM, ref fV);
                            if ( ((nMEd > 0) && (nM > 0)) 
                                || ((nMEd == 0) && ((fVEd > 0) && (fV > 0))) )
                            {// ���� ��������� �������� �� ������
                                bWillBeEdit = true;
                            }

                            nMEd -= nM;
                            fVEd -= fV;
                        }
                        else
                        {// � ������� �������� �� ���� �������
                            nMEd = sc.nKolM_zvk - nM;
                            fVEd = sc.fKolE_zvk - fV;
                        }

                        if ((nMEd <= 0) && (fVEd <= 0))
                        {// �����-�� ������������ ��������� ���������� ��� ����� ����������
                            sc.nDocCtrlResult = AppC.RC_ALREADY;
                            sErr = "������ ��� ���������!";

                            DialogResult
                                dr = MessageBox.Show("�������� ���� (Enter)?\n(ESC) - ���������� ����", sErr,
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.OK)
                                return (AppC.RC_CANCEL);

                            // ��� ����� ������ �������
                            sc.nDest = NSI.DESTINPROD.USER;
                            bUseZVK = false;
                        }
                        if (bUseZVK)
                        {// ����� ����������� ��������������?
                            if (sc.bVes == true)
                            {// ��� ��������
                                // ������ ��������������� ���������� ���� � ����������� �� ����
                                bUseZVK = false;
                            }
                            else
                            {// ��� ��������
                                // ���-�� ���������� ��� ��������������� ����������?
                                //if ( ((nMEd > 0) && (fVEd == 0) && (sc.fEmk == 0)) )
                                //    bWillBeEdit = true;

                                if (!((nMEd > 0) && (fVEd == 0)))
                                    bWillBeEdit = true;

                                // 
                                if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                                {
                                }
                                else
                                {
                                    if (sc.nRecSrc == (int)NSI.SRCDET.SSCCT)
                                    {// ������ �� ��������� �� �������� �������, ������ �� ������������
                                        bUseZVK = false;
                                    }
                                }

                                bWillBeEdit |= VerifyMestByPoddon(ref sc, ref nMEd, ref fVEd);
                                nMEd = Math.Max(nMEd, 0);
                            }

                            if ((sc.nDest != NSI.DESTINPROD.TOTALZ) && (sc.nDest != NSI.DESTINPROD.USER))
                                sc.nDest = NSI.DESTINPROD.PARTZ;
                        }
                    }
                }
            }

            if (bUseZVK == false)
            {// ������ ����������� ��� �� ����������� �� ��������
                do
                {
                    if ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR_BUTTON))
                    {// �� ������� ���������� �������������� ��� �������� � ��������
                        nMEd = sc.nMest;
                        fVEd = sc.fVsego;
                        break;
                    }

                    if (sc.bVes == true)
                    {//  ��� �������� ������ ��� ����� ������
                            switch (sc.tTyp)
                            {
                                case AppC.TYP_TARA.TARA_POTREB:
                                    nMEd = 0;
                                    break;
                                case AppC.TYP_TARA.TARA_TRANSP:
                                    //if (sc.sKMC.StartsWith("70"))
                                    //    nMEd = sc.nMestPal;
                                    //if (nMEd <= 0)
                                    //    nMEd = 1;
                                    if (sc.sKMC.StartsWith("70"))
                                        nMEd = sc.nMestPal;
                                    else
                                        nMEd = 1;
                                    break;
                                case AppC.TYP_TARA.TARA_PODDON:
                                    nMEd = (sc.nMestPal > 0) ? sc.nMestPal : -1;
                                    break;
                            }
                    }
                    else
                    {// ��� �������� ������ ��� ����� ������

                        //if ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR))
                        //{// �� ������� ���������� ��������������
                        //    nMEd = sc.nMest;
                        //    fVEd = sc.fVsego;
                        //}

                        if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                        {// ����������� ���������� ��� ����-����������

                            // ���������� ������ � �������, ��� ����� ����� ������ ���������� (��������)
                            if ((sc.nRecSrc == (int)NSI.SRCDET.SCAN) || (sc.nRecSrc == (int)NSI.SRCDET.HANDS))
                            {
                                nMEd = sc.nMestPal;
                                fVEd = nMEd * sc.fEmk;
                            }
                        }
                        else { }// �� ��������� nMest = 0, fVsego=0
                    }
                } while (false);
            }


            if (sc.bVes)
                fVEd = sc.fVes;

            sc.nMest = nMEd;
            sc.fVsego = fVEd;

            return (nRet);
        }




        /// �������� ������������� ���������� ��� �������� ������
        public bool VerifyMestByPoddon(ref PSC_Types.ScDat sc, ref int nMZ, ref FRACT fVZ)
        {
            bool
                bWillBeEdit = false;
            int
                nEmkPal,
                nM = nMZ;
            FRACT
                fV = fVZ;
            do
            {
                if (xCDoc.xDocP.nTypD == AppC.TYPD_INV)
                    // ��� �������������� �������� ���
                    break;

                try
                {
                    nEmkPal = ((StrAndInt)sc.xEmks.Current).IntCode;
                }
                catch
                {
                    nEmkPal = 0;
                }

                if ((nEmkPal > 0) && (nMZ > 0))
                {// ���� �� ������� ��������
                    if ((nMZ > nEmkPal) && (!sc.bVes))
                    {// ������ ������ �������
                        if (!xPars.aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon)
                        {// ������ ������������ �������
                            nM = nMZ % nEmkPal;
                            if (nM == 0)
                                nM = nEmkPal;
                        }
                        else
                        {// ������ ������������ ������ �������
                            nM = nEmkPal;
                        }
                    }
                }

                // ���������� ������ � �������, ��� ����� ����� ������ ���������� (��������)
                if ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR_BUTTON))
                {// �� ������� ���������� ��������������
                    if (sc.nMest <= nM)
                    {
                        nM = sc.nMest;
                        fV = sc.fVsego;
                        break;
                    }
                }

                if ((nM != 0) && (sc.fEmk != 0))
                    fV = nM * sc.fEmk;

            } while (false);

            if (nM != nMZ)
                bWillBeEdit = true;

            nMZ = nM;
            fVZ = fV;

            return (bWillBeEdit);
        }




        private int FindSSCCInZVK(ScanVarGP xSc, ref PSC_Types.ScDat sc)
        {
            int nRet = AppC.RC_NOSSCC;

            sc.ZeroZEvals();
            if (bZVKPresent)
            {
                if (xCDoc.xDocP.TypOper == AppC.TYPOP_OTGR)
                {
                    DataView dv;
                    string sF = "(SYSN={0})AND(" +
                        ( ((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) > 0) ? "SSCC" : "SSCCINT") + "='{1}')";
                    sF = String.Format(sF, xCDoc.nId, xSc.Dat);
                    dv = new DataView(xNSI.DT[NSI.BD_DIND].dt,
                        sF, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);
                    if (dv.Count > 0)
                    {
                        sc.drTotKey = dv[0].Row;
                        sc.lstAvailInZVK.Add(sc.drTotKey);
                        sc.nCurAvail = 0;
                        nRet = AppC.RC_OK;
                    }
                }
            }
            return (nRet);
        }





        // ���������� ��������� "��������"-"�������" ��� ������� �������� scCur (���������-�������-����-������)
        private bool TryEvalNewZVKTTN(ref PSC_Types.ScDat scD, bool bUpdateGUI)
        {
            bool 
                bRet = AppC.RC_CANCELB,
                bDopValuesNeed = false;
            int
                nRegZVKUsing = 0;

            if (AppC.RC_OK == EvalZVKMest(ref scD, null, 0, 0))
            {
                if (!bUpdateGUI)
                    nRegZVKUsing = -1;

                if (AppC.RC_OK == Prep4Ed(ref scD, ref bDopValuesNeed, nRegZVKUsing))
                {
                    //if (bUpdateGUI)
                    //    VerifyMestByPoddon(ref scD);

                    PInfReady();
                    if (bUpdateGUI)
                    {
                        ShowDopInfKMPL(scD.nKolM_alr + scD.nKolM_alrT);
                        SetDopFieldsForEnter(false, true);
                    }
                    bRet = AppC.RC_OKB;
                }
            }
            return (bRet);
        }




        public bool PrintEtikPoddon(string sDop, string sSSCC, DataRow[] drPodd)
        {
            bool 
                bRePrint = false,
                bRet = AppC.RC_CANCELB;
            int 
                nPodd = 0,
                nNewPodd = 0,
                nRet = AppC.RC_OK;
            string 
                sTmp,
                sRf,
                sH = "",
                sErr = "";
            DataView
                dv;
            DataSet 
                dsTrans;
            DataRow[] 
                drD = null;
            ServerExchange 
                xSE = new ServerExchange(this);

            if ((sDop.Length == 0) && (sSSCC.Length == 0))
            {// ������ �� ������ � ������������ SSCC
                if ((xSm.CurPrinterMOBName.Length > 0) || (xSm.CurPrinterSTCName.Length > 0))
                {
                    sH = (xSm.CurPrinterMOBName.Length > 0) ? xSm.CurPrinterMOBName : xSm.CurPrinterSTCName;
                }
                else
                {
                    Srv.ErrorMsg("�������� �������", true);
                    return (bRet);
                }
            }

            if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
            {
                sRf = xCDoc.DefDetFilter() + String.Format(" AND (NPODDZ={0})", xCDoc.xNPs.Current);
                dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "", DataViewRowState.CurrentRows);
                if (dv.Count > 0)
                {
                    drD = new DataRow[dv.Count];
                    for (int i = 0; i < dv.Count; i++)
                        drD[i] = dv[i].Row;
                }
                else
                {
                    Srv.ErrorMsg("����������� ������", true);
                    return (bRet);
                }
            }
            else
            {// ��� ������������� ���������

                // ����� ������ ����� ����������/�����������
                sRf = xCDoc.DefDetFilter() + "AND(NPODDZ>0)";
                dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "NPODDZ DESC", DataViewRowState.CurrentRows);
                nPodd = (dv.Count > 0) ? (int)dv[0].Row["NPODDZ"] + 1 : nPodd = 1;

                // ������ ������ �������
                sRf = xCDoc.DefDetFilter() + "AND( (ISNULL(NPODDZ, -1)=-1)OR(NPODDZ=0) )";
                dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "", DataViewRowState.CurrentRows);
                if (dv.Count > 0)
                {
                    nNewPodd = nPodd;
                    drD = new DataRow[dv.Count];
                    for (int i = 0; i < dv.Count; i++)
                        drD[i] = dv[i].Row;
                    //for (int i = 0; i < drD.Length; i++)
                    //    drD[i]["NPODDZ"] = nPodd;
                    //tCurrPoddon.Text = nPodd.ToString();
                }
                else
                {// ����� ��������������� ���, ��������, ������� ����������� ������������
                    try
                    {
                        nPodd = (int)drDet["NPODDZ"];
                        sTmp = drDet["SSCC"].ToString();
                        if (sTmp.Length == 0)
                        {
                            if (sSSCC.Length > 0)
                                sTmp = sSSCC;
                        }
                        if ((nPodd > 0) && (sTmp.Length > 0))
                        {
                            sRf = String.Format("������ � {0}", nPodd);
                            DialogResult drPr = MessageBox.Show("����������� �������� (Enter)?\n(ESC) - ��������", sRf,
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (drPr == DialogResult.OK)
                            {
                                dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, xCDoc.DefDetFilter() + String.Format(" AND (NPODDZ={0})", nPodd), "", DataViewRowState.CurrentRows);
                                drD = new DataRow[dv.Count];
                                for (int i = 0; i < dv.Count; i++)
                                    drD[i] = dv[i].Row;
                                bRePrint = true;
                                sSSCC = sTmp;
                                sDop = String.Format("PAR=(SSCC={0},TYPOP=REPRINT);", sSSCC);
                            }
                            else
                            {
                                return (bRet);
                            }
                        }
                        else
                            nPodd = -1;
                    }
                    catch
                    {
                        nPodd = -1;
                    }
                    if (nPodd < 0)
                    {
                        Srv.ErrorMsg("����������� ������", true);
                        return (bRet);
                    }
                }
            }

            // �� ��������� - ��� ������������� � ��������
            if (drPodd == null)
                drPodd = drD;

                xCUpLoad = new CurUpLoad(xPars);
                xCUpLoad.sCurUplCommand = AppC.COM_PRNDOC;

                dsTrans = xNSI.MakeWorkDataSet(xNSI.DT[NSI.BD_DOCOUT].dt,
                          xNSI.DT[NSI.BD_DOUTD].dt, new DataRow[] { xCDoc.drCurRow }, drPodd, xSm, xCUpLoad);


                    sErr = xSE.ExchgSrv(AppC.COM_PRNDOC, sH, sDop, null, dsTrans, ref nRet, 20);
                    if ((nRet != AppC.RC_OK) || (xSE.ServerRet != AppC.RC_OK))
                    {
                        Srv.ErrorMsg(sErr, true);
                    }
                    else
                    {
                        sH = "";
                        if (xSE.ServerAnswer.ContainsKey("SSCC"))
                            sH = xSE.ServerAnswer["SSCC"];
                        else if (xSE.AnswerPars.ContainsKey("SSCC"))
                            sH = xSE.AnswerPars["SSCC"];
                        else if (sSSCC.Length > 0)
                            sH = sSSCC;

                        if (!bRePrint)
                        {
                            for (int i = 0; i < drPodd.Length; i++)
                            {
                                if (sH.Length > 0)
                                    drPodd[i]["SSCC"] = sH;
                                if (nNewPodd > 0)
                                    drPodd[i]["NPODDZ"] = nNewPodd;
                            }
                            if (nNewPodd > 0)
                                tCurrPoddon.Text = nNewPodd.ToString();
                        }
                        else
                        {
                            nNewPodd = nPodd;
                        }

                        bRet = AppC.RC_OKB;
                        AddSSCC2SSCCTable(sH, nNewPodd, xCDoc, drPodd.Length, 0, 1);
                        sErr = "������ ����������";
                        if (xCDoc.xDocP.TypOper != AppC.TYPOP_KMPL)
                        {// ��� ������������� ���������
                            if (sH.Length > 0)
                                sErr = "SSCC=" + sH + "\n" + sErr;
                            sH = String.Format("������ � {0}", nNewPodd);
                        }
                        else
                        {
                            sH = String.Format("������ � {0}", xCDoc.xNPs.Current);
                            xCDoc.xNPs.TryNext(true, true);
                        }
                        MessageBox.Show(sErr, sH);
                    }
                    Back2Main();
            return (bRet);
        }

        // �������� �� ������� ������������� �� ���������
        private int ServConfScan(ref PSC_Types.ScDat scD, ServerExchange xSE)
        {
            int 
                nRet = AppC.RC_OK;
            string 
                sErr = "";
            DataSet 
                dsTrans;
            DataRow[] 
                drD = null;

            //MakeTempDOUTD();

            drD = new DataRow[1] { xNSI.AddDet(scD, xCDoc, null, false) };
            // ID ������-������
            try
            {
                drD[0]["NPP_ZVK"] = scD.lstAvailInZVK[scD.nCurAvail]["NPP"];
            }
            catch
            {
                drD[0]["NPP_ZVK"] = -1;
            }

            xCUpLoad = new CurUpLoad(xPars);
            xCUpLoad.sCurUplCommand = AppC.COM_CHKSCAN;

            dsTrans = xNSI.MakeWorkDataSet(xNSI.DT[NSI.BD_DOCOUT].dt,
                      xNSI.DT[NSI.BD_DOUTD].dt, new DataRow[] { xCDoc.drCurRow }, drD, xSm, xCUpLoad);

            sErr = xSE.ExchgSrv(AppC.COM_CHKSCAN, "", "", null, dsTrans, ref nRet);
            if (xSE.ServerRet != AppC.EMPTY_INT)
            {// ����� �� ������� �������� �������
                nRet = xSE.ServerRet;
                if (nRet != AppC.RC_OK)
                {// � �� �������� �� �����-��...
                    //bRet = AppC.RC_CANCELB;
                    //Srv.ErrorMsg(sErr, true);
                }
            }

            //Back2Main();

            return (nRet);
        }

        private const string CONFSCAN = "ConfScan";
        // �������� �� ������� ������������� �� ���������
        public int ConfScanOrNot(DataRow dr, bool AppPar4ConfScan)
        {
            int
                t,
                nRet = 0;

            object 
                x;
            ExprDll.Action
                xFind;

            try
            {
                xFind = xGExpr.run.FindFunc(CONFSCAN);
                if (xFind != null)
                {
                    x = xGExpr.run.ExecFunc(CONFSCAN, new object[] { dr, AppPar4ConfScan });

                    if (x is int)
                        nRet = (int)x;
                    else
                        nRet = 0;
                }
                else
                {
                    if (AppPar4ConfScan)
                    {// ���������� ���� ������� �������������
                        if ((int)dr["TYPOP"] == AppC.TYPOP_DOCUM)
                        {
                            t = (int)dr["TD"];
                            if ((t >= 0) && (t <= 3))
                            {
                                nRet = 1;
                            }
                        }
                    }
                    else
                    {
                        nRet = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Srv.ErrorMsg(ex.Message);
                nRet = 0;
            }
            return (nRet);
        }


    }
}
