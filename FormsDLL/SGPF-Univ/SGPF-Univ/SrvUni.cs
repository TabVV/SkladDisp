//#define DEB_MODE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using SavuSocket;

using ScannerAll;
using PDA.Service;
using PDA.OS;
using SkladGP;

using FRACT = System.Decimal;

namespace SGPF_Univ
{
    public partial class PrintBlank : Form
    {

        private MainF xMainF = null;
        private NSI xNSI;

        private bool bEditMode = false;
           
        private AppC.EditListC aEd;

        // не обрабатывать введенный символ
        private bool bSkipKey = false;

        private BarcodeScanner.BarcodeScanEventHandler ehOldScan = null;

        // типы динамических контролов для установки параметров 
        internal const string C_COMBO   = "ComboBox";
        internal const string C_CHECK   = "CheckBox";
        internal const string C_LBL     = "Label";
        internal const string C_TEXT    = "TextBox";
        internal const string C_DATE    = "DateBox";
        internal const string C_CPRN    = "ComboPrn";
        internal const string C_MEMI    = "ListBox";

        internal const string C_GDAT    = "GenDat";

        internal const string C_DATE_STD = "DateTimePicker";
        internal const string C_CPRN_STD = "ComboBoxEx";

        private class ComboBoxEx : ComboBox
        {
            // открыт список или нет
            private bool m_Opened = false;

            public bool ListOpened
            {
                get { return m_Opened; }
                set { m_Opened = value; }
            }
        }

        private const string EMPTY_SCPFX = "EMPTY_SCAN_PREFIX";


        [Serializable]
        public class OnePar
        {
            // для создания контрола
            int
                m_Height = 20,          // pixels
                m_HeightLines = 1;      // string lines
            int
                m_CurInd;

            string
                m_ScanPref = EMPTY_SCPFX,
                m_ParType,
                m_ParName,
                m_Capt,
                m_ParDefVal;
            int
                m_Enable = 1;

            object m_xObj;

            public OnePar() { }

            public OnePar(Control c)
            {
                xObj = c;
            }

            //public string ScanPrefix()
            //{
            //    return( EMPTY_SCPFX );
            //}


            // Тип контрола для установки параметра
            public string PARTYPE
            {
                get { return m_ParType; }
                set { m_ParType = value; }
            }

            // Имя Control параметра
            public string PARNAME
            {
                get { return m_ParName; }
                set { m_ParName = value; }
            }

            // Заголовок (Label) параметра
            public string PARCAPT
            {
                get { return m_Capt; }
                set { m_Capt = value; }
            }

            // Высота в строках
            public int HEIGHT
            {
                get { return m_HeightLines; }
                set { m_HeightLines = value; }
            }

            // Доступность для редактирования
            public int ENABLE
            {
                get { return m_Enable; }
                set { m_Enable = value; }
            }

            // Список значений
            public List<string> PARLISTVAL;

            // Индекс значения (по умолчанию) параметра
            public int CURIND
            {
                get { return m_CurInd; }
                set { m_CurInd = value; }
            }

            // Значение (по умолчанию) параметра
            public string CURVAL
            {
                get { return m_ParDefVal; }
                set { m_ParDefVal = value; }
            }

            // Префикс при сканировании
            public string SCANPRFX
            {
                get { return m_ScanPref; }
                set { m_ScanPref = value; }
            }


            public object xObj
            {
                get { return m_xObj; }
                set { m_xObj = value; }
            }

            public int GetH()
            {
                return (m_Height);
            }


        }

        //[Serializable]
        public class Pars4Print
        {
            // Первичный массив параметров (получен из XML)
            public OnePar[] xParsCtrls = null;

            // Доступные для редактирования (отображаемые) параметры
            [XmlIgnore]
            public Dictionary<string, Control> dicEditPars;
            // НеДоступные для редактирования (неотображаемые)параметры
            [XmlIgnore]
            public Dictionary<string, OnePar> dicGenDat;

            public Pars4Print()
            {
                dicEditPars = new Dictionary<string,Control>();
                dicGenDat = new Dictionary<string,OnePar>();
            }
        }



        // начальный режим работы формы
        int nRegFrm = AppC.R_BLANK;

        // список доступных для выбора бланков
        private BindingSource bsBlanks;

        // массив Control с парметрами для печати
        Pars4Print xPPrn, xPBuf;

        // флаг перекодировки клавиши для DateTimePicker
        private bool bMySimulKey = false;

        // содержание справки
        private List<string> 
            lstHelp = new List<string>() { 
                        "F1     - справка",
                        "F2     - выполнить",
                        "CTRL-2 - выполнить",
                        "SPaCe  - раскрыть список",
                        "F3     - раскрыть список",
                        "ENTER  - выбор в списке",
                        "       - переход на следующий",
                        "-><-   - смена значения",
                        "^V     - переход на следующий",
                        "       - переход внутри даты",
                        "ESC    - выход"
                        };

        // обработчик клавиш для текущей функции
        public Srv.CurrFuncKeyHandler 
            ehCurrFunc = null;

        private Srv.HelpShow xHelpS;
        private MainF.ServerExchange xSE;

        // *** для отладки
        string
            sFSer = "fobj.xml",
            sFLoad = "";

        public PrintBlank()
        {
            InitializeComponent();
        }

        // для передачи параметров в форму

        // обработка параметров и подготовка формы
        public bool AfterConstruct(object[] xAPars)
        {
            bool bRet = false;

            xMainF = (MainF)xAPars[0];
            xNSI = xMainF.xNSI;
            // параметр для сервера
            Srv.ExchangeContext.CMD_EXCHG = (string)xAPars[1];

            nRegFrm = (int)xAPars[2];
            bsBlanks = (BindingSource)xAPars[6];
            SetBlankList(bsBlanks);

            Srv.ExchangeContext.sPrinterSTC = (string)xAPars[3];
            Srv.ExchangeContext.sPrinterMOB = (string)xAPars[4];
            Srv.ExchangeContext.dr4Prn = (DataRow)xAPars[5];

            switch (nRegFrm)
            {
                case AppC.R_BLANK:
#if DEB_MODE
            // *** для отладки
            sFSer = sFLoad = "fobj.xml";

            //xNSI.DT[NSI.NS_BLANK].dt.Rows.Clear();
            //DataRow dr = xNSI.DT[NSI.NS_BLANK].dt.NewRow();
            //dr["TD"] = 7;
            //dr["KBL"] = "ABRK01";
            //dr["NAME"] = "Акт забраковки";
            //xNSI.DT[NSI.NS_BLANK].dt.Rows.Add(dr);
            //dr = xNSI.DT[NSI.NS_BLANK].dt.NewRow();
            //dr["TD"] = -1;
            //dr["KBL"] = "SSCC02";
            //dr["NAME"] = "Ярлык";
            //xNSI.DT[NSI.NS_BLANK].dt.Rows.Add(dr);
            // *** для отладки
#endif
                    //bRet = SetBlankList();
                    bRet = true;
                    break;
                case AppC.R_PARS:
                    //object x;
                    //string sXML = (string)xAPars[3];

                    //Srv.ExchangeContext.sHeadLine = "<< Установите параметры >>";
                    //// выбора бланка не будет
                    //this.Controls.Remove(lbFuncs);  
                    //lBlankCmb.Text = Srv.ExchangeContext.sHeadLine;
                    //try
                    //{
                    //    if (Srv.ReadXMLObj(typeof(Pars4Print), out x, sXML) == AppC.RC_OK)
                    //    {
                    //        xPPrn = (Pars4Print)x;
                    //        if (ShowPars(xPPrn) == AppC.RC_OK)
                    //            bRet = true;
                    //        else
                    //            this.DialogResult = DialogResult.Abort;
                    //    }
                    //}
                    //finally
                    //{
                    //    if (sXML.Length > 0)
                    //        File.Delete(sXML);
                    //}
                    bRet = BlankSelected();
                    break;
                default:
                    this.DialogResult = DialogResult.Abort;
                    break;
            }

            if (bRet)
            {
            }
            return (bRet);
        }


        // операции после выбора бланка
        private bool BlankSelected()
        {
            bool
                bRet = false;
            int
                nRet;

            try
            {
                this.Controls.Remove(lbFuncs);
                lBlankCmb.Text = Srv.ExchangeContext.sHeadLine;
                lBlankCmb.Enabled = false;
                nRet = CallServer();
                bRet = (nRet == AppC.RC_OK) ? true : false;
                if (this.DialogResult == DialogResult.OK)
                    bRet = false;
            }
            catch
            {
                Srv.ErrorMsg("Ошибка подготовки формы");
            }
            return (bRet);
        }







        //private bool SetBlankList()
        //{
        //    bool bRet = false;

        //    bsBlanks = new BindingSource();
        //    try
        //    {
        //        string sRf = String.Format("(TD={0})OR(TD<0)OR(ISNULL(TD,-1)<0)", xMainF.xCDoc.xDocP.nTypD);
        //        //string sRf = String.Format("(TD={0})OR(TD<0)", xMainF.xCDoc.xDocP.nTypD);
        //        DataView dv = new DataView(xNSI.DT[NSI.NS_BLANK].dt, sRf,
        //            "TD DESC", DataViewRowState.CurrentRows);
        //        bsBlanks.DataSource = dv;
        //        if (dv.Count > 0)
        //        {
        //            lbFuncs.SelectedIndex = 0;
        //            lbFuncs.DataSource = bsBlanks;
        //            lbFuncs.DisplayMember = "NAME";
        //            bRet = true;
        //        }
        //        else
        //        {
        //            Srv.ErrorMsg("Нет бланков!", true);
        //        }
        //    }
        //    catch { }
        //    return (bRet);
        //}



        private bool SetBlankList(BindingSource bs)
        {
            bool bRet = false;
            try
            {
                lbFuncs.SelectedIndex = 0;
                lbFuncs.DataSource = bs;
                lbFuncs.DisplayMember = "NAME";
                if (bs.Count == 1)
                {
                    FillContext();
                }
                bRet = true;
            }
            catch { }
            return (bRet);
        }



        private void PrintBlank_Load(object sender, EventArgs e)
        {

            if (this.Tag != null)
            {
                if (AfterConstruct((object[])this.Tag))
                {
                    // Включить TouchScreen
                    xMainF.xBCScanner.TouchScr(true);
                    ehOldScan = new BarcodeScanner.BarcodeScanEventHandler(OnScanPL);
                    xMainF.xBCScanner.BarcodeScan += ehOldScan;
                    xHelpS = new Srv.HelpShow(this);
                    BeginEditP(null);
                }
                else
                {// ошибка, ничего не показываем
                    this.DialogResult = DialogResult.Cancel;
                }
            }
        }

        private void PrintBlank_Closing(object sender, CancelEventArgs e)
        {
            if (bEditMode)
                EndEditP();
            // Отключить TouchScreen
            xMainF.xBCScanner.TouchScr(false);
            if (ehOldScan != null)
            {
                xMainF.xBCScanner.BarcodeScan -= ehOldScan;
                ehOldScan = null;
            }
            this.Tag = new object[]{
                Srv.ExchangeContext.sPrinterSTC,
                Srv.ExchangeContext.sPrinterMOB};
            // форма закрывается, обмен закончен
            Srv.ExchangeContext.ExchgReason = AppC.EXCHG_RSN.NO_EXCHG;
        }


        private void OnScanPL(object sender, BarcodeScannerEventArgs e)
        {
            int nPfxL;
            string s;
            Control xC;
            OnePar xOrigPar;

            if (e.nID != BCId.NoData)
            {
                if (bEditMode)
                {
                    xC = aEd.WhichSetCur();
                    xOrigPar = WhatPar4Control(xC);
                    if (xOrigPar.SCANPRFX != EMPTY_SCPFX)
                    {// параметр может сканироваться
                        nPfxL = xOrigPar.SCANPRFX.Length;

                        if ((nPfxL == 0) ||
                        ((e.Data.Length >= nPfxL) && (e.Data.Substring(0, nPfxL) == xOrigPar.SCANPRFX)))
                        {
                            xC.Text = e.Data;
                        }
                    }
                }

                //if (e.nID == BCId.Code128)
                //{
                //    switch (e.Data.Length)
                //    {
                //        case 14:
                //            // похоже на № путевого
                //            break;
                //        case 12:
                //            if (e.Data.Substring(0, 3) == "778")
                //            {// похоже на № пропуска
                //            }
                //            else if (e.Data.Substring(0, 2) == "99")
                //            {// похоже на адрес (№ шлюза)
                //            }
                //            break;
                //    }
                //}
            }
        }

        // выделение всего поля при входе (текстовые поля)
        private void SelAllTextF(object sender, EventArgs e)
        {
            TextBox xT = (TextBox)sender;
            xT.SelectAll();
        }

        // Начало редактирования
        public void BeginEditP(Control xC)
        {
            aEd = new AppC.EditListC(new AppC.VerifyEditFields(VerifyB));
            if (nRegFrm == AppC.R_BLANK)
            {// выбор бланка
                aEd.AddC(lbFuncs, true);
                aEd.SetCur(aEd[0]);
            }
            else
            {// выбор принтера и установка параметров печати формы
                if (xPPrn != null)
                {
                    foreach(Control xEC in xPPrn.dicEditPars.Values)
                        // все, кроме меток
                        if (((string)xEC.Tag).IndexOf("ParLabel") < 0)
                            aEd.AddC(xEC);
                    aEd.SetCur((xC == null) ? aEd[0] : xC);
                }
            }
            bEditMode = true;
        }

        // Корректность введенного
        private AppC.VerRet VerifyB()
        {
            AppC.VerRet v;
            v.nRet = AppC.RC_CANCEL;
            if (true)
            {// можно завершить
                v.nRet = AppC.RC_OK; ;
            }
            v.cWhereFocus = null;
            return (v);
        }

        // Завершение редактирования
        public void EndEditP()
        {
            if (aEd != null)
            {
                if (nRegFrm == AppC.R_BLANK)
                {// в режиме выбора бланка
                    nRegFrm = AppC.R_PARS;
                }
                aEd.EditIsOver(this);
                bEditMode = false;
            }
        }

        private int CallServer()
        {
            int nRet = AppC.RC_CANCEL;
            // обращение к серверу для печати заданного бланка
            try
            {
                nRet = Try2Print();
                switch (nRet)
                {
                    case AppC.RC_OK:
                        // бланк распечатан, заканчиваем
                        this.DialogResult = DialogResult.OK;
                        break;
                    case AppC.RC_NEEDPARS:
                        // нужна установка дополнительных параметров
                        if (xPBuf != null)
                        {// что-то пришло отсервера
                            EndEditP();
                            // новые параметры
                            //DelOldParControls(true);
                            DelOldPars();
                            xPPrn = xPBuf;
                            //xSaved = new List<OnePar>();
                            //foreach(OnePar x in xPPrn.xParsCtrls)
                            //{
                            //    if (x.PARTYPE == C_GDAT)
                            //        xSaved.Add(x);
                            //}
                            nRet = ShowPars(xPPrn);
                            xPBuf = null;
                            if (nRet == AppC.RC_OK)
                                BeginEditP(null);
                            else
                                this.DialogResult = DialogResult.Abort;
                        }
                        break;
                }
            }
            catch (Exception e)
            { 
                Srv.ErrorMsg(e.Message);
                this.DialogResult = DialogResult.Abort;
            }
            return (nRet);
        }


        // Обработка клавиш
        private void PrintBlank_KeyDown(object sender, KeyEventArgs e)
        {
            int 
                nFunc;

            bool ret = true;
            string sCtrlType;
            Control xC = aEd.WhichSetCur();

            sCtrlType = xC.GetType().Name;
            bSkipKey = false;
            nFunc = xMainF.xFuncs.TryGetFunc(e);
            // пробросим ненужные
            if ((nFunc == AppC.F_HELP) ||
                (nFunc == AppC.F_GENFUNC) ||
                (nFunc == AppC.F_PRNBLK) ||
                (nFunc == AppC.F_LOAD_DOC) ||
                (nFunc == AppC.F_UPLD_DOC))
            { }
            else
                nFunc = -1;

            if (ehCurrFunc != null)
            {// клавиши ловит одна из функций
                //KeyEventArgs ne = new KeyEventArgs(e.KeyData);
                ret = ehCurrFunc(nFunc, e, ref ehCurrFunc);
                if (e.Handled)
                {
                    bSkipKey = true;
                    return;
                }
            }
                
            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case AppC.F_HELP:
                        xHelpS.ShowInfo(lstHelp, ref ehCurrFunc);
                        break;
                    case AppC.F_LOAD_DOC:
                        if (sCtrlType == C_CPRN_STD)
                            
                        {// для ComboBox
                            W32.SimulMouseClick(
                                xC.Location.X + xC.Width - 10, 
                                xC.Location.Y + xC.Height - 10, this);
                            ((ComboBoxEx)xC).ListOpened = true;
                        }
                        break;
                    case AppC.F_SIMSCAN:
#if DEB_MODE
                        // имитация обращения к серверу
                        try
                        {
                            EndEditP();
                            SimulParsSet();
                            CreateParsSet();
                            BeginEditP(null);
                        }
                        catch
                        {
                            nFunc = 67;
                        }
#endif
                        break;
                    case AppC.F_GENFUNC:
                    case AppC.F_PRNBLK:
                    case AppC.F_UPLD_DOC:
                        CallServer();
                        break;
                }
            }
            else
            {

                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        if (sCtrlType == C_CPRN_STD)
                        {// для ComboBox
                            if (((ComboBoxEx)xC).ListOpened == true)
                            {
                                ((ComboBoxEx)xC).ListOpened =
                                ret = false;
                            }
                        }
                        if (ret)
                            this.DialogResult = DialogResult.Cancel; 
                        break;
                    case W32.VK_RIGHT:
                    case W32.VK_LEFT:
                        ret = ChangeValByLRight(e.KeyValue, sCtrlType);
                        break;
                    case W32.VK_UP:
                    case W32.VK_DOWN:
                        ret = ChangeValByUpDown(e.KeyValue, sCtrlType);
                        if (!ret)
                        {
                            if (sCtrlType != C_DATE_STD)
                            {
                                if ((sCtrlType == C_CPRN_STD) && ((ComboBoxEx)aEd.Current).ListOpened)
                                { }
                                else
                                {
                                    ret = true;
                                    aEd.TryNext((e.KeyValue == W32.VK_UP) ? AppC.CC_PREV : AppC.CC_NEXT);
                                }
                            }
                        }
                        break;
                    case W32.VK_SPACE:
                        ret = false;
                        if (sCtrlType == C_CPRN_STD)
                        {// для ComboBox
                            if (((ComboBoxEx)xC).ListOpened == false)
                            {
                                W32.SimulMouseClick(
                                    xC.Location.X + xC.Width - 10,
                                    xC.Location.Y + xC.Height - 10, this);
                                ((ComboBoxEx)xC).ListOpened = true;
                            }
                        }
                        break;
                    case W32.VK_PERIOD:
                        ret = false;
                        if (sCtrlType == C_CPRN_STD)
                        {// для ComboBox - возможно, вход в редактирование
                            if (((ComboBoxEx)xC).ListOpened == false)
                            {
                            }
                        }
                        break;
                    case W32.VK_ENTER:
                        bSkipKey = true;
                        if (sCtrlType == "ListBox")
                            CallServer();
                        else
                        {
                            if (sCtrlType == C_CPRN_STD)
                            {// для ComboBox
                                if (((ComboBoxEx)aEd.Current).ListOpened)
                                {
                                    ((ComboBoxEx)aEd.Current).ListOpened = false;
                                    ret = false;
                                    break;
                                }
                            }
                            aEd.TryNext(AppC.CC_NEXT);
                        }
                        break;
                    default:
                        if (nRegFrm == AppC.R_BLANK)
                        {// в режиме выбора бланка
                            #region В режиме просмотра
                            switch (e.KeyValue)
                            {
                                case W32.VK_ENTER:
                                    break;
                                default:
                                    ret = false;
                                    break;
                            }
                            #endregion
                        }
                        else
                        {// в режиме редактирования
                            #region В режиме редактирования
                            switch (e.KeyValue)
                            {
                                case W32.VK_ENTER:
                                    break;
                                default:
                                    ret = false;
                                    break;
                            }

                            #endregion
                        }
                        break;
                }


            }
            e.Handled = ret;
            bSkipKey = ret;
        }

        private void PrintBlank_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipKey == true)
            {
                bSkipKey = false;
                e.Handled = true;
            }
        }

        // изменение значений параметров стрелками (влево-вправо)
        private bool ChangeValByLRight(int nKey, string sTName)
        {
            bool bRet = true;
            int
                nCur,
                nCount;

            switch (sTName)
            {
                case C_CPRN_STD:
                    nCount = ((ComboBox)aEd.Current).Items.Count;
                    if (nCount > 1)
                    {
                        nCur = ((ComboBox)aEd.Current).SelectedIndex;
                        if (nKey == W32.VK_RIGHT)
                        {// показать следующий
                            if (nCur == (nCount - 1))
                                nCur = 0;
                            else
                                nCur++;
                        }
                        else
                        {// показать предыдущий
                            if (nCur > 0)
                                nCur--;
                            else
                                nCur = nCount - 1;
                        }
                        ((ComboBox)aEd.Current).SelectedIndex = nCur;
                    }
                    break;
                case "CheckBox":
                    ((CheckBox)aEd.Current).Checked = !((CheckBox)aEd.Current).Checked;
                    break;
                case C_DATE_STD:
                    if (bMySimulKey)
                    {// я сам этот код сгененрировал из Up-Down
                        bMySimulKey = false;
                        bRet = false;
                    }
                    else
                    {// реальная клавиша, превращаем в Up-Down
                        bMySimulKey = true;
                        int nNewKey = (nKey == W32.VK_LEFT) ? W32.VK_DOWN : W32.VK_UP;
                        W32.SimulKey(nNewKey, nNewKey);
                    }
                    break;
                default:
                    bRet = false;
                    break;
            }
            return (bRet);
        }

        // изменение значений параметров стрелками (вверх-вниз)
        private bool ChangeValByUpDown(int nKey, string sTName)
        {
            bool bRet = true;
            int
                nCur,
                nCount;

            switch (sTName)
            {
                case "ListBox":
                    nCount = ((ListBox)aEd.Current).Items.Count;
                    if (nCount > 1)
                    {
                        nCur = ((ListBox)aEd.Current).SelectedIndex;
                        if (nKey == W32.VK_DOWN)
                        {// показать следующий
                            if (nCur == (nCount - 1))
                                nCur = 0;
                            else
                                nCur++;
                        }
                        else
                        {// показать предыдущий
                            if (nCur > 0)
                                nCur--;
                            else
                                nCur = nCount - 1;
                        }
                        ((ListBox)aEd.Current).SelectedIndex = nCur;
                    }
                    break;
                case C_DATE_STD:
                    if (bMySimulKey)
                    {// я сам этот код сгененрировал из Left-Right
                        bMySimulKey = false;
                        bRet = false;
                    }
                    else
                    {// реальная клавиша, превращаем в Left-Right
                        bMySimulKey = true;
                        int nNewKey = (nKey == W32.VK_UP) ? W32.VK_LEFT : W32.VK_RIGHT;
                        W32.SimulKey(nNewKey, nNewKey);
                    }
                    break;
                default:
                    bRet = false;
                    break;
            }
            return (bRet);
        }





        // подготовка DataSet для выгрузки
        //private DataSet DocDataSet(DataRow dr, CurUpLoad xU,  int bDet)
        //{
        //    DataSet ds1Rec = null;
        //    if (dr != null)
        //    {
        //        DataTable dtMastNew = xNSI.DT[NSI.BD_DOCOUT].dt.Clone();
        //        DataTable dtDetNew = xNSI.DT[NSI.BD_DOUTD].dt.Clone();
        //        DataTable dtBNew = xNSI.DT[NSI.BD_SPMC].dt.Clone();
        //        DataRow[] aDR, childRows;


        //        dtMastNew.LoadDataRow(dr.ItemArray, true);
        //        ds1Rec = new DataSet("dsMOne");
        //        ds1Rec.Tables.Add(dtMastNew);

        //        if (bDet >= 1)
        //        {
        //            if (bDet == 1)
        //            {

        //                childRows = dr.GetChildRows(NSI.REL2TTN);
        //                foreach (DataRow chRow in childRows)
        //                {
        //                    dtDetNew.LoadDataRow(chRow.ItemArray, true);
        //                    aDR = chRow.GetChildRows(NSI.REL2BRK);
        //                    foreach (DataRow bR in aDR)
        //                        dtBNew.LoadDataRow(bR.ItemArray, true);
        //                }
        //            }
        //            else if (bDet == 2)
        //            {
        //                if (xMainF.xCDoc.drCurRow != null)
        //                {
        //                    if (Srv.ExchangeContext.dr4Prn != null)
        //                    {
        //                        dtDetNew.LoadDataRow(Srv.ExchangeContext.dr4Prn.ItemArray, true);
        //                        aDR = Srv.ExchangeContext.dr4Prn.GetChildRows(NSI.REL2BRK);
        //                        foreach (DataRow bR in aDR)
        //                            dtBNew.LoadDataRow(bR.ItemArray, true);

        //                    }
        //                    else
        //                    {
        //                        Srv.ErrorMsg("Нет строки!");
        //                        return (null);
        //                    }
        //                }
        //                else
        //                {
        //                    Srv.ErrorMsg("Нет документа!");
        //                    return (null);
        //                }
        //            }
        //            ds1Rec.Tables.Add(dtDetNew);
        //            ds1Rec.Tables.Add(dtBNew);
        //        }

        //    }
        //    return (ds1Rec);
        //}

        private void FillContext()
        {
            int n;
            string 
                sH = "Сделайте выбор",
                sPar;

            try
            {
                sH = (string)((DataRowView)lbFuncs.SelectedItem)["NAME"];
                sPar = (string)((DataRowView)lbFuncs.SelectedItem)["KBL"];
            }
            catch
            {
                sPar = "BLANK=";
            }
            //sPar += (cmbPrn.Items.Count > 0)?String.Format(";PRN={0}", cmbPrn.SelectedItem):";PRN=";
            try
            {
                n = (int)((DataRowView)lbFuncs.SelectedItem)["PS"];
            }
            catch
            {
                n = 1;
            }

            Srv.ExchangeContext.sBlankCode = sPar;
            Srv.ExchangeContext.sHeadLine = String.Format("<< {0} >>", sH);
            Srv.ExchangeContext.FlagDetailRows = n;
        }

        // запрос сервера на печать бланка
        private int Try2Print()
        {
            int 
                nRet = AppC.RC_OK;
            string 
                sPar = "",
                sH = "Отпечатано...",
                sErr = "";
            LoadFromSrv dgRead;
            DataSet dsD = null;
            MemoryStream fs = null;
            XmlWriter writer;

            xSE = new MainF.ServerExchange(xMainF);
            dgRead = new LoadFromSrv(LoadParList);
            if (nRegFrm == AppC.R_BLANK)
            {
                FillContext();
                lBlankCmb.Text = Srv.ExchangeContext.sHeadLine;
                lBlankCmb.Enabled = false;
            }

            //Cursor crsOld = Cursor.Current;
            //Cursor.Current = Cursors.WaitCursor;

            xMainF.xCUpLoad = new CurUpLoad();
            //dsD = DocDataSet(xMainF.xCDoc.drCurRow, xMainF.xCUpLoad, Srv.ExchangeContext.FlagDetailRows);
            dsD = xMainF.DocDataSet4GF(xMainF.xCDoc.drCurRow, xMainF.xCUpLoad, Srv.ExchangeContext.FlagDetailRows);

            if (dsD == null)
            {
                return(AppC.RC_CANCEL);
            }
            if ((nRegFrm != AppC.R_BLANK) &&
                (xPPrn != null))
            {
                try
                {
                    Pars4Print xP = ParsRetBack(xPPrn);

                    fs = new MemoryStream();
                    XmlSerializer serializer = new XmlSerializer(typeof(Pars4Print));

                    XmlWriterSettings xWS = new XmlWriterSettings();
                    xWS.Encoding = Encoding.UTF8;
                    xWS.Indent = true;
                    xWS.OmitXmlDeclaration = true;
                    //xWS.ConformanceLevel = ConformanceLevel.Fragment;

                    writer = XmlTextWriter.Create(fs, xWS);

                    // убрать лишние атрибуты из заголовка класса
                    XmlSerializerNamespaces xns = new XmlSerializerNamespaces();
                    xns.Add(String.Empty, String.Empty);
                    serializer.Serialize(writer, xP, xns);

                    // Преобразование в UTF-8 добавляет в начало маркер кодировки (3 байта)
                    // два последних поменяем на CR/LF
                    fs.Seek(1, SeekOrigin.Begin);
                    fs.Write(new byte[2]{13, 10}, 0, 2);
                    fs.Seek(1, SeekOrigin.Begin);
                    int nL = (int)(fs.Length - 1);
                    xSE.XMLPars = new byte[nL];
                    for (int j = 0; j < nL; j++)
                        xSE.XMLPars[j] = Convert.ToByte(fs.ReadByte());

                    writer.Close();

                    //Srv.WriteXMLObjTxt(typeof(Pars4Print), xP, sFSer);
                    //using (FileStream fss = File.OpenRead(sFSer))
                    //{
                    //    fss.Read(xMainF.xCUpLoad.aAddDat, 0, (int)fss.Length);
                    //    fss.Close();
                    //}




                }
                catch (Exception ex)
                {
                    sErr = "Ошибка подготовки параметров";
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

            switch (Srv.ExchangeContext.ExchgReason)
            {
                case AppC.EXCHG_RSN.USER_COMMAND:
                    sPar = String.Format("PAR=(FUNC={0},BLANK={1},PRN={2},PRNMOB={3})",
                        AppC.COM_PRNBLK, Srv.ExchangeContext.sBlankCode,
                        Srv.ExchangeContext.sPrinterSTC, Srv.ExchangeContext.sPrinterMOB);
                    break;
                case AppC.EXCHG_RSN.SRV_INIT:
                    sPar = String.Format("PAR=(FUNC={0},PRN={1},PRNMOB={2})",
                        Srv.ExchangeContext.CMD_EXCHG, 
                        Srv.ExchangeContext.sPrinterSTC, 
                        Srv.ExchangeContext.sPrinterMOB);
                    break;
            }

            sErr = xSE.ExchgSrv(AppC.COM_GENFUNC, sPar, "", dgRead, dsD, ref nRet, 50);
            //Cursor.Current = crsOld;
            if (nRet != AppC.RC_OK)
            {
                nRet = xSE.ServerRet;
                if (nRet != AppC.RC_NEEDPARS)
                {
                    sH = "Ошибка";
                    Srv.PlayMelody(W32.MB_4HIGH_FLY);
                    Srv.ErrorMsg(sErr, sH, false);
                }
                else
                {
                    //sH = "Дополнительно!";
                    //sErr = "Установите нужные параметры";
                }
            }
            return (nRet);
        }

        // загрузка присланных параметров-контролов
        private void LoadParList(SocketStream stmX, Dictionary<string, string> aC, DataSet ds,
            ref string sErr, int nRetSrv)
        {
            const string sXMLErr = "Ошибка загрузки XML";
            string sXMLFile = "";
            object x;

            if (nRetSrv == AppC.RC_NEEDPARS)
            {
                try
                {
                    if (stmX.ASReadS.OutFile.Length == 0)
                    {
                        stmX.ASReadS.TermDat = AppC.baTermMsg;
                        if (stmX.ASReadS.BeginARead(true, 1000 * 20) != SocketStream.ASRWERROR.RET_FULLMSG)
                            throw new System.Net.Sockets.SocketException(10061);
                    }
                    sXMLFile = stmX.ASReadS.OutFile;

                    xPBuf = null;
                    if (Srv.ReadXMLObj(typeof(Pars4Print), out x, sXMLFile) == AppC.RC_OK)
                        sErr = "OK";
                    else
                        throw new Exception(sXMLErr);
                    xPBuf = (Pars4Print)x;
                }
                catch
                {
                    sErr = sXMLErr;
                    xPBuf = null;
                }
                finally
                {
                    if (sXMLFile.Length > 0)
                        File.Delete(sXMLFile);
                }
            }
        }

        private string ParCtrlName(string sPType, int i)
        {
            string sRet = "";
            return(sRet);
        }



        // начало размещения параметров
        int
            nXStart = 3,
            nYStart = 24;

        // разместить на форме новый набор параметров
        public int ShowPars(Pars4Print xPP)
        {
            int
                nRet = AppC.RC_OK,
                i = 0,
                j,
                nCtrlX = nXStart,
                nCtrlY = nYStart,

                nCtrlW = 220,
                nCtrlH = 20,

                nSpaceAftL = 0,
                nSpaceAftC = 6;
            string
                sE = "";
            Font tF;
            OnePar[] xP = xPP.xParsCtrls;
            OnePar 
                xC = null;

            Control xCurCtrl;
            ComboBoxEx xCmb;
            CheckBox xChk;
            TextBox xText;
            Label xL;
            DateTimePicker xDT;

            this.SuspendLayout();
            try
            {
                for (i = 0; i < xP.Length; i++)
                {
                    xC = xP[i];
                    if (xC == null)
                        continue;
                    xCurCtrl = null;

                    if ((xC.PARTYPE != C_CHECK) &&
                        (xC.PARTYPE != C_LBL) &&
                        (xC.PARTYPE != C_MEMI) &&
                        (xC.PARTYPE != C_GDAT))
                    {// заголовок (надпись) параметра
                        xL = new Label();
                        xL.Location = new System.Drawing.Point(nCtrlX, nCtrlY);
                        xL.Name = String.Format("ParLabel{0}", i);
                        xL.Tag = xL.Name;
                        xL.Size = new System.Drawing.Size(nCtrlW, nCtrlH - 2);
                        xL.Text = xC.PARCAPT;

                        tF = new Font(xL.Font.Name, 10, FontStyle.Bold);
                        xL.Font = tF;
                        this.Controls.Add(xL);
                        nCtrlY = nCtrlY + 18 + nSpaceAftL;
                    }

                    switch (xC.PARTYPE)
                    {
                        case C_CPRN:
                        case C_COMBO:
                            xCmb = new ComboBoxEx();
                            xCmb.Location = new System.Drawing.Point(nCtrlX, nCtrlY);
                            xCmb.Name = xC.PARNAME;
                            xCmb.Tag = String.Format("Par{0}{1}", xC.PARTYPE, i);
                            xCmb.Size = new System.Drawing.Size(nCtrlW, nCtrlH);
                            xCmb.DataSource = xC.PARLISTVAL;
                            xCmb.DropDownStyle = ComboBoxStyle.DropDownList;
                            //xCmb.DropDownStyle = ComboBoxStyle.DropDown;
                            xC.xObj = xCmb;
                            xCurCtrl = xCmb;
                            this.Controls.Add(xCmb);
                            if (xC.CURIND >= 0)
                                xCmb.SelectedIndex = xC.CURIND;
                            nCtrlY = nCtrlY + nCtrlH + nSpaceAftC;
                            break;
                        case C_CHECK:
                            xChk = new CheckBox();
                            xChk.Location = new System.Drawing.Point(nCtrlX, nCtrlY);
                            xChk.Name = xC.PARNAME;
                            xChk.Tag = String.Format("ParCheckBox{0}", i);
                            xChk.Size = new System.Drawing.Size(nCtrlW, nCtrlH);
                            xChk.Text = xC.PARCAPT;
                            xCurCtrl = xChk;
                            xChk.Checked = (xC.CURVAL == "1") ? true : false;
                            xC.xObj = xChk;
                            this.Controls.Add(xChk);
                            nCtrlY = nCtrlY + nCtrlH + nSpaceAftC;
                            break;
                        case C_TEXT:
                            xText = new TextBox();
                            xText.Location = new System.Drawing.Point(nCtrlX, nCtrlY);
                            xText.Name = xC.PARNAME;
                            xText.Tag = String.Format("ParTextBox{0}", i);
                            xText.Size = new System.Drawing.Size(nCtrlW, nCtrlH);
                            xText.Text = xC.CURVAL;
                            xText.GotFocus += new EventHandler(SelAllTextF);
                            xCurCtrl = xText;
                            xC.xObj = xText;
                            this.Controls.Add(xText);
                            nCtrlY = nCtrlY + nCtrlH + nSpaceAftC;
                            break;

                        case C_DATE:
                            xDT = new DateTimePicker();
                            xDT.Location = new System.Drawing.Point(nCtrlX, nCtrlY);
                            xDT.Name = xC.PARNAME;
                            xDT.Tag = String.Format("ParDTime{0}", i);
                            xDT.Size = new System.Drawing.Size(nCtrlW, nCtrlH);

                            try
                            {
                                xDT.Value = DateTime.ParseExact(xC.CURVAL, "yyyyMMdd", null);
                            }
                            catch { xDT.Value = DateTime.MinValue; }
                            if (xDT.Value == DateTime.MinValue)
                            {
                                try
                                {
                                    xDT.Value = DateTime.Parse(xC.CURVAL);
                                }
                                catch { xDT.Value = DateTime.MinValue; }
                            }

                            if (xDT.Value == DateTime.MinValue)
                                xDT.Value = DateTime.Now;

                            // смена значений - стрелками
                            xDT.ShowUpDown = true;
                            xCurCtrl = xDT;
                            xC.xObj = xDT;
                            this.Controls.Add(xDT);
                            nCtrlY = nCtrlY + nCtrlH + nSpaceAftC;
                            break;

                        case C_GDAT:
                            xPP.dicGenDat.Add(xP[i].PARNAME, xP[i]);
                            break;
                        case C_LBL:
                            xL = new Label();
                            xL.Location = new System.Drawing.Point(nCtrlX, nCtrlY);
                            xL.Name = xC.PARNAME;
                            xL.Tag = String.Format("ParLabel{0}", i);
                            j = ((xC.HEIGHT > 1) ? xC.HEIGHT : 1) * 16;
                            xL.Size = new System.Drawing.Size(nCtrlW, j);
                            xL.Text = xC.PARCAPT;
                            tF = new Font(xL.Font.Name, 10, FontStyle.Regular);
                            xL.Font = tF;
                            xL.BackColor = Color.WhiteSmoke;
                            this.Controls.Add(xL);
                            xCurCtrl = xL;
                            xC.xObj = xL;
                            nCtrlY = nCtrlY + j + nSpaceAftC;
                            break;
                        case C_MEMI:
                            break;
                        default:
                            sE = String.Format("Неизвестный тип {0}\nПараметр {1}", xC.PARTYPE, i);
                            Srv.ErrorMsg(sE, true);
                            nRet = AppC.RC_CANCEL;
                            break;
                    }
                    if (xCurCtrl != null)
                        xPP.dicEditPars.Add(xCurCtrl.Name, xCurCtrl);
                }
            }
            catch 
            {
                //sE = (xC == null) ? "" : xC.PARNAME;
                //throw new Exception(String.Format("Ошибка размещения {0}", sE));
                throw new Exception(String.Format("Ошибка размещения {0}", xC.PARNAME));
            }
            finally 
            {
                this.ResumeLayout();
            }
            return (nRet);
        }

        // убрать с формы предыдущий набор параметров
        // или подготовить массив параметров-контролов для возврата на сервер
        //public Pars4Print DelOldParControls(bool bDelOnly)
        //{
        //    int
        //        nNewLen,
        //        i, j;

        //    Pars4Print xP = null;
        //    List<Control> lC = new List<Control>();
        //    OnePar xOrigPar;
        //    this.SuspendLayout();
        //    foreach (Control c in this.Controls)
        //    {
        //        if (c.Tag != null)
        //        {
        //            if (c.Tag.ToString().StartsWith("Par"))
        //            {
        //                if (bDelOnly ||
        //                    ((!bDelOnly)&& (c.GetType().Name != "Label")))
        //                    lC.Add(c);
        //            }
        //        }
        //    }
        //    nNewLen = lC.Count + xSaved.Count;
        //    if (nNewLen > 0)
        //    {
        //        Type xT;

        //        xP = new Pars4Print();
        //        xP.xParsCtrls = new OnePar[nNewLen];
        //        for (i = 0; i < lC.Count; i++)
        //        {
        //            if (bDelOnly)
        //            {
        //                this.Controls.Remove(lC[i]);
        //                lC[i] = null;
        //            }
        //            else
        //            {
        //                xOrigPar = WhatPar4Control(lC[i]);
        //                xT = lC[i].GetType();

        //                xP.xParsCtrls[i]         = new OnePar();
        //                xP.xParsCtrls[i].PARTYPE = xOrigPar.PARTYPE;
        //                xP.xParsCtrls[i].PARNAME = xOrigPar.PARNAME;
        //                xP.xParsCtrls[i].PARCAPT = xOrigPar.PARCAPT;
        //                switch (xT.Name)
        //                {
        //                    case C_CPRN_STD:
        //                        if (((ComboBox)lC[i]).Items.Count > 0)
        //                        {
        //                            xP.xParsCtrls[i].CURIND = j = ((ComboBox)lC[i]).SelectedIndex;
        //                            xP.xParsCtrls[i].CURVAL = ((ComboBox)lC[i]).Items[j].ToString();
        //                            j = ((string)((ComboBox)lC[i]).Tag).IndexOf(C_CPRN);
        //                            if (j >= 0)
        //                            {
        //                                if (xP.xParsCtrls[i].CURVAL.IndexOf("MOBPRN") >= 0)
        //                                    ExchangeContext.sPrinterMOB = xP.xParsCtrls[i].CURVAL;
        //                                else
        //                                    ExchangeContext.sPrinterSTC = xP.xParsCtrls[i].CURVAL;
        //                            }
        //                        }
        //                        break;
        //                    case "CheckBox":
        //                        xP.xParsCtrls[i].CURVAL = (((CheckBox)lC[i]).Checked)?"1":"0";
        //                        break;
        //                    case "TextBox":
        //                        xP.xParsCtrls[i].CURVAL = ((TextBox)lC[i]).Text;
        //                        break;
        //                    case C_DATE_STD:
        //                        xP.xParsCtrls[i].CURVAL = ((DateTimePicker)lC[i]).Value.ToString("yyyyMMdd");
        //                        break;
        //                }
        //            }
        //        }
        //        if (!bDelOnly)
        //            foreach (OnePar x in xSaved)
        //                xP.xParsCtrls[i] = x;
        //    }
        //    this.ResumeLayout();
        //    return (xP);
        //}

        private OnePar WhatPar4Control(Control xC)
        {
            OnePar xP = null;
            foreach(OnePar x in xPPrn.xParsCtrls)
                if (x.PARNAME == xC.Name)
                {
                    xP = x;
                    break;
                }
            return (xP);
        }











































        // убрать с формы предыдущий набор параметров
        public void DelOldPars()
        {
            int i = 0;
            List<Control> lC = new List<Control>();

            this.SuspendLayout();
            foreach (Control c in this.Controls)
                if ((c.Tag != null) &&
                    (c.Tag.ToString().StartsWith("Par")))
                    lC.Add(c);
            for (i = 0; i < lC.Count; i++)
            {
                this.Controls.Remove(lC[i]);
                lC[i] = null;
            }
            this.ResumeLayout();
        }

        // подготовить массив параметров-контролов для возврата на сервер
        public Pars4Print ParsRetBack(Pars4Print xPP)
        {
            int
                nNewLen,
                i, j;
            Pars4Print xP = null;
            OnePar xOrigPar;

            nNewLen = xPP.dicEditPars.Count + xPP.dicGenDat.Count;
            if (nNewLen > 0)
            {
                //Type xT;

                xP = new Pars4Print();
                xP.xParsCtrls = new OnePar[nNewLen];
                i = 0;
                foreach (Control xEC in xPP.dicEditPars.Values)
                {
                    xOrigPar = WhatPar4Control(xEC);

                    xP.xParsCtrls[i] = new OnePar();
                    xP.xParsCtrls[i].PARTYPE = xOrigPar.PARTYPE;
                    xP.xParsCtrls[i].PARNAME = xOrigPar.PARNAME;
                    xP.xParsCtrls[i].PARCAPT = xOrigPar.PARCAPT;
                    xP.xParsCtrls[i].SCANPRFX = (xOrigPar.SCANPRFX == EMPTY_SCPFX) ? "" : xOrigPar.SCANPRFX;

                    switch (xOrigPar.PARTYPE)
                    {
                        case C_COMBO:
                        case C_CPRN:
                            if (((ComboBox)xEC).Items.Count > 0)
                            {
                                xP.xParsCtrls[i].CURIND = j = ((ComboBox)xEC).SelectedIndex;
                                xP.xParsCtrls[i].CURVAL = ((ComboBox)xEC).Items[j].ToString();
                                //j = ((string)((ComboBox)xEC).Tag).IndexOf(C_CPRN);
                                if (xOrigPar.PARTYPE == C_CPRN)
                                {
                                    if (xP.xParsCtrls[i].CURVAL.IndexOf("MOBPRN") >= 0)
                                        Srv.ExchangeContext.sPrinterMOB = xP.xParsCtrls[i].CURVAL;
                                    else
                                        Srv.ExchangeContext.sPrinterSTC = xP.xParsCtrls[i].CURVAL;
                                }
                            }
                            break;
                        case C_CHECK:
                            xP.xParsCtrls[i].CURVAL = (((CheckBox)xEC).Checked) ? "1" : "0";
                            break;
                        case C_TEXT:
                            xP.xParsCtrls[i].CURVAL = ((TextBox)xEC).Text;
                            break;
                        case C_DATE:
                            xP.xParsCtrls[i].CURVAL = ((DateTimePicker)xEC).Value.ToString("yyyyMMdd");
                            break;
                    }
                    i++;
                }
                foreach (OnePar x in xPP.dicGenDat.Values)
                    xP.xParsCtrls[i++] = x;
            }
            return (xP);
        }



#if DEB_MODE
        #region Для отладки

        //private void btSer_Click(object sender, EventArgs e)
        //{
        //    SimulParsSet();
        //}

        //private void btReset_Click(object sender, EventArgs e)
        //{
        //    CreateParsSet();
        //    BeginEditP(null);
        //}


        // вариант набора параметров
        private int nParPack = 0;

        private void SimulParsSet()
        {
            xPPrn = new Pars4Print();

            if (nParPack == 0)
            {
                xPPrn.xParsCtrls = new OnePar[] { new OnePar(), new OnePar(), new OnePar() };
                xPPrn.xParsCtrls[0].PARTYPE = C_CHECK;
                xPPrn.xParsCtrls[0].PARNAME = "ParCheckBox1";
                xPPrn.xParsCtrls[0].PARCAPT = "Печатать исполнителя";
                xPPrn.xParsCtrls[0].CURVAL = "1";

                xPPrn.xParsCtrls[1].PARTYPE = C_COMBO;
                xPPrn.xParsCtrls[1].PARNAME = "ParComboBox1";
                xPPrn.xParsCtrls[1].PARCAPT = "Водители";
                xPPrn.xParsCtrls[1].PARLISTVAL = new List<string>();
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("1111111111");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("2222");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("3333333444441111111111");
                xPPrn.xParsCtrls[1].CURIND = 2;

                xPPrn.xParsCtrls[2].PARTYPE = C_DATE;
                xPPrn.xParsCtrls[2].PARNAME = "ParDate1";
                xPPrn.xParsCtrls[2].PARCAPT = "Дата чего-то там";
                xPPrn.xParsCtrls[2].CURVAL  = "20120817";

                nParPack++;
            }
            else if (nParPack == 1)
            {
                xPPrn.xParsCtrls = new OnePar[15];
                //xPPrn.xParsCtrls = new OnePar[] { new OnePar(), new OnePar(), new OnePar(), 
                //    new OnePar(), new OnePar() };
                xPPrn.xParsCtrls[0] = new OnePar();
                xPPrn.xParsCtrls[0].PARTYPE = C_TEXT;
                xPPrn.xParsCtrls[0].PARNAME = "ParTextBox1";
                xPPrn.xParsCtrls[0].PARCAPT = "ФИО предводителя";
                xPPrn.xParsCtrls[0].CURVAL = "Какая-то хрень";

                xPPrn.xParsCtrls[1] = new OnePar();
                xPPrn.xParsCtrls[1].PARTYPE = C_CPRN;
                xPPrn.xParsCtrls[1].PARNAME = "ParComboPrn";
                xPPrn.xParsCtrls[1].PARCAPT = "Водители";
                xPPrn.xParsCtrls[1].PARLISTVAL = new List<string>();
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-1");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-2");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-3");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-4");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-5");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-6");
                xPPrn.xParsCtrls[1].PARLISTVAL.Add("STCPRN-7");
                xPPrn.xParsCtrls[1].CURIND = 6;

                xPPrn.xParsCtrls[2] = new OnePar();
                xPPrn.xParsCtrls[2].PARTYPE = C_CHECK;
                xPPrn.xParsCtrls[2].PARNAME = "ParCheckBox1";
                xPPrn.xParsCtrls[2].PARCAPT = "Морду вареньем";
                xPPrn.xParsCtrls[2].CURVAL = "1";

                xPPrn.xParsCtrls[3] = new OnePar();
                xPPrn.xParsCtrls[3].PARTYPE = C_COMBO;
                xPPrn.xParsCtrls[3].PARNAME = "ParComboBox1";
                xPPrn.xParsCtrls[3].PARCAPT = "Закуска";
                xPPrn.xParsCtrls[3].PARLISTVAL = new List<string>();
                xPPrn.xParsCtrls[3].PARLISTVAL.Add("Горячая ");
                xPPrn.xParsCtrls[3].PARLISTVAL.Add("Холодная");
                xPPrn.xParsCtrls[3].PARLISTVAL.Add("Холявная");
                xPPrn.xParsCtrls[3].CURIND = 1;

                for (int i = 4; i < 13; i++)
                {
                    xPPrn.xParsCtrls[i] = new OnePar();
                    xPPrn.xParsCtrls[i].PARTYPE = C_TEXT;
                    xPPrn.xParsCtrls[i].PARNAME = String.Format("ParTextBox{0}", i - 2);
                    xPPrn.xParsCtrls[i].PARCAPT = String.Format("Заголовок для BOX {0}", i - 2);
                    xPPrn.xParsCtrls[i].CURVAL = "Какая-то хрень";
                }

                nParPack++;
            }
            else if (nParPack == 2)
            {
                xPPrn.xParsCtrls = new OnePar[] { new OnePar() };
                xPPrn.xParsCtrls[0].PARTYPE = C_COMBO;
                xPPrn.xParsCtrls[0].PARNAME = "ParComboBox1";
                xPPrn.xParsCtrls[0].PARCAPT = "Экспедиторы";
                xPPrn.xParsCtrls[0].PARLISTVAL = new List<string>();
                xPPrn.xParsCtrls[0].PARLISTVAL.Add("ФФФФФФФФФФыыыыы");
                xPPrn.xParsCtrls[0].PARLISTVAL.Add("ДЛпппппРР");
                xPPrn.xParsCtrls[0].PARLISTVAL.Add("333444441111111111");
                nParPack = 0;
            }
            Srv.WriteXMLObjTxt(typeof(Pars4Print), xPPrn, sFSer);
        }

        private void CreateParsSet()
        {
            object x;
            xPPrn = null;
            Srv.ReadXMLObj(typeof(Pars4Print), out x, sFLoad);
            xPPrn = (Pars4Print)x;
            //DelOldParControls(true);
            DelOldPars();
            ShowPars(xPPrn);
        }

        #endregion
#endif





    }
}