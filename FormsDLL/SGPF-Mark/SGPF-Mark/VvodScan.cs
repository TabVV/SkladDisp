using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ScannerAll;
using PDA.Service;
using PDA.OS;
using PDA.BarCode;
using SkladGP;

using FRACT = System.Decimal;

namespace SGPF_Mark
{
    public partial class NPodd2Sscc : Form
    {
        private bool
            bInScanProceed = false;

        ScanVarGP
            xScanPrev = null,
            xScan = null;

        // текущее сканирование
        //private PSC_Types.ScDat 
        //    scCur;

        private void SelAllTextF(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        // установка полей ввода [+ сброс информационных]
        private void SetDetFields(ref PSC_Types.ScDat scCur, bool bClearInf)
        {
            string
                s;

            if (bClearInf == true)
            {
                this.tKrKMC.Text =
                this.tEmk.Text =
                this.lKMCName.Text =
                this.tDTV.Text =
                this.tParty.Text =
                this.tMest.Text =
                this.tVes.Text =
                this.tVsego.Text = "";
                this.tSSCC.Text = "";
            }
            else
            {// информационные поля отображаются
                s = xMark.ScanFirst.sN;
                if (xMark.ScanFirst.drMC != null)
                    s += " [" + (string)xMark.ScanFirst.drMC["GRADUS"] + "]";
                this.lKMCName.Text = s;


                this.tKrKMC.Text = (xMark.ScanFirst.nKrKMC <= 0) ? "" : xMark.ScanFirst.nKrKMC.ToString();
                //this.tEmk.Text = (scCur.fEmk <= 0) ? "" : scCur.fEmk.ToString();
                this.tEmk.Text = ((xMark.ScanFirst.bVes) ? xMark.ScanFirst.nKolSht : (int)xMark.ScanFirst.fEmk).ToString();
                this.tDTV.Text = xMark.ScanFirst.sDataIzg;

                this.tParty.Text = xMark.ScanFirst.nParty;
                this.tMest.Text = (xMark.ScanFirst.nMest == AppC.EMPTY_INT) ? "" : xMark.ScanFirst.nMest.ToString();
                this.tVsego.Text = (xMark.ScanFirst.fVsego == 0) ? "" : xMark.ScanFirst.fVsego.ToString();
                this.tVes.Text = (xMark.ScanFirst.fVes == 0) ? "" : xMark.ScanFirst.fVes.ToString();
            }
        }

        private void SetAvailDopFields(bool bClearInf)
        {
            tParty.Enabled =
                tDTV.Enabled =
                tMest.Enabled = bClearInf;
            if (xMark.ScanFirst.xEmks.Count > 1)
            {
                tEmk.Enabled = bClearInf;
            }
            else
                tEmk.Enabled = !bClearInf;

            tVsego.Enabled = xMark.ScanFirst.bVes;
        }


        private void tKrKMC_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    int nM = int.Parse(sT);
                    PSC_Types.ScDat sTmp = new PSC_Types.ScDat(new ScannerAll.BarcodeScannerEventArgs(BCId.Unknown, ""));
                    if (true == xNSI.GetMCData("", ref sTmp, nM, true))
                    {
                        xMark.ScanFirst = sTmp;

                        xMark.ScanFirst.nMest = xMark.ScanFirst.nMestPal;
                        xMark.NumberOfScans = 1;
                        EvalTot(xMark.ScanFirst.nMestPal);

                        xMark.ScanFirst.nRecSrc = (int)NSI.SRCDET.HANDS;
                        if (xMark.ScanFirst.bVes)
                            xMF.TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt, ref xMark.ScanFirst, 0);

                        if (xMark.ScanFirst.bVes)
                        {
                            tVes.Visible = true;
                        }

                        SetDetFields(ref xMark.ScanFirst, false);
                        SetAvailDopFields(true);
                        if (enWrapMode.CurMode == AppC.WRAP_MODES.WRP_BY_NSI)
                            chWrapp.Checked = WrappSet(xNSI.DT[NSI.NS_MC].dt, xMark.ScanFirst, 0);
                    }
                    else
                    {
                        e.Cancel = true;
                        Srv.ErrorMsg("Нет в справочнике!", "Код " + nM.ToString(), true);
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
            //    scCur.nKrKMC = 0;
            //if (e.Cancel || (scCur.nKrKMC == 0))
            //{
            //    bSetByHand = false;
            //}
                xMark.ScanFirst.nKrKMC = 0;
            //if (e.Cancel || (xMark.ScanFirst.nKrKMC == 0))
            //{
            //    bSetByHand = false;
            //}
        }

