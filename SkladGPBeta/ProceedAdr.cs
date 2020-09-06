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


        public enum ADR_TYPE : int
        {
            UNKNOWN = 0,
            OBJECT = 1,            // объект
            CHANNEL = 2,            // канал
            LEVEL = 4,            // ярус
            STELLAGE = 8,            // стеллаж
            ZONE = 32,           // зона
            HIGHBAY = 128,          // высотка
            VIRTUAL = 512,          // код пользователя/паллеты
            SSCC = 1024          // SSCC
        }

        /// информация по адресу для операции
        public class AddrInfo
        {
            public static DataTable
                dtA = null;
            // функция создания строкового представления адреса
            public static ExprDll.RUN
                xR = null;

            // структура для ПХУ: ХХ-камера, ХХХ-стеллаж, ХХ-ряд, Х-ярус, Х-№ поддона, Х-цифровой код буквы склада
            private string
                m_FullAddr = "",           // адрес ячейки-зоны
                m_AddrName = "";
            private int
                m_Sklad;
            private DateTime
                m_dtScan = DateTime.Now;

            private ScanVarGP
                m_Scan;

            public ADR_TYPE
                nType = ADR_TYPE.UNKNOWN;         // тип адреса


            public AddrInfo() { }

            // Происхождение адреса - сканирование
            public AddrInfo(ScanVarGP xSc, int nSklad)
            {
                m_Scan = xSc;
                m_Sklad = nSklad;
                ScanDT = xSc.ScanDTime;
                if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0)
                {
                    nType = ADR_TYPE.SSCC;
                    Addr = xSc.Dat;
                }
                else
                {
                    nType = ADR_TYPE.OBJECT;
                    Addr = xSc.Dat.Substring(2);
                }
            }

            // Происхождение - из таблицы
            public AddrInfo(string sA, int nSklad)
            {
                m_Scan = null;
                m_Sklad = nSklad;
                ScanDT = DateTime.MinValue;
                Addr = sA;
            }

            // строка адреса
            public string Addr
            {
                get { return m_FullAddr; }
                set
                {
                    try
                    {
                        m_FullAddr = value.Trim();
                        if (nType == ADR_TYPE.UNKNOWN)
                        {
                            if (m_FullAddr.Length == 9)
                                nType = ADR_TYPE.OBJECT;
                            else
                            {
                                if ((m_FullAddr.Length == 20) && (m_FullAddr.Substring(0, 2) == "00"))
                                {
                                    nType = ADR_TYPE.SSCC;
                                }
                                else
                                    if (m_FullAddr.IndexOf("USID") >= 0)
                                        nType = ADR_TYPE.VIRTUAL;
                            }
                        }

                        if (m_FullAddr.Length > 0)
                            m_AddrName = AdrName(m_FullAddr, dtA, xR);
                        else
                            m_AddrName = "";

                    }
                    catch
                    {
                        nType = ADR_TYPE.UNKNOWN;
                    }
                }
            }

            //private string x;
            // символьное отображение адреса
            public string AddrShow
            {
                get { return m_AddrName; }
                set { m_AddrName = value; }
            }

            // визуальное представление адреса
            private string AdrName(string sA, DataTable NS_Adr, ExprDll.RUN xFun4Name)
            {
                string
                    sN = "";
                DataRow
                    dr;

                try
                {
                    try
                    {
                        if (nType == ADR_TYPE.SSCC)
                        {
                            sN = String.Format("SSCC-{0}...{1}", m_FullAddr.Substring(2, 1), m_FullAddr.Substring(m_FullAddr.Length - 1 - 4, 4));
                        }
                        else
                        {
                            dr = NS_Adr.Rows.Find(new object[] { sA });
                            sN = ((string)dr["NAME"]).Trim();
                        }
                    }
                    catch { sN = ""; }

                    if (sN.Length == 0)
                    {
                        if (nType < ADR_TYPE.VIRTUAL)
                        {
                            if (xR != null)
                            {
                                sN = (string)xFun4Name.ExecFunc(AppC.FEXT_ADR_NAME, new object[] { m_Sklad, sA });
                            }
                            else
                            {
                                //                    sMesto = m_FullAddr.Substring(2, 3);
                                //                    sCanal = m_FullAddr.Substring(5, 2);
                                //                    sYarus = m_FullAddr.Substring(7, 1);
                                //                    nType = (m_FullAddr.Substring(5, 4) == "0000") ? ADR_TYPE.ZONE : ADR_TYPE.OBJECT;
                                //                            x = String.Format("{0}-{1}.{2}", sMesto, sCanal, sYarus);
                                if (m_FullAddr.Length >= 8)
                                {// Для ПХУ: ХХ-камера, ХХХ-стеллаж, ХХ-ряд, Х-ярус, Х-№ поддона, Х-цифровой код буквы склада
                                    sN = String.Format("{0}-{1}-{2}.{3}",
                                    sA.Substring(0, 2),
                                    sA.Substring(2, 3),
                                    sA.Substring(5, 2),
                                    sA.Substring(7, 1));
                                }
                                else
                                {// Для высотки
                                    sN = String.Format("{0}-{1}.{2}",
                                    sA.Substring(2, 3),
                                    sA.Substring(0, 2),
                                    sA.Substring(5, 1));
                                }
                            }
                        }
                        else if (nType == ADR_TYPE.VIRTUAL)
                            sN = String.Format("V-<поддон>", Addr.Substring(4));
                    }
                }
                catch
                {
                    sN = "";
                }
                if (sN.Length == 0)
                    sN = sA;
                return (sN);
            }

            // время сканирования адреса
            public DateTime ScanDT
            {
                get { return m_dtScan; }
                set { m_dtScan = value; }
            }

        }



        Color

            С_ADR_EMP = Color.LightSteelBlue,                         // адрес пуст
            C_OPR_READY = Color.Yellow,                          // стрелки движения
            С_ADR_SET = Color.LightSkyBlue,                     // адрес установлен
            С_OBJ_EMP = Color.LightSteelBlue,                         // объект пуст
            С_OBJ_SET = Color.CornflowerBlue;                   // адрес установлен

        // отображение адресов и статуса операции
        public void ShowOperState(CurOper xOp)
        {
            ShowOperState(xOp, -1);
        }


        // отображение адресов и статуса операции
        public void ShowOperState(CurOper xOp, int nM)
        {
            string
                A1 = xOp.GetSrc(true),
                A2 = xOp.GetDst(true),
                x = "";

            if (nM < 0)
                x = "";
            else
            {
                x = "> " + nM.ToString();
                lObjDirection.ForeColor = Color.Black;
            }

            if (xCDoc == null)
                return;

            lAdrFrom.SuspendLayout();
            lAdrTo.SuspendLayout();
            lObjDirection.SuspendLayout();

            if (xCDoc.xOper == xOp)
            {
                if ((xOp.nOperState & AppC.OPR_STATE.OPR_SRC_SET) > 0)
                {// источник установлен
                    lAdrFrom.BackColor = С_ADR_SET;
                }
                else
                    lAdrFrom.BackColor = С_ADR_EMP;

                if ((xOp.nOperState & AppC.OPR_STATE.OPR_DST_SET) > 0)
                {// приемник установлен
                    lAdrTo.BackColor = С_ADR_SET;
                }
                else
                    lAdrTo.BackColor = С_ADR_EMP;

                if ((xOp.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                {// продукт установлен
                    lObjDirection.BackColor = С_OBJ_SET;
                }
                else
                    lObjDirection.BackColor = С_OBJ_EMP;

                if ((xOp.nOperState & AppC.OPR_STATE.OPR_READY) > 0)
                {// операция готова к передаче на сервер
                    x = ">=>=>";
                    lObjDirection.ForeColor = C_OPR_READY;
                    //if (nM < 0)
                    //    x = "";
                }
                lObjDirection.Text = x;
            }
            else
            {
                lAdrFrom.BackColor = С_ADR_EMP;
                lAdrTo.BackColor = С_ADR_EMP;
                lObjDirection.BackColor = С_OBJ_EMP;
                //x = "";
            }
            lAdrFrom.Text = A1;
            lAdrTo.Text = A2;
            lObjDirection.Text = x;

            lObjDirection.ResumeLayout();
            lAdrFrom.ResumeLayout();
            lAdrTo.ResumeLayout();
        }


        // допустимость ввода продукции
        private bool CanSetOperObj()
        {
            bool
                bSrcNeed = AppC.xDocTInf[xCDoc.xDocP.nTypD].AdrFromNeed,
                bDstNeed = AppC.xDocTInf[xCDoc.xDocP.nTypD].AdrToNeed,
                ret = true;

            if (xPars.UseAdr4DocMode)
            {
                if (bSrcNeed)
                {// адрес-источник нужен ?

                    if (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.MOVEMENT)
                    {
                        if ((xSm.xAdrFix1 != null) && (!xCDoc.xOper.IsFillSrc()))
                        {// зафиксирован адрес
                            xCDoc.xOper.SetOperSrc(xSm.xAdrFix1, xCDoc.xDocP.nTypD, true);
                        }
                    }

                    if ((xCDoc.xOper.GetSrc(false).Length > 0) 
                        || ((xCDoc.drCurRow["CHKSSCC"] is int) && ((int)xCDoc.drCurRow["CHKSSCC"] > 0))
                        || ( IsDoc4Check()))
                    { }
                    else
                    {
                        ret = false;
                        Srv.ErrorMsg("Не указан адрес!");
                    }


                }
            }

            return (ret);
        }

        /// добавление виртуального адреса (поддон/шлюз)
        private bool MayAddDefaultAdr()
        {
            bool
                ret = true;
            string
                sDefAdr;
            if (xPars.UseAdr4DocMode)
            {
                sDefAdr = String.Format("USID{0}{1}", xSm.MACAdr, xSm.sUser);
                AddrInfo xA = new AddrInfo(sDefAdr, xSm.nSklad);
                xA.ScanDT = DateTime.Now;
                if (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.PRIHOD)
                {
                    if (!AppC.xDocTInf[xCDoc.xDocP.nTypD].AdrFromNeed)
                    {// адрес-источник можно установить по умолчанию
                        if (xCDoc.xOper.GetSrc(false).Length == 0)
                        {
                            xCDoc.xOper.SetOperSrc(xA, xCDoc.xDocP.nTypD, false);
                        }
                    }
                }
                if (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.RASHOD)
                {
                    if (!AppC.xDocTInf[xCDoc.xDocP.nTypD].AdrToNeed)
                    {// адрес-источник можно установить по умолчанию
                        if (xCDoc.xOper.GetDst(false).Length == 0)
                        {
                            xCDoc.xOper.SetOperDst(xA, xCDoc.xDocP.nTypD, false);
                        }
                    }
                }

            }

            return (ret);
        }

        /// какой из адресов следует заполнить (по типу движения документа)
        private int SrcOrDest(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        {
            bool
                bSrcNeed = AppC.xDocTInf[xCDoc.xDocP.nTypD].AdrFromNeed,
                bDstNeed = AppC.xDocTInf[xCDoc.xDocP.nTypD].AdrToNeed;
            int
                nNumDocType = xCDoc.xDocP.nTypD,
                nSrcOrDst = 0;

            AppC.MOVTYPE
                MoveType = AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType;

            if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_ADR_OBJ) > 0)
            {// это обычный адрес
                switch (MoveType)
                {
                    case AppC.MOVTYPE.AVAIL:        // инвентаризации
                        nSrcOrDst = 1;
                        break;
                    case AppC.MOVTYPE.RASHOD:       // расходные документы
                        nSrcOrDst = 1;
                        break;
                    case AppC.MOVTYPE.PRIHOD:       // документы поступления
                        nSrcOrDst = 2;
                        break;
                    case AppC.MOVTYPE.MOVEMENT:     // документы перемещения
                        if (!xCDoc.xOper.IsFillSrc())
                            nSrcOrDst = 1;
                        else
                        {
                            if (!xCDoc.xOper.IsFillDst() && ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) == 0))
                                nSrcOrDst = 0;      // будет запрос
                            else
                                nSrcOrDst = 2;      // будет приемник
                        }
                        break;
                    default:
                        if (xCDoc.xOper.IsFillSrc())
                        {// источник - задан

                            if (bDstNeed)
                            {//... - true
                                if (!xCDoc.xOper.IsFillDst())
                                    nSrcOrDst = 2;
                            }
                            else
                            {//... - false
                                // надо посмотреть, чем заполнен
                                if (!xCDoc.xOper.IsFillDst())
                                    nSrcOrDst = 1;
                            }
                        }
                        else
                        {// источник - пусто
                            if (bSrcNeed)
                                nSrcOrDst = 1;
                            else
                            {
                                if (bDstNeed)
                                {// false - true
                                    if (!xCDoc.xOper.IsFillDst())
                                        nSrcOrDst = 2;
                                }
                                else
                                {// false - false
                                    if (!xCDoc.xOper.IsFillDst())
                                        nSrcOrDst = 1;
                                }
                            }
                        }
                        break;
                }
            }
            else
            {// для SSCC

                nSrcOrDst = AppC.RC_NOTADR;

                //switch (MoveType)
                //{
                //    case AppC.MOVTYPE.AVAIL:        // инвентаризации
                //        if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_SRC_SET) > 0)
                //            nSrcOrDst = AppC.RC_NOTADR;                                 // использовать содержимое
                //        else
                //        {
                //            if (xPars.UseAdr4DocMode)
                //                nSrcOrDst = AppC.RC_CONTINUE;                               // сначала - адрес
                //            else
                //                nSrcOrDst = AppC.RC_NOTADR;                                 // использовать содержимое
                //        }
                //        break;
                //    case AppC.MOVTYPE.RASHOD:       // расходные документы
                //        if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_SRC_SET) == 0)
                //            nSrcOrDst = 1;
                //        else
                //        {
                //            if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                //                nSrcOrDst = 2;
                //            else
                //                nSrcOrDst = AppC.RC_NOTADR;
                //        }
                //        break;
                //    case AppC.MOVTYPE.PRIHOD:       // документы поступления
                //        if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                //            nSrcOrDst = 2;                                              // использовать содержимое
                //        else
                //            nSrcOrDst = AppC.RC_NOTADR;                                 // сначала - адрес
                //        break;
                //    case AppC.MOVTYPE.MOVEMENT:     // документы перемещения
                //        if ((xCDoc.xOper.nOperState == AppC.OPR_STATE.OPR_EMPTY))
                //        {
                //            nSrcOrDst = 1;                                              // адрес-источник
                //        }
                //        else
                //        {
                //            if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                //            {// есть объект, но он должен быть коробка или штучка
                //                nSrcOrDst = 2;                                              // адрес-источник
                //            }
                //            else
                //            {
                //                nSrcOrDst = AppC.RC_NOTADR;
                //            }

                //        }
                //        nSrcOrDst = AppC.RC_NOTADR;
                //        break;
                //    default:
                //        if (xCDoc.xOper.IsFillSrc())
                //        {// источник - задан

                //            if (bDstNeed)
                //            {//... - true
                //                if (!xCDoc.xOper.IsFillDst())
                //                    nSrcOrDst = 2;
                //            }
                //            else
                //            {//... - false
                //                // надо посмотреть, чем заполнен
                //                if (!xCDoc.xOper.IsFillDst())
                //                    nSrcOrDst = 1;
                //            }
                //        }
                //        else
                //        {// источник - пусто
                //            if (bSrcNeed)
                //                nSrcOrDst = 1;
                //            else
                //            {
                //                if (bDstNeed)
                //                {// false - true
                //                    if (!xCDoc.xOper.IsFillDst())
                //                        nSrcOrDst = 2;
                //                }
                //                else
                //                {// false - false
                //                    if (!xCDoc.xOper.IsFillDst())
                //                        nSrcOrDst = 1;
                //                }
                //            }
                //        }
                //        break;
                //}
            }

            if (nSrcOrDst == 0)
            {// выяснить не удалось
                DialogResult drQ = MessageBox.Show("\"Источник\" - Yes\n\"Приемник\" - No\nОтмена - Cancel",
                    "Какой адрес установить?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (drQ == DialogResult.Yes)
                    nSrcOrDst = 1;
                else if (drQ == DialogResult.No)
                    nSrcOrDst = 2;
            }

            return (nSrcOrDst);
        }

        /// обработка полученного адреса
        private int ProceedAdrNew(ScanVarGP xSc, ref PSC_Types.ScDat scD)
        {
            int
                nRet = AppC.RC_OK,
                nSrcOrDst = 0; // 1-From, 2-To
            bool
                IsSSCC,
                bOperReady;
            string
                sA1,
                sA2;
            AddrInfo
                xA;

            // значение адреса (без AI)
            //scD.sN = xSc.Dat.Substring(2);
            if ((xSc.bcFlags & ScanVarGP.BCTyp.SP_SSCC) > 0)
            {
                IsSSCC = true;
                scD.sN = xSc.Dat;                               // SSCC полностью
            }
            else
            {
                IsSSCC = false;
                scD.sN = xSc.Dat.Substring(2);                  // значение адреса (без AI)
            }

            if ((xSm.xAdrFix1 != null) && (!IsSSCC))
            {// зафиксирован адрес, пришел еще один
                //if (!xCDoc.xOper.bObjOperScanned)
                if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) == 0)
                {// поддон еще не сканировался, сейчас пришел адрес отправителя
                    nSrcOrDst = 1;
                    xCDoc.xOper.SetOperDst(xSm.xAdrFix1, xCDoc.xDocP.nTypD, false);
                }
                else
                {// поддон уже сканировался, сейчас пришел адрес получателя
                    nSrcOrDst = 2;
                    xCDoc.xOper.SetOperSrc(xSm.xAdrFix1, xCDoc.xDocP.nTypD, false);
                }
            }
            else
            {// фиксированных адресов пока не было
                nSrcOrDst = SrcOrDest(xSc, ref scD);
            }

            //    if ((nSrcOrDst == 1) || (nSrcOrDst == 2))
            //    {// отсканированным адресом следует воспользоваться  как адресом отправителя или получателя
            //        AddrInfo xA = new AddrInfo(xSc, xSm.nSklad);


            //    if (!xCDoc.xOper.IsFillSrc())
            //        nSrcOrDst = 1;
            //    else
            //    {
            //        if (xCDoc.nTypOp == AppC.TYPOP_DOCUM)
            //            nSrcOrDst = 1;
            //        else
            //        {
            //            if (!xCDoc.xOper.IsFillDst() && !xCDoc.xOper.bObjOperScanned)
            //            {
            //                if (xCDoc.xOper.xAdrSrc.Addr != scD.sN)
            //                {
            //                    DialogResult drQ = MessageBox.Show("Изменить \"ИЗточник\" (Enter)?\n(ESC) - отмена",
            //                        "Снова адрес!",
            //                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            //                    if (drQ == DialogResult.OK)
            //                        nSrcOrDst = 1;
            //                    else
            //                        nSrcOrDst = 0;
            //                }
            //                else
            //                    nSrcOrDst = 1;
            //            }
            //            else
            //                nSrcOrDst = 2;
            //        }
            //    }
            //}




            //if (nSrcOrDst > 0)
            //{// отсканированным адресом следует воспользоваться  как адресом отправителя или получателя

            //    if (nSrcOrDst == 1)
            //    {// это источник
            //        if (xCDoc.xOper.GetDst(false) == xAFromScan.Addr)
            //            xAFromScan = null;
            //        else
            //            xCDoc.xOper.SetOperSrc(xAFromScan, xCDoc.xDocP.nTypD);
            //    }
            //    else
            //    {// это приемник
            //        if (xCDoc.xOper.GetSrc(false) == xAFromScan.Addr)
            //            xAFromScan = null;
            //        else
            //            xCDoc.xOper.SetOperDst(xAFromScan, xCDoc.xDocP.nTypD);
            //    }
            //    if (xAFromScan == null)
            //        Srv.ErrorMsg("Адреса совпадают...", scD.sN, true);
            //    else
            //    {
            //        if (xCDoc.nTypOp != AppC.TYPOP_DOCUM)
            //        {
            //            if (bShowTTN)
            //            {
            //                tEAN.Text = xCDoc.xOper.GetSrc(true);
            //                tDatMC.Text = xCDoc.xOper.GetDst(true);
            //                nRet = IsOperReady(true, drDet);
            //                if (nRet == AppC.RC_OK)
            //                    RetAfterTempMove();
            //            }
            //            else
            //                Srv.ErrorMsg("ТТН!", true);
            //        }
            //        if (nSrcOrDst == 1)
            //        {//для источника определяем содержимое
            //            if (xPars.GetAdrContentFromSrv)
            //            {//если это требуется
            //                nRet = AdrResult(xSc, ref scD, xAFromScan);
            //            }
            //        }

            //    }
            //}



            if ((nSrcOrDst == 1) || (nSrcOrDst == 2))
            {// отсканированным адресом следует воспользоваться  как адресом отправителя или получателя
                xA = new AddrInfo(xSc, xSm.nSklad);

                //if (!xCDoc.xOper.bObjOperScanned 
                    
                if (((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) == 0)
                    && (xPars.UseAdr4DocMode))
                {// пока только для ... инвентаризации ...
                    if (xCDoc.xOper.nOperState == AppC.OPR_STATE.OPR_EMPTY)
                    {
                        PSC_Types.ScDat scEmp = new PSC_Types.ScDat(new BarcodeScannerEventArgs(BCId.Code128, ""));
                        if (!bInEasyEditWait)
                            SetDetFields(true, ref scEmp);
                    }
                }

                if (nSrcOrDst == 1)
                {// это источник
                    xCDoc.xOper.SetOperSrc(xA, xCDoc.xDocP.nTypD, true);
                    sA1 = xA.AddrShow;
                    sA2 = xCDoc.xOper.GetDst(false);
                }
                else
                {// это приемник
                    sA1 = xCDoc.xOper.GetSrc(false);
                    sA2 = xA.AddrShow;

                    if (xA.Addr == sA1)
                    {
                        Srv.ErrorMsg("Адреса совпадают...", sA2, true);
                        return (AppC.RC_CANCEL);
                    }

                    if (xCDoc.xOper.xAdrDst_Srv != null)
                    {// а сервер задавал адрес приемника
                        if (xCDoc.xOper.xAdrDst_Srv.Addr != xA.Addr)
                        {// и они разные
                            DialogResult drQ = MessageBox.Show(String.Format(
                                "(Yes) - {0}\n<<с сервера>>\n(No) - {1}\n<<отсканирован>>\n(ESC) - отмена", xCDoc.xOper.xAdrDst_Srv.Addr, xA.Addr),
                                "Адрес назначения ?!",
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (drQ == DialogResult.Yes)
                                xA = xCDoc.xOper.xAdrDst_Srv;           // сохраним приемником рекомендованный
                            else if (drQ == DialogResult.No)
                            {                                           // сохраним приемником отсканированный
                            }
                            else
                                return (AppC.RC_CANCEL);
                        }
                    }

                    xCDoc.xOper.SetOperDst(xA, xCDoc.xDocP.nTypD, true);
                }

                //var d = ZeroCell(xCDoc.xOper.xAdrSrc, xCDoc.xOper.xAdrDst);
                //if (d != null)
                //{
                //    xCDoc.xOper = new CurOper(xCDoc.xDocP.DType);
                //}

                bOperReady = (xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_READY) > 0;

                if (bOperReady)
                {// операция готова к отправке, все введено
                    nRet = IsOperReady();
                    if (nRet == AppC.RC_OK)
                    {
                        if (IsAutoMark())
                        {
                        }
                        else
                        {
                            if (xCDoc.xDocP.nTypD == AppC.TYPD_OPR)
                                RetAfterTempMove();                     // возможен возврат из временного перемещения
                        }
                    }
                }
                else
                {
                    //if (!xCDoc.xOper.bObjOperScanned)
                    if ((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_OBJ_SET) == 0)
                    {
                        if (nSrcOrDst == 1)
                        {//для источника определяем содержимое
                            if (AppC.xDocTInf[xCDoc.xDocP.nTypD].TryFrom && xPars.UseAdr4DocMode)
                            {//если это требуется
                                if (!IsSSCC)
                                    nRet = AdrResult(xSc, ref scD, xA);
                                else
                                    nRet = AppC.RC_CONTINUE;
                            }
                        }
                        else
                        {
                            //tAdrTo.Text = xCDoc.xOper.GetDst(true);
                        }
                    }
                }
            }
            else
            {
                if (nSrcOrDst == AppC.RC_NOTADR)
                    nRet = AppC.RC_CONTINUE;
            }

            return (nRet);
        }


        private int IsOperReady()
        {
            return (IsOperReady(false));
        }


        private int IsOperReady(bool bMandatorySend)
        {
            bool
                bNeed2Send = false;
            int
                nRet = AppC.RC_OPNOTREADY;
            DataRow
                drOpr = xCDoc.xOper.OperObj;

            if (((xCDoc.xOper.nOperState & AppC.OPR_STATE.OPR_READY) > 0) ||
                bMandatorySend)
            {// готовность операции имеется 
                nRet = AppC.RC_HALFOK;
                if (bMandatorySend)
                    bNeed2Send = true;
                else
                {
                    if (xPars.OpAutoUpl)
                    {// автовыгрузка операций установлена
                        if (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType == AppC.MOVTYPE.MOVEMENT)
                            // внутрискладское
                            bNeed2Send = true;
                        else
                        {
                            if ((xPars.UseAdr4DocMode) && (AppC.xDocTInf[xCDoc.xDocP.nTypD].MoveType != AppC.MOVTYPE.AVAIL))
                                bNeed2Send = true;
                        }
                    }
                }
                if (bNeed2Send)
                {// выгрузка по готовности каждой операции установлена и это не инвентаризация
                    nRet = SetOverOPR(false, drOpr, AppC.COM_VOPR);
                    //if (nRet == AppC.RC_OK)
                    //    xCDoc.xOper = new CurOper();
                }
                else
                // или уже отправили или начинаем новую
                    xCDoc.xOper = new CurOper(false);
            }

            return (nRet);
        }



        private int SetOverOPR(bool bAfterScan, DataRow drOpr)
        {
            return (SetOverOPR(bAfterScan, drOpr, ""));
        }

        private int SetOverOPR(bool bAfterScan, DataRow drOpr, string sComm)
        {
            bool
                bNeedTrans;
            int
                nRet = AppC.RC_OK;
            ServerExchange
                xSE = new ServerExchange(this);

                if (drOpr != null)
                {
                    bNeedTrans = (((AppC.OPR_STATE)drOpr["STATE"] != AppC.OPR_STATE.OPR_TRANSFERED) ||
                        (xCDoc.xDocP.nTypD != AppC.TYPD_OPR));
                    
                    if (bNeedTrans)
                    {
                        if (bAfterScan)
                        {
                            if ((scCur.sKMC == (string)drDet["KMC"]) &&
                                (scCur.nParty == (string)drDet["NP"]) &&
                                (scCur.dDataIzg.ToString("yyyyMMdd") == (string)drDet["DVR"]))
                                bAfterScan = false;
                        }
                        if (!bAfterScan)
                        {// выгрузка по кнопочке
                            //drOpr["STATE"] = AppC.OPR_STATE.OPR_READY;
                            xCUpLoad = new CurUpLoad(xPars);
                            xDP = xCUpLoad.xLP;

                            xCUpLoad.bOnlyCurRow = true;
                            xCUpLoad.drForUpl = drOpr;
                            xCUpLoad.sCurUplCommand = sComm;

                            if (xPars.OpAutoUpl)
                            {// авто-выгрузка операций
                                string sL = UpLoadDoc(xSE, ref nRet);
                                if (xSE.ServerRet == AppC.RC_OK)
                                {
                                    AddrInfo xA =  xCDoc.xOper.xAdrSrc;
                                    xCDoc.xOper = new CurOper(false);
                                    if (IsAutoMark())
                                    {
                                        xCDoc.xOper.SetOperSrc(xA, xCDoc.xDocP.nTypD, false);
                                        // Имитация вызова
                                        //W32.SimulKey(W32.VK_SHIFT, 0);
                                        //W32.SimulKey(W32.VK_D1, 2);
                                    }
                                    //return (nRet);
                                }

                                if (nRet != AppC.RC_OK)
                                {
                                    if (nRet == AppC.RC_HALFOK)
                                    {
                                        Srv.PlayMelody(W32.MB_2PROBK_QUESTION);
                                        MessageBox.Show(sL, "Предупреждение!");
                                    }
                                    else
                                        Srv.ErrorMsg(sL, true);
                                }

                                if ((xSE.ServerRet != AppC.EMPTY_INT) &&
                                    (xSE.ServerRet != AppC.RC_OK))
                                {// операция выгрузки не прошла на сервере (содержательная ошибка)
                                    //if (xSE.ServerRet == 99)
                                    //    CompareAddrs(xCDoc.xOper.xAdrDst.Addr, String.Format("---{0}-----------После выгрузки", xSE.ServerRet), true);
                                    xCDoc.xOper.SetOperDst(null, xCDoc.xDocP.nTypD, true);
                                }
                            }
                            else
                                xCDoc.xOper = new CurOper(true);
                            xCUpLoad = null;
                        }
                    }
                }
                else
                    Srv.ErrorMsg("Продукция не определена!");
            return (nRet);
        }




        private int AdrResult(ScanVarGP xSc, ref PSC_Types.ScDat scD, AddrInfo xA)
        {
            int
                nRec,
                nRet = AppC.RC_OK;
            DataRow
                dr;
            DialogResult
                dRez;

            nRet = ConvertAdr2Lst(xA, AppC.COM_ADR2CNT, "ROW", false, NSI.SRCDET.FROMADR);
            if (nRet == AppC.RC_OK)
            {
                nRec = xCLoad.dtZ.Rows.Count;
                if (nRec == 1)
                {
                    scD = new PSC_Types.ScDat(new ScannerAll.BarcodeScannerEventArgs(ScannerAll.BCId.NoData, ""));
                    //SetVirtScan(xCLoad.dtZ.Rows[0], ref scD, true, false);
                    SetVirtScan(xCLoad.dtZ.Rows[0], ref scD, true, true);
                    scD.nRecSrc = (int)NSI.SRCDET.FROMADR;
                    xCDoc.xOper.SSCC = scD.sSSCC;

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
                            System.Drawing.Rectangle
                                recInf,
                                screen = Screen.PrimaryScreen.Bounds;

                            recInf = new System.Drawing.Rectangle((screen.Width - INFWIN_WIDTH) / 2, 200, INFWIN_WIDTH, INFWIN_HEIGHT);
                            xDestInfo = new Srv.HelpShow(this, recInf, 1, FontS, 0);
                        }
                        xDestInfo.ShowInfo(new string[] { scD.xOp.xAdrDst.AddrShow }, ref ehCurrFunc);
                    }
                    scD.xOp = (xCDoc.xOper == null) ? new CurOper(false) : xCDoc.xOper;


                    if ((xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM) 
                        || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMPL)
                        || (xCDoc.xDocP.TypOper == AppC.TYPOP_KMSN))
                    {// будет редактирование количества "отсканированной" продукции
                        return (AppC.RC_CONTINUE);
                    }

                    // далее выполняется только для операций

                    if (xCDoc.xDocP.TypOper == AppC.TYPOP_MOVE)
                    {
                        if (scD.tTyp == AppC.TYP_TARA.TARA_PODDON)
                        {// для поддонов редактирования не будет
                            scCur = scD;
                            if (AddDet1(ref scD, out dr))
                            {
                                //xCDoc.xOper.bObjOperScanned = true;
                                xCDoc.xOper.SetOperObj(dr, xCDoc.xDocP.nTypD, false);
                                //SetDetFields(false);
                                if (dr != null)
                                {
                                    drDet = dr;
                                    //dr["SSCC"] = scD.sSSCC;
                                    //xCDoc.xOper.SSCC = scD.sSSCC;
                                }
                            }
                        }
                    }
                    //IsOperReady(false);

                }
                else if (nRec > 1)
                {// ROW - добавление группы
                    if (xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM)
                    {
                        dRez = MessageBox.Show(
                            String.Format("Новых строк {0}\nДобавить (Enter)?\n(ESC)- вывод на экран", xCLoad.dtZ.Rows.Count),
                            "Добавление продукции", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    }
                    else
                    {
                        dRez = DialogResult.Cancel;
                    }
                    if (dRez == DialogResult.OK)
                    {
                        //nRet = AddGroupDet(AppC.RC_MANYEAN, (int)NSI.SRCDET.FROMADR, xA.AddrShow);
                        nRet = AddGroupDet(AppC.RC_MANYEAN, (int)NSI.SRCDET.FROMADR, "");
                    }
                    else
                    {
                        if (AppC.RC_OK == ConvertAdr2Lst(xA, "TXT"))
                        {
                            // справочная информация, просто выводится
                            xHelpS.ShowInfo(xInf, ref ehCurrFunc);
                        }
                    }
                }
            }
            return (nRet);
        }


        /// вставка записи с виртульной продукцией (пусто, SSCC)
        private DataRow AddVirtProd(ref PSC_Types.ScDat sc)
        {
            DataRow
                drFictProd = null;

            drFictProd = xNSI.AddDet(sc, xCDoc, null);
            if (drFictProd != null)
            {
                xCDoc.xOper.SetOperObj(drFictProd, xCDoc.xDocP.nTypD, false);
                if (bShowTTN)
                {
                    drDet = drFictProd;
                    scCur = sc;
                    SetCurRow(dgDet, "ID", (int)drFictProd["ID"]);
                }
                SetDetFields(false);
                AfterAddScan(this, new EventArgs());
            }
            return (drFictProd);
        }


    }



}    
