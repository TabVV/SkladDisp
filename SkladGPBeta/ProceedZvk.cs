using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Drawing;

using ScannerAll;
using PDA.OS;
using PDA.BarCode;
using PDA.Service;

using FRACT = System.Decimal;

namespace SkladGP
{

    public partial class MainF : Form
    {

        /// заполнение DataRow заявки недостающими данными
        private int SetOneDetZ(ref PSC_Types.ScDat sD, DataTable dtZVK, DataRow dZ, DataRow drCurDoc, ref int nNPP)
        {
            bool
                bFNsi,
                bSomeDateIsSet = true;
            int
                //nIDOriginal,
                //nPrzPl = 0,
                nCondition = 0,
                nM = 0,
                nMest = 0;
            string
                //sNameFromSrv,
                sKMC = "",
                sEAN = "";
            object[]
                aIt = dZ.ItemArray;
            //DataSet
            //    dsZ;
            DataRow
                dr,
                drMZ = dtZVK.NewRow();

            //nIDOriginal = (int)drMZ["ID"];
            drMZ.ItemArray = aIt;
            try
            {
                bFNsi = false;
                sD.fEmk = 0;
                nMest = (int)drMZ["KOLM"];
                sKMC = (drMZ["KMC"] is string) ? drMZ["KMC"].ToString() : "";
                if (sKMC.Length > 0)
                {
                    dr = xNSI.DT[NSI.NS_MC].dt.Rows.Find(new object[] { sKMC });
                    bFNsi = sD.GetFromNSI(sD.s, dr, xNSI.DT[NSI.NS_MC].dt, false);
                    if (bFNsi)
                    {
                        drMZ["KRKMC"] = sD.nKrKMC;
                        drMZ["SNM"] = sD.sN;
                    }
                    else
                    {
                        drMZ["KRKMC"] = 0;
                        drMZ["SNM"] = "Неизвестно";
                    }
                }
                if (drMZ["EAN13"] == System.DBNull.Value)
                {
                    if (bFNsi)
                        drMZ["EAN13"] = sEAN = sD.sEAN;
                    else
                        drMZ["EAN13"] = "";
                }
                else
                    sEAN = (string)drMZ["EAN13"];
                if ((!bFNsi) && (sEAN.Length > 0))
                {
                    if (xNSI.GetMCDataOnEAN(sEAN, ref sD, false) == true)
                    {
                        drMZ["KMC"] = sD.sKMC;
                        drMZ["KRKMC"] = sD.nKrKMC;
                        drMZ["SNM"] = sD.sN;
                    }
                    else
                    {
                        drMZ["KRKMC"] = 0;
                        drMZ["SNM"] = "Неизвестно";
                    }
                }
            }
            catch
            {
                drMZ["KRKMC"] = 0;
                drMZ["SNM"] = "Неизвестно";
                nMest = 0;
                //if (swProt != null)
                //{
                //    sE = String.Format("{0} - Загрузка BC={1} EAN={2}",
                //    DateTime.Now.ToString("dd.MM.yy HH:mm:ss - "),
                //    dMDoc["DOCBC"],
                //    sEAN);
                //    swProt.WriteLine(sE);
                //}
            }
            if ((dZ["SNM"] is string) && (((string)dZ["SNM"]).Length > 0))
                drMZ["SNM"] = dZ["SNM"];

            nM = nMest;
            if (drMZ["EMK"] == System.DBNull.Value)
            {
                if (nM > 0)
                {
                    if (sD.fEmk == 0)
                        sD.fEmk = (FRACT)((int)(((FRACT)drMZ["KOLE"]) / nM));
                    drMZ["EMK"] = sD.fEmk;
                }
                else
                    drMZ["EMK"] = 0;
            }
            drMZ["READYZ"] = NSI.READINESS.NO;

            if ( (drMZ["SSCC"] is string) && (drMZ["SSCC"].ToString().Length > 0))
            {// код паллетты внешней задан
                nCondition |= (int)NSI.SPECCOND.SSCC;
            }
            else
                drMZ["SSCC"] = "";
                
            if ((drMZ["SSCCINT"] is string) && (drMZ["SSCCINT"].ToString().Length > 0))
            {// код паллетты внутренней задан
                nCondition |= (int)NSI.SPECCOND.SSCC_INT;
            }
            else
                drMZ["SSCCINT"] = "";

            if (drMZ["NP"] is string)
            {
                if (((string)drMZ["NP"] == "*")
                    || ((string)drMZ["NP"] == "-1"))
                {
                    drMZ["NP"] = "";
                    nCondition |= (int)NSI.SPECCOND.DATE_SET_EXACT;
                }
                else
                {
                    if (((string)drMZ["NP"]).Length > 0)
                    {// партия задана
                        nCondition |= (int)NSI.SPECCOND.PARTY_SET;
                    }
                }
            }
            else
                drMZ["NP"] = "";


            //if (drMZ["DTG"] is string)
            //{
            //    try
            //    {
            //        DateTime dG = DateTime.ParseExact((string)drMZ["DTG"], "yyyyMMdd", null);
            //        if ((dG.Year < 2000) || (dG == DateTime.MinValue))
            //            drMZ["DTG"] = null;
            //    }
            //    catch
            //    {
            //        drMZ["DTG"] = null;
            //    }
            //}

            try
            {
                bSomeDateIsSet = DateCond(drMZ, "DVR", ref nCondition);
            }
            catch
            {
                bSomeDateIsSet = false;
                drMZ["DVR"] = null;
            }

            // 18.01.18 - годность только в информативных целях
            //try
            //{
            //    bClearParty = bClearParty & DateCond(drMZ, "DTG");
            //}
            //catch
            //{
            //    drMZ["DTG"] = null;
            //}

            if ((!bSomeDateIsSet) && ((nCondition & (int)NSI.SPECCOND.DATE_SET_EXACT) > 0))
                nCondition -= (int)NSI.SPECCOND.DATE_SET_EXACT;

            drMZ["COND"] = nCondition;

            if (drMZ["NPP"] is int)
                nNPP = (int)drMZ["NPP"];
            else
                drMZ["NPP"] = nNPP;

            //drMZ["ID"] = nIDOriginal;
            drMZ["SYSN"] = drCurDoc["SYSN"];
            dtZVK.Rows.Add(drMZ);

            //AddAKMC(dsZ, dZ, drMZ);
            return (nM);
        }


        private bool DateCond(DataRow drMZ, string sDateField, ref int nCondition)
        {
            bool
                bIsDateSet = false;

            if ( !String.IsNullOrEmpty((string)drMZ[sDateField]) )
            {
                if (DateTime.ParseExact((string)drMZ[sDateField], "yyyyMMdd", null) > DateTime.MinValue)
                {// дата выработки/годности задана
                    bIsDateSet = true;
                    nCondition |= (int)NSI.SPECCOND.DATE_SET;

                    nCondition |= (int)((sDateField == "DVR") ? NSI.SPECCOND.DATE_V_SET : NSI.SPECCOND.DATE_G_SET);

                    if ((nCondition & (int)NSI.SPECCOND.PARTY_SET) > 0)
                        nCondition |= (int)NSI.SPECCOND.DATE_SET_EXACT;

                }
            }
            return (bIsDateSet);
        }


        /// поиск подходящих строк в заявке
        private int FindRowsInZVK(DataRow dr, ref PSC_Types.ScDat sc, string nParty, FRACT fCurEmk, int nRetAtMy, bool bEvalInPall)
        {
            bool
                bGoodDate = false,
                bUseDTG = false;
            int
                //nState = (int)dr["READYZ"],
                nCond = (int)dr["COND"],
                nRet = AppC.RC_ALREADY;
            //nRet = AppC.RC_ALREADY;
            string
                sKindDat = "",
                sDat = "";
            DateTime
                //dCurDat,
                dDat;

            #region Подбор подходящих строк заявки
            do
            {// установка возможных адресов позиционирования в заявке
                if ((int)dr["READYZ"] != (int)NSI.READINESS.FULL_READY)
                {// строка заявки пока не выполнена

                    nRet = AppC.RC_OK;
                    if (bEvalInPall)
                    {// для комплектации учитываем поддон (если это необходимо)
                        if (xCDoc.xNPs.Current != (int)dr["NPODDZ"])
                        {// это чужой
                            if (nRetAtMy == AppC.RC_OK)
                            {// а чужие пропускаем, на своем было
                                nRet = AppC.RC_BADPODD;
                                break;
                            }
                            if (sc.sErr == "")
                                sc.sErr = String.Format("Другой ({0}) поддон!", dr["NPODDZ"]);
                        }
                    }

                    if ((nCond & (int)NSI.SPECCOND.DATE_G_SET) > 0)
                    {
                        sDat = (string)dr["DTG"];
                        sKindDat = "годности";
                        bUseDTG = true;
                    }
                    else if ((nCond & (int)NSI.SPECCOND.DATE_V_SET) > 0)
                    {
                        sDat = (string)dr["DVR"];
                        sKindDat = "выработки";
                        bUseDTG = false;
                    }
                    else
                        sDat = "";

                    if (sDat.Length > 0)
                    {// есть ограничения по дате
                        nRet = AppC.RC_BADDATE;
                        dDat = DateTime.ParseExact(sDat, "yyyyMMdd", null);
                        if (bUseDTG)
                        {// проверка годности
                            if (sc.dDataGodn >= dDat)
                            {
                                nRet = AppC.RC_OK;
                                bGoodDate = true;
                            }
                            if (((nCond & (int)NSI.SPECCOND.DATE_SET_EXACT) > 0) && (sc.dDataGodn != dDat))
                            {
                                nRet = AppC.RC_BADDATEXCT;
                                bGoodDate = false;
                            }
                        }
                        else
                        {// проверка выработки
                            if (sc.dDataIzg >= dDat)
                            {
                                nRet = AppC.RC_OK;
                                bGoodDate = true;
                            }
                            if (((nCond & (int)NSI.SPECCOND.DATE_SET_EXACT) > 0) && (sc.dDataIzg != dDat))
                            {
                                nRet = AppC.RC_BADDATEXCT;
                                bGoodDate = false;
                            }
                        }

                        if (nRet == AppC.RC_OK)
                        {
                            if (((nCond & (int)NSI.SPECCOND.PARTY_SET) > 0) && (nParty != sc.nParty))
                            {// партия и дата должны точно совпадать
                                    nRet = AppC.RC_BADPARTY;
                            }
                            else
                            {// партия не задавалась
                                if (!WhatPrtKeyF_Emk(dr, ref sc, fCurEmk))
                                    nRet = AppC.RC_NOEANEMK;
                            }
                        }
                        else
                        {// по дате не проходит
                            //nRet = AppC.RC_BADDATE;
                            sc.sErr = String.Format("Дата {0} ({1})\nнет соответствия:\n{2}!", sKindDat,
                                ((bUseDTG) ? sc.dDataGodn : sc.dDataIzg).ToString("dd.MM.yy"),
                                dDat.ToString("dd.MM.yy"));
                        }
                        break;
                    }

                    if (nParty.Length > 0)
                    {// указана конкретная партия
                        if (nParty == sc.nParty)
                        {// точное совпадение ключа
                            nRet = (WhatTotKeyF_Emk(dr, ref sc, fCurEmk)) ? AppC.RC_OK : AppC.RC_NOEANEMK;
                        }
                        else
                            nRet = AppC.RC_BADPARTY;
                        break;
                    }

                    // общий случай, закрывается любой партией или датой
                    if (WhatPrtKeyF_Emk(dr, ref sc, fCurEmk))
                        nRet = AppC.RC_OK;
                    else
                        nRet = AppC.RC_NOEANEMK;
                    break;
                }
            } while (false);
            #endregion

            return (nRet);
        }



        /// контроль всего документа на соответствие заявке
        private int ControlDocZVK(DataRow drD, List<string> lstProt)
        {
            return(ControlDocZVK(drD, lstProt, ""));
        }



        /// контроль всего документа на соответствие заявке
        private int ControlDocZVK(DataRow drD, List<string> lstProt, string s1Pallet)
        {
            bool
                bGood_KMC,
                bIsKMPL;
            int
                i = 0,
                nM,
                iStart,
                iCur,
                iCurSaved,
                iTMax, iZMax,
                nDokState = AppC.RC_OK,
                nM_KMC,
                nBad_NPP,
                nRet;
            string
                s1,
                s2,
                sOldKMC,
                sKMC,
                sFlt;
            FRACT
                fE,
                fE_KMC,
                fV = 0;
            object
                xProt;

            DataRow
                drC;
            DataView
                dv,
                dvZ, dvT;
            RowObj
                xR;

            //TimeSpan tsDiff;
            //int t1 = Environment.TickCount, t2, t3, td1, td2, tc = 0, tc1 = 0, tc2 = 0;
            //t2 = t1;

            if (drD == null)
                drD = xCDoc.drCurRow;

            bIsKMPL = (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL) ? true : false;

            string sRf = String.Format("(SYSN={0})", drD["SYSN"]);

            PSC_Types.ScDat sc = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
            lstProt.Add(HeadLineCtrl(drD));

            // пока что результат контроля неизвестен
            drD["DIFF"] = NSI.DOCCTRL.UNKNOWN;

            // вся продукция из заявки по документу
            dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "KMC", DataViewRowState.CurrentRows);

            iZMax = dvZ.Count;
            if (iZMax <= 0)
            {
                nDokState = AppC.RC_CANCEL;
                lstProt.Add("*** Заявка отсутствует! ***");
            }
            else
                bZVKPresent = true;

            /// вся продукция из ТТН по документу
                //dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KMC,EMK DESC", DataViewRowState.CurrentRows);

            if (s1Pallet.Length > 0)
                sRf = String.Format("{0} AND (SSCC='{1}')", sRf, s1Pallet);

            dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "KMC,EMK DESC", DataViewRowState.CurrentRows);
            iTMax = dvT.Count;
            if (iTMax <= 0)
            {
                nDokState = AppC.RC_CANCEL;
                lstProt.Add("*** ТТН отсутствует! ***");
            }
            dvZ.BeginInit();
            dvT.BeginInit();

            if (nDokState == AppC.RC_OK)
            {
                foreach (DataRowView dr in dvZ)
                {// сброс всех статусов
                    dr["READYZ"] = NSI.READINESS.NO;
                }
                foreach (DataRowView dr in dvT)
                {// сброс всех назначений строк
                    dr["DEST"] = NSI.DESTINPROD.UNKNOWN;
                    dr["NPP_ZVK"] = -1;
                }


                lstProt.Add("<->----- ТТН ------<->");

                sOldKMC = "";
                fE_KMC = 0;
                nM_KMC = 0;
                while (i < iTMax)
                {

                    if ((int)dvT[i]["DEST"] != (int)NSI.DESTINPROD.UNKNOWN)
                    {// строка накладной уже обработана
                        i++;
                        continue;
                    }

                    drC = dvT[i].Row;
                    // что за объект в строке?
                    xR = new RowObj(drC);

                    if (xR.AllFlags == (int)AppC.OBJ_IN_DROW.OBJ_NONE)
                    {
                        lstProt.Add("Нет продукции/SSCC");
                        i++;
                        continue;
                    }
                    if (!xR.IsEAN)
                    {// один из SSCC
                        sFlt = "";
                        if (xR.IsSSCCINT)
                            sFlt += String.Format("AND(SSCCINT='{0}')", xR.sSSCCINT);
                        if (xR.IsSSCC)
                            sFlt += String.Format("AND(SSCC='{0}')", xR.sSSCC);

                        DataView dvZSC = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf + sFlt, "SSCC,SSCCINT",
                            DataViewRowState.CurrentRows);
                        if (dvZSC.Count > 0)
                            dvZSC[0].Row["READYZ"] = NSI.READINESS.FULL_READY;
                        else
                        {// SSCC не найден
                            lstProt.Add(String.Format("Подд.{0} {1}:нет заявки", xR.sSSCCINT, xR.sSSCC));
                        }
                        i++;
                        continue;
                    }

                    sc.sEAN = (string)drC["EAN13"];
                    sc.sKMC = (string)drC["KMC"];
                    sc.nKrKMC = (int)drC["KRKMC"];

                    sc.bVes = ((int)(drC["SRP"]) > 0) ? true : false;

                    sc.fEmk = (FRACT)drC["EMK"];
                    sc.nParty = (string)drC["NP"];
                    sc.dDataIzg = DateTime.ParseExact((string)drC["DVR"], "yyyyMMdd", null);
                    //sc.nTara = (string)drC["KRKT"];
                    sc.nTara = (string)drC["KTARA"];

                    sc.nRecSrc = (int)NSI.SRCDET.CR4CTRL;

                    //td1 = Environment.TickCount;

                    iStart = dvZ.Find(sc.sKMC);
                    //iStart = dvZ.Find(sc.sKMC);
                    if (iStart != -1)
                        nRet = EvalZVKMest(ref sc, dvZ, iStart, iZMax);
                    //nRet = LookAtZVK(ref sc, dvZ, iStart, iZMax);
                    else
                        nRet = AppC.RC_NOEAN;

                    //tc += (Environment.TickCount - td1);

                    iCur = -1;
                    if (nRet == AppC.RC_OK)
                    {// есть или единички или такая емкость
                        //td1 = Environment.TickCount;

                        if ((bIsKMPL) || true)
                        {
                            sc.nMest = (int)drC["KOLM"];
                            sc.fVsego = (FRACT)drC["KOLE"];
                            EvalZVKStateNew(ref sc, drC);

                            if (sOldKMC != sc.sKMC)
                            {// смена кода
                                iCurSaved = i;

                                //EvalEnteredKol(dvT, i, ref sc, sc.sKMC, sc.fEmk, out nM, out fV, true);

                                iCur = i;

                                //nRet = EvalDiffZVK(ref sc, dvZ, dvT, lstProt, iStart, iZMax, ref iCur, iTMax, nM, fV);

                                //if (nDokState != AppC.RC_CANCEL)
                                //{
                                //    if (nRet != AppC.RC_OK)
                                //        nDokState = nRet;
                                //}
                                sOldKMC = sc.sKMC;
                            }
                        }
                        else
                        {
                            EvalEnteredKol(dvT, i, ref sc, sc.sKMC, sc.fEmk, out nM, out fV, true);

                            //td2 = Environment.TickCount;
                            //tc1 += (td2 - td1);

                            iCur = i;
                            nRet = EvalDiffZVK(ref sc, dvZ, dvT, lstProt, iStart, iZMax, ref iCur, iTMax, nM, fV);

                            //tc2 += (Environment.TickCount - td2);

                            if (nDokState != AppC.RC_CANCEL)
                            {
                                if (nRet != AppC.RC_OK)
                                    nDokState = nRet;
                            }
                        }
                    }
                    else
                    {
                        switch (nRet)
                        {

                            case AppC.RC_NOEAN:
                                // код отсутствует
                                s1 = "";
                                xProt = "";
                                fE = -100;
                                break;
                            case AppC.RC_NOEANEMK:
                                // емкость отсутствует
                                s1 = "емк.";
                                xProt = sc.fEmk;
                                fE = sc.fEmk;
                                break;
                            case AppC.RC_BADPARTY:
                                // нет партии
                                s1 = "парт.";
                                xProt = sc.nParty;
                                fE = sc.fEmk;
                                break;
                            default:
                                s1 = String.Format("емк={0}", sc.fEmk);
                                xProt = String.Format("парт-{0}", sc.nParty);
                                fE = sc.fEmk;
                                break;
                        }
                        nDokState = AppC.RC_CANCEL;

                        lstProt.Add(String.Format("_{0} {1} {2}:нет заявки", sc.nKrKMC, s1, xProt));
                        iCur = SetTTNState(dvT, ref sc, fE, NSI.DESTINPROD.USER, i, iTMax);
                    }
                    if (iCur != -1)
                        i = iCur;

                    i++;
                }


                if (s1Pallet.Length > 0)
                    return (nDokState);


                //t2 = Environment.TickCount;

                lstProt.Add("<->---- Заявка ----<->");
                sOldKMC = "";
                fE_KMC = 0;
                nM_KMC = 0;
                bGood_KMC = true;
                nBad_NPP = 0;
                for (i = 0; i < dvZ.Count; i++)
                {

                    drC = dvZ[i].Row;
                    xR = new RowObj(drC);
                    sKMC = (string)drC["KMC"];

                    if (sOldKMC != sKMC)
                    {// смена кода
                        if (nBad_NPP > 1)
                        {
                            Total4KMC(dvZ[i-1].Row, sOldKMC, true, true, lstProt, nM_KMC, fE_KMC);
                        }
                        sOldKMC = sKMC;
                        fE_KMC = 0;
                        nM_KMC = 0;
                        bGood_KMC = true;
                        nBad_NPP = 0;
                    }

                    try
                    {
                        if (xR.IsEAN)
                        {
                            if ((FRACT)drC["EMK"] > 0)
                                nM_KMC += (int)(drC["KOLM"]);
                            else
                                fE_KMC += (FRACT)(drC["KOLE"]);

                            if ((NSI.READINESS)drC["READYZ"] != NSI.READINESS.FULL_READY)
                            {
                                nDokState = AppC.RC_CANCEL;
                                bGood_KMC = false;
                                nBad_NPP++;
                                Total4KMC(drC, sKMC, xR.IsEAN, false, lstProt, (int)(drC["KOLM"]), (FRACT)(drC["KOLE"]));
                            }
                        }
                        else
                        {
                            if (xR.IsSSCCINT || xR.IsSSCC)
                            {
                                if ((NSI.READINESS)drC["READYZ"] != NSI.READINESS.FULL_READY)
                                    lstProt.Add(String.Format("Подд.{0} {1}:нет ввода", xR.sSSCCINT, xR.sSSCC));
                            }
                        }
                    }
                    catch
                    {
                    }


                }
                if (i > 0)
                {
                    if (nBad_NPP > 1)
                    {
                        i--;
                        drC = dvZ[i].Row;
                        xR = new RowObj(drC);
                        //sKMC = (string)drC["KMC"];
                        Total4KMC(drC, sOldKMC, xR.IsEAN, true, lstProt, nM_KMC, fE_KMC);
                    }
                }

            }

            if (nDokState == AppC.RC_CANCEL)
            {
                drD["DIFF"] = NSI.DOCCTRL.ERRS;
                lstProt.Add("!!!===! ОШИБКИ Контроля !===!!!");
            }
            else if (nDokState == AppC.RC_WARN)
            {
                drD["DIFF"] = NSI.DOCCTRL.WARNS;
                lstProt.Add("== Результат - Предупреждения ==");
            }
            else if (nDokState == AppC.RC_OK)
            {
                drD["DIFF"] = NSI.DOCCTRL.OK;
                lstProt.Add("=== Результат - нет ошибОК ===");
            }

            dvT.EndInit();
            dvZ.EndInit();

            //t3 = Environment.TickCount;
            //tsDiff = new TimeSpan(0, 0, 0, 0, t3 - t1);

            //lstProt.Add(String.Format("Всего - {0}, заявка - {1}, ZVK-{2}, TTN-{3}, Diff-{4}",
            //    tsDiff.TotalSeconds,
            //    new TimeSpan(0, 0, 0, 0, t3 - t2).TotalSeconds,
            //    new TimeSpan(0, 0, 0, 0, tc).TotalSeconds,
            //    new TimeSpan(0, 0, 0, 0, tc1).TotalSeconds,
            //    new TimeSpan(0, 0, 0, 0, tc2).TotalSeconds));
            //MessageBox.Show(new TimeSpan(0, 0, 0, 0, tss).TotalSeconds.ToString());
            return(nDokState);
        }

        private void Total4KMC(DataRow drC, string sKMC, bool bIsProd, bool bFullKMC, List<string> lstProt, int nM_KMC, FRACT fE_KMC)
        {
            int
                nKrKMC = 0,
                nDiff,
                nM;
            string
                sFlt,
                s2,
                s1;
            FRACT
                fDiff,
                fV;
            DataView
                dv;


            try
            {
                if (bIsProd)
                {
                    nKrKMC = (int)drC["KRKMC"];
                    sFlt = String.Format("{0} AND (KMC='{1}')", xCDoc.DefDetFilter(), sKMC);
                    if (!bFullKMC)
                        sFlt += String.Format(" AND (NPP_ZVK={0})", drC["NPP"]);

                    dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt,
                        sFlt, "EMK, DVR, NP DESC", DataViewRowState.CurrentRows);

                    nM = 0;
                    fV = 0;
                    foreach (DataRowView drv in dv)
                    {
                        if ((FRACT)(drv.Row["EMK"]) > 0)
                            nM += (int)(drv.Row["KOLM"]);
                        else
                            fV += (FRACT)(drv.Row["KOLE"]);
                    }

                    nDiff = nM_KMC - nM;
                    fDiff = fE_KMC - fV;

                    s1 = ((NSI.READINESS)drC["READYZ"] == NSI.READINESS.NO) ? "нет ввода" : "частично";
                    s2 = (bFullKMC) ? "\x3A3" : drC["NPP"].ToString();
                    if ((FRACT)drC["EMK"] > 0)
                        lstProt.Add(String.Format("({5})_{0}:{1}:{3}-{2}={4}М", nKrKMC, s1, nM, nM_KMC, nDiff, s2));
                    else
                        lstProt.Add(String.Format("({5})_{0}:{1}:{3}-{2}={4}Ед", nKrKMC, s1, fV, fE_KMC, fDiff, s2));
                }
            }
            catch
            {
            }
            
        }



    
    
    }


}    
