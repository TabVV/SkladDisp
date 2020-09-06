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
            OBJECT = 1,                    // объект
            STELLAGE = 2,                    // стеллаж
            ZONE = 4                     // зона
        }

        // информация по адресу для операции
        public class AddrInfo
        {
            // структура: ХХ-камера, ХХХ-место хранения, ХХ-канал, Х-ярус, Х-номер поддона
            private string m_FullAddr = "";           // адрес ячейки-зоны

            public string sMesto = "";           // адрес ячейки-зоны
            public string sCanal = "";           // адрес ячейки-зоны
            public string sYarus = "";           // адрес ячейки-зоны

            public string sName = "";           // наименование ячейки-зоны
            public string sCat = "";            // категория ячейки-зоны

            public bool bFixed = false;         // адрес зафиксирован

            public ADR_TYPE nType = ADR_TYPE.UNKNOWN;         // тип адреса

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


            // строка адреса
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
            // символьное отображение адреса
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

        // обработка выполненного сканирования
        private void OnScan(object sender, BarcodeScannerEventArgs e)
        {
            bool 
                bRet = AppC.RC_CANCELB,
                bEasyEd,
                bDupScan;
            int nRet = AppC.RC_CANCEL;
            string sErr = "";

            // началась обработка сканирования
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

                    #region Обработка скана
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
                                {// подтверждение операции иногда допустимо
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
                                //{// вызывалась функция фиксации адреса
                                //    nSpecAdrWait = 0;
                                //    xFPan.HideP();
                                //    if (xSm.xAdrFix1 != null)
                                //    {
                                //        sErr = String.Format("Фиксированный {0}\n адрес сброшен...", xSm.xAdrFix1.Addr);
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
                                {// должна быть продукция
                                    if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_OLD_ETIK) == ScanVarGP.BCTyp.SP_OLD_ETIK) ||
                                         (xScan.Id != BCId.Code128))
                                    {// старая этикетка или EAN13
                                        bRet = TranslSCode(ref sc);
                                    }
                                    else
                                    {// новая этикетка
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
                                            // разбор этикетки стандартным способом по AI
                                            bRet = NewTranslSCode(ref sc);
                                    }
                                }

                                if (!bRet)
                                    throw new Exception("Ошибка сканирования!");

                                if (xPars.WarnNewScan == true)
                                {// предупреждение для неоконченного ввода
                                    if ((bInEasyEditWait && !bDupScan) || (bEditMode == true))
                                    {
                                        Srv.ErrorMsg("Закончите ввод!", true);
                                        break;
                                    }
                                }

                                bEasyEd = IsEasyEdit();
                                if (bEasyEd)
                                {// для режима упрощенного ввода
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
                    Srv.ErrorMsg(sE + "\n" + ex.Message, "Ошибка сканирования", true);
                }
            }
            // обработка сканирования окончена
            bInScanProceed = false;
            ResetTimerReLogon(true);
        }

        // проверка содержимого ячейки после скана продукции
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
                    {// операция выгрузки не прошла на сервере (содержательная ошибка)
                        Srv.ErrorMsg(sL, "Ошибка размещения", true);
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
                        {// выгрузка по кнопочке
                            drDet["STATE"] = AppC.OPR_STATE.OPR_OVER;
                            xCUpLoad = new CurUpLoad(xPars);
                            xDP = xCUpLoad.xLP;

                            xCUpLoad.bOnlyCurRow = true;
                            xCUpLoad.drForUpl = drDet;
                            //xFPan = new FuncPanel(this, this.pnLoadDocG);
                            //EditOverBeforeUpLoad(AppC.RC_OK, 0);

                            if (xPars.OpAutoUpl)
                            {// авто-выгрузка операций
                                string sL = UpLoadDoc(xSE, ref nRet);
                                if (xSE.ServerRet == AppC.RC_OK)
                                    xCDoc.xOper = new CurOper();

                                if (nRet != AppC.RC_OK)
                                {
                                    if (nRet == AppC.RC_HALFOK)
                                    {
                                        Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                                        MessageBox.Show(sL, "Предупреждение!");
                                    }
                                    else
                                        Srv.ErrorMsg(sL, true);
                                }

                                if ((xSE.ServerRet != AppC.EMPTY_INT) &&
                                    (xSE.ServerRet != AppC.RC_OK))
                                {// операция выгрузки не прошла на сервере (содержательная ошибка)
                                if (xSE.ServerRet == 99)
                                    CompareAddrs(xCDoc.xOper.xAdrDst.Addr, String.Format("---{0}-----------После выгрузки", xSE.ServerRet), true);
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
            {// Добавление группы строк (скомплектованный поддон)
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
            {// дальнейшую обработку сканирования прекращаем
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
            {// действительно присутствовал в заявке
                dr = AddDetSSCC(xSc, xCDoc.nId, xT, "");
                if (dr != null)
                    drDet = dr;
            }
            else
            {// в заявке нету, надо лезть на сервер
                ret = ConvertSSCC2Lst(xSE, xSc, ref scD, true);
                if (ret == AppC.RC_OK)
                {// это один код, дальше обычные проверки
                    ret = AppC.RC_WARN;
                }
                else
                {
                    AddGroupDet(ret);
                    // в любом случае обработку скана заканчиваем
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
        //    {// вызывалась функция фиксации/сброса адреса
        //        nSpecAdrWait = 0;
        //        xFPan.HideP();
        //        // дальше клавиши не обрабатываю
        //        ehCurrFunc -= Keys4FixAddr;

        //        if (xSm.xAdrFix1 != null)
        //        {
        //            sErr = "Фиксированный адрес сброшен...";
        //            xSm.xAdrFix1 = null;
        //        }
        //        else
        //        {
        //            xSm.xAdrFix1 = new AddrInfo(scD.sN);
        //            xSm.xAdrFix1.sName = xNSI.AdrName(scD.sN);
        //            xSm.xAdrFix1.nType = (xScan.bcFlags == ScanVarGP.BCTyp.SP_ADR_OBJ) ? ADR_TYPE.OBJECT : ADR_TYPE.ZONE;
        //            sErr = "Адрес зафиксирован...";
        //        }
        //        sH = xSm.xAdrFix1.sName;
        //        Srv.ErrorMsg(sErr, sH, true);
        //        lDocInf.Text = CurDocInf(xCDoc.xDocP);
        //    }
        //    return (nRet);
        //}


        // обработка полученного адреса
        private int ProceedAdr(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        {
            int 
                ret = AppC.RC_OK,
                nUseAdr = 0; // 1-From, 2-To

            // значение адреса
            scD.sN = xSc.Dat.Substring(2);

            if (xCDoc.nTypOp == AppC.TYPOP_DOCUM)
            {
                DialogResult dRez = MessageBox.Show(
                    "Вывод на экран (Enter)?\n(ESC)- добавить строки", "Содержимое адреса",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                //DialogResult dRez = (xCDoc.xDocP.nTypD != AppC.TYPD_INV) ? DialogResult.OK :
                //    MessageBox.Show("Вывод на экран (Enter)?\n(ESC)- добавить строки", "Содержимое адреса",
                //    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                string sTypeSPR = (dRez == DialogResult.OK)?"TXT":"ROW";
                ConvertAdr2Lst(scD.sN, sTypeSPR);
            }

            if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_ZONE) == ScanVarGP.BCTyp.SP_ADR_ZONE) ||
                 ((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) == ScanVarGP.BCTyp.SP_ADR_OBJ))
                {// Адрес зоны или объекта
                if (nSpecAdrWait > 0)
                {// вызывалась функция фиксации адреса
                    nSpecAdrWait = 0;
                    xFPan.HideP();
                    // дальше клавиши не обрабатываю
                    ehCurrFunc -= Keys4FixAddr;

                    xSm.xAdrFix1 = new AddrInfo(scD.sN);
                    xSm.xAdrFix1.sName = xNSI.AdrName(scD.sN);
                    xSm.xAdrFix1.nType = (xScan.bcFlags == ScanVarGP.BCTyp.SP_ADR_OBJ) ? ADR_TYPE.OBJECT : ADR_TYPE.ZONE;
                    Srv.ErrorMsg("Адрес зафиксирован...", xSm.xAdrFix1.sName, true);
                    lDocInf.Text = CurDocInf(xCDoc.xDocP);
                    return (ret);
                }
            }

            if (xSm.xAdrFix1 != null)
            {// зафиксирован адрес, пришел еще один
                if (!xCDoc.xOper.bObjOperScanned)
                {// поддон еще не сканировался, сейчас пришел адрес отправителя
                    nUseAdr = 1;
                    xCDoc.xOper.xAdrDst = xSm.xAdrFix1;
                }
                else
                {// поддон уже сканировался, сейчас пришел адрес получателя
                    nUseAdr = 2;
                    xCDoc.xOper.xAdrSrc = xSm.xAdrFix1;
                }
            }
            else
            {// фиксированных адресов пока не было

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
                            DialogResult drQ = MessageBox.Show("Изменить \"ИЗточник\" (Enter)?\n(ESC) - отмена",
                                "Снова адрес!",
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
            {// отсканированным адресом следует воспользоваться  как адресом отправителя или получателя
                AddrInfo xA = new AddrInfo(scD.sN, xNSI.AdrName(scD.sN), this);

                if (nUseAdr == 1)
                {// это источник
                    if (xCDoc.xOper.GetDst(false) == xA.Addr)
                        xA = null;
                    else
                        xCDoc.xOper.xAdrSrc = xA;
                }
                else
                {// это приемник
                    if (xCDoc.xOper.GetSrc(false) == xA.Addr)
                        xA = null;
                    else
                        xCDoc.xOper.xAdrDst = xA;
                }
                if (xA == null)
                    Srv.ErrorMsg("Адреса совпадают...", scD.sN, true);
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
                swT.WriteLine(String.Format("{3} Scanned***{0}***---Приемник***{1}***===={2}", xScan.Dat, sA, sReason, DateTime.Now.ToString("dd.MM.yy hh:mm:ss")));
                swT.Close();
            }
        }


        // обработка SSCC при вводе
        //int ProceedSSCC(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        //{
        //    int ret = AppC.RC_OK;
        //    bool bMaySet = true;
        //    DataRow dr;
        //    RowObj xR;
        //    DialogResult dRez;

        //    if ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) == ScanVarGP.BCTyp.SP_SSCC_EXT)
        //    {// Внешние SSCC
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
        //                            String.Format("SSCC={0}\nОтменить (Enter)?\n(ESC)-изменить SSCC", xR.sSSCC),
        //                            "Уже маркирован!", MessageBoxButtons.OKCancel,
        //                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        //                        bMaySet = (dRez == DialogResult.OK) ? false : true;
        //                    }
        //                    //if ((AppC.OPR_STATE)drDet["STATE"] == AppC.OPR_STATE.OPR_UPL)
        //                    //{// уже выгрузили
        //                    //    dRez = MessageBox.Show(
        //                    //        String.Format("SSCC={0}\nОтменить (Enter)?\n(ESC)-проставить SSCC", xR.SName),
        //                    //        "Уже выгружался!", MessageBoxButtons.OKCancel,
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
        //                // операция перемещения
        //                if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() &&
        //                    (xSm.xAdrFix1 == null))
        //                {
        //                    Srv.ErrorMsg("Адрес не указан!", true);
        //                    break;
        //                }

        //                ret = ConvertSSCC2Lst(xSc, ref scD, false);
        //                if (ret == AppC.RC_OK)
        //                {
        //                    if (xCLoad.dtZ.Rows.Count == 1)
        //                    {// однородный поддон
        //                        dr = xNSI.AddDet(scD, xCDoc, null);
        //                        if (dr != null)
        //                        {
        //                            dr["SSCC"] = xSc.Dat;
        //                            xCDoc.xOper.bObjOperScanned = true;
        //                        }
        //                    }
        //                    else
        //                    {// сборный поддон идет одной строкой
        //                        dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
        //                    }
        //                }
        //                else
        //                {// получить расшифровку SSCC от сервера не удалось
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
        //                    {// продолжается обычная обработка скана
        //                        ret = AppC.RC_WARN;
        //                    }
        //                }
        //                else
        //                {
        //                    ret = AddGroupDet(ref scD, ret);
        //                    // в любом случае обработку скана заканчиваем
        //                    ret = AppC.RC_OK;
        //                }
        //                break;
        //            case AppC.TYPOP_OTGR:
        //                ret = SSCC4OTG(xSc, ref scD, ScanVarGP.BCTyp.SP_SSCC_EXT);
        //                //ret = FindSSCCInZVK(xSc, ref scD);
        //                //if (ret == AppC.RC_OK)
        //                //{// действительно присутствовал в заявке
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
        //    {// Внутренний SSCC (сборный поддон)
        //        switch (xCDoc.nTypOp)
        //        {
        //            case AppC.TYPOP_MARK:
        //                // будет маркировка сборного поддона
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
        //                    {// продолжается обычная обработка скана
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

        // обработка SSCC при вводе
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


            // Внешние SSCC
            bExt = ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) == ScanVarGP.BCTyp.SP_SSCC_EXT) ? true : false;

            switch (xCDoc.nTypOp)
            {
                case AppC.TYPOP_PRMK:
                case AppC.TYPOP_MARK:
                    if (!bExt && (xCDoc.nTypOp == AppC.TYPOP_MARK))
                    {
                        // будет маркировка сборного поддона
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
                                String.Format("SSCC={0}\nОтменить (Enter)?\n(ESC)-изменить SSCC", xR.sSSCC),
                                "Уже маркирован!", MessageBoxButtons.OKCancel,
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
                    // операция перемещения
                    if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() &&
                        (xSm.xAdrFix1 == null))
                    {
                        Srv.ErrorMsg("Адрес не указан!", true);
                        break;
                    }
                    ret = ConvertSSCC2Lst(xSE, xSc, ref scD, true);
                    if (ret == AppC.RC_OK)
                    {
                        if (xCLoad.dtZ.Rows.Count == 1)
                        {// однородный поддон
                            //dr = xNSI.AddDet(scD, xCDoc, null);
                            AddDet1(ref scD, out dr);
                            if (dr != null)
                            {
                                dr["SSCC"] = xSc.Dat;
                                xCDoc.xOper.bObjOperScanned = true;
                            }
                        }
                        else
                        {// сборный поддон идет одной строкой
                            dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
                        }
                    }
                    else
                    {// получить расшифровку SSCC от сервера не удалось
                        if ((xSE.ServerRet == AppC.EMPTY_INT) ||
                            (xSE.ServerRet == AppC.RC_OK))
                        {// но это не ошибка на сервере, возможно, сетевая ошибка
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
                        {// продолжается обычная обработка скана
                            ret = AppC.RC_WARN;
                        }
                    }
                    else
                    {
                        ret = AddGroupDet(ret);
                        // в любом случае обработку скана заканчиваем
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

        // добавление в список ТТН отмаркированного поддона
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
                        sN = "Маркир.";
                    }
                    else
                    {
                        dr["SSCCINT"] = xSc.Dat;
                        sN = "Скомпл.";
                    }
                    sN = String.Format("{1} поддон №{0}", dr["NPODD"], sN);
                }
                dr["SNM"] = sN;

                // для PrimaryKey
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


        // заполнение структуры ScDat на основе прочитанного штрих-кода
        // (находится там же)
        private bool TranslSCode(ref PSC_Types.ScDat s)
        {
            bool
                bFind = false,     // связь со справочником MC не установлена (пока)
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
                                case "01":                          // глобальный номер товара
                                case "02":
                                    s.sEAN = Srv.CheckSumModul10(sS.Substring(1, 12));
                                    s.sGTIN = sS.Substring(0, 14);
                                    sS = sS.Substring(14);
                                    break;
                                case "10":                          // номер партии
                                    s.nParty = int.Parse(sS.Substring(0, 4)).ToString();
                                    sS = sS.Substring(4);
                                    break;
                                case "11":                          // дата изготовления (ГГММДД)
                                    sP = sS.Substring(0, 6);
                                    s.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);
                                    //bFind = xNSI.GetMCData(s.sKMCFull, ref s, 0);
                                    //sTypDoc.sDataIzg = sTypDoc.dDataIzg.ToString("dd.MM.yy");
                                    sS = sS.Substring(6);
                                    break;
                                case "30":                          // количество мест на поддоне
                                    s.nMestPal = int.Parse(sS.Substring(0, 4));
                                    s.nTypVes = AppC.TYP_PALET;
                                    s.tTyp = AppC.TYP_TARA.TARA_PODDON;
                                    sS = sS.Substring(4);
                                    break;
                                case "37":                          // количество изделий
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
                            {// для штучных
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
                        if (sIdPrim == "2")     // весовая продукция или внутренний код
                        {
                            bFind = xNSI.IsAlien(sS, ref s);
                            if (!bFind)
                            {
                                sS = sS.Substring(1);
                                s.fVes = FRACT.Parse(sS.Substring(5, 6)) / 1000;
                                if (sS.Substring(0, 1) != "9")
                                {// на транспортной единице (ящик или поддон)
                                    s.nParty = int.Parse(sS.Substring(0, 3)).ToString();
                                    s.nKrKMC = int.Parse(sS.Substring(3, 2));
                                }
                                else
                                {// на отдельной единице весовой продукции
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

        // заполнение структуры ScDat на основе прочитанного штрих-кода
        // (находится там же)
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

                        // количество в ящике или паллетте
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
                        {// весовой товар
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
            {// на ящиках кривые этикетки могут неправильно давать емкость
                if ((s.drSEMK != null) && (n > 0))
                {// удалось установить емкость по справочнику
                    if (s.fEmk != n)
                    {
                        string
                            sP = String.Format("Несовпадение емкостей!\nВ штрихкоде - {0}\nВ справочнике - {1}\nОтменить сканирование(Enter)?\n(ESC)-подвердить {0}", n, s.fEmk);
                        DialogResult dr = MessageBox.Show(sP, String.Format("Несовпадение:{0} <> {1}", n, s.fEmk),
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
        // заполнение структуры ScDat на основе прочитанного штрих-кода
        // новый формат
        private bool TranslMTNew(ref PSC_Types.ScDat s)
        {
            bool
                bPoddon = false,
                bFind = false,          // связь со справочником MC не установлена (пока)
                ret = false;
            string
                sTaraType,
                sP,
                sS = s.s;

            sTaraType = sS.Substring(0, 2);
            if (sTaraType == "52")
            {// для поддонов
                bPoddon = true;
                s.tTyp = AppC.TYP_TARA.TARA_PODDON;
            }
            else if (sTaraType == "53")
            {// для тарных мест
                bPoddon = false;
                s.tTyp = AppC.TYP_TARA.TARA_TRANSP;
            }
            sS = sS.Substring(2);

            // код материала
            s.sEAN = Srv.CheckSumModul10("20" + sS.Substring(0, 10));
            bFind = xNSI.GetMCDataOnEAN(s.sEAN, ref s, true);
            sS = sS.Substring(10);

            // SysN документа (заключение)
            s.nNPredMT = int.Parse(sS.Substring(0, 9)) * (-1);
            sS = sS.Substring(9);

            // дата годности(изготовления) (ГГММДД)
            sP = sS.Substring(0, 6);
            s.dDataIzg = DateTime.ParseExact(sP, "yyMMdd", null);
            s.sDataIzg = sP.Substring(4, 2) + "." + sP.Substring(2, 2) + "." +
                sP.Substring(0, 2);

            sS = sS.Substring(6);

            // емкость/количество единиц
            s.fVsego = Srv.Str2VarDec(sS.Substring(0, 7));
            s.fVes = s.fVsego;
            s.fEmk = s.fVes;
            s.fEmk_s = s.fEmk;
            sS = sS.Substring(7);

            if (bPoddon)
            {
                // № поддона
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










        // определение емкости в штуках для весового товара по справочнику емкостей
        // возможные варианты - 1 транспортная упаковка (одно место)
        //                      2 потребительская тара (одна штука)
        //                      3 поддон (несколько мест)
        // могут быть установлены:
        // - емкость
        // - тара
        // - количество штук
        // - тип упаковки
        private bool TrySetEmk(DataTable dtM, DataTable dtD, ref PSC_Types.ScDat sc, FRACT fVesU)
        {
            const int MAXDIFF = 1000000;
            bool ret = false;

            bool bTryComp = false,       // хоть что-то вычисляли для определения типа упаковки?
                bNot1Sht = true,         // это не единичный продукт
                bNot1Pal = true;         // это не палетта

            int 
                //nPrPl = 0,               // № произв. площ.
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
            {// поиск или подбор емкости по коду продукции и возможно считанному весу

                DataRelation myRelation = dtM.ChildRelations["KMC2Emk"];
                DataRow[] childRows = sc.drMC.GetChildRows(myRelation);

                //if (sc.nParty.Length == 4)
                //    nPrPl = sc.nParty.Substring(0, 1);

                foreach (DataRow chRow in childRows)
                {
                    fCE = (FRACT)chRow["EMK"];
                    if ( fCE != 0)
                    {// емкость указана
                        if (fVesU > 0)
                        {// и вес имеется
                            bTryComp = true;
                            fSignDiff = fVesU - fCE;
                            fDiff = Math.Abs(fSignDiff);
                            fDiffPercent = fDiff * 100 / fCE;

                            //bNot1Sht |= (fDiffPercent < 40) || (fDiffPercent > 100);

                            //bNot1Pal |= (fDiffPercent < 200);

                            if (fDiffPercent <= nVesVar)
                            {// похоже на 1 тарное место, минимизируем только отклонения в пределах 40%
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
                            {// или штука или поддон
                                if (fVesU < fCE)
                                {// явно не поддон
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
                                {// для поддонов емкость определяется по вводу мест
                                    // или емкости поддона (справочник МЦ)
                                    bNot1Sht = true;
                                    bNot1Pal = false;
                                }
                            }


                        }
                        else
                        {// вес не указан, определение тары и штук/ящик
                            if (sc.fEmk > 0)
                            {// уже введена
                                if ((sc.fEmk == (FRACT)chRow["EMK"]) || true)
                                {// введенная совпала
                                    if (((int)chRow["PR"] > 0) || (fEmk == 0))
                                    {// одна из приоритетных или вобще не иыбирали
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
                    {// подбирать нечего, только одна емкость
                        fEmk = (FRACT)chRow["EMK"];
                        //sTara = (string)chRow["KT"];
                        sTara = (string)chRow["KTARA"];
                        nSht = (int)chRow["KRK"];
                        nEP = (int)chRow["EMKPOD"];
                    }
                }

                if ((sc.drSEMK == null) || bEditMode)
                {// по ITF/справочнику емкостей не получилось
                    sc.fEmk = fEmk;
                    sc.fEmk_s = sc.fEmk;
                    sc.nTara = sTara;
                    sc.nKolSht = nSht;
                    sc.nMestPal = nEP;
                }

                if (bTryComp == true)
                {
                    if (bNot1Sht == false)
                    {// 1 штука
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
                            {// что-то поменялось
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
            {// справочник отсутствует, по умолчанию - ???
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
                        // потребительская упаковка
                        sMsg = String.Format("Это ящик {0:N1}(Enter)?\n(ESC) - потребительская тара", fEZ);
                    else if (fEZ != fCurE)
                        sMsg = String.Format("(ENT) - взять {0:N1} из заявки ?\n(ESC) - использовать {1:N1}", fEZ, fCurE);
                    if (sMsg.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show(sMsg, "Емкость неизвестна!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                        if (dr == DialogResult.OK)
                            fE = fEZ;
                    }
                }
            }
            return (fE);
        }

        // проверка (установка) емкости в штуках по справочнику емкостей
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
        //    {// поиск емкости по коду продукции и возможно считанному весу
        //        DataRelation myRelation = dtM.ChildRelations["KMC2Emk"];
        //        DataRow[] childRows = sc.drMC.GetChildRows(myRelation);
        //        if (childRows.Length == 1)
        //        {// подбирать нечего, только одна емкость
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
        //                {// емкость по умолчанию
        //                    fEmk_Def = fCE;
        //                    nEmkPod_Def = nEmkPod;
        //                }

        //                if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
        //                {
        //                    if (nEmkPod == sc.nMestPal)
        //                    {
        //                        fEmk = fCE;
        //                        if ((int)chRow["PR"] > 0)
        //                        {// емкость по умолчанию
        //                            fEmk_Def = fCE;
        //                            nEmkPod_Def = (int)chRow["EMKPOD"];
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (fCE != 0)
        //                    {// емкость указана
        //                        if (fCE == sc.fEmk)
        //                        {// емкость совпала
        //                            fEmk = fCE;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            if (fEmk == 0)
        //            {// установить емкость ящика не удалось
        //                if (fEmk_Def != 0)
        //                {
        //                    if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
        //                    {// предложим ввести ту, что по умолчанию
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
        //                    DialogResult dr = MessageBox.Show("Отменить сканирование(Enter)?\n(ESC)-подвердить емкость",
        //                        "Несовпадение емкостей",
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
                        {// добавляем только несовпадающие емкости
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

        // проверка (установка) емкости в штуках по справочнику емкостей
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
        //    {// поиск емкости по коду продукции
        //        if (sc.tTyp == AppC.TYP_TARA.TARA_PODDON)
        //        {//для поддонов

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
        //                {//такой укладки не существует, возможна нестандартная укладка
        //                    sa = GetEmk4KMC(ref sc, String.Format("(KMC='{0}')", sc.sKMC), true, out nDefEmk);
        //                    if (sa.Length == 1)
        //                    {
        //                        fEmk = sa[0].DecDat;
        //                    }
        //                }
        //            }
        //            else
        //            {//придется выбирать, т.к. несколько вариантов укладки с таким количеством мест на поддоне
        //            }
        //        }
        //        else if ((sc.tTyp != AppC.TYP_TARA.TARA_POTREB) || (sc.nRecSrc == (int)NSI.SRCDET.HANDS))
        //        {// для ящиков

        //            sF = String.Format("(KMC='{0}')", sc.sKMC);
        //            if (sc.fEmk > 0)
        //                sF = String.Format(sF + "AND(EMK={0})", sc.fEmk);

        //            //sa = GetEmk4KMC(ref sc, sF, false, out nDefEmk);
        //            sa = GetEmk4KMC(ref sc, sF, true, out nDefEmk);
        //            if (sa.Length >= 1)
        //            {// все хорошо, такое может быть, например при разной укладке
        //                if (sa.Length == 1)
        //                {
        //                    fEmk = sa[0].DecDat;
        //                }
        //            }
        //            else
        //            {
        //                if (sc.nRecSrc == (int)NSI.SRCDET.SCAN)
        //                {
        //                    sF = String.Format("Нет Е={0}", sc.fEmk);
        //                    DialogResult dr = MessageBox.Show("Отменить сканирование(Enter)?\n(ESC)-подвердить емкость", sF,
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

            sc.fKolE_alrT = 0;          // уже введено единиц данного кода (мест = 0)
            sc.nKolM_alrT = 0;          // уже введено мест данного кода
            sc.fMKol_alrT = 0;

            sc.drEd = null;
            sc.drMest = null;


            // фильтр - SYSN + EAN13
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
                {// это единицы
                    if (desProd == NSI.DESTINPROD.TOTALZ)
                    {// единицы для конкретных партий из заявки
                        sc.fKolE_alrT += fV;
                    }
                    else
                    {// единицы из любой партии
                        sc.fKolE_alr += fV;
                    }
                    if (nParty == nP)
                    {// если совпали партии и дата выработки
                        if (sDVyr == sc.dDataIzg.ToString("yyyyMMdd"))
                        {// если суммировать - только сюда
                            sc.drEd = dr;
                        }
                    }
                }
                else
                {// места
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
                        {// закрывают общую часть
                            sc.nKolM_alr += nM;
                            sc.fMKol_alr += fV;
                        }
                        if (nParty == nP)
                        {
                            if (sDVyr == sc.dDataIzg.ToString("yyyyMMdd"))
                            {// если суммировать - только сюда
                                sc.drMest = dr;
                            }
                        }
                    }
                }

                i++;

                // строк с данным кодом
                ret++;
            }

            if (fE == 0)
            {// емкость неизвестна
                sc.nKolM_alr = nMest;
                sc.fMKol_alr = fVsego;
            }
            return (ret);
        }




        // проверка (установка) KMC по GTIN
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
                {// емкости присутствуют 
                    if (dv.Count > 1)
                    {// и их несколько
                        if (nEFromBC > 0)
                        {// емкость в таре указана
                            if (sc.tTyp == AppC.TYP_TARA.TARA_TRANSP)
                            {
                                if (xSc.dicSc.ContainsKey("310"))
                                {// весовой товар
                                    sF += String.Format("AND(KRK={0})", nEFromBC);
                                }
                                else
                                {// штучный товар
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
