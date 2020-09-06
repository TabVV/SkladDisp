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
using SkladAll;


namespace SkladGP
{
    public sealed partial class NSI : NSIAll
    {

        // инициализация для таблиц SSCC
        public void InitTableSSCC(DataGrid dg)
        {
            dsM.Tables.Add(DT[BD_SSCC].dt);

            dsM.Relations.Add(REL2SSCC, 
                DT[BD_DOCOUT].dt.Columns["SYSN"], 
                DT[BD_SSCC].dt.Columns["SYSN"]);

            dg.SuspendLayout();
            DT[BD_SSCC].dg = dg;
            CreateTableStylesSSCC(dg);
            dg.DataSource = DT[BD_SSCC].dt;
            ChgGridStyle(BD_SSCC, GDET_SCAN);
            dg.ResumeLayout();
        }


        // стили таблицы SSCC
        private void CreateTableStylesSSCC(DataGrid dg)
        {
            //DataGridTextBoxColumn
            //    sColk;
            ServClass.DGTBoxColorColumn
                sC;
            Color
                colSpec = Color.PaleGoldenrod,
                colGreen = Color.LightGreen;
            double
                nKoef = Screen.PrimaryScreen.Bounds.Width / 240.0;
            int
                nWMAdd = 0;
#if WMOBILE
            nWMAdd = 4;
#else
            nWMAdd = 0;
#endif


            dg.TableStyles.Clear();
            // Для результатов сканирования
            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = GDET_SCAN.ToString();

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SSCC);
            sC.MappingName = "NPODDZ";
            sC.HeaderText = "№ ";
            sC.Width = (int)(25 * nKoef + nWMAdd); ;
            sC.NullText = "";
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SSCC);
            sC.MappingName = "SSCC";
            sC.HeaderText = "Номер SSCC";
            sC.Width = (int)(146 * nKoef + nWMAdd);
            sC.AlternatingBackColor = C_READY_TTN;
            sC.AlternatingBackColorSpec = C_TNSFD_TTN;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SSCC);
            sC.MappingName = "MONO";
            sC.HeaderText = "M";
            sC.Width = (int)(18 * nKoef + nWMAdd - 6);
            sC.AlternatingBackColor = C_READY_TTN;
            sC.AlternatingBackColorSpec = C_TNSFD_TTN;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SSCC);
            sC.MappingName = "STATE";
            sC.HeaderText = "Сост";
            sC.Width = (int)(40 * nKoef + nWMAdd - 6);
            sC.AlternatingBackColor = C_READY_TTN;
            sC.AlternatingBackColorSpec = C_TNSFD_TTN;
            ts.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);

        }

    }

    public partial class MainF : Form
    {

        // переход на вкладку SSCC
        private void EnterInSSCC()
        {
            string
                sRf;
            DataView
                dv;
            //DataTable
              //  dtD = ((DataTable)this.dgSSCC.DataSource);

            //ShowRegVvod();
            if (drShownDoc != xCDoc.drCurRow)
            {// сменился документ
                NewDoc();
            }

            sRf = xCDoc.DefDetFilter();
            dv = new DataView(xNSI.DT[NSI.BD_SSCC].dt, sRf, "", DataViewRowState.CurrentRows);

            lDocInfSSCC.Text = CurDocInf(xCDoc.xDocP);
            lSSCCState.Text = String.Format("Всего SSCC = {0}", dv.Count);

            //if (xCDoc.xDocP.DType.MoveType == AppC.MOVTYPE.AVAIL)
            //{// инвентаризации - всегда в ТТН
            //    ChgDetTable(null, NSI.BD_DOUTD);
            //}

            //tCurrPoddon.Text = xCDoc.xNPs.Current.ToString();
            dgSSCC.Focus();
        }

        private void dgSSCC_CurrentCellChanged(object sender, EventArgs e)
        {
            int
                i = 0;
            if (xCDoc.drCurRow != null)
            {
            }
        }

        // Обработка сканирования на панели Документов
        private void ProceedScanSSCC(ScanVarGP xSc, ref PSC_Types.ScDat s)
        {
            int
                nRet = AppC.RC_OK;
            string
                sH,
                sPar,
                sErr = "";
            CurLoad
                ret = null;
            ServerExchange
                xSE = new ServerExchange(this);

            if ((s.xSCD.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0)
            {
                AddSSCC2SSCCTable(s.xSCD.Dat, 0, xCDoc, 0,  0, 1);
            }

        }



        // Обработка клавиш
        private bool SSCCList_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool
                ret = false;// клавиша еще не обработана
            int
                nRet = AppC.RC_OK;

            if ((nFunc > 0) && (ret == false))
            {//в режиме просмотра
                ret = true;
                switch (nFunc)
                {
                    case AppC.F_UPLD_DOC:
                        // повторная выгрузка

                        //ServerExchange xSE = new ServerExchange(this);

                        //xFPan.UpdateHelp("Идет выгрузка данных...");

                        //string sL = UpLoadDoc(xSE, ref nRet);

                        //if ((xSE.ServerRet != AppC.EMPTY_INT) && (xSE.ServerRet != AppC.RC_OK))
                        //{// операция выгрузки не прошла на сервере (содержательная ошибка)

                        //    Srv.ErrorMsg(sL, true);
                        //}
                        DelTTN4Doc(xCDoc.drCurRow);
                        UploadDocs2Server(AppC.F_INITREG, new KeyEventArgs(Keys.Enter), ref ehCurrFunc);
                        ret = false;
                        break;

                    case AppC.F_LOAD_DOC:
                    //case AppC.F_CNTSSCC:
                        if (IsDoc4Check())
                            LoadAllSSCC();
                        break;

                    case AppC.F_DEL_ALLREC:
                    case AppC.F_DEL_REC:
                        //if (IsDoc4Check())
                        //    break;
                        DelDetail4Doc(dgSSCC, nFunc);
                        //ShowStatDoc();
                        //ShowOperState(xCDoc.xOper);
                        break;
                    default:
                        nFunc = 0;
                        ret = false;
                        break;
                }
            }

            if ((nFunc <= 0) && (ret == false))
            {// для режима редактирования
                ret = true;
                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        //Back2Main();
                        tcMain.SelectedIndex = PG_SCAN;
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            e.Handled = bSkipChar = ret;
            return (ret);
        }


        private void LoadAllSSCC()
        {
            int
                nRet = AppC.RC_OK,
                nK,
                nRezCtrl;
            string
                sE,
                sSSCC = "";
            PSC_Types.ScDat 
                scD = scCur;
            ServerExchange
                xSE = new ServerExchange(this);

            DelTTN4Doc(xCDoc.drCurRow);

            DataRow[] drSSCCAll = xCDoc.drCurRow.GetChildRows(xNSI.dsM.Relations[NSI.REL2SSCC]);
            foreach (DataRow drSSCC in drSSCCAll)
            {
                if ((int)drSSCC["IN_TTN"] > 0)
                {
                    sSSCC = (string)drSSCC["SSCC"];
                    nRet = ConvertSSCC2Lst(xSE, sSSCC, ref scD, false);
                    if ((nRet == AppC.RC_OK) || (nRet == AppC.RC_MANYEAN))
                    {
                        //if (nRet == AppC.RC_MANYEAN)
                        //    nRet = AddGroupDet(AppC.RC_MANYEAN, (int)NSI.SRCDET.SSCCT, sSSCC);

                        nRet = AddGroupDet(AppC.RC_MANYEAN, (int)NSI.SRCDET.SSCCT, sSSCC, false);
                    }
                    if (nRet != AppC.RC_OK)
                    {
                        Srv.ErrorMsg("Ошибка загрузки " + sSSCC, "Код-" + nRet.ToString(), true);
                        DelTTN4Doc(xCDoc.drCurRow);
                        break;
                    }

                }
            }
            if (nRet == AppC.RC_OK)
            {

                if (bZVKPresent)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    xNSI.DT[NSI.BD_DIND].dt.AcceptChanges();
                    List<DataRow>
                        lDD = new List<DataRow>();
                    DataView
                        dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, xCDoc.DefDetFilter(), "KMC", DataViewRowState.CurrentRows);
                    //for (int i = 0; i < dvZ.Count; i++)
                    //{
                    //    if ( PSC_Types.IsTara((string)dvZ[i].Row["EAN13"], (int)dvZ[i].Row["KRKMC"]) )
                    //    {
                    //        lDD.Add(dvZ[i].Row);
                    //        dvZ[i].Row.Delete();
                    //    }
                    //}

                    foreach(DataRowView drv in dvZ)
                    {
                        if (PSC_Types.IsTara((string)drv.Row["EAN13"], (int)drv.Row["KRKMC"]))
                        {
                            lDD.Add(drv.Row);
                            drv.Row.Delete();
                        }
                    }


                    try
                    {
                        xInf = new List<string>();
                        nRezCtrl = ControlDocZVK(null, xInf, "");
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                        foreach (DataRow dr in lDD)
                            dr.RejectChanges();
                    }
                    xHelpS.ShowInfo(xInf, ref ehCurrFunc);
                }
            }
        }



        // --- удаление в подчиненном списке строки
        private void DelDetail4Doc(DataGrid dgDel, int nFunc)
        {
            DataTable dtDel = ((DataTable)dgDel.DataSource);
            if (dtDel != xNSI.DT[NSI.BD_DIND].dt)
            {
                DataView dvDetail = dtDel.DefaultView;
                int ret = dvDetail.Count;
                if (ret >= 1)
                {
                    DataRow
                        dr4Del = dvDetail[dgDel.CurrentRowIndex].Row;
                    if (nFunc == AppC.F_DEL_REC)
                    {
                        dtDel.Rows.Remove(dr4Del);
                        if (dtDel == xNSI.DT[NSI.BD_DOUTD].dt)
                            ClearZVKState(dr4Del["KMC"].ToString(), dr4Del["NPP_ZVK"]);
                    }
                    else
                    {
                        DialogResult dr = MessageBox.Show("Отменить удаление всех (Enter)?\r\n(ESC) - все удалить без сомнений",
                            "Удаляются все строки!",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dr != DialogResult.OK)
                        {
                            string sRel = (dtDel == xNSI.DT[NSI.BD_DOUTD].dt) ? NSI.REL2TTN : NSI.REL2SSCC;

                            DataRow[] drMDetZ = xCDoc.drCurRow.GetChildRows(xNSI.dsM.Relations[sRel]);
                            foreach (DataRow drDel in drMDetZ)
                            {
                                dtDel.Rows.Remove(drDel);
                            }
                            if (dtDel == xNSI.DT[NSI.BD_DOUTD].dt)
                                ClearZVKState("", null);
                        }
                    }
                    if (dtDel == xNSI.DT[NSI.BD_DOUTD].dt)
                    {
                        ChangeDetRow(false);
                        xCDoc.xOper = new CurOper(true);
                        xCDoc.drCurRow["DIFF"] = NSI.DOCCTRL.UNKNOWN;
                    }
                }
            }
        }




        public void MayAddSSCC(ref PSC_Types.ScDat sc)
        {
            if ((sc.xSCD.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0)
            {
                if (xCLoad != null)
                {
                    if ((sc.sSSCC == xCLoad.sSSCC))
                    {
                        if (((sc.nMest == sc.nMestPal) && (xCLoad.dtZ.Rows.Count == 1)) ||
                             ((xCLoad.dtZ.Rows.Count > 1)))
                        {
                            AddSSCC2SSCCTable(sc.sSSCC, 0, xCDoc, xCLoad.dtZ.Rows.Count, 0, 1);
                        }
                    }
                }
            }
        }



        // поиск в списке SSCC отмаркированного поддона
        public DataRow FindSCCTInSSCCList(string sSSCC, int nID, ref int nPP)
        {
            DataRow
                ret = null;
            string
                sRf = ((DataTable)dgSSCC.DataSource).DefaultView.RowFilter;
            DataView
                dv;

            sRf = String.Format("SYSN={0} AND SSCC='{1}'", nID, sSSCC);
            dv = new DataView(xNSI.DT[NSI.BD_SSCC].dt, sRf, "", DataViewRowState.CurrentRows);
            if (dv.Count >= 1)
            {
                ret = dv[0].Row;
            }
            else
            {
                if (nPP < 0)
                {
                    sRf = String.Format("SYSN={0}", nID);
                    dv = new DataView(xNSI.DT[NSI.BD_SSCC].dt, sRf, "", DataViewRowState.CurrentRows);
                    if (nPP <= 0)
                    {
                        if (dv.Count >= 1)
                            nPP = (int)(dv[dv.Count - 1].Row["NPODDZ"]) + 1;
                        else
                            nPP = 1;
                    }
                }
            }
            return (ret);
        }

        private int GetMaxInRows(DataTable dt, string sField, string sFilt)
        {
            int
                ret = 0;
            string
                sRf;

            try
            {
                DataView
                    dvZ = new DataView(dt, sFilt, sField, DataViewRowState.CurrentRows);
                if (dvZ.Count > 0)
                {
                    ret = (int)(dvZ[dvZ.Count - 1].Row[sField]);
                }
            }
            catch { }

            return (ret);
        }

        // добавление очередного SSCC в список SSCC
        public DataRow AddSSCC2SSCCTable(string sSSCC, int nPP, CurDoc xCDoc, int SQUs, int FromSRV, int FromTTN)
        {
            bool 
                bAddNew;
            DataRow
                ret = null;
            int
                nKey = (int)xCDoc.drCurRow["SYSN"];

            try
            {
                bAddNew = false;
                ret = FindSCCTInSSCCList(sSSCC, nKey, ref nPP);
                if (ret == null)
                {
                    bAddNew = true;
                    ret = xNSI.DT[NSI.BD_SSCC].dt.NewRow();
                    ret["SYSN"] = nKey;
                    ret["SSCC"] = sSSCC;
                    ret["MONO"] = SQUs;
                    ret["STATE"] = 1;

                    if (nPP == 0)
                        nPP = GetMaxInRows(xNSI.DT[NSI.BD_SSCC].dt, "NPODDZ", xCDoc.DefDetFilter()) + 1;
                    ret["NPODDZ"] = nPP;
                    

                }
                ret["IN_ZVK"] = FromSRV;
                ret["IN_TTN"] = FromTTN;
                if (bAddNew)
                {
                    xNSI.DT[NSI.BD_SSCC].dt.Rows.Add(ret);
                }
            }
            catch
            {
                Srv.ErrorMsg("Ошибка добавления SSCC!");
            }
            return (ret);
        }






        public int ProceedSSCC(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        {
            ServerExchange
                xSE = null;
            return (ProceedSSCC(xSc, ref scD, xSE));
        }


        public int ProceedSSCC(ScanVarGP xSc, ref PSC_Types.ScDat scD, ServerExchange xSE)
        {
            int
                ret = AppC.RC_OK;                       // по умолчанию - обработку скана заканчиваем
            bool
                bExt,
                bCallExch = false,
                bMaySet = true;
            string
                sSSCC = "";
            DataRow
                dr = null;
            RowObj
                xR;
            DialogResult
                dRez;
            //ServerExchange 
            //    xSE = new ServerExchange(this);


            if (xSE == null)
            {
                xSE = new ServerExchange(this);
                bCallExch = true;
            }

            // Внешние SSCC
            bExt = ((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC_EXT) > 0) ? true : false;

            sSSCC =
                scD.sSSCC =
                xCDoc.xOper.SSCC = xSc.Dat;

            if (bCallExch)
            {
                ret = ConvertSSCC2Lst(xSE, xSc.Dat, ref scD, false);
            }
            else
                ret = (xCLoad.dtZ.Rows.Count == 1) ? AppC.RC_OK : AppC.RC_MANYEAN;


            if (xSE.AnswerPars.ContainsKey("ADRCELL"))
            {
                AddrInfo xA = new AddrInfo(xSE.AnswerPars["ADRCELL"], xSm.nSklad);
                if (xA.Addr.Length > 0)
                {
                    if (!xCDoc.xOper.IsFillSrc())
                        xCDoc.xOper.SetOperSrc(xA, xCDoc.xDocP.nTypD, true);
                }
            }



            switch (xCDoc.xDocP.TypOper)
            {
                case AppC.TYPOP_PRMK:
                case AppC.TYPOP_MARK:
                    if (!bExt && (xCDoc.xDocP.TypOper == AppC.TYPOP_MARK))
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
                            SetOverOPR(false, drDet);
                        }
                    }
                    break;
                case AppC.TYPOP_MOVE:
                    // операция перемещения

                    if (!xCDoc.xOper.IsFillSrc() && !xCDoc.xOper.IsFillDst() &&
                        (xSm.xAdrFix1 == null))
                    {
                            Srv.ErrorMsg("Адрес не указан!", true);
                            return (ret);
                    }

                    if ((ret == AppC.RC_OK) || (ret == AppC.RC_MANYEAN))
                    {
                        if (xCLoad.dtZ.Rows.Count == 1)
                        {// однородный поддон
                            if (xCDoc.xOper.IsFillSrc() && ((xCDoc.xOper.xAdrSrc.nType & ADR_TYPE.SSCC) > 0))
                            {
                                Srv.ErrorMsg("SSCC-Адрес недопустим!", true);
                                break;
                            }
                            else
                            {
                                // 090.08.18
                                //AddDet1(ref scD, out dr);
                                ret = AppC.RC_WARN;
                            }
                        }
                        else
                        {// сборный поддон идет одной строкой
                            dr = AddDetSSCC(xSc, xCDoc.nId, ScanVarGP.BCTyp.SP_SSCC_EXT, "");
                            if (dr is DataRow)
                            {
                                ret = AppC.RC_OK;
                                if (AppPars.ShowSSCC)
                                {
                                    ShowSSCCContent(xCLoad.dtZ, xSc.Dat, xSE, xCDoc.xOper.xAdrSrc, ref ehCurrFunc);
                                }
                            }
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
                    //IsOperReady(dr);
                    IsOperReady();
                    break;
                case AppC.TYPOP_DOCUM:
                case AppC.TYPOP_KMPL:
                    // документ или комплектация
                    if ((!xCDoc.xOper.IsFillSrc()
                        && (xPars.UseAdr4DocMode)
                        && (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.RASHOD)))
                    {
                            Srv.ErrorMsg("Адрес не указан!", true);
                            return (ret);
                    }

                    if (ret == AppC.RC_OK)
                    {
                        if (xCLoad.dtZ.Rows.Count == 1)
                        {// монопаллет, продолжается обычная обработка скана
                            if (!bCallExch)
                            {
                                AddDet1(ref scD, out dr);
                            }
                            else
                                ret = AppC.RC_WARN;
                        }
                    }
                    else if (ret == AppC.RC_MANYEAN)
                    {// для сборного поддона
                        if (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.AVAIL)
                            // в инвентаризацию - добавление без запроса
                            ret = AddGroupDet(ret, (int)NSI.SRCDET.SSCCT, xSc.Dat);
                        else
                        {
                            if (AppPars.ShowSSCC)
                                WaitScan4Func(AppC.F_CNTSSCC, "Содержимое SSCC", "Отсканируйте SSCC", xSc);
                        }
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

        // добавление в список ТТН отмаркированного/сборного поддона
        private DataRow AddDetSSCC(ScanVarGP xSc, int nId, ScanVarGP.BCTyp xT, string sN)
        {
            bool
                bDataRowNew;
            int
                nM = 0;
            FRACT
                fE = 0;

            DateTime
                dtCr;
            DataRow
                ret = null,
                dr;
            PSC_Types.ScDat
                sc = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
            try
            {

                sc.sSSCC = xSc.Dat;
                sc.nNomPodd = int.Parse(xSc.Dat.Substring(13, 7));

                //sc.sN = ((xT & ScanVarGP.BCTyp.SP_SSCC_EXT) > 0)?"Маркир.":"Скомпл.";
                //sc.sN = String.Format("{0} п-н №{1}", sc.sN, sc.nNomPodd);
                sc.sN = String.Format("SSCC № {0}...{1}", sc.sSSCC.Substring(2, 4), sc.sSSCC.Substring(15, 5));

                //sc.nKrKMC = 60 + int.Parse(xSc.Dat.Substring(2, 1));
                sc.nKrKMC = AppC.KRKMC_MIX;

                EvalGroupDetStat(xCLoad.dtZ, out nM, out fE);
                //sc.nMest = nM;
                //sc.fVsego = fE;
                sc.nMest = 1;
                sc.fVsego = 0;

                sc.nNomPodd = int.Parse(xSc.Dat.Substring(12, 7));

                sc.nKolM_alr = nM;
                sc.fKolE_alr = fE;
                scCur = sc;
                //ret = AddVirtProd(ref sc);
                bDataRowNew = AddDet1(ref scCur, out dr);
                scCur = sc;
                SetDopFieldsForEnter(true);
                ret = dr;
            }
            catch //(Exception e)
            {
                ret = null;
            }
            return (ret);
        }


        private void WhatSSCCContent()
        {
            int
                nRet;
            string
                sSSCC;
            ServerExchange
                xSE = new ServerExchange(this);

            try
            {
                if (tcMain.SelectedIndex == PG_SCAN)
                {
                    sSSCC = (string)drDet["SSCC"];
                    try
                    {
                        if ((xCLoad.sSSCC == sSSCC) && (xCLoad.sComLoad == AppC.COM_ZSC2LST) && (xCLoad.dtZ.Rows.Count > 0))
                        {
                            ShowSSCCContent(xCLoad.dtZ, sSSCC, null, xCDoc.xOper.xAdrSrc, ref ehCurrFunc);
                            return;
                        }
                    }
                    catch { }
                    if (scCur.nKrKMC == AppC.KRKMC_MIX)
                    {
                        PSC_Types.ScDat scD = scCur;
                        nRet = ConvertSSCC2Lst(xSE, sSSCC, ref scD, false);
                        if ((nRet == AppC.RC_OK) || (nRet == AppC.RC_MANYEAN))
                        {
                            ShowSSCCContent(xCLoad.dtZ, sSSCC, xSE, xCDoc.xOper.xAdrSrc, ref ehCurrFunc);
                            return;
                        }
                    }
                }
            }
            catch { }

            xCDoc.sSSCC = "";
            WaitScan4Func(AppC.F_CNTSSCC, "Содержимое SSCC", "Отсканируйте SSCC");
        }


        /// подготовить и отобразить содержимое SSCC
        private void ShowSSCCContent(DataTable dtZ, string sSSCC, ServerExchange xSE, AddrInfo xA, ref Srv.CurrFuncKeyHandler ehKeybHdl)
        {
            int
                nTotMest,
                nM;
            string
                sUser = "",
                sFIO = "";
            char
                cExCh = '=';
            DataRow
                xd;
            DateTime
                dVyr;
            List<string>
                lKMC = new List<string>(),
                lCur = new List<string>();
            FRACT
                fTotEd,
                fE;

            nTotMest = 0;
            fTotEd = 0;
                try
                {
                    string sA = "";
                    try
                    {
                        sA = xA.AddrShow;
                    }
                    catch{}

                    try
                    {
                        sUser = xSE.AnswerPars["USER"];
                        if ((sUser == AppC.SUSER) || (sUser == AppC.GUEST))
                            sFIO = (sUser == AppC.SUSER) ? "Admin" : "Работник склада";
                        else
                        {
                            NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_USER, new object[] { sUser }, "NMP");
                            if (zS.bFind)
                                sFIO = sUser + '-' + zS.sName;
                            else
                                sFIO = sUser;
                        }
                    }
                    catch
                    {
                        sFIO = "";
                    }

                    xInf = aKMCName(String.Format("{0} ({1}) {2}", sSSCC.Substring(2), sA, sFIO), false);
                    xInf.Add(aKMCName("", true, cExCh)[0]);

                    if (dtZ.Rows.Count > 0)
                    {
                        DataView
                            dv = new DataView(dtZ, "", "KMC", DataViewRowState.CurrentRows);

                        foreach (DataRowView dva in dv)
                        {
                            xd = dva.Row;
                            try
                            {
                                nM = (int)xd["KOLM"];
                            }
                            catch{ nM = 0; }
                            try
                            {
                                fE = (FRACT)xd["KOLE"];
                            }
                            catch{ fE = 0; }

                            try
                            {
                                dVyr = DateTime.ParseExact((string)xd["DVR"], "yyyyMMdd", null);
                            }
                            catch { dVyr = DateTime.MinValue; }
                            nTotMest += nM;
                            fTotEd += fE;

                            if (!lKMC.Contains((string)xd["KMC"]))
                                lKMC.Add((string)xd["KMC"]);

                            lCur.Add(String.Format("{0,4} {1}", xd["KRKMC"], xd["SNM"]));
                            lCur.Add(String.Format("{0} {1,6} {2,5:F1} {3,6} {4,7}", dVyr.ToString("dd.MM"), xd["NP"], xd["EMK"], nM, fE));
                            lCur.Add(aKMCName("", true)[0]);
                        }
                        xInf.Add(String.Format("Всего SKU: {0}  Мест:{1}  Ед.:{2}", lKMC.Count, nTotMest, fTotEd));
                        xInf.Add(aKMCName("", true, cExCh)[0]);
                        xInf.Add(" Двыр   №пт  Емк    Мест     Ед.");
                        xInf.Add(aKMCName("", true, cExCh)[0]);
                        xInf.AddRange(lCur);
                    }
                    else
                    {
                        xInf.Add("Нет сведений!");
                        Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                    }
                    //xHelpS.ShowInfo(xInf, ref ehKeybHdl);
                    Srv.HelpShow
                        xSSCCCont = new Srv.HelpShow(this);
                    //xSSCCCont.ShowInfo(xInf, ref ehKeybHdl);
                    xSSCCCont.ShowInfo(null,
                                    (tcMain.SelectedIndex == PG_DOC) ? dgDoc :
                                    (tcMain.SelectedIndex == PG_SCAN) ? dgDet : null, 
                                    xInf, ref ehKeybHdl);

                }
                catch (Exception ex)
                {
                    int ggg = 999;
                }


        }

        // ожидание сканирования для спецрежимов
        private void WaitScan4Func(int nWaitMode, string sMsg, string sHeader)
        {
            WaitScan4Func(nWaitMode, sMsg, sHeader, null);
        }

        // ожидание сканирования для спецрежимов
        private void WaitScan4Func(int nWaitMode, string sMsg, string sHeader, ScanVarGP xSc)
        {
            nSpecAdrWait = nWaitMode;
            tbPanP2G.Visible = false;
            xFPan.IFaceReset(true);
            if ((nWaitMode == AppC.F_GENSCAN)
                || (nWaitMode == AppC.F_SIMSCAN))
                xFPan.InfoHeightUp(true, 2);
            xFPan.ShowP(6, 28, sMsg, sHeader);

            if (nWaitMode == AppC.F_SIMSCAN)
            {
                tbPanP1G.Focus();
            }

            ehCurrFunc += new Srv.CurrFuncKeyHandler(Keys4FixAddr);
            if (xSc != null)
            {
                SpecScan(xSc);
                W32.keybd_event(W32.VK_ENTER, W32.VK_ENTER, 0, 0);
                W32.keybd_event(W32.VK_ENTER, W32.VK_ENTER, W32.KEYEVENTF_KEYUP, 0);
            }
        }



        /// готовность заказа
        public bool IsZkzReady(bool bNeedAsk)
        {
            bool
                bRet = AppC.RC_OKB;
            string
                sRf;
            DataView
                dvM;

            if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
            {
                //sRf = xCDoc.DefDetFilter() + String.Format("AND(READYZ<>{0})", (int)NSI.READINESS.FULL_READY);

                sRf = xCDoc.DefDetFilter() + String.Format(" AND (NPODDZ={0}) AND (READYZ<>{1})", xCDoc.xNPs.Current, (int)NSI.READINESS.FULL_READY);

                dvM = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "", DataViewRowState.CurrentRows);
                if (dvM.Count > 0)
                {
                    bRet = AppC.RC_CANCELB;
                    if (bNeedAsk)
                    {
                        DialogResult
                            dr = MessageBox.Show("Отменить маркировку (Enter)?\n(ESC) - продолжить!", "Заявка не выполнена!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dr != DialogResult.OK)
                            bRet = AppC.RC_OKB;
                    }
                }
            }
            return (bRet);
        }


        private void SetFilterOnSSCCC(bool bSetOnePoddon)
        {
            int
                nRet;
            string
                sFP = "",
                sRf,
                sSSCC = "";
            DataRow
                drZ;
            DataView
                dvM;

            dgDet.SuspendLayout();
            try
            {
                if (tcMain.SelectedIndex == PG_SCAN)
                {
                    // циклическое переключение на следующий режим (установка/сброс)
                    if (bSetOnePoddon)
                    {// выполняется установка
                        try
                        {
                            if (bShowTTN == false)
                            {// пока в заявке, но найдем в ТТН
                                drZ = drDet;
                                sRf = xCDoc.DefDetFilter() + String.Format("AND(KMC='{0}')", (string)drDet["KMC"]);
                                dvM = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "", DataViewRowState.CurrentRows);
                                if (dvM.Count > 0)
                                {
                                    ChgDetTable(dvM[0].Row, NSI.BD_DOUTD);
                                }
                                else
                                {
                                    Srv.ErrorMsg(String.Format("Код {} в ТТН не найден!", drZ["KRKMC"]), true);
                                }
                            }

                            sSSCC = (string)drDet["SSCC"];
                            if (sSSCC.Length > 0)
                            {
                                sFP = String.Format("AND(SSCC={0})", sSSCC);
                            }
                            else
                            {
                                sFP = "AND(LEN(SSCC)=0)";
                            }
                        }
                        catch { }
                        xNSI.DT[NSI.BD_DOUTD].sTFilt = sFP;
                        xSm.FilterTTN = NSI.FILTRDET.SSCC;
                        Srv.ErrorMsg(String.Format("Установлен для:\nSSCC='{0}'", sSSCC), "Фильтр по SSCC", false);
                    }
                    else
                    {// выполняется сброс
                        xNSI.DT[NSI.BD_DOUTD].sTFilt = "";
                        xSm.FilterTTN = NSI.FILTRDET.UNFILTERED;
                        Srv.ErrorMsg("Сброшен!", "Фильтр по SSCC", false);
                    }

                    //xNSI.DT[NSI.BD_DOUTD].dt.DefaultView.RowFilter = xCDoc.DefDetFilter() + xNSI.DT[NSI.BD_DOUTD].sTFilt;

                    sFP =
                    xCDoc.DefDetFilter() + xNSI.DT[NSI.BD_DOUTD].sTFilt;
                    xNSI.DT[NSI.BD_DOUTD].dt.DefaultView.RowFilter = sFP;

                    ShowStatDoc();
                    xNSI.SortName(bShowTTN, ref sFP);
                    lSortInf.Text = sFP;
                }
            }
            catch { }
            finally
            {
                dgDet.ResumeLayout();
            }

        }


        ///
        // обработка SSCC при вводе
        //public int ProceedSSCC(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        //{
        //    int
        //        ret = AppC.RC_OK;
        //    bool
        //        bTryServer = false;
        //    string
        //        sSSCC = xSc.Dat;

        //    DataRow
        //        dr = null;
        //    ServerExchange
        //        xSE = new ServerExchange(this);
        //    AppC.MOVTYPE
        //        MoveType = xCDoc.xDocP.DType.MoveType;

        //    if (tcMain.SelectedIndex == PG_SCAN)
        //    {
        //        switch (MoveType)
        //        {
        //            case AppC.MOVTYPE.AVAIL:        // инвентаризации
        //                bTryServer = true;
        //                break;
        //            case AppC.MOVTYPE.RASHOD:       // расходные документы
        //                bTryServer = true;
        //                break;
        //            case AppC.MOVTYPE.PRIHOD:       // документы поступления
        //                bTryServer = false;
        //                break;
        //            case AppC.MOVTYPE.MOVEMENT:     // документы перемещения
        //                bTryServer = false;
        //                break;
        //            default:
        //                bTryServer = false;
        //                break;
        //        }
        //    }
        //    else if (tcMain.SelectedIndex == PG_SSCC)
        //        bTryServer = false;

        //    if (bTryServer)
        //    {
        //            ret = ConvertSSCC2Lst(xSE, xSc, ref scD, xNSI.DT[NSI.BD_DOUTD].dt, false);
        //            if (ret == AppC.RC_OK)
        //            {
        //                if (xCLoad.dtZ.Rows.Count == 1)
        //                {// однородный поддон
        //                    scD.sSSCC = sSSCC;
        //                    dr = AddVirtProd(ref scD);
        //                }
        //            }
        //    }
        //    if (dr == null)
        //        dr = AddDetSSCC(xSc);
        //    if (dr != null)
        //        drDet = dr;
        //    IsOperReady();

        //    return (ret);
        //}

        /// добавление в список ТТН отмаркированного поддона
        //private DataRow AddDetSSCC(ScanVarGP xSc)
        //{
        //    DataRow
        //        ret = null;
        //    try
        //    {

        //        PSC_Types.ScDat sc = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
        //        sc.sSSCC = xSc.Dat;
        //        sc.nNomPodd = int.Parse(xSc.Dat.Substring(13, 7));
        //        //sc.sN = String.Format("Поддон №{0}", sc.nNomPodd);
        //        sc.sN = String.Format("SSCC № {0}...{1}", sc.sSSCC.Substring(2, 4), sc.sSSCC.Substring(15, 5));
        //        sc.nKrKMC = 0;
        //        ret = AddVirtProd(ref sc);
        //    }
        //    catch //(Exception e)
        //    {
        //        ret = null;
        //    }
        //    return (ret);
        //}




        /// добавление в список ТТН отмаркированного поддона
        //private DataRow AddListSSCC(string sSSCC, int nNomPoddon)
        //{
        //    DataRow
        //        ret = null;
        //    try
        //    {
        //        Add2SSCCTable(sSSCC, nNomPoddon, xCDoc, null, true);
        //    }
        //    catch { }
        //    return (ret);
        //}



        private int TryLoadSSCC(string sSSCC, int nRet)
        {
            int
                nM,
                nRec = 0,
                nNPP = 1;

            DataRow
                dr = null;
            DataTable
                dtD = xNSI.DT[NSI.BD_DIND].dt;
            DataSet
                ds;

            ServerExchange
                xSE = new ServerExchange(this);
            PSC_Types.ScDat
                scD = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));

            //if (nRet < 0)
            //{
            //    nRet = ConvertSSCC2Lst(xSE, sSSCC, ref scD, true, xNSI.DT[NSI.BD_DIND].dt);
            //}
            if ((nRet == AppC.RC_OK) || (nRet == AppC.RC_MANYEAN))
            {
                xCDoc.drCurRow["CHKSSCC"] = 1;

                // для контроля удаляем все предыдущие
                DataRow[] drMDet = xCDoc.drCurRow.GetChildRows(xNSI.dsM.Relations[NSI.REL2TTN]);
                foreach (DataRow drDel in drMDet)
                {
                    xNSI.dsM.Tables[NSI.BD_DOUTD].Rows.Remove(drDel);
                }
                DataRow[] drMDetZ = xCDoc.drCurRow.GetChildRows(xNSI.dsM.Relations[NSI.REL2ZVK]);
                foreach (DataRow drDel in drMDetZ)
                {
                    xNSI.dsM.Tables[NSI.BD_DIND].Rows.Remove(drDel);
                }

                ds = xCLoad.dsZ;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    nRec = xCLoad.dtZ.Rows.Count;
                    nM = 0;
                    nNPP = 1;
                    foreach (DataRow drA in xCLoad.dtZ.Rows)
                    {
                        scD = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
                        nM += SetOneDetZ(ref scD, dtD, drA, xCDoc.drCurRow, ref nNPP);
                        nNPP++;
                    }
                    scCur = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
                    SetDetFields(true);
                    Srv.ErrorMsg(String.Format("{0} строк загружено", nRec), String.Format("SSCC...{0}", sSSCC.Substring(15,5)), false);
                }
                catch (Exception exx)
                {
                    Srv.ErrorMsg(exx.Message, "Ошибка загрузки!", true);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            return (nNPP - 1);
        }


    }
}
