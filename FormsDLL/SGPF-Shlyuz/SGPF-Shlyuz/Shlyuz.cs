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
        // ��������/������
        private const int 
            IO_COME = 1,
            IO_GOUT = 2;

        // ������� ����� �������� (�� �������� ��� ��)
        private int 
            nRegFixing;

        private bool
            bEditMode = false,
            bSkipKey = false;           // �� ������������ ��������� ������
        
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

        // ����� ��� ��������
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


            // ������� ����� ��������
            private int m_nRegFix;

            public ParkA(int nR)
            {
                RegFix = nR;
            }


            // ��� ��������
            public int ParkIO
            {
                get { return m_Dir; }
                set { m_Dir = value; }
            }
            // � �����
            public int NShl
            {
                get { return m_Shl; }
                set { m_Shl = value; }
            }
            // � ���������
            public int NPtvList
            {
                get { return m_Doc; }
                set { m_Doc = value; }
            }
            // � �����
            public int NSm
            {
                get { return m_Sm; }
                set { m_Sm = value; }
            }

            // ��� ��������� �������� ��������
            public int LastOper
            {
                get { return m_LastOp; }
                set { m_LastOp = value; }
            }

            // ID ������ ���������� ����
            public DataRow DRCome
            {
                get { return m_DrCome; }
                set { m_DrCome = value; }
            }

            // � �������� ���������� ����
            public string Propusk
            {
                get { return m_NPropusk; }
                set { m_NPropusk = value; }
            }

            // ����� �����
            public string ShlAdr
            {
                get { return m_ShlAdr; }
                set { m_ShlAdr = value; }
            }

            // ������� ����� ��������
            public int RegFix
            {
                get { return m_nRegFix; }
                set { m_nRegFix = value; }
            }


            // ��������� ������ ���� ��� �������� ��������?
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
            // ������ ��� ��� ��-�����
            xMainF.aAvtoPark = null;
            nRegFixing = (int)xMainF.xDLLPars;

            xPark = new ParkA(nRegFixing);
            dvAvto = xNSI.DT[NSI.BD_SOTG].dt.DefaultView;

            SetBindShlyuz();

            // ��������� �����
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
            {// �� ��������� - ��������
                //lHeadP.SuspendLayout();
                lHeadP.BackColor = colCome;
                //lHeadP.ResumeLayout();
                if (nRegFixing == AppC.FX_PTLST)
                    BeginEditP(null);
            }
            else
            {// ��������� ���� �������� ��������
                xPark.DRCome = dvAvto.Table.NewRow();
                xPark.DRCome.ItemArray = xMainF.aAvtoPark;
                bsSh.Filter = string.Format("ID={0}", xPark.DRCome["ID"]);
                bsSh.ResetBindings(false);
                dgShlyuz.Focus();
                // ������ ������������ ������
                xPark.NShl = (int)xPark.DRCome["NSH"];
                xPark.NPtvList = (int)xPark.DRCome["ND"];
                xPark.NSm = int.Parse((string)xPark.DRCome["KSMEN"]);
                tNDoc.DataBindings[0].ReadValue();
                tSm.DataBindings[0].ReadValue();
                tShlAddr.DataBindings[0].ReadValue();
                tIn.Text = "������  " + (string)xPark.DRCome["DTP"];
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
            {// ������������� ��������
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
                    {// ������ �� ����� (� �����)
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
                                // ������ �� � ��������
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
                                {// ������ �� � ��������
                                    xPark.Propusk = e.Data.Substring(3);
                                    tPropusk.Text = e.Data.Substring(3);
                                    TryFixAvto();
                                }
                                //else if (e.Data.Substring(0, 2) == "99")
                                //{// ������ �� ����� (� �����)
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


        // �������� � ������
        private void SetBindShlyuz()
        {
            bsSh = new BindingSource();
            bsSh.DataSource = dvAvto;


            // � ��������
            tPropusk.DataBindings.Add("Text", xPark, "Propusk");
            tPropusk.DataBindings[0].DataSourceNullValue = "";
            tPropusk.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            // ����� �����
            tShlAddr.DataBindings.Add("Text", xPark, "ShlAdr");
            tShlAddr.DataBindings[0].DataSourceNullValue = 0;
            tShlAddr.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            // � �����
            tShNomPP.DataBindings.Add("Text", xPark, "NShl");
            tShNomPP.DataBindings[0].DataSourceNullValue = 0;
            tShNomPP.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            // � �����
            tSm.DataBindings.Add("Text", xPark, "NSm");
            tSm.DataBindings[0].DataSourceNullValue = 0;

            // � ���������
            tNDoc.DataBindings.Add("Text", xPark, "NPtvList");
            tNDoc.DataBindings[0].DataSourceNullValue = 0;

            // ���� ��������
            lDTCome.DataBindings.Add("Text", bsSh, "DTP");
            lDTCome.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.Never;

            // ���� ������
            lDTOut.DataBindings.Add("Text", bsSh, "DTU");
            lDTOut.DataBindings[0].DataSourceUpdateMode = DataSourceUpdateMode.Never;
        }

        // ��������� ����� ���� ��� ����� (��������� ����)
        private void SelAllTextF(object sender, EventArgs e)
        {
            TextBox xT = (TextBox)sender;
            xT.SelectAll();
            if (xT.Equals(tAvto))
            {
                tAvto.DataBindings.Clear();
            }
        }

        // ����� ������� ����
        private void ShlzStyle(DataGrid dg)
        {
            ServClass.DGTBoxColorColumn sC;
            System.Drawing.Color colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = NSI.BD_SOTG;

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "KAVT";
            sC.HeaderText = "  � ����";
            sC.Width = 72;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "ROUTE";
            sC.HeaderText = "      �������";
            sC.Width = 130;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "KSMEN";
            sC.HeaderText = "��";
            sC.Width = 30;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_SOTG);
            sC.MappingName = "NSH";
            sC.HeaderText = "����";
            sC.Width = 40;
            ts.GridColumnStyles.Add(sC);
            dg.TableStyles.Add(ts);
        }

        // �������� ����������� ��������
        private bool IsParkAvail()
        {
            bool ret = false;
            if (xPark.ParkIO == IO_COME)
            {// ��� ���������� ��������
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
            {// ��� ���������� ������
                if (bsSh.Count > 0)
                {
                    if ((dgShlyuz.Focused || (bsSh.Count == 1)))
                        ret = true;
                }
                tOut.Enabled = ret;
            }
            return (ret);
        }

        // ��������� ������
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
                        // �������� ������ ������
                        LoadAvtoList();
                        tAvto.Enabled = true;
                        IsParkAvail();
                        tAvto.Focus();
                        //tAvto_TextChanged(tAvto, new EventArgs());
                        break;
                    case AppC.F_UPLD_DOC:
                        // ��������� ��������
                        TryFixAvto();
                        break;
                }
                if (bEditMode == false)
                {// ������ � ������ ���������
                    switch (nFunc)
                    {
                        case AppC.F_ADD_REC:
                            // ����� ��������
                            if (xPark.ParkIO == IO_GOUT)
                                IOChange(0);
                            BeginEditP(null);
                            break;
                        case AppC.F_GOFIRST:
                        case AppC.F_GOLAST:
                            // 1-�/���������
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
                        {// � ������ ��������� (����� ����)
                            #region � ������ ���������
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
                                        Srv.ErrorMsg("�������� �����!", true);
                                    break;
                                default:
                                    ret = false;
                                    break;
                            }
                            #endregion
                        }
                        else
                        {// � ������ ��������������
                            #region � ������ ��������������
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

        // ������ ��������������
        public void BeginEditP(Control xC)
        {
            bool
                bEn = false;

            tIn.Text  = "������   <ENT>";
            tOut.Text = "����      <ENT>";
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

        // ������������ ����������
        private AppC.VerRet VerifyB()
        {
            AppC.VerRet v;
            v.nRet = AppC.RC_CANCEL;
            if (xPark.NShl > 0)
            {// ����� ���������
                v.nRet = AppC.RC_OK; ;
            }
            v.cWhereFocus = null;
            return (v);
        }

        // ���������� ��������������
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

        // ����� ��������
        private void IOChange(int Needed)
        {
            xPark.ParkIO = (Needed == 0) ? ((xPark.ParkIO == 1) ? 2 : 1) : Needed;
            lHeadP.SuspendLayout();
            if (xPark.ParkIO == IO_GOUT)
            {
                lHeadP.Text = "<=<   ������   >=>";
                lHeadP.BackColor = colGOut;

            }
            else
            {// ��������
                lHeadP.Text = " >=>   ��������   <=<";
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
                {// �������� ����
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
                sErr = "������ ������ XML";
                string sXMLFile = "";

                //int nFileSize = ServClass.ReadXMLWrite2File(stmX.SStream, ref sXMLFile);
                stmX.ASReadS.TermDat = AppC.baTermMsg;
                if (stmX.ASReadS.BeginARead(true, 1000 * 60) == SocketStream.ASRWERROR.RET_FULLMSG)
                    sXMLFile = stmX.ASReadS.OutFile;
                else
                    throw new System.Net.Sockets.SocketException(10061);

                sErr = "������ �������� XML";
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



        // �������� ����������
        //public void FakeEx()
        //{
        //    xNSI.DT[NSI.BD_SOTG].dt.BeginInit();
        //    DataRow dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();
        //    dr["KSMEN"] = xPark.NSm;
        //    dr["SYSN"]  = 12;
        //    dr["DTP"]  = DateTime.Now;
        //    dr["NSH"]  = 0;
        //    dr["KEKS"]  = 0;
        //    dr["KAVT"]  = "��2535-1";
        //    dr["ROUTE"] = "����������, ������";
        //    xNSI.DT[NSI.BD_SOTG].dt.Rows.Add(dr);
        //    dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();

        //    dr["KSMEN"] = xPark.NSm;
        //    dr["SYSN"] = 13;
        //    dr["DTP"] = DateTime.Now; ;
        //    dr["NSH"] = 0;
        //    dr["KEKS"] = 0;
        //    dr["KAVT"] = "4567KJ";
        //    dr["ROUTE"] = "������ ����, ������";
        //    xNSI.DT[NSI.BD_SOTG].dt.Rows.Add(dr);
        //    dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();

        //    dr["KSMEN"] = xPark.NSm;
        //    dr["SYSN"] = 14;
        //    dr["DTP"] = DateTime.Now;
        //    dr["NSH"] = 0;
        //    dr["KEKS"] = 0;
        //    dr["KAVT"] = "��2544-8";
        //    dr["ROUTE"] = "��������,������,����,��������";
        //    xNSI.DT[NSI.BD_SOTG].dt.Rows.Add(dr);
        //    dr = xNSI.DT[NSI.BD_SOTG].dt.NewRow();

        //    xNSI.DT[NSI.BD_SOTG].dt.EndInit();
        //    //Srv.ErrorMsg("Loaded!");
        //}



        // �������� ������� ��������/������
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
                Srv.ErrorMsg("��� �����������!", true);
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
                {// ������������� ��������
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
                    tIn.Text = "������ " + sTime;
                    IOChange(IO_GOUT);
                    IsParkAvail();
                    bsSh.Filter = string.Format("ID={0}", nId);
                    bsSh.ResetBindings(false);
                }
                else
                {// ������������� ������
                    //dvAvto.Table.Rows.Clear();
                    if (dr != null)
                        dr["DTU"] = sTime;
                    tOut.Text = "����    " + sTime;
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

        // �������� �� ������ (��� ����������) �������� ��������/������
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
            {// �������� �� ���������
                if (xPark.ParkIO == IO_COME)
                {// ����� ��������
                    if (xPark.Propusk.Length > 0)
                    {// ������� ������������
                        if (xPark.ShlAdr.Length > 0)
                        {// ����� ����� ����������
                            bReady = true;
                            sPar = String.Format(sPar + ",PRPSK={0},ADRCELL={1}", xPark.Propusk, xPark.ShlAdr);
                        }
                    }
                }
                else
                {// ����� ������������ �����
                    if (xPark.ShlAdr.Length > 0)
                    {// ����� ����� ����������
                        sPar = String.Format(sPar + ",ADRCELL={0}", xPark.ShlAdr);
                        if (xPark.Propusk.Length > 0)
                        {// ������� ������������ (���� ���� ����������)
                            bReady = true;
                            sPar = String.Format(sPar + ",PRPSK={0}", xPark.Propusk);
                        }
                    }
                }
            }
            else
            {// �������� �� ������� ������
            }

            if (bReady)
            {// � ��������, ����� ����������� ���������
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
                    {// ������������� ��������
                        tIn.SuspendLayout();
                        tIn.BackColor = colCome;
                        tIn.Text = "������ " + sTime;
                        IOChange(IO_GOUT);
                        IsParkAvail();
                        bsSh.Filter = string.Format("ID={0}", nId);
                        bsSh.ResetBindings(false);
                        tIn.ResumeLayout();
                    }
                    else
                    {// ������������� ������
                        //dvAvto.Table.Rows.Clear();
                        tOut.SuspendLayout();
                        tOut.BackColor = colGOut;
                        if (dr != null)
                            dr["DTU"] = sTime;
                        tOut.Text = "����    " + sTime;
                        tOut.ResumeLayout();
                    }
                    Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                    Srv.ErrorMsg(sErr, "������������...", false);
                    this.Close();
                }
                else
                {
                    Srv.PlayMelody(W32.MB_4HIGH_FLY);
                    Srv.ErrorMsg(sErr);
                    if (xSE.ServerRet != AppC.EMPTY_INT)
                        // ��� �������� ���������, ������ ���������
                        this.Close();
                }

            }
        }

        // ��� �������� ���������� � �����
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