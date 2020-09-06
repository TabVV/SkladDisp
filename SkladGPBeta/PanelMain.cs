using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Reflection;

using ScannerAll;
using ExprDll;
using SavuSocket;
using PDA.OS;
using PDA.Service;

using FRACT = System.Decimal;

namespace SkladGP
{
    public partial class MainF : Form
    {

        // указатель на главное меню формы
        MainMenu 
            mmSaved;

        // форма авторизации
        //private Avtor fAv = null;

        // объект параметров
        public AppPars 
            xPars;

        // объект-сканер
        public BarcodeScanner xBCScanner = null;
        public BarcodeScanner.BarcodeScanEventHandler ehScan = null;
        
        // режимы гридов на вкладках
        private ScrMode 
            xScrDoc, xScrDet;

        // словарь доустимых функций
        public FuncDic 
            xFuncs;

        // класс работы со справочниками
        public NSI 
            xNSI;

        // текущий сеанс
        public Smena 
            xSm;

        // словарь с блоками кода
        public Dictionary<string, Srv.ExprAct> 
            xExpDic = new Dictionary<string, Srv.ExprAct>();
        public Expr 
            xGExpr = new Expr();

        public FuncPanel 
            xFPan;

        // текущие значения панели документов
        public CurDoc 
            xCDoc = null;

        // индикатор батареи
        private BATT_INF 
            xBBI;

        // индексы панелей
        public const int PG_DOC     = 0;
        public const int PG_SCAN    = 1;
        public const int PG_SSCC    = 2;
        public const int PG_NSI     = 3;
        public const int PG_PAR     = 4;
        public const int PG_SRV     = 5;


        // флаг режима редактирования
        public bool bEditMode = false;

        // обработчик клавиш для текущей функции
        //delegate bool CurrFuncKeyHandler(int nF, KeyEventArgs e);
        Srv.CurrFuncKeyHandler 
            ehCurrFunc = null;

        // протокол отладки/работы
        public static System.IO.StreamWriter 
            swProt;

        // флаг нормальной авторизации
        //private bool bGoodAvtor = false;

        // строка с данными по прибывшей машине (шлюзы)
        public object[] aAvtoPark = null;

        // объект с параметрами для внешних форм
        public object 
            xDLLPars = null;
        public object[] 
            xDLLAPars = null;

        public string 
            sExeDir;

        // объект с параметрами для обмена по сети с севером
        public ServerExchange 
            xExchg = null;


        // флаги для обработки клавиатурного ввода
        private bool 
            bSkipChar = false;                 // не обрабатывать введенный символ

        private Srv.HelpShow 
            xHelpS, 
            xDestInfo = null;

        private Srv.PicShow
            xPicShow = null;

        List<string> xInf;

        private void InitializeDop(BarcodeScanner xSc, Size BatSize, Point BatLoc)
        {
            string 
                sExePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            long 
                nMACNow;


            sExeDir = System.IO.Path.GetDirectoryName(sExePath) + "\\";
            xBCScanner = xSc;
#if SYMBOL
                if (xBCScanner.nTermType == TERM_TYPE.SYMBOL)
                {
                    if (AppPars.bArrowsWithShift == true)
                    {
                        ((ScannerAll.Symbol.SymbolBarcodeScanner)xBCScanner).SetSpecKeyALP(true, AlpHandle);
                    }
                }
#endif
#if PSC
                if ((xBCScanner.nTermType == TERM_TYPE.PSC4220) || (xBCScanner.nTermType == TERM_TYPE.PSC4410))
                    ((ScannerAll.PSC.PSCBarcodeScanner)xBCScanner).SetScanHandler(this);
#endif
#if NRDMERLIN
            ((ScannerAll.Nordic.Nordics)xBCScanner).BarCodeScanKey =
            (int)ScannerAll.Nordic.Nordics.VK_NRD.VK_SCAN;
            //((ScannerAll.Nordic.NordicMerlin)xBCScanner).RFIDScanKey = W32.VK_F13;
            xBCScanner.Start();
#endif

            xFPan = new FuncPanel(this, this.pnLoadDocG);

            xPars = (AppPars)AppPars.InitPars(System.IO.Path.GetDirectoryName(sExePath));

            // параметры приложения, которые не сохраняются в настройках на диске
            SetParAppFields();

            // параметры приложения, которые можно изменить и сохраняются на диске
            SetBindAppPars();


            //timeOffset = 10 
            TimeSync.SyncAsync(xPars.NTPSrv, 10);

            xNSI = new NSI(xPars, this, new string[]{NSI.NS_USER, NSI.NS_SKLAD, NSI.NS_SUSK});
            xNSI.ConnDTGrid(dgDoc, dgDet);
            xNSI.InitTableSSCC(dgSSCC);
            FiltForDocs(xPars.bHideUploaded, xNSI.DT[NSI.BD_DOCOUT]);

            Smena.ReadSm(ref xSm, xPars.sDataPath);

            nMACNow = long.Parse(xBCScanner.WiFi.MACAddreess, System.Globalization.NumberStyles.HexNumber);
            if ((nMACNow > 0) && (xSm.MACAdr != xBCScanner.WiFi.MACAddreess))
            {
                xSm.MACAdr = xBCScanner.WiFi.MACAddreess;
                xSm.SaveCS(xPars.sDataPath, xSm.nDocs);
            }

            // настройка выполняемых функций на клавиши конкретного терминала
            SetMainFuncDict(xBCScanner.nTermType, sExeDir);

            // создать индикатор батареи
            xBBI = new BATT_INF(pnPars, BatSize, BatLoc);
            xBBI.BIFont = 8F;

            //Font f = lFCgh.Font;
            //f = new Font("MM2000LC", f.Size, f.Style);
            //lFCgh.Font = f;
            
            //lFCgh.Text = "\x88F\x891";
            //byte[] bArr = { 0xE2, 0x86, 0x91 };
            //lFCgh.Text = Encoding.UTF8.GetString(new byte[] { 0xE0, 0x08, 0x8F });
            //lFCgh.Text = Encoding.UTF8.GetString(bArr, 0, 3);

            lFCgh.Text = "\xAD\xAF";

            //lFCgh.Refresh();
        }

        private DialogResult xLogonResult;
        public ManualResetEvent evReadNSI = null;


        private StreamWriter ProtStream(string sFName)
        {
            return (ProtStream(sFName, 500, 5));
        }

        private StreamWriter ProtStream(string sFName, int nMaxLength)
        {
            return (ProtStream(sFName, nMaxLength, 5));
        }

        private StreamWriter ProtStream(string sFName, int nMaxLength, int nDiv)
        {
            //bool
            //    bAppendWhenOpen = true;
            long
                MAX_PROT = 1024 * nMaxLength;
            string 
                s = "";

            StreamWriter 
                sw = null;

            try
            {
                if (File.Exists(sFName))
                {
                    using (FileStream fs = System.IO.File.Open(sFName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        long nNewLen = fs.Length;
                        if (nNewLen > MAX_PROT)
                        {
                            if (nDiv <= 0)
                                nDiv = 5;
                            //byte[] fileData = new byte[fs.Length];
                            //fs.Read(fileData, 0, (int)fs.Length);
                            //s = Encoding.UTF8.GetString(fileData, 0, fileData.Length);
                            //s = DateTime.Now.ToString("") + " - Обновление\n" + s.Substring(s.Length - s.Length / 5);
                            nNewLen = nNewLen / nDiv;
                            fs.Seek(0, SeekOrigin.End);
                            fs.Seek(-nNewLen, SeekOrigin.End);
                            byte[] fileData = new byte[nNewLen];
                            nNewLen = fs.Read(fileData, 0, (int)nNewLen);
                            s = Encoding.UTF8.GetString(fileData, 0, (int)nNewLen);
                            s = DateTime.Now.ToString("") + " - Уменьшение буфера....\n" + s;
                            fs.SetLength(0);
                        }
                        fs.Close();
                    }
                }
                //sw = new StreamWriter(sFName, bAppendWhenOpen);

                Stream ssfs = new FileStream(sFName, FileMode.Append, FileAccess.Write, FileShare.Read);
                sw = new StreamWriter(ssfs);
                sw.AutoFlush = true;
                sw.Write(s);
            }
            catch
            {
                sw = null;
            }
            return(sw);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int
                nProtSize = 500;

            xScrDoc = new ScrMode(dgDoc);
            xScrDet = new ScrMode(dgDet);

            //swProt = File.CreateText(xPars.sDataPath + "ProtTerm.txt");
            if (xPars.DebugLevel > 0)
                nProtSize = xPars.DebugLevel;
            swProt = ProtStream(xPars.sDataPath + "ProtTerm.txt", nProtSize);

            // чтение - в форме авторизации
            evReadNSI = new ManualResetEvent(false);

            xDLLPars = AppC.AVT_LOGON;
            xLogonResult = CallDllForm(sExeDir + "SGPF-Avtor.dll", false);

            SetEditMode(false);

            if (xLogonResult != DialogResult.OK)
            {
                evReadNSI.Set();
                this.Close();
            }
            else
            {
                try
                {
                    evReadNSI.WaitOne();
                    evReadNSI = null;
                    ehScan = new BarcodeScanner.BarcodeScanEventHandler(OnScan);
                    xBCScanner.BarcodeScan += ehScan;
                    AfterAddScan += new ScanProceededEventHandler(OnPoddonReady);
                    //ssListen = new SocketStream(11001);
                    //_processCommand = new ProcessSocketCommandHandler(ProcessSocketCommand);
                    //ssListen.MessageFromServerRecived += new MessageFromServerEventHandler(cs_MessageFromServerRecived);
                    //xNSI.AllNsiInf(true);
                    //xNSI.LoadLocNSI(new string[] { NSI.BD_TINF }, NSI.LOAD_ANY);
                    if (xNSI.DT[NSI.BD_TINF].nState == NSI.DT_STATE_READ)
                        xNSI.DT[NSI.BD_TINF].dt.AcceptChanges();

                    if (!AfterAuth(AppC.AVT_LOGON))
                        this.Close();
                    else
                    {
                        xHelpS = new Srv.HelpShow(this);
                        xPicShow = new Srv.PicShow(this);
                        WriteAllToReg();
                        InitPanels();
                        // показать индикатор батареи
                        xBBI.EnableShow = true;
                        Cursor.Current = Cursors.WaitCursor;

                        xNSI.DSRestore(xPars.sDataPath, Smena.DateDef, xPars.Days2Save, true);
                        xSm.nDocs = xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Count;
                        Cursor.Current = Cursors.Default;

                        //Srv.LoadInterCode(xGExpr, xExpDic, xNSI.DT[NSI.BD_PASPORT]);
                        //xPars.CanEditIDNum = (xSm.sUser == AppC.SUSER) ? true : false;
                        if (xSm.sUser == AppC.SUSER)
                            xPars.CanEditIDNum = true;

                        EnterInDoc();

                    }

                }
                finally
                {
                }
            }
        }

        // таймеры на выход из программы
        private bool AfterAuth(int nReg)
        {
            bool bRet = AppC.RC_OKB;

            if (xSm.urCur != Smena.USERRIGHTS.USER_SUPER)
            {
                if ((xPars.ReLogon > 0) && !ResetTimerSmEnd())
                {// авторизация проверяется сервером
                    Srv.ErrorMsg("Смена окончена!", true);
                    bRet = false;
                }
                else
                    ResetTimerReLogon(false);
            }
            return (bRet);
        }

        // обработчик окончания смены
        void xtmSmEnd_Tick(object sender, EventArgs e)
        {
            bool bQuit = false;

            xSm.xtmSmEnd.Enabled = false;

            if (!bInScanProceed)
            {
                if (xSm.xtmTOut != null)
                    // заодно отключаем и все остальные
                    xSm.xtmTOut.Enabled = false;
                xDLLPars = AppC.AVT_LOGOFF;
                DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Avtor.dll", true);
                bQuit = (xDRslt != DialogResult.OK) ? true : !AfterAuth(AppC.AVT_LOGOFF);
                if (bQuit)
                    this.Close();
            }
            else
            {// попробуем еще раз через 10 секунд
                xSm.xtmSmEnd.Interval = 10 * 1000;
                xSm.xtmSmEnd.Enabled = false;
            }
        }

        // обработчик таймаута по бездействию терминала
        void xtmTOut_Tick(object sender, EventArgs e)
        {
            bool bQuit = false;

            xSm.xtmTOut.Enabled = false;
            if (!bInScanProceed)
            {
                if (xSm.xtmSmEnd != null)
                    // заодно отключаем и все остальные
                    xSm.xtmSmEnd.Enabled = false;
                xDLLPars = AppC.AVT_TOUT;
                DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Avtor.dll", true);
                bQuit = (xDRslt != DialogResult.OK) ? true : !AfterAuth(AppC.AVT_TOUT);
                if (bQuit)
                    this.Close();
            }
            else
            {// попробуем еще раз через 10 секунд
                xSm.xtmTOut.Interval = 10 * 1000;
                xSm.xtmTOut.Enabled = true;
            }
        }

        // сброс таймеров по завершении смены пользователя
        private bool ResetTimerSmEnd()
        {
            bool bTimerStarted = true;
            if (xSm.tMinutes2SmEnd.TotalMinutes > 0)
            {
                // авторизация проверяется сервером и имеет ограничения по времени
                if (xSm.xtmSmEnd != null)
                {
                    xSm.xtmSmEnd.Enabled = false;
                    //xSm.xtmSmEnd = null;
                }
                else
                {
                    xSm.xtmSmEnd = new System.Windows.Forms.Timer();
                    xSm.xtmSmEnd.Tick += new EventHandler(xtmSmEnd_Tick);
                }
                xSm.xtmSmEnd.Interval = (int)xSm.tMinutes2SmEnd.TotalMilliseconds;
                xSm.xtmSmEnd.Enabled = true;
            }
            return (bTimerStarted);
        }

        // старт/перезапуск таймера бездействия терминала
        private bool ResetTimerReLogon(bool bRestart)
        {
            bool bTimerStarted = false;
            int nMinutesReLogon = Math.Abs(xPars.ReLogon);

            if (xSm.urCur == Smena.USERRIGHTS.USER_SUPER)
                return (false);

            if ((bRestart) && (xSm.xtmTOut != null))
            {// таймер может быть отключен
                if (xSm.xtmTOut.Enabled == false)
                    return (false);
            }

            if (nMinutesReLogon >= Smena.MIN_TIMEOUT)
            {// таймаут для перелогина от 5 минут
                if (xSm.xtmTOut != null)
                {
                    xSm.xtmTOut.Enabled = false;
                    //xSm.xtmTOut = null;
                }
                else
                {
                    xSm.nMSecondsTOut = nMinutesReLogon * 60 * 1000;
                    xSm.xtmTOut = new System.Windows.Forms.Timer();
                    xSm.xtmTOut.Tick += new EventHandler(xtmTOut_Tick);

                }
                xSm.xtmTOut.Interval = xSm.nMSecondsTOut;
                xSm.xtmTOut.Enabled = true;
                bTimerStarted = true;
            }
            return(bTimerStarted);
        }
        private bool WriteAllToReg()
        {
            return (WriteAllToReg(false));
        }

        private bool WriteAllToReg(bool bFlagProt)
        {
            const string sApp = "SkladGP";
            bool 
                ret = true;
            string 
                sUserFlag = xSm.sUser + " " + xSm.sUName,
                sAppVer = (string)Srv.AppVerDT()[0];

            if (bFlagProt)
                sUserFlag += " !!!";
            else
            {
                ret &= Srv.WriteRegInfo("SkladgpLastRunTime", xSm.dBeg.ToString(), sApp);     // Время последнего запуска программы
                ret &= Srv.WriteRegInfo("SkladgpVer", sAppVer, sApp);          // Версия программы SkladGP
                ret &= Srv.WriteRegInfo("SkladgpReg",
                    (xSm.RegApp == AppC.REG_DOC) ? "DOC" : "MRK", sApp);   // Режим работы SkladGP
            }
            ret &= Srv.WriteRegInfo("FIO", sUserFlag, sApp);           // ФИО пользователя, загрузившего программу

            return (ret);
        }


        // вывод панели обмена данными
        public class FuncPanel
        {
            private const int
                C_REGH = 22;

            private bool
                bActive;
            private int
                nWMDelta;                       // поправка на WinMobile экран

            private MainF 
                xF;
            private Point 
                pInvisible,
                pVisible;

            public Panel 
                xPan;
            private Control 
                xRetHere = null,
                xTReg;
            private Label 
                xLabH, xLabF;

            public FuncPanel(MainF f, Panel xPl)
            {
                xF = f;
                if (xPl == null)
                {
                    xPan = xF.pnLoadDocG;
                }
                else
                {
                    xPan = xPl;
                }

                xTReg = xF.tbPanP1G;
                xLabH = xF.lFuncNamePanG;
                xLabF = xF.lpnLoadInfG;
                xLabF.Text = "<Enter> - начать";
                pInvisible = xPan.Location;
#if (DOLPH7850 || DOLPH9950)
                nWMDelta = -23;
#else
                nWMDelta = 0;
#endif

                pVisible = (xF.tcMain.SelectedIndex == PG_DOC) ? new Point(6, 60) : new Point(6, 90);
                bActive = false;
            }

            private void ShowPNow(int x, int y, string sH, string sR)
            {
                if (bActive == false)
                {
                    bActive = true;
                    xPan.SuspendLayout();
                    xPan.Left = x;
                    xPan.Top = y + nWMDelta;
                    xLabH.Text = sH;
                    xTReg.Text = sR;

                    xPan.Visible = true;
                    xPan.Enabled = true;
                    xPan.ResumeLayout();
                    xPan.Refresh();
                }
            }

            public void ShowP(string s, string sR)
            {
                ShowPNow(pVisible.X, pVisible.Y, s, sR);
            }

            public void ShowP(int x, int y, string s, string sR)
            {
                ShowPNow(x, y, s, sR);
            }

            public void ShowP(int x, int y, string s, string sR, Control cWhereBack)
            {
                xRetHere = cWhereBack;
                ShowPNow(x, y, s, sR);
            }


            public void UpdateHead(string s)
            {
                xLabH.Text = s;
                xLabH.Refresh();
            }

            public void UpdateReg(string s)
            {
                xTReg.Text = s;
                xTReg.Refresh();
            }
            public void UpdateSrv(string s)
            {
                xF.tbPanP2G.Text = s;
                xF.tbPanP2G.Refresh();
            }

            public void UpdateHelp(string s)
            {
                xLabF.Text = s;
                xLabF.Refresh();
            }

            public void HideP()
            {
                HideP(null);
            }

            public void HideP(Control cFocus)
            {
                if (bActive == true)
                {
                    bActive = false;
                    xPan.Location = pInvisible;
                    xPan.Visible = false;
                    xPan.Enabled = false;
                    if (cFocus != null)
                        cFocus.Focus();
                    else if (xRetHere != null)
                        xRetHere.Focus();
                    xRetHere = null;
                }
            }

            public void IFaceReset(bool bClear)
            {
                if (bClear)
                {
                    xF.lSrvGName.Text = "";
                    xF.tbPanP2G.Text = "";
                    xF.lFCgh.Text = "";
                    xF.lpnLoadInfG.Text = "";
                }
                else
                {
                    xF.lSrvGName.Text = "Сервер";
                    xF.lFCgh.Text = "\xAD\xAF";
                    xF.lpnLoadInfG.Text = "<Enter> - начать";
                    xTReg.Height = C_REGH;
                }
            }

            private int
                m_OldHeight = -1;
            public void InfoHeightUp(bool bSetNew, int nKoeff)
            {
                int
                    h;
                if (bSetNew)
                {
                    h = (m_OldHeight != -1) ? m_OldHeight : xF.tbPanP1G.Height;
                    m_OldHeight = h;
                    xF.tbPanP1G.Height = h * nKoeff;
                    xF.tbPanP1G.BringToFront();
                }
                else
                {
                    if (m_OldHeight != -1)
                    {
                        xF.tbPanP1G.Height = m_OldHeight;
                        m_OldHeight = -1;
                        xF.tbPanP1G.SendToBack();
                    }
                }
            }


            public bool IsShown
            {
                get {return bActive;}
            }

            public string RegInf
            {
                get { return xTReg.Text; }
                set
                {
                    xTReg.Text = value;
                    xTReg.Refresh();
                }
            }

            public Control RegControl
            {
                get { return xTReg; }
            }

        }


        private void SelAllTextF(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }




        private void InitPanels()
        {
            // Ссылки на Controls формы для панели параметров (документы, загрузка,...)
            //DocPars.tKSkl = this.tKSkl_p;
            //DocPars.tNSkl = this.tNSkl_p;

            DocPars.tKUch = this.tKUch_p;
            DocPars.tDate = this.tDateD_p;
            
            DocPars.tKTyp = this.tKT_p;
            DocPars.tNTyp = this.tNT_p;

            DocPars.tKEks = this.tKEks_p;
            DocPars.tKPol = this.tKPol_p;

            //if (xSm.RegApp == AppC.REG_OPR)
            //{
            //    lPoluch.Text = "Операция";
            //    //Smena.TypDef = AppC.TYPD_OPR;
            //}

            //colPanelBack = tpScan.BackColor;        // 
        }


        private void SaveCurData(bool bFinalSaving)
        {
            DataTable dtU = xNSI.DT[NSI.BD_TINF].dt.GetChanges(DataRowState.Unchanged);
            try
            {
                if ((dtU == null) ||
                    (dtU.Rows.Count != xNSI.DT[NSI.BD_TINF].dt.Rows.Count))
                    // что-то все же поменяли
                    xNSI.DT[NSI.BD_TINF].dt.WriteXml(xPars.sNSIPath + xNSI.DT[NSI.BD_TINF].sXML);

                // сохранение рабочих данных (если есть)
                if (xLogonResult == DialogResult.OK)
                {// авторизация прошла успешно
                    if (bFinalSaving)
                        xSm.SaveCS(xPars.sDataPath, xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Count);
                    xNSI.DSSave(xPars.sDataPath);
                }
                if (bFinalSaving)
                {
                    if (swProt != null)
                        swProt.Close();
                }
            }
            catch { }
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (xBCScanner != null)
                xBCScanner.Dispose();
            SaveCurData(true);

            Cursor.Current = Cursors.Default;
        }

        // восстановление рабочих данных (при необходимости)
        //private void TryRestoreUserDat()
        //{
        //    //Smena xSaved = null;
        //    //int nRet = Smena.ReadSm(ref xSaved, xPars.sDataPath);

        //    //int nRet = Smena.ReadSm(ref xSaved, xPars.sDataPath);
        //    //if (nRet == AppC.RC_OK)
        //    //{
        //        if (xSm.nDocs > 0)
        //        {// данные действительно есть
        //            //nRet = xNSI.DSRestore(false, xPars.sDataPath, Smena.DateDef, xPars.Days2Save);
        //            xNSI.DSRestore(xPars.sDataPath, Smena.DateDef, xPars.Days2Save);
        //            xSm.nDocs = xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Count;
        //        }
        //    //}
        //}



        private int nPrevTab = PG_DOC;
        private bool bPrevMode;

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (nPrevTab == PG_PAR)
            {
                xBCScanner.TouchScr(false);
                nPrevTab = -1;
                SetEditMode(bPrevMode);
            }

            switch (tcMain.SelectedIndex)
            {
                case PG_DOC:
                    EnterInDoc();
                    break;
                case PG_SCAN:
                    bool ChkDoc = false;
                    try
                    {
                        ChkDoc = ((int)xCDoc.drCurRow["SSCCONLY"] > 0) ? true : false;
                    }
                    catch
                    {
                        ChkDoc = false;
                    }

                    if ((ChkDoc) && (nPrevTab == PG_DOC))
                        tcMain.SelectedIndex = PG_SSCC;
                    else
                        EnterInScan();
                    break;
                case PG_SSCC:
                    EnterInSSCC();
                    break;
                case PG_NSI:
                    EnterInNSI();
                    break;
                case PG_PAR:
                    bPrevMode = bEditMode;
                    //nPrevTab = PG_PAR;
                    xBCScanner.TouchScr(true);
                    EnterInPars();
                    break;
                case PG_SRV:
                    EnterInServ();
                    break;
            }
            nPrevTab = tcMain.SelectedIndex;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            bool 
                bAlreadyProceed = false,   // клавиша уже обработана
                bHandledKey = false;
            int 
                i = -1,
                nF = 0;

            #region Убрать Scan-клавишу
            if (e.Modifiers == Keys.None)
            {// Scan-клавишу для Honeywell не обрабатываем
#if DOLPH9950
                if (e.KeyValue == 42)
                {
                    xBCScanner.Start();
                    e.Handled = true;
                    bSkipChar = e.Handled;
                    return;
                }
#endif
#if DOLPH7850
                if (e.KeyValue == 42)
                {
                    e.Handled = true;
                    bSkipChar = e.Handled;
                    return;
                }
#endif
#if HWELL6100
                if (e.KeyValue == 193)
                {
                    e.Handled = true;
                    bSkipChar = e.Handled;
                    return;
                }
#endif
#if NRDMERLIN
                if (e.KeyValue == (int)ScannerAll.Nordic.Nordics.VK_NRD.VK_SCAN)
                {
                    e.Handled = true;
                    bSkipChar = e.Handled;
                    ((ScannerAll.Nordic.Nordics)xBCScanner).PerformScan();
                    return;
                }
#endif

            }
            #endregion

            bSkipChar = ServClass.HandleSpecMode(e, bEditMode, xBCScanner);
            if (bSkipChar == true)
                return;

            nF = xFuncs.TryGetFunc(e);

            if ( bEditMode || bInEasyEditWait || (nSpecAdrWait == AppC.F_SIMSCAN) )
            {// для режима редактирования
                if ( Srv.IsDigKey(e, ref i) ||
                    (e.KeyValue == W32.VK_BACK) || 
                    (e.KeyValue == W32.VK_PERIOD))
                {
                    if (e.Modifiers == Keys.None)
                        nF = 0;
                }
            }
            try
            {
                if (ehCurrFunc != null)
                {// клавиши ловит одна из функций
                    bAlreadyProceed = ehCurrFunc(nF, e, ref ehCurrFunc);
                }
                else
                {
                    // обработка функций и клавиш с учетом текущего Control
                    if (tcMain.SelectedIndex == PG_DOC)
                        bAlreadyProceed = Doc_KeyDown(nF, e);
                    else if (tcMain.SelectedIndex == PG_SCAN)
                        bAlreadyProceed = Vvod_KeyDown(nF, e);
                    else if (tcMain.SelectedIndex == PG_SSCC)
                        bAlreadyProceed = SSCCList_KeyDown(nF, e);
                    else if (tcMain.SelectedIndex == PG_NSI)
                        bAlreadyProceed = NSI_KeyDown(nF, e);
                    else if (tcMain.SelectedIndex == PG_PAR)
                        bAlreadyProceed = AppPars_KeyDown(nF, e);
                }

                if ((nF > 0) && (bAlreadyProceed == false))
                {// общая обработка функций
                    bHandledKey = ProceedFunc(nF, e, sender);
                }
            }
            catch (Exception ex)
            {
                string
                    sPrt,
                    sE = "Ошибка обработки";
                    
                sPrt = String.Format("{0}\n{1}", sE, ex.Message);

                WriteProt(DateTime.Now.ToString("dd.MM.yy HH:mm:ss - ") + sPrt + "\n");

                Srv.ErrorMsg(sPrt, sE, true);
                if (aEdVvod != null)
                {
                    aEdVvod.EditIsOver();
                    SetEditMode(false);
                }
            }


            // а здесь - только клавиши
            e.Handled = bAlreadyProceed || bHandledKey;
            if ((bAlreadyProceed == false) && (bHandledKey == false))
            {
                switch (e.KeyValue)     // для всех вкладок
                {
                    case W32.VK_ENTER:
                        e.Handled = true;
                        break;
                }
            }

            bSkipChar = e.Handled || bAlreadyProceed || bHandledKey;
            ResetTimerReLogon(true);
            //ClearAttentionInfo();
        }

        private void MainF_KeyUp(object sender, KeyEventArgs e)
        {

            try
            {
                if (e.KeyCode == (Keys)42)
                {
#if DOLPH9950
                    //--- If Still Trying to Decode, Cancel the Operation ---
                    //oDecodeAssembly.CancelScanBarcode();
                    xBCScanner.Stop();

                    //--- Add the KeyDown Event Handler ---
                    //this.KeyDown += new KeyEventHandler(Form1_KeyDown);

                    //--- The Key was Handled ---
                    e.Handled = true;
#endif
                }
            }
            catch
            {
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
           if (bSkipChar == true)
            {
                e.Handled = true;
                bSkipChar = false;
            }
        }
        private DialogResult CallDllForm(string sAs, bool bClearScHdlr)
        {
            return(CallDllForm(sAs, bClearScHdlr, null));
        }

        private DialogResult CallDllForm(string sAs, bool bClearScHdlr, object[] xPars4Form)
        {
            DialogResult xRet = DialogResult.None;
            Assembly xAs;
            Form xDllForm = null;
            bool bOldTOut = false;

            if (bClearScHdlr)
                xBCScanner.BarcodeScan -= ehScan;
            try
            {

                if (xSm.xtmTOut != null)
                {
                    bOldTOut = xSm.xtmTOut.Enabled;
                    xSm.xtmTOut.Enabled = false;
                }

                xAs = Assembly.LoadFrom(sAs);
                Type[] xTT = xAs.GetTypes();
                xDllForm = (Form)Activator.CreateInstance(xAs.GetTypes()[0]);

                // передача параметров в форму
                if (xPars4Form == null)
                    xDllForm.Tag = this;
                else
                    xDllForm.Tag = xPars4Form;

                // Вызов события EnableChanged для завершения инициализации формы
                //if (!xDllForm.Enabled)
                //    xDllForm.Enabled = true;
                //else
                //{// альтернативный способ вызова формы
                //    //xDllForm.Parent = this;

                //    xDllForm.Enabled = false;
                //    xDllForm.Enabled = true;
                //    //xDllForm.Text += " ";
                //}

                // обработка переданных параметров
                //xDllForm.Enabled = !(xDllForm.Enabled);

                //if ( ((xDllForm.Owner == null) && (xDllForm.DialogResult == DialogResult.None)) ||
                //    (xDllForm.Tag == null))
                //    xRet = xDllForm.ShowDialog();
                //else
                //{
                //    xRet = xDllForm.DialogResult; 
                //}

                // стары способ
                //if (((xDllForm.Owner == null) && (xDllForm.DialogResult == DialogResult.None))||
                //    // новый
                //    (xDllForm.DialogResult == DialogResult.Retry))
                //    xRet = xDllForm.ShowDialog();
                //else
                //    xRet = xDllForm.DialogResult;


                xRet = xDllForm.ShowDialog();


            }
            catch (Exception e)
            {
                Srv.ErrorMsg("Форма " + sAs, true);
            }
            finally
            {
                if (xDllForm != null)
                {
                    xDLLPars = xDllForm.Tag;
                    xDllForm.Dispose();
                }
                if (bClearScHdlr)
                    xBCScanner.BarcodeScan += ehScan;
                xAs = null;
                if (xSm.xtmTOut != null)
                    xSm.xtmTOut.Enabled = bOldTOut;
            }
            return xRet;
        }













        private BindingSource SetBlankList()
        {
            BindingSource
                bsBlanks = new BindingSource();
            try
            {
                string sRf = String.Format("(TD={0})OR(TD<0)OR(ISNULL(TD,-1)<0)", xCDoc.xDocP.nTypD);
                DataView dv = new DataView(xNSI.DT[NSI.NS_BLANK].dt, sRf,
                    "TD DESC", DataViewRowState.CurrentRows);
                bsBlanks.DataSource = dv;
            }
            catch { }
            return (bsBlanks);
        }



        private int CallFrmPars()
        {
            int
                nRegCall,
                nRet = AppC.RC_CANCEL;
            DataRow
                drD = null;
            DialogResult xDRslt;
            BindingSource
                bsBlanks;

            bsBlanks = SetBlankList();
            if (bsBlanks.Count > 0)
            {
                if (bsBlanks.Count == 1)
                {
                    nRegCall = AppC.R_PARS;
                }
                else
                {
                    nRegCall = AppC.R_BLANK;
                }
                if (tcMain.SelectedIndex == PG_SCAN)
                    drD = drDet;

                //nRegCall = AppC.R_BLANK;
                Srv.ExchangeContext.ExchgReason = AppC.EXCHG_RSN.USER_COMMAND;
                xDRslt = CallDllForm(sExeDir + "SGPF-Univ.dll", true,
                    new object[] {this, AppC.COM_PRNBLK, nRegCall,
                            xSm.CurPrinterSTCName, xSm.CurPrinterMOBName, drD, bsBlanks});
                if (xDRslt == DialogResult.OK)
                {
                    xDLLAPars = (object[])xDLLPars;
                    xSm.CurPrinterSTCName = (string)xDLLAPars[0];
                    xSm.CurPrinterMOBName = (string)xDLLAPars[1];
                }
                Srv.ExchangeContext.ExchgReason = AppC.EXCHG_RSN.NO_EXCHG;

                if (tcMain.SelectedIndex == PG_SCAN)
                    dgDet.Focus();
                else
                    dgDoc.Focus();
            }
            else
            {
                Srv.ErrorMsg("Нет бланков!", true);
            }
            return (nRet);
        }


        private bool IsDupSSCC(string sSSCC)
        {
            DataView dvT = new DataView(xNSI.DT[NSI.BD_DOUTD].dt, 
                String.Format("(SYSN={0}) AND (SSCC='{1}')", xCDoc.drCurRow["SYSN"], sSSCC),
                "KMC,EMK DESC", DataViewRowState.CurrentRows);
            return( (dvT.Count > 0)?true:false );
        }

        Srv.CurrFuncKeyHandler
            ehCurrFuncW4 = null;

        private bool Keys4FixAddr(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            int 
                nRet = AppC.RC_OK,
                nFunc = (int)nF;
            string
                sSSCC,
                sErr,
                sPar;
            bool
                bSimulScan = false,
                bHandledByOther = false,
                bCloseWait = false,
                bKeyHandled = false;
            ServerExchange
                xSE;

            try
            {
                if (ehCurrFuncW4 != null)
                {// клавиши ловит одна из функций
                    bHandledByOther = ehCurrFuncW4(nFunc, e, ref ehCurrFuncW4);
                }
            }
            catch
            {
                Srv.ErrorMsg("Ошибка обработки", true);
                bHandledByOther = true;
            }

            bKeyHandled = bHandledByOther;

            if (!bHandledByOther)
            {
                if (nFunc > 0)
                {
                    //if (nFunc != PDA.Service.AppC.F_HELP)
                        bKeyHandled = true;
                }
                else
                {
                    if ((e.KeyValue == W32.VK_ENTER) ||
                        ((e.KeyValue == W32.VK_TAB) && (
                        (nSpecAdrWait == AppC.F_CELLINF)
                        || (nSpecAdrWait == AppC.F_SIMSCAN)
                        || (nSpecAdrWait == AppC.F_CHKSSCC)
                        || (nSpecAdrWait == AppC.F_CNTSSCC))))
                    {
                        bCloseWait = true;
                        switch (nSpecAdrWait)
                        {
                            case AppC.F_CHKSSCC:
                                // Загрузка SSCC в заявку
                            case AppC.F_CNTSSCC:
                                // содержимое SSCC
                                sSSCC = xCDoc.sSSCC;

                                if (sSSCC.Length == 20)
                                {
                                    PSC_Types.ScDat scD = scCur;

                                    try
                                    {
                                        xSE = xCLoad.xLastSE;
                                        nRet = (xCLoad.dtZ.Rows.Count > 1) ? AppC.RC_MANYEAN : AppC.RC_OK;
                                    }
                                    catch
                                    {
                                        xSE = new ServerExchange(this);
                                        if (nSpecAdrWait == AppC.F_CNTSSCC)
                                            nRet = ConvertSSCC2Lst(xSE, sSSCC, ref scD, false);
                                        else
                                            nRet = ConvertSSCC2Lst(xSE, sSSCC, ref scD, true, xNSI.DT[NSI.BD_DIND].dt);
                                    }

                                    if ((nRet == AppC.RC_OK) || (nRet == AppC.RC_MANYEAN))
                                    {
                                        if (e.KeyValue == W32.VK_ENTER)
                                        {// показать содержимое SSCC
                                            ShowSSCCContent(xCLoad.dtZ, sSSCC, xSE, xCDoc.xOper.xAdrSrc, ref ehCurrFuncW4);
                                            bCloseWait = false;
                                        }
                                        else if (e.KeyValue == W32.VK_TAB)
                                        {// добавить содержимое SSCC
                                            if (nSpecAdrWait == AppC.F_CNTSSCC)
                                            {
                                                if (!IsDupSSCC(sSSCC))
                                                {
                                                    AddGroupDet(AppC.RC_MANYEAN, (int)NSI.SRCDET.SSCCT, sSSCC);
                                                    AddSSCC2SSCCTable(sSSCC, 0, xCDoc, xCLoad.dtZ.Rows.Count, 0, 1);
                                                }
                                                else
                                                    Srv.ErrorMsg("Уже добавлялся!", sSSCC, false);
                                            }
                                            else
                                            {// контроль
                                                if (TryLoadSSCC(xCDoc.sSSCC, nRet) > 0)
                                                    lDocInf.Text = CurDocInf(xCDoc.xDocP);
                                            }
                                        }
                                    }
                                }
                                break;
                            case AppC.F_SETADRZONE:
                                // функция фиксации адреса
                                if (xSm.xAdrForSpec != null)
                                {
                                    xSm.xAdrFix1 = xSm.xAdrForSpec;
                                    lDocInf.Text = CurDocInf(xCDoc.xDocP);
                                }
                                break;
                            case AppC.F_CELLINF:
                                if (xSm.xAdrForSpec != null)
                                {
                                    xCDoc.xOper.SetOperSrc(xSm.xAdrForSpec, xCDoc.xDocP.nTypD, true);
                                    if ((xCDoc.xDocP.nTypD == AppC.TYPD_OPR) ||
                                        (e.KeyValue == W32.VK_ENTER))
                                    {
                                        sPar = "TXT";
                                        bCloseWait = false;
                                    }
                                    else
                                    {
                                        sPar = "ROW";
                                    }
                                    nRet = ConvertAdr2Lst(xSm.xAdrForSpec, AppC.COM_CELLI, sPar, true, NSI.SRCDET.FROMADR_BUTTON, ref ehCurrFuncW4);
                                }
                                break;
                            case AppC.F_CLRCELL:
                                if (xSm.xAdrForSpec != null)
                                {
                                    xSE = new ServerExchange(this);
                                    xSE.FullCOM2Srv = String.Format("COM={0};KSK={1};MAC={2};KP={3};PAR=(KSK={1},ADRCELL={4});",
                                        AppC.COM_CCELL,
                                        xSm.nSklad,
                                        xSm.MACAdr,
                                        xSm.sUser,
                                        xSm.xAdrForSpec.Addr
                                        );
                                    sErr = xSE.ExchgSrv(AppC.COM_CCELL, "", "", null, null, ref nRet);
                                    if (xSE.ServerRet == AppC.RC_OK)
                                    {
                                        Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                                        Srv.ErrorMsg(xSm.xAdrForSpec.AddrShow, "Очищено...", false);
                                    }
                                    else
                                    {
                                        Srv.ErrorMsg(sErr, "Ошибка!", true);
                                        bCloseWait = false;
                                    }
                                }
                                break;
                            case AppC.F_REFILL:
                                if (xSm.xAdrForSpec != null)
                                {
                                    xSE = new ServerExchange(this);
                                    xSE.FullCOM2Srv = String.Format("COM={0};KSK={1};MAC={2};KP={3};PAR=(KSK={1},ADRCELL={4});",
                                        AppC.COM_REFILL,
                                        xSm.nSklad,
                                        xSm.MACAdr,
                                        xSm.sUser,
                                        xSm.xAdrForSpec.Addr
                                        );
                                    sErr = xSE.ExchgSrv(AppC.COM_REFILL, "", "", null, null, ref nRet);
                                    if (xSE.ServerRet == AppC.RC_OK)
                                    {
                                        Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                                        Srv.ErrorMsg(xSm.xAdrForSpec.AddrShow, "Отправлено...", false);
                                    }
                                    else
                                    {
                                        Srv.ErrorMsg(sErr, "Ошибка!", true);
                                        bCloseWait = false;
                                    }
                                }
                                break;



                            case AppC.F_SIMSCAN:
                                if (tbPanP1G.Text.Length > 0)
                                {
                                    bSimulScan = true;
                                    sSimulScan = tbPanP1G.Text;
                                    xSimScan = new BarcodeScannerEventArgs(BCId.Code128, sSimulScan);
                                    //if (this.InvokeRequired)
                                    //    ;
                                    //ThreadPool.QueueUserWorkItem(CallScanWithPause, this);
                                    //OnScan(this, eScan);
                                }
                                break;


                        }
                    }
                    else
                    {
                        switch (e.KeyValue)
                        {
                            case W32.VK_ESC:
                                bCloseWait = true;
                                break;
                            default:
                                if (nSpecAdrWait == AppC.F_SIMSCAN)
                                    bKeyHandled = false;
                                else
                                    bKeyHandled = true;
                                break;
                        }
                    }

                    if (bCloseWait)
                    {
                        bKeyHandled = true;
                        xFPan.InfoHeightUp(false, 2);
                        xFPan.IFaceReset(false);
                        xFPan.HideP( 
                            (tcMain.SelectedIndex == PG_DOC) ? dgDoc : 
                            (tcMain.SelectedIndex == PG_SCAN) ? dgDet : dgSSCC );
                        // дальше клавиши не обрабатываю
                        ehCurrFunc -= Keys4FixAddr;
                        nSpecAdrWait = 0;
                        xSm.xAdrForSpec = null;
                        ShowOperState(xCDoc.xOper);
                        //Application.DoEvents();
                        if (xCLoad != null)
                        {
                            xCLoad.dtZ = null;
                            xCLoad.xLastSE = null;
                        }
                        //Back2Main();
                        if (bSimulScan)
                        {
                            OnScan(this, xSimScan);
                        }
                    }
                }
            }
            else
            {
            }

            return (bKeyHandled);
        }

        static BarcodeScannerEventArgs 
            xSimScan;
        static string 
            sSimulScan = "";
        static void CallScanWithPause(Object xStateInfo)
        {
            MainF x = (MainF)xStateInfo;
            Thread.Sleep(1000 * 5);
            MessageBox.Show("11111");
            //string ss = x.xFPan.RegInf;

            BarcodeScannerEventArgs eScan = new BarcodeScannerEventArgs(BCId.Code128, MainF.sSimulScan);
            
            x.OnScan(x, eScan);
        }


        /// обработка глобальных функций (все вкладки)
        private bool ProceedFunc(int nFunc, KeyEventArgs e, object sender)
        {
            bool 
                ret = false;
            string
                //sH,
                sMsg;
            DialogResult 
                xDRslt;

            if (bEditMode == false)
            {// функции только для режима просмотра
                switch (nFunc)     // для всех вкладок
                {
                    case AppC.F_MENU:
                        CreateMMenu();              // главное меню
                        ret = true;
                        break;
                    case AppC.F_LOAD_DOC:           // загрузка документов
                        LoadDocFromServer(AppC.F_INITREG, e, ref ehCurrFunc);
                        ret = true;
                        break;
                    case AppC.F_UPLD_DOC:           // выгрузка документов
                        UploadDocs2Server(AppC.F_INITREG, e, ref ehCurrFunc);
                        ret = true;
                        break;
                    case AppC.F_VES_CONF:
                        sMsg = "Подтверждение по ENTER\n";
                        if (AppPars.bVesNeedConfirm == true)
                            sMsg += "ВЫКЛючено";
                        else
                            sMsg += "ВКЛючено";

                        AppPars.bVesNeedConfirm = !AppPars.bVesNeedConfirm;
                        MessageBox.Show(sMsg);
                        if (tcMain.SelectedIndex == PG_SCAN)
                            ShowRegVvod();
                        ret = true;
                        break;
                    case AppC.F_CONFSCAN:
                        sMsg = "Подтверждение скан - ";
                        if (xPars.ConfScan == true)
                            sMsg += "ВЫКЛючено";
                        else
                            sMsg += "ВКЛючено";

                        xPars.ConfScan = !xPars.ConfScan;
                        if (xCDoc != null)
                            xCDoc.bConfScan = (ConfScanOrNot(xCDoc.drCurRow, xPars.ConfScan)>0)?true:false;
                        MessageBox.Show(sMsg);
                        if (tcMain.SelectedIndex == PG_SCAN)
                            ShowRegVvod();
                        ret = true;
                        break;
                    case AppC.F_STARTQ1ST:
                        xPars.aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon = !xPars.aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon;
                        sMsg = "Сначала - ";
                        if (!xPars.aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon)
                            sMsg += "ящики";
                        else
                            sMsg += "поддоны";
                        MessageBox.Show(sMsg, "Смена параметра");
                        if (tcMain.SelectedIndex == PG_SCAN)
                            ShowRegVvod();
                        ret = true;
                        break;
                    case AppC.F_SHLYUZ:
                        xDLLPars = AppC.FX_PRPSK;
                        xDRslt = CallDllForm(sExeDir + "SGPF-Shlyuz.dll", true);
                        switch (tcMain.SelectedIndex)
                        {
                            case PG_DOC:
                                dgDoc.Focus();
                                break;
                            case PG_SCAN:
                                dgDet.Focus();
                                break;
                        }
                        ret = true;
                        break;
                    case AppC.F_SETADRZONE:           // установка фиксированного адреса
                        if (xPars.UseFixAddr)
                        {
                            if (tcMain.SelectedIndex == PG_SCAN)
                            {
                                if (xSm.xAdrFix1 != null)
                                {
                                    Srv.ErrorMsg("Фиксированный адрес сброшен...", xSm.xAdrFix1.AddrShow, true);
                                    xSm.xAdrFix1 = null;
                                    lDocInf.Text = CurDocInf(xCDoc.xDocP);
                                }
                                else
                                {
                                    //nSpecAdrWait = 1;
                                    //sMsg = "Фиксация адреса";
                                    //sH = "Отсканируйте адрес";
                                    //xFPan.ShowP(6, 28, sMsg, sH);
                                    //// дальше клавиши обработаю сам
                                    //ehCurrFunc += new Srv.CurrFuncKeyHandler(Keys4FixAddr);
                                    WaitScan4Func(AppC.F_SETADRZONE, "Фиксация адреса", "Отсканируйте адрес");
                                }
                            }
                            else
                                Srv.ErrorMsg("На вкладке 'Ввод'!");
                        }
                        ret = true;
                        break;
                    case AppC.F_MARKWMS:   // серверу - сведения по SSCC
                        //xDRslt = CallDllForm(sExeDir + "SGPF-Mark.dll", true);
                        xDRslt = CallDllForm(sExeDir + "SGPF-Mark.dll", true, new object[] { this, false });
                        if ((xDRslt == DialogResult.Abort) && (xSm.RegApp == AppC.REG_MARK))
                            this.Close();
                        break;
                    case AppC.F_REFILL:           // пополнение адреса
                        WaitScan4Func(AppC.F_REFILL, "Пополнение адреса", "Отсканируйте адрес");
                        ret = true;
                        break;
                    case AppC.F_ZZKZ1:              // загрузка заказа
                        LoadOneZkz();
                        ret = true;
                        break;
                    case AppC.F_SHOWPIC:            // схема укладки заказа
                        try
                        {
                            string s = (string)xCDoc.drCurRow["PICTURE"];
                            xPicShow.ShowInfo(s, ref ehCurrFunc, Srv.PicShow.PICSRCTYPE.BASE64, null, null);
                        }
                        catch
                        {
                            Srv.ErrorMsg("Отсутствует изображение!");
                        }
                        ret = true;
                        break;
                    case AppC.F_SSCCSH:             // отображение содержимого
                        AppPars.ShowSSCC = !AppPars.ShowSSCC;
                        sMsg = "Вывод содержимого\nSSCC на экран:\n";
                        if (AppPars.ShowSSCC)
                            sMsg += "ВКЛЮЧЕНО";
                        else
                            sMsg += "ВЫКЛЮЧЕНО";
                        MessageBox.Show(sMsg, "Смена параметра");
                        ret = true;
                        break;
                    case AppC.F_CHKSSCC:
                        // Загрузка SSCC в заявку
                        ret = true;
                        bool bClearCHK = false;
                        try
                        {
                            if ((int)xCDoc.drCurRow["CHKSSCC"] == 1)
                                bClearCHK = true;
                        }
                        catch { }
                        if (bClearCHK)
                        {
                            xCDoc.drCurRow["CHKSSCC"] = 0;
                            lDocInf.Text = CurDocInf(xCDoc.xDocP);
                            Srv.ErrorMsg("НЕТ контроля SSCC!");
                        }
                        else
                            WaitScan4Func(AppC.F_CHKSSCC, "Контроль SSCC", "Отсканируйте SSCC");
                        break;
                    case AppC.F_SIMSCAN:
                        if ((tcMain.SelectedIndex == PG_SCAN)
                            || (tcMain.SelectedIndex == PG_SSCC))
                        {
                            WaitScan4Func(AppC.F_SIMSCAN, "Строка сканирования", "");
                        }
                        ret = true;
                        break;
                }
            }


            switch (nFunc)     // для всех вкладок
            {
                case AppC.F_HELP:
                    // вывод панели c окном помощи
                    xHelpS.ShowInfo(xFuncs.GetFHelp(), ref ehCurrFunc);
                    ret = true;
                    break;
                case AppC.F_LASTHELP:
                    xHelpS.ShowInfo(xInf, ref ehCurrFunc);
                    ret = true;
                    break;
                case AppC.F_ADR2CNT:
                    int x = ConvertAdr2Lst(xCDoc.xOper.xAdrSrc, AppC.COM_ADR2CNT, "ROW", true, NSI.SRCDET.FROMADR);
                    ret = true;
                    break;
                case AppC.F_CNTSSCC:
                    WhatSSCCContent();
                    ret = true;
                    break;
                case AppC.F_VIEW_DOC:
                // просмотр детальных строк
                    if (tcMain.SelectedIndex == PG_DOC)
                        tcMain.SelectedIndex = PG_SCAN;
                    else
                        tcMain.SelectedIndex = PG_DOC;
                    ret = true;
                    break;
                case AppC.F_PREVPAGE:
                    // предыдущая страница
                    if (tcMain.SelectedIndex == 0)
                        tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
                    else
                        tcMain.SelectedIndex--;
                    ret = true;
                    break;
                case AppC.F_NEXTPAGE:
                    // следующая страница
                    if (tcMain.SelectedIndex == (tcMain.TabPages.Count - 1))
                        tcMain.SelectedIndex = 0;
                    else
                        tcMain.SelectedIndex++;
                    ret = true;
                    break;
                case AppC.F_LOGOFF:
                    ret = true;
                    break;
                case AppC.F_QUIT:
                    ExitApp();
                    break;
                //case AppC.F_PRNBLK:
                //    xDLLPars = AppC.FX_PRPSK;
                //    xDRslt = CallDllForm(sExeDir + "SGPF-Prn.dll", true);
                //    switch (tcMain.SelectedIndex)
                //    {
                //        case PG_DOC:
                //            dgDoc.Focus();
                //            break;
                //        case PG_SCAN:
                //            dgDet.Focus();
                //            break;
                //    }
                //    ret = true;
                //    break;
                case AppC.F_PRNBLK:
                case AppC.F_GENFUNC:
                    CallFrmPars();
                    ret = true;
                    break;
            }
            e.Handled |= ret;
            return (ret);
        }

        private void ExitApp()
        {
            DialogResult dr = MessageBox.Show(" Выход ?  (Enter)\nпродолжить работу (ESC)",
                "Завершение работы", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (dr == DialogResult.OK)
                this.Close();
        }

        private void SetEditMode(bool bEdit)
        {
            bEditMode = bEdit;
#if SYMBOL
            if ((xBCScanner.nTermType == TERM_TYPE.SYMBOL) && (xBCScanner.nKeys == 48))
            {
                if (AppPars.bArrowsWithShift == true)
                {
                    ((ScannerAll.Symbol.SymbolBarcodeScanner)xBCScanner).bALP = !bEdit;
                }
            }
#endif
        }

        // Пункты главного меню
        private MenuItem 
            miServ = new MenuItem(),
            miExch = new MenuItem(),
            miNsi = new MenuItem();

        private void Create1MenuItem(MenuItem xMI, int nN, string sMName, EventHandler eH)
        {
                xMI.MenuItems.Add(new MenuItem());
                xMI.MenuItems[nN].Click += new EventHandler(eH);
                xMI.MenuItems[nN].Text = String.Format("&{0} {1}", nN + 1, sMName);
        }

        private void CreateMMenu()
        {
            int
                nSrv = 0,
                nYForClick = ((xBCScanner.nTermType == ScannerAll.TERM_TYPE.DOLPH7850) ||
                               (xBCScanner.nTermType == ScannerAll.TERM_TYPE.DOLPH9950)) ? 315 : 5;

            if (this.mmSaved == null)
            {
                miExch.Text = "&Документы";
                miNsi.Text  = "&НСИ";
                miServ.Text = "&Сервис";

                // меню Документы
                Create1MenuItem(miExch, nSrv++, "Сохранить",        MMenuClick_SaveCur);
                Create1MenuItem(miExch, nSrv++, "Восстановить",     MMenuClick_RestDat);
                Create1MenuItem(miExch, nSrv++, "Загрузка",         MMenuClick_Load);
                Create1MenuItem(miExch, nSrv++, "Выгрузка",         MMenuClick_WriteSock);
                Create1MenuItem(miExch, nSrv++, "НСИ",              MMenuClick_LoadNSI);
                Create1MenuItem(miExch, nSrv++, "Корректировка",    MMenuClick_Corr);
                Create1MenuItem(miExch, nSrv++, "Параметры",        MMenuClick_SessPars);

                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems[nSrv++].Text = "-";

                Create1MenuItem(miExch, nSrv++, "Выход",            MMenuClick_Exit);

                // меню НСИ
                nSrv = 0;
                Create1MenuItem(miNsi, nSrv++, "Загрузка НСИ", MMenuClick_LoadNSI);

                // меню сервисных функций
                nSrv = 0;
                if (xSm.urCur > Smena.USERRIGHTS.USER_KLAD)
                {
                    Create1MenuItem(miServ, nSrv++, "Установка времени",    MMenuClick_SetTime);
                }
                Create1MenuItem(miServ, nSrv++, "Подключение к сети",       MMenuClick_Reconnect);
                Create1MenuItem(miServ, nSrv++, "Сканирование",             MMenuClick_DoScan);
                if (xSm.urCur > Smena.USERRIGHTS.USER_KLAD)
                {
                    Create1MenuItem(miServ, nSrv++, "Настройки клавиатуры", MMenuClick_KeyMap);
                }
                Create1MenuItem(miServ, nSrv++, "Версия",                   MMenuClick_AppVer);
                Create1MenuItem(miServ, nSrv++, "Очистка ячейки",           MMenuClick_ClearCell);

                // Create a MainMenu and assign MenuItem objects.
                MainMenu mainMenu1 = new MainMenu();
                mainMenu1.MenuItems.Add(miExch);
                mainMenu1.MenuItems.Add(miNsi);
                mainMenu1.MenuItems.Add(miServ);

                // Bind the MainMenu to Form1.
                this.mmSaved = mainMenu1;
            }
            this.SuspendLayout();
            if (this.Menu == null)
            {
                this.Menu = this.mmSaved;
                W32.SimulMouseClick(5, nYForClick, this);
            }
            else
                this.Menu = null;
            this.ResumeLayout();
        }



        // "Сохранить"
        private void MMenuClick_SaveCur(object sender, EventArgs e)
        {// сохранить текущие данные
            Cursor crsOld = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                SaveCurData(false);
                MessageBox.Show("Выполнено...", "Сохранение");
            }
            finally
            {
                Cursor.Current = crsOld;
                CreateMMenu();
            }
        }

        private void Go1stLast(DataGrid dg, int nWhatPage)
        {
            CurrencyManager cmDoc = (CurrencyManager)BindingContext[dg.DataSource];
            if (cmDoc.Count > 0)
            {
                cmDoc.Position = (nWhatPage == AppC.F_GOFIRST) ? 0 : cmDoc.Count - 1;
                dg.Refresh();
            }
        }



        // "Восстановить"
        private void MMenuClick_RestDat(object sender, EventArgs e)
        {
            DialogResult drQ = MessageBox.Show("Восстановить данные(Enter)?\n(ESC) - отмена", "Восстановление",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (drQ == DialogResult.OK)
            {
                if (tcMain.SelectedIndex != PG_DOC)
                    tcMain.SelectedIndex = PG_DOC;
                Cursor crsOld = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                if (AppC.RC_OK == xNSI.DSRestore(xPars.sDataPath, Smena.DateDef, xPars.Days2Save, false))
                {
                    Go1stLast(dgDoc, AppC.F_GOFIRST);
                    MessageBox.Show("Выполнено...", "Восстановление");
                }
                else
                {
                    //dgDoc.Refresh();
                    RestShowDoc(false);
                    Srv.ErrorMsg("Нет данных восстановления!");
                }
                Cursor.Current = crsOld;
            }
            CreateMMenu();
        }

        // "Загрузка"
        private void MMenuClick_Load(object sender, EventArgs e)
        {
            LoadDocFromServer(AppC.F_INITREG, new KeyEventArgs(Keys.Enter), ref ehCurrFunc);
            StatAllDoc();
            CreateMMenu();
        }

        // "Выгрузка"
        private void MMenuClick_WriteSock(object sender, EventArgs e)
        {
            UploadDocs2Server(AppC.F_INITREG, new KeyEventArgs(Keys.Enter), ref ehCurrFunc);
            StatAllDoc();
            CreateMMenu();
        }

        // "НСИ"
        private void MMenuClick_LoadNSI(object sender, EventArgs e)
        {
            CheckNSIState(true);
            CreateMMenu();
        }

        // "Корректировка"
        private void MMenuClick_Corr(object sender, EventArgs e)
        {
            CreateMMenu();
        }

        // "Параметры"
        private void MMenuClick_SessPars(object sender, EventArgs e)
        {
            xDLLPars = AppC.AVT_PARS;
            DialogResult xDRslt = CallDllForm(sExeDir + "SGPF-Avtor.dll", true);
            CreateMMenu();
        }

        private void MMenuClick_Exit(object sender, EventArgs e)
        {
            CreateMMenu();
            ExitApp();
        }


        private void MMenuClick_SetTime(object sender, EventArgs e)
        {
            string sHead = String.Format("Текущее: {0}", DateTime.Now.TimeOfDay.ToString()),
                sAfter = "Ошибка синхронизации";

            Cursor.Current = Cursors.WaitCursor;
            if (TimeSync.Sync(xPars.NTPSrv, 123, 10000, 3600))
                sAfter = String.Format("Новое время: {0}", DateTime.Now.TimeOfDay.ToString());

            if (xBCScanner != null)
                xBCScanner.Dispose();
            xBCScanner = null;
            Thread.Sleep(1500);
            xBCScanner = BarcodeScannerFacade.GetBarcodeScanner(this);
            xBCScanner.BarcodeScan += ehScan;

            sAfter += "\nСканер перезапущен...";
            MessageBox.Show(sAfter, sHead);
            Cursor.Current = Cursors.Default;

            CreateMMenu();
        }

        private void MMenuClick_Reconnect(object sender, EventArgs e)
        {
            ServerExchange xSE = new ServerExchange(this);
            //xBCScanner.WiFi.IsEnabled = true;
            xBCScanner.WiFi.ShowWiFi(pnLoadDocG, true);

            xFPan.ShowP(6, 50, "Переподключение к сети", "Wi-Fi");
            if (!xSE.TestConn(true, xBCScanner, xFPan))
                Srv.ErrorMsg("Не удалось подключиться");
            else
            {
                //MessageBox.Show("Завершено...", "Инициализация Wi-Fi");
                Thread.Sleep(4000);
                string sI = xBCScanner.WiFi.WiFiInfo();
                string sFM = String.Format("Есть подключение: {0}\nMAC: {1}", sI, xSm.MACAdr);
                MessageBox.Show(sFM, "Инициализация Wi-Fi");
            }
            xFPan.HideP();
            CreateMMenu();
        }

        // Выгрузить настройки клавиатуры
        private void MMenuClick_KeyMap(object sender, EventArgs e)
        {
            if (xFuncs.SaveKMap() == AppC.RC_OK)
            {
                Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                MessageBox.Show("Настройки сохранены", "Сохранение");
            }
            CreateMMenu();
        }

        // Выполнить пример сканирования
        private void MMenuClick_DoScan(object sender, EventArgs e)
        {
            WaitScan4Func(AppC.F_GENSCAN, "Выполните сканирование", "");
            CreateMMenu();
        }

        //private string VerApp(Version version)
        //{
        //    var buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
        //    return( String.Format("{0}.{1} {2}", version.Major, version.Minor, buildDate.ToString("dd.MM.yyyy") ) );
        //}

        // Версия софтины
        private void MMenuClick_AppVer(object sender, EventArgs e)
        {
            //string 
            //    sAppVer = VerApp(Assembly.GetExecutingAssembly().GetName().Version);
            //string[]
            //    sD = sAppVer.Split(new char[]{' '});

            object[]
                xV = Srv.AppVerDT();

            Srv.ErrorMsg(String.Format("Версия ПО - {0}\nот {1}\nT = {2}",
                xV[1], ((DateTime)xV[2]).ToString("dd.MM.yyyy"), System.Net.Dns.GetHostName()), 
                "Информация", true);

            CreateMMenu();
        }

        // Очистка
        private void MMenuClick_ClearCell(object sender, EventArgs e)
        {
            WaitScan4Func(AppC.F_CLRCELL, "Очистка адреса", "Отсканируйте адрес");
            CreateMMenu();
        }

        private void FiltForDocs(bool bHide, NSI.TableDef di)
        {
            string s;
            if (bHide == true)
            {
                s = String.Format("(SOURCE<>{0})", NSI.DOCSRC_UPLD);
                tDocCtrlState.Text = "Ф";
            }
            else
            {
                s = "";
                tDocCtrlState.Text = "";
            }
            di.sTFilt = s;
            di.dt.DefaultView.RowFilter = di.sTFilt;

        }


        //private void ToPageHeader(TabPage pgT)
        //{
        //    Control cTab0 = ServClass.GetPageControl(pgT, 0);
        //    cTab0.Focus();

        //    W32.keybd_event(W32.VK_SHIFT, W32.VK_SHIFT,  W32.KEYEVENTF_SILENT, 0);
        //    W32.keybd_event(W32.VK_TAB, W32.VK_TAB,  W32.KEYEVENTF_SILENT, 0);
        //    W32.keybd_event(W32.VK_TAB, W32.VK_TAB, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
        //    W32.keybd_event(W32.VK_SHIFT, W32.VK_SHIFT, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
        //}

        //delegate void ProcessSocketCommandHandler(object data);
        //ProcessSocketCommandHandler _processCommand;


        // получили сообщение
        //void cs_MessageFromServerRecived(object sender)
        //{
        //    object[] args = { sender };
        //    Invoke(_processCommand, args);
        //}

        //static int nServCall = 0;

        //private void ProcessSocketCommand(object data)
        //{
        //    string s1;
        //    byte[] bbuf = new byte[256];
        //    string sTypDoc = (string)data;
        //    MessageBox.Show("Что-то хотят!\r\n" + sTypDoc);
        //    if (++nServCall > 5)
        //        nServCall = 0;
        //    switch(nServCall){
        //        case 0:
        //            s1 = "Слышу!";
        //            break;
        //        case 1:
        //            s1 = "Ну чего?!";
        //            break;
        //        case 2:
        //            s1 = "Некогда мне!";
        //            break;
        //        case 3:
        //        case 4:
        //        case 5:
        //            s1 = "Отъе...сь !!!";
        //            break;
        //        default:
        //            s1 = "не надоело?...";
        //            break;
        //    }
        //    m_ssExchg.Connect();
        //    System.IO.Stream stm = m_ssExchg.GetStream();
        //    bbuf = Encoding.UTF8.GetBytes(s1);
        //    stm.Write(bbuf, 0, bbuf.Length);
        //    m_ssExchg.Disconnect();

        //}


        // подсветка заголовка на вкладке "Ввод"
        private void dgDet_LostFocus(object sender, EventArgs e)
        {
            //if ((nCurVvodState == VV_STATE_SHOW) && (tcMain.SelectedIndex == PG_SCAN))
            //    ToPageHeader(tpScan);
        }

        private void tFiction_GotFocus(object sender, EventArgs e)
        {
            int j, k;
            j = 7;
            k = 99;
            j = k - 5;
            k = j + 56;
        }









        // подготовка DataSet для обмена с сервером в диалоге(GENFUNC)
        public DataSet DocDataSet4GF(DataRow dr, CurUpLoad xU, int nDet4Upload)
        {
            DataSet ds1Rec = null;
            if (dr != null)
            {
                DataTable dtMastNew = xNSI.DT[NSI.BD_DOCOUT].dt.Clone();
                DataTable dtDetNew = xNSI.DT[NSI.BD_DOUTD].dt.Clone();
                DataTable dtBNew = xNSI.DT[NSI.BD_SPMC].dt.Clone();
                DataRow[] aDR, childRows;


                dtMastNew.LoadDataRow(dr.ItemArray, true);
                ds1Rec = new DataSet("dsMOne");
                ds1Rec.Tables.Add(dtMastNew);

                if (nDet4Upload >= 1)
                {
                    if (nDet4Upload == 1)
                    {

                        childRows = dr.GetChildRows(NSI.REL2TTN);
                        foreach (DataRow chRow in childRows)
                        {
                            dtDetNew.LoadDataRow(chRow.ItemArray, true);
                            aDR = chRow.GetChildRows(NSI.REL2BRK);
                            foreach (DataRow bR in aDR)
                                dtBNew.LoadDataRow(bR.ItemArray, true);
                        }
                    }
                    else if (nDet4Upload == 2)
                    {
                        if (xCDoc.drCurRow != null)
                        {
                            if (Srv.ExchangeContext.dr4Prn != null)
                            {
                                dtDetNew.LoadDataRow(Srv.ExchangeContext.dr4Prn.ItemArray, true);
                                aDR = Srv.ExchangeContext.dr4Prn.GetChildRows(NSI.REL2BRK);
                                foreach (DataRow bR in aDR)
                                    dtBNew.LoadDataRow(bR.ItemArray, true);

                            }
                            else
                            {
                                Srv.ErrorMsg("Нет строки!");
                                return (null);
                            }
                        }
                        else
                        {
                            Srv.ErrorMsg("Нет документа!");
                            return (null);
                        }
                    }
                    ds1Rec.Tables.Add(dtDetNew);
                    ds1Rec.Tables.Add(dtBNew);
                }

            }
            return (ds1Rec);
        }

        private int nSound = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            //int
            //    nRet = AppC.RC_OK;
            DataSet dsD = DocDataSet4GF(xCDoc.drCurRow, xCUpLoad, Srv.ExchangeContext.FlagDetailRows);
            //LoadFromSrv dgRead = new LoadFromSrv(LoadParList4GF);
            //LoadFromSrv dgRead = null;

            //string sPar = String.Format("PAR=(FUNC={0},BLANK={1},PRN={2},PRNMOB={3})",
            //            AppC.COM_PRNBLK, "0301000005", "", "");
                    
            //xCUpLoad = new CurUpLoad();
            //xCUpLoad.aAddDat = null;

            //string sErr = ExchgSrv(AppC.COM_GENFUNC, sPar, "", dgRead, dsD, ref nRet);

        }


        #region NOT_Used_yet

        /*
            // --- для частичной загрузки справочников
            //nnSS = xNSI;
            //thReadNSI = new Thread(new ThreadStart(ReadInThread));
            //thReadNSI.Start();

        private Thread thReadNSI;
        private void Form1_Activated(object sender, EventArgs e)
        {
            //fAv.ShowDialog();
            if (xNSI.DT[NSI.NS_MC].nState == NSI.DT_STATE_INIT)
            {// справочник матценностей еще не грузили
                nnSS = xNSI;
                thReadNSI = new Thread(new ThreadStart(ReadInThread));
                thReadNSI.Start();
            }
        }
         * 
         private static NSI nnSS;
        private static void ReadInThread()
        {
            nnSS.LoadLocNSI(new int[] {}, 0);
        }
         * 
         * 
         * 
         * 
        //private RUN xRUN;
        private Expr xExp;
        // подготовка для работы с интерпретатором
        private int IT_Prep(string sF)
        {
            int ret = 0;
            //string sIT_Is = IT_ReadProg(sF);
            //if (sIT_Is != "")
            //{
            //    xRUN = new RUN();
            //    Expr xExp = new Expr(xRUN);

            //    xExp.Run(sIT_Is);
            //    xRUN.exec(xExp.GetAction());
            //}
            return (ret);
        }


        // чтение исходника
        private string IT_ReadProg(string sFile)
        {
            string ret = "";

            if (System.IO.File.Exists(sFile))
            {
                try
                {
                    using (System.IO.StreamReader sr = System.IO.File.OpenText(sFile))
                    {
                        ret = sr.ReadToEnd();


                        String input;
                        int i = 0;
                        while ((input = sr.ReadLine()) != null)
                        {
                            i++;
                        }
                        sr.Close();
                    }
                }
                catch { }
            }

            return (ret);
        }

        // запуск интерпретатора
        private void btIT_Run_Click(object sender, EventArgs e)
        {

            //int ret = 0;
            //string sIT_Is = IT_ReadProg(tIT_Path.Text);
            string sIT_Is = (string)xNSI.DT[NSI.BD_PASPORT].dt.Rows[0]["MD"];
            if (sIT_Is != "")
            {
                //xRUN = new RUN();
                Expr xExp = new Expr();

                if (chbIT_Run.Checked == true)
                {
                    Action a = xExp.Parse(sIT_Is);

                    xExp.run.ExecFunc("ControlDoc", new object[] { xNSI.DT[NSI.BD_DOCOUT].dt, xNSI.DT[NSI.BD_DIND].dt, xNSI.DT[NSI.BD_DOUTD].dt, 0 }, a);
                    //xExp.Run.Exec(a);
                    //Action xAct = xExp.GetAction();
                    //xAct.SetE("fMain", this);
                    //xAct.SetE("dfMain", this);
                    //xRUN.exec(xAct);
                }
            }
        }

         * * 
        // транслированный код контроля документов
        private Expr xDocControl = null;
        private Action actDocControl = null;
        private string sDocCtrlMsg = "";

        private void LoadInterCode(bool bTranslateMD)
        {
            int nRet = 0;
            string sIT_Is = "";

            if (bTranslateMD != true)
            {
                nRet = LoadAllNSISrv(NSI.I_PASPORT, null, false);
            }
            if (nRet == 0)
            {
                try
                {
                    if (xNSI.DT[NSI.BD_PASPORT].nState == NSI.DT_STATE_READ)
                    {
                        if (xNSI.DT[NSI.BD_PASPORT].dt.Rows.Count > 0)
                        {
                            sIT_Is = (string)xNSI.DT[NSI.BD_PASPORT].dt.Rows[0]["MD"];
                        }
                    }
                }
                catch
                {
                    if (bTranslateMD != true)
                        Srv.ErrorMsg("Ошибка загрузки контроля!");
                }

                if (sIT_Is != "")
                {
                    xDocControl = new Expr();
                    try
                    {
                        actDocControl = xDocControl.Parse(sIT_Is);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка трансляции!");
                    }
                }
            }
        }

        // запуск контроля
        private void MMenuClick_RunControl(object sender, EventArgs e)
        {
            if (xCDoc.drCurRow != null)
            {
                RunDocControl(xCDoc.drCurRow);
            }
            CreateMMenu();
        }



        // просмотр результатов контроля
        private void MMenuClick_SeeControl(object sender, EventArgs e)
        {
            MessageBox.Show(sDocCtrlMsg, "Результаты контроля");
            CreateMMenu();
        }


        private int RunDocControl(DataRow dr)
        {
            int t1,
                nRet = 0;
            TimeSpan tsDiff;

            if (xDocControl != null)
            {
                //xDocControl.run.ExecFunc("ControlDoc", new object[] { xNSI.DT[NSI.BD_DOCOUT].dt, 
                //    xNSI.DT[NSI.BD_DIND].dt, xNSI.DT[NSI.BD_DOUTD].dt, 0 }, actDocControl);
                try
                {
                    List<string> lstStr = new List<string>();

                    //DataRow[] childRowsZVK = dr.GetChildRows(NSI.REL2ZVK);
                    //DataRow[] childRowsTTN = dr.GetChildRows(NSI.REL2TTN);

                    string sRf = String.Format("(SYSN={0})", dr["SYSN"]),
                        sSort = "KRKMC,EMK DESC";

                    // вся продукция из заявки по документу
                    DataView dvZ = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, sSort, 
                        DataViewRowState.CurrentRows);

                    // вся продукция из ТТН по документу
                    DataView dvT = new DataView(xNSI.DT[NSI.BD_DIND].dt, sRf, sSort, 
                        DataViewRowState.CurrentRows);



                    t1 = Environment.TickCount;

                    //object xRet = xDocControl.run.ExecFunc(AppC.DOC_CONTROL, 
                    //    new object[] { dr, childRowsZVK, childRowsTTN, lstStr }, actDocControl);

                    object xRet = xDocControl.run.ExecFunc(AppC.DOC_CONTROL,
                        new object[] { dr, dvZ, dvT, lstStr, this}, actDocControl);

                    tsDiff = new TimeSpan(0, 0, 0, 0, Environment.TickCount - t1);

                    nRet = (int)xRet;

                    lstStr.Add(String.Format("Выполнение - {0}", tsDiff.TotalSeconds));

                    if (nRet != 0)
                    {
                        tDocCtrlState.BackColor = Color.Tomato;
                        ShowInf(lstStr);

                        //sDocCtrlMsg = "";
                        //int nStrInMB = System.Math.Min(10, lstStr.Count);
                        //int nOst = (lstStr.Count > 10) ? lstStr.Count - 10 : 0;
                        //for (int i = 0; i < nStrInMB; i++)
                        //    sDocCtrlMsg += lstStr[i] + "\r\n";

                        //if (nOst > 0)
                        //    sDocCtrlMsg += nOst.ToString() + " сообщений еще...";
                    }
                    else
                    {
                        tDocCtrlState.BackColor = Color.Gainsboro;
                        ShowInf(lstStr);
                    }

                }
                catch (Exception ex) {
                MessageBox.Show(ex.Message);
                }
            }
            return (nRet);
        }
         * 
* 
 * 
 */

        #endregion


        // транслированный код контроля документов
        //private Expr xDocControl = null;
        //private Action actDocControl = null;


        //public List<ExprList> xExpDic;

        //public void LoadInterCode(bool bTranslateMD)
        //{
        //    string sCurBlk;
        //    Expr xEx;
        //    Action xAct;

        //    if (xNSI.DT[NSI.BD_PASPORT].nState == NSI.DT_STATE_READ)
        //    {
        //        xExpDic = new Dictionary<string, Srv.ExprAct>();
        //        foreach (DataRow dr in xNSI.DT[NSI.BD_PASPORT].dt.Rows)
        //        {
        //            sCurBlk = (string)dr["KD"];
        //            xEx = new Expr();
        //            try
        //            {
        //                if (bTranslateMD)
        //                    xAct = xEx.Parse((string)dr["MD"]);
        //                else
        //                    xAct = xEx.Parse((string)dr["MD"]);
        //                xExpDic.Add(sCurBlk, new Srv.ExprAct(xEx, xAct));

        //                //object xRet = xDocControl.run.ExecFunc("NameAdr",
        //                //    new object[] { xSm.nSklad, "0123456789" }, actDocControl);

        //            }
        //            catch
        //            {
        //                Srv.ErrorMsg("Ошибка трансляции! " + sCurBlk);
        //            }
        //        }
        //        //Smena.xDD = xSm.xExpDic;
        //    }

        //}



    }
}