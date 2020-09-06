using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Threading;

using SavuSocket;
using PDA.Service;
using ScannerAll;

namespace SkladGP
{

    public delegate void LoadFromSrv(SocketStream s, Dictionary<string, string> aC, DataSet ds,
                                     ref string sE, int nRetSrv);

    public partial class MainF : Form
    {
        // отображение уровня сигнала
        private void pnLoadDocG_EnabledChanged(object sender, EventArgs e)
        {
            //bool bShowNow = ((Control)sender).Enabled && xBCScanner.WiFi.IsEnabled;
            bool bShowNow = ((Control)sender).Enabled && xBCScanner.WiFi.IsShownState;
            xBCScanner.WiFi.ShowWiFi(pnLoadDocG, bShowNow);
        }


        // формирование массива строк для выгрузки
        private DataRow[] PrepDataArrForUL(int nReg)
        {
            int nRet = AppC.RC_OK;
            string sRf = "";
            DataRow[] ret = null;

            if (nReg == AppC.UPL_CUR)
            {
                if (xCDoc.xDocP.TypOper == AppC.TYPOP_DOCUM)
                {
                    if (((int)xCDoc.drCurRow["SOURCE"] == NSI.DOCSRC_UPLD) && (!xPars.bUseSrvG))
                    {
                        string sErr = "Уже выгружен!";
                        DialogResult dr;
                        dr = MessageBox.Show("Отменить выгрузку (Enter)?\r\n (ESC) - выгрузить повторно",
                        sErr,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                        if (dr == DialogResult.OK)
                        {
                            nRet = AppC.RC_CANCEL;
                        }
                        else
                            nRet = AppC.RC_OK;
                    }
                }
                if (nRet == AppC.RC_OK)
                {
                    ret = new DataRow[] { xCDoc.drCurRow };
                    xCUpLoad.naComms = new List<int>();
                    xCUpLoad.naComms.Add( (int)xCDoc.drCurRow["TD"] );
                }
            }
            else if (nReg == AppC.UPL_ALL)
            {
                // фильтр - текущий для Grid документов + невыгруженные
                sRf = xNSI.DT[NSI.BD_DOCOUT].dt.DefaultView.RowFilter;
                if (sRf != "")
                {
                    sRf = "(" + sRf + ")AND";
                }
                sRf += String.Format("(SOURCE<>{0})", NSI.DOCSRC_UPLD);
                ret = PrepForAll(sRf);
            }
            else if (nReg == AppC.UPL_FLT)
            {
                //sRf = FiltForDoc(xCUpLoad.SetFiltInRow(xNSI));
                sRf = xCUpLoad.SetFiltInRow();
                ret = PrepForAll(sRf);
            }
            return (ret);
        }

        private DataRow[] PrepForAll(string sRf)
        {
            // все неотгруженные документы
            DataView dv = new DataView(xNSI.DT[NSI.BD_DOCOUT].dt, sRf, "", DataViewRowState.CurrentRows);
            DataRow[] drA = new DataRow[dv.Count];
            xCUpLoad.naComms = new List<int>();
            for (int i = 0; i < dv.Count; i++)
            {
                drA.SetValue(dv[i].Row, i);
                xCUpLoad.naComms.Add( (int)drA[i]["TD"]);
            }
            return (drA);
        }

        private string UpLoadDoc(ServerExchange xSExch, ref int nR)
        {
            int i,
                nRet = AppC.RC_OK;
            string nComm, 
                sErr = "",
                sAllErr = "";
            DataSet dsTrans;
            DataRow[] drAUpL = null;
            LoadFromSrv 
                dgL = null;

            try
            {
                drAUpL = PrepDataArrForUL(xCUpLoad.ilUpLoad.CurReg);
                if (drAUpL != null)
                {
                    if (xCUpLoad.sCurUplCommand != AppC.COM_CKCELL)
                        dgL = new LoadFromSrv(SetUpLoadState);
                    for (i = 0; i < drAUpL.Length; i++)
                    {
                        nRet = AppC.RC_OK;
                        nComm = (xCUpLoad.naComms[i] == AppC.TYPD_INV) ? AppC.COM_VINV :
                                (xCUpLoad.naComms[i] == AppC.TYPD_VPER) ? AppC.COM_VVPER :
                                (xCUpLoad.naComms[i] == AppC.TYPD_OPR) ? AppC.COM_VOPR : AppC.COM_VTTN;

                        switch ((int)(drAUpL[i]["TYPOP"]))
                        {
                            case AppC.TYPOP_MARK:
                                nComm = AppC.COM_VMRK;
                                break;
                            case AppC.TYPOP_OTGR:
                            case AppC.TYPOP_KMPL:
                                nComm = AppC.COM_VKMPL;
                                break;
                        }

                        if (xCUpLoad.sCurUplCommand.Length == 0)
                            xCUpLoad.sCurUplCommand = nComm;

                        dsTrans = xNSI.MakeWorkDataSet(xNSI.DT[NSI.BD_DOCOUT].dt,
                                  xNSI.DT[NSI.BD_DOUTD].dt, new DataRow[] { drAUpL[i] }, null, xSm, xCUpLoad);

                        sErr = xSExch.ExchgSrv(xCUpLoad.sCurUplCommand, "", "", dgL, dsTrans, ref nRet, 300);

                        if ((xSExch.ServerRet == AppC.RC_OK) && (sErr != "OK"))
                            nRet = AppC.RC_HALFOK;
                        if (nRet != AppC.RC_OK)
                        {
                            nR = nRet;
                            sAllErr += sErr + "\n";
                        }
                    }
                }
                else
                {
                    nRet = AppC.RC_NODATA;
                    sErr = "Нет данных для передачи";
                }
            }
            catch (Exception)
            {
                nRet = AppC.RC_NODATA;
                sErr = "Ошибка подготовки";
            }
            if (sAllErr.Length == 0)
            {
                nR = nRet;
                sAllErr = sErr;
            }
            return (sAllErr);
        }

        private void SetUpLoadState(SocketStream stmX, Dictionary<string, string> aC,
            DataSet dsU, ref string sErr, int nRetSrv)
        {
            DataView 
                dv;
            DataRow 
                dr;

            foreach (DataRow drT in dsU.Tables[0].Rows)
            {
                dr = xNSI.DT[NSI.BD_DOCOUT].dt.Rows.Find(new object[] { (int)drT["SYSN"] });
                if (null != dr)
                {
                    if (xCUpLoad.sCurUplCommand != AppC.COM_VOPR)
                    {
                        dr["SOURCE"] = NSI.DOCSRC_UPLD;
                    }
                    foreach (DataRow drD in dsU.Tables[1].Rows)
                    {
                        dv = new DataView(xNSI.DT[NSI.BD_DOUTD].dt,
                            String.Format("(ID={0})", (int)drD["ID"]), "", DataViewRowState.CurrentRows);
                        dv[0].Row["STATE"] = AppC.OPR_STATE.OPR_TRANSFERED;
                    }
                }
            }

            sErr = "OK - Передача завершена";
        }






        /// заполнение DataRow картинок
        private void AddPicsDoc(DataRelation rlPics, DataRow drZvkDoc, DataTable dtPics)
        {
            int
                nId;
            object[]
                aRec;
            DataRow
                drP;
            DataRow[] 
                drPicsFromSrv;

            drPicsFromSrv = drZvkDoc.GetChildRows(rlPics);
            foreach (DataRow drPicSrv in drPicsFromSrv)
            {
                drP = dtPics.NewRow();
                nId = (int)drP["ID"];

                aRec = drPicSrv.ItemArray;
                drP.ItemArray = aRec;
                drP["ID"] = nId;
                drP["SYSN"] = drZvkDoc["SYSN"];

                dtPics.Rows.Add(drP);
            }
        }

        /// удаление детальных для документа
        private void DelDetail4Doc(DataRow drDoc, DataTable dtDet, string sRel)
        {
            DataRow[] drDetZ;
            drDetZ = drDoc.GetChildRows(xNSI.dsM.Relations[sRel]);
            //if (drDetZ.Length > 0)
            //{// их все и удаляем
                foreach (DataRow drDel in drDetZ)
                {
                    dtDet.Rows.Remove(drDel);
                }
            //}
        }


        // добавление полученных заявок в рабочие таблицы
        private int AddZ(CurLoad xCL, ref string sErr)
        {
            int 
                nRet = AppC.RC_OK,
                nNPP,
                nM = 0;
            string s;
            PSC_Types.ScDat sD = new PSC_Types.ScDat();
            //object xNewKey;

            DataSet ds = xCL.dsZ;
            DataRow drMDoc;
            DataRow[] drDetZ, drMDetZ;
            DataTable dt = xNSI.DT[NSI.BD_DOCOUT].dt,
                dtD = xNSI.DT[NSI.BD_DIND].dt;

            // пока удалим связи и таблицу ТТН из DataSet
            //DataRelation dRel = xNSI.dsM.Relations[NSI.REL2TTN];

            //xNSI.dsM.Relations.Remove(NSI.REL2TTN);
            //xNSI.dsM.Tables.Remove(xNSI.dsM.Tables[NSI.BD_DOUTD]);

            if (xCL.ilLoad.CurReg == AppC.UPL_CUR)
            {// заявка только для текущего документа

                drMDoc = xCDoc.drCurRow;
                //object[] xCur = drMDoc.ItemArray;

                if (ds.Tables[NSI.BD_ZDET].Rows.Count > 0)
                {// имеются детальные строки для загрузки
                    // а это загруженные ранее

                    //drDetZ = drMDoc.GetChildRows(xNSI.dsM.Relations[NSI.REL2ZVK]);
                    //if (drDetZ.Length > 0)
                    //{// их все и удаляем
                    //    foreach (DataRow drDel in drDetZ)
                    //    {
                    //        xNSI.dsM.Tables[NSI.BD_DIND].Rows.Remove(drDel);
                    //    }
                    //}
                    DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_DIND], NSI.REL2ZVK);
                    DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_PICT], NSI.REL2PIC);

                    // установка существующего ключа (SYSN)
                    // каскадно должен измениться и в детальных
                    ds.Tables[NSI.BD_ZDOC].Rows[0]["SYSN"] = drMDoc["SYSN"];
                    nM = 0;
                    nNPP = 1;
                    foreach (DataRow drA in ds.Tables[NSI.BD_ZDET].Rows)
                    {
                        //nM += SetOneDetZ(ref sD, dtD, drA.ItemArray);
                        nM += SetOneDetZ(ref sD, dtD, drA, drMDoc, ref nNPP);
                        nNPP++;
                    }
                    drMDoc["SOURCE"] = NSI.DOCSRC_LOAD;
                    drMDoc["CHKSSCC"] = 0;
                    drMDoc["MESTZ"] = nM;
                    if (xCL.CheckIt)
                    {
                        drMDoc["SSCCONLY"] = 1;
                        DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_DOUTD], NSI.REL2TTN);
                    }
                    else
                        drMDoc["SSCCONLY"] = 0;
                    AddPicsDoc(ds.Relations[NSI.REL2PIC], ds.Tables[NSI.BD_ZDOC].Rows[0], xNSI.dsM.Tables[NSI.BD_PICT]);

                    //drMDoc["CONFSCAN"] = ConfScanOrNot(drMDoc, xPars.ConfScan);
                }
                else
                {
                    sErr = "Детальные строки не найдены!";
                    nRet = AppC.RC_CANCEL;
                }
            }
            else
            {// загрузка всего, что пришло (ALL или по фильтру)

                // пока ничего не загрузили
                xCL.dr1st = null;

                for (int i = 0; i < ds.Tables[NSI.BD_ZDOC].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[NSI.BD_ZDOC].Rows[i];
                    drDetZ = dr.GetChildRows(ds.Relations[NSI.REL2ZVK]);
                    if ((drDetZ != null) && (drDetZ.Length > 0))
                    {// имеются детальные строки для загрузки

                        // по-хорошему, надо искать по нормальному ключу, а не по SYSN
                        //nPKey = (int)dr["SYSN"];
                        s = FiltForDocExchg(dr, xCL);

                        DataRow[] aDr = dt.Select(s);
                        if (aDr.Length > 0)
                        {
                            drMDoc = aDr[0];
                            if (xCL.nCommand == AppC.F_ZZKZ1)
                            {
                                if (MainF.swProt != null)
                                {
                                    //string sCurr = String.Format("Date={0}, SM={1}, PD={2}, SYSN={3}", drMDoc["DT"], drMDoc["KSMEN"], drMDoc["KEKS"], drMDoc["SYSN"]);
                                    swProt.WriteLine(String.Format("{2} Перезапись заказа (SYSN={1})! Ф={0}", s, drMDoc["SYSN"], DateTime.Now.ToString("dd.MM.yy HH:mm:ss - ")));
                                    WriteAllToReg(true);
                                }
                            }

                        }
                        else
                            drMDoc = null;

                        //drMDoc = dt.Rows.Find(new object[] { nPKey });

                        if (null != drMDoc)
                        {// ранее уже грузили

                            //drMDetZ = drMDoc.GetChildRows(xNSI.dsM.Relations[NSI.REL2ZVK]);
                            //if (drDetZ.Length > 0)
                            //{// их все и удаляем
                            //    foreach (DataRow drDel in drMDetZ)
                            //    {
                            //        xNSI.dsM.Tables[NSI.BD_DIND].Rows.Remove(drDel);
                            //    }
                            //}
                            DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_DIND], NSI.REL2ZVK);
                            DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_PICT], NSI.REL2PIC);
                        }
                        else
                        {// новая заявка
                            drMDoc = dt.NewRow();
                            object x = drMDoc["SYSN"];
                            drMDoc.ItemArray = dr.ItemArray;
                            //for(int ii = 0; ii < dr.ItemArray.Length; ii++)
                            //{
                            //    if (!(dr.ItemArray[ii].GetType() == typeof(System.DBNull)))
                            //        drMDoc.ItemArray[ii] = dr.ItemArray[ii];
                            //}

                            drMDoc["SYSN"] = x;
                            drMDoc["SOURCE"] = NSI.DOCSRC_LOAD;
                            drMDoc["TIMECR"] = DateTime.Now;
                            if (xCL.CheckIt)
                            {
                                drMDoc["SSCCONLY"] = 1;
                                DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_DOUTD], NSI.REL2TTN);
                            }
                            else
                                drMDoc["SSCCONLY"] = 0;

                            if (xCL.nCommand == AppC.F_LOAD_DOC)
                                drMDoc["TYPOP"] = AppC.TYPOP_DOCUM;
                            else
                                if ( (xCL.nCommand == AppC.F_LOADKPL)
                                    || (xCL.nCommand == AppC.F_ZZKZ1))
                                drMDoc["TYPOP"] = AppC.TYPOP_KMPL;
                                else
                                    drMDoc["TYPOP"] = AppC.TYPOP_PRMK;

                            dt.Rows.Add(drMDoc);
                        }
                        // установка существующего ключа (SYSN)
                        // каскадно должен измениться и в детальных
                        //dr["DIFF"] = NSI.DOCCTRL.UNKNOWN;
                        dr["SYSN"] = drMDoc["SYSN"];
                        nM = 0;
                        nNPP = 1;
                        foreach (DataRow drZ in drDetZ)
                        {
                            //nM += SetOneDetZ(ref sD, dtD, drZ.ItemArray);
                            nM += SetOneDetZ(ref sD, dtD, drZ, drMDoc, ref nNPP);
                            nNPP++;
                        }
                        drMDoc["SOURCE"] = NSI.DOCSRC_LOAD;
                        drMDoc["CHKSSCC"] = 0;
                        drMDoc["MESTZ"] = nM;
                        if (xCL.CheckIt)
                        {
                            drMDoc["SSCCONLY"] = 1;
                            DelDetail4Doc(drMDoc, xNSI.dsM.Tables[NSI.BD_DOUTD], NSI.REL2TTN);
                        }
                        else
                            drMDoc["SSCCONLY"] = 0;

                        AddPicsDoc(ds.Relations[NSI.REL2PIC], ds.Tables[NSI.BD_ZDOC].Rows[i], xNSI.dsM.Tables[NSI.BD_PICT]);
                        //drMDoc["CONFSCAN"] = ConfScanOrNot(drMDoc, xPars.ConfScan);
                        if (xCL.dr1st == null)
                            xCL.dr1st = drMDoc;
                    }
                    else
                    {
                        sErr = String.Format("{0}-Детальные строки не найдены!", dr["SYSN"]);
                        nRet = AppC.RC_CANCEL;
                    }
                }
            }

            // возвращаем таблицу обратно и связи в DataSet
            //xNSI.dsM.Tables.Add(xNSI.dsM.Tables[NSI.BD_DOUTD]);
            //xNSI.dsM.Relations.Add(dsRel);


            return (nRet);
        }

        private string FiltForDocExchg(DataRow drZ, CurLoad xCL)
        {
            int n;
            string s;

            string sF = String.Format("(TD={0}) AND (DT={1}) AND (KSK={2})", drZ["TD"], drZ["DT"], drZ["KSK"]);

            s = "AND(ISNULL(NUCH,-1)=-1)";
            try
            {
                n = (int)drZ["NUCH"];
                if (n > 0)
                {
                    s = "AND(NUCH=" + n.ToString() + ")";
                }
                else
                    drZ["NUCH"] = System.DBNull.Value;
            }
            catch { s = ""; }
            finally
            {
                sF += s;
            }

            s = "AND(ISNULL(KSMEN,'')='')";
            try
            {
                s = (string)drZ["KSMEN"];
                if (s.Length > 0)
                {
                    s = "AND(KSMEN='" + s + "')";
                }
                else
                    drZ["KSMEN"] = System.DBNull.Value;
            }
            catch { }
            finally
            {
                sF += s;
            }



            s = "AND(ISNULL(KEKS,-1)=-1)";
            try
            {
                n = (int)drZ["KEKS"];
                if (n > 0)
                {
                    s = "AND(KEKS=" + n.ToString() + ")";
                }
                else
                    drZ["KEKS"] = System.DBNull.Value;
            }
            catch { s = ""; }
            finally
            {
                sF += s;
            }


            s = "AND(ISNULL(KRKPP,-1)=-1)";
            try
            {
                n = (int)drZ["KRKPP"];
                if (n > 0)
                {
                    s = "AND(KRKPP=" + n.ToString() + ")";
                }
                else
                    drZ["KRKPP"] = System.DBNull.Value;
            }
            catch { s = ""; }
            finally
            {
                sF += s;
            }

            //------
            if ((xCL.nCommand == AppC.F_LOADKPL) || (xCL.nCommand == AppC.F_ZZKZ1))
            {
                //sF += "AND(TYPOP=" + AppC.TYPOP_KMPL.ToString() + ")";
                //sF = String.Format(sF + "AND(TYPOP={0})AND(NOMD={1})", AppC.TYPOP_KMPL, xCLoad.drPars4Load["NOMD"]);
                sF = String.Format(sF + "AND(TYPOP={0})AND(NOMD={1})", AppC.TYPOP_KMPL, drZ["NOMD"]);
            }
            else if (xCL.nCommand == AppC.F_LOADOTG)
                sF += "AND(TYPOP=" + AppC.TYPOP_OTGR.ToString() + ")";
            //------

            return ("(" + sF + ")");
        }



        public class ServerExchange
        {
            private int 
                m_RetAppSrv;

            private string 
                m_FullCom = "",
                m_ParString;


            private SocketStream 
                m_ssExchg;
            private MainF 
                xMF;
            private byte[] 
                m_aParsInXML;

            private Dictionary<string, string> 
                dicServAns,
                dicParsAnswer;

            private Srv.ExchangeContext
                m_ExCtxt;

            public void TraiceWiFi(string sE)
            {
                string
                    sIPList,
                    sDTime,
                    sAddInf = "";

                if (MainF.swProt != null)
                {
                    if (xMF.xPars.DebugLevel > 500)
                    {
                        sDTime = DateTime.Now.ToString("dd.MM.yy HH:mm:ss - ") + sE;
                        sIPList = xMF.xBCScanner.WiFi.WiFiInfo();

                        switch (xMF.xBCScanner.nTermType)
                        {
                            case TERM_TYPE.DL_SCORP:
                                sAddInf = String.Format("Сигнал % {0} Уровень {1} Точка '{2}'", 
                                    xMF.xBCScanner.WiFi.SignalPercent, 
                                    xMF.xBCScanner.WiFi.SignalQuality, 
                                    xMF.xBCScanner.WiFi.SSID);
                                break;
                            default:
                                break;
                        }
                        swProt.WriteLine( String.Format("{0} Доп.инф. {1}", sDTime, sAddInf) );
                    }
                }
            }


            private byte[] SetCommand2Srv(string nComCode, string sP, string sMD5)
            {
                string
                    //sCom = "COM=" + nComCode,
                    sCom = String.Format("COM={0};KSK={1}", nComCode, xMF.xSm.nSklad),
                    sUserCode = ";KP=" + xMF.xSm.sUser,
                    sR = ",",
                    sRet = "",
                    sPar = ";";

                DocPars 
                    xP = xMF.xCDoc.xDocP;


                //if (sP.Length > 0)
                //    sPar = ";PAR=" + sP + ";";

                if (FullCOM2Srv.Length > 0)
                    return( Encoding.UTF8.GetBytes(FullCOM2Srv + Encoding.UTF8.GetString(AppC.baTermCom, 0, AppC.baTermCom.Length)) );

                switch (nComCode)
                {
                    case AppC.COM_ZSPR:
                        //sPar = ";PAR=" + sP + ";" + sMD5;
                        sPar = String.Format(";PAR=(KSK={0},BLANK={1});", xP.nSklad, sP) + sMD5;
                        break;
                    case AppC.COM_UNLDZKZ:
                    case AppC.COM_VKMPL:
                        sRet = "NUCH=" + xMF.xCDoc.sLstUchNoms.Replace(',', '/');
                        sRet = "PAR=(" + sRet + ")";

                        sPar = ";" + sRet + ";";
                        break;


                    case AppC.COM_PRNDOC:
                        //sPar = ";PRN=" + sP + ";";
                        sPar = String.Format(";PRN={0};{1}", sP, sMD5);
                        //if (sMD5.Length > 0)
                        //    sPar += sMD5 + ";";
                        break;
                    case AppC.COM_GENFUNC:
                    case AppC.COM_PRNBLK:
                        sPar = String.Format(";{0};", sP);
                        break;
                    case AppC.COM_UNKBC:
                        sPar = String.Format(";{0};", sP);
                        break;

                    case AppC.COM_VINV:
                    case AppC.COM_VTTN:
                    case AppC.COM_VVPER:
                    case AppC.COM_VOPR:
                    case AppC.COM_VMRK:
                    case AppC.COM_GETPRN:
                    case AppC.COM_CHKSCAN:
                    case AppC.COM_CKCELL:
                        break;
                    case AppC.COM_ZKMPLST:
                    case AppC.COM_ZKMPD:
                    case AppC.COM_ZZVK:
                    case AppC.COM_ZSC2LST:

                        xP = (xMF.xCLoad.ilLoad.CurReg == AppC.UPL_CUR) ? xMF.xCDoc.xDocP : xMF.xCLoad.xLP;

                        sRet = "KSK=" + xP.nSklad.ToString();
                        sRet += sR + "DT=" + xP.dDatDoc.ToString("yyyyMMdd");

                        if (nComCode == AppC.COM_ZZVK)
                        {
                            if ((xP.nUch != AppC.EMPTY_INT) && (xP.nUch > 0))
                                sRet += sR + "NUCH=" + xP.nUch.ToString();
                            sRet += sR + "TD=" + xP.nTypD.ToString();
                            if (xP.sSmena != "")
                                sRet += sR + "KSMEN=" + xP.sSmena;
                            if ((xP.nEks != AppC.EMPTY_INT) && (xP.nEks > 0))
                                sRet += sR + "KEKS=" + xP.nEks.ToString();
                            if ((xP.nPol != AppC.EMPTY_INT) && (xP.nPol > 0))
                                sRet += sR + "KPP=" + xP.nPol.ToString();
                            if (xP.sNomDoc != "")
                                sRet += sR + "ND=" + xP.sNomDoc;
                        }
                        if (nComCode == AppC.COM_ZKMPLST)
                        {
                            sRet += sR + "TYPOP=" + ((xMF.xCLoad.nCommand == AppC.F_LOADKPL) ? "KMPL" : "OTGR");
                        }
                        if (nComCode == AppC.COM_ZKMPD)
                        {
                            if (xMF.xCLoad.sSSCC != "")
                            {
                                sRet += sR + "SSCC=" + xMF.xCLoad.sSSCC;
                            }
                            else
                            {
                                if (xP.lSysN != 0)
                                    sRet += sR + "SYSN=" + xP.lSysN.ToString();
                                if (xMF.xSm.LstUchKompl.Length > 0)
                                {
                                    string sx = xMF.xSm.LstUchKompl.Replace(',', '/');
                                    sRet += sR + "NUCH=" + sx;
                                }
                                if ((xP.nEks != AppC.EMPTY_INT) && (xP.nEks > 0))
                                    sRet += sR + "KEKS=" + xP.nEks.ToString();
                                if ((xP.nPol != AppC.EMPTY_INT) && (xP.nPol > 0))
                                    sRet += sR + "KPP=" + xP.nPol.ToString();

                                sRet += sR + "KSMEN=" + xP.sSmena;
                                if (xP.sNomDoc != "")
                                    sRet += sR + "ND=" + xP.sNomDoc;

                            }
                        }
                        if (nComCode == AppC.COM_ZSC2LST)
                        {
                            if (xMF.xCLoad.sSSCC != "")
                                sRet += sR + "SSCC=" + xMF.xCLoad.sSSCC;
                        }

                        if (xMF.xCLoad.ilLoad.CurReg == AppC.UPL_CUR)
                            sRet += sR + "CD=1";

                        xMF.xCLoad.sFilt = sRet;
                        sRet = "PAR=(" + sRet + ")";

                        sPar = ";" + sRet + ";";
                        break;
                    case AppC.COM_VOTG:
                    case AppC.COM_ZOTG:
                    case AppC.COM_ZPRP:
                    case AppC.COM_CCELL:
                    case AppC.COM_CELLI:
                    case AppC.COM_KMCI:
                    case AppC.COM_A4MOVE:
                    case AppC.COM_ADR2CNT:

                        sPar = ";PAR=" + sP + ";";
                        break;
                    case AppC.COM_LOGON:
                        sUserCode = ";KP=" + sMD5;
                        sPar = ";PAR=" + sP + ";";
                        break;
                    default:
                        //sCom = "ERR";
                        //throw new SystemException("Неизвестная команда");
                        break;
                }

                sCom += ";MAC=" + xMF.xSm.MACAdr +
                    sUserCode +
                    sPar +
                    Encoding.UTF8.GetString(AppC.baTermCom, 0, AppC.baTermCom.Length);

                byte[] baCom = Encoding.UTF8.GetBytes(sCom);
                return (baCom);
            }


            public ServerExchange(MainF x)
            {
                xMF = x;
                XMLPars = null;
            }


            private void SelSrvPort(string sCom, string sPar1, out string sH, out int nP)
            {
                int i;

                sH = xMF.xPars.sHostSrv;
                nP = xMF.xPars.nSrvPort;
                if (xMF.xPars.bUseSrvG)
                {
                    switch (sCom)
                    {
                        case AppC.COM_ZSPR:
                            string sLH = (string)xMF.xNSI.BD_TINF_RW(sPar1)["LOAD_HOST"];
                            if (sLH.Length > 0)
                            {
                                try
                                {
                                    int nLP = (int)xMF.xNSI.BD_TINF_RW(sPar1)["LOAD_PORT"];
                                    if (nLP > 0)
                                    {
                                        sH = sLH;
                                        nP = nLP;
                                    }
                                }
                                catch { }
                            }
                            break;
                        case AppC.COM_VINV:
                        case AppC.COM_VVPER:
                        case AppC.COM_VTTN:
                            if (xMF.xCUpLoad.nSrvGind >= 0)
                            {
                                i = xMF.xCUpLoad.nSrvGind - 1;
                                if (xMF.xCUpLoad.nSrvGind == 0)
                                {
                                    i = 0;
                                }
                                else
                                {
                                }
                                sH = xMF.xPars.aSrvG[i].sSrvHost;
                                nP = xMF.xPars.aSrvG[i].nPort;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            // строка команды серверу
            public string FullCOM2Srv
            {
                get { return m_FullCom; }
                set { m_FullCom = value; }
            }

            // поток
            public SocketStream CurSocket
            {
                get { return m_ssExchg; }
                set { m_ssExchg = value; }
            }

            // значение параметра RET в ответе сервера
            public int ServerRet
            {
                get { return m_RetAppSrv; }
                set { m_RetAppSrv = value; }
            }

            // строка параметров ответа
            public string StringAnsPars
            {
                get { return m_ParString; }
                set { m_ParString = value; }
            }

            // список параметров ответа
            public Dictionary<string, string> ServerAnswer
            {
                get { return dicServAns; }
                set { dicServAns = value; }
            }

            // список параметров ответа
            public Dictionary<string, string> AnswerPars
            {
                get { return dicParsAnswer; }
                set { dicParsAnswer = value; }
            }

            // XML-представление параметров общей формы
            public byte[] XMLPars
            {
                get { return m_aParsInXML; }
                set { m_aParsInXML = value; }
            }
            // Контекст вызова внешней формы (dll)
            public Srv.ExchangeContext ExchgContext
            {
                get { return m_ExCtxt; }
                set { m_ExCtxt = value; }
            }


            public bool TestConn(bool bForcibly, BarcodeScanner xBCS, FuncPanel xFP)
            {
                bool ret = true;
                //string sOldInf = xFPan.RegInf;
                WiFiStat.CONN_TYPE cT = xBCS.WiFi.ConnectionType();

                if ((cT == WiFiStat.CONN_TYPE.NOCONNECTIONS) || (bForcibly))
                {
                    bool bHidePan = false;

                    if (!xFP.IsShown)
                    {
                        //xBCS.WiFi.IsEnabled = true;
                        xBCS.WiFi.ShowWiFi((Control)xFP.xPan, true);
                        xFP.ShowP(6, 50, "Переподключение к сети", "Wi-Fi");
                        bHidePan = true;
                    }

                    Cursor crsOld = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    xFP.RegInf = "Переподключение Wi-Fi...";
                    ret = xBCS.WiFi.ResetWiFi(2);
                    if (ret)
                    {
                        Thread.Sleep(4000);
                        xBCS.WiFi.GetIPList();
                        xFP.RegInf = "IP: " + xBCS.WiFi.IPCurrent;
                    }
                    else
                        xFP.RegInf = "Wi-Fi недоступен...";
                    if (bHidePan)
                        xFP.HideP();

                    Cursor.Current = crsOld;
                }
                return (ret);
            }


            public string ExchgSrv(string nCom, string sPar1, string sDop,
                LoadFromSrv dgRead, DataSet dsTrans, ref int ret)
            {
                //return (ExchgSrv(nCom, sPar1, sDop, dgRead, dsTrans, ref ret, 60, -2));
                return (ExchgSrv(nCom, sPar1, sDop, dgRead, dsTrans, ref ret, 180, -2));
            }

            public string ExchgSrv(string nCom, string sPar1, string sDop,
                LoadFromSrv dgRead, DataSet dsTrans, ref int ret, int nTimeOutR)
            {
                return (ExchgSrv(nCom, sPar1, sDop, dgRead, dsTrans, ref ret, nTimeOutR, -2));
            }


            // обмен данными с сервером в формате XML
            // nCom - номер команды
            // sPar1
            // nTOutRead - таймаут на ожидание ответа от сервера
            public string ExchgSrv(string nCom, string sPar1, string sDop,
                LoadFromSrv dgRead, DataSet dsTrans, ref int ret, int nTOutRead, int nBufSize)
            {
                string
                    sOutFileXML = "",
                    sC,
                    sHost,
                    sAdr,
                    sErr;
                int 
                    nPort;

                SocketStream.ASRWERROR 
                    nRErr;

                System.IO.Stream 
                    stm = null;

                ret = AppC.RC_CANCEL;
                ServerRet = AppC.EMPTY_INT;
                if (xMF.xCLoad != null)
                {
                    xMF.xCLoad.xLastSE = this;
                    xMF.xCLoad.sFileFromSrv = "";
                }


                SelSrvPort(nCom, sPar1, out sHost, out nPort);
                sAdr = sHost + ":" + nPort.ToString();
                sErr = sAdr + "-нет соединения!";

                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    CurSocket = new SocketStream(sHost, nPort);
                    if (!TestConn(false, xMF.xBCScanner, xMF.xFPan))
                    {
                        TraiceWiFi(sErr);
                        //throw new System.Net.Sockets.SocketException(11053);
                    }
                    else
                    {
                        //MessageBox.Show("Good reset!");
                    }

                    //TraiceWiFi(nCom + " - перед Connect");
                    stm = CurSocket.Connect();

                    // поток создан, отправка команды
                    sErr = sAdr + "-команда не отправлена";
                    byte[] baCom = SetCommand2Srv(nCom, sPar1, sDop);
                    //stm.Write(baCom, 0, baCom.Length);
                    //stm.Write(AppC.baTermCom, 0, AppC.baTermCom.Length);

                    // 20 секунд на запись команды
                    CurSocket.ASWriteS.TimeOutWrite = 1000 * 10;
                    CurSocket.ASWriteS.BeginAWrite(baCom, baCom.Length);

                    if ((dsTrans != null) || (XMLPars != null))
                    {// передача данных при выгрузке
                        //sErr = sAdr + "-ошибка выгрузки";
                        //dsTrans.WriteXml(stm, XmlWriteMode.IgnoreSchema);
                        //sErr = sAdr + "-ошибка завершения";

                        sErr = sAdr + "-ошибка выгрузки";
                        MemoryStream mst = new MemoryStream();
                        if (dsTrans != null)
                            dsTrans.WriteXml(mst, XmlWriteMode.IgnoreSchema);

                        if (XMLPars != null)
                        {
                            mst.Write(XMLPars, 0, XMLPars.Length);
                        }

                        // терминатор сообщения
                        mst.Write(AppC.baTermMsg, 0, AppC.baTermMsg.Length);

                        byte[] bm1 = mst.ToArray();
                        mst.Close();
                        // 60 секунд на запись данных
                        CurSocket.ASWriteS.TimeOutWrite = 1000 * 180;
                        CurSocket.ASWriteS.BeginAWrite(bm1, bm1.Length);
                    }
                    else
                    {
                        sErr = sAdr + "-ошибка завершения";
                        // 10 секунд на запись терминатора сообщения
                        CurSocket.ASWriteS.TimeOutWrite = 1000 * 30;
                        // терминатор сообщения
                        CurSocket.ASWriteS.BeginAWrite(AppC.baTermMsg, AppC.baTermMsg.Length);
                    }


                    //int nCommLen = 0;
                    //byte[] bAns = ReadAnswerCommand(stm, ref nCommLen);
                    //sC = Encoding.UTF8.GetString(bAns, 0, nCommLen - AppC.baTermCom.Length);

                    sErr = sAdr + "-нет ответа сервера!";
                    // 120 секунд на чтение ответа
                    //m_ssExchg.ASReadS.TimeOutRead = 1000 * 120;

                    //m_ssExchg.ASReadS.BufSize = 256;
                    //nRErr = m_ssExchg.ASReadS.BeginARead(bUseFileAsBuf, 1000 * nTOutRead);

                    if (nBufSize > 0)
                        CurSocket.ASReadS.BufSize = nBufSize;
                    nRErr = CurSocket.ASReadS.BeginARead(1000 * nTOutRead);

                    switch (nRErr)
                    {
                        case SocketStream.ASRWERROR.RET_FULLBUF:   // переполнение буфера
                            sErr = " длинная команда";
                            throw new System.Net.Sockets.SocketException(10061);
                        case SocketStream.ASRWERROR.RET_FULLMSG:   // сообщение полностью получено
                            sC = CurSocket.ASReadS.GetMsg();
                            break;
                        default:
                            TraiceWiFi("Сетевая ошибка (чтение ответа)");
                            throw new System.Net.Sockets.SocketException(10061);
                    }


                    sErr = sAdr + "-ошибка чтения";
                    //Dictionary<string, string> aComm = SrvCommandParse(sC);
                    ServerAnswer = Srv.SrvAnswerParParse(sC);

                    SyncTimeWithSrv();

                    if (ServerAnswer.ContainsKey("PAR"))
                    {
                        StringAnsPars = ServerAnswer["PAR"];
                        StringAnsPars = StringAnsPars.Substring(1, StringAnsPars.Length - 2);
                        AnswerPars = Srv.SrvAnswerParParse(StringAnsPars, new char[] { ',' });
                    }


                    //TraiceWiFi("Ответ получен...");
                    ServerRet = int.Parse(ServerAnswer["RET"]);

                    if ((ServerAnswer["COM"] == nCom) &&
                        ((ServerRet == AppC.RC_OK) ||
                        (ServerRet == AppC.RC_NEEDPARS) ||
                        (ServerRet == AppC.RC_HALFOK)))
                    {
                        CurSocket.ASReadS.OutFile = "";
                        if (ServerRet == AppC.RC_NEEDPARS)
                        {
                            CurSocket.ASReadS.TermDat = AppC.baTermMsg;
                            if (CurSocket.ASReadS.BeginARead(true, 1000 * nTOutRead) == SocketStream.ASRWERROR.RET_FULLMSG)
                            {
                                //TraiceWiFi("Доп.Данные получены...");
                                sOutFileXML = CurSocket.ASReadS.OutFile;
                            }
                            else
                            {
                                TraiceWiFi("Сетевая ошибка (чтение данных)");
                                throw new System.Net.Sockets.SocketException(10061);
                            }
                        }

                        if (dgRead != null)
                            dgRead(CurSocket, ServerAnswer, dsTrans, ref sErr, ServerRet);
                        try
                        {
                            sErr = ServerAnswer["MSG"];
                        }
                        catch { sErr = "OK"; }
                        //dgRead(m_ssExchg, aComm, dsTrans, ref sErr, nRetSrv);
                        //else
                        //{
                        //    sErr = "OK";
                        //}
                    }
                    else
                    {
                        if (ServerAnswer["MSG"] != "")
                            sErr = ServerAnswer["MSG"];
                        else
                            sErr = sAdr + "\n Отложено выполнение";
                    }
                    ret = ServerRet;

                }
                catch (Exception e)
                {
                    //sC = e.Message;
                    sErr = e.Message;
                    TraiceWiFi(sErr);
                    ret = 3;
                }
                finally
                {
                    CurSocket.Disconnect();
                    Cursor.Current = Cursors.Default;
                    if (ServerRet == AppC.RC_NEEDPARS)
                    {
                        if (Srv.ExchangeContext.ExchgReason == AppC.EXCHG_RSN.NO_EXCHG)
                        {
                            Srv.ExchangeContext.ExchgReason = AppC.EXCHG_RSN.SRV_INIT;
                            Srv.ExchangeContext.CMD_EXCHG = nCom;
                            DialogResult xDRslt = xMF.CallDllForm(xMF.sExeDir + "SGPF-Univ.dll", true,
                                new object[] { this, nCom, AppC.R_PARS, sOutFileXML });
                            Srv.ExchangeContext.ExchgReason = AppC.EXCHG_RSN.NO_EXCHG;
                            if (xDRslt == DialogResult.OK)
                            {
                            }
                        }
                    }
                }
                return (sErr);
            }

            private static bool
                //bNeedSync = true;            // для синхронизации по серверу терминалов = true
                bNeedSync = false;
            private bool SyncTimeWithSrv()
            {
                bool
                    ret = false;
                string
                    sFullDT,
                    sCDT,
                    sCTM;
                
                DateTime 
                    dSrv;

                if (!bNeedSync)
                    return (false);

                try
                {
                    sCDT = ServerAnswer["CDT"];
                    sCTM = ServerAnswer["CTM"];
                    sFullDT = String.Format("{0} {1}", sCDT, sCTM);
                    dSrv = DateTime.ParseExact(sFullDT, "yyyyMMdd H:mm:ss", null);
                    dSrv = DateTime.SpecifyKind(dSrv, DateTimeKind.Local).ToUniversalTime();
                    dSrv.AddMilliseconds(-1000);
                    ret  = PDA.Service.TimeSync.SetSystemTime(dSrv);
                    bNeedSync = false;
                    
                    //sFullDT = String.Format("{0} {1}", sCDT, sCTM);
                    //dSrv = DateTime.ParseExact(sFullDT, "yyyyMMdd H:mm:ss", null);
                    //dSrv = DateTime.SpecifyKind(dSrv, DateTimeKind.Local).ToUniversalTime();
                    //ret = SetSystemTime(dSrv);
                    
                }
                catch { ret = false; }
                return(ret);
            }



        }
















    }
}
