using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;

using ScannerAll;
using PDA.OS;
using PDA.Service;


namespace SkladGP
{
    public partial class MainF : Form
    {
        //public event EventParsHandler ParsEvents;

        public delegate void EditParsOver(int nRetCode, int nFunc);
        EditParsOver dgOver;

        
        private DocPars
            xDP;                // текущие параметры (при редактировании)

        // текущая функция 
        private int nCurFunc;

        // значения типа документа до редактирования
        private int nTypDOld;

        // флаг работы с параметрами
        private bool bWorkWithDocPars = false;

        // флаг завершения ввода
        //private bool bQuitEdPars = false;
                
        private BarcodeScanner.BarcodeScanEventHandler ehParScan;

        // предыдущий обработчик клавиатуры
        Srv.CurrFuncKeyHandler oldKeyH;



        // с какого поля начать: первого доступного или первого пустого
        public enum CTRL1ST : int
        {
            START_AVAIL = 1,
            START_EMPTY = 2,
            START_LAST  = 3,
        }


        private void OnScanPar(object sender, BarcodeScannerEventArgs e)
        {
            if ((e.nID != BCId.NoData) && (bEditMode == true))
            {
                if ((e.nID == BCId.Code128) && (e.Data.Length == 14))
                {// Путевой лист или ТТН
                    if (tNom_p.Enabled)
                        tNom_p.Text = e.Data.Substring(7);
                }
            }
        }


        // вход в режим ввода/корректировки параметров
        public void EditPars(int nReg, DocPars x, CTRL1ST FirstEd, AppC.VerifyEditFields dgVer, EditParsOver dgEnd)
        {
            xDP = x;
            if (x != null)
            {
                //bQuitEdPars = false;
                nCurFunc = nReg;
                bWorkWithDocPars = true;
                //ServClass.dgVerEd = new AppC.VerifyEditFields(dgVer);
                dgOver = new EditParsOver(dgEnd);
                SetParFields(xDP);

                oldKeyH = ehCurrFunc;
                ehCurrFunc = new Srv.CurrFuncKeyHandler(PPars_KeyDown);
                ehParScan = new BarcodeScanner.BarcodeScanEventHandler(OnScanPar);
                xBCScanner.BarcodeScan -= ehScan;
                xBCScanner.BarcodeScan += ehParScan;

                BeginEditPars(FirstEd, dgVer);

            }
        }

        // установка времени прибытия
        private void SetTime2Load()
        {
            string
                sT = "";
            DateTime
                dtP;
            try
            {
                dtP = 
                DateTime.ParseExact((string)xCDoc.drCurRow["DTPRIB"], "dd.MM.yyyy HH:mm", null);
                sT = dtP.ToString("HH:mm");
                if (dtP.DayOfYear != DateTime.Now.DayOfYear)
                {
                    sT += String.Format("/{0}", dtP.Day);
                }
            }
            catch
            {
                sT = "";
            }
            lTime2Load.Text = sT;

        }

        // сброс/установка полей ввода/вывода
        private void SetParFields(DocPars xDP)
        {
            int 
                n = xDP.nTypD;
            string 
                sIS;

            DocPars.tKTyp.Text = (n == AppC.EMPTY_INT) ? "" : xDP.nTypD.ToString();
            sIS = DocPars.TypDName(n);
            DocPars.tNTyp.Text = (sIS.Length > 0) ? sIS : "<Неизвестный>";
            xDP.sTypD = DocPars.tNTyp.Text;

            tSm_p.Text = xDP.sSmena;
            tDateD_p.Text = DateTime.Now.ToString("dd.MM.yy");
            if (xDP.dDatDoc != DateTime.MinValue)
            {
                tDateD_p.Text = xDP.dDatDoc.ToString("dd.MM.yy");
            }
            tKSkl_p.Text = "";
            tNSkl_p.Text = "";
            if (xDP.nSklad != AppC.EMPTY_INT)
            {
                tKSkl_p.Text = xDP.nSklad.ToString();
                NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_SKLAD, new object[] { xDP.nSklad }, "NAME");
                if (zS.bFind == true)
                {
                    tNSkl_p.Text = zS.sName;
                    xDP.sSklad = zS.sName;
                }

            }
            tKUch_p.Text = "";
            if ((xDP.nUch != AppC.EMPTY_INT) && (xDP.nUch != 0))
            {
                tKUch_p.Text = xDP.nUch.ToString();
            }

            tNom_p.Text = xDP.sNomDoc;


            tKEks_p.Text = "";
            tNEks_p.Text = "";



            if (xDP.nEks != AppC.EMPTY_INT)
            {
                tKEks_p.Text = xDP.nEks.ToString();

                if (xDP.TypOper != AppC.TYPOP_KMPL)
                {
                    NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_EKS, new object[] { xDP.nEks }, "FIO");
                    if (zS.bFind == true)
                    {
                        tNEks_p.Text = zS.sName;
                        xDP.sEks = zS.sName;
                    }
                }
                else
                {
                    tNEks_p.Text = xDP.sEks;
                }
            }
            else
                xDP.sEks = "";


            tKPol_p.Text = "";
            tNPol_p.Text = "";
            if (xDP.nPol != AppC.EMPTY_INT)
            {
                tKPol_p.Text = xDP.nPol.ToString();
                if (xDP.nTypD == AppC.TYPD_OPR)
                {
                    sIS = DocPars.OPRName(ref xDP.nPol);
                }
                else if (xDP.TypOper != AppC.TYPOP_KMPL)
                {
                    NSI.RezSrch zS = xNSI.GetNameSPR((xDP.nTypD == AppC.TYPD_VPER) ?
                        NSI.NS_SKLAD : NSI.NS_PP, new object[] { xDP.nPol }, "NAME");
                    sIS = zS.sName;
                }
                else
                {
                    sIS = xDP.sPol;
                }
                tNPol_p.Text = sIS;
                xDP.sPol = sIS;

            }
            else
                xDP.sPol = "";
            SetTime2Load();
        }

        // куда поставить фокус ввода на панели
        private Control Where1Empty()
        {
            Control cRet = null;
            foreach(Control xC in aEdVvod)
                if (xC.Enabled && (xC.Text.Length == 0))
                {
                    cRet = xC;
                    break;
                }
            return (cRet);
        }

        // завершение режима ввода/корректировки параметров
        public void EndEditPars(int nKey)
        {
            int nRet = (nKey == W32.VK_ENTER) ? AppC.RC_OK : AppC.RC_CANCEL;
            ehCurrFunc -= PPars_KeyDown;
            ehCurrFunc = oldKeyH;
            xBCScanner.BarcodeScan -= ehParScan;
            xBCScanner.BarcodeScan += ehScan;

            aEdVvod.EditIsOver();

            bWorkWithDocPars = false;
            SetEditMode(false);
            dgOver(nRet, nCurFunc);
        }

        // проверка склада
        private void tKSkl_p_Validating(object sender, CancelEventArgs e)
        {
            string 
                sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    int nS = int.Parse(sT);
                    NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_SKLAD, new object[] { nS }, "NAME");
                    tNSkl_p.Text = zS.sName;
                    if (zS.bFind == false)
                        e.Cancel = true;
                    else
                    {
                        xDP.nSklad = nS;
                        xDP.sSklad = zS.sName;
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
            {
                xDP.nSklad = AppC.EMPTY_INT;
            }

            if ((true == e.Cancel) || (xDP.nSklad == AppC.EMPTY_INT))
            {
                Srv.PlayMelody(W32.MB_3GONG_EXCLAM);
                xDP.sSklad = "";
                xDP.nUch = AppC.EMPTY_INT;
                tKUch_p.Text = "";
            }
        }

        // проверка участка
        private void tKUch_p_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    int nS = int.Parse(tKSkl_p.Text),
                        nU = int.Parse(tKUch_p.Text);
                    NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_SUSK, new object[] { nS, nU }, "NAME");
                    if (zS.bFind == false)
                        e.Cancel = true;
                    else
                        xDP.nUch = nU;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                xDP.nUch = AppC.EMPTY_INT;
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }

        // проверка даты
        private void tDateD_p_Validating(object sender, CancelEventArgs e)
        {
            string sD = ((TextBox)sender).Text.Trim();
            if (sD.Length > 0)
            {
                try
                {
                    sD = Srv.SimpleDateTime(sD, Smena.DateDef);
                    DateTime d = DateTime.ParseExact(sD, "dd.MM.yy", null);
                    xDP.dDatDoc = d;
                    ((TextBox)sender).Text = sD;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                xDP.dDatDoc = DateTime.MinValue;
        }

        // проверка смены
        private void tSm_p_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                    NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_SMEN, new object[] { sT }, "NAME");
                    //02.05.11 !!! e.Cancel = !zS.bFind;
            }
            xDP.sSmena = sT;
        }

        // сохранение предыдущего значения типа документа
        private void SaveOldTyp(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
            nTypDOld = xDP.nTypD;
        }

        // изменение типа, вывод нименования
        private void tKT_p_TextChanged(object sender, EventArgs e)
        {
            if (bWorkWithDocPars == true)
            {// при просмотре не проверяется
                int nTD = AppC.EMPTY_INT;
                string s = "";

                try
                {
                    nTD = int.Parse(tKT_p.Text);
                    s = DocPars.TypDName(nTD);
                }
                catch { s = ""; }

                if (s.Length == 0)
                {
                    xDP.nTypD = AppC.EMPTY_INT;
                    s = "<Неизвестный>";
                }
                else
                {
                    xDP.nTypD = nTD;
                }

                tNT_p.Text = s;
            }
        }



        // проверка типа
        private void tKT_p_Validating(object sender, CancelEventArgs e)
        {
            if (xDP.nTypD == AppC.EMPTY_INT)
            {
                if (tKT_p.Text.Trim().Length > 0)
                    e.Cancel = true;
            }
            else
            {
                if (xDP.nTypD != nTypDOld)
                {// сменился тип документа
                }
            }

        }

        // тип документа все-таки сменился
        private void tKT_p_Validated(object sender, EventArgs e)
        {
            int i;
            if (xDP.nTypD != nTypDOld)
            {
                if (xDP.nTypD == AppC.EMPTY_INT)
                {
                    tKT_p.Text = "";
                    tNT_p.Text = "";
                    //e.Cancel = true;
                    //ServClass.TBColor((TextBox)sender, true);
                    for (i = 0; i < aEdVvod.Count; i++ )
                    {
                        aEdVvod[i].Enabled = true;
                    }
                }
                else
                {
                    bool bNomEn = true, bEksEn = true, bPolEn = true;
                    SetTypSensitive(xDP.nTypD, ref bEksEn, ref bPolEn, ref bNomEn);

                    tKEks_p.Enabled = bEksEn;
                    if (!bEksEn)
                        tKEks_p.Text = "";
                    tKPol_p.Enabled = bPolEn;
                    if (!bPolEn)
                        tKPol_p.Text = "";
                    tNom_p.Enabled = bNomEn;
                    if (!bNomEn)
                        tNom_p.Text = "";
                }
                //EnableDocF(xDP.nTypD, 2);
            }
            //ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }




        // проверка номера документа
        private void tNom_p_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length == 0)
            {
                if (xDP.nTypD == AppC.TYPD_VPER)
                {
                    //ServClass.ChangeEdArrDet(new Control[] { tKEks_p }, new Control[] { tNom_p }, aEdVvod);
                    tKEks_p.Enabled = false;
                    tNom_p.Enabled = true;
                }
            }
            xDP.sNomDoc = sT;
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }

        // проверка экспедитора
        private void tKEks_p_Validating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            if (sT.Length > 0)
            {
                try
                {
                    int nE = int.Parse(sT);
                    if (xDP.TypOper != AppC.TYPOP_KMPL)
                    {

                        NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_EKS, new object[] { nE }, "FIO");
                        tNEks_p.Text = zS.sName;
                        if (zS.bFind == false)
                            e.Cancel = true;
                        else
                        {
                            xDP.nEks = nE;
                            xDP.sEks = zS.sName;
                        }
                    }
                    else
                    {
                        xDP.nEks = nE;
                    }
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
            {
                xDP.nEks = AppC.EMPTY_INT;
                xDP.sEks = "";
                if (xDP.nTypD == AppC.TYPD_VPER)
                {
                    //ServClass.ChangeEdArrDet(new Control[] { tNom_p }, new Control[] { tKEks_p }, aEdVvod);
                    tNom_p.Enabled = true;
                    tKEks_p.Enabled = false;
                }

            }
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);

        }

        // проверка получателя
        private void tKPol_p_Validating(object sender, CancelEventArgs e)
        {
            NSI.RezSrch 
                zS;
            string 
                sT = ((TextBox)sender).Text.Trim(),
                sN = "";
            if (sT.Length > 0)
            {
                try
                {
                    int nK = int.Parse(sT);
                    if (xDP.nTypD == AppC.TYPD_OPR)
                    {
                        sN = DocPars.OPRName(ref nK);
                        xDP.TypOper = nK;
                    }
                    else if (xDP.TypOper != AppC.TYPOP_KMPL)
                    {
                        zS = xNSI.GetNameSPR((xDP.nTypD == AppC.TYPD_VPER) ?
                            NSI.NS_SKLAD : NSI.NS_PP, new object[] { nK }, "NAME");
                        sN = zS.sName;
                    }
                    else
                    {
                    }

                    tNPol_p.Text = sN;
                    tKPol_p.Text = sT;
                    xDP.nPol = nK;
                    xDP.sPol = sN;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
            {
                xDP.nPol = AppC.EMPTY_INT;
                xDP.sPol = "";
            }
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }



        // обработка функций и клавиш на панели
        private bool PPars_KeyDown(object nF, KeyEventArgs e, ref Srv.CurrFuncKeyHandler kh)
        {
            int nFunc = (int)nF;
            bool ret = true;

            if (nFunc > 0)
            {
                ret = false;
            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        EndEditPars(e.KeyValue);
                        break;
                    case W32.VK_UP:
                    case W32.VK_DOWN:
                        aEdVvod.TryNext((e.KeyValue == W32.VK_UP) ? AppC.CC_PREV : AppC.CC_NEXT);
                        break;
                    case W32.VK_ENTER:
                        bSkipChar = true;
                        if (aEdVvod.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                            //if (bQuitEdPars == true)
                            EndEditPars(e.KeyValue);
                        break;
                    case W32.VK_TAB:
                        aEdVvod.TryNext((e.Shift) ? AppC.CC_PREV : AppC.CC_NEXT);
                        ret = false;
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            e.Handled |= ret;
            return (ret);
        }





        // создание массива управления редактированием полей
        private void BeginEditPars(CTRL1ST FirstEd, AppC.VerifyEditFields dgV)
        {
            int i;
            bool 
                bSklU,
                bNomEn = true, 
                bEksEn = true,
                bPolEn = true;
            aEdVvod = new AppC.EditListC(dgV);

            //bSklU = (xSm.RegApp == AppC.REG_OPR) ? false : true;

            // для загрузки/выгрузки - доступно все для любых режимов
            bSklU = (((nCurFunc == AppC.F_ADD_REC)||(nCurFunc == AppC.F_CHG_REC)) && (xDP.nTypD == AppC.TYPD_OPR)) ? false : true;

            aEdVvod.AddC(tKSkl_p, bSklU);
            aEdVvod.AddC(tKUch_p, bSklU);
            aEdVvod.AddC(tDateD_p, bSklU);
            aEdVvod.AddC(tSm_p, true);
            aEdVvod.AddC(tKT_p, true);

            SetTypSensitive(xDP.nTypD, ref bEksEn, ref bPolEn, ref bNomEn);

            aEdVvod.AddC(tNom_p, bNomEn);
            aEdVvod.AddC(tKEks_p, bEksEn);
            aEdVvod.AddC(tKPol_p, bPolEn);

            // по умолчанию - с первого доступного
            Control 
                xC = null, 
                xEnbF = null,
                xEnbL = null;

            // установка доступных
            for (i = 0; i < aEdVvod.Count; i++)
            {
                if (aEdVvod[i].Enabled)
                {
                    if (xEnbF == null) 
                        xEnbF = aEdVvod[i];
                    xEnbL = aEdVvod[i];
                }
            }

            if (FirstEd == CTRL1ST.START_EMPTY)
                xC = Where1Empty();
            else if (FirstEd == CTRL1ST.START_AVAIL)
                xC = xEnbF;
            else 
                xC = xEnbL;
            if (xC == null)
                xC = xEnbF;

            aEdVvod.SetCur(xC);
            SetEditMode(true);
        }

        private void SetTypSensitive(int nT, ref bool bE, ref bool bP, ref bool bN)
        {
            switch (nT)
            {
                case AppC.TYPD_SAM:
                    bE = false;
                    break;
                case AppC.TYPD_SVOD:
                    bP = false;
                    break;
                case AppC.TYPD_INV:
                    bE = false;
                    bP = false;
                    break;
                case AppC.TYPD_VPER:
                    if (xDP.nEks == AppC.EMPTY_INT)
                        bE = false;
                    break;
                case AppC.TYPD_OPR:
                    bE = false;
                    bP = false;
                    bN = false;
                    break;
                case AppC.TYPD_BRK:
                    bE = false;
                    bP = false;
                    bN = true;
                    break;
                default:
                    break;
            }
        }





    }
}
