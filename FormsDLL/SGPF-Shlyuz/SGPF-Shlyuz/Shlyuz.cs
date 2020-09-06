using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using SavuSocket;

using ScannerAll;
using PDA.Service;
using PDA.OS;
//using PDA.BarCode;
using SkladGP;

using FRACT = System.Decimal;

namespace SGPF_Shlyuz
{
    public partial class Shlyuz : Form
    {
        // прибытие/убытие
        private const int 
            IO_COME = 1,
            IO_GOUT = 2;

        // текущий режим фиксации (по пропуску или ПЛ)
        private int 
            nRegFixing;

        private bool
            bEditMode = false,
            bSkipKey = false;           // не обрабатывать введенный символ
        
        private MainF 
            xMainF = null;
        private NSI 
            xNSI;


        private BindingSource 
            bsSh;
        private DataView 
            dvAvto;
        private ParkA 
            xPark;
           
        private AppC.EditListC 
            aEd;

        // цвета для операций
        private Color 
            colCome = Color.PaleGreen,
            colGOut = Color.Gold;

        private BarcodeScanner.BarcodeScanEventHandler ehOldScan = null;

        class ParkA
        {
            private int m_Dir = 1;
            private int m_Shl = 0;
            private int m_Doc = 0;
            private int m_Sm  = 0;

            private int m_LastOp = 0;
            private DataRow m_DrCome = null;

            private int m_DirP = -1;
            private int m_ShlP = -1;
            private int m_DocP = -1;
            private int m_SmP  = -1;

            private string m_NPropusk = "";
            private string m_ShlAdr = "";

            public MainF.AddrInfo
                xA;


            // текущий режим фиксации
            private int m_nRegFix;

            public ParkA(int nR)
            {
                RegFix = nR;
            }


            // код операции
            public int ParkIO
            {
                get { return m_Dir; }
                set { m_Dir = value; }
            }
            // № шлюза
            public int NShl
            {
                get { return m_Shl; }
                set { m_Shl = value; }
            }
            // № документа
            public int NPtvList
            {
                get { return m_Doc; }
                set { m_Doc = value; }
            }
            // № смены
            public int NSm
            {
                get { return m_Sm; }
                set { m_Sm = value; }
            }

            // Код последней успешной операции
            public int LastOper
            {
                get { return m_LastOp; }
                set { m_LastOp = value; }
            }

            // ID строки прибывшего авто
            public DataRow DRCome
            {
                get { return m_DrCome; }
                set { m_DrCome = value; }
            }

            // № пропуска прибывшего авто
            public string Propusk
            {
                get { return m_NPropusk; }
                set { m_NPropusk = value; }
            }

            // адрес шлюза
            public string ShlAdr
            {
                get { return m_ShlAdr; }
                set { m_ShlAdr = value; }
            }

            // текущий режим фиксации
            public int RegFix
            {
                get { return m_nRegFix; }
                set { m_nRegFix = value; }
            }


            // параметры списка авто для операции менялись?
            public bool IsChangePars()
            {
                bool ret = true;
                if (((m_DirP < 0) || (m_Dir == m_DirP)) &&
                    //((m_ShlP < 0) || (m_Shl == m_ShlP)) &&
                    ((m_SmP < 0)  || (m_Sm  == m_SmP)))
                    ret = false;
                ret = true;
                return (ret);
            }
            public void SaveOldPars()
            {
                m_DirP = m_Dir;
                m_ShlP = m_Shl;
                m_SmP = m_Sm;
                m_DocP = m_Doc;
            }
        }

        private MainF.ServerExchange xSE;

        public Shlyuz()
        {
            InitializeComponent();
        }


        public void AfterConstruct(MainF xM)
        {
            xMainF = xM;
            xNSI = xM.xNSI;
            // каждый раз все по-новой
            xMainF.aAvtoPark = null;
            nRegFixing = (int)xMainF.xDLLPars;

            xPark = new ParkA(nRegFixing);
            dvAvto = xNSI.DT[NSI.BD_SOTG].dt.DefaultView;

            SetBindShlyuz();

            // Настройки грида
            dgShlyuz.SuspendLayout();
            dgShlyuz.DataSource = bsSh;
            ShlzStyle(dgShlyuz);
            dgShlyuz.ResumeLayout();

            ehOldScan = new BarcodeScanner.BarcodeScanEventHandler(OnScanPL);
            xMainF.xBCScanner.BarcodeScan += ehOldScan;
            if (xMainF.xSm.urCur >= Smena.USERRIGHTS.USER_BOSS_SKLAD)
            {
                tPropusk.Enabled = true;
                tShlAddr.Enabled = true;
            }
            if (xMainF.aAvtoPark == null)
            {// по умолчанию - прибытие
                //lHeadP.SuspendLayout();
                lHeadP.BackColor = colCome;
                //lHeadP.ResumeLayout();
                if (nRegFixing == AppC.FX_PTLST)
                    BeginEditP(null);
            }
            else
            {// последней была операция прибытия
                xPark.DRCome = dvAvto.Table.NewRow();
                xPark.DRCome.ItemArray = xMainF.aAvtoPark;
                bsSh.Filter = string.Format("ID={0}", xPark.DRCome["ID"]);
                bsSh.ResetBindings(false);
                dgShlyuz.Focus();
                // сейчас предлагается убытие
                xPark.NShl = (int)xPark.DRCome["NSH"];
                xPark.NPtvList = (int)xPark.DRCome["ND"];
                xPark.NSm = int.Parse((string)xPark.DRCome["KSMEN"]);
                tNDoc.DataBindings[0].ReadValue();
                tSm.DataBindings[0].ReadValue();
                tShlAddr.DataBindings[0].ReadValue();
                tIn.Text = "Прибыл  " + (string)xPark.DRCome["DTP"];
                tAvto.Text = (string)xPark.DRCome["KAVT"];
                IOChange(IO_GOUT);
                IsParkAvail();
            }
        }

        private void Shlyuz_Closing(object sender, CancelEventArgs e)
        {
            if (nRegFixing == AppC.FX_PTLST)
                EndEditP();
            xMainF.aAvtoPark = null;
            if (xPark.LastOper == IO_COME)
            {// зафиксировали прибытие
                if (xPark.DRCome != null)
                    xMainF.aAvtoPark = xPark.DRCome.ItemArray;
            }
            bsSh.RemoveFilter();
            xMainF.xBCScanner.BarcodeScan -= ehOldScan;
        }


        private void OnScanPL(object sender, BarcodeScannerEventArgs e)
        {
            string s;
            ScanVarGP
                xScan;


            if (e.nID != BCId.NoData)
            {
                xScan = new ScanVarGP(e, xNSI.DT["NS_AI"].dt);

                if (e.nID == BCId.Code128)
                {

                    if (((xScan.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0))
                    {// похоже на адрес (№ шлюза)
                        s = e.Data.Substring(2);
                        xPark.ShlAdr = s;
                        xPark.xA = new MainF.AddrInfo(xScan, xMainF.xSm.nSklad);

                        tShlAddr.Text = s;
                        lShlName.Text = xPark.xA.AddrShow;
                        TryFixAvto();
                    }
                    else
                    {
                        switch (e.Data.Length)
                        {
                            case 14:
                                // похоже на № путевого
                                tNDoc.Text = e.Data.Substring(7);
                                xPark.NPtvList = int.Parse(tNDoc.Text);
                                if ((bEditMode == true) && (xPark.NShl > 0))
                                {
                                    EndEditP();
                                    if ((bsSh.Count == 0) || (xPark.IsChangePars()))
                                    {
                                        LoadAvtoList();
                                    }
                                }
                                break;
                            case 12:
                                if (e.Data.Substring(0, 3) == "778")
                                {// похоже на № пропуска
                                    xPark.Propusk = e.Data.Substring(3);
                                    tPropusk.Text = e.Data.Substring(3);
                                    TryFixAvto();
                                }
                                //else if (e.Data.Substring(0, 2) == "99")
                                //{// похоже на адрес (№ шлюза)
                                //    s = e.Data.Substring(2);
                                //    xPark.ShlAdr = s;
                                //    tShlAddr.Text = s;
                                //    TryFixAvto();
                                //}

                                break;
                        }
                    }

                }
            }
        }


        // Привязка к данным
        private void SetBindShlyuz()
        {
            bsSh = new BindingSource();
            bsSh.DataSource = dvAvto;


            // № пропуска
            tPropusk.DataBindings.Add("Text", xPark, "Propusk");
            tPropusk.DataBindings[0].DataSourceNullValue = "";
            tPropusk.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            // Адрес шлюза
            tShlAddr.DataBindings.Add("Text", xPark, "ShlAdr");
            tShlAddr.DataBindings[0].DataSourceNullValue = 0;
            tShlAddr.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            // № шлюза
            tShNomPP.DataBindings.Add("Text", xPark, "NShl");
            tShNomPP.DataBindings[0].DataSourceNullValue = 0;
            tShNomPP.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            // № смены
            tSm.DataBindings.Add("Text", xPark, "NSm");
            tSm.DataBindings[0].DataSourceNullValue = 0;

            // № документа
            tNDoc.DataBindings.Add("Text", xPark, "NPtvList");
            tNDoc.DataBindings[0].DataSourceNullValue = 0;

            // Дата прибытия
            lDTCome.DataBindings.Add("Text", bsSh, "DTP");
            lDTCome.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.Never;

            // Дата убытия
            lDTOut.DataBindings.Add("Text", bsSh, "DTU");
            lDTOut.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.Never;
        }

        // выделение всего поля при входе (текстовые поля)
        private void SelAllTextF(object sender, EventArgs e)
        {
            TextBox xT = (TextBox)sender;
            xT.SelectAll();
            if (xT.Equals(tAvto))
            {
                tAvto.DataBindings.Clear();
            }
        }

        // стили таблицы авто
        private void ShlzStyle(DataGrid dg)
        {
            ServClass.DGTBoxColorColumn sC;
            System.Drawing.Color colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = NSI.BD_SOTG;

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "KAVT";
            sC.HeaderText = "  № авто";
            sC.Width = 72;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "ROUTE";
            sC.HeaderText = "      Маршрут";
            sC.Width = 130;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "KSMEN";
            sC.HeaderText = "См";
            sC.Width = 30;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "NSH";
            sC.HeaderText = "Шлюз";
            sC.Width = 40;
            ts.GridColumnStyles.Add(sC);
            dg.TableStyles.Add(ts);
        }

        // проверка доступности операций
        private bool IsParkAvail()
        {
            bool ret = false;
            if (xPark.ParkIO == IO_COME)
            {// для выполнения прибытия
                if (bsSh.Count > 0)
                {
                    if ((dgShlyuz.Focused || (bsSh.Count == 1)))
                        ret = true;
                }
                else
                {
                    if ((xPark.NSm > 0) &&
                        (xPark.NPtvList > 0) &&
                        (tAvto.Text.Length >= 4))
                        ret = true;
                }
                tIn.Enabled = ret;
            }
            else
            {// для выполнения убытия
                if (bsSh.Count > 0)
                {
                    if ((dgShlyuz.Focused || (bsSh.Count == 1)))
                        ret = true;
                }
                tOut.Enabled = ret;
            }
            return (ret);
        }

        // Обработка клавиш
        private void Shlyuz_KeyDown(object sender, KeyEventArgs e)
        {
            int 
                nFunc = 0;
            bool 
                ret = true;

            bSkipKey = false;
            nFunc = xMainF.xFuncs.TryGetFunc(e);
            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case AppC.F_LOAD_DOC:
                        // загрузка нового списка
                        LoadAvtoList();
                        tAvto.Enabled = true;
                        IsParkAvail();
                        tAvto.Focus();
                        //tAvto_TextChanged(tAvto, new EventArgs());
                        break;
                    case AppC.F_UPLD_DOC:
                        // повторная выгрузка
                        TryFixAvto();
                        break;
                }
                if (bEditMode == false)
                {// только в режиме просмотра
                    switch (nFunc)
                    {
                        case AppC.F_ADD_REC:
                            // новая операция
                            if (xPark.ParkIO == IO_GOUT)
                                IOChange(0);
                            BeginEditP(null);
                            break;
                        case AppC.F_GOFIRST:
                        case AppC.F_GOLAST:
                            // 1-я/последняя
                            if (dgShlyuz.Focused)
                            {
                                if (nFunc == AppC.F_GOFIRST)
                                    bsSh.MoveFirst();
                                else
                                    bsSh.MoveLast();
                            }
                            else
                                ret = false;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else
                    ret = false;
            }
            else
            {

                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        this.Close();
                        break;
                    case W32.VK_RIGHT:
                    case W32.VK_LEFT:
                        IOChange(0);
                        break;
                    default:
                        if (bEditMode == false)
                        {// в режиме просмотра (выбор авто)
                            #region В режиме просмотра
                            switch (e.KeyValue)
                            {
                                case W32.VK_UP:
                                    if (tAvto.Focused == true)
                                    {
                                        if (bsSh.Count > 0)
                                            dgShlyuz.Focus();
                                    }
                                    else
                                    {
                                        if (bsSh.Position == 0)
                                            tAvto.Focus();
                                        else
                                            ret = false;
                                    }
                                    break;
                                case W32.VK_DOWN:
                                    if (tAvto.Focused == true)
                                    {
                                        if (bsSh.Count > 0)
                                            dgShlyuz.Focus();
                                    }
                                    else
                                    {
                                        if (bsSh.Position == bsSh.Count - 1)
                                            tAvto.Focus();
                                        else
                                            ret = false;
                                    }
                                    break;
                                case W32.VK_ENTER:
                                    bSkipKey = true;
                                    if (IsParkAvail())
                                        SaveAvtoPark();
                                    else
                                        Srv.ErrorMsg("Сделайте выбор!", true);
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
                                case W32.VK_UP:
                                    aEd.TryNext(AppC.CC_PREV);
                                    break;
                                case W32.VK_DOWN:
                                    aEd.TryNext(AppC.CC_NEXT);
                                    break;
                                case W32.VK_ENTER:
                                    bSkipKey = true;
                                    if (aEd.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                                    {
                                        EndEditP();
                                        if ((bsSh.Count == 0) || (xPark.IsChangePars()))
                                        {
                                            LoadAvtoList();
                                        }
                                    }
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

        private void Shlyuz_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipKey == true)
            {
                bSkipKey = false;
                e.Handled = true;
            }
        }

        // Начало редактирования
        public void BeginEditP(Control xC)
        {
            bool
                bEn = false;

            tIn.Text  = "Прибыл   <ENT>";
            tOut.Text = "Убыл      <ENT>";
            tIn.Enabled = false;
            tOut.Enabled = false;
            tAvto.Enabled = false;

            aEd = new AppC.EditListC(new AppC.VerifyEditFields(VerifyB));


            if (xPark.RegFix == AppC.FX_PTLST)
            {
                aEd.AddC(tNDoc);
                //aEd.AddC(tSm);
                aEd.AddC(tShlAddr);

                aEd.SetCur((xC == null) ? aEd[0] : xC);
            }

            bEditMode = true;
            xPark.LastOper = 0;
            bsSh.RemoveFilter();
            bsSh.ResetBindings(false);
        }

        // Корректность введенного
        private AppC.VerRet VerifyB()
        {
            AppC.VerRet v;
            v.nRet = AppC.RC_CANCEL;
            if (xPark.NShl > 0)
            {// можно завершить
                v.nRet = AppC.RC_OK; ;
            }
            v.cWhereFocus = null;
            return (v);
        }

        // Завершение редактирования
        public void EndEditP()
        {
            if (bEditMode == true)
            {
                aEd.EditIsOver();
                bEditMode = false;
                tAvto.Enabled = true;
                tAvto.Focus();
            }
        }

        // смена операции
        private void IOChange(int Needed)
        {
            xPark.ParkIO = (Needed == 0) ? ((xPark.ParkIO == 1) ? 2 : 1) : Needed;
            lHeadP.SuspendLayout();
            if (xPark.ParkIO == IO_GOUT)
            {
                lHeadP.Text = "<=<   УБЫТИЕ   >=>";
                lHeadP.BackColor = colGOut;

            }
            else
            {// Прибытие
                lHeadP.Text = " >=>   ПРИБЫТИЕ   <=<";
                lHeadP.BackColor = colCome;
            }
            lHeadP.ResumeLayout();
        }


        private void LoadAvtoList()
        {
            LoadAvtoList(false);
        }


        private void LoadAvtoList(bool bUsePropusk)
        {
            int nRet = AppC.RC_OK;
            string sPar,
                sErr = "";
            LoadFromSrv dgRead;

            xSE = new MainF.ServerExchange(xMainF);
            dgRead = new LoadFromSrv(AvtoList);
            sPar = String.Format("(IO={0},NSH={1},KSK={2}", xPark.ParkIO, xPark.NShl, xMainF.xSm.nSklad);
            if (xPark.NSm > 0)
                sPar += ",KSMEN=" + xPark.NSm.ToString();
            if (xPark.NPtvList > 0)
                sPar += ",NPL=" + xPark.NPtvList.ToString();
            if (xPark.Propusk.Length > 0)
                sPar += ",PRPSK=" + xPark.Propusk.Trim();
            sPar += ")";

            //Cursor crsOld = Cursor.Current;
            //Cursor.Current = Cursors.WaitCursor;
            sErr = xSE.ExchgSrv(AppC.COM_ZOTG, sPar, "", dgRead, null, ref nRet);
            //FakeEx();
            //Cursor.Current = crsOld;
            bsSh.RemoveFilter();
            if (nRet == AppC.RC_OK)
            {
                xPark.SaveOldPars();
                IsParkAvail();
                //lAvtoCount.Text = bsSh.Count.ToString();
                //xPark.SaveOldPars();
            }
            else
            {
                ((DataView)bsSh.DataSource).Table.Rows.Clear();
                Srv.ErrorMsg(sErr, true);
                if (nRet == 24)
                {// неверный шлюз
                    BeginEditP(tShlAddr);
                }
            }
        }


        private void AvtoList(SocketStream stmX, Dictionary<string, string> aC, DataSet ds,
            ref string sErr, int nRetSrv)
        {
            object xDS = dgShlyuz.DataSource;
            dgShlyuz.SuspendLayout();
            dgShlyuz.DataSource = null;
            try
            {
                sErr = "Ошибка чтения XML";
                string sXMLFile = "";

                //int nFileSize = ServClass.ReadXMLWrite2File(stmX.SStream, ref sXMLFile);
                stmX.ASReadS.TermDat = AppC.baTermMsg;
                if (stmX.ASReadS.BeginARead(true, 1000 * 60) == SocketStream.ASRWERROR.RET_FULLMSG)
                    sXMLFile = stmX.ASReadS.OutFile;
                else
                    throw new System.Net.Sockets.SocketException(10061);

                sErr = "Ошибка загрузки XML";
                DataSet dsZ = new DataSet("dsZ");
                DataTable dt = xNSI.DT[NSI.BD_SOTG].dt;
                dt.BeginInit();
                dt.Rows.Clear();
                System.Xml.XmlReader xmlRd = System.Xml.XmlReader.Create(sXMLFile);
                dt.ReadXml(xmlRd);
                xmlRd.Close();
                System.IO.File.Delete(sXMLFile);
                dt.EndInit();
                sErr = "OK";
            }
            finally
            {
                dgShlyuz.DataSource = xDS;
                bsSh.ResetBindings(false);
                dgShlyuz.ResumeLayout();
            }
        }



        // Тестовое заполнение
        //public void FakeEx()
        //{
        //    xNSI.DT[NSI.BD_SOTG].dt.BeginInit();
        //    DataRow dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();
        //    dr["KSMEN"] = xPark.NSm;
        //    dr["SYSN"]  = 12;
        //    dr["DTP"]  = DateTime.Now;
        //    dr["NSH"]  = 0;
        //    dr["KEKS"]  = 0;
        //    dr["KAVT"]  = "АЕ2535-1";
        //    dr["ROUTE"] = "Мстиславль, Кричев";
        //    xNSI.DT[NSI.BD_SOTG].dt.Rows.Add(dr);
        //    dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();

        //    dr["KSMEN"] = xPark.NSm;
        //    dr["SYSN"] = 13;
        //    dr["DTP"] = DateTime.Now; ;
        //    dr["NSH"] = 0;
        //    dr["KEKS"] = 0;
        //    dr["KAVT"] = "4567KJ";
        //    dr["ROUTE"] = "Маркет Лайн, Милком";
        //    xNSI.DT[NSI.BD_SOTG].dt.Rows.Add(dr);
        //    dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();

        //    dr["KSMEN"] = xPark.NSm;
        //    dr["SYSN"] = 14;
        //    dr["DTP"] = DateTime.Now;
        //    dr["NSH"] = 0;
        //    dr["KEKS"] = 0;
        //    dr["KAVT"] = "АЕ2544-8";
        //    dr["ROUTE"] = "Сморгонь,Ошмяны,Ивье,Островец";
        //    xNSI.DT[NSI.BD_SOTG].dt.Rows.Add(dr);
        //    dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();

        //    xNSI.DT[NSI.BD_SOTG].dt.EndInit();
        //    //Srv.ErrorMsg("Loaded!");
        //}



        // Записать отметку прибытия/убытия
        private void SaveAvtoPark()
        {
            int nSys = 0,
                nId = 0,
                nRet = AppC.RC_OK;
            string sAvt = "",
                sPar,
                sTime,
                sErr;
            DataRow dr = null;

            xSE = new MainF.ServerExchange(xMainF);
            if (xPark.LastOper == xPark.ParkIO)
            {
                Srv.ErrorMsg("Уже выполнялось!", true);
                return;
            }


            sTime = DateTime.Now.ToString("HH:mm:ss"); 
            if (bsSh.Count > 0)
            {
                dr = ((DataRowView)bsSh.Current).Row;
                nSys = (int)dr["SYSN"];
                sAvt = (string)dr["KAVT"];
                nId  = (int)dr["ID"];
            }
            else
            {
                nSys = 0;
                sAvt = tAvto.Text;
            }

            sPar = String.Format("(IO={0},NSH={1},KAVT={2},SYSN={3},KSK={4}", xPark.ParkIO, xPark.NShl, sAvt, nSys, xMainF.xSm.nSklad);
            if (xPark.NSm > 0)
                sPar += ",KSMEN=" + xPark.NSm.ToString();
            if (xPark.NPtvList > 0)
                sPar += ",ND=" + xPark.NPtvList.ToString();
            sPar += ")";

            //Cursor crsOld = Cursor.Current;
            //Cursor.Current = Cursors.WaitCursor;
            sErr = xSE.ExchgSrv(AppC.COM_VOTG, sPar, "", null, null, ref nRet);
            //FakeExW();
            //Cursor.Current = crsOld;

            xPark.SaveOldPars();
            if (nRet == AppC.RC_OK)
            {
                Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                if (!dgShlyuz.Focused)
                    dgShlyuz.Focus();
                tAvto.Enabled = false;
                xPark.LastOper = xPark.ParkIO;
                if (xPark.ParkIO == IO_COME)
                {// зафиксировали прибытие
                    if (dr != null)
                    {
                        xPark.DRCome = dvAvto.Table.NewRow();
                        xPark.DRCome.ItemArray = (object[])dr.ItemArray.Clone();
                        xPark.DRCome["NSH"] = xPark.NShl;
                        xPark.DRCome["KSMEN"] = xPark.NSm;
                        xPark.DRCome["ND"] = xPark.NPtvList;
                        xPark.DRCome["DTP"] = sTime;
                        xPark.DRCome["ID"] = dr["ID"];
                    }
                    tIn.Text = "Прибыл " + sTime;
                    IOChange(IO_GOUT);
                    IsParkAvail();
                    bsSh.Filter = string.Format("ID={0}", nId);
                    bsSh.ResetBindings(false);
                }
                else
                {// зафиксировали убытие
                    //dvAvto.Table.Rows.Clear();
                    if (dr != null)
                        dr["DTU"] = sTime;
                    tOut.Text = "Убыл    " + sTime;
                }
                this.Close();
            }
            else
            {
                Srv.ErrorMsg(sErr, true);
            }
            //FakeExW();

        }



        private void tAvto_TextChanged(object sender, EventArgs e)
        {
            if ((bEditMode == false) && (tAvto.Focused))
            {
                string sF = tAvto.Text;
                if (sF.Length > 0)
                {
                    sF = String.Format("[KAVT] LIKE '%{0}%'", sF);
                    bsSh.Filter = sF;
                }
                else
                    bsSh.RemoveFilter();
                bsSh.ResetBindings(false);
                IsParkAvail();
            }
        }


        private void dgShlyuz_GotFocus(object sender, EventArgs e)
        {
            if (bEditMode == false)
            {
                if (tAvto.Enabled == true)
                {
                    if (tAvto.DataBindings.Count == 0)
                    {
                        tAvto.DataBindings.Add("Text", bsSh, "KAVT");
                        tAvto.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.Never;
                    }
                }
                IsParkAvail();
            }
        }


        private void dgShlyuz_LostFocus(object sender, EventArgs e)
        {
            if (tAvto.Enabled)
                tAvto.Focus();
        }

        // Выгрузка на сервер (при готовности) сведений прибытия/убытия
        private void TryFixAvto()
        {
            int 
                nId = 0,
                nRet = AppC.RC_OK;
            string 
                sPar,
                sTime,
                sErr;
            DataRow dr = null;

            bool bReady = false;
            xSE = new MainF.ServerExchange(xMainF);


            sPar = String.Format("(IO={0},KSK={1}", xPark.ParkIO, xMainF.xSm.nSklad);

            if (xPark.RegFix == AppC.FX_PRPSK)
            {// фиксация по пропускам
                if (xPark.ParkIO == IO_COME)
                {// режим прибытия
                    if (xPark.Propusk.Length > 0)
                    {// пропуск отсканирован
                        if (xPark.ShlAdr.Length > 0)
                        {// адрес шлюза обязателен
                            bReady = true;
                            sPar = String.Format(sPar + ",PRPSK={0},ADRCELL={1}", xPark.Propusk, xPark.ShlAdr);
                        }
                    }
                }
                else
                {// режим освобождения шлюза
                    if (xPark.ShlAdr.Length > 0)
                    {// адрес шлюза обязателен
                        sPar = String.Format(sPar + ",ADRCELL={0}", xPark.ShlAdr);
                        if (xPark.Propusk.Length > 0)
                        {// пропуск отсканирован (пока тоже обязателен)
                            bReady = true;
                            sPar = String.Format(sPar + ",PRPSK={0}", xPark.Propusk);
                        }
                    }
                }
            }
            else
            {// фиксация по путевым листам
            }

            if (bReady)
            {// в принципе, можно попробовать выгрузить
                if (xPark.NSm > 0)
                    sPar = String.Format(sPar + ",KSMEN={0}", xPark.NSm);
                if (xPark.NPtvList > 0)
                    sPar = String.Format(sPar + ",ND={0}", xPark.NPtvList);
                sPar += ")";

                //Cursor crsOld = Cursor.Current;
                //Cursor.Current = Cursors.WaitCursor;
                sErr = xSE.ExchgSrv(AppC.COM_ZPRP, sPar, "", null, null, ref nRet, 20);
                //FakeExW();
                //Cursor.Current = crsOld;

                xPark.SaveOldPars();
                if (xSE.ServerRet == AppC.RC_OK)
                {
                    if (!dgShlyuz.Focused)
                        dgShlyuz.Focus();
                    tAvto.Enabled = false;
                    xPark.LastOper = xPark.ParkIO;
                    sTime = DateTime.Now.ToShortTimeString();
                    if (xPark.ParkIO == IO_COME)
                    {// зафиксировали прибытие
                        tIn.SuspendLayout();
                        tIn.BackColor = colCome;
                        tIn.Text = "Прибыл " + sTime;
                        IOChange(IO_GOUT);
                        IsParkAvail();
                        bsSh.Filter = string.Format("ID={0}", nId);
                        bsSh.ResetBindings(false);
                        tIn.ResumeLayout();
                    }
                    else
                    {// зафиксировали убытие
                        //dvAvto.Table.Rows.Clear();
                        tOut.SuspendLayout();
                        tOut.BackColor = colGOut;
                        if (dr != null)
                            dr["DTU"] = sTime;
                        tOut.Text = "Убыл    " + sTime;
                        tOut.ResumeLayout();
                    }
                    Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                    Srv.ErrorMsg(sErr, "Зафиксирован...", false);
                    this.Close();
                }
                else
                {
                    Srv.PlayMelody(W32.MB_4HIGH_FLY);
                    Srv.ErrorMsg(sErr);
                    if (xSE.ServerRet != AppC.EMPTY_INT)
                        // код возврата определен, сервер возражает
                        this.Close();
                }

            }
        }

        // для передачи параметров в форму
        private void Shlyuz_Activated(object sender, EventArgs e)
        {
            if (this.Tag != null)
            {
                AfterConstruct((MainF)this.Tag);
                this.Tag = null;
            }
        }

    }
}