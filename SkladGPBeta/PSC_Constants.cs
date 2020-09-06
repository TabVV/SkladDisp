using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.ComponentModel;

using PDA;
using PDA.Service;
using PDA.BarCode;
using ScannerAll;

using FRACT = System.Decimal;

namespace PDA.Service
{
    public partial class AppC
    {

        // ����� ��� ��� �����������
        public const string GUEST = "000";
        //public const int RC_ADM_ONLY = 1;

        // ��� ������
        public const int RC_NODATA = 301;

        // � ������ ��������������
        public const int RC_EDITEND = 6;
        public const int RC_WARN = 7;
        public const int RC_SAMEVES = 8;

        public const int RC_NOEAN       = 10;           // ��������� �����������
        public const int RC_NOEANEMK    = 11;           // ��������� ������ ������� �����������
        public const int RC_ALREADY    = 12;           // ������ �� ��������� ���������
        public const int RC_ZVKONLY     = 13;           // ������ ���� ���� ������
        public const int RC_BADTABLE    = 14;           // ������������ �������
        public const int RC_BADSCAN     = 15;           // ��������� �� �������� � ������
        public const int RC_BADPODD     = 16;           // ��������� �� ������ �������
        public const int RC_UNVISPOD    = 17;           // ��������� �� ����������� �������� View
        public const int RC_NOAUTO      = 18;           // ��������� �� ����������� ���������
        //public const int RC_BADDVR      = 19;           // ���� �������� �� ��������
        public const int RC_NOSSCC      = 20;           // SSCC �����������
        public const int RC_MANYEAN     = 21;           // SSCC �������������� � ������
        public const int RC_BADPARTY    = 22;           // �� �� ������, ��� � ������
        public const int RC_BADDATEXCT  = 23;           // ���������� ���� �� ��������
        public const int RC_NOTADR      = 24;           // SSCC �� ������������ ��� �����
        public const int RC_BADDATE     = 25;           // ���� �������� (��� ���������) �� ��������
        public const int RC_NOTALLDATA  = 26;           // �������� ������

        public const int RC_OPNOTREADY  = 30;           // �������� ��� �� ������

        public const int RC_CONTINUE    = 40;           // ���������� ���������

        // ���� �������� �������
        public const int RC_NEEDPARS    = 50;           // ��� ������ ��������� ���������� ���������
        public const int RC_FILLPARS    = 52;           // ��������� �� �������������, � �����������
        public const int RC_UNCHGPARS   = 54;           // ��������� �� ����������
        public const int RC_HALFOK      = 99;           // �� ��� � �������, ����� ��������� ��������

        // ������ ������ � ��������

        public const int DT_ESC = -1;       // ������ ��������

        public const int DT_SHOW = 0;
        public const int DT_ADDNEW = 1;
        public const int DT_CHANGE = 2;

        public const int DT_LOAD_DOC = 10;  // �������� ����������
        public const int DT_UPLD_DOC = 20;  // �������� ����������

        // ������ ������ ����������
        public const int REG_DOC        = 1;    // ��������������
        //public const int REG_OPR        = 2;    // ������������
        public const int REG_MARK       = 2;    // ����������
        public const string 
            TOTAL_AVAIL                 = "*+", // ��������� ��� ������
            TOTAL_RESTRICT              = "*-"; // ��������� ��� ������

        // ���� ����������
        public const int TYPD_SAM       = 0;    // ���������
        public const int TYPD_CVYV      = 1;    // �����������
        public const int TYPD_SVOD      = 2;    // ����
        public const int TYPD_VPER      = 3;    // ���������� �����������
        public const int TYPD_SCHT      = 4;    // ����
        public const int TYPD_INV       = 5;    // ��������������
        public const int TYPD_OPR       = 6;    // �������� ���� ������� ��������
        public const int TYPD_BRK       = 7;    // ��� �����
        public const int TYPD_PRIH      = 8;    // ��������� �����
        public const int TYPD_ZKZ       = 9;    // ����� �� ������������

        // ���� ��������
        public const int TYPOP_PRMK     = 1;    // ������� � ������������
        public const int TYPOP_MARK     = 2;    // ����������
        public const int TYPOP_OTGR     = 3;    // ��������
        public const int TYPOP_MOVE     = 4;    // ����������� �� ������
        public const int TYPOP_DOCUM    = 5;    // ������ ����� ��������� (�������)
        public const int TYPOP_KMPL     = 6;    // ������������
        public const int TYPOP_KMSN     = 7;    // �����������������
        public const int TYPOP_INVENT   = 8;    // �������������� �� �������

        // �������
        public const int F_CTRLDOC      = 9;    // �������� ���������
        public const int F_CHG_LIST   = 10;   // ����� ����� ����
        public const int F_ADD_SCAN     = 11;   // �������������� ��������������� ������
        public const int F_MAINPAGE     = 12;   // ������� �� ������� �������
        public const int F_NEXTDOC      = 22;   // ��������� ��������
        public const int F_PREVDOC      = 23;   // ���������� ��������
        public const int F_CHG_SORT     = 24;   // ����� ����������
        public const int F_TOT_MEST     = 25;   // ����� ����
        public const int F_SAMEKMC      = 26;   // ����� �� ��� � ���/������
        public const int F_LASTHELP     = 27;   // �������� ��������� ����
        public const int F_CHGSCR       = 28;   // ����� ������������� ������
        public const int F_FLTVYP       = 29;   // ������ ����������� ������
        public const int F_EASYEDIT     = 31;   // ������ ����
        public const int F_PODD         = 32;   // ���� ��������
        //public const int F_PODDPLUS     = 33;   // ��������� ����_��_�������
        //public const int F_PODDMIN      = 34;   // ��������� ����_��_�������
        public const int F_ZVK2TTN      = 37;   // ������� ��������� �� ������ � �����������
        public const int F_BRAKED       = 38;   // ���� �����
        public const int F_SHLYUZ       = 39;   // ���� �������� � �������� ��� ��������
        public const int F_OPROVER      = 44;   // ������������� ���������� ��������
        public const int F_LOADKPL      = 45;   // �������� ������������
        public const int F_SETPODD      = 46;   // ��������� ������/ID �������
        public const int F_LOADOTG      = 47;   // �������� ��������

        public const int F_SETADRZONE   = 48;   // ��������� �������������� ������

        public const int F_PRNDOC       = 49;   // ������ ���������
        public const int F_SETPRN       = 50;   // ��������� �������� ��������

        public const int F_KMCINF       = 51;   // ���������� � ���������� ���������
        public const int F_CELLINF      = 52;   // ���������� � ���������� ������
        public const int F_PRNBLK       = 53;   // ������ ������
        public const int F_CONFSCAN     = 54;   // ����� ������
        public const int F_EXLDPALL     = 55;   // ������ ������� �� �������
        public const int F_SETPODDCUR   = 56;   // ��������� ������/ID ������� ��� ������� �������
        public const int F_MARKWMS      = 57;   // ������� - �������� �� SSCC
        public const int F_STARTQ1ST    = 58;   // � ������ ���������� ��������

        public const int F_A4MOVE       = 59;   // �� ������ ������ �������� ���������� ��� �����������
        public const int F_ADR2CNT      = 60;   // �� ������ ������ �������� ����������
        public const int F_JOINPCS      = 61;   // ���������� ������� � �������
        public const int F_TMPMOV       = 62;   // ��������� ����� ��� �����������

        public const int F_GENFUNC      = 63;   // ����� ������������ �������
        public const int F_REFILL       = 64;   // ���������� ������
        public const int F_GENSCAN      = 65;   // ������ ������������

        public const int F_NEWOPER      = 66;   // ����� ��������
        public const int F_CLRCELL      = 67;   // ������� ����������� ������
        public const int F_CNTSSCC      = 68;   // ���������� SSCC
        public const int F_ZZKZ1        = 69;   // �������� ������ ������ ��� ����������

        public const int F_FLTSSCC      = 70;   // ������ �� SSCC
        public const int F_SSCCSH       = 71;   // ����-���������� SSCC
        public const int F_NEXTPL       = 72;   // ������� �� ��������� ������
        public const int F_CHKSSCC      = 73;   // �������� SSCC
        public const int F_SHOWPIC      = 74;   // ����� �������
        public const int F_LOAD4CHK     = 75;   // �������� ��������� ��� �������������� ��������
        public const int F_CHG_VIEW     = 76;   // ����� ����� ����

        public const int F_VES_CONF     = 150;  // ������������� ����
        public const int F_LOGOFF       = 200;  // ����� ������������
        public const int F_SIMSCAN        = 500;  // ��� �������

        // ���� �������, �� ����������
        public const int F_INITREG = 99999; // ������������� �������/������
        public const int F_INITRUN = 99988; // ������������� � ������ ����������/������
        public const int F_OVERREG = 88888; // ���������� �������/������

        // ���������� ������ (KeyValue �� KeyEventArgs ��� KeyDown)
        // --- ���������

        //public const int K_ESC = 0x1B; // 27 - (Esc)
        //public const int K_ENTER = 0x0D; // 13 - (Enter)
        //public const int K_LOAD_DOC = 0x38; // 56 - (*)

        //public const int K_ADD_DOC_S = 115;          //115 - (F4)
        //public const int K_VIEW_DOC_S = 119;          //119 - (F8) - �������� ���������
        //public const int K_ADD_DOC = 0xBB;         //187 - (+ ��� = (��� Shift))

        //public const int K_CHG_DOC = 0xBD;  //189 - (-)
        //public const int K_HOME = 0xC3;  //195 - (Home)
        //public const int K_QUIT = 0xC4;  //196 - (FN2-Esc)

        // ������ ���������� ������
        internal const string   COM_ZSPR    = "ZSPR";       // �������� ������������
        internal const string   COM_VINV    = "VINV";       // �������� ��������������
        internal const string   COM_VVPER   = "VOTV";       // �������� ���������� �����������
        internal const string   COM_ZZVK    = "ZTTN";       // �������� ������
        internal const string   COM_VTTN    = "VTTN";       // �������� ���

        public const string     COM_ZOTG    = "ZOTG";       // �������� �������� � ��������/������
        public const string     COM_VOTG    = "VOTG";       // �������� �������� � ��������/������

        public const string     COM_ZPRP    = "ZPRP";       // �������� �������� � ��������/������ (�������)
        public const string     COM_CCELL   = "CLRCELL";    // ������� ������
        public const string     COM_CELLI   = "CELLINF";    // ���������� � ������
        public const string     COM_KMCI    = "KMCINF";     // ���������� � ���������� ���������
        public const string     COM_CKCELL  = "CELLCHK";    // �������� ����������� ���������� � ������
        public const string     COM_A4MOVE  = "CELLMOV";    // �� ������ ������ �������� ���������� ��� �����������

        public const string     COM_ADR2CNT = "CELLCTNT";   // �� ������ ������ �������� ����������

        public const string     COM_VOPR    = "VOPR";       // �������� ��������
        public const string     COM_VMRK    = "VMRK";       // �������� ����������

        internal const string   COM_ZKMPLST = "ZLSTZKZ";    // �������� ������ ������� �� ������������ ��� ������
        internal const string   COM_ZKMPD   = "ZZKZ";       // �������� ������ �� ������������
        internal const string   COM_VKMPL   = "VZKZ";       // �������� ������ �� ������������
        internal const string   COM_UNLDZKZ = "UNLDZKZ";    // ����� �� �������������� ������ �� ������������
        public const string     COM_ZSC2LST = "SSCC2LST";   // �������� ������ ��������� �� SSCC

        internal const string   COM_PRNDOC  = "DOCPRN";     // ������ ������ ���������
        internal const string   COM_GETPRN  = "GETPRN";     // �������� ������ ��������� ���������
        public const string     COM_PRNBLK  = "BLKPRN";     // ������ ������������� ���������
        public const string     COM_UNKBC   = "UNKBC";      // ������� ������������ ��������

        public const string     COM_GENFUNC = "GENFUNC";    //  ��������� � ������� ������������� ���������

        internal const string   COM_CHKSCAN = "CONFSCAN";   // ������ ������� �� ������������ ������
        public  const string    COM_MARKWMS = "PRMARK";     // ������� - �������� �� SSCC
        public  const string    COM_REFILL  = "REFILL";     // ���������� ������

        public const string     COM_LOGON   = "LOGON";      // ������ ������� �� �����������

        // ���������� ��� �������
        public static byte[] baTermCom = { 13, 10 };
        // ���������� ��� ������������ ������
        public static byte[] baTermMsg = { 13, 10, 0x2E, 13, 10 };

        // ���� ���������
        public const int PRODTYPE_SHT = 0;                    // �������
        public const int PRODTYPE_VES = 1;                    // �������

        // ���� ��������� ������
        //internal const int TYP_VES_UNK = 0;
        //internal const int TYP_VES_1ED = 1;
        //internal const int TYP_VES_TUP = 2;
        //internal const int TYP_VES_PAL = 3;

        // ���� ����������
        internal const int TYP_BC_OLD   = 1;
        internal const int TYP_BC_NEW = 2;

        internal const int TYP_BC_PALET = 11;

        [Flags]
        public enum TYP_TARA
        {
            UNKNOWN,                                    // �� ����������
            TARA_POTREB,                                  // ����������� �� ������
            TARA_TRANSP,                                  // ����������� �� ������
            TARA_PODDON                                  // ����������� �� ������
        }


        // ���� �������� ��� ����������
        public enum MOVTYPE : int
        {
            PRIHOD = 1,        // ������
            RASHOD = 2,        // ������ 
            AVAIL = 3,        // �������
            MOVEMENT = 4         // ����������
        }


        //public enum OPR_STATE : int
        //{
        //    OPR_EMPTY   = 0,                                    // �������� ��� �� ����������
        //    OPR_START   = 1,                                    // �������� ������
        //    OPR_OVER    = 2,                                    // �������� ��������
        //    OPR_UPL     = 3                                     // �������� ���������
        //}


        [Flags]
        public enum OPR_STATE : int
        {
            OPR_EMPTY       = 0,                            // �������� ��� �� ����������
            OPR_SRC_SET     = 1,                            // �������� ����������
            OPR_DST_SET     = 2,                            // �������� ����������
            OPR_SRV_SET     = 4,                            // ����� � ������� ����������
            OPR_OBJ_SET     = 8,                            // �������� ������
            OPR_READY       = 16,                           // �������� ��������
            OPR_TRANSFERED  = 32,                           // �������� ���������
            OPR_EDITING     = 64                            // �������� �������������
        }

        // ������ ������������ ������
        public enum REG_SWITCH : int
        {
            SW_NEXT     = 0,                                // ��������� �� �������
            SW_CLEAR    = 1,                                // ������������� �����
            SW_SET      = 2                                 // ������������� ���������
        }

        // ��� ������� � ��������� ������
        [Flags]
        public enum OBJ_IN_DROW : int
        {
            OBJ_NONE,                                    // �� ����������
            OBJ_EAN,                                    // EAN
            OBJ_SSCCINT,                                    //
            OBJ_SSCC                                    // 
        }

        // ���� �������� ��� �������� �������� �����
        public enum VALNSI : int
        {
            NO_NSI          = 0,
            ANY_AVAIL       = 1,
            EMPTY_NOT_AVAIL = 11,
            UNKNOWN_CODE    = 21
        }

        //// ������ ��������� ����� ������������
        //public enum WRP_MODES : int
        //{
        //    WRP_BY_NSI      = 1,                            // �� �����������
        //    WRP_ASK_EVERY   = 2,                            // ������ ����������
        //    WRP_ALW_SET     = 4,                            // ������ ����������
        //    WRP_ALW_RESET   = 8                             // ������ �������
        //}

        // ������ ��������� ����� ������������
        public class WRAP_MODES
        {
            public const int
            WRP_BY_NSI = 1,                            // �� �����������
            WRP_ASK_EVERY = 2,                            // ������ ����������
            WRP_ALW_SET = 4,                            // ������ ����������
            WRP_ALW_RESET = 8;                             // ������ �������

            private int
                m_Cur = WRP_BY_NSI;

            // ������� ��� ���������
            public int CurMode
            {
                get { return m_Cur; }
                set { m_Cur = value; }
            }

            public void SwitchNext()
            {
                CurMode =
                (CurMode == WRP_BY_NSI) ? WRP_ASK_EVERY :
                (CurMode == WRP_ASK_EVERY) ? WRP_ALW_SET :
                (CurMode == WRP_ALW_SET) ? WRP_ALW_RESET : WRP_BY_NSI;
            }


            public override string ToString()
            {
                string
                    s = "";
                switch (CurMode)
                {
                    case WRP_BY_NSI:
                        s = "�� �����������";
                        break;
                    case WRP_ASK_EVERY:
                        s = "�� �������";
                        break;
                    case WRP_ALW_SET:
                        s = "������������";
                        break;
                    case WRP_ALW_RESET:
                        s = "��� �������";
                        break;
                    default:
                        s = "����������";
                        break;
                }
                return s;
            }
        }



        // ������ ���������� ����� ��������
        public const int OPOV_SCPROD = 1;                    // ������������ ���������
        public const int OPOV_SCADR2 = 2;                    // ������������ ����������


        public const int KRKMC_MIX   = 69;                    // ������� ��� ��� ������� ��������

        // 
        internal const string 
                sIDTmp = "TMPOPR";

        // ����� ������� ��������������
        internal const string DOC_CONTROL = "ControlDoc";
        /// �������� ���������� ���������
        /// object xRet = xDocControl.run.ExecFunc
        /// ���������: DOC_CONTROL, 
        /// new object[] { dr, childRowsZVK, childRowsTTN, lstStr }, actDocControl);
        /// nRet = (int)xRet;
        internal const string SCAN_OVER = "ScanOver";
        /// ����� ��������� ����������� ������������
        /// object xRet = xDocControl.run.ExecFunc(DOC_CONTROL, new object[] { dr, childRowsZVK, childRowsTTN, 
        /// ���������: SCAN_OVER
        ///                                        lstStr }, actDocControl);
        /// nRet = (int)xRet;
        /// 
        internal const string 
            FEXT_ADR_NAME       = "NameAdr",                // ���������� ������������� ������
            FEXT_CONF_SCAN      = "ConfScan";               // ������������� ����� �� �������

        // ������ ��������
        public const int FX_PRPSK = 1;         // �� ��������
        public const int FX_PTLST = 2;         // �� �������� �����

        // ����� ������ ����� ������ � ��������
        public const int R_BLANK = 1;           // ����� ������
        public const int R_PARS = 2;            // ��������� ���������� �� �������

        // �������� ����� ����������
        public static Dictionary<int, SkladGP.DocTypeInf>
                        xDocTInf;

    }
}

namespace SkladGP
{
    
    public sealed class AppPars
    {
        public static int MAXDocType = 9;

        // ������� ������� � �������
        public static int MAXProductsType = 2;
        public static int MAXFields = 7;

        public struct ParsForMType
        {
            public bool bMestConfirm;
            public bool bMAX_Kol_EQ_Poddon;
            public int nDefEmkVar;
            public bool b1stPoddon;
        }

        // ��������� ��� ���� ��������� (������� ��� �������)
        public struct OneVesPars
        {
            public bool bScan;
            public bool bEdit;
            public bool bVvod;
            public string sDefVal;

            public OneVesPars(bool bV)
            {
                bScan = false; 
                bEdit = false; 
                bVvod = bV; 
                sDefVal = "";
            }

            public OneVesPars(bool bS, bool bE, bool bV, string sDV)
            {
                bScan = bS; bEdit = bE; bVvod = bV; sDefVal = sDV;
            }


        }

        // ��������� ��� ���� ���������
        public struct ParsForDoc
        {
            public bool bShowFromZ;
            public bool bTestBefUpload;
            public bool bSumVes;
        }

        public class FieldDef
        {
            //public int nFieldNum;
            public string sFieldName;
            public OneVesPars[] aVes = new OneVesPars[MAXProductsType];

            //public FieldDef(string sF)
            //{
            //    sFieldName = sF;
            //    aVes = new OneVesPars[MAXProductsType];
            //}
            public void SetFieldDef(string sF, OneVesPars[] aP)
            {
                sFieldName = sF;
                aVes = aP;
            }
        }

        // ��������� ��� ��������
        public struct ParsForSrvOp
        {
            public string sOper;
            public bool bUse;
        }

        public class ServerPool
        {
            public string sSrvComment;
            public string sSrvHost;
            public bool bActive;
            public int nPort;
            public WiFiStat.CONN_TYPE ConType;
            public string sProfileWiFi;

            public ParsForSrvOp[] aSrvOp = new ParsForSrvOp[7];
        }


        //===***===
        // ���� � ��������� �����
        private string m_AppStore;
        // ���� � ���
        private string m_NSIPath;
        // ���� � ������
        private string m_DataPath;

        // HOST-m_Name �������
        private string m_Host;
        // � ����� ������� (����� �������)
        private int m_SrvPort;
        // � ����� ������� (����� �����������)
        private int m_SrvPortM;
        // NTP-������
        private string m_NTP;

        // ���/���� ����� ����������� � ��������
        private bool m_WaitSock;
        // ��������������
        private bool m_AutoSave = false;
        // ������ ��������
        private bool m_UseSrvG = false;

        private string
            m_AppAvailModes = "1+";

        //-----*****-----*****-----
        // ������� ���� (������)
        private int m_CurField;
        // ������� ��� ��������� (������)
        private int m_CurVesType;
        // ������ �� ���������� ����� ��� ������ ������������
        private bool m_WarnNewScan = false;

        
        private int
            m_CurDocType,                   // ������� ��� ���������
            m_Days2Save,                    // ���� �������� ����������
            m_DebugLevel = 0,               // ������� �������
            m_ReLogon;                      // ������� ���������� ������ (�����)

        
        private bool
            m_HidUpl;                       // �������� ����������� ���������

        //===***===
        private bool 
            m_OpAutoUpl = true,                     // ����-�������� ��� ��������
            m_UseFixAddr = false,                   // ������������ ������������� ������
            m_OpChkAdr = true,                      // �������� ������ ��� ��������
            m_UseAdr4DocMode = false,               // ������������� ������� � �������������� ������

            m_ConfScan = true,                      // ����������� ������ ����� ������������
            m_Ask4biddScan = true,                  // ������� �� ����������� ������������
            m_BadPartyForbidd = true,               // ������� ������ �� ������ ������/����

            m_SendTG2WMS = true,                    // ������ ��� ���������� "������"
            m_CanEditIDNum = false;                 // ���� � ID-����� �������

        //private AppC.WRP_MODES
        //    m_WrapMode = AppC.WRP_MODES.WRP_BY_NSI; // �������� ��� ���������� "������������"

        private AppC.WRAP_MODES
            m_WrapMode = new AppC.WRAP_MODES();    // �������� ��� ���������� "������������"

        // ������ ���������� ��������
        private int 
            m_OpOver = AppC.OPOV_SCPROD;


        // ������� ���������� �������� ������ � Shift
        public static bool 
            bArrowsWithShift = true;



        /// ������ �����
        /// 

        #region ����� ����������� ��� ������ �������
        
        public static bool
            bVesNeedConfirm = true,                         // ������������� ���� ��� �������� ������
            ShowSSCC = true,
            bUseHours = false;                              // ������������ ���� � ������ ����������

        #endregion

        // ���������� ������ ��� �������� ������
        //public bool parVvodVESNewRec = true;

        // ���������� ������ ��� �������� ������
        //public bool parVvodSHTNewRec = false;

        // ����������� ����� ���������� ������ � ������
        public bool parVvodShowExact = true;

        /// ������ ������ � �����������
        // �������� ���������� ��� ����������� � ����
        //public bool parDocControl = false;



        // ������� � �����������
        //private static string sFilePars = NSI.sPathBD + "TermPars.xml";
        private static string sFilePars = "TermPars.xml";
        //private static NSI xNSI = null;

        public AppPars()
        {
            m_AppStore = @"\BACKUP\OAO_SP\SkladGP";
            m_NSIPath = @"\BACKUP\BDGP\";
            m_DataPath = @"\BACKUP\BDGP\";

            m_Host = "BPR_SERV3";
            m_NTP  = "10.0.0.221";
            m_SrvPort = 11010;
            m_SrvPortM = 11001;
            m_WaitSock = false;
       
            m_UseSrvG = false;

            CurVesType = CurDocType = CurField = 0;

            aFields[0] = new FieldDef();
            aFields[0].SetFieldDef("tKMC", new OneVesPars[2] { 
                new OneVesPars(true), 
                new OneVesPars(true) });

            aFields[1] = new FieldDef();
            aFields[1].SetFieldDef("tParty", new OneVesPars[2] { 
                new OneVesPars(true), 
                new OneVesPars(true) });

            aFields[2] = new FieldDef();
            aFields[2].sFieldName = "tEAN";

            aFields[3] = new FieldDef();
            aFields[3].SetFieldDef("tDatMC", new OneVesPars[2] { 
                new OneVesPars(true), 
                new OneVesPars(true) });

            aFields[4] = new FieldDef();
            aFields[4].SetFieldDef("tMest", new OneVesPars[2] { 
                new OneVesPars(true, true, true, "1"), 
                new OneVesPars(true, true, true, "1") });

            aFields[5] = new FieldDef();
            aFields[5].SetFieldDef("tEmk", new OneVesPars[2] { 
                new OneVesPars(true), 
                new OneVesPars(true) });

            aFields[6] = new FieldDef();
            aFields[6].SetFieldDef("tVsego", new OneVesPars[2] { 
                new OneVesPars(true), 
                new OneVesPars(true) });

            // ��������� �� ����� ����������
            //--- �������
            aParsTypes[AppC.PRODTYPE_SHT].bMestConfirm = true;
            aParsTypes[AppC.PRODTYPE_SHT].bMAX_Kol_EQ_Poddon = true;
            aParsTypes[AppC.PRODTYPE_SHT].nDefEmkVar = 0;
            aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon = false;

            aParsTypes[AppC.PRODTYPE_VES].bMestConfirm = true;
            aParsTypes[AppC.PRODTYPE_VES].bMAX_Kol_EQ_Poddon = true;
            aParsTypes[AppC.PRODTYPE_VES].nDefEmkVar = 20;
            aParsTypes[AppC.PRODTYPE_VES].b1stPoddon = false;

            m_WarnNewScan = true;

            // ��������� �� ����� ����������
            SetArrDoc(ref this.aDocPars);

            m_Days2Save = 100;
            ReLogon = -1;
            m_HidUpl = false;
            OpAutoUpl = true;
        }

        //---***---***---
        #region ����� ���������

        // ���� � ��������� �����
        public string sAppStore
        {
            get { return m_AppStore; }
            set { m_AppStore = value; }
        }
        // ���� � ���
        public string sNSIPath
        {
            get { return m_NSIPath; }
            set { m_NSIPath = value; }
        }
        // ���� � ������
        public string sDataPath
        {
            get { return m_DataPath; }
            set { m_DataPath = value; }
        }

        // HOST-m_Name �������
        public string sHostSrv
        {
            get { return m_Host; }
            set { m_Host = value; }
        }
        // � ����� ������� (����� �������)
        public int nSrvPort
        {
            get { return m_SrvPort; }
            set { m_SrvPort = value; }
        }

        // NTP-������
        public string NTPSrv
        {
            get { return m_NTP; }
            set { m_NTP = value; }
        }


        // � ����� ������� (����� �����������)
        public int nSrvPortM
        {
            get { return m_SrvPortM; }
            set { m_SrvPortM = value; }
        }
        // ���/���� ����� ����������� � ��������
        public bool bWaitSock
        {
            get { return m_WaitSock; }
            set { m_WaitSock = value; }
        }
        // ��������������
        public bool bAutoSave
        {
            get { return m_AutoSave; }
            set { m_AutoSave = value; }
        }
        // ������ ��������
        public bool bUseSrvG
        {
            get { return m_UseSrvG; }
            set { m_UseSrvG = value; }
        }

        // ���������� ������ ������ ����������
        public string AppAvailModes
        {
            get { return m_AppAvailModes; }
            set { m_AppAvailModes = value; }
        }


        // �������� �� ������ ��� ���������� � �������
        public bool GetAdrContentFromSrv
        {
            get { return m_UseAdr4DocMode; }
            set { m_UseAdr4DocMode = value; }
        }

        // ������������� ������� � �������������� ������
        public bool UseAdr4DocMode 
        {
            get { return m_UseAdr4DocMode ; }
            set { m_UseAdr4DocMode = value; }
        }

        // ������������� ������� � �������������� ������
        public int DebugLevel
        {
            get { return m_DebugLevel; }
            set { m_DebugLevel = value; }
        }


        #endregion
        //-----*****-----*****-----
        #region ��������� �����

        // �������� ����� ������ ��� ����� ���������
        public ParsForMType[] aParsTypes = new ParsForMType[MAXProductsType];

        // �������� ����� ������ ��� �����
        public FieldDef[] aFields = new FieldDef[MAXFields];

        // ������� ����
        public int CurField
        {
            get { return m_CurField; }
            set { m_CurField = value; }
        }
        // ������� ���
        public int CurVesType
        {
            get { return m_CurVesType; }
            set { m_CurVesType = value; }
        }

        // ���� ��������� ����� ������� (����� � �����������)
        //public bool bAddNewRow
        //{
        //    get { return aParsTypes[CurVesType].bAddNewRow; }
        //    set { aParsTypes[CurVesType].bAddNewRow = value; }
        //}

        // ������������� ���� ��� �����
        public bool bConfMest
        {
            get { return aParsTypes[CurVesType].bMestConfirm; }
            set { aParsTypes[CurVesType].bMestConfirm = value; }
        }
        // ������������ ���������� - ������
        public bool bMaxKolEQPodd
        {
            get { return aParsTypes[CurVesType].bMAX_Kol_EQ_Poddon; }
            set { aParsTypes[CurVesType].bMAX_Kol_EQ_Poddon = value; }
        }

        // ������� ���������� ���� ������ �����
        public int MaxVesVar
        {
            get { return aParsTypes[CurVesType].nDefEmkVar; }
            set { aParsTypes[CurVesType].nDefEmkVar = value; }
        }

        // � ������ ���������� �������� (false - ������� �� �������, true - ������ �������)
        public bool bStart1stPoddon
        {
            get { return aParsTypes[CurVesType].b1stPoddon; }
            set { aParsTypes[CurVesType].b1stPoddon = value; }
        }


        // ����������� ���� ����� ������������
        public bool bAfterScan
        {
            get { return aFields[CurField].aVes[CurVesType].bScan; }
            set { aFields[CurField].aVes[CurVesType].bScan = value; }
        }
        // ����������� ���� ��� ��������������
        public bool bEdit
        {
            get { return aFields[CurField].aVes[CurVesType].bEdit; }
            set { aFields[CurField].aVes[CurVesType].bEdit = value; }
        }
        // ����������� ���� ��� �����
        public bool bManual
        {
            get { return aFields[CurField].aVes[CurVesType].bVvod; }
            set { aFields[CurField].aVes[CurVesType].bVvod = value; }
        }
        // ������ �� ���������� ����� ��� ������ ������������
        public bool WarnNewScan
        {
            get { return m_WarnNewScan; }
            set { m_WarnNewScan = value; }
        }

        // ������� �� ����������� ������������
        public bool Ask4biddScan 
        {
            get { return m_Ask4biddScan; }
            set { m_Ask4biddScan = value; }
        }

        // ������� ������ �� ������ ������/����
        public bool BadPartyForbidd
        {
            get { return m_BadPartyForbidd; }
            set { m_BadPartyForbidd = value; }
        }


        #endregion

        #region ��������� ����������

        // �������� ��� ����� ���������
        //public ParsForDoc[] aDocPars = new ParsForDoc[MAXDocType + 1];
        public ParsForDoc[] aDocPars = new ParsForDoc[0];

        // ������� ��� ���������
        public int CurDocType
        {
            get { return m_CurDocType; }
            set { m_CurDocType = value; }
        }

        // ���������� �� ��������� �� ������
        public bool bKolFromZvk
        {
            get { return aDocPars[CurDocType].bShowFromZ; }
            set { aDocPars[CurDocType].bShowFromZ = value; }
        }

        // �������� ��������� ����� ���������
        public bool bTestBeforeUpload
        {
            get { return aDocPars[CurDocType].bTestBefUpload; }
            set { aDocPars[CurDocType].bTestBefUpload = value; }
        }

        // ����������� ������� ���������
        public bool bSumVesProd
        {
            get { return aDocPars[CurDocType].bSumVes; }
            set { aDocPars[CurDocType].bSumVes = value; }
        }

        // ���� �������� ���������
        public int Days2Save
        {
            get { return m_Days2Save; }
            set { m_Days2Save = value; }
        }

        // ������� ���������� ������ (�����)
        public int ReLogon
        {
            get { return m_ReLogon; }
            set { m_ReLogon = value; }
        }

        // �������� ����������� ���������
        public bool bHideUploaded
        {
            get { return m_HidUpl; }
            set { m_HidUpl = value; }
        }

        // ����������� ������ ����� ������������
        public bool ConfScan
        {
            get { return m_ConfScan; }
            set { m_ConfScan = value; }
        }

        // ���� � ID-����� �������
        public bool CanEditIDNum 
        {
            get { return m_CanEditIDNum ; }
            set { m_CanEditIDNum = value; }
        }

        // ������ ��� ���������� "������"
        public bool SendTG2WMS
        {
            get { return m_SendTG2WMS ; }
            set { m_SendTG2WMS = value; }
        }

        // ������ ��� ���������� "������������"
        //public AppC.WRP_MODES WrapMode
        //{
        //    get { return m_WrapMode; }
        //    set { m_WrapMode = value; }
        //}

        // ������ ��� ���������� "������������"
        public AppC.WRAP_MODES WrapMode
        {
            get { return m_WrapMode; }
            set { m_WrapMode = value; }
        }


        #endregion


        #region ��������� ������������� ������
        //===***===
        // ����-�������� ��� ��������
        public bool OpAutoUpl
        {
            get { return m_OpAutoUpl; }
            set { m_OpAutoUpl = value; }
        }

        // ������ ���������� ��������
        public int OpOver
        {
            get { return m_OpOver; }
            set { m_OpOver = value; }
        }

        // �������� ������ ��� ��������
        public bool OpChkAdr
        {
            get { return m_OpChkAdr; }
            set { m_OpChkAdr = value; }
        }

        // ������������ ������������� ������
        public bool UseFixAddr 
        {
            get { return m_UseFixAddr ; }
            set { m_UseFixAddr = value; }
        }

        #endregion



        #region ��������� ��������

        // �������� ��� ����� ���������
        public ServerPool[] aSrvG;

        #endregion




        private static bool SetArrDoc(ref ParsForDoc[] aP)
        {
            bool ret = AppC.RC_CANCELB;
            int nOldLen = aP.Length;
            if (nOldLen < (MAXDocType + 1))
            {
                ParsForDoc[] aDP = new ParsForDoc[MAXDocType + 1];
                aP.CopyTo(aDP, 0);
                // ��������� �� ����� ����������
                nOldLen--;
                if (nOldLen < AppC.TYPD_SAM)
                {
                    aDP[AppC.TYPD_SAM].bShowFromZ = true;
                    //aDP[AppC.TYPD_SAM].bTestBefUpload = false;
                    //aDP[AppC.TYPD_SAM].bSumVes = false;
                }
                if (nOldLen < AppC.TYPD_CVYV)
                {
                    aDP[AppC.TYPD_CVYV].bShowFromZ = true;
                    //aDP[AppC.TYPD_CVYV].bTestBefUpload = false;
                    //aDP[AppC.TYPD_CVYV].bSumVes = false;
                }
                if (nOldLen < AppC.TYPD_SVOD)
                {
                    aDP[AppC.TYPD_SVOD].bShowFromZ = true;
                    //aDP[AppC.TYPD_SVOD].bTestBefUpload = false;
                    //aDP[AppC.TYPD_SVOD].bSumVes = false;
                }
                if (nOldLen < AppC.TYPD_VPER)
                {
                    aDP[AppC.TYPD_VPER].bShowFromZ = true;
                    //aDP[AppC.TYPD_VPER].bTestBefUpload = false;
                    //aDP[AppC.TYPD_VPER].bSumVes = false;
                }
                if (nOldLen < AppC.TYPD_SCHT)
                {
                    aDP[AppC.TYPD_SCHT].bShowFromZ = true;
                    //aDP[AppC.TYPD_SCHT].bTestBefUpload = false;
                    //aDP[AppC.TYPD_SCHT].bSumVes = false;
                }
                if (nOldLen < AppC.TYPD_INV)
                {
                    aDP[AppC.TYPD_INV].bShowFromZ = false;
                    //aDP[AppC.TYPD_INV].bTestBefUpload = false;
                    aDP[AppC.TYPD_INV].bSumVes = true;
                }
                if (nOldLen < AppC.TYPD_OPR)
                {
                    aDP[AppC.TYPD_OPR].bShowFromZ = false;
                    //aDP[AppC.TYPD_OPR].bTestBefUpload = false;
                    //aDP[AppC.TYPD_OPR].bSumVes = false;
                }
                if (nOldLen < AppC.TYPD_BRK)
                {
                    aDP[AppC.TYPD_BRK].bShowFromZ = false;
                    //aDP[AppC.TYPD_BRK].bTestBefUpload = false;
                    //aDP[AppC.TYPD_BRK].bSumVes = false;
                }
                // ��������� �����
                if (nOldLen < AppC.TYPD_PRIH)
                    aDP[AppC.TYPD_BRK].bShowFromZ = false;
                if (nOldLen < AppC.TYPD_ZKZ)
                {
                    aDP[AppC.TYPD_BRK].bShowFromZ = true;
                    //aDP[AppC.TYPD_BRK].bTestBefUpload = false;
                    //aDP[AppC.TYPD_BRK].bSumVes = false;
                }
                aP = aDP;
                ret = AppC.RC_OKB;
            }
            return (ret);
        }

        public static object InitPars(string sPath)
        {
            bool bNeedSave = false;
            int nRet = AppC.RC_OK;
            object xx = null;
            AppPars xNew = null;

            sFilePars = sPath + "\\" + sFilePars;

            nRet = Srv.ReadXMLObj(typeof(AppPars), out xx, sFilePars);
            xNew = (AppPars)xx;
            if (nRet != AppC.RC_OK)
            {
                if (xNew == null)
                {
                    bNeedSave = true;
                    xNew = new AppPars();
                }
            }
            else
            {// ��������� � �����
                xNew = (AppPars)xx;
                bNeedSave = SetArrDoc(ref xNew.aDocPars);
                // ���������� �� ������� �� ��������� ��� ������� ����
                if (xNew.aParsTypes[AppC.PRODTYPE_VES].nDefEmkVar == 0)
                    xNew.aParsTypes[AppC.PRODTYPE_VES].nDefEmkVar = 20;
                xNew.AppAvailModes = xNew.AppAvailModes.PadRight(2,'+');
            }
            if ((xNew.aSrvG == null) || (xNew.aSrvG.Length == 0))
            {
                xNew.aSrvG = new ServerPool[]{
                    new ServerPool(),
                    new ServerPool()
                };
                xNew.aSrvG[0].sSrvComment = "������";
                xNew.aSrvG[0].sSrvHost = "TRESERV";
                xNew.aSrvG[0].nPort = 11010;
                xNew.aSrvG[0].bActive = true;
                xNew.aSrvG[0].ConType = WiFiStat.CONN_TYPE.ACTIVESYNC;

                xNew.aSrvG[1].sSrvComment = "������";
                xNew.aSrvG[1].sSrvHost = "213.184.242.101";
                xNew.aSrvG[1].nPort = 17210;
                xNew.aSrvG[1].bActive = true;
                xNew.aSrvG[1].ConType = WiFiStat.CONN_TYPE.WIFI;
                xNew.aSrvG[1].sProfileWiFi = "BmkTerm";
            }
            if (bNeedSave)
                SavePars(xNew);
            return (xNew);
        }

        public static int SavePars(AppPars x)
        {
            return (Srv.WriteXMLObjTxt(typeof(AppPars), x, sFilePars));
        }


    }

    /// ����� ����������� (� �������������)
    public class StrAndInt
    {
        private string
            m_Name,
            m_NameAdd1 = "",
            m_NameAdd2 = "";
        private int
            m_Code,
            m_CodeAdd1 = -1,
            m_CodeAdd2 = -1,
            m_CodeAdd3 = -1;
        private FRACT
            m_Dec;
        private DataRow
            m_DRow = null;

        public StrAndInt() { }

        public StrAndInt(string s, int i)
        {
            SName = s;
            IntCode = i;
        }
        public StrAndInt(string s, object i, object sa1, object sa2, object ia1, object ia2)
        {
            SName = s;
            IntCode = (i is int) ? (int)i : 0;

            //SNameAdd1 = (sa1 is string) ? (string)sa1 : "";
            if (sa1 is string)
            {
                SNameAdd1 = (string)sa1;
                m_CodeAdd3 = 0;
            }
            else
            {
                SNameAdd1 = "";
                m_CodeAdd3 = (sa1 is int) ? (int)sa1 : 0;
            }

            //SNameAdd2 = (sa2 is string) ? (string)sa2 : "";
            if (sa2 is string)
            {
                SNameAdd2 = (string)sa2;
                m_DRow = null;
            }
            else
            {
                SNameAdd2 = "";
                m_DRow = (sa2 is DataRow) ? (DataRow)sa2 : null;
            }

            IntCodeAdd1 = (ia1 is int) ? (int)ia1 : 0;
            IntCodeAdd2 = (ia2 is int) ? (int)ia2 : 0;
        }

        public string SName
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        public string SNameAdd1
        {
            get { return m_NameAdd1; }
            set { m_NameAdd1 = value; }
        }
        public string SNameAdd2
        {
            get { return m_NameAdd2; }
            set { m_NameAdd2 = value; }
        }

        public int INumber
        {
            get { return m_Code; }
            set { m_Code = value; }
        }
        public int IntCode
        {
            get { return m_Code; }
            set { m_Code = value; }
        }
        public int IntCodeAdd1
        {
            get { return m_CodeAdd1; }
            set { m_CodeAdd1 = value; }
        }
        public int IntCodeAdd2
        {
            get { return m_CodeAdd2; }
            set { m_CodeAdd2 = value; }
        }
        public int IntCodeAdd3
        {
            get { return m_CodeAdd3; }
            set { m_CodeAdd3 = value; }
        }
        public FRACT DecDat
        {
            get { return m_Dec; }
            set { m_Dec = value; }
        }
        public DataRow NSIRow
        {
            get { return m_DRow; }
            set { m_DRow = value; }
        }
    }

    /// ������ ������������-�����-������
    public class RowObj
    {
        public RowObj(DataRow d) { WhatObjInDRow(d); }

        public bool IsEAN = false;
        public string EAN13 = "";

        public bool IsSSCCINT = false;
        public string sSSCCINT = "";

        public bool IsSSCC = false;
        public string sSSCC = "";

        public AppC.OBJ_IN_DROW AllFlags = AppC.OBJ_IN_DROW.OBJ_NONE;

        public RowObj WhatObjInDRow(DataRow drC)
        {

            EAN13 = (drC["EAN13"] == System.DBNull.Value) ? "" : drC["EAN13"].ToString();
            sSSCCINT = (drC["SSCCINT"] == System.DBNull.Value) ? "" : drC["SSCCINT"].ToString();
            sSSCC = (drC["SSCC"] == System.DBNull.Value) ? "" : drC["SSCC"].ToString();

            if (EAN13.Length > 0)
            {
                IsEAN = true;
                AllFlags |= AppC.OBJ_IN_DROW.OBJ_EAN;
            }
            if (sSSCCINT.Length > 0)
            {
                IsSSCCINT = true;
                AllFlags |= AppC.OBJ_IN_DROW.OBJ_SSCCINT;
            }
            if (sSSCC.Length > 0)
            {
                IsSSCC = true;
                AllFlags |= AppC.OBJ_IN_DROW.OBJ_SSCC;
            }
            return (this);
        }


    }

    // ���� �������

    public class Smena
    {
        public struct ObjInf
        {
            public string ObjName;
        }

        // ����������� ������� ����������� ��� ��������
        public static int MIN_TIMEOUT = 2;

        //public static Dictionary<string, ExprAct> xDD = null;


        private string m_OldUser;                       // ��� ����������� ������������
        //private int m_Sklad;                          // ��� ������
        //private int m_Uch;                            // ��� �������


        private string m_SDate = "";                        // ���� �� ���������
        // ���� ����������
        public string DocData
        {
            get { return m_SDate; }
            set
            {
                try
                {
                    DateDef = DateTime.ParseExact(value, "dd.MM.yy", null);
                }
                catch
                {
                    DateDef = DateTime.Now;
                }
                m_SDate = DateDef.ToString("dd.MM.yy");
            }
        }

        //public Dictionary<string, ExprAct> xExpDic = null;

        public static DateTime DateDef;                 // ���� �� ���������
        public static int SklDef = 0;                   // ��� ������
        public static int UchDef = 0;                   // ��� �������

        public static string 
            EnterPointID = "",
            SmenaDef = "";                              // ��� �����

        //public static int 
        //    TypDef = AppC.TYPD_SVOD;                    // ��� ���������


        private static string sXML = "CS.XML";          // ��� ����� � ����������� ������������/�����

        private DataRow
            m_DocBefTmpMove = null;

        private int
            m_DocType = AppC.TYPD_VPER,                 // ��� ���������
            m_RegApp = AppC.REG_DOC,                    // ����� ������  �� ��������� - � �����������
            m_CurrNum4Invent = 0;                       // ������� ����� ��� ��������� �������

        // ������ �������� ��� ������������
        private List<int> 
            aUch = null;

        // ������� �������
        //private int m_CurPrn = -1;

        // ������ ���������
        //public ObjInf[] aPrn = null;

       
        private string
            //m_DevID = "",
            m_MAC = "000000000000",                     // MAC-�����
            m_LstUch = "",                              // ������ �������� ��� ������������
            //m_FilterTTN = "",                           // ������� ������ ���
            m_CurPrnMOB = "",                           // ������� ��������� �������
            m_CurPrnSTC = "";                           // ������� ������������ �������

            
        public static BindingList<StrAndInt> 
            bl;

        // ����� �������������
        public enum USERRIGHTS : int
        {
            USER_KLAD       = 1,                        // ���������
            USER_BOSS_SMENA = 10,                       // ��������� �����
            USER_BOSS_SKLAD = 100,                      // ��������� ������
            USER_ADMIN      = 1000,                     // ��������� �����
            USER_SUPER      = 2000                      // 
        }

        // ��������� ������� ����� ������������
        public string sUser = "";                       // ��� ������������
        public string sUName = "";                      // ���
        public string sUserPass = "";
        public string sUserTabNom = "";
        public USERRIGHTS urCur;                        // ������� �����    


        // ����� (� �������) �� ����� ����� (������ ������)
        public TimeSpan tMinutes2SmEnd = TimeSpan.FromMinutes(0);
        //public DateTime dtSmEnd;

        // ������ �� ����� ����� ��� ������������
        public Timer xtmSmEnd = null;

        // ������ �� ������� ��������� ������ �����
        public Timer xtmTOut = null;

        // ������������ �������� � ��������
        public int nMSecondsTOut = 0;



        // ����-����� ��������� �������� ���� ������������
        //public DateTime dtLoadNS;

        public DateTime dBeg;                           // ������ �����
        public DateTime dEnd;                           // ��������� �����
        public int nLogins;

        public bool bInLoadUpLoad = false;

        public int nDocs = -1;

        // ��� ������
        public int nSklad
        {
            get { return SklDef; }
            set { SklDef = value; }
        }

        // ��� �������
        public int nUch
        {
            get { return UchDef; }
            set
            {
                UchDef = value;
                Uch2Lst(value, true);
            }
        }

        // ��� �����
        public string DocSmena
        {
            get { return SmenaDef; }
            set { SmenaDef = value; }
        }

        // ��� ���� ���������
        public int DocType
        {
            get { return m_DocType; }
            set { m_DocType = value; }
        }


        // ����� ������ ����������
        public int RegApp
        {
            get { return m_RegApp; }
            set { m_RegApp = value; }
        }

        // ������ �������� ��� ������������
        public string LstUchKompl
        {
            get { return m_LstUch; }
            set { m_LstUch = value; }
        }

        public MainF.AddrInfo 
            xAdrForSpec = null,
            xAdrFix1 = null;


        // ������� �������
        //public int CurPrinter
        //{
        //    get { return m_CurPrn; }
        //    set { m_CurPrn = value; }
        //}

        // ��� �������� ��������
        //public string CurPrinterName
        //{
        //    get { return ((m_CurPrn >= 0)?aPrn[m_CurPrn].ObjName : ""); }
        //}

        // ��� �������� (����������) ��������
        public string CurPrinterMOBName
        {
            get { return m_CurPrnMOB; }
            set { m_CurPrnMOB = value; }
        }

        // ��� �������� (�������������) ��������
        public string CurPrinterSTCName
        {
            get { return m_CurPrnSTC; }
            set { m_CurPrnSTC = value; }
        }


        // ��� ������
        public int Curr4Invent
        {
            get { return m_CurrNum4Invent; }
            set { m_CurrNum4Invent = value; }
        }

        // MAC-�����
        public string MACAdr
        {
            get { return m_MAC; }
            set { m_MAC = value; }
        }

        // ����� ���������, �� �������� ������� ��������� �����������
        public DataRow DocBeforeTmpMove(object drSave)
        {
            DataRow
                ret = m_DocBefTmpMove;

            if (!(drSave is int))
                m_DocBefTmpMove = (DataRow)drSave;
            return (ret);
        }


        public static int ReadSm(ref Smena xS, string sPath)
        {
            object x;
        
            bl = new BindingList<StrAndInt>();
            bl.Add(new StrAndInt("��������������", AppC.REG_DOC));
            //bl.Add(new StrAndInt("������������", AppC.REG_OPR));
            bl.Add(new StrAndInt("����������", AppC.REG_MARK));

            int nRet = Srv.ReadXMLObj(typeof(Smena), out x, sPath + sXML);
            if (nRet == AppC.RC_OK)
            {
                xS = (Smena)x;
                xS.m_OldUser = xS.sUser;
                xS.sUser = "";
                xS.sUserPass = "";
                //xS.CurPrinter = -1;
                xS.xtmTOut = null;
                xS.xtmSmEnd = null;
                xS.nMSecondsTOut = 0;
                xS.nDocs = -1;
                xS.xAdrFix1 = null;

                xS.CurPrinterMOBName = xS.CurPrinterSTCName = "";
                //xS.xExpDic = null;
            }
            else
                xS = new Smena();
            
            return (nRet);
        }

        public int SaveCS(string sP, int nD)
        {
            this.nDocs = nD;
            return( Srv.WriteXMLObjTxt(typeof(Smena), this, sP + sXML) );
        }


        public int Uch2Lst(int nU)
        {
            return (Uch2Lst(nU, false));
        }

        public int Uch2Lst(int nU, bool bSet1)
        {
            int nRet = 0;

            if (bSet1)
                aUch = null;

            if (aUch == null)
            {
                aUch = new List<int>(0);
            }

            if (nU > 0)
            {
                if (!aUch.Contains(nU))
                {
                    aUch.Add(nU);
                    aUch.Sort();
                }


                nRet = aUch.Count;
            }
            LstUchKompl = "";
            for (int i = 0; i < aUch.Count; i++)
            {
                if (i > 0)
                    LstUchKompl += ",";
                LstUchKompl += aUch[i].ToString();
            }


            return (nRet);
        }

        // ���������
        public NSI.FILTRDET 
            FilterTTN = NSI.FILTRDET.UNFILTERED,
            FilterZVK = NSI.FILTRDET.UNFILTERED;

    }

    // �������� ���� ���������
    public class DocTypeInf
    {
        private string
            m_Name;

        private int
            m_NumCode;

        private bool
            m_AdrFromNeed = true,
            m_AdrToNeed = false,
            m_TryGetFromServer = true;

        private AppC.MOVTYPE
            m_MoveType = AppC.MOVTYPE.RASHOD;


        public DocTypeInf() { }

        public DocTypeInf(int nC)
        {
            NumCode = nC;
        }

        public DocTypeInf(int nC, string sN, AppC.MOVTYPE nMT)
        {
            NumCode = nC;
            Name = sN;
            MoveType = nMT;
        }

        public DocTypeInf(int nC, string sN, bool A1, bool A2, AppC.MOVTYPE nMT)
        {
            NumCode = nC;
            Name = sN;
            AdrFromNeed = A1;
            AdrToNeed = A2;
            MoveType = nMT;
        }

        // ��� ����
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        // �������� ��� ���� (��� �����)
        public int NumCode
        {
            get { return m_NumCode; }
            set { m_NumCode = value; }
        }

        // ��� �������� �� ������
        public AppC.MOVTYPE MoveType
        {
            get { return m_MoveType; }
            set { m_MoveType = value; }
        }

        // �������������� ������-���������
        public bool AdrFromNeed
        {
            get { return m_AdrFromNeed; }
            set { m_AdrFromNeed = value; }
        }

        // �������������� ������-���������
        public bool AdrToNeed
        {
            get { return m_AdrToNeed; }
            set { m_AdrToNeed = value; }
        }

        // �������� ���������� �� ���������
        public bool TryFrom
        {
            get { return m_TryGetFromServer; }
            set { m_TryGetFromServer = value; }
        }

    }


    public class DocPars
    {

        private int
            m_nTypD = AppC.TYPD_SVOD,
            m_TypOp = -1;

        //public static
        //    Dictionary<int, string> dicTypD = null;

        //public static TextBox tKSkl = null;    // 
        //public static TextBox tNSkl = null;    // 

        public static TextBox tKUch = null;    // 
        public static TextBox tDate = null;    // 
        
        public static TextBox tKTyp = null;    // 
        public static TextBox tNTyp = null;    // 

        public static TextBox tKEks = null;    // 
        public static TextBox tKPol = null;    // 

        public int nSklad;              // ��� ������
        public string sSklad;           // ������������ ������
        public int nUch;                // ��� �������
        public DateTime dDatDoc;        // ���� ���������
        public string sSmena;           // ��� �����
        //public int nTypD;               // ��� ��������� (���)
        public string sTypD;            // ��� ��������� (������������)
        public string sNomDoc;          // � ���������
        public int nEks;                // ��� �����������
        public string sEks;             // ��� �����������
        public int nPol;                // ��� ���������� 
        public string sPol;             // ������������ ���������� 

        public long lSysN;              // � ���������

        public DocPars(int nReg):this(nReg, null){}

        public DocPars(int nReg, Smena xS)
        {
            nSklad = Smena.SklDef;
            nUch = Smena.UchDef;
            dDatDoc = Smena.DateDef;
            sSmena = Smena.SmenaDef;
            sNomDoc = "";
            nEks = AppC.EMPTY_INT;
            sEks = "";
            nPol = AppC.EMPTY_INT;
            if (xS != null)
            {
                nTypD = xS.DocType;
                //nPol = AppC.TYPOP_MOVE;
                //nTypD = AppC.TYPD_OPR;
            }
            else
            {
                nTypD = AppC.TYPD_SVOD;
            }
        }
        // ��� ��������� (���)
        public int nTypD
        {
            get { return m_nTypD; }
            set 
            { 
                m_nTypD = value;
                if (m_nTypD == AppC.TYPD_OPR)
                {
                    TypOper = AppC.TYPOP_MOVE;
                }
                else
                    TypOper = AppC.TYPOP_DOCUM;
            }
        }

        // ��� �������� ���������
        public int TypOper
        {
            get { return m_TypOp; }
            set { m_TypOp = value; }
        }


        public static string TypDName(int nTD)
        {
            string 
                s = "";
            try
            {
                //s = dicTypD[nTD];
                s = AppC.xDocTInf[nTD].Name;
            }
            catch
            {
                s = "";
            }
            return (s);
        }

        public static string OPRName(ref int nOpr)
        {
            string s = "����������";
            switch (nOpr)
            {
                case AppC.TYPOP_PRMK:
                    s = "����� � ������������";
                    break;
                case AppC.TYPOP_MARK:
                    s = "����������";
                    break;
                case AppC.TYPOP_KMPL:
                    s = "������������";
                    break;
                case AppC.TYPOP_OTGR:
                    s = "��������";
                    break;
                case AppC.TYPOP_MOVE:
                    s = "����������� �� ������";
                    break;
                case AppC.TYPOP_KMSN:
                    s = "�����������������";
                    break;
                case AppC.TYPOP_INVENT:
                    s = "��������������";
                    break;
                default:
                    nOpr = AppC.EMPTY_INT;
                    break;
            }
            return (s);
        }
    }


    public class PoddonInfo
    {
        private bool m_Complected;
        private bool m_UpLoaded;
        private string m_SSCC;

        public bool IsComplected
        {
            get { return m_Complected; }
            set { m_Complected = value; }
        }

        public bool IsUpLoaded
        {
            get { return m_UpLoaded; }
            set { m_UpLoaded = value; }
        }

        public string SSCC
        {
            get { return m_SSCC; }
            set { m_SSCC = value; }
        }

    }

    public class PoddonList : SortedList<int, PoddonInfo>
    {
        private int 
            m_CurI = -1;

        public int Current
        {
            get { return ((m_CurI >= 0)? base.Keys[m_CurI]: 0); }
            set
            {
                int i = base.IndexOfKey(value);
                if (i != -1)
                    m_CurI = i;
            }
        }

        // ������� �������� �� ���������
        public int TryNext(bool bSetCur, bool bForward)
        {
            int i = -1;

            if (base.Count > 0)
            {
                if (bForward)
                    i = ((m_CurI == -1) || (m_CurI == (base.Count - 1))) ? 0 : (m_CurI + 1);
                else
                    i = ((m_CurI == -1) || (m_CurI == 0)) ? base.Count - 1 : (m_CurI - 1);
            }
            if (i >= 0)
            {
                if (bSetCur)
                    m_CurI = i;
                i = base.Keys[i];
            }
            else
                i = 0;
            return (i);
        }

        // �������� �������
        public string RangeN()
        {
            string s = "";
            if (base.Count > 0)
            {
                s = base.Keys[0].ToString();
                if (base.Count > 1)
                    s += "-" + base.Keys[base.Count - 1].ToString();
            }
            return (s);
        }


    }


    public class CurOper
    {
        private MainF.AddrInfo
            m_xAdrSrc = null,                           // �����-��������
            m_xAdrDst_Srv = null,                       // ����� � �������
            m_xAdrDst = null;                           // �����-��������

        private DataRow
            m_drObj = null;                             // ������ ��������

        private string
            m_SSCC_Src = "",
            m_SSCC_Dst = "";


        // ������������� ������� �������� ����� ���������
        private bool SetOperState(int nT, bool bShowOper)
        {
            bool
                bRet = false;

            AppC.OPR_STATE
                st;

            if ((nOperState & AppC.OPR_STATE.OPR_OBJ_SET) == AppC.OPR_STATE.OPR_OBJ_SET)
            {
                switch (nT)
                {
                    case AppC.TYPD_PRIH:       // ��������� �����������
                        if ((nOperState & AppC.OPR_STATE.OPR_DST_SET) == AppC.OPR_STATE.OPR_DST_SET)
                            bRet = true;
                        break;
                    case AppC.TYPD_OPR:             // ��������� �����������
                        if (IsFillSrc() && IsFillDst())
                            bRet = true;
                        break;
                    default:
                        // ��� ��������� � �������������� ����� ��������
                        if ((nOperState & AppC.OPR_STATE.OPR_SRC_SET) == AppC.OPR_STATE.OPR_SRC_SET)
                            bRet = true;
                        break;
                }
            }
            else
            {
                bRet = false;
            }

            if (bRet)
            {
                nOperState |= AppC.OPR_STATE.OPR_READY;
                OperObj["STATE"] = nOperState;
                //Srv.PlayMelody(PDA.OS.W32.MB_4HIGH_FLY);
            }
            else
            {
                nOperState &= ~AppC.OPR_STATE.OPR_READY;
                //if (bObjOperScanned)
                if ((nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                {
                    OperObj["STATE"] = nOperState;
                }
            }
            if (bShowOper)
                xMF.ShowOperState(this);
            return (bRet);
        }

        
        public static MainF
            xMF = null;                                 // ��� ����������� �� �����

        //public bool
        //    bObjOperScanned = false;                    // ���� ��������� ������� ��������

        public AppC.OPR_STATE
            nOperState = AppC.OPR_STATE.OPR_EMPTY;      // ������� ������ ��������

        public CurOper(bool RefreshNeed)
        {
            if (RefreshNeed)
                xMF.ShowOperState(this);
        }

        public bool IsFillAll()
        {
            bool 
                bRet = AppC.RC_CANCELB;

            if ((xAdrSrc != null) && (xAdrDst != null))
            {// ������ ����������� ?
                if ((xAdrSrc.Addr != "") && (xAdrDst.Addr != ""))
                {
                    //bRet = bObjOperScanned;
                    bRet = (nOperState & AppC.OPR_STATE.OPR_DST_SET) > 0;
                }
            }
            return (bRet);
        }

        public bool IsFillSrc()
        {
            return (((xAdrSrc != null) && (xAdrSrc.Addr != "")));
        }
        public bool IsFillDst()
        {
            return (((xAdrDst != null) && (xAdrDst.Addr != "")));
        }

        public string GetSrc(bool bAdrName)
        {
            return ( (xAdrSrc != null)? (bAdrName)?xAdrSrc.AddrShow:xAdrSrc.Addr : "");
        }
        public string GetDst(bool bAdrName)
        {
            return ((xAdrDst != null) ? (bAdrName)?xAdrDst.AddrShow:xAdrDst.Addr : "");
        }


        // �����-��������, ����������� � ��������
        public MainF.AddrInfo xAdrSrc
        {
            get { return m_xAdrSrc; }
        }

        // ��������� ������ � �������� ����������
        public void SetOperSrc(MainF.AddrInfo xA, int nT, bool bShowOper)
        {
            m_xAdrSrc = xA;
            //m_DT = xT;
            if (m_xAdrSrc != null)
            {
                nOperState |= AppC.OPR_STATE.OPR_SRC_SET;
                //if ((bObjOperScanned) 
                    
                if (((nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                    && (m_drObj != null))
                {
                    m_drObj["ADRFROM"] = xAdrSrc.Addr;
                    m_drObj["TIMEOV"] = DateTime.Now;
                }
            }
            else
            {
                nOperState &= ~AppC.OPR_STATE.OPR_SRC_SET;
                //if ((bObjOperScanned) 
                if (((nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                    && (m_drObj != null))
                {
                    m_drObj["ADRFROM"] = "";
                    m_drObj["TIMEOV"] = DateTime.Now;
                }
            }
            SetOperState(nT, bShowOper);
        }

        // �����-��������, ����������� � ��������
        public MainF.AddrInfo xAdrDst
        {
            get { return m_xAdrDst; }
        }

        // ��������� ������-��������� � �������� ����������
        public void SetOperDst(MainF.AddrInfo xA, int nT, bool bShowOper)
        {
            m_xAdrDst = xA;
            //m_DT = xT;
            //if ((bObjOperScanned) 
            if (((nOperState & AppC.OPR_STATE.OPR_OBJ_SET) > 0)
                && (m_drObj != null))
            {
                if (m_xAdrDst != null)
                {
                    nOperState |= AppC.OPR_STATE.OPR_DST_SET;
                    m_drObj["ADRTO"] = xA.Addr;
                }
                else
                {
                    nOperState &= ~AppC.OPR_STATE.OPR_DST_SET;
                    m_drObj["ADRTO"] = "";
                }
                m_drObj["TIMEOV"] = DateTime.Now;
            }
            else
            {
                if (m_xAdrDst != null)
                    nOperState |= AppC.OPR_STATE.OPR_DST_SET;
                else
                    nOperState &= ~AppC.OPR_STATE.OPR_DST_SET;
            }
            SetOperState(nT, bShowOper);
        }

        // �����, ������������� �������� (�� ������ �� ������)
        public MainF.AddrInfo xAdrDst_Srv
        {
            get { return m_xAdrDst_Srv; }
            set
            {
                m_xAdrDst_Srv = value;
                if (m_xAdrDst_Srv != null)
                {
                    nOperState |= AppC.OPR_STATE.OPR_SRV_SET;
                }
                else
                {
                    nOperState &= ~AppC.OPR_STATE.OPR_SRV_SET;
                }
            }
        }

        // ������ (���������), ����������� � ��������
        public DataRow OperObj
        {
            get { return m_drObj; }
        }

        // ��������� ������-��������� � �������� ����������
        public void SetOperObj(DataRow xDR, int nT, bool bShowOper)
        {
            m_drObj = xDR;
            //m_DT = xT;
            if (m_drObj != null)
            {
                //bObjOperScanned = true;
                nOperState |= AppC.OPR_STATE.OPR_OBJ_SET;
                m_drObj["TIMEOV"] = DateTime.Now;

                if ((nOperState & AppC.OPR_STATE.OPR_SRC_SET) > 0)
                    m_drObj["ADRFROM"] = xAdrSrc.Addr;

                if ((nOperState & AppC.OPR_STATE.OPR_DST_SET) > 0)
                    m_drObj["ADRTO"] = xAdrDst.Addr;
            }
            else
            {
                //bObjOperScanned = false;
                nOperState &= ~AppC.OPR_STATE.OPR_OBJ_SET;
            }
            SetOperState(nT, bShowOper);
        }

        // SSCC, ����������� � ��������
        public string SSCC
        {
            get { return m_SSCC_Src; }
            set { m_SSCC_Src = value; }
        }

        // SSCC-����������, ����������� � ��������
        public string SSCC_Dst
        {
            get { return m_SSCC_Dst; }
            set { m_SSCC_Dst = value; }
        }



    }


    // ������� ��������
    public class CurDoc
    {
        // ID ������������ ��������� � ������� �����
        private string
            m_ID_Load = "";

        public int 
            nId = AppC.EMPTY_INT,                       // ��� ���������
            //nTypOp = AppC.TYPOP_PRMK,                   // ��� ��������
            nStrokZ,                                    // ����� � ������
            nStrokV,                                    // ����� �������
            nDocSrc;                                    // ������������� ��������� (�������� ��� ������)

        public string 
            sSSCC,                                      // ������� SSCC �������
            sLstUchNoms = "";                           // ������ ������� ��������

        public bool 
            bSpecCond,                                  // ������ ������� ��� ��������� �����
            bEasyEdit = false,
            bTmpEdit = false,
            bConfScan = false,                          // ���� �������� �� ������� ���������������/��������� ������
            bFreeKMPL = false;                          // ���� ��������� ������������

        public DataRow
            drCurSSCC = null,
            drCurRow = null;                            // ������� ������ � ������� ����������

        public DocPars 
            xDocP;                                      // ��������� ��������� (���, �����,...)

        public PoddonList 
            xNPs;                                       //  ������ ������� ��������

        public CurOper 
            xOper;


        public CurDoc(Smena xS) : this(xS, AppC.DT_SHOW) { }

        public CurDoc(Smena xS, int nReg){
            //if (xS.RegApp == AppC.REG_DOC)
            //{
            //    //nTypOp = AppC.TYPOP_DOCUM;
            //    xDocP = new DocPars(nReg);
            //}
            //else
            //{
            //    //nTypOp = AppC.TYPOP_PRMK;
            //    xDocP = new DocPars(nReg, xS);
            //}

            xDocP = new DocPars(nReg, xS);
            InitNew();
        }


        // ID ������������ ��������� � ������� �����
        public string ID_DocLoad
        {
            get { return m_ID_Load; }
            set { m_ID_Load = value; }
        }


        public void InitNew()
        {
        
            sLstUchNoms = "";                 // ������ ������� ��������
            xNPs = new PoddonList();
        
            //FilterTTN = NSI.FILTRDET.UNFILTERED;
            //FilterZVK = NSI.FILTRDET.UNFILTERED;
        
            bEasyEdit = false;
            bTmpEdit = false;
            xOper = new CurOper(false);
        }



        public string DefDetFilter()
        {
            string sF = "";
            try
            {
                sF = String.Format("(SYSN={0})", nId);
            }
            catch { sF = "(TRUE)"; }
            return (sF);
        }
        
    }

    // ������� ��������
    public class CurLoad
    {
        public IntRegsAvail ilLoad; //����� ��������

        public bool
            CheckIt = false;

        // ������� ��������
        public int nCommand = 0;

        // ��������� �������
        public DocPars xLP;


        // ��������� ��������
        public DataSet dsZ;

        // ��������� �������� (������� �� ���������� BD_DOUTD)
        public DataTable dtZ = null;

        public string 
            sComLoad,               // ���������� ������� ��� �������
            sFileFromSrv = "",      // ��� ���������� �����, ����������� � �������
            sSSCC="",               // ��������� ������� ��������
            sFilt;                  // ���������� ��������� �������

        public DataRow 
            dr1st = null,           // ������ � 1-� ����������� ����������
            drPars4Load = null;     // ������ � ����������� ��� ��������

        public MainF.ServerExchange
            xLastSE = null;

        public CurLoad()
            : this(AppC.UPL_CUR) {}

        public CurLoad(bool bIsChk)
            : this(AppC.UPL_CUR, bIsChk) { }

        public CurLoad(int nRegLoad)
            : this(nRegLoad, false) { }

        public CurLoad(int nRegLoad, bool bIsChk)
        {
            xLP = new DocPars(AppC.DT_LOAD_DOC);
            ilLoad = new IntRegsAvail(nRegLoad);
            CheckIt = bIsChk;
        }
    }

    // ��������� �������� �������
    public class IntRegsAvail
    {
        private struct RegAttr
        {
            public int RegValue;
            public string RegName;
            public bool bRegAvail;

            public RegAttr(int RV, string RN, bool RA)
            {
                RegValue = RV;
                RegName = RN;
                bRegAvail = RA;
            }
        }

        private List<RegAttr> lRegs;
        private int nI;

        public IntRegsAvail() : this(AppC.UPL_CUR) { }

        public IntRegsAvail(int nSetCur)
        {
            lRegs = new List<RegAttr>(5);
            lRegs.Add(new RegAttr(AppC.UPL_CUR, "�������", true));
            lRegs.Add(new RegAttr(AppC.UPL_ALL, "���", false));
            lRegs.Add(new RegAttr(AppC.UPL_FLT, "�� �������", false));

            nI = 0;
            CurReg = nSetCur;
        }

        // ����� �� ��������� ��������
        private int FindByVal(int V)
        {
            int ret = -1;
            int nK = 0;
            foreach (RegAttr ra in lRegs)
            {
                if (ra.RegValue == V)
                {
                    ret = nK;
                    break;
                }
                nK++;
            }
            return (ret);
        }

        // ������� �����
        public int CurReg {
            get { return (lRegs[nI].RegValue); }
            set
            {
                int nK = FindByVal(value);
                if (nK >= 0)
                    nI = nK;
            }
        }

        // ������������ �������� ������
        public string CurRegName
        {
            get { return (lRegs[nI].RegName); }
        }

        // ���������� ����������� �������� ������
        public bool CurRegAvail
        {
            get { return (lRegs[nI].bRegAvail); }
            set { 
                RegAttr ra = lRegs[nI];
                ra.bRegAvail = value;
                lRegs[nI] = ra;
            }
        }

        // ���������� ���������/���������� ��������� ������
        public string NextReg(bool bUp)
        {
            int nK;

            if (bUp == true)
            {// ����� ����������
                nK = (nI == lRegs.Count - 1) ? 0: nI + 1;
                while ((nK < lRegs.Count) && (nK != nI))
                {
                    if (lRegs[nK].bRegAvail == true)
                    {
                        nI = nK;
                        break;
                    }
                    nK++;
                    if (nK == lRegs.Count)
                        nK = 0;
                }
            }
            else
            {
                nK = (nI == 0)? lRegs.Count - 1 : nI - 1;
                while ((nK >= 0) && (nK != nI))
                {
                    if (lRegs[nK].bRegAvail == true)
                    {
                        nI = nK;
                        break;
                    }
                    if (nK == 0)
                        nK = lRegs.Count - 1;
                    else
                        nK--;
                }
            }

            return (lRegs[nI].RegName);
        }

        // ���� ����������� ��� ����
        public void SetAllAvail(bool bFlag)
        {
            for (int i = 0; i < lRegs.Count; i++ )
            {
                RegAttr ra = lRegs[i];
                ra.bRegAvail = bFlag;
                lRegs[i] = ra;
            }
        }

        // ���������� ����������� �����������
        public bool SetAvail(int nReg, bool v)
        {
            bool ret = false;
            int nK = FindByVal(nReg);
            if (nK >= 0)
            {
                RegAttr ra = lRegs[nK];
                ra.bRegAvail = v;
                lRegs[nK] = ra;
                ret = true;
            }
            return (ret);
        }


    }




    public class ServerInf
    {
        private string m_SrvComment;
        private string m_SrvHost;
        private int m_SrvPort;

        public ServerInf() { }

        public ServerInf(string sH, int nP)
        {
        }


    }


    // ������ ��������
    public class GroupServers
    {

        private AppPars xPApp;

        // ������ ������� � ������
        public int nSrvGind;

        private BindingList<ServerInf> blSrvG;

        private List<string> lSrvG;

        public List<int> naComms;



        public GroupServers()
            : this(AppC.UPL_CUR, null) { }

        public GroupServers(AppPars xP)
            : this(AppC.UPL_CUR, xP) { }

        public GroupServers(int nRegUpl, AppPars xP)
        {
            xPApp = xP;
            nSrvGind = -1;
            lSrvG = new List<string>();
            lSrvG.Clear();
            if (xP != null)
            {
                if (xP.bUseSrvG)
                {
                    if (xP.aSrvG.Length > 1)
                    {
                        lSrvG.Add("���");
                        nSrvGind = 1;
                    }
                    else
                        nSrvGind = 0;
                    foreach (AppPars.ServerPool xS in xP.aSrvG)
                    {
                        lSrvG.Add(xS.sSrvComment);
                    }

                }
            }
        }


        public string CurSrv
        {
            get { return (nSrvGind >= 0) ? lSrvG[nSrvGind] : xPApp.sHostSrv; }
        }
        public int NextSrv()
        {
            if (nSrvGind >= 0)
            {
                nSrvGind = (lSrvG.Count - 1 == nSrvGind) ? 1 : nSrvGind + 1;
            }
            return (nSrvGind);
        }
    }

    // ������� ��������
    public class CurUpLoad
    {
        //����� ��������
        public IntRegsAvail ilUpLoad;

        // ������ ������� � ������
        public int nSrvGind;

        private List<string> lSrvG;
        private AppPars xParsApp;


        // ��������� �������
        public DocPars xLP;

        public List<int> naComms;

        // ������� ������� ��������
        public string sCurUplCommand = "";

        // �������� ������ ������� ������ (��� ��������)
        public bool bOnlyCurRow = false;

        public DataRow drForUpl = null;

        // �������������� ������ �������� (��������� ������)
        //public byte[] aAddDat = null;

        public CurUpLoad()
            : this(AppC.UPL_CUR, null) {}

        public CurUpLoad(AppPars xP)
            : this(AppC.UPL_CUR, xP) { }

        public CurUpLoad(int nRegUpl, AppPars xP)
        {
            xParsApp = xP;
            xLP = new DocPars(AppC.DT_UPLD_DOC);
            ilUpLoad = new IntRegsAvail(nRegUpl);
            nSrvGind = -1;
            lSrvG = new List<string>();
            lSrvG.Clear();
            if (xP != null)
            {
                if (xP.bUseSrvG)
                {
                    if (xP.aSrvG.Length > 1)
                    {
                        lSrvG.Add("���");
                        nSrvGind = 1;
                    }
                    else
                        nSrvGind = 0;
                    foreach (AppPars.ServerPool xS in xP.aSrvG)
                    {
                        lSrvG.Add(xS.sSrvComment);
                    }

                }
            }

        }

        //public DataRow SetFiltInRow(NSI xNSI)
        public string SetFiltInRow()
        {

            string sF = String.Format("(TD={0}) AND (DT={1}) AND (KSK={2})",
                xLP.nTypD, xLP.dDatDoc.ToString("yyyyMMdd"), xLP.nSklad);

            if (xLP.nUch != AppC.EMPTY_INT)
                sF += "AND(NUCH=" + xLP.nUch.ToString() + ")";

            if (xLP.sSmena != "")
                sF += "AND(KSMEN='" + xLP.sSmena + "')";

            if (xLP.nEks != AppC.EMPTY_INT)
                sF += "AND(KEKS=" + xLP.nEks.ToString() + ")";

            if (xLP.nPol != AppC.EMPTY_INT)
                sF += "AND(KRKPP=" + xLP.nPol.ToString() + ")";
            return ("(" + sF + ")");

/*
            DataRow drFilt = xNSI.DT[NSI.BD_DOCOUT].dt.NewRow();

            drFilt["TD"] = xLP.nTypD;

            drFilt["DT"] = xLP.dDatDoc.ToString("yyyyMMdd");
 
            if (xLP.nPol != AppC.EMPTY_INT)
                drFilt["KRKPP"] = xLP.nPol;

            if (xLP.nSklad != AppC.EMPTY_INT)
                drFilt["KSK"] = xLP.nSklad;

            if (xLP.nUch != AppC.EMPTY_INT)
                drFilt["NUCH"] = xLP.nUch;

            if (xLP.nEks != AppC.EMPTY_INT)
                drFilt["KEKS"] = xLP.nEks;

            drFilt["KSMEN"] = xLP.sSmena;
            drFilt["NOMD"] = xLP.sNomDoc;
            return (drFilt);
 */ 
        }

        public string CurSrv
        {
            get { return (nSrvGind >= 0) ? lSrvG[nSrvGind] : xParsApp.sHostSrv; }
        }
        public int NextSrv()
        {
            if (nSrvGind >= 0)
            {
                nSrvGind = (lSrvG.Count - 1 == nSrvGind) ? 1 : nSrvGind + 1;
            }
            return (nSrvGind);
        }
    }


    public sealed class PSC_Types
    {

        public struct ScDat
        {
            // ���������� ������������
            public ScannerAll.BCId ci;      // ��� �����-����
            public string s;                // �����-���

            // ������� �� �����-����
            //public int nParty;              // ������
            public string 
                nParty;                     // ������

            public string sDataIzg;         // ���� ������������ (���������)
            public DateTime 
                dDataGodn,                  // ���� ��������
                dDataIzg;                   // ���� ������������
            public FRACT fEmk;              // ������� � ������ (��� ��������) ��� 
                                            // ��� �������� (��� ��������); 0 - ��������� �����

            public string
                nTara;                      // ��� ����(C(10))
            public int
                nKolSht,                    // ���������� � ������ (��� ��������)
                nMestPal;                   // ���������� ���� �� �������

            public int
                nKolG,                      // ���������� �����
                nMest;                      // ���������� ����

            public FRACT 
                fVes,                       // ���
                fVsego;                     // ����� ���� /���

            //public int 
            //    nTypVes;             // ��� �������� (TYP_VES_TUP,...)

            public int 
                nNPredMT;                   // � ������������ ��� ���������


            // ����� ����� -???
            //public int nKolSht;             // ���������� (�����)
            //public float nKolVes;           // ���������� (���)
            // ����� ����� -???

            public bool bFindNSI;           // ������� ����� � ���

            //--- ����������� ������
            public FRACT fKolE_alr;         // ��� ������� ������ ������� ���� (���� = 0)
            public int nKolM_alr;           // ��� ������� ���� ������� ����
            public FRACT fMKol_alr;         // ��� ������� ���������� ��������� (���� != 0)
            //--- ����������� ������ (������ ����������)
            public FRACT fKolE_alrT;        // ��� ������� ������ ������� ���� (���� = 0)
            public int
                nMAlr_NPP,                  // ��� ������� ���� ������� ���� (��� ������������)
                nKolM_alrT;                 // ��� ������� ���� ������� ����
            public FRACT
                fVAlr_NPP,                  // ��� ������� ������ ������� ���� (��� ������������)
                fMKol_alrT;        // ��� ������� ���������� ��������� (���� != 0)

            //--- ������ - ����������� ������
            public FRACT fKolE_zvk;         // ��������� ������ ������� ���� �����
            public int nKolM_zvk;           // ���� ������� ���� � ������� �� ������ �����

            // ������ (���)
            public System.Data.DataRow drEd;            // ���� ����������� ������� � ���
            public System.Data.DataRow drMest;          // ���� ����������� ����� � ���

            // ������ �� ������
            public System.Data.DataRow drTotKey;        // ������ �� ����� � ���������� �������
            public System.Data.DataRow drPartKey;       // ������ �� ����� � ����� �������
            public System.Data.DataRow drTotKeyE;       // ������ �� �������� � ���������� �������
            public System.Data.DataRow drPartKeyE;      // ������ �� �������� � ����� �������

            public System.Data.DataRow
                //drSEMK,                                 // ������ � ����������� ��������
                drMC;                                   // ������ � ����������� ������������
            // �� ����������� ������������
            public string sKMC;             // ������ ���
            public int nKrKMC;              // ������� ���
            public string sN;               // ������������
            public int nSrok;               // ���� ���������� (����)

            public bool 
                bEmkByITF,
                bVes;               // ������� ��������

            public string
                sSSCC,
                sSSCCInt,
                sGTIN,
                sEAN,                       // EAN-��� ���������
            
            sGrK;             // ��������� ��� ���������
            public FRACT fEmk_s;            // ��� �������������� ������� ��� ������������� ����=0
            //public int EmkPod;

            // ���������� ������
            public NSI.DESTINPROD nDest;                // ��� �� ������ ����������� (����� ��� ������ �����)
            // ������������� ������
            public int nRecSrc;
            public DateTime dtScan;

            // ��������� �������� �� ������� ����-�������
            public int nDocCtrlResult;

            // ���� ����������� ���� ����������
            public bool bAlienMC;
            public bool bNewAlienPInf;
            public string sIntKod;

            public int nNomPodd;
            public int nNomMesta;
            public AppC.TYP_TARA tTyp;

            // ��������� �� ������ ��� ������������
            public string sErr;

            // ��������� ������� ��� �������� ZVK/TTN
            public string sFilt4View;

            // ������ ����� ������, ������� ������������ ����������� ������� �������������
            public List<DataRow> lstAvailInZVK;
            public int nCurAvail;

            public CurOper xOp;

            public Srv.Collect4Show<StrAndInt>
                xEmks;

            public ScanVarGP 
                xSCD;

            public ScDat(ScannerAll.BarcodeScannerEventArgs e) : this(e, null, new ScanVarGP(e) ) { }

            public ScDat(ScannerAll.BarcodeScannerEventArgs e, ScanVarGP xScan) : this(e, null, xScan) { }

            public ScDat(ScannerAll.BarcodeScannerEventArgs e, CurOper x, ScanVarGP xSc)
            {
                ci = e.nID;                         // ��� �����-����
                s = e.Data;                         // �����-���

                xSCD = xSc;

                nParty = 
                    sDataIzg = "";
                dDataIzg = 
                    dDataGodn = DateTime.MinValue;

                nMest = 
                    nKolG = 
                    nMestPal = 0;

                fEmk = 0;
                fVsego = 0;
                fVes = 0;

                nKolSht = 0;

                //nTypVes = AppC.TYP_VES_UNK;
                bFindNSI = false;

                drEd = null;
                drMest = null;
                drMC = null;
                //drSEMK = null;

                fKolE_alr = 0;
                nKolM_alr = 0;
                fMKol_alr = 0;

                fKolE_alrT = 0;
                nKolM_alrT = 0;
                fMKol_alrT = 0;

                fKolE_zvk = 0;       // ������ ������� ���� ����
                nKolM_zvk = 0;       // ���� ������� ����  �� ������

                drTotKey = null;     // ������ �� ����� � ���������� �������
                drPartKey = null;    // ������ �� ����� � ����� �������
                drTotKeyE = null;    // ������ �� �������� � ���������� �������
                drPartKeyE = null;   // ������ �� �������� � ����� �������

                sKMC = "";
                nKrKMC = AppC.EMPTY_INT;
                sN = "<����������>";
                nSrok = 0;
                nTara = "";

                bEmkByITF = bVes = false;

                sEAN = sSSCCInt = sSSCC = sGTIN = 
                sGrK = "";

                fEmk_s = 0;
                //EmkPod = 0;

                nDest = NSI.DESTINPROD.GENCASE;
                nDocCtrlResult = AppC.RC_CANCEL;

                nRecSrc = (int)NSI.SRCDET.SCAN;
                dtScan = DateTime.Now;
            
                bAlienMC = false;
                bNewAlienPInf = false;
                sIntKod = "";
                sErr = "";
                sFilt4View = "";
                lstAvailInZVK = new List<DataRow>();
                lstAvailInZVK.Clear();
                nCurAvail = -1;
            
                nNomPodd = 0;
                nNomMesta = 0;
                tTyp = AppC.TYP_TARA.UNKNOWN;
                xOp = (x == null)?new CurOper(false):x;
                nNPredMT = 0;
                nMAlr_NPP = 0;                  // ��� ������� ���� ������� ���� (��� ������������)
                fVAlr_NPP = 0;                  // ��� ������� ������ ������� ���� (��� ������������)

                xEmks = new Srv.Collect4Show<StrAndInt>(new StrAndInt[0]);
            }

            // ��������� ��������� ����� ��� ������
            public void ZeroZEvals()
            {
                fKolE_zvk = 0;       // ������ ������� ���� ����
                nKolM_zvk = 0;       // ���� ������� ����  �� ������

                drTotKey = null;     // ������ �� ����� � ���������� �������
                drPartKey = null;    // ������ �� ����� � ����� �������
                drTotKeyE = null;    // ������ �� �������� � ���������� �������
                drPartKeyE = null;   // ������ �� �������� � ����� �������

                sErr = "";
                sFilt4View = "";
                lstAvailInZVK.Clear();
                nCurAvail = -1;
            }

            //public bool IsTara(string sEAN, int nKrKMC)
            //{
            //    bool
            //        ret = false;

            //    if (nKrKMC > 0)
            //    {// ����� �� ��������
            //        if (((nKrKMC >= 1) && (nKrKMC <= 8)) ||
            //              (nKrKMC == 46) ||
            //              (nKrKMC == 43) ||
            //              (nKrKMC == 41))
            //        {
            //            ret = true;
            //        }
            //    }
            //    else
            //    {// ����� �� EAN
            //        switch (sEAN)
            //        {
            //            case "4100000000041":
            //            case "2010050100023":
            //            case "2010050100207":
            //            case "2010050100313":
            //            case "2010050100405":
            //            case "2010050100436":
            //            case "2010050100474":
            //            case "2010050200174":
            //            case "2010050300089":

            //            case "2600000000437":
            //            case "2600000000468":

            //                ret = true;
            //                break;
            //        }
            //    }
            //    return (ret);
            //}

            // �������� ������ �� ����������� �� EAN ��� ����
            //public bool GetFromNSI_Old(string s, DataRow dr, DataTable dtMC)
            //{
            //    bFindNSI = false;

            //    if (dr != null)
            //    {
            //        drMC = dr;
            //        sKMC = dr["KMC"].ToString();
            //        nKrKMC = int.Parse(dr["KRKMC"].ToString());
            //        sN = dr["SNM"].ToString();
            //        nSrok = int.Parse(dr["SRR"].ToString());
            //        string sS = dr["SRP"].ToString();
            //        if (sS.Length > 0)
            //        {
            //            bVes = int.Parse(sS) > 0 ? true : false;
            //        }
            //        else
            //            bVes = false;

            //        sEAN = dr["EAN13"].ToString();
            //        sGrK = dr["GKMC"].ToString();
            //        bFindNSI = true;
            //        if (dDataIzg != DateTime.MinValue)
            //        {
            //            DateTime dReal = dDataIzg.AddHours((double)nSrok);
            //            sDataIzg = dDataIzg.ToString("dd.MM.yy") + "/";
            //            if (AppPars.bUseHours == true)
            //                sDataIzg += dReal.ToString("HH").Substring(0, 2) + "� ";
            //            sDataIzg += dReal.ToString("dd.MM");
            //        }

            //        // ����� ������� �� ���� ��������� � �������� ���������� ����
            //        if (drSEMK == null)
            //        {
            //            DataRow[] childRows = drMC.GetChildRows(dtMC.ChildRelations[NSI.REL2EMK]);
            //            if (childRows.Length == 1)
            //            {// ��������� ������, ������ ���� �������
            //                //fEmk_s = (FRACT)childRows[0]["EMK"];
            //                fEmk = fEmk_s = (FRACT)childRows[0]["EMK"];
            //                nMestPal = (int)childRows[0]["EMKPOD"];
            //                //nTara = (childRows[0]["KT"] is string)?(string)childRows[0]["KT"]:"";
            //                nTara = (childRows[0]["KTARA"] is string) ? (string)childRows[0]["KTARA"] : "";
            //                nKolSht = (childRows[0]["KRK"] is int) ? (int)childRows[0]["KRK"] : 0;
            //            }
            //        }
            //    }
            //    else
            //        sN = s + "-???";

            //    return (bFindNSI);
            //}




            public bool GetFromNSI(string s, DataRow dr, DataTable dtMC)
            {
                return( GetFromNSI(s, dr, dtMC, true) );
            }




            // �������� ������ �� ����������� �� EAN ��� ����
            public bool GetFromNSI(string s, DataRow dr, DataTable dtMC, bool bFullInfo)
            {
                int
                    nFoundGTIN,
                    nDefEmk;
                bFindNSI = false;

                if (dr != null)
                {
                    bFindNSI = true;
                    drMC = dr;
                    sKMC = dr["KMC"].ToString();
                    nKrKMC = int.Parse(dr["KRKMC"].ToString());
                    sN = dr["SNM"].ToString();
                    nSrok = int.Parse(dr["SRR"].ToString());

                    //string sS = dr["SRP"].ToString();
                    //if (sS.Length > 0)
                    //{
                    //    bVes = int.Parse(sS) > 0 ? true : false;
                    //}
                    //else
                    //    bVes = false;
                    try
                    {
                        bVes = (int.Parse(dr["SRP"].ToString()) > 0)?true:false;
                    }
                    catch { bVes = false; }

                    sEAN = dr["EAN13"].ToString();
                    sGrK = dr["GKMC"].ToString();

                    if (bFullInfo)
                    {// ������� ��������� ��� ������� ������ ���������� �� ���������

                        if (dDataIzg != DateTime.MinValue)
                        {
                            DateTime dReal = dDataIzg.AddHours((double)nSrok);
                            sDataIzg = dDataIzg.ToString("dd.MM.yy") + "/";
                            if (AppPars.bUseHours == true)
                                sDataIzg += dReal.ToString("HH").Substring(0, 2) + "� ";
                            sDataIzg += dReal.ToString("dd.MM");
                        }

                        // ����� ������� �� ���� ��������� � �������� ���������� ����
                        DataRow[] draEmk = drMC.GetChildRows(dtMC.ChildRelations[NSI.REL2EMK]);
                        xEmks = new Srv.Collect4Show<StrAndInt>(GetEmk4KMC(dr, draEmk, out nDefEmk, out nFoundGTIN));
                        if (xEmks.Count > 0)
                        {
                            if (xEmks.Count == 1)
                            {// ��������� ������, ������ ���� �������
                                //drSEMK = draEmk[0];
                                xEmks.CurrIndex = 0;
                            }
                            else
                            {
                                if (nFoundGTIN < 0)
                                {
                                    if (nDefEmk >= 0)
                                        nFoundGTIN = nDefEmk;
                                }
                                nFoundGTIN = Math.Max(nFoundGTIN, 0);
                                xEmks.CurrIndex = nFoundGTIN;
                            }
                            StrAndInt xS = (StrAndInt)xEmks.Current;
                            fEmk = fEmk_s = xS.DecDat;
                            if (nMestPal <= 0)
                                nMestPal = xS.IntCode;
                            nTara = xS.SNameAdd1;
                            nKolSht = xS.IntCodeAdd1;
                        }
                    }

                }
                else
                    sN = s + "-???";

                return (bFindNSI);
            }

            //public StrAndInt[] GetEmk4KMC_Old(ref PSC_Types.ScDat sc, DataRow drMC, bool bOrigOnly, out int nDefaultEmk)
            //{
            //    bool
            //        bOrigEmk;
            //    int
            //        jMax = 0;
            //    string
            //        sF = String.Format("(KMC='{0}')", sc.sKMC);
            //    //DataView
            //    //    dv;
            //    DataRow[]
            //        draE = null;
            //    DataRelation
            //        //myRelation = xNSI.DT[NSI.NS_MC].dt.ChildRelations[NSI.REL2EMK];
            //    myRelation = null;

            //    FRACT
            //        fCurEmk;
            //    StrAndInt[]
            //        sa,
            //        siTmp;

            //    sa = new StrAndInt[0];
            //    nDefaultEmk = -1;
            //    if (IsTara("", sc.nKrKMC))
            //        return (sa);
            //    try
            //    {
            //        //draE = xNSI.DT[NSI.NS_SEMK].dt.Select(sF, "EMK", DataViewRowState.CurrentRows);

            //        //if (dvS == null)
            //        //{
            //        //    dvS = new DataView(xNSI.DT[NSI.NS_SEMK].dt);
            //        //    dvS.Sort = "KMC";
            //        //}

            //        //xNSI.DT[NSI.NS_SEMK].SetAddSort("KMC");
            //        //draE = xNSI.DT[NSI.NS_SEMK].dt.Select(sF);

            //        //xNSI.DT[NSI.NS_SEMK].SetAddSort("KMC");
            //        draE = drMC.GetChildRows(myRelation);

            //        if (draE.Length > 0)
            //        {
            //            siTmp = new StrAndInt[draE.Length];
            //            for (int i = 0; i < draE.Length; i++)
            //            {
            //                fCurEmk = (FRACT)draE[i]["EMK"];
            //                bOrigEmk = true;
            //                if (bOrigOnly)
            //                {// ��������� ������ ������������� �������
            //                    for (int j = 0; j < jMax; j++)
            //                    {
            //                        if (siTmp[j].DecDat == fCurEmk)
            //                        {
            //                            bOrigEmk = false;
            //                            break;
            //                        }
            //                    }
            //                }
            //                if (bOrigEmk)
            //                {
            //                    //siTmp[jMax] = new StrAndInt(jMax.ToString(), (int)dv[i].Row["EMKPOD"]);
            //                    siTmp[jMax] = new StrAndInt(jMax.ToString(), (int)draE[i]["EMKPOD"],
            //                        (string)draE[i]["KTARA"], (string)draE[i]["GTIN"],
            //                        (int)draE[i]["KRK"], (int)draE[i]["PR"]);
            //                    siTmp[jMax].DecDat = fCurEmk;

            //                    if ((int)draE[i]["PR"] > 0)
            //                        nDefaultEmk = jMax;
            //                    jMax++;
            //                }
            //            }
            //            sa = new StrAndInt[jMax];
            //            for (int j = 0; j < jMax; j++)
            //                sa[j] = siTmp[j];
            //            if (nDefaultEmk < 0)
            //                nDefaultEmk = 0;
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        jMax = e.Message.Length;
            //    }
            //    return (sa);
            //}


            //// ��������� ������ ��������
            //public StrAndInt[] GetEmk4KMCv0(DataRow drMC, DataRow[] draE, out int nDefaultEmk)
            //{
            //    bool
            //        bOrigOnly = true,
            //        bOrigEmk;
            //    int
            //        jMax = 0;
            //    string
            //        sF = String.Format("(KMC='{0}')", this.sKMC);
                

            //    FRACT
            //        fCurEmk;
            //    StrAndInt[]
            //        sa,
            //        siTmp;

            //    sa = new StrAndInt[0];
            //    nDefaultEmk = -1;
            //    if (IsTara("", this.nKrKMC))
            //        return (sa);
            //    try
            //    {
            //        if (draE.Length > 0)
            //        {
            //            siTmp = new StrAndInt[draE.Length];
            //            for (int i = 0; i < draE.Length; i++)
            //            {
            //                fCurEmk = (FRACT)((this.bVes) ? draE[i]["KRK"] : draE[i]["EMK"]);
            //                bOrigEmk = true;
            //                if (bOrigOnly)
            //                {// ��������� ������ ������������� �������
            //                    for (int j = 0; j < jMax; j++)
            //                    {
            //                        if (siTmp[j].DecDat == fCurEmk)
            //                        {
            //                            bOrigEmk = false;
            //                            break;
            //                        }
            //                    }
            //                }
            //                if (bOrigEmk)
            //                {
            //                    //siTmp[jMax] = new StrAndInt(jMax.ToString(), (int)dv[i].Row["EMKPOD"]);
            //                    siTmp[jMax] = new StrAndInt(jMax.ToString(), 
            //                        (int)draE[i]["EMKPOD"],
            //                        (string)draE[i]["KTARA"], 
            //                        (string)draE[i]["GTIN"],
            //                        (int)draE[i]["KRK"], 
            //                        (int)draE[i]["PR"]);
            //                    siTmp[jMax].DecDat = fCurEmk;

            //                    if ((int)draE[i]["PR"] > 0)
            //                        nDefaultEmk = jMax;
            //                    jMax++;
            //                }
            //            }
            //            sa = new StrAndInt[jMax];
            //            for (int j = 0; j < jMax; j++)
            //                sa[j] = siTmp[j];
            //            if (nDefaultEmk < 0)
            //                nDefaultEmk = 0;
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        jMax = e.Message.Length;
            //    }
            //    return (sa);
            //}


            // ��������� ������ ��������
            public StrAndInt[] GetEmk4KMC(DataRow drMC, DataRow[] draE, out int nDefaultEmk, out int nFGTIN)
            {
                int
                    //nBadElem = 0,
                    j = 0,
                    i;
                StrAndInt[]
                    siRet,
                    siTmp;

                //sa = new StrAndInt[0];
                nDefaultEmk = nFGTIN = -1;

                siTmp = new StrAndInt[draE.Length];

                if (PSC_Types.IsTara("", this.nKrKMC))
                    return (new StrAndInt[0]);
                try
                {
                    if (draE.Length > 0)
                    {
                        for (i = 0, j = 0; i < draE.Length; i++, j++)
                        {
                            //j++;
                            siTmp[j] = new StrAndInt(j.ToString(),
                                draE[i]["EMKPOD"],
                                draE[i]["KTARA"],
                                draE[i]["GTIN"].ToString(),
                                draE[i]["KRK"],
                                draE[i]["PR"]);
                            //siTmp[i].DecDat = (FRACT)draE[i]["EMK"];
                            siTmp[j].DecDat = (draE[i]["EMK"] is FRACT) ? (FRACT)draE[i]["EMK"] : 0;
                            if ((siTmp[j].DecDat <= 0) && (siTmp[j].IntCodeAdd1 <= 0))
                            {
                                j--;
                                continue;
                            }
                            else
                            {
                                if (siTmp[j].IntCodeAdd2 > 0)
                                    nDefaultEmk = j;
                                if (bEmkByITF)
                                {
                                    if (sGTIN == siTmp[j].SNameAdd2)
                                        nFGTIN = j;
                                }
                                else
                                {
                                    if ((siTmp[j].DecDat == this.fEmk) && (siTmp[j].DecDat > 0))
                                    {
                                        nFGTIN = j;
                                    }
                                }
                            }
                        }
                        //if ((nFGTIN < 0) && (nDefaultEmk >= 0))
                        //    nFGTIN = nDefaultEmk;
                        if (j < i)
                        {
                            siRet = new StrAndInt[j];
                            for (i = 0; i < j; i++)
                                siRet[i] = siTmp[i];
                            siTmp = siRet;
                        }
                    }
                }
                catch (Exception e)
                {
                    i = e.Message.Length;
                }
                return (siTmp);
            }



        }

        public static bool IsTara(string sEAN, int nKrKMC)
        {
            bool
                ret = false;

            if (nKrKMC > 0)
            {// ����� �� ��������
                if (((nKrKMC >= 1) && (nKrKMC <= 8)) ||
                      (nKrKMC == 46) ||
                      (nKrKMC == 43) ||
                      (nKrKMC == 41))
                {
                    ret = true;
                }
            }
            else
            {// ����� �� EAN
                switch (sEAN)
                {
                    case "4100000000041":
                    case "2010050100023":
                    case "2010050100207":
                    case "2010050100313":
                    case "2010050100405":
                    case "2010050100436":
                    case "2010050100474":
                    case "2010050200174":
                    case "2010050300089":

                    case "2600000000437":
                    case "2600000000468":

                        ret = true;
                        break;
                }
            }
            return (ret);
        }

        public struct FuncKey
        {
            public int nF;
            public int nKeyValue;
            public Keys kMod;
            public FuncKey(int f, int v, Keys m)
            {
                nF = f;
                nKeyValue = v;
                kMod = m;
            }
        }
    }
}
