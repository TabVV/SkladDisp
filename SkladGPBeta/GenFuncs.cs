using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

using PDA.OS;
using PDA.Service;
using PDA.BarCode;
using SavuSocket;

using FRACT = System.Decimal;


namespace SkladGP
{
    public partial class MainF : Form
    {
        public CurLoad 
            xCLoad = null;                      // текущий объект загрузки документов
        public bool
            Doc4Chk = false;

        // объект текущей выгрузки
        public CurUpLoad xCUpLoad = null;

        private bool bInUpload = false;

        // текущий режим (все, по фильтру,...)
        private IntRegsAvail irFunc = null;



        /// контроль документов
        private bool ControlDocs(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            int nFunc = (int)nF;
            bool bKeyHandled = true;

            if (nFunc == AppC.F_INITREG)
            {
                // дальше клавиши обработаю сам
                ehCurrFunc += new Srv.CurrFuncKeyHandler(ControlDocs);
                if (irFunc == null)
                {
                    irFunc = new IntRegsAvail();
                    irFunc.SetAllAvail(false);
                    irFunc.SetAvail(AppC.UPL_CUR, true);
                    irFunc.SetAvail(AppC.UPL_ALL, true);
                }

                //pnLoadDoc.Left = dgDoc.Left + 5;
                //pnLoadDoc.Top = dgDoc.Top + 25;

                //tbPanP1.Text = irFunc.CurRegName;

                // заполнение полей для загрузки
                //lFuncNamePan.Text = "Контроль документов";
                //lpnLoadInf.Text = "<Enter>-выполнить контроль";

                //pnLoadDoc.Visible = true;

                xFPan.ShowP("Контроль документов", irFunc.CurRegName);
                //tFiction.Focus();

            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_LEFT:
                        //tbPanP1.Text = irFunc.NextReg(false);
                        xFPan.UpdateReg(irFunc.NextReg(false));
                        break;
                    case W32.VK_RIGHT:
                        //tbPanP1.Text = irFunc.NextReg(false);
                        xFPan.UpdateReg(irFunc.NextReg(false));
                        break;
                    case W32.VK_ESC:
                    case W32.VK_ENTER:
                        if (e.KeyValue == W32.VK_ENTER)
                        {
                            if (xCDoc.drCurRow != null)
                            {
                                Cursor crsOld = Cursor.Current;
                                Cursor.Current = Cursors.WaitCursor;

                                xInf = new List<string>();
                                try
                                {
                                    if (irFunc.CurReg == AppC.UPL_ALL)
                                        ControlAllDoc(xInf);
                                    else
                                        ControlDocZVK(xCDoc.drCurRow, xInf);
                                }
                                finally
                                {
                                    Cursor.Current = crsOld;
                                }
                                xHelpS.ShowInfo(xInf, ref kh);
                            }
                        }
                        //pnLoadDoc.Visible = false;
                        //pnLoadDoc.Left = 350;
                        xFPan.HideP();
                        bInUpload = false;
                        // дальше клавиши не обрабатываю
                        ehCurrFunc -= ControlDocs;
                        dgDoc.Focus();
                        break;
                }
            }
            return (bKeyHandled);
        }