        // проверка емкости
        private void tEmk_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            string sT = ((TextBox)sender).Text.Trim();
            try
            {
                ((TextBox)sender).Text = (xMark.ScanFirst.bVes)?
                    ((StrAndInt)xMark.ScanFirst.xEmks.Current).IntCodeAdd1.ToString():
                    ((StrAndInt)xMark.ScanFirst.xEmks.Current).DecDat.ToString();
            }
            catch
            {
                e.Cancel = true;
            }
        }



        // дата выработки
        private void tDTV_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    sT = Srv.SimpleDateTime(sT, Smena.DateDef);
                    DateTime d = DateTime.ParseExact(sT, "dd.MM.yy", null);
                    xMark.ScanFirst.dDataIzg = d;
                    xMark.ScanFirst.sDataIzg = sT;
                    ((TextBox)sender).Text = sT;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                xMark.ScanFirst.dDataIzg = DateTime.MinValue;
            //if (e.Cancel || (xMark.ScanFirst.dDataIzg == DateTime.MinValue))
            //{
            //    bSetByHand = false;
            //}
        }

        private void tParty_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    if (sT.Length > 4)
                    {
                        Srv.ErrorMsg("Партия - 4 знака!", true);
                        e.Cancel = true;
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            if (!e.Cancel)
            {
                xMark.ScanFirst.nParty = sT;
                ((TextBox)sender).Text = sT;
            }

        }


        private void tMest_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            string sT = ((TextBox)sender).Text.Trim();
            int
                nM = 0;

            if (sT.Length > 0)
            {
                try
                {
                    nM = int.Parse(sT);
                    ((TextBox)sender).Text = sT;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            if (!e.Cancel)
            {
                EvalTot(nM);
            }
        }


        private void EvalTot(int nM)
        {
            int
                nE,
                nV = 0;

            if (xMark.ScanFirst.bVes)
            {
                nE = xMark.ScanFirst.nKolSht;
            }
            else
            {
                nE = (int)xMark.ScanFirst.fEmk;
            }
            xMark.ScanFirst.nMest = nM;
            nV = nM * nE;
            xMark.ScanFirst.fVsego = nV;
            tVsego.Text = nV.ToString();
        }


        private void tVsego_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            int
                nKolG = 0;

            string sT = ((TextBox)sender).Text.Trim();
            try
            {
                nKolG = int.Parse(sT);
                if (nKolG > 0)
                {
                    xMark.ScanFirst.fVsego = (FRACT)nKolG;
                }
                if (xMark.ScanFirst.bVes)
                {
                    if (xMark.SrcInfo == NSI.SRCDET.HANDS)
                    {
                        tVes.Enabled = true;
                    }
                }
            }
            catch
            {
                e.Cancel = true;
            }
            ((TextBox)sender).Text = ((int)xMark.ScanFirst.fVsego).ToString();
        }

        private void tVes_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            FRACT
                nKolG = 0;

            string sT = ((TextBox)sender).Text.Trim();
            try
            {
                nKolG = FRACT.Parse(sT);
                if (nKolG > 0)
                {
                    xMark.ScanFirst.fVes = nKolG;
                }
            }
            catch
            {
                e.Cancel = true;
            }
            ((TextBox)sender).Text = (xMark.ScanFirst.fVes).ToString();

        }


        private string GetDopInf()
        {
            System.Reflection.AssemblyName xAN = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            string sV = xAN.Version.ToString();
            int nPoint = sV.LastIndexOf(".");
            return (String.Format("ВХОД={1} ({2})    <F1>-помощь  v{0}", sV.Substring(nPoint), sDevID, nPal4Doc) );
        }

        // обработка выполненного сканирования
        private void OnScan(object sender, BarcodeScannerEventArgs e)
        {
            bool
                bRet = true,
                bDupScan;
            int 
                t1,t2,
                nRet = AppC.RC_CANCEL;
            string 
                sP,sP1,
                sErr = "";

            // началась обработка сканирования
            bInScanProceed = true;
            if (e.nID != BCId.NoData)
            {
                try
                {
                    PSC_Types.ScDat sc = new PSC_Types.ScDat(e);
                    xScan = new ScanVarGP(e, xNSI.DT["NS_AI"].dt);
                    bDupScan = ((xScanPrev != null) && (xScanPrev.Dat == xScan.Dat)) ? true : false;

                    sc.sN = e.Data + "-???";
                    #region Обработка скана
                    do
                    {
                        if (xScan.Dat.StartsWith("91E"))
                        {// адрес ID-точки
                            MainCycleStart(xScan.Dat.Substring(4), true);
                            break;
                        }

                        if (sDevID.Length == 0)
                        {
                            Srv.ErrorMsg("Введите или\nотсканируйте № точки!", "Ошибка", true);
                            break;
                        }

                        xMark.NumberOfScans++;
                        xMark.SrcInfo = NSI.SRCDET.SCAN;

                        //if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) == ScanVarGP.BCTyp.SP_SSCC_EXT) ||
                        //    ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_PRT) == ScanVarGP.BCTyp.SP_SSCC_PRT) ||
                        //    ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_INT) == ScanVarGP.BCTyp.SP_SSCC_INT))
                        if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0) ||
                            ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_PRT) > 0))
                        {
                            int nRetSSCC = ProceedSSCC(xScan, ref sc, aEd.Current);
                            if (nRetSSCC == AppC.RC_QUIT)
                            {// SSCC привязан, новый поддон
                                xScan = null;
                                break;
                            }
                            if (nRetSSCC != AppC.RC_HALFOK)
                            {// не было удачной трансляции SSCC
                                xScan = null;
                                break;
                            }
                            // далее будет вывод удачной трансляции SSCC
                            if ((xScan.bcFlags & ScanVarGP.BCTyp.SP_SSCC_PRT) > 0) 
                                // SSCC для ящиков
                                bRet = (ProceedProd(xScan, ref sc, bDupScan, false) == AppC.RC_OK) ? true : false;

                        }
                        else
                        {// должна быть продукция
                            if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_OLD_ETIK) == ScanVarGP.BCTyp.SP_OLD_ETIK) ||
                                 (xScan.Id != BCId.Code128))
                            {// старая этикетка или EAN13
                                bRet = xMF.TranslSCode(ref sc, ref sErr);
                                if ((sc.ci == BCId.EAN13) || (sc.ci == BCId.Interleaved25) ||
                                    ((sc.s.Length >= 13) && (sc.s.Length <= 14)))
                                    sc.tTyp = AppC.TYP_TARA.TARA_POTREB;
                            }
                            else
                            {// новая этикетка

                                if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDV) == ScanVarGP.BCTyp.SP_MT_PRDV))
                                    bRet = xMF.TranslMT(ref sc);
                                else if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_MT_PRDVN) == ScanVarGP.BCTyp.SP_MT_PRDVN))
                                    bRet = xMF.TranslMTNew(ref sc);
                                else
                                {
                                    // разбор этикетки стандартным способом по AI
                                    t1 = Environment.TickCount;
                                    bRet = xMF.NewTranslSCode(ref sc, xScan);
                                    t2 = Environment.TickCount;
                                    sP = Srv.TimeDiff(t1, t2, 3);
                                }
                            }
                            if (bRet)
                            {
                                t1 = Environment.TickCount;
                                // возможно, контрольное сканирование

                                if ((xMark.ScanFirst.ci != BCId.Unknown) && (xMark.ScanFirst.s is string))
                                {
                                    if (xMark.ScanFirst.sEAN == sc.sEAN)
                                        break;
                                    // не совпали коды
                                    Srv.ErrorMsg(xMark.ScanFirst.sEAN + "\n" + sc.sEAN, "Разные EAN!", true);
                                }

                                bRet = (ProceedProd(xScan, ref sc, bDupScan, false) == AppC.RC_OK) ? true : false;
                                t2 = Environment.TickCount;
                                sP1 = Srv.TimeDiff(t1, t2, 3);
                            }
                            //aEd.SetCur( (sc.tTyp == AppC.TYP_TARA.TARA_PODDON) ? tSSCC : tMest );
                            //tMest.Enabled = true;
                        }

                        if (!bRet)
                            throw new Exception("Ошибка сканирования!");
                        //aEd.SetCur((sc.tTyp == AppC.TYP_TARA.TARA_PODDON) ? tSSCC : tMest);
                        aEd.SetCur( tSSCC );
                        tMest.Enabled = true;
                        tKrKMC.Enabled = false;

                        if (xMark.ScanFirst.bVes)
                        {
                            tVes.Visible = true;
                            tVsego.Enabled = true;
                        }
                        if (xMark.ScanFirst.tTyp != AppC.TYP_TARA.TARA_PODDON)
                        {

                            if (xMark.ScanFirst.bVes)
                            {
                                tVes.Enabled = true;
                            }

                            if (xMark.ScanFirst.tTyp == AppC.TYP_TARA.TARA_POTREB)
                            {
                                tParty.Enabled = tDTV.Enabled = true;
                                aEd.SetCur(tParty);
                            }
                        }

                        SetDetFields(ref xMark.ScanFirst, false);
                        if (enWrapMode.CurMode == AppC.WRAP_MODES.WRP_BY_NSI)
                            chWrapp.Checked = WrappSet(xNSI.DT[NSI.NS_MC].dt, xMark.ScanFirst, 0); 

                        xScanPrev = xScan;
                    } while (false);
                    #endregion

                }
                catch (Exception ex)
                {
                    string sE = String.Format("{0}({1}){2}", xScan.Id.ToString(), xScan.Dat.Length, xScan.Dat);
                    Srv.ErrorMsg(sE + "\n" + ex.Message, "Ошибка сканирования", true);
                }
            }
            // обработка сканирования окончена
            bInScanProceed = false;
        }

        private string InFocus()
        {
            string
                ret = "";
            foreach (Control c in aEd)
            {
                if (c.Focused)
                {
                    ret = c.Name;
                    break;
                }
            }
            return (ret);
        }

        private int TrySend2Serv(string sSSCC)
        {
            int
                ret = AppC.RC_NOSSCC;
            AppC.VerRet
                xR;

            if (sSSCC.Length > 0)
            {
                xR = VerifyB();
                ret = xR.nRet;
                if (ret == AppC.RC_OK)
                {
                    xMark.ScanFirst.sSSCC = sSSCC;
                    ret = SendCOMMark(sSSCC);
                    WrappSet(xNSI.DT[NSI.NS_MC].dt, xMark.ScanFirst, 1);
                    if (ret == AppC.RC_OK)
                    {
                        Srv.ErrorMsg(sSSCC, "Отправлено!", false);
                        if (bAutoMark)
                        {
                            EndEditB(false, sSSCC);
                        }
                        else
                        {
                            ret = AppC.RC_QUIT;
                            BeginEditB(false);
                        }
                    }
                }
                else
                {
                    aEd.SetCur(xR.cWhereFocus);
                }
            }
            else
                Srv.ErrorMsg("Нет SSCC!", true);
            return (ret);
        }

        private void SetDefEmk(ref PSC_Types.ScDat scD)
        {
            int
                i;
            StrAndInt
                xE;
            for (i = 0; i < scD.xEmks.Count; i++ )
            {
                scD.xEmks.CurrIndex = i;
                xE = (StrAndInt)scD.xEmks.Current;
                if (xE.DecDat == scD.fEmk)
                {
                    scD.nKolSht = xE.IntCodeAdd1;
                    break;
                }
            }
        }


        private int ProceedSSCC(ScanVarGP xS, ref PSC_Types.ScDat scD, Control xCCur)
        {
            int
                ret = AppC.RC_OK;
            string 
                sSSCC = xS.Dat;
            MainF.ServerExchange 
                xSE = new MainF.ServerExchange(xMF);

            switch (InFocus())
            {
                case "tKrKMC":

                    xSE.FullCOM2Srv = String.Format("COM={0};KSK={6};MAC={1};KP={2};PAR=(KSK={3},DT={4},SSCC={5},TYPOP=MARK);",
                        AppC.COM_ZSC2LST,
                        xMF.xSm.MACAdr,
                        xMF.xSm.sUser,
                        xMF.xCDoc.xDocP.nSklad,
                        xMF.xCDoc.xDocP.dDatDoc.ToString("yyyyMMdd"),
                        sSSCC,
                        xMF.xCDoc.xDocP.nSklad
                        );
                    try
                    {
                        //ret = xMF.ConvertSSCC2Lst(xSE, sSSCC, true, true, ref scD);
                        ret = xMF.ConvertSSCC2Lst(xSE, sSSCC, ref scD, true);
                        if ((ret == AppC.RC_OK) && (scD.sKMC.Length > 0))
                        {
                            scD.tTyp = ((xS.bcFlags & ScanVarGP.BCTyp.SP_SSCC_PRT) > 0)?
                                AppC.TYP_TARA.TARA_TRANSP:AppC.TYP_TARA.TARA_PODDON;
                            if (scD.bVes)
                            {
                                if (scD.nKolSht == 0)
                                    // определить количество в штуках
                                    SetDefEmk(ref scD);
                                if (scD.nKolG == 0)
                                {
                                    scD.nKolG = scD.nMest * scD.nKolSht;
                                }
                                scD.fVsego = scD.nKolG;
                            }
                            xMark.ScanFirst = scD;
                            //EvalTot(xMark.ScanFirst.nMestPal);
                            ret = AppC.RC_HALFOK;
                        }
                        else
                            throw new Exception("Bad SSCC");
                    }
                    catch
                    {
                        Srv.ErrorMsg("Недопустимое содержимое", sSSCC, true);
                        ret = AppC.RC_CANCEL;
                    }
                    break;
                case "tSSCC":
                    xMark.SSCC = sSSCC;
                    tSSCC.Text = sSSCC;
                    ret = TrySend2Serv(sSSCC);
                    break;
            }
            //scCur = scD;
            return (ret);
        }


        // продукция на какой-то из наших этикеток
        private int ProceedProd(ScanVarGP xSc, ref PSC_Types.ScDat sc, bool bDupScan, bool bEasyEd)
        {
            //bool 
            //    bDopValuesNeed = true;
            int
                nRet = AppC.RC_CANCEL;

            #region Обработка скана продукции
            do
            {
                if (!bDupScan)
                {// первичный скан штрихкода, предыдущий был другой
                }
                else
                {// повторный скан того же штрихкода - это, как правило, подтверждение ввода
                }

                    //scCur = sc;
                    if (!sc.bFindNSI)
                        Srv.ErrorMsg("Код не найден! Обновите НСИ!", true);

                    nRet = AppC.RC_OK;
                    if (sc.bVes == true)
                    {
                        //scCur.fVsego = scCur.fVes;
                        //FRACT fE = scCur.fVes;

                        // 11.07.14 взято в коммент
                        if (sc.nRecSrc != (int)NSI.SRCDET.SSCCT)
                        {
                            if (!xScan.dicSc.ContainsKey("37") && (xScan.dicSc.Count == 4))
                            {
                                sc.tTyp = AppC.TYP_TARA.TARA_POTREB;
                                sc.fEmk = sc.fEmk_s = 0;
                            }

                            // нужно по-другому
                            if ((sc.tTyp == AppC.TYP_TARA.TARA_PODDON) && (sc.nMestPal > 0))
                            {
                                PSC_Types.ScDat
                                    scTmp = sc;
                                bool
                                    bSetEmk = xMF.TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt,
                                            ref scTmp, scTmp.fVes / scTmp.nMestPal);
                                if ((bSetEmk == true) && (scTmp.tTyp == AppC.TYP_TARA.TARA_TRANSP))
                                {
                                    sc.fEmk = scTmp.fEmk;
                                    sc.nMest = sc.nMestPal;
                                }
                            }
                            else
                                xMF.TrySetEmk(xNSI.DT[NSI.NS_MC].dt, xNSI.DT[NSI.NS_SEMK].dt, ref sc, sc.fVes);
                        }
                    }
                    else
                    {
                        if (sc.nMestPal <= 0)
                        {
                            StrAndInt si;
                            for(int i = 0; i < sc.xEmks.Count; i++)
                            {
                                sc.xEmks.CurrIndex = i;
                                si = (StrAndInt )sc.xEmks.Current;
                                if (si.DecDat == sc.fEmk)
                                {
                                    sc.nMestPal = si.IntCode;
                                    sc.nTara = si.SNameAdd1;
                                    sc.nKolSht = si.IntCodeAdd1;
                                }
                            }
                        }
                    }

                    if (AppC.RC_OK == nRet)
                    {
                        sc.nMest = sc.nMestPal;
                        xMark.ScanFirst = sc;
                        xMark.NumberOfScans = 1;
                        EvalTot(xMark.ScanFirst.nMestPal);
                    }
            } while (false);
            #endregion

            return (nRet);
        }

        private void SuccNewDoc()
        {
            if (chNewDoc.Checked)
                nPal4Doc = 1;
            else
                nPal4Doc++;
            chNewDoc.Checked = false;
            if (enWrapMode.CurMode == AppC.WRAP_MODES.WRP_ALW_SET)
                chWrapp.Checked = true;
            else if (enWrapMode.CurMode == AppC.WRAP_MODES.WRP_ALW_RESET)
                chWrapp.Checked = false;

        }

        public int SendCOMMark(string sSSCC)
        {
            int
                nRet = AppC.RC_OK;
            string
                sAddPars = "",
                sL,
                sScanProd;
            DataSet dsTrans;
            //CurUpLoad
            //    xCUpLoad;
            MainF.ServerExchange
                xSE = new MainF.ServerExchange(xMF);

            // вместе с командой отдаем заголовок документа
            //xCUpLoad = new CurUpLoad();
            //xCUpLoad.sCurUplCommand = AppC.COM_MARKWMS;

            dsTrans = MakeWorkDataSet1Det(null, xNSI.DT[NSI.BD_DOUTD].dt, xMark.ScanFirst, sSSCC);

            sScanProd = (xMark.NumberOfScans > 1)?xMark.ScanFirst.s : "";

            sAddPars = String.Format(",DEVID={0}{1}", 
                sDevID, 
                (chNewDoc.Checked)?",ND=1":"");

            //if (bAskWrapMandatory)
            if (enWrapMode.CurMode == AppC.WRAP_MODES.WRP_ASK_EVERY)
            {
                DialogResult dr = MessageBox.Show("Застрейчеван - (ENT)\n" +
                                                  "Без стрейча  - (ESC)",
                    "Стрейчевание ?", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                chWrapp.Checked = (dr == DialogResult.OK) ? true : false;
            }

            xMark.WrapFlag = (chWrapp.Checked) ? "Y" : "N";

            xSE.FullCOM2Srv = String.Format("COM={0};KSK={8};MAC={1};KP={2};PAR=(BARCODE={3},SSCC={4},TYPOP={5}{6},TG={7});", 
                AppC.COM_MARKWMS,
                xMF.xSm.MACAdr,
                xMF.xSm.sUser,
                sScanProd,
                sSSCC,
                (chMsg2WMS.Checked)?"PRIHOD":"MARK",
                sAddPars,
                xMark.WrapFlag,
                xMF.xSm.nSklad
                );

            sL = xSE.ExchgSrv(AppC.COM_MARKWMS, "", "", null, dsTrans, ref nRet, 60);

            if (nRet == AppC.RC_OK)
            {
                SuccNewDoc();
            }
            else
            {// просто сохраним запись ??? -  если была сетевая ошибка! при ошибке сервера ничего сохранять не надо!
                if (xSE.ServerRet == AppC.RC_OK)
                {// сервер против этой записи ничего не имеет
                    SuccNewDoc();
                }
                Srv.ErrorMsg(sL);
            }
            //lHelp.Text = GetDopInf();
            xBattInf.BIUserText = GetDopInf();

            return (nRet);
        }
        // подготовка DataSet для выгрузки
        public DataSet MakeWorkDataSet1Det(DataTable dtM, DataTable dtD, PSC_Types.ScDat s, string sSSCC)
        {

            DataTable 
                dtMastNew = null,
                dtDetNew = dtD.Clone();
            DataRow
                ret;

            if (dtM != null)
                dtMastNew = dtM.Clone();

            ret = dtDetNew.NewRow();

            ret["KRKMC"] = s.nKrKMC;
            ret["SNM"] = s.sN;
            ret["KOLM"] = s.nMest;
            ret["KOLE"] = (s.bVes)?s.fVes:s.fVsego;
            ret["EMK"] = s.fEmk;
            ret["NP"] = s.nParty;
            ret["DVR"] = s.dDataIzg.ToString("yyyyMMdd");
            ret["EAN13"] = s.sEAN;
            ret["SYSN"] = -500;
            ret["SRP"] = (s.bVes == true) ? 1 : 0;
            ret["GKMC"] = s.sGrK;

            ret["KTARA"] = s.nTara;
            ret["KOLSH"] = s.nKolSht;

            ret["VES"] = s.fVes;
            ret["KOLG"] = s.fVsego;

            ret["DEST"] = s.nDest;
            ret["KMC"] = s.sKMC;

            ret["SRC"] = s.nRecSrc;

            ret["TIMECR"] = s.dtScan;

            ret["NPODD"] = s.nNomPodd;
            ret["NMESTA"] = s.nNomMesta;

            ret["ADRFROM"] = s.xOp.GetSrc(false);
            ret["ADRTO"] = s.xOp.GetDst(false);

            // отладка чудес всяких
            ret["GKMC"] = s.s;
            ret["SSCC"] = sSSCC;
            ret["SSCCINT"] = s.sSSCCInt;

            ret["NPODDZ"] = 0;

            ret["SYSPRD"] = s.nNPredMT;
            dtDetNew.Rows.Add(ret);

            drWithSSCC = ret;

            DataSet ds1Rec = new DataSet("dsMOne");
            if (dtMastNew != null)
                ds1Rec.Tables.Add(dtMastNew);
            ds1Rec.Tables.Add(dtDetNew);
            return (ds1Rec);
        }

        // nMode -  0 - определить стрейчевку по умолчанию
        //          1 - установить стрейчевку по умолчанию
        private bool WrappSet(DataTable dtMC, PSC_Types.ScDat s, int nMode)
        {
            int 
                nWP;
            string
                sKeyWrapp = (s.sKMC.Length > 0)?s.sKMC:"",
                sValWrapp = "N";
            if (!dicWrap.ContainsKey(sKeyWrapp))
            {
                try
                {
                    DataView xRowDView = new DataView(dtMC, String.Format("KMC='{0}'", sKeyWrapp), "", DataViewRowState.CurrentRows);
                    if (xRowDView != null)
                    {
                        if (xRowDView[0].Row["WRAPP"] is string)
                        {
                            try
                            {
                                nWP = int.Parse(((string)xRowDView[0].Row["WRAPP"]));
                            }
                            catch
                            {
                                nWP = 0;
                            }
                            sValWrapp = (nWP > 0) ? "Y" : "N";
                        }

                    }
                }
                catch
                {
                }
            }
            else
                sValWrapp = dicWrap[sKeyWrapp];

            if (nMode == 1)
                dicWrap[sKeyWrapp] = (chWrapp.Checked)?"Y":"N";

            return ((sValWrapp == "Y")?true:false);

        }


    }
}