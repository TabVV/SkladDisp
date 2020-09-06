using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;

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
        class MarkPall
        {

            // источник сведений о содержимом поддона
            public NSI.SRCDET
                SrcInfo = NSI.SRCDET.HANDS;

            public int
                NumberOfScans = 0;

            public string
                WrapFlag = "",
                SSCC = "";

            public PSC_Types.ScDat
                //ScanSecond;
                ScanFirst;
        }

        private const int CMD_SIGHN = 8;

        private string
            sDevID = "";            // номер ID-точки
        private int
            nPal4Doc = 0;           // паллет отправлено для текущего документа
        private DataRow
            drWithSSCC = null;
        private AppC.WRAP_MODES
            enWrapMode;

        private MainF
            xMF;
        private NSI
            xNSI;

        private BarcodeScanner.BarcodeScanEventHandler
            ehOldScan;

        private bool
            //bSetByHand = false,
            bAutoMark = false,
            //bAskWrapMandatory = false,        // обязательный запрос о срейчевании
            bEditMode = false,
            bSkipKey = false;               // не обрабатывать введенный символ

        private AppC.EditListC
            aEd;

        private Srv.HelpShow 
            xHelpS;
        
        // обработчик клавиш для текущей функции
        Srv.CurrFuncKeyHandler 
            ehCurrFunc = null;
        
        // экран помощи
        private List<string>
            lstHelp = new List<string>();

        // словарь флагов стрейчевания
        private Dictionary<string, string>
            dicWrap = new Dictionary<string, string>();



        private MarkPall
            xMark = null;

        private PDA.OS.BATT_INF
            xBattInf;

        public NPodd2Sscc()
        {
            InitializeComponent();
        }

        private void NPodd2Sscc_Activated(object sender, EventArgs e)
        {
            if (this.Tag != null)
            {
                AfterConstruct(this.Tag);
                this.Tag = null;
            }
        }

        // разбор параметров от главной формы
        private void AfterConstruct(object xx)
        {
            object[]
                x = (object[])xx;

            try
            {
                xMF = (MainF)(x[0]);
                bAutoMark = (bool)(x[1]);
            }
            catch { }

            xNSI = xMF.xNSI;

            //ehOldScan = new BarcodeScanner.BarcodeScanEventHandler(OnScanPL);
            ehOldScan = new BarcodeScanner.BarcodeScanEventHandler(OnScan);
            xMF.xBCScanner.BarcodeScan += ehOldScan;
            xHelpS = new Srv.HelpShow(this);
            lstHelp = new List<string>(new string[] { 
                xMF.xFuncs.TryGetFuncKeys(AppC.F_UPLD_DOC).PadRight(CMD_SIGHN ) +    " - выгрузка сведений", 
                xMF.xFuncs.TryGetFuncKeys(AppC.F_CHG_REC).PadRight(CMD_SIGHN ) +     " - новая паллетта" ,
                "->,<-".PadRight(CMD_SIGHN )                                 +       " - смена емкости",
                xMF.xFuncs.TryGetFuncKeys(AppC.F_EASYEDIT).PadRight(CMD_SIGHN ) +    " - новый документ",
                xMF.xFuncs.TryGetFuncKeys(AppC.F_LOAD_DOC).PadRight(CMD_SIGHN ) +    " - стрейчевание",
                xMF.xFuncs.TryGetFuncKeys(AppC.F_A4MOVE).PadRight(CMD_SIGHN )   +    " - режим стрейч",
                xMF.xFuncs.TryGetFuncKeys(AppC.F_KMCINF).PadRight(CMD_SIGHN ) +      " - без WMS",
                xMF.xFuncs.TryGetFuncKeys(AppC.F_QUIT).PadRight(CMD_SIGHN ) +        " - выход",
                xMF.xFuncs.TryGetFuncKeys(AppC.F_MARKWMS).PadRight(CMD_SIGHN ) +     " - возврат в главную" 
            });
            //System.Reflection.AssemblyName xAN = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            //string sV = xAN.Version.ToString();
            //int nPoint = sV.LastIndexOf(".");
            //lHelp.Text = String.Format("<F1> - помощь           v{0}", sV.Substring(nPoint));
            xMF.CheckNSIState(false);

            this.Tag = null;

            // номер ID-точки
            tDevID.Text = sDevID = Smena.EnterPointID;

            chNewDoc.Checked = (sDevID.Length > 0)?false:true;

            enWrapMode = xMF.xPars.WrapMode;
            SetWrapp();

            if (xMF.xPars.CanEditIDNum)
            {
                tDevID.Focus();
            }
            else
            {
                tDevID.Enabled = false;
                if (sDevID.Length > 0)
                    MainCycleStart(sDevID, false);
            }
        }

        private void NPodd2Sscc_Closing(object sender, CancelEventArgs e)
        {
            if (ehOldScan != null)
                xMF.xBCScanner.BarcodeScan -= ehOldScan;
            if (sDevID.Length > 0)
                Smena.EnterPointID = sDevID;
        }

        // Начало редактирования
        public void BeginEditB(bool b1stRun)
        {
            drWithSSCC = null;

            tVes.Visible = false;

            //chNewDoc.Checked = b1stRun && (!bAutoMark);

            aEd = new AppC.EditListC(new AppC.VerifyEditFields(VerifyB));
            aEd.AddC(tKrKMC);
            aEd.AddC(tEmk, false);
            aEd.AddC(tParty, false);
            aEd.AddC(tDTV, false);
            aEd.AddC(tMest, false);
            aEd.AddC(tVsego, false);
            aEd.AddC(tVes, false);
            aEd.AddC(tSSCC);
            xMark = new MarkPall();
            SetDetFields(ref xMark.ScanFirst, true);

            bEditMode = true;
            aEd.SetCur(aEd[0]);
        }

        // Корректность введенного
        private AppC.VerRet VerifyB()
        {
            int
                n = 0;
            AppC.VerRet
                v;

            v.nRet = AppC.RC_CANCEL;
            v.cWhereFocus = null;
            try
            {
                if (xMark.ScanFirst.sKMC.Length > 0)
                {
                    if (xMark.ScanFirst.nMest > 0)
                    {
                        if (xMark.ScanFirst.nParty.Length > 0)
                        {
                            if (xMark.ScanFirst.dDataIzg != DateTime.MinValue)
                                v.nRet = AppC.RC_OK;
                            else
                            {
                                Srv.ErrorMsg("Введите дату");
                                v.cWhereFocus = tDTV;
                            }
                        }
                        else
                        {
                            Srv.ErrorMsg("Введите партию");
                            v.cWhereFocus = tParty;
                        }
                    }
                    else
                    {
                        Srv.ErrorMsg("Нулевое количество");
                        v.cWhereFocus = tMest;
                    }
                }
            }
            catch
            {
                n = 0;
            }

            return (v);
        }


        // Завершение редактирования
        public void EndEditB(bool bGlobalQuit)
        {
            EndEditB(bGlobalQuit, "");
        }

        // Завершение редактирования
        public void EndEditB(bool bGlobalQuit, string sSSCC)
        {
            DialogResult
                DiaRes = DialogResult.Abort;

            if (sDevID.Length > 0)
            {
                if ((bAutoMark)&&(sSSCC.Length > 0))
                {// SSCC сформирован и его надо вернуть
                    this.Tag = new object[] { sSSCC, xMark.ScanFirst, drWithSSCC };
                    DiaRes = DialogResult.OK;
                }
                else
                {

                    DialogResult dr = MessageBox.Show(" Выход ?  (Enter)\nпродолжить работу (ESC)",
                        "Завершение работы", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (dr == DialogResult.OK)
                    {
                        DiaRes = (bGlobalQuit && (xMF.xSm.RegApp == AppC.REG_MARK)) ? DialogResult.Abort : DialogResult.OK;
                        bEditMode = false;
                        aEd.EditIsOver();
                    }
                    else
                        DiaRes = DialogResult.Cancel;
                }
            }
            this.DialogResult = DiaRes;
        }




        // Обработка клавиш
        private void NPodd2Sscc_KeyDown(object sender, KeyEventArgs e)
        {
            int nFunc = 0;
            bool 
                ret = false;// клавиша еще не обработана

            bSkipKey = false;

            nFunc = xMF.xFuncs.TryGetFunc(e);

            try
            {
                if (ehCurrFunc != null)
                {// клавиши ловит одна из функций
                    ret = ehCurrFunc(nFunc, e, ref ehCurrFunc);
                }
            }
            catch
            {
                Srv.ErrorMsg("Ошибка обработки", true);
                ret = true;
            }

            if (sDevID.Length == 0)
            {
                if ((nFunc == AppC.F_QUIT) ||
                    (nFunc == AppC.F_MARKWMS) ||
                    (nFunc == AppC.F_HELP))
                {
                }
                else
                {
                    if ((nFunc <= 0) && (e.KeyValue == W32.VK_ENTER))
                    {
                        chMsg2WMS.Focus();
                    }
                    return;
                }
            }

            if ((nFunc > 0) && (ret == false))
            {//в режиме просмотра
                ret = true;
                switch (nFunc)
                {
                    case AppC.F_HELP:
                        // вывод панели c окном помощи
                        xHelpS.ShowInfo(lstHelp, ref ehCurrFunc);
                        break;
                    case AppC.F_UPLD_DOC:
                        // повторная выгрузка
                        if (tSSCC.Text.Length == 20)
                        {
                            if (tSSCC.Text != xMark.SSCC)
                                xMark.SSCC = tSSCC.Text;
                            TrySend2Serv(xMark.SSCC);
                        }
                        break;
                    case AppC.F_CHG_REC:
                        // новый поддон
                        BeginEditB(false);
                        break;
                    case AppC.F_KMCINF:
                        // снять/установить флаг телеграмм для WMS
                        chMsg2WMS.Checked = !chMsg2WMS.Checked;
                        break;
                    case AppC.F_EASYEDIT:
                        // снять/установить флаг нового документа
                        chNewDoc.Checked = !chNewDoc.Checked;
                        break;
                    case AppC.F_LOAD_DOC:
                        // снять/установить флаг стрейчевания
                        chWrapp.Checked = !chWrapp.Checked;
                        break;
                    case AppC.F_A4MOVE:
                        // снять/установить флаг запроса о стрейчевании
                        //bAskWrapMandatory = !bAskWrapMandatory;
                        //Srv.ErrorMsg("Запрос о стрейче\n" + ((bAskWrapMandatory)?"включен":"выключен") + "...", "Смена параметра", true);

                        enWrapMode.SwitchNext();
                        Srv.ErrorMsg("Стрейчевание паллеты:\n" + enWrapMode.ToString(), "Смена параметра", true);
                        SetWrapp();
                        break;
                    case AppC.F_QUIT:
                        EndEditB(true);
                        break;
                    case AppC.F_MARKWMS:
                        EndEditB(false);
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
                    case W32.VK_UP:
                        aEd.TryNext(AppC.CC_PREV);
                        break;
                    case W32.VK_DOWN:
                        aEd.TryNext(AppC.CC_NEXT);
                        break;
                    case W32.VK_ENTER:
                        aEd.TryNext(AppC.CC_NEXTOVER);
                        break;
                    case W32.VK_ESC:
                        //EndEditB(false);
                        break;


                    case W32.VK_LEFT:
                    case W32.VK_RIGHT:
                        if (
                            (((aEd.Current == tEmk) || (aEd.Current == tParty) || tVsego.Enabled) 
                            && (xMark.ScanFirst.xEmks.Count > 1)
                            && (aEd.Current != tSSCC))
                            )
                        {
                            StrAndInt xS = null;
                            if (xMark.ScanFirst.xEmks.Current == null)
                                xS = xMark.ScanFirst.xEmks.MoveEx(Srv.Collect4Show<StrAndInt>.DIR_MOVE.FORWARD);
                            else
                            {
                                if (e.KeyValue == W32.VK_LEFT)
                                    xS = xMark.ScanFirst.xEmks.MoveEx(Srv.Collect4Show<StrAndInt>.DIR_MOVE.BACK);
                                else
                                    xS = xMark.ScanFirst.xEmks.MoveEx(Srv.Collect4Show<StrAndInt>.DIR_MOVE.FORWARD);
                            }
                            //if (scCur.xEmks.Current != null)
                            //{
                            xMark.ScanFirst.fEmk = xS.DecDat;
                            xMark.ScanFirst.nTara = xS.SNameAdd1;
                            xMark.ScanFirst.nKolSht = xS.IntCodeAdd1;

                            //xMark.ScanFirst.nMestPal = xMark.ScanFirst.nMest = xS.IntCode;

                            //tEmk.Text = xMark.ScanFirst.fEmk.ToString();
                            EvalTot(xMark.ScanFirst.nMest);
                            this.tEmk.Text = ((xMark.ScanFirst.bVes) ? xMark.ScanFirst.nKolSht : (int)xMark.ScanFirst.fEmk).ToString();

                            //tVsego.Text = xMark.ScanFirst.fVsego.ToString();
                            //}
                            ret = true;
                        }
                        break;




                    default:
                        ret = false;
                        break;
                }
            }
            e.Handled = bSkipKey = ret;
        }

        private void NPodd2Sscc_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipKey == true)
            {
                bSkipKey = false;
                e.Handled = true;
            }
        }

        // ввод номера точки
        private void tDevID_Validating(object sender, CancelEventArgs e)
        {
            if (bInScanProceed)
                return;

            string sT = ((TextBox)sender).Text.Trim();
                        
            e.Cancel = true;
            if (sT.Length > 0)
            {
                try
                {
                    int n = int.Parse(sT);
                    if ((n >= 1) && (n <= 99))
                    {
                        e.Cancel = false;
                    }
                }
                catch
                {
                }
            }
            if (!e.Cancel)
            {
                lHelp.TextAlign = ContentAlignment.TopRight;
                MainCycleStart(sT, true);
            }

        }

        private void MainCycleStart(string sID, bool b1stRun)
        {
            sDevID = sID;
            //lHelp.Text = GetDopInf();

            xBattInf = new BATT_INF(this, new Size(240, 21), new Point(0, 299));
            xBattInf.BIUserText = GetDopInf();
            //xBattInf.SetBIFont(22, FontStyle.Regular);
            xBattInf.BIFont = 10;
            xBattInf.SetLevels(new object[][] { 
                new object[]{ 5, "Опасно", Color.Red, Color.White },
                new object[]{ 10, "Низкий", Color.Olive, Color.White}, 
                new object[]{ 45, "Средний", Color.MediumBlue, Color.White}, 
                new object[]{ 75, "Высокий", Color.SteelBlue, Color.Black},
                new object[]{ 100, "Отличный", Color.SkyBlue, Color.Black} });

            xBattInf.EnableShow = true;

            tDevID.Visible = false;
            BeginEditB(b1stRun);
        }

        private void chWrapp_CheckStateChanged(object sender, EventArgs e)
        {
            if (xMark != null)
                xMark.WrapFlag = (chWrapp.Checked) ? "Y" : "N";
        }

        private void SetWrapp()
        {
            switch (enWrapMode.CurMode)
            {
                case AppC.WRAP_MODES.WRP_ALW_SET:
                    chWrapp.Enabled = false;
                    chWrapp.Checked = true;
                    break;
                case AppC.WRAP_MODES.WRP_ALW_RESET:
                    chWrapp.Enabled = false;
                    chWrapp.Checked = false;
                    break;
                default:
                    chWrapp.Enabled = true;
                    break;
            }
        }



    }
}