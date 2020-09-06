using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using ScannerAll;
using PDA.OS;
using PDA.Service;
using SkladGP;

namespace SGPF_Avtor
{
    public partial class Avtor : Form
    {

        private struct OldVals
        {
            public int nSklad;
            public int nUch;
            public string sDatDoc;
            public int nReg;

        }

        static private MainF xMF = null;

        private BarcodeScanner.BarcodeScanEventHandler ehScan = null;

        static private NSI xNSI;
        static private Smena xSm;
        static private OldVals xOldSm;

        // режим вызова: начало или завершение
        public int nCurReg = AppC.AVT_LOGON;

        // флаг смены имени пользователя User
        private bool bUserChanged = false;

        // флаг обработки сканирования
        private bool bScanProceed = false;

        private AppC.EditListC aEd;

        // не обрабатывать введенный символ
        private bool bSkipKey = false;

        // поток для загрузки НСИ
        private Thread
            xTh = null;

        public Avtor()
        {
            InitializeComponent();
        }


        // прочитать оставшиеся справочники
        static private void ReadLocNSI()
        {
            xNSI.LoadLocNSI(new string[] { }, SkladAll.NSIAll.LOAD_EMPTY);
            xMF.evReadNSI.Set();
        }

        private bool AfterConstruct(MainF xF)
        {
            bool
                cbAvail = true,
                ret = false;
            int
                i;

            xNSI = xF.xNSI;
            xSm = xF.xSm;
            nCurReg = (int)xF.xDLLPars;

            // центровка формы
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            this.Location = new Point((screen.Width - this.Width) / 2,
                (screen.Height - this.Height) / 2);

            SetBindSmenaPars();

            i = WhatNextMode(ref cbAvail, 1);
            if (i < 0)
            {
                Srv.ErrorMsg("Запуск программы запрещен!");
            }
            else
            {
                cbReg.SelectedValue = xSm.RegApp = i;
                SetAvtFields();

                if (xMF.evReadNSI != null)
                {// загрузку НСИ выполнять параллельно авторизации
                    xTh = new Thread(new ThreadStart(ReadLocNSI));
                    xTh.Priority = ThreadPriority.Lowest;
                    xTh.Start();
                }
                if (nCurReg != AppC.AVT_PARS)
                {// парметры изменяются только с клавиатуры
                    ehScan = new BarcodeScanner.BarcodeScanEventHandler(OnScanBage);
                    xMF.xBCScanner.BarcodeScan += ehScan;
                }
                else
                {
                    xOldSm.nSklad = xSm.nSklad;
                    xOldSm.nUch = xSm.nUch;
                    xOldSm.sDatDoc = xSm.DocData;
                    xOldSm.nReg = xSm.RegApp;
                }

                //if ( i < 0 )
                //{
                //    Srv.ErrorMsg("Нет разрешенных режимов!");
                //    this.DialogResult = DialogResult.Cancel;
                //}
                //else
                //    BeginEditAvt(nCurReg, i);

                BeginEditAvt(nCurReg, cbAvail);
                ret = true;
            }
            return (ret);
        }

        private void SetBindSmenaPars()
        {
            Binding bi;

            bi = new Binding("Text", xSm, "nSklad");
            tDefKSKL.DataBindings.Add(bi);
            bi = new Binding("Text", xSm, "nUch");
            tDefUch.DataBindings.Add(bi);
            tDefUch.DataBindings[0].DataSourceNullValue = 0;
            bi = new Binding("Text", xSm, "DocData");
            tDefDateDoc.DataBindings.Add(bi);

            cbReg.DataSource = Smena.bl;
            cbReg.DisplayMember = "SName";
            cbReg.ValueMember = "INumber";
            cbReg.SelectedValue = xSm.RegApp;
            cbReg.SelectedValueChanged += new EventHandler(cbReg_SelectedValueChanged);
        }

        private void Avtor_Closing(object sender, CancelEventArgs e)
        {
            if (ehScan != null)
                xMF.xBCScanner.BarcodeScan -= ehScan;
            if (this.DialogResult == DialogResult.Cancel)
            {
                if (xTh != null)
                {// загрузка НСИ могла выполняться
                    xTh.Abort();
                }
            }
        }

        private void OnScanBage(object sender, BarcodeScannerEventArgs e)
        {
            int nTab = 0;
            string sP;

            bScanProceed = true;
            if ((e.nID == BCId.Code128) && (e.Data.Length == 10))
            {
                if (e.Data.Substring(0, 3) == "777")
                {
                    try
                    {
                        nTab = int.Parse(e.Data.Substring(3));
                        if (nTab > 0)
                        {
                            if (nCurReg != AppC.AVT_TOUT)
                            {



                                DataView dv = new DataView(xNSI.DT[NSI.NS_USER].dt,
                                                String.Format("TABN='{0}'", nTab), "", DataViewRowState.CurrentRows);
                                if (dv.Count > 0)
                                {
                                    sP = (string)dv[0].Row["PP"];
                                    if (ValidUserPass((string)dv[0].Row["KP"], (string)dv[0].Row["PP"],
                                        (string)dv[0].Row["TABN"], (string)dv[0].Row["NMP"]))
                                    {
                                        bUserChanged = false;
                                        aEd.SetAvail(tPass, false);
                                        tPass.Text = sP;
                                        aEd.SetAvail(tUser, false);
                                        tUser.Text = xSm.sUName;
                                        aEd.SetCur(tDefKSKL);
                                    }
                                    else
                                    {
                                        bUserChanged = true;
                                        aEd.SetAvail(tPass, true);
                                        tPass.Text = "";
                                        aEd.SetAvail(tUser, true);
                                        tUser.Text = "";
                                        aEd.SetCur(tUser);
                                    }
                                }
                            }
                            else
                            {
                                if (nTab.ToString() == xSm.sUserTabNom)
                                {
                                    EndEditAvt(true);
                                }
                                else
                                    Srv.ErrorMsg("Другой пользователь!", true);
                            }
                        }
                    }
                    catch
                    {
                        nTab = 0;
                    }
                }
            }
            bScanProceed = false;
        }





        void cbReg_SelectedValueChanged(object sender, EventArgs e)
        {
            xSm.RegApp = (int)cbReg.SelectedValue;
        }

        // Начало редактирования
        public void BeginEditAvt(int nReg, bool cbE)
        {
            //bool bF;
            //aEd = new AppC.EditList(new AppC.VerifyEditFields(VerifyAvt));
            aEd = new AppC.EditListC(new AppC.VerifyEditFields(VerifyAvt));

            if ((nReg != AppC.AVT_TOUT) && (nReg != AppC.AVT_PARS))
                aEd.AddC(tUser);
            else
            {
                tUser.Text = xSm.sUName;
                tUser.Enabled = false;
            }

            if (nReg != AppC.AVT_PARS)
                aEd.AddC(tPass);
            else
                tPass.Enabled = false;

            if (nReg != AppC.AVT_TOUT)
            {
                // склады грузили ?
                aEd.AddC(tDefKSKL, (xNSI.DT[NSI.NS_SKLAD].nState == NSI.DT_STATE_READ) ? true : false);
                aEd.AddC(tDefUch, IsUch(xSm.nSklad));
                aEd.AddC(tDefDateDoc);
                //aEd.AddC(cbReg, ((xSm.nSklad == 1) || (xSm.nSklad == 8)) ? true : false);
                aEd.AddC(cbReg, cbE);
            }
            aEd.SetCur(aEd[0]);
        }

        // Корректность введенного
        private AppC.VerRet VerifyAvt()
        {
            AppC.VerRet v;
            bool bGoodUser = false;

            try
            {
                if (nCurReg != AppC.AVT_TOUT)
                {
                    //if (tUser.Enabled)
                    //    bGoodUser = ValidUserPass(xSm.sUser, xSm.sUserPass, "", "");
                    //else
                    //    bGoodUser = true;

                    bGoodUser = ValidUserPass(xSm.sUser, xSm.sUserPass, "", "");
                }
                else
                {
                    if (nCurReg != AppC.AVT_PARS)
                        bGoodUser = (tPass.Text == xSm.sUserPass) ? true : false;
                    else
                        bGoodUser = true;
                }

            }
            catch
            {
                bGoodUser = false;
            }

            if (!bGoodUser)
            {
                v.nRet = AppC.RC_CANCEL;
                Srv.ErrorMsg("Неверный пользователь или пароль!");
            }
            else
                v.nRet = AppC.RC_OK;

            v.cWhereFocus = null;
            return (v);
        }

        // Завершение редактирования
        public void EndEditAvt(bool bAuthPassed)
        {
            //aEd.EditIsOver(this);
            if (bAuthPassed)
            {// все нормально, данные корректны
                if (nCurReg == AppC.AVT_LOGON)
                    xSm.dBeg = DateTime.Now;
                if ((nCurReg == AppC.AVT_LOGON) ||
                    (nCurReg == AppC.AVT_PARS))
                    xSm.Uch2Lst(xSm.nUch, true);
                this.DialogResult = DialogResult.OK;

            }
            else
            {
                if (nCurReg == AppC.AVT_PARS)
                {
                    xSm.nSklad = xOldSm.nSklad;
                    xSm.nUch = xOldSm.nUch;
                    xSm.DocData = xOldSm.sDatDoc;
                    xSm.RegApp = xOldSm.nReg;
                }
                this.DialogResult = DialogResult.Cancel;
            }
        }


        // сброс/установка полей ввода/вывода
        private void SetAvtFields()
        {
            tUser.Text = "";
            tPass.Text = "";

            tDefNameSkl.Text = xNSI.GetNameSPR(NSI.NS_SKLAD, new object[] { xSm.nSklad }, "NAME").sName;

            if ((tDefUch.Enabled == true) && (xSm.nUch > 0))
            {
                tDefNameUch.Text = xNSI.GetNameSPR(NSI.NS_SUSK,
                new object[] { xSm.nSklad, xSm.nUch }, "NAME").sName;
            }
            else
            {
                tDefUch.Text = "";
                tDefNameUch.Text = "";
            }
            //tDefDateDoc.Text = DateTime.Now.ToString("dd.MM.yy");
            tDefDateDoc.DataBindings[0].ReadValue();
        }

        private void SelAllTextF(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        // есть участки для склада?
        private bool IsUch(int nSklad)
        {
            DataView dv = new DataView(xNSI.DT[NSI.NS_SUSK].dt, String.Format("KSK={0}", nSklad),
                "", DataViewRowState.CurrentRows);
            return ((dv.Count > 0) ? true : false);
        }


        // установка флага смены пользователя
        private void tUser_TextChanged(object sender, EventArgs e)
        {
            string s = tUser.Text.Trim().ToUpper();
            if (!bScanProceed)
            {
                bUserChanged = true;
                if ((s.Length <= AppC.SUSER.Length) && (s == AppC.SUSER.Substring(0, s.Length)))
                    tUser.PasswordChar = '*';
                else
                    tUser.PasswordChar = Char.MinValue;
            }
        }

        // сброс флага смены пользователя
        private void tUser_Validated(object sender, EventArgs e)
        {
            bUserChanged = false;
        }

        // проверка имени пользователя
        private void tUser_Validating(object sender, CancelEventArgs e)
        {
            if (bUserChanged == true)
            {
                string s = tUser.Text.Trim().ToUpper();
                if (s.Length > 0)
                {
                    if ((s == AppC.SUSER) || (s == AppC.GUEST))
                    {// пароль не нужен
                        tUser.Text = (s == AppC.SUSER) ? "Admin" : "Работник склада";
                        aEd.SetAvail(tPass, false);
                    }
                    else
                    {
                        try
                        {
                            aEd.SetAvail(tPass, true);
                            NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_USER, new object[] { s }, "NMP");
                            if (zS.bFind == false)
                                e.Cancel = true;
                            else
                                tUser.Text = zS.sName;
                        }
                        catch
                        {
                            e.Cancel = true;
                        }
                    }
                }
                if (e.Cancel != true)
                {
                    xSm.sUser = s;
                    xSm.sUName = tUser.Text;
                }
            }
        }

        // проверка пароля
        private void tPass_Validating(object sender, CancelEventArgs e)
        {
            string sP = tPass.Text.Trim();
            if ((nCurReg == AppC.AVT_LOGON) && (sP.Length > 0) && (xSm.sUser.Length > 0))
                e.Cancel = !ValidUserPass(xSm.sUser, sP, "", "");
        }