        // выгрузка документов
        // nRegUpl - что выгружать (текущий, все, фильтр)
        private bool UploadDocs2Server(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            int nFunc = (int)nF;
            int nY = 28;                // панель для PG_DOC
            bool bKeyHandled = false;

            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case AppC.F_INITREG:
                        if (bInUpload == false)
                        {
                            if ((tcMain.SelectedIndex == PG_DOC) 
                                || (tcMain.SelectedIndex == PG_SCAN)
                                || (tcMain.SelectedIndex == PG_SSCC))
                            {
                                bInUpload = true;

                                // дальше клавиши обработаю сам
                                ehCurrFunc += new Srv.CurrFuncKeyHandler(UploadDocs2Server);

                                //if (xCUpLoad == null)
                                //{
                                //    xCUpLoad = new CurUpLoad(xPars);
                                //    if (xCLoad != null)
                                //    {// возьмем фильтр оттуда
                                //        xCUpLoad.xLP = xCLoad.xLP;
                                //    }
                                //    xDP = xCUpLoad.xLP;
                                //}

                                xCUpLoad = new CurUpLoad(xPars);
                                if (xCLoad != null)
                                {// возьмем фильтр оттуда
                                    xCUpLoad.xLP = xCLoad.xLP;
                                }
                                xDP = xCUpLoad.xLP;


                                xFPan = new FuncPanel(this, this.pnLoadDocG);

                                if ((tcMain.SelectedIndex == PG_SCAN)
                                    || (tcMain.SelectedIndex == PG_SSCC))
                                {// только текущий
                                    nY = 38;
                                    xCUpLoad.ilUpLoad.CurReg = AppC.UPL_CUR;
                                    xCUpLoad.ilUpLoad.SetAllAvail(false);
                                }
                                else
                                {
                                    xCUpLoad.ilUpLoad.SetAllAvail(true);
                                    //xCUpLoad.ilUpLoad.SetAvail(AppC.UPL_ALL, false);
                                }


                                //xBCScanner.WiFi.IsEnabled = true;
                                xBCScanner.WiFi.ShowWiFi(pnLoadDocG, true);
                                xFPan.ShowP(6, nY, "Выгрузка документов", xCUpLoad.ilUpLoad.CurRegName);
                                xFPan.UpdateSrv(xCUpLoad.CurSrv);

                                //tFiction.Focus();
                            }
                        }
                        break;
                }
            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_LEFT:
                        xFPan.UpdateReg(xCUpLoad.ilUpLoad.NextReg(false));
                        bKeyHandled = true;
                        break;
                    case W32.VK_RIGHT:
                        xFPan.UpdateReg(xCUpLoad.ilUpLoad.NextReg(true));
                        bKeyHandled = true;
                        break;
                    case W32.VK_ESC:
                        EndOfUpLoad(AppC.RC_CANCEL);
                        bKeyHandled = true;
                        break;
                    case W32.VK_DOWN:
                    case W32.VK_UP:
                        xCUpLoad.NextSrv();
                        xFPan.UpdateSrv(xCUpLoad.CurSrv);
                        bKeyHandled = true;
                        break;
                    case W32.VK_ENTER:
                        if (xCUpLoad.ilUpLoad.CurReg == AppC.UPL_FLT)
                            EditPars(AppC.F_UPLD_DOC, xCUpLoad.xLP, CTRL1ST.START_EMPTY, VerifyBeforeUpLoad, EditOverBeforeUpLoad);
                        else
                        {
                            if (xCUpLoad.ilUpLoad.CurReg == AppC.UPL_CUR)
                                xCUpLoad.xLP = xCDoc.xDocP;
                            EditOverBeforeUpLoad(AppC.RC_OK, AppC.F_UPLD_DOC);
                        }
                        bKeyHandled = true;
                        break;
                }
            }

            return (bKeyHandled);
        }

        // обработка окончания ввода параметров для выгрузки
        private AppC.VerRet VerifyBeforeUpLoad()
        {
            object 
                xErr = null;
            bool 
                bRet = false;
            AppC.VerRet 
                v;

            v.nRet = AppC.RC_OK;

            if (xCDoc.xDocP.nTypD != AppC.TYPD_OPR)
            {
                bRet = VerifyPars(xCUpLoad.xLP, AppC.F_UPLD_DOC, ref xErr);
            }
            else
            {
                if (bShowTTN)
                    //bRet = (IsOperReady(drDet) == AppC.RC_OK);
                    bRet = (IsOperReady() == AppC.RC_OK);
                else
                    Srv.ErrorMsg("Выгрузка из ТТН!", true);
            }

            if (bRet != true)
                v.nRet = AppC.RC_CANCEL;
            //else
            //    bQuitEdPars = true;
            v.cWhereFocus = (Control)xErr;
            return (v);
        }

        // автосохранение перед обменом данными
        private void AutoSaveDat()
        {
            if (xPars.bAutoSave == true)
            {
                xFPan.UpdateReg("Автосохранение...");

                Cursor.Current = Cursors.WaitCursor;
                // сохранение рабочих данных (если есть)
                    xSm.SaveCS(xPars.sDataPath, xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Count);
                    xNSI.DSSave(xPars.sDataPath);
                Cursor.Current = Cursors.Default;
            }
        }

        // обработка окончания ввода параметров для выгрузки
        private void EditOverBeforeUpLoad(int nRetEdit, int nUICall)
        {
            int nRet = AppC.RC_OK;
            ServerExchange xSE = new ServerExchange(this);

            if (nRetEdit == AppC.RC_OK)
            {// закончили по Enter, начало выгрузки
                AutoSaveDat();
                xFPan.UpdateHelp("Идет выгрузка данных...");

                string sL = UpLoadDoc(xSE, ref nRet);

                if ((xSE.ServerRet != AppC.EMPTY_INT) && (xSE.ServerRet != AppC.RC_OK))
                {// операция выгрузки не прошла на сервере (содержательная ошибка)

                    Srv.ErrorMsg(sL, true);
                    if (nUICall == 0)
                    {
                        xCDoc.xOper.SetOperDst(null, xCDoc.xDocP.nTypD, true);
                    }
                }

                if ((nUICall != 0) || (nRet != AppC.RC_OK))
                {
                    Srv.ErrorMsg(sL, String.Format("Код завершения-{0}", nRet), false);
                    EndOfUpLoad(AppC.RC_OK);
                }
                CheckNSIState(false);
            }
        }


        // завершение выгрузки
        private void EndOfUpLoad(int nRet)
        {
            xFPan.HideP();
            bInUpload = false;
            // дальше клавиши не обрабатываю
            ehCurrFunc -= UploadDocs2Server;

            if (nRet == AppC.RC_OK)
            {// что-то выгрузилось
                StatAllDoc();
            }
            xFPan = new FuncPanel(this, this.pnLoadDocG);
            xCDoc.xOper = new CurOper(false);
            Back2Main();
        }




        private bool bInLoad = false;
        // загрузка документов
        private bool LoadDocFromServer(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            int nFunc = (int)nF;
            int nY = 28;                // панель для PG_DOC
            bool bKeyHandled = false;

            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case AppC.F_INITRUN:
                    case AppC.F_INITREG:
                        if (bInLoad == false)
                        {
                            if ((tcMain.SelectedIndex == PG_DOC) || (tcMain.SelectedIndex == PG_SCAN))
                            {
                                bInLoad = true;

                                // дальше клавиши обработаю сам
                                ehCurrFunc += new Srv.CurrFuncKeyHandler(LoadDocFromServer);
                                //kh += new Srv.CurrFuncKeyHandler(LoadDocFromServer);

                                if (xCLoad == null)
                                {
                                    xCLoad = new CurLoad(AppC.UPL_FLT, Doc4Chk);
                                    xDP = xCLoad.xLP;
                                }

                                if (tcMain.SelectedIndex == PG_SCAN)
                                {// на вкладке Ввод
                                    //pnLoadDoc.Left = dgDet.Left + 5;
                                    //pnLoadDoc.Top = dgDet.Top + 26;
                                    nY = 38;
                                    xCLoad.ilLoad.CurReg = AppC.UPL_CUR;
                                    xCLoad.ilLoad.SetAllAvail(false);
                                    //tbPanP1.Enabled = false;
                                }
                                else
                                {// на вкладке Документы
                                    //pnLoadDoc.Left = dgDoc.Left + 5;
                                    //pnLoadDoc.Top = dgDoc.Top + 25;
                                    if (xCDoc.drCurRow == null)
                                    {// документов еще нет
                                        xCLoad.ilLoad.CurReg = AppC.UPL_FLT;
                                        xCLoad.ilLoad.SetAllAvail(false);
                                        xCLoad.ilLoad.SetAvail(AppC.UPL_FLT, true);
                                    }
                                    else
                                    {
                                        xCLoad.ilLoad.SetAllAvail(true);
                                        xCLoad.ilLoad.SetAvail(AppC.UPL_ALL, false);
                                        //tbPanP1.Enabled = true;
                                    }

                                }
                                    //tbPanP1.Text = xCLoad.ilLoad.CurRegName;

                                    // заполнение полей для загрузки
                                    //lFuncNamePan.Text = "Загрузка документов";
                                    //lpnLoadInf.Text = "<Enter>-начать загрузку";

                                //xBCScanner.WiFi.IsEnabled = true;
                                xBCScanner.WiFi.ShowWiFi(pnLoadDocG, true);

                                xFPan.ShowP(6, nY, "Загрузка документов", xCLoad.ilLoad.CurRegName);
                                if (nFunc != AppC.F_INITRUN)
                                    // для ручного ввода участок обнуляется
                                    xCLoad.xLP.nUch = AppC.EMPTY_INT;
                                if (nFunc == AppC.F_INITREG)
                                {
                                    if (xCDoc.drCurRow == null)
                                    {// документов еще нет
                                        EditPars(AppC.F_LOAD_DOC, xCLoad.xLP, CTRL1ST.START_EMPTY, VerifyBeforeLoad, EditOverBeforeLoad);
                                    }

                                }
                                else
                                {
                                    if (xCLoad.ilLoad.CurReg == AppC.UPL_FLT)
                                        EditPars(AppC.F_LOAD_DOC, xCLoad.xLP, CTRL1ST.START_LAST, VerifyBeforeLoad, EditOverBeforeLoad);
                                    else
                                        EditOverBeforeLoad(AppC.RC_OK, AppC.F_LOAD_DOC);
                                }


                                }
                        }
                        break;
                    case AppC.F_OVERREG:
                        break;
                }
            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_LEFT:
                        //tbPanP1.Text = xCLoad.ilLoad.NextReg(false);
                        xFPan.UpdateReg(xCLoad.ilLoad.NextReg(false));
                        bKeyHandled = true;
                        break;
                    case W32.VK_RIGHT:
                        //tbPanP1.Text = xCLoad.ilLoad.NextReg(true);
                        xFPan.UpdateReg(xCLoad.ilLoad.NextReg(true));
                        bKeyHandled = true;
                        break;
                    case W32.VK_ESC:
                        EndOfLoad(AppC.RC_CANCEL);
                        bKeyHandled = true;
                        break;
                    case W32.VK_ENTER:
                        if (xCLoad.ilLoad.CurReg == AppC.UPL_FLT)
                            EditPars(AppC.F_LOAD_DOC, xCLoad.xLP, CTRL1ST.START_EMPTY, VerifyBeforeLoad, EditOverBeforeLoad);
                        else
                            EditOverBeforeLoad(AppC.RC_OK, AppC.F_LOAD_DOC);
                        bKeyHandled = true;
                        break;
                }
            }

            return (bKeyHandled);
        }

        // позиционирование на указанную строку
        private bool SetCurRow(DataGrid dg, string sF, int nSys)
        {
            bool bRet = AppC.RC_CANCELB;
            CurrencyManager cmDoc = (CurrencyManager)BindingContext[dg.DataSource];
            for (int i = cmDoc.Count - 1; i >= 0; i--)
            {
                if ((int)(((DataRowView)cmDoc.List[i]).Row[sF]) == nSys)
                {
                    cmDoc.Position = i;
                    bRet = AppC.RC_OKB;
                    break;
                }
            }
            return (bRet);
        }

        // завершение загрузки
        private void PosOnLoaded(int nRet)
        {
            if (nRet == AppC.RC_OK)
            {// есть что показывать после загрузки
                if ((tcMain.SelectedIndex != PG_DOC) && (tcMain.SelectedIndex != PG_SCAN))
                    tcMain.SelectedIndex = PG_DOC;
                if ((xCLoad.dr1st != null) && (xCLoad.dr1st.Table == xNSI.DT[NSI.BD_DOCOUT].dt))
                {
                    SetCurRow(dgDoc, "SYSN", (int)xCLoad.dr1st["SYSN"]);
                }
                RestShowDoc(false);
                StatAllDoc();
            }
            CheckNSIState(false);
        }


        // завершение загрузки
        private void EndOfLoad(int nRet)
        {
            xFPan.HideP();
            bInLoad = false;

            // дальше клавиши не обрабатываю
            ehCurrFunc -= LoadDocFromServer;

            PosOnLoaded(nRet);
            Back2Main();
        }


        /// bUnCond - безусловная загрузка справочников
        public void CheckNSIState(bool bUnCond)
        {
            bool
                bNeedLoad;
            DataRow
                drTI;
            DateTime
                dtCur = DateTime.Now;
            List<string>
                lTNames = new List<string>();
            try
            {
                if (bUnCond)
                    xNSI.DT[NSI.BD_TINF].dt.Rows.Clear();

                foreach (KeyValuePair<string, NSI.TableDef> td in xNSI.DT)
                {
                    bNeedLoad = false;
                    if (((td.Value.nType & NSI.TBLTYPE.NSI) == NSI.TBLTYPE.NSI) &&
                        ((td.Value.nType & NSI.TBLTYPE.LOAD) == NSI.TBLTYPE.LOAD))   // НСИ загружаемое
                    {
                        bNeedLoad = true;
                        try
                        {
                            if ((drTI = xNSI.DT[NSI.BD_TINF].dt.Rows.Find(td.Key)) is DataRow)
                            {
                                if (drTI["FLAG_LOAD"].ToString() == NSI.NSI_NOT_LOAD)
                                    bNeedLoad = false;
                                else
                                {
                                    if ( (((DateTime)drTI["LASTLOAD"]).Date >= dtCur.Date && !bUnCond) )
                                    {
                                            bNeedLoad = false;
                                    }
                                    else if (dtCur.TimeOfDay < TimeSpan.Parse("06:00:00"))
                                            bNeedLoad = false;
                                }
                            }
                        }
                        catch { }
                    }
                    if (bNeedLoad)
                        lTNames.Add(td.Key);
                }
                if (lTNames.Count > 0)
                    LoadNsiMenu(!bUnCond, lTNames.ToArray());
            }
            catch { }

            if (tcMain.SelectedIndex == PG_SCAN)
                dgDet.Focus();
            else if (tcMain.SelectedIndex == PG_DOC)
                dgDoc.Focus();
        }


        // обработка окончания ввода параметров
        private AppC.VerRet VerifyBeforeLoad()
        {
            AppC.VerRet v;
            v.nRet = AppC.RC_OK;
            object xErr = null;
            bool bRet = VerifyPars(xCLoad.xLP, AppC.F_LOAD_DOC, ref xErr);
            if (bRet != true)
                v.nRet = AppC.RC_CANCEL;
            //else
            //    bQuitEdPars = true;
            v.cWhereFocus = (Control)xErr;
            return (v);
        }

        // обработка окончания ввода параметров
        private void EditOverBeforeLoad(int nRetEdit, int nF)
        {
            int 
                nRet = AppC.RC_OK;
            ServerExchange 
                xSE = new ServerExchange(this);

            if (nRetEdit == AppC.RC_OK)
            {// закончили по Enter, начало загрузки
                AutoSaveDat();

                if ((xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL) && (xCLoad.ilLoad.CurReg == AppC.UPL_CUR))
                {// повторная загрузка комплектации
                        xCLoad.sSSCC = "";
                        xCLoad.drPars4Load = xNSI.DT[NSI.BD_KMPL].dt.NewRow();
                        xCLoad.drPars4Load["TD"] = xCDoc.xDocP.nTypD;
                        xCLoad.drPars4Load["KRKPP"] = xCDoc.xDocP.nPol;
                        xCLoad.drPars4Load["KSMEN"] = xCDoc.xDocP.sSmena;
                        xCLoad.drPars4Load["DT"] = xCDoc.xDocP.dDatDoc.ToString("yyyyMMdd");
                        xCLoad.drPars4Load["KSK"] = xCDoc.xDocP.nSklad;
                        xCLoad.drPars4Load["NUCH"] = xCDoc.sLstUchNoms;
                        xCLoad.drPars4Load["KEKS"] = xCDoc.xDocP.nEks;
                        xCLoad.drPars4Load["NOMD"] = xCDoc.xDocP.sNomDoc;
                        xCLoad.drPars4Load["SYSN"] = (int)(xCDoc.xDocP.lSysN);
                        xCLoad.drPars4Load["TYPOP"] = xCDoc.xDocP.TypOper;
                        LoadKomplLst(xCLoad.drPars4Load, AppC.F_LOADKPL);
                }
                else
                {
                    LoadFromSrv dgL = new LoadFromSrv(DocFromSrv);
                    xCLoad.nCommand = AppC.F_LOAD_DOC;
                    xCLoad.sComLoad = AppC.COM_ZZVK;
                    string sL = xSE.ExchgSrv(AppC.COM_ZZVK, "", "", dgL, null, ref nRet);

                    MessageBox.Show("Загрузка окончена - " + sL, "Код - " + nRet.ToString());
                }
                EndOfLoad(AppC.RC_OK);
            }
        }


        private void DocFromSrv(SocketStream stmX, Dictionary<string, string> aC,
            DataSet ds, ref string sErr, int nRetSrv)
        {
            bool bMyRead = false;
            sErr = "Ошибка чтения XML";
            string sXMLFile = "";
            //int nFileSize = ServClass.ReadXMLWrite2File(stmX.SStream, ref sXMLFile);

            if (stmX.ASReadS.OutFile.Length == 0)
            {
                bMyRead = true;
                stmX.ASReadS.TermDat = AppC.baTermMsg;
                if (stmX.ASReadS.BeginARead(true, 1000 * 60) != SocketStream.ASRWERROR.RET_FULLMSG)
                    throw new System.Net.Sockets.SocketException(10061);
            }
            xCLoad.CheckIt = Doc4Chk;
            xCLoad.sFileFromSrv =
            sXMLFile = stmX.ASReadS.OutFile;

            xCLoad.dsZ = xNSI.MakeDataSetForLoad(xNSI.DT[NSI.BD_DOCOUT].dt, xNSI.DT[NSI.BD_DIND].dt, xNSI.DT[NSI.BD_SSCC].dt, "dsZ");

            sErr = "Ошибка загрузки XML";
            xCLoad.dsZ.BeginInit();
            xCLoad.dsZ.EnforceConstraints = false;
            System.Xml.XmlReader xmlRd = System.Xml.XmlReader.Create(sXMLFile);
            xCLoad.dsZ.ReadXml(xmlRd);
            xmlRd.Close();
            if (bMyRead)
                System.IO.File.Delete(sXMLFile);
            xCLoad.dsZ.EndInit();
            xCLoad.dr1st = null;
            nRetSrv = AddZ(xCLoad, ref sErr);
            if (nRetSrv == AppC.RC_OK)
            {
                sErr = "OK";
                Doc4Chk = false;
            }
            else
                throw new Exception(sErr);
        }






        // запрос на список комплектаций/заявку на комплектацию
        private bool LoadKomplLst(DataRow drPars, int nFunc)
        {
            bool bRet = AppC.RC_OKB;
            int nRet = AppC.RC_OK;
            string nCom = AppC.COM_ZKMPLST;
            LoadFromSrv dgL = null;
            ServerExchange xSE = new ServerExchange(this);

            xDP = xCLoad.xLP;
            xCLoad.nCommand = nFunc;

            if (drPars == null)
            {// загрузка списка комплектаций
                if (nFunc == AppC.F_LOADKPL)
                {
                    dgL = new LoadFromSrv(LstKomplFromSrv);
                }
                else
                {
                    dgL = new LoadFromSrv(LstKomplFromSrv);
                }
            }
            else
            {// загрузка заявки на комплектацию
                nCom = AppC.COM_ZKMPD;
                if (nFunc == AppC.F_LOADKPL)
                {
                    if (xCLoad.sSSCC == "")
                    {//
                        xDP.dDatDoc = DateTime.ParseExact((string)drPars["DT"], "yyyyMMdd", null);
                        xDP.nEks = (int)drPars["KEKS"];
                        xDP.lSysN = (long)drPars["SYSN"];
                    }
                    dgL = new LoadFromSrv(DocFromSrv);
                }
                else
                {
                    if (xCLoad.sSSCC == "")
                    {
                        xDP.dDatDoc = DateTime.ParseExact((string)drPars["DT"], "yyyyMMdd", null);
                        xDP.nEks = (int)drPars["KEKS"];
                        xDP.lSysN = (long)drPars["SYSN"];
                    }
                    dgL = new LoadFromSrv(DocFromSrv);
                }
            }

            string sL = xSE.ExchgSrv(nCom, "", "", dgL, null, ref nRet);
            if (nRet == AppC.RC_OK)
            {
                bRet = AppC.RC_OKB;
                if (drPars != null)
                {
                    if ((xCLoad.dr1st != null) && (xCLoad.dr1st.Table.TableName == NSI.BD_DOCOUT))
                    {
                        xCLoad.dr1st["TYPOP"] = (xCLoad.nCommand == AppC.F_LOADKPL) ? AppC.TYPOP_KMPL : AppC.TYPOP_OTGR;
                        xCLoad.dr1st["LSTUCH"] = xSm.LstUchKompl;
                        xCLoad.dr1st["DIFF"] = (int)(xCLoad.xLP.lSysN);
                        if (xCLoad.ilLoad.CurReg == AppC.UPL_FLT)
                            PosOnLoaded(nRet);
                    }
                }
            }
            else
            {
                bRet = AppC.RC_CANCELB;
                Srv.ErrorMsg(sL);
            }
            return (bRet);
        }





        /// запрос на один заказ для комплектации
        private bool LoadOneZkz()
        {
            bool 
                bRet = AppC.RC_OKB;
            int 
                nRet = AppC.RC_OK;
            string
                sRf,
                nCom = AppC.COM_ZKMPD;
            DataView
                dvM;
            LoadFromSrv
                dgL = new LoadFromSrv(DocFromSrv);
            ServerExchange 
                xSE = new ServerExchange(this);

            xCLoad = new CurLoad();
            //xDP = xCLoad.xLP;
            xCLoad.nCommand = AppC.F_ZZKZ1;
            xCLoad.ilLoad.CurReg = AppC.UPL_FLT;


            if (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
            {
                //sRf = xCDoc.DefDetFilter() + String.Format("AND(READYZ<>{0})", (int)NSI.READINESS.FULL_READY);
                //dvM = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, "", DataViewRowState.CurrentRows);
                if (!IsZkzReady(false))
                {
                    DialogResult
                        dr = MessageBox.Show("Отменить загрузку (Enter)?\n(ESC) - загрузить еще!", "Заявка не выполнена!",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (dr == DialogResult.OK)
                        return (AppC.RC_CANCELB);

                }
            }

            xSE.FullCOM2Srv = String.Format("COM={0};KSK={1};MAC={2};KP={3};PAR=(KSK={1},DT={4},TYPOP=R);",
                AppC.COM_ZKMPD,
                xSm.nSklad,
                xSm.MACAdr,
                xSm.sUser,
                Smena.DateDef.ToString("yyyyMMdd")
                );

            string sL = xSE.ExchgSrv(nCom, "", "", dgL, null, ref nRet);
            if (nRet == AppC.RC_OK)
            {
                bRet = AppC.RC_OKB;
                if ((xCLoad.dr1st != null) && (xCLoad.dr1st.Table.TableName == NSI.BD_DOCOUT))
                {
                    xCLoad.dr1st["TYPOP"] = AppC.TYPOP_KMPL;
                    if (xCLoad.ilLoad.CurReg == AppC.UPL_FLT)
                        PosOnLoaded(nRet);
                }
            }
            else
            {
                bRet = AppC.RC_CANCELB;
                Srv.ErrorMsg(sL);
                if (MainF.swProt != null)
                {
                    swProt.WriteLine(String.Format("Заказ не загружен! Параметры:{0}", xSE.FullCOM2Srv));
                    if ((xCLoad.nCommand == AppC.F_ZZKZ1) && (xCLoad.sFileFromSrv.Length > 0))
                        WriteAllToReg(true);
                }
            }
            return (bRet);
        }

        private void LstKomplFromSrv(SocketStream stmX, Dictionary<string, string> aC,
            DataSet ds, ref string sErr, int nRetSrv)
        {
            bool bMyRead = false;
            DataTable dt = xNSI.DT[NSI.BD_KMPL].dt;
            string sOldName = dt.TableName;

            sErr = "Ошибка чтения XML";
            string sXMLFile = "";

            try
            {
                if (stmX.ASReadS.OutFile.Length == 0)
                {
                    bMyRead = true;
                    stmX.ASReadS.TermDat = AppC.baTermMsg;
                    if (stmX.ASReadS.BeginARead(true, 1000 * 60) != SocketStream.ASRWERROR.RET_FULLMSG)
                        throw new System.Net.Sockets.SocketException(10061);
                }
                sXMLFile = stmX.ASReadS.OutFile;

                dt.TableName = NSI.BD_ZDOC;

                sErr = "Ошибка загрузки XML";

                dt.BeginInit();
                dt.BeginLoadData();
                dt.Clear();

                System.Xml.XmlReader xmlRd = System.Xml.XmlReader.Create(sXMLFile);
                dt.ReadXml(xmlRd);
                xmlRd.Close();
                if (bMyRead)
                    System.IO.File.Delete(sXMLFile);
                dt.EndLoadData();
                dt.EndInit();


                sErr = "OK";
            }
            finally
            {
                dt.TableName = sOldName;
            }
        }

        private bool IsUsedSSCC(string sSSCC)
        {
            bool bRet = AppC.RC_CANCELB;
            string sRf = xCDoc.DefDetFilter() + String.Format("AND(SSCC='{0}')", sSSCC);
            DataView dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, sRf, "", DataViewRowState.CurrentRows);
            if (dv.Count > 0)
                bRet = AppC.RC_OKB; 
            return (bRet);
        }

        private bool SetVirtScan(DataRow dr, ref PSC_Types.ScDat scD, bool bIsPallet, bool bEmkInf)
        {
            bool
                bRet = true;
            AddrInfo
                xA;

            try
            {

                scD.sKMC = (dr["KMC"] is string) ? (string)dr["KMC"] : "";
                scD.nKrKMC = (dr["KRKMC"] is int) ? (int)dr["KRKMC"] : 0;

                scD.tTyp = (bIsPallet) ? AppC.TYP_TARA.TARA_PODDON : AppC.TYP_TARA.TARA_TRANSP;

                scD.nRecSrc = (int)NSI.SRCDET.SSCCT;
                scD.sEAN = dr["EAN13"].ToString();
                scD.nParty = (string)dr["NP"];

                scD.nMest = (int)dr["KOLM"];
                scD.fEmk = (FRACT)dr["EMK"];

                scD.fVsego = scD.fVes = (FRACT)dr["KOLE"];

                scD.nKolG = (dr["KOLG"] is int) ? (int)dr["KOLG"] : 0;
                scD.nKolSht = (dr["KOLSH"] is int) ? (int)dr["KOLSH"] : 0;

                scD.sDataIzg = dr["DVR"].ToString();
                scD.dDataIzg = DateTime.ParseExact(scD.sDataIzg, "yyyyMMdd", null);
                scD.sDataIzg = scD.dDataIzg.ToString("dd.MM.yy");

                //bRet = xNSI.GetMCData("", ref scD, scD.nKrKMC);

                scD.sSSCCInt = (dr["SSCCINT"] is string) ? (string)dr["SSCCINT"] : "";

                //if (dr["SSCC"] is string)
                //    xCDoc.sSSCC = (string)dr["SSCC"];
                if (dr["SSCC"] is string)
                    scD.sSSCC = (string)dr["SSCC"];

                if ((dr["ADRFROM"] is string) && (((string)dr["ADRFROM"]).Length > 0))
                    scD.xOp.SetOperSrc(new AddrInfo((string)dr["ADRFROM"], xSm.nSklad), xCDoc.xDocP.nTypD, false);
                if ((dr["ADRTO"] is string) && (((string)dr["ADRTO"]).Length > 0))
                    scD.xOp.SetOperDst(new AddrInfo((string)dr["ADRTO"], xSm.nSklad), xCDoc.xDocP.nTypD, false);

                if (scD.sKMC.Length > 0)
                {
                    bRet = scD.GetFromNSI(scD.s,
                        xNSI.DT[NSI.NS_MC].dt.Rows.Find(new object[] { scD.sKMC }),
                        xNSI.DT[NSI.NS_MC].dt, bEmkInf);
                }
                else
                {
                    bRet = xNSI.GetMCDataOnEAN(scD.sEAN, ref scD, false);
                }

                if (bRet)
                {

                    scD.nMest = (int)dr["KOLM"];
                    if (bIsPallet)
                        scD.nMestPal = scD.nMest;

                    //scD.fEmk = (FRACT)dr["EMK"];

                    try { scD.nNomPodd = (int)dr["NPODD"]; }
                    catch { scD.nNomPodd = 0; }

                    try { scD.nNomMesta = (int)dr["NMESTA"]; }
                    catch { scD.nNomMesta = 0; }

                    scD.nNPredMT = (dr["SYSPRD"] is int) ? ((int)dr["SYSPRD"]) : 0;
                }
                scD.nTara = (dr["KTARA"] is string) ? ((string)dr["KTARA"]) : "";
                if (dr["SNM"] is string)
                    scD.sN = (string)dr["SNM"];
            }
            catch
            {
            }
            return (bRet);
        }

        // буферная таблица для преобразовпания SSCC->список продукции
        //private DataTable dtL = null;


        public int ConvertSSCC2Lst(ServerExchange xSE, string sSSCC, ref PSC_Types.ScDat scD, bool bInfoOnEmk)
        {
            return( ConvertSSCC2Lst(xSE, sSSCC, ref scD, bInfoOnEmk, xNSI.DT[NSI.BD_DOUTD].dt) );
        }

        public int ConvertSSCC2Lst(ServerExchange xSE, string sSSCC, ref PSC_Types.ScDat scD, bool bInfoOnEmk, DataTable dtResult)
        {
            int nRec,
                nRet = AppC.RC_OK;

            DataSet 
                dsTrans = null;

            // вместе с командой отдаем заголовок документа
            xCUpLoad = new CurUpLoad(xPars);
            xCUpLoad.sCurUplCommand = AppC.COM_ZSC2LST;
            if (xCDoc.drCurRow is DataRow)
                dsTrans = xNSI.MakeWorkDataSet(xNSI.DT[NSI.BD_DOCOUT].dt, xNSI.DT[NSI.BD_DOUTD].dt, new DataRow[] { xCDoc.drCurRow }, null, xSm, xCUpLoad);


            LoadFromSrv dgL = new LoadFromSrv(LstFromSSCC);

            xCLoad = new CurLoad();
            xCLoad.sComLoad = AppC.COM_ZSC2LST;
            xCLoad.sSSCC = sSSCC;
            xCLoad.xLP.lSysN = xCDoc.nId;
            xCLoad.dtZ = xNSI.MakeTempDOUTD(dtResult, NSI.BD_DOUTD);
            xCLoad.dsZ = xNSI.MakeDataSetForLoad(xNSI.DT[NSI.BD_DOCOUT].dt, xCLoad.dtZ, xNSI.DT[NSI.BD_SSCC].dt, "dsM");

            string sL = xSE.ExchgSrv(AppC.COM_ZSC2LST, "", "", dgL, dsTrans, ref nRet, 20);

            if (xCLoad.dtZ.Rows.Count > 0)
            {
                nRet = TestProdBySrv(xSE, nRet);

                if (nRet == AppC.RC_OK)
                {
                    nRec = xCLoad.dtZ.Rows.Count;
                    if (nRec == 1)
                    {// будем изображивать сканирование
                        SetVirtScan(xCLoad.dtZ.Rows[0], ref scD, true,
                            //((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC_PRT) > 0)?false:true,
                            bInfoOnEmk);
                        scD.sSSCC = sSSCC;
                    }
                    else
                    {// добавление группы ???
                        nRet = AppC.RC_MANYEAN;
                    }
                }
            }
            else
            {// просто сохраним запись ??? -  если была сетевая ошибка! при ошибке сервера ничего сохранять не надо!
                if (xSE.ServerRet != AppC.RC_OK)
                    Srv.ErrorMsg(sL);
            }

            return (nRet);
        }




        private void LstFromSSCC(SocketStream stmX, Dictionary<string, string> aC,
            DataSet ds, ref string sErr, int nRetSrv)
        {
            bool 
                bMyRead = false;
            string 
                sE,
                sXMLFile = "";
            System.Xml.XmlReader 
                xmlRd = null;

            xCLoad.dtZ = xCLoad.dsZ.Tables[1];
            sErr = "Ошибка чтения XML";

            if (stmX.ASReadS.OutFile.Length == 0)
            {
                bMyRead = true;
                stmX.ASReadS.TermDat = AppC.baTermMsg;
                if (stmX.ASReadS.BeginARead(true, 1000 * 60) != SocketStream.ASRWERROR.RET_FULLMSG)
                    throw new System.Net.Sockets.SocketException(10061);
            }
            sXMLFile = stmX.ASReadS.OutFile;

            sErr = "Ошибка загрузки XML";
            //dtL.BeginInit();
            //dtL.BeginLoadData();
            //dtL.Clear();

            xCLoad.dsZ.BeginInit();
            xCLoad.dsZ.EnforceConstraints = false;

            try
            {
                xmlRd = System.Xml.XmlReader.Create(sXMLFile);
                xCLoad.dsZ.ReadXml(xmlRd);
                xCLoad.dsZ.EnforceConstraints = true;
                xCLoad.dsZ.EndInit();
            }
            catch (Exception e) 
            {
                int i = xCLoad.dtZ.Rows.Count;
                sE = "";
                if (i-- > 0)
                {
                    sE = String.Format("\nПоследняя({0}) строка:\n{1} {2}", i + 1, xCLoad.dtZ.Rows[i]["KRKMC"], xCLoad.dtZ.Rows[i]["SNM"]);
                }
                Srv.ErrorMsg(e.Message + sE, "Ошибка в данных", true);
            }
            finally
            {
                if (xmlRd != null)
                    xmlRd.Close();
            }

            if (bMyRead)
                System.IO.File.Delete(sXMLFile);

            //dtL.EndLoadData();
            //dtL.EndInit();


            if (xCLoad.dtZ.Rows.Count < 1)
            {
                if (xCLoad.sComLoad == AppC.COM_ADR2CNT)
                {
                }
                else
                    throw new Exception("Нет данных");
            }
            try
            {
                sErr = aC["MSG"];
            }
            catch
            {
                sErr = "OK";
            }
        }


        private int ConvertAdr2Lst(AddrInfo xAdrSrc, string sTypeInf)
        {
            return( ConvertAdr2Lst(xAdrSrc, AppC.COM_CELLI, sTypeInf, true, NSI.SRCDET.FROMADR) );
        }

        private int ConvertAdr2Lst(AddrInfo xAdrSrc, string sCOM, string sTypeInf, bool bProceedResult, NSI.SRCDET srcAdd)
        {
            //return (ConvertAdr2Lst(xAdrSrc, AppC.COM_CELLI, sTypeInf, true, NSI.SRCDET.FROMADR, ref ehCurrFunc));
            return (ConvertAdr2Lst(xAdrSrc, sCOM, sTypeInf, bProceedResult, srcAdd, ref ehCurrFunc));
        }

        private int ConvertAdr2Lst(AddrInfo xAdrSrc, string sCOM, string sTypeInf, bool bProceedResult, NSI.SRCDET srcAdd, ref Srv.CurrFuncKeyHandler kbh)
        {
            int
                nRet = AppC.RC_OK;
            string
                sNom = "",
                sQ;

            DataRow
                dr;
            DataSet 
                dsTrans = null;
            PSC_Types.ScDat
                scD;

            LoadFromSrv 
                dgL = null;
            ServerExchange 
                xSE = new ServerExchange(this);

            //sAdr = xAdrSrc.Addr;
            
            // буфер для приема данных с сервера
            //MakeTempDOUTD(xNSI.DT[NSI.BD_DOUTD].dt);

            xCLoad = new CurLoad();
            xCLoad.xLP.lSysN = xCDoc.nId;
            //xCLoad.dtZ = dtL;
            xCLoad.dsZ = xNSI.MakeDataSetForLoad(xNSI.DT[NSI.BD_DOCOUT].dt, xNSI.DT[NSI.BD_DOUTD].dt, xNSI.DT[NSI.BD_SSCC].dt, "dsM");
            xCLoad.dtZ = xCLoad.dsZ.Tables[1];

            sQ = String.Format("(KSK={0},ADRCELL={1},TYPE={2}", xSm.nSklad, xAdrSrc.Addr, sTypeInf);
            dgL = new LoadFromSrv(LstFromSSCC);
            switch (sCOM)
            {
                case AppC.COM_CELLI:
                    if (sTypeInf == "TXT")
                        dgL = new LoadFromSrv(InfAbout);
                    break;
                case AppC.COM_A4MOVE:
                    sNom = (xSm.Curr4Invent > 0) ?
                        String.Format(",ND={0}", xSm.Curr4Invent) :
                        "";
                    sQ += String.Format(",TIMECR={0}{1}", xAdrSrc.ScanDT.ToString("s"), sNom);
                    break;
                case AppC.COM_ADR2CNT:
                    // ???
                    xCUpLoad = new CurUpLoad(xPars);
                    xCUpLoad.sCurUplCommand = AppC.COM_ADR2CNT;

                    dsTrans = xNSI.MakeWorkDataSet(xNSI.DT[NSI.BD_DOCOUT].dt,
                              xNSI.DT[NSI.BD_DOUTD].dt, new DataRow[] { xCDoc.drCurRow }, null, xSm, xCUpLoad);
                    break;
            }

            sQ += ")";

            xCLoad.sComLoad = sCOM;
            string sL = xSE.ExchgSrv(sCOM, sQ, "", dgL, dsTrans,  ref nRet, 120);

            if (sCOM == AppC.COM_ADR2CNT)
            {
                //if (xCLoad.dtZ.Rows.Count > 0)
                //    nRet = TestProdBySrv(xSE, nRet);

                if (nRet == AppC.RC_OK)
                {
                    sNom = "";
                    if (xSE.ServerRet != AppC.EMPTY_INT)
                    {
                        if (xSE.ServerAnswer.ContainsKey("ND"))
                            sNom = xSE.ServerAnswer["ND"];
                        else if (xSE.AnswerPars.ContainsKey("ND"))
                            sNom = xSE.AnswerPars["ND"];

                        try
                        {
                            xSm.Curr4Invent = int.Parse(sNom);
                        }
                        catch
                        {
                            xSm.Curr4Invent = 0;
                        }
                    }
                }
            }

            if (nRet == AppC.RC_OK)
            {

                if (!bProceedResult)
                    return (nRet);

                if (sTypeInf == "TXT")
                {// справочная информация, просто выводится
                    xHelpS.ShowInfo(xInf, ref kbh);
                }
                else
                {
                    if (sTypeInf == "MOV")
                    {
                        if ((xCLoad.dtZ.Rows.Count == 1) && (
                            (xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) ||
                            (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)))
                            nRet = TestProdBySrv(xSE, nRet);

                        if (nRet == AppC.RC_OK)
                        {
                            scD = new PSC_Types.ScDat(new ScannerAll.BarcodeScannerEventArgs(ScannerAll.BCId.NoData, ""));
                            SetVirtScan(xCLoad.dtZ.Rows[0], ref scD, true, false);
                            xCDoc.xOper.SSCC = scD.sSSCC;
                            scD.nRecSrc = (int)srcAdd;
                            AddDet1(ref scD, out dr);
                            // Если пришел адрес назначения
                            if ((scD.xOp.xAdrDst != null) && (scD.xOp.xAdrDst.Addr.Length > 0))
                            {
                                xCDoc.xOper.xAdrDst_Srv = scD.xOp.xAdrDst;          // сохранить рекомендации сервера
                                if (xDestInfo == null)
                                {
                                    int
                                        FontS = 28,
                                        INFWIN_WIDTH = 230,
                                        INFWIN_HEIGHT = 90;
                                    Rectangle
                                        recInf,
                                        screen = Screen.PrimaryScreen.Bounds;

                                    recInf = new Rectangle((screen.Width - INFWIN_WIDTH) / 2, 200, INFWIN_WIDTH, INFWIN_HEIGHT);
                                    xDestInfo = new Srv.HelpShow(this, recInf, 1, FontS, 0);
                                }
                                xDestInfo.ShowInfo(new string[] { scD.xOp.xAdrDst.AddrShow }, ref ehCurrFunc);
                            }
                        }
                    }
                    else
                    {// ROW - добавление группы
                        DialogResult dRez = MessageBox.Show(
                            String.Format("Новых строк {0}\nДобавить (Enter)?\n(ESC)- отменить", xCLoad.dtZ.Rows.Count),
                            "Добавление продукции", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dRez == DialogResult.OK)
                        {
                            nRet = AppC.RC_MANYEAN;
                            AddGroupDet(nRet, (int)srcAdd, "");
                        }
                    }
                }
            }
            else
            {
                Srv.ErrorMsg(sL);
            }

            return (nRet);
        }

        private void ClearAttentionInfo()
        {
            if (xDestInfo != null)
            {
                xDestInfo.StopShow(ref ehCurrFunc);
            }
        }








        public FuncPanel xPrnPan;
        // выбор принтера из списка доступных
        public int SetCurPrinter(bool bShowPanel)
        {
            int nRet = AppC.RC_OK;
            ServerExchange xSE = new ServerExchange(this);

            LoadFromSrv dgL = new LoadFromSrv(GetPrnList);

            xCLoad = new CurLoad();
            xCLoad.xLP.lSysN = xCDoc.nId;

            //xCLoad.dr1st = null;
            xCLoad.nCommand = AppC.F_SETPRN;
            xCLoad.sComLoad = AppC.COM_GETPRN;

            //Cursor crsOld = Cursor.Current;
            //Cursor.Current = Cursors.WaitCursor;

            string sL = xSE.ExchgSrv(AppC.COM_GETPRN, "", "", dgL, null, ref nRet);

            //Cursor.Current = crsOld;
            if (bShowPanel)
            {
                int nY = (tcMain.SelectedIndex == PG_SCAN) ? 38 : 28;
                if ((nRet == AppC.RC_OK) && (nCurPrnAny >= 0))
                {
                    //xBCScanner.WiFi.IsEnabled = false;
                    xBCScanner.WiFi.ShowWiFi(pnLoadDocG, true);

                    xPrnPan = new FuncPanel(this, null);
                    xPrnPan.IFaceReset(true);

                    xPrnPan.ShowP(6, nY, "Выбор принтера", aPrnAny[nCurPrnAny].ObjName);
                    xPrnPan.UpdateSrv("<- сменить ->");
                    xPrnPan.UpdateHelp("<Enter> - выбрать");
                    ehCurrFunc += new Srv.CurrFuncKeyHandler(PrinterChoiceKeyHandler);
                }
                else
                {
                    Srv.ErrorMsg(sL, true);
                }
            }
            return (xSE.ServerRet);
        }


        private Smena.ObjInf[] aPrnAny = null;
        private int 
            nCountPrnAny = 0,
            nCurPrnAny = -1;

        private void GetPrnList(SocketStream stmX, Dictionary<string, string> aC,
            DataSet ds, ref string sErr, int nRetSrv)
        {
            bool bMyRead = false;
            object x;
            sErr = "Ошибка чтения XML";
            string sXMLFile = "";

            if (stmX.ASReadS.OutFile.Length == 0)
            {
                bMyRead = true;
                stmX.ASReadS.TermDat = AppC.baTermMsg;
                if (stmX.ASReadS.BeginARead(true, 1000 * 60) != SocketStream.ASRWERROR.RET_FULLMSG)
                    throw new System.Net.Sockets.SocketException(10061);
            }
            sXMLFile = stmX.ASReadS.OutFile;

            sErr = "Ошибка загрузки XML";
            try
            {
                int nRet = Srv.ReadXMLObj(typeof(Smena.ObjInf[]), out x, sXMLFile);
                if (nRet == AppC.RC_OK)
                {
                    aPrnAny = ((Smena.ObjInf[])x);
                    nCountPrnAny = aPrnAny.Length;
                    if (nCountPrnAny > 0)
                    {
                        sErr = "OK";
                        nCurPrnAny = 0;
                    }
                    else
                    {
                        sErr = "Принтеров нет";
                        nCurPrnAny = -1;
                        throw new System.Net.Sockets.SocketException(10061);
                    }
                }
                else
                {
                    nCurPrnAny = -1;
                    throw new System.Net.Sockets.SocketException(10061);
                }
            }
            finally
            {
                if (bMyRead)
                    System.IO.File.Delete(sXMLFile);
            }
        }






        public void Back2Main()
        {
            if (((tcMain.SelectedIndex != PG_DOC) && (tcMain.SelectedIndex != PG_SCAN))
                || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL))
            {
                if (xScrDet.CurReg != 0)
                {// когда выйти из полноэкранного
                    Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                    xScrDet.NextReg(AppC.REG_SWITCH.SW_CLEAR, tNameSc);
                }
                tcMain.SelectedIndex = PG_DOC;
            }
            ((tcMain.SelectedIndex == PG_DOC) ? dgDoc : dgDet).Focus();
        }

        private bool PrinterChoiceKeyHandler(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            bool bKeyHandled = true;
            int
                nFunc = (int)nF;

            switch (e.KeyValue)
            {
                case W32.VK_LEFT:
                    nCurPrnAny = (nCurPrnAny == 0) ? nCountPrnAny - 1 : nCurPrnAny - 1;
                    xPrnPan.UpdateReg(aPrnAny[nCurPrnAny].ObjName);
                    break;
                case W32.VK_RIGHT:
                    nCurPrnAny = (nCurPrnAny == nCountPrnAny - 1) ? 0 : nCurPrnAny + 1;
                    xPrnPan.UpdateReg(aPrnAny[nCurPrnAny].ObjName);
                    break;
                case W32.VK_ESC:
                case W32.VK_ENTER:
                    xPrnPan.HideP();
                    xPrnPan.IFaceReset(false);
                    // дальше клавиши не обрабатываю
                    ehCurrFunc -= PrinterChoiceKeyHandler;
                    xPrnPan = null;
                    if (e.KeyValue == W32.VK_ENTER)
                        xSm.CurPrinterMOBName = aPrnAny[nCurPrnAny].ObjName;
                    Back2Main();
                    break;
                default:
                    bKeyHandled = false;
                    break;
            }
            return (bKeyHandled);
        }






        private void InfAbout(SocketStream stmX, Dictionary<string, string> aC,
            DataSet ds, ref string sErr, int nRetSrv)
        {
            bool bMyRead = false;
            List<string> lstI = new List<string>();
            sErr = "Ошибка чтения XML";
            string sXMLFile = "";
            System.IO.StreamReader sr;

            if (stmX.ASReadS.OutFile.Length == 0)
            {
                bMyRead = true;
                stmX.ASReadS.TermDat = AppC.baTermMsg;
                if (stmX.ASReadS.BeginARead(true, 1000 * 60) != SocketStream.ASRWERROR.RET_FULLMSG)
                    throw new System.Net.Sockets.SocketException(10061);
            }
            sXMLFile = stmX.ASReadS.OutFile;

            sErr = "Ошибка загрузки XML";

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (sr = new System.IO.StreamReader(sXMLFile))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        lstI.Add(line);
                    }
                    //string[] aI = new string[lstI.Count];
                    //lstI.CopyTo(aI);
                    //xInf = aI;
                    xInf = lstI;
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            finally 
            {
                if (bMyRead)
                    System.IO.File.Delete(sXMLFile);
            }

            sErr = "OK";
        }


        private void GetKMCInf(int nF)
        {
            int nRet = AppC.RC_CANCEL;
            ServerExchange xSE = new ServerExchange(this);

            string 
                sQ, 
                sCom = AppC.COM_KMCI, 
                sErr;
            if (xCDoc != null)
            {
                sQ = String.Format("(KSK={0}", xSm.nSklad);
                if (nF == AppC.F_KMCINF)
                {
                    //sCom = AppC.COM_KMCI;
                    if (xNSI.GetMCData("", ref scCur, scCur.nKrKMC, true))
                    {
                        nRet = AppC.RC_OK;
                        sQ = String.Format(sQ + ",KMC={0}", scCur.drMC["KMC"]);
                        if (scCur.nParty.Length > 0)
                            sQ = String.Format(sQ + ",NP={0}", scCur.nParty);
                        if (scCur.dDataIzg != DateTime.MinValue)
                            sQ = String.Format(sQ + ",DVR={0}", scCur.dDataIzg.ToString("yyyyMMdd"));
                    }
                }
                sQ += ")";

                if (nRet == AppC.RC_OK)
                {

                    //Cursor crsOld = Cursor.Current;
                    //Cursor.Current = Cursors.WaitCursor;

                    LoadFromSrv dgL = new LoadFromSrv(InfAbout);
                    sErr = xSE.ExchgSrv(sCom, sQ, "", dgL, null, ref nRet, 30);

                    //Cursor.Current = crsOld;
                    if (xSE.ServerRet == AppC.RC_OK)
                    {
                        //ShowInf(xInf);
                        xHelpS.ShowInfo(xInf, ref ehCurrFunc);
                    }
                    else
                        Srv.ErrorMsg(sErr, "Ошибка!", true);
                }
            }

        }



        //class InfoWin
        //{


        //    public InfoWin()
        //    {
        //    }



        //}





        // сброс полей текущей операции
        private void NewOper()
        {
            DataRow
                drObj = xCDoc.xOper.OperObj;
            CurrencyManager
                cmDoc = (CurrencyManager)BindingContext[dgDoc.DataSource];

            if (drObj != null)
            {
                int
                    nPrevPos = cmDoc.Position;

                xNSI.dsM.Tables[NSI.BD_DOUTD].Rows.Remove(drObj);
            }
            xCDoc.xOper = new CurOper(false);
            xCDoc.xOper.SetOperObj(null, xCDoc.xDocP.nTypD, true);
            ChangeDetRow(true);
            //ShowOperState(xCDoc.xOper);
        }



        /// строку символов - в список для отображения в окне Help
        private List<string> aKMCName(string sN, bool bRazd)
        {
            return (aKMCName(sN, bRazd, '-'));
        }

        /// строку символов - в список для отображения в окне Help
        private List<string> aKMCName(string sN, bool bRazd, char cM)
        {
            int
                l;
            string
                ss = sN;
            List<string>
                aS = new List<string>();

            while (ss.Length > 0)
            {
                l = (ss.Length >= 33) ? 33 : ss.Length;
                aS.Add(ss.Substring(0, l));
                ss = ss.Substring(l);
            }
            if (bRazd)
                aS.Add(" ".PadRight(32, cM));

            return (aS);
        }

        // показать статистику по документу
        private void ShowTotMest()
        {
            try
            {
                xInf = aKMCName(CurDocInf(xCDoc.xDocP), true);
                TotMest(NSI.REL2TTN, xInf);
                TotMest(NSI.REL2ZVK, xInf);
                xHelpS.ShowInfo(xInf, ref ehCurrFunc);
            }
            catch { }
        }

        // всего мест по списку продукции (заявка или ТТН)
        private int TotMest(string sRel, List<String> xI)
        {
            int
                nState,
                nTTNTrans = 0,
                nTTNReady = 0,
                nMTTN = 0;
            FRACT
                fTotKolE = 0,
                fTotVes = 0;
            DataRow[]
                chR = xCDoc.drCurRow.GetChildRows(sRel);
            try
            {
                foreach (DataRow dr in chR)
                {
                    nMTTN += (int)dr["KOLM"];
                    if (sRel == NSI.REL2TTN)
                    {
                        if ((int)dr["SRP"] > 0)
                        {
                            fTotVes += (FRACT)dr["KOLE"];
                        }
                        else
                            fTotKolE += (FRACT)dr["KOLE"];
                        nState = (int)dr["STATE"] & (int)AppC.OPR_STATE.OPR_READY;
                        if (nState > 0)
                            nTTNReady++;

                        nState = (int)dr["STATE"] & (int)AppC.OPR_STATE.OPR_TRANSFERED;
                        if (nState > 0)
                            nTTNTrans++;
                    }
                    else
                    {
                        fTotKolE += (FRACT)dr["KOLE"];
                        nState = (int)dr["READYZ"] & (int)NSI.READINESS.FULL_READY;
                        if (nState > 0)
                            nTTNReady++;
                    }

                }
            }
            catch { }
            if (xI != null)
            {
                xI.Add(String.Format("  Строк в {0} - {1}", (sRel == NSI.REL2TTN) ? "ТТН" : "Заявке", chR.Length));
                if (sRel == NSI.REL2TTN)
                {
                    xI.Add(String.Format("    из них выгружено - {0}", nTTNTrans));
                    xI.Add(String.Format("    из них готово к выгрузке - {0}", nTTNReady));
                }
                else
                    xI.Add(String.Format("    из них выполнено - {0}", nTTNReady));


                xI.Add(String.Format("    мест - {0}, вес - {1}", nMTTN, fTotVes));
                xI.Add(String.Format("    штук - {0}", fTotKolE));
            }
            return (nMTTN);
        }


        // показать статистику по продукции
        private void ShowTotMestProd()
        {
            try
            {
                if (drDet != null)
                {
                    xInf = aKMCName((string)drDet["SNM"], true);
                    TotMestProd(xNSI.DT[NSI.BD_DOUTD].dt, true, xInf);
                    TotMestProd(xNSI.DT[NSI.BD_DIND].dt, false, xInf);
                    xHelpS.ShowInfo(xInf, ref ehCurrFunc);
                }
            }
            catch { }
        }


        /// собрать статистику по продукции для одного списка
        private int TotMestProd(DataTable dtD, bool bIsTTN, List<String> xI)
        {
            int
                nState,
                nTTNTrans = 0,
                nTTNReady = 0,
                nMTTN = 0;
            FRACT
                fTotKolE = 0,
                fTotVes = 0;
            DataRow
                dr;
            DateTime
                dGodnCurr;
            AddrInfo
                xa;
            string
                sA;

            string sRf = String.Format("(SYSN={0})AND(KMC='{1}')", drDet["SYSN"], drDet["KMC"]);

            DataView
                dv = new DataView(dtD, sRf, "DVR", DataViewRowState.CurrentRows);

            try
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    dr = dv[i].Row;
                    nMTTN += (int)dr["KOLM"];
                    if (bIsTTN)
                    {
                        if ((int)dr["SRP"] > 0)
                        {
                            fTotVes += (FRACT)dr["KOLE"];
                        }
                        else
                            fTotKolE += (FRACT)dr["KOLE"];
                        nState = (int)dr["STATE"] & (int)AppC.OPR_STATE.OPR_READY;
                        if (nState > 0)
                            nTTNReady++;

                        nState = (int)dr["STATE"] & (int)AppC.OPR_STATE.OPR_TRANSFERED;
                        if (nState > 0)
                            nTTNTrans++;
                    }
                    else
                    {
                        fTotKolE += (FRACT)dr["KOLE"];
                        nState = (int)dr["READYZ"] & (int)NSI.READINESS.FULL_READY;
                        if (nState > 0)
                            nTTNReady++;
                    }
                    xa = new AddrInfo((string)dr["ADRFROM"], xSm.nSklad);
                    //try
                    //{
                    //    dGodnCurr = DateTime.ParseExact((string)dr["DTG"], "yyyyMMdd", null);
                    //}
                    //catch
                    //{
                    //    dGodnCurr = DateTime.MinValue;
                    //}
                    try
                    {
                        dGodnCurr = DateTime.ParseExact((string)dr["DVR"], "yyyyMMdd", null);
                    }
                    catch
                    {
                        dGodnCurr = DateTime.MinValue;
                    }

                    sA = (xa.AddrShow.Length > 0) ? xa.AddrShow : " ".PadRight(10);
                    xI.Add(String.Format("{0} {1} {2} {3}",
                        sA,
                        dGodnCurr.ToString("dd.MM.yy"),
                        dr["KOLM"].ToString().PadLeft(4),
                        dr["KOLE"].ToString().PadLeft(7)));
                }
            }
            catch { }
            if (xI != null)
            {
                xI.Add(String.Format("  Строк в {0} - {1}", (bIsTTN) ? "ТТН" : "Заявке", dv.Count));
                if (bIsTTN)
                {
                    xI.Add(String.Format("    из них выгружено - {0}", nTTNTrans));
                    xI.Add(String.Format("    из них готово к выгрузке - {0}", nTTNReady));
                }
                else
                    xI.Add(String.Format("    из них выполнено - {0}", nTTNReady));


                xI.Add(String.Format("    мест - {0}, вес - {1}", nMTTN, fTotVes));
                xI.Add(String.Format("    штук - {0}", fTotKolE));
            }
            return (nMTTN);
        }







    }
}
