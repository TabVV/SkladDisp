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

        // накопленные количества по различным категориям заявки (ограничения по партиям - датам)
        // и соответствующие им количества из ТТН
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
                        // автоматический вызов привязки поддона для комплектации
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
                        //{// объект операции установлен
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
                sH = "Отгрузка запрещена!",
                sMess = "";
            bool
                b4biddScan = true;
                            
            if (nRet != AppC.RC_OK)
            {
                if (xSE.ServerRet != AppC.EMPTY_INT)
                {// Ответ от сервера получить удалось
                    try
                    {
                        sMess = xSE.ServerAnswer["MSG"];
                    }
                    catch
                    {
                        sMess = "Недопустимая продукция!";
                    }

                    if (xSE.ServerRet == AppC.RC_HALFOK)
                    {// сервер желает пообщаться
                        if (sMess.Length == 0)
                            sMess = "Недопустимая продукция!";
                        sMess += "\n(OK-отказ)\n(ESC-отгрузить)";

                        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                        DialogResult drQ = MessageBox.Show(sMess, sH,
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (drQ != DialogResult.OK)
                        {
                            b4biddScan = false;
                            nRet = AppC.RC_OK;
                        }
                        else
                        {// Enter - отказ от сканирования, выводить ничего не надо
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
                    sMess = "Сервер недоступен!";
                    sMess += "\n(OK-отказ)\n(ESC-продолжить ввод)";

                    DialogResult drQ = MessageBox.Show(sMess, "Ошибка!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (drQ != DialogResult.OK)
                    {
                        b4biddScan = false;
                        nRet = AppC.RC_OK;
                    }
                    else
                    {// Enter - отказ от сканирования, выводить ничего не надо
                    }

                }
            }





            return (nRet);
        }

        //private int ProceedProd(ScanVarGP xSc, ref PSC_Types.ScDat sc, bool bDupScan, bool bEasyEd, bool bNewBC)



        /// продукция на какой-то из наших этикеток
        private bool ProceedProd(ref PSC_Types.ScDat sc, bool bDupScan, bool bEasyEd, bool bNewBC)
        {
            int 
                nRet = AppC.RC_CANCEL;
            bool
                bNewDetAdded = false,
                bDopValuesNeed = true;
            DataRow
                drNewProd = null;


            #region Обработка скана продукции
            do
            {
                if (!CanSetOperObj())
                    break;

                xCDoc.bConfScan = (ConfScanOrNot(xCDoc.drCurRow, xPars.ConfScan) > 0) ? true : false;

                if (!bDupScan)
                {// первичный скан штрихкода, предыдущий был другой
                    if (xCDoc.xDocP.TypOper == AppC.TYPOP_MOVE)
                    {
                        if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() && 
                            (xSm.xAdrFix1 == null) )


                        //bool g = !xCDoc.xOper.IsFillSrc();
                        //g &= !xCDoc.xOper.IsFillDst();
                        //g &= (xSm.xAdrFix1 == null);
                        //if (g)
                        {
                            Srv.ErrorMsg("Адрес не указан!", true);
                            break;
                        }
                    }
                }
                else
                {// повторный скан того же штрихкода - это, как правило, подтверждение ввода
                    if ((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) && (!bEasyEd))
                    {
                        if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                        {
                            if (sc.nRecSrc == (int)NSI.SRCDET.SCAN)
                            {
                                Srv.ErrorMsg("Тот же поддон!", true);
                                break;
                            }
                        }
                    }
                }

                if (xCDoc.drCurRow != null)
                {
                    scCur = sc;
                    if (!sc.bFindNSI)
                        Srv.ErrorMsg("Код не найден! Обновите НСИ!", true);

                    bDopValuesNeed = true;
                    nRet = AppC.RC_OK;
                    if (scCur.bVes == true)
                    {
                        scCur.fVsego = scCur.fVes;
                        //FRACT fE = scCur.fVes;

                        // 11.07.14 взято в коммент
                        if (scCur.nRecSrc != (int)NSI.SRCDET.SSCCT)
                        {
                            if (!xScan.dicSc.ContainsKey("37") && (xScan.dicSc.Count == 4))
                            {
                                scCur.tTyp = AppC.TYP_TARA.TARA_POTREB;
                                scCur.fEmk = scCur.fEmk_s = 0;
                            }


                            if ((!bNewBC) || (!scCur.bEmkByITF))
                            {
                                // нужно по-другому
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
                        {// подтверждение ввода отключено
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
                            DialogResult dr = MessageBox.Show("Отменить ввод (Enter)?\r\n(ESC) - продолжить ввод",
                                String.Format("Тот же вес-{0}!", scCur.fVes), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.OK)
                                nRet = AppC.RC_CANCEL;
                        }
                        fDefVes = scCur.fVes;
                        if (scCur.ci == BCId.EAN13)
                        {
                            if (xCDoc.xDocP.nTypD == AppC.TYPD_ZKZ)
                            {// в заказы отгружаются только места !!!
                                nRet = AppC.RC_CANCEL;
                                Srv.ErrorMsg("Это код получателя!", true);
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
                            {// редактирование пока отменяется, но есть ли все данные?
                                bDopValuesNeed = (VerifyVvod().nRet == AppC.RC_CANCEL) ? true : false;
                                //int tR = VerifyVvod().nRet;
                            }

                            if (xCDoc.bConfScan)
                            {// документ требует подтверждения ввода для каждой продукции
                                //if ((sc.nRecSrc != (int)NSI.SRCDET.FROMADR) &&
                                //    (sc.nRecSrc != (int)NSI.SRCDET.SSCCT))
                                //    {// полученную с сервера не проверяем
                                //    if (TestProdBySrv(ref sc) != AppC.RC_OK)
                                //        break;
                                //}

                                // полученную с сервера ПОКА! проверяем
                                if (TestProdBySrv(ref scCur) != AppC.RC_OK)
                                    break;
                            }

                            if ((bDopValuesNeed == true))
                            {// будет редактирование
                                int nR = IsGeneralEdit(ref scCur);
                                if (nR == AppC.RC_OK)
                                    AddOrChangeDet(AppC.F_ADD_SCAN);
                                else if ((nR == AppC.RC_CANCEL) || (nR == AppC.RC_NOTALLDATA))
                                {
                                    //Srv.ErrorMsg("Неоднозначность!\r\nПерейдите в обычный режим!");
                                    ZVKeyDown(AppC.F_PODD, null, ref ehCurrFunc);
                                }
                                else if (nR == AppC.RC_BADTABLE)
                                {
                                    //Srv.ErrorMsg("Переключаюсь в заявку...\r\nПовторите сканирование");
                                    ChgDetTable(null, "");
                                }
                                else if (nR == AppC.RC_ZVKONLY)
                                {
                                    Srv.ErrorMsg("Только с заявкой!");
                                }
                            }
                            else
                            {
                                bNewDetAdded = AddDet1(ref scCur, out drNewProd);
                                SetDopFieldsForEnter(true);
                            }
                        }
                        else
                        {// отказ от сканирования, надо бы запись перечитать
                            ChangeDetRow(true);
                        }
                    }
                    else
                    {// отказ от сканирования, надо бы запись перечитать
                        ChangeDetRow(true);
                    }

                }
            } while (false);
            #endregion

            return (bNewDetAdded);
        }
        
        // количество мест по заявке
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
                            sErr = String.Format("Тот же вес-{0}!", sc.fVes);
                            if (sc.nKrKMC == nOldKrk)
                                bNeedAsk = true;
                            break;
                        case AppC.RC_NOEAN:
                            sErr = sc.nKrKMC.ToString() + "-нет в заявке!";
                            if (sc.nKrKMC == nOldKrk)
                            {// предыдущий раз об этом спрашивали ?
                                if (bAskKrk == true)
                                    bNeedAsk = false;
                            }
                            //if (IsEasyEdit())
                            //{
                            //}
                            break;
                        case AppC.RC_BADPARTY:
                            sErr = String.Format("Партии {0} нет в заявке!", sc.nParty);
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
                            //sErr = String.Format("Изг(заяв):{0}\nИзг(скан):{1}", dV,sc.sDataIzg);
                            //sH = String.Format("{0} / {1}", sc.nKrKMC, sc.fEmk);
                            sErr = sc.sErr;
                            bNeedAsk = true;
                            break;
                        case AppC.RC_NOEANEMK:
                            sErr = sc.nKrKMC.ToString() + "/" + sc.fEmk.ToString() + "-нет в заявке!";
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
                            sErr = "Снимите фильтр по поддону!";
                            break;
                        case AppC.RC_NOAUTO:
                            sErr = "Заявка не определилась!";
                            bNeedAsk = true;
                            break;
                        case AppC.RC_ALREADY:
                            sErr = "Заявка уже выполнена!";
                            bNeedAsk = true;
                            break;
                        default:
                            sErr = "Несоотвествие заявке!";
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
                                    sErr = "Требуется точная\n партия/дата!!!";
                                }
                            }
                        }
                    }

                    if (bNeedAsk == true)
                    {
                        DialogResult
                            dr = MessageBox.Show(sErr + ((sDopMsg.Length > 0)?"\n" + sDopMsg:"") + "\n\nОтменить ввод (Enter)?\n(ESC) - продолжить ввод",
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
                    {// Запроса не будет, только сообщение 
                        if (bInScanProceed || bEditMode)
                        {// обработка сканирования
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

        // fCurEmk - емкость в строке заявки
        private bool WhatTotKeyF_Emk(DataRow dr, ref PSC_Types.ScDat sc, FRACT fCurEmk)
        {
            bool bSet = false;
            if (fCurEmk == 0)
            {// в заявке - единицы
                if ((sc.fEmk == 0) || bEditMode)
                {// при контроле емкости должны совпасть точно
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
                //{// при контроле емкости должны совпасть точно
                //    if (sc.drPartKeyE == null)
                //    {
                //        sc.drPartKeyE = dr;
                //    }
                //    bSet = true;
                //}
                if ((sc.fEmk == 0) || (sc.nRecSrc != (int)NSI.SRCDET.CR4CTRL))
                {// при контроле емкости должны совпасть точно
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


        // строка с фильтром для установки на заявки и ТТН
        // при определении задания и уже сканированных
        private string FilterKompl(int nSys, string sKMC, bool bUsePoddon)
        {
            int nCurPoddon = 0;
            string ret = "(SYSN={0})AND(KMC='{1}')";

            if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
            {// для комплектации может понадобиться ограничение по поддону

                //if (xSm.FilterTTN == NSI.FILTRDET.NPODD)


                if (bUsePoddon)
                {// фильтр по поддону установлен
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

        // указано в заявке
        // Код возврата:
        // RC_NOEAN - кода нет в заявке, никакие данные не заполняются
        // RC_NOEANEMK - отдельных единиц кода нет в заявке, мест с такой емкостью тоже
        //               заполнено nKolM_zvk - всего каких-то мест
        // RC_OK - есть или единички (fKolE_zvk != 0, тогда возможны drPartKeyE != null drTotKeyE != null) 
        //         или места (nKolM_zvk != 0, тогда возможны drPartKey != null drTotKey != null)

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

            // фильтр - SYSN + KMC
            if (bInScanProceed || bEditMode)
            {// вызов при вводе данных после сканирования
                nMaxR = 0;
                if (bEvalInPall) 
                {// по текущему поддону
                    sc.sFilt4View = FilterKompl(xCDoc.nId, sc.sKMC, true);
                    dv = new DataView(xNSI.DT[NSI.BD_DIND].dt,
                        sc.sFilt4View, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);
                    nMaxR = dv.Count;
                    if (nMaxR > 0)
                    {// если есть незакрытые - работаем с поддоном, иначе - весь документ
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
                    {// по текущему поддону ничего не нашлось
                        if (dv.Count > 0)
                        {// а вот для других имеется
                            if (xSm.FilterTTN == NSI.FILTRDET.NPODD)
                            {// другие поддоны мы попросту не увидим
                                return (AppC.RC_UNVISPOD);
                            }
                            else
                            {// другие поддоны мы увидим, но спросить надо
                                nRet = AppC.RC_BADPODD;
                            }
                        }
                        else
                        {// по другим поддонам также ничего нет
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
                {// емкость не указана (заявлены отдельные единицы продукции)
                    sc.fKolE_zvk += (FRACT)dr["KOLE"];
                }
                else
                {// емкость присутствует в заявке, фиксируем места
                    nM = (int)dr["KOLM"];
                    nMest += nM;
                    if (fCurEmk == sc.fEmk)
                    {// в скан-данных емкость такая же и ненулевая
                        nMestEmk += nM;
                    }
                }

                // что-то из заявки закрывается этим сканированием?
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
            } // основной цикл


            if (nMaxR > 0)
            {// какие-то строки все-таки были
                if (sc.fEmk == 0)
                {// емкость на основании скан-данных определить не удалось
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
                            break;                                          // работаем только с одной строкой
                    }
                    nRet = AppC.RC_OK;
                }
                else
                {// искать незакрытые пытались - ничего не вышло
                    nRet = nRFind;
                }

                if (nRet == AppC.RC_OK)
                {// пока еще все в порядке
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


        /// подходит ли уже отсканированная продукция к заявке?
        /// dr - строка заявки
        //private bool CmpFromTTN2ZVK_(int nCond, FRACT fEmk, DateTime dDat, string nP,
        //    FRACT fEmk_TTN, DateTime dDat_TTN, string sParty_TTN)
        //{
        //    bool bMayUsed = false;

        //    #region Сопоставление строки заявки и текущих данных в списке ТТН
        //    do
        //    {
        //        if (fEmk == fEmk_TTN)
        //        {
        //            if (nCond != (int)NSI.SPECCOND.NO)
        //            {// есть ограничения про дате или партии
        //                if (nCond == (int)NSI.SPECCOND.PARTY_SET)
        //                {// указана конкретная партия от xx/yy/zz
        //                    if ((nP == sParty_TTN) && (dDat == dDat_TTN))
        //                    {// точное совпадение ключа
        //                        bMayUsed = true;
        //                    }
        //                    break;
        //                }
        //                if (dDat_TTN >= dDat)
        //                {// по сроку годности проходит
        //                    bMayUsed = true;
        //                    break;
        //                }
        //            }
        //            else
        //            {// общий случай, закрывается любой партией или датой
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


        // подходит ли уже отсканированная продукция к заявке?
        // dr - строка заявки
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

            #region Сопоставление строки заявки и текущих данных в списке ТТН
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
                    {// есть ограничения про дате или партии
                        if (nCond == (int)NSI.SPECCOND.PARTY_SET)
                        {// указана конкретная партия от xx/yy/zz
                            if ((sParty_ZVK == sParty_TTN) && (dDat_ZVK == dDat_TTN))
                            {// точное совпадение ключа
                                bMayUsed = true;
                            }
                            break;
                        }
                        if (dDat_TTN >= dDat_ZVK)
                        {// по сроку годности проходит
                            bMayUsed = true;
                            break;
                        }
                    }
                    else
                    {// общий случай, закрывается любой партией или датой
                        bMayUsed = true;
                        break;
                    }
                }
                if (++iZ >= sc.lstAvailInZVK.Count)
                    bTryNext = false;
            } while (!bMayUsed && bTryNext);

            #endregion

            if (bMayUsed)
                // выход по break - всегда!!!
                sc.nCurAvail = iZ;
            //{
            //    int k = 12 * 6;
            //    bMayUsed = false;
            //}
            return (bMayUsed);
        }




        // обработка списка ТТН
        // сколько уже отсканировано
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

                // отсканировано мест с точным совпадением ключа
                nKolM_alrT = 0,
                // отсканировано мест с допустимым совпадением ключа
                nKolM_alr = 0;

            DateTime
                dDVyr;
                //dDVyr_ZVK = DateTime.Now;

            FRACT 
                fEm = 0,
                fEmk_ZVK = 0,

                fV = 0,
            
                // отсканировано единиц с точным совпадением ключа
                fKolE_alrT = 0,
                // отсканировано единиц с допустимым совпадением ключа
                fKolE_alr = 0;

            //NSI.DESTINPROD 
            //    desProd;
            DataRow
                dr,
                drZ = null;

            // всего мест по коду отсканировано
            nM_A = 0;
            // всего единиц по коду отсканировано
            fE_A = 0;

            // пока никуда ничего не суммируем
            sc.drEd = sc.drMest = null;

            bInEasy = IsEasyEdit();

            if ((sc.lstAvailInZVK.Count > 0) && (sc.nCurAvail >= 0))
            {// существует заявка для данного скана (работаем с одной из возможных строк)
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

            // фильтр - SYSN + KMC [+ № поддона]
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

                // уже есть точно такая же в списке ТТН?
                bSame = ((nParty == sc.nParty) && (dDVyr.Date == sc.dDataIzg.Date)) ? true : false;

                // но в некоторых случаях такой же код не суммируется
                //if (bInScanProceed || bEditMode)
                //{
                //    if ((bInEasy) || (!bSame))
                //    //if (bInEasy)
                //    {
                //        bTry2FindSimilar = CmpFromTTN2ZVK(nCond, fEmk_ZVK, dDVyr_ZVK, nParty_ZVK, fEm, dDVyr, nParty);
                //    }
                //}

                
                if (bUsingZVK)
                {// включить текущую строку из ТТН в список подходящих?
                    //bTry2FindSimilar = CmpFromTTN2ZVK(nCond, fEmk_ZVK, dDVyr_ZVK, nParty_ZVK, fEm, dDVyr, nParty);
                    bTry2FindSimilar = CmpFromTTN2ZVK(ref sc, fEm, dDVyr, nParty);
                }
                else
                    bTry2FindSimilar = true;

                if (bTry2FindSimilar)
                {
                    if (fEm == 0)
                    {// отсканированные единицы

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
                                {// в сформированные поддоны не суммируем
                                    sc.drEd = dr;
                                }
                            }
                        }
                        else
                        {// единицы из любой партии
                            fKolE_alr += fV;
                            if (bDocControl)
                                dr["DEST"] = NSI.DESTINPROD.PARTZ;
                        }
                    }
                    else
                    {// отсканированные места
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
                                    {// в сформированные поддоны не суммируем
                                        sc.drMest = dr;
                                    }
                                }
                            }
                            else
                            {// закрывают общую часть
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
                
        // расчет оставшегося для ввода количества
        // nNonCondUsing = -1 - не использовать заявку, даже если есть
        // nNonCondUsing =  1 - использовать заявку, даже если не требуется
        private int Prep4Ed(ref PSC_Types.ScDat sc, ref bool bWillBeEdit, int nUnCondUsing)
        {
            int
                nM = 0,                 // расчитано количество мест, уже отсканированных в ТТН
                nMEd = 0,               // предлагать количество мест для редактирования/подтверждения
                nRet = AppC.RC_OK;
            bool
                bUseZVK = false;        // с заявкой что-то не сраслось, все по умолчанию

            string
                sErr;

            FRACT
                fVEd = 0,               // предлагать количество единиц для редактирования/подтверждения
                fV = 0;                 // расчитано количество единиц , уже отсканированных в ТТН


            DataRow 
                drZ;
            DataView 
                dv4Sum = null;

            if (xCDoc.xDocP.nTypD == AppC.TYPD_OPR)
            {// операции обрабатываются отдельно

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
            {// заявка имеется
                if (sc.nCurAvail >= 0)
                {// в заявке найдены подходящие

                    drZ = sc.lstAvailInZVK[sc.nCurAvail];

                    if (((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) 
                        || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL))
                        && (xCDoc.xDocP.nTypD != AppC.EMPTY_INT))
                    {// для документа требуется брать количество из заявки?
                        if (xPars.aDocPars[xCDoc.xDocP.nTypD].bShowFromZ)
                        {// да, общий параметр требует использовать
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
                    {// пробуем использовать

                        //if (IsEasyEdit())
                        //{// в упрощенном работаем с одной строкой заявки

                        if ( IsEasyEdit()
                            || (xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM)
                            || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL) )
                        {// в упрощенном работаем,..27-02-18..,  с одной строкой заявки

                            nMEd = (int)drZ["KOLM"];
                            fVEd = (FRACT)drZ["KOLE"];

                            //if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
                            //{
                            //    PrevKol(ref sc, ref nM, ref fV);
                            //}
                                
                            PrevKol(ref sc, ref nM, ref fV);
                            if ( ((nMEd > 0) && (nM > 0)) 
                                || ((nMEd == 0) && ((fVEd > 0) && (fV > 0))) )
                            {// была частичная отгрузка по строке
                                bWillBeEdit = true;
                            }

                            nMEd -= nM;
                            fVEd -= fV;
                        }
                        else
                        {// в обычном работаем со всей заявкой
                            nMEd = sc.nKolM_zvk - nM;
                            fVEd = sc.fKolE_zvk - fV;
                        }

                        if ((nMEd <= 0) && (fVEd <= 0))
                        {// какие-то неправильные расчетные количества для ввода получаются
                            sc.nDocCtrlResult = AppC.RC_ALREADY;
                            sErr = "Заявка уже выполнена!";

                            DialogResult
                                dr = MessageBox.Show("Отменить ввод (Enter)?\n(ESC) - продолжить ввод", sErr,
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.OK)
                                return (AppC.RC_CANCEL);

                            // все равно решилл вводить
                            sc.nDest = NSI.DESTINPROD.USER;
                            bUseZVK = false;
                        }
                        if (bUseZVK)
                        {// когда понадобится редактирование?
                            if (sc.bVes == true)
                            {// для весового
                                // просто устанавливается количество мест в зависимости от тары
                                bUseZVK = false;
                            }
                            else
                            {// для штучного
                                // что-то загадочное для автоматического добавления?
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
                                    {// похоже на продукцию из сборного поддона, ничего не корректируем
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
            {// заявка отсутствует или не принимается во внимание
                do
                {
                    if ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR_BUTTON))
                    {// на поддоне количества подтверждаются для штучного и весового
                        nMEd = sc.nMest;
                        fVEd = sc.fVsego;
                        break;
                    }

                    if (sc.bVes == true)
                    {//  для весового товара без учета заявки
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
                    {// для штучного товара без учета заявки

                        //if ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR))
                        //{// на поддоне количества подтверждаются
                        //    nMEd = sc.nMest;
                        //    fVEd = sc.fVsego;
                        //}

                        if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
                        {// расчитанное пригодится при авто-добавлении

                            // количества пришли с сервера, при вводе могут только уменьшится (возможно)
                            if ((sc.nRecSrc == (int)NSI.SRCDET.SCAN) || (sc.nRecSrc == (int)NSI.SRCDET.HANDS))
                            {
                                nMEd = sc.nMestPal;
                                fVEd = nMEd * sc.fEmk;
                            }
                        }
                        else { }// по умолчанию nMest = 0, fVsego=0
                    }
                } while (false);
            }


            if (sc.bVes)
                fVEd = sc.fVes;

            sc.nMest = nMEd;
            sc.fVsego = fVEd;

            return (nRet);
        }




        /// контроль предлагаемого количества для штучного товара
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
                    // для инвентаризации контроля нет
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
                {// мест на паллете известно
                    if ((nMZ > nEmkPal) && (!sc.bVes))
                    {// заявка больше паллеты
                        if (!xPars.aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon)
                        {// первым предлагается остаток
                            nM = nMZ % nEmkPal;
                            if (nM == 0)
                                nM = nEmkPal;
                        }
                        else
                        {// первым предлагается полная паллета
                            nM = nEmkPal;
                        }
                    }
                }

                // количества пришли с сервера, при вводе могут только уменьшится (возможно)
                if ((sc.nRecSrc == (int)NSI.SRCDET.SSCCT) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR) || (sc.nRecSrc == (int)NSI.SRCDET.FROMADR_BUTTON))
                {// на поддоне количества подтверждаются
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





        // перерасчет количеств "заявлено"-"собрано" для текущих значений scCur (продукция-емкость-дата-партия)
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
            {// запрос на печать и формирование SSCC
                if ((xSm.CurPrinterMOBName.Length > 0) || (xSm.CurPrinterSTCName.Length > 0))
                {
                    sH = (xSm.CurPrinterMOBName.Length > 0) ? xSm.CurPrinterMOBName : xSm.CurPrinterSTCName;
                }
                else
                {
                    Srv.ErrorMsg("Выберите принтер", true);
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
                    Srv.ErrorMsg("Отсутствуют данные", true);
                    return (bRet);
                }
            }
            else
            {// для произвольного документа

                // какой поддон будет проставлен/сформирован
                sRf = xCDoc.DefDetFilter() + "AND(NPODDZ>0)";
                dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "NPODDZ DESC", DataViewRowState.CurrentRows);
                nPodd = (dv.Count > 0) ? (int)dv[0].Row["NPODDZ"] + 1 : nPodd = 1;

                // состав нового поддона
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
                {// вновь отсканированных нет, возможно, попытка распечатать существующий
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
                            sRf = String.Format("Поддон № {0}", nPodd);
                            DialogResult drPr = MessageBox.Show("Распечатать повторно (Enter)?\n(ESC) - отменить", sRf,
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
                        Srv.ErrorMsg("Отсутствуют данные", true);
                        return (bRet);
                    }
                }
            }

            // по умолчанию - все непривязанные к поддонам
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
                        sErr = "Данные отправлены";
                        if (xCDoc.xDocP.TypOper != AppC.TYPOP_KMPL)
                        {// для произвольного документа
                            if (sH.Length > 0)
                                sErr = "SSCC=" + sH + "\n" + sErr;
                            sH = String.Format("Поддон № {0}", nNewPodd);
                        }
                        else
                        {
                            sH = String.Format("Поддон № {0}", xCDoc.xNPs.Current);
                            xCDoc.xNPs.TryNext(true, true);
                        }
                        MessageBox.Show(sErr, sH);
                    }
                    Back2Main();
            return (bRet);
        }

        // получить от сервера подтверждение на продукцию
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
            // ID строки-заявки
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
            {// Ответ от сервера получить удалось
                nRet = xSE.ServerRet;
                if (nRet != AppC.RC_OK)
                {// И он оказался не очень-то...
                    //bRet = AppC.RC_CANCELB;
                    //Srv.ErrorMsg(sErr, true);
                }
            }

            //Back2Main();

            return (nRet);
        }

        private const string CONFSCAN = "ConfScan";
        // получить от сервера подтверждение на продукцию
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
                    {// установлен флаг запроса подтверждения
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