        private void tDefKSKL_Validating(object sender, CancelEventArgs e)
        {
            int nS = 0;
            string sS = tDefKSKL.Text.Trim();

            if (sS.Length > 0)
            {
                try
                {
                    nS = int.Parse(sS);
                    if (nS > 0)
                    {
                        NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_SKLAD, new object[] { nS }, "NAME");
                        e.Cancel = !zS.bFind;
                        tDefNameSkl.Text = zS.sName;
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            if (nS <= 0)
            {
                aEd.SetAvail(tDefUch, false);
                tDefUch.Text = "";
                tDefNameUch.Text = "";
            }
            else
            {
                aEd.SetAvail(tDefUch, IsUch(nS));
            }
            //if (nCurReg != AppC.AVT_TOUT)
            //    aEd.SetAvail(cbReg, ((nS == 1) || (nS == 8)) ? true : false);
        }

        // проверка участка
        private void tDefUch_Validating(object sender, CancelEventArgs e)
        {
            int nS = int.Parse(tDefKSKL.Text),
                nU = 0;
            string sU = tDefUch.Text.Trim();
            if (sU.Length > 0)
            {
                try
                {
                    nU = int.Parse(sU);
                    if (nU > 0)
                    {
                        NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_SUSK, new object[] { nS, nU }, "NAME");
                        e.Cancel = !zS.bFind;
                        tDefNameUch.Text = zS.sName;
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            if (nU <= 0)
            {
                tDefUch.Text = "0";
                tDefNameUch.Text = "";
            }
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }

        // проверка даты
        private void tDefDateDoc_Validating(object sender, CancelEventArgs e)
        {
            string sD = tDefDateDoc.Text.Trim();
            if (sD.Length > 0)
            {
                try
                {
                    sD = Srv.SimpleDateTime(sD);
                    DateTime d = DateTime.ParseExact(sD, "dd.MM.yy", null);
                    ((TextBox)sender).Text = sD;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
        }

        private int NextStep(bool bNext, int nCur)
        {
            if (bNext)
                // попытка вперед
                return ((nCur == cbReg.Items.Count - 1) ? 0 : nCur + 1);
            else
                // попытка назад
                return ((nCur == 0) ? cbReg.Items.Count - 1 : nCur - 1);
        }


        private int WhatNextMode(ref bool bNext, int nFunc)
        {
            int
                nI,
                nSpecItem = 0,
                nItems,
                ret = -1;

            // возможные варианты 
            // *+
            // *-
            // х+
            // х-
            nItems = Smena.bl.Count;
            if ((xMF.xPars.AppAvailModes != AppC.TOTAL_RESTRICT) && (xMF.xPars.AppAvailModes != AppC.TOTAL_AVAIL))
            {
                try
                {
                    nSpecItem = int.Parse(xMF.xPars.AppAvailModes.Substring(0, 1));
                }
                catch 
                {
                    //nSpecItem = AppC.REG_DOC;
                    nSpecItem = 1;
                }
            }



            switch (nFunc) 
            { 
                case 1:         // общее количество
                    if (xMF.xPars.AppAvailModes == AppC.TOTAL_RESTRICT)
                        ret = -1;                       // запрещены все режимы
                    else
                    {
                        if (xMF.xPars.AppAvailModes == AppC.TOTAL_AVAIL)
                            ret = xSm.RegApp;            // разрешены все режимы
                        else
                        {
                            if (xMF.xPars.AppAvailModes.Substring(1, 1) == "-")
                            {// конкретный режим запрещен
                                if (xSm.RegApp == nSpecItem)
                                    ret = NextStep(true, xSm.RegApp - 1) + 1;
                                else
                                    ret = xSm.RegApp;
                            }
                            else
                            {
                                ret = nSpecItem;    // конкретный режим разрешен
                                bNext = false;
                            }
                        }
                    }
                    break;

                case 2:
                    nI = cbReg.SelectedIndex;
                    ret = NextStep(bNext, nI);
                    if (xMF.xPars.AppAvailModes != AppC.TOTAL_AVAIL)
                    {// только если какой-то запретили
                        if ((ret + 1) == nSpecItem)
                        {
                            ret = NextStep(bNext, ret);
                            //ret = NextStep(bNext, ret + 1);
                        }
                    }
                    break;
            }

            return (ret);
        }

        private void Avtor_KeyDown(object sender, KeyEventArgs e)
        {
            bool ret = true;

            bSkipKey = ServClass.HandleSpecMode(e, true, xMF.xBCScanner);
            if (bSkipKey)
                return;

            switch (e.KeyValue)
            {
                case W32.VK_UP:
                    aEd.TryNext(AppC.CC_PREV);
                    break;
                case W32.VK_DOWN:
                    aEd.TryNext(AppC.CC_NEXT);
                    break;
                case W32.VK_ENTER:
                    if (aEd.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                        EndEditAvt(true);
                    break;
                case W32.VK_ESC:
                    EndEditAvt(false);
                    break;
                case W32.VK_LEFT:
                case W32.VK_RIGHT:
                    if (cbReg.Focused)
                    {
                        bool b = (e.KeyValue == W32.VK_RIGHT);
                        cbReg.SelectedIndex = WhatNextMode(ref b, 2);
                    }
                    break;
                case W32.VK_TAB:
                    if (e.Shift == true)
                        aEd.TryNext(AppC.CC_PREV);
                    else
                        aEd.TryNext(AppC.CC_NEXT);
                    break;
                default:
                    ret = false;
                    break;
            }

            e.Handled = ret;
            bSkipKey = ret;
        }

        private void Avtor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipKey == true)
            {
                bSkipKey = false;
                e.Handled = true;
            }
        }


        private void Time2SmEnd(SavuSocket.SocketStream stmX, Dictionary<string, string> aC,
            DataSet ds, ref string sErr, int nRetSrv)
        {
            double d = 0;
            if (nRetSrv == AppC.RC_OK)
            {
                try
                {
                    d = double.Parse(aC["SM2END"]);
                }
                catch
                {
                    d = 0;
                }
            }
            xSm.tMinutes2SmEnd = TimeSpan.FromMinutes(d);
            //xSm.dtSmEnd = DateTime.Now + xSm.tMinutes2SmEnd;
            return;
        }

        public string CheckUserLogin(string sUser, string sTNom, ref int nR)
        {
            string sErr = "";
            MainF.ServerExchange xSE = new MainF.ServerExchange(xMF);

            if (xMF.xPars.ReLogon > 0)
            {
                sErr = String.Format("(KSK={0},REG={1},TABN={2})", xSm.nSklad,
                    (nCurReg == AppC.AVT_LOGON) ? "SMSTART" :
                    (nCurReg == AppC.AVT_TOUT) ? "SMCONT" : "SMOVER", sTNom);

                LoadFromSrv dgL = new LoadFromSrv(Time2SmEnd);

                //Cursor crsOld = Cursor.Current;
                //Cursor.Current = Cursors.WaitCursor;
                sErr = xSE.ExchgSrv(AppC.COM_LOGON, sErr, sUser, dgL, null, ref nR);
                ///Cursor.Current = crsOld;
                nR = xSE.ServerRet;
            }
            else
            {
                xSm.tMinutes2SmEnd = TimeSpan.FromMinutes(60 * 24);
                //xSm.dtSmEnd = DateTime.Now + xSm.tMinutes2SmEnd;
            }

            return (sErr);
        }

        // проверка пароля для непустого пользователя
        public bool ValidUserPass(string sUser, string sPass, string sTabN, string sUN)
        {
            bool ret = false;
            int nRet = AppC.RC_OK;

            if (sUser.Length > 0)
            {
                if ((sUser != AppC.SUSER) && (sUser != AppC.GUEST))
                {
                    try
                    {
                        if (sTabN.Length > 0)
                        {// отсканирован штрихкод с табельным
                            ret = true;
                        }
                        else
                        {
                            DataView dv = new DataView(xNSI.DT[NSI.NS_USER].dt,
                                            String.Format("KP='{0}'", sUser), "", DataViewRowState.CurrentRows);
                            if (dv.Count == 1)
                            {
                                if (sPass == (string)dv[0].Row["PP"])
                                {
                                    ret = true;
                                    sUN   = (string)dv[0].Row["NMP"];
                                    sTabN = (string)dv[0].Row["TABN"];
                                }
                            }
                        }

                        string sE = CheckUserLogin(sUser, sTabN, ref nRet);
                        if (nRet != AppC.RC_OK)
                        {
                            ret = false;
                            Srv.ErrorMsg(sE, true);
                        }
                    }
                    catch { }
                }
                else
                {
                    ret = true;
                    sUN = (sUser == AppC.SUSER) ? "Admin" : "Работник склада";
                }
                if (ret)
                {
                    xSm.sUser = sUser;
                    xSm.sUName = sUN;
                    xSm.sUserPass = sPass;
                    xSm.sUserTabNom = sTabN;
                    xSm.urCur = (sUser == AppC.SUSER) ? Smena.USERRIGHTS.USER_SUPER :
                        Smena.USERRIGHTS.USER_KLAD;
                }
            }
            return (ret);
        }


        private void Avtor_Activated(object sender, EventArgs e)
        {
            if (this.Tag != null)
            {
                xMF = ((MainF)this.Tag);
                this.Tag = null;
                if (!AfterConstruct(xMF))
                    this.DialogResult = DialogResult.Cancel;
            }

        }



        #region Not_Used
        /*
        private Thread thReadNSI;               // догрузка справочников ()
        private static NSI nnSS;
        private static void ReadInThread()
        {
            nnSS.LoadLocNSI(new int[] { NSI.I_USER, NSI.I_SKLAD, NSI.I_SUSK, NSI.I_SMEN }, 0);
        }

* 
 * 
 */
        #endregion

    }
}