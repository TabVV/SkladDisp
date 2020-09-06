using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using PDA.Service;

using FRACT = System.Decimal;
using SkladAll;
//using PDA.Service;

//using System.Globalization;

namespace SkladGP
{
    public sealed partial class NSI : NSIAll
    {
        public const string NS_MC       = "NS_MC";
        public const string NS_PP       = "NS_PP";
        public const string NS_EKS      = "NS_EKS";
        public const string NS_USER     = "NS_USER";
        public const string NS_SMEN     = "NS_SMEN";
        public const string NS_SKLAD    = "NS_SKLAD";
        public const string NS_SUSK     = "NS_SUSK";
        public const string NS_SEMK     = "NS_SEMK";
        public const string NS_PRPR     = "NS_PRPR";
        public const string NS_KRUS     = "NS_KRUS";
        public const string NS_AI       = "NS_AI";
        public const string NS_ADR      = "NS_ADR";
        public const string NS_BLANK    = "NS_BLANK";
        public const string NS_SBLK     = "NS_SBLK";

        public const string NS_KREAN    = "NS_KREAN";
        public const string BD_TINF     = "BD_TINF";
        public const string NS_TYPD     = "NS_TYPD";

        public const string BD_PASPORT  = "BD_PASPORT";

        public const string BD_DOCOUT   = "BD_DOCOUT";
        public const string BD_DOUTD    = "BD_DOUTD";
        public const string BD_DIND     = "BD_DIND";
        public const string BD_SPMC     = "BD_SPMC";
        public const string BD_SOTG     = "BD_SOTG";
        public const string BD_SSCC     = "BD_SSCC";
        public const string BD_PICT     = "BD_PICT";

        // ������� ��� ������
        public const string BD_ZDOC     = "BD_ZTTN";
        public const string BD_ZDET     = "BD_STTN";

        public const string BD_KMPL     = "BD_KMPL";

        // ����� ����� ���������
        public const string REL2TTN     = "DOC2TTN";
        public const string REL2ZVK     = "DOC2ZVK";
        public const string REL2PIC     = "DOC2PIC";

        public const string REL2BRK     = "TTN2BRK";

        public const string REL2EMK     = "KMC2EMK";
        public const string REL2SSCC    = "DOC2SSCC";

        // ���� ���������� ��������� �����
        public new enum TABLESORT : int
        {
            NO = 0,                            // ��� ����������
            KODMC = 1,                            // �� �������� ����
            NAMEMC = 2,                            // �� ������������
            RECSTATE = 3,                            // �� ������� ������
            MAXDET = 4                             // ������������ ��������
        }


        // ������ ������
        public enum READINESS : int
        {
            NO          = 0,
            PART_READY  = 20,                           // �������� ���������
            FULL_READY = 100                            // ��������� ���������
        }

        //// �������������� ������� �� ������� ������
        //public enum SPECCOND : int
        //{
        //    NO              = 0,
        //    DATE_SET        = 20,                           // �� ������ ��������� ���� ���������
        //    DATE_SET_EXT    = 50,                           // ������ ������������ ��������� ���� ���������
        //    PARTY_SET       = 100,                          // ������ ������ � ����� ���������
        //    SSCC_INT        = 200,
        //    SSCC            = 500
        //}

        // �������������� ������� �� ������� ������
        public enum SPECCOND : int
        {
            NO              = 0,
            DATE_V_SET      = 4,                            // �� ������ ��������� ���� ���������
            DATE_G_SET      = 16,                           // �� ������ ��������� ���� ��������
            DATE_SET        = 32,                           // ���-�� �� ���� ��� ������
            DATE_SET_EXACT  = 64,                           // ������ ���������� ����
            PARTY_SET       = 128,                          // ������ ������ � ����� ���������
            SSCC_INT        = 256,
            SSCC            = 512
        }


        // ���������� ����� ��������� �����
        public enum DESTINPROD: int
        {
            UNKNOWN = 0,
            GENCASE = 1,                                // ����� ������
            TOTALZ  = 2,                                // ������ ������������ ������ (EAN-EMK-NP)
            PARTZ   = 3,                                // ��������� ������������ ������
            USER    = 10,                               // ���������� User
        }

        // ������������� ��������� �����
        public enum SRCDET : int
        {
            SCAN            = 1,                        // �������������
            FROMZ           = 2,                        // ����������� �� ������
            HANDS           = 3,                        // ������� �������
            SSCCT           = 4,                        // ��������� ����� SSCC
            FROMADR         = 5,                        // ��������� ����� �����
            FROMADR_BUTTON  = 6,                        // ��������� ����� �����
            CR4CTRL         = 7                         // �������� ��� �������� ���������
        }

        // ���������� ��������� �����
        [Flags]
        public enum FILTRDET
        {
            UNFILTERED = 0,                             // ��� �������
            READYZ,                                     // �� ���������� ������
            NPODD,                                      // �� ������� ��������
            SSCC                                        // �� SSCC ��������
        }

        // ��������� �������� ���������
        public enum DOCCTRL : int
        {
            UNKNOWN = 0,                                // �������� �� ����������
            OK = 1,                                     // ������ ������������ ������
            WARNS = 2,                                  // ���� ��������������
            ERRS = 3                                    // ���� ������
        }


        // ������� ������ ��� ������
        internal const int GDOC_VNT         = 0;        // ��� �����������
        internal const int GDOC_INV         = 1;        // ��� ��������������
        internal const int GDOC_CENTR       = 2;        // ��� ������������

        internal const int GDOC_NEXT        = 999;      // ��������� �� ������

        // ������� ������ ��� ��������� �����
        internal const int GDET_SCAN        = 0;        // ��� �������������
        internal const int GDET_ZVK         = 1;        // ��� ������
        internal const int GDET_ZVK_KMPL    = 2;        // ��� ��������������
        internal const int GDET_ZVK_KMPLV   = 3;        // ��� ������������ � �������

        // ������������� ���������
        internal const int DOCSRC_LOAD = 1;             // ��������
        internal const int DOCSRC_CRTD = 2;             // ������ �������
        internal const int DOCSRC_UPLD = 3;             // ��������
       

        public DataSet dsM;
        public DataSet dsNSI;

        public static MainF xFF;

        public NSI(AppPars xP, MainF xF, string[] aNSINames)
        {
            sPathNSI = xP.sNSIPath;
            sPathBD = xP.sDataPath;
            xFF = xF;

            CreateTables();

            if (aNSINames != null)
                LoadLocNSI(aNSINames, SkladAll.NSIAll.LOAD_EMPTY);

            dsNSI = new DataSet("dsNSI");
            dsNSI.Tables.Add(DT[NS_MC].dt);
            dsNSI.Tables.Add(DT[NS_SEMK].dt);

            try
            {
                dsNSI.Relations.Add(REL2EMK, DT[NS_MC].dt.Columns["KMC"], DT[NS_SEMK].dt.Columns["KMC"]);
            }
            catch { }

            dsM = new DataSet("dsM");
            dsM.Tables.Add(DT[BD_DOCOUT].dt);
            dsM.Tables.Add(DT[BD_DOUTD].dt);
            dsM.Tables.Add(DT[BD_DIND].dt);
            dsM.Tables.Add(DT[BD_SPMC].dt);
            dsM.Tables.Add(DT[BD_PICT].dt);

            DataColumn dcDocHeader = DT[BD_DOCOUT].dt.Columns["SYSN"];
            dsM.Relations.Add(REL2TTN, dcDocHeader, DT[BD_DOUTD].dt.Columns["SYSN"]);
            dsM.Relations.Add(REL2ZVK, dcDocHeader, DT[BD_DIND].dt.Columns["SYSN"]);
            dsM.Relations.Add(REL2PIC, dcDocHeader, DT[BD_PICT].dt.Columns["SYSN"]);

            dsM.Relations.Add(REL2BRK, DT[BD_DOUTD].dt.Columns["ID"], 
                                       DT[BD_SPMC].dt.Columns["ID"]);
        }

        // �������� ��� �� ��������� (���������) NEW!!!
        // nReg - LOAD_EMPTY ��� LOAD_ANY (������� ��-������)
        public void LoadLocNSI(string[] aI, int nR)
        {
            float fLoadAll = 0;

            if (aI.Length == 0)
            {
                aI = new string[DT.Keys.Count];
                DT.Keys.CopyTo(aI, 0);
            }
            foreach(string sTN in aI)
            {
                if (Read1NSI(DT[sTN], nR))
                {
                    fLoadAll += float.Parse(DT[sTN].sDTStat);
                    AfterLoadNSI(DT[sTN].dt.TableName, false, "");
                }
            }
        }




        // �������� ������
        private void CreateTables()
        {
            DT = new Dictionary<string, TableDef>();

            // ���������� � ������������
            DT.Add(BD_TINF, new TableDef(BD_TINF, new DataColumn[]{
                new DataColumn("DT_NAME", typeof(string)),              // ��� �������
                new DataColumn("LASTLOAD", typeof(DateTime)),           // ���� ��������� ������� ��������
                new DataColumn("LOAD_HOST", typeof(string)),            // Host (IP) ������� ��������
                new DataColumn("LOAD_PORT", typeof(int)),               // ���� ������� ��������
                new DataColumn("FLAG_LOAD", typeof(string)),            // ����� �������� � �������
                new DataColumn("MD5", typeof(string)) }));              // ����������� ����� MD5
            DT[BD_TINF].dt.PrimaryKey = new DataColumn[] { DT[BD_TINF].dt.Columns["DT_NAME"] };
            DT[BD_TINF].nType = TBLTYPE.NSI | TBLTYPE.INTERN;           // ������ ���
            DT[BD_TINF].dt.Columns["LOAD_HOST"].DefaultValue = "";
            DT[BD_TINF].dt.Columns["LOAD_PORT"].DefaultValue = 0;
            DT[BD_TINF].dt.Columns["FLAG_LOAD"].DefaultValue = "";
            DT[BD_TINF].dt.Columns["MD5"].DefaultValue = "";

            // ����� EAH-KMC
            DT.Add(NS_KREAN, new TableDef(NS_KREAN, new DataColumn[]{
                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("KMC", typeof(string)),                  // ��� (C(10))
                new DataColumn("KRKMC", typeof(int)) }));               // ������� ��� (N(4))
            DT[NS_KREAN].dt.PrimaryKey = new DataColumn[] { DT[NS_KREAN].dt.Columns["KRKMC"] };
            DT[NS_KREAN].nType = TBLTYPE.CREATE | TBLTYPE.NSI;          // ������ ���

            // ���� ����������
            DT.Add(NS_TYPD, new TableDef(NS_TYPD, new DataColumn[]{
                new DataColumn("KOD", typeof(int)),                     // ��� ���������EAN13 (C(13))
                new DataColumn("NAME", typeof(string)) }));             // ������������ ����
            DT[NS_TYPD].dt.PrimaryKey = new DataColumn[] { DT[NS_TYPD].dt.Columns["KOD"] };
            DT[NS_TYPD].nType = TBLTYPE.INTERN | TBLTYPE.NSI;

            // ������� ������
            DT.Add(BD_PASPORT, new TableDef(BD_PASPORT, new DataColumn[]{
                new DataColumn("KD", typeof(string)),                   // ��� ������
                new DataColumn("TD", typeof(string)),                   // ��� ������
                new DataColumn("NAME", typeof(string)),                 // ������������
                new DataColumn("SD", typeof(string)),                   // ��������
                new DataColumn("MD", typeof(string)) }));               // ��������
            DT[BD_PASPORT].dt.PrimaryKey = new DataColumn[] { DT[BD_PASPORT].dt.Columns["KD"] };
            //DT[BD_PASPORT].nType = TBLTYPE.BD | TBLTYPE.PASPORT | TBLTYPE.LOAD;
            DT[BD_PASPORT].nType = TBLTYPE.PASPORT | TBLTYPE.NSI | TBLTYPE.LOAD;
            DT[BD_PASPORT].Text = "�������";

            // ���������� �������������
            DT.Add(NS_USER, new TableDef(NS_USER, new DataColumn[]{
                new DataColumn("KP", typeof(string)),                   // ��� ������������
                new DataColumn("NMP", typeof(string)),                  // ��� ������������
                new DataColumn("PP", typeof(string)),                   // ������
                new DataColumn("TABN", typeof(string)) }));             // ��������� �����
            DT[NS_USER].dt.PrimaryKey = new DataColumn[] { DT[NS_USER].dt.Columns["KP"] };
            DT[NS_USER].Text = "������������";

            // �����������
            DT.Add(NS_MC, new TableDef(NS_MC, new DataColumn[]{
                new DataColumn("KMC", typeof(string)),                  // ��� (C(10))
                new DataColumn("SNM", typeof(string)),                  // ����������� (C(30))
                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("SRR", typeof(int)),                     // ���� ���������� (����) (N(6))
                new DataColumn("SRP", typeof(int)),                     // ������� �������� (1-�������) (N(3))
                new DataColumn("KRKMC", typeof(int)),                   // ������� ��� (N(4))
                new DataColumn("WRAPP", typeof(string)),                // ��� ��������� ������������
                new DataColumn("GRADUS", typeof(string)),               // ������������� ����� ��� ���������
                new DataColumn("GKMC", typeof(string))  }));            // ��������� ��� (C(10))
            //DT[NS_MC].dt.PrimaryKey = new DataColumn[] { DT[NS_MC].dt.Columns["EAN13"] };
            DT[NS_MC].dt.PrimaryKey = new DataColumn[] { DT[NS_MC].dt.Columns["KMC"] };
            DT[NS_MC].dt.Columns["SRP"].AllowDBNull = false;
            DT[NS_MC].Text = "���. ��������";

            // ���������� ��������
            DT.Add(NS_SEMK, new TableDef(NS_SEMK, new DataColumn[]{
                    new DataColumn("KMC", typeof(string)),              // ��� ��������(C(10))
                    new DataColumn("KT", typeof(string)),               // ��� ����(C(10))
                    new DataColumn("KTARA", typeof(string)),            // ��� ����(C(10))
                    new DataColumn("EMK", typeof(FRACT)),               // �������/���   (N(?))
                    new DataColumn("EMKPOD", typeof(int)),              // ������� ������� � ������ ������
                    new DataColumn("KRK", typeof(int)),                 // ���������� ���� (N(5))
                    new DataColumn("GTIN", typeof(string)),             // GTIN (C(14))
                    new DataColumn("WRAPP", typeof(string)),            // ��� ��������� ������������
                    new DataColumn("PR", typeof(int)) }));              // ��������� (N(4))
            DT[NS_SEMK].Text = "�������";
            DT[NS_SEMK].dt.Columns["EMK"].DefaultValue = 0;
            DT[NS_SEMK].dt.Columns["EMKPOD"].DefaultValue = 0;
            //DT[NS_SEMK].dt.PrimaryKey = new DataColumn[] { 
                //DT[NS_SEMK].dt.Columns["GTIN"] 
                //DT[NS_SEMK].dt.Columns["KMC"],
            //};

            // ����������� / ����������
            DT.Add(NS_PP, new TableDef(NS_PP, new DataColumn[]{
                new DataColumn("KPL", typeof(string)),                  // ��� ����������� (C(8))
                new DataColumn("KPP", typeof(string)),                  // ������ ��� ����������
                new DataColumn("KRKPP", typeof(int)),                   // ��� (N(4))
                new DataColumn("NAME", typeof(string)) }));             // ������������ (C(50))
            DT[NS_PP].dt.PrimaryKey = new DataColumn[] { DT[NS_PP].dt.Columns["KRKPP"] };
            DT[NS_PP].Text = "����������-�����������";

            // �����������
            DT.Add(NS_EKS, new TableDef(NS_EKS, new DataColumn[]{
                new DataColumn("KEKS", typeof(int)),                    // ��� ����������� (N(5))
                new DataColumn("FIO", typeof(string)) }));              // ��� ����������� (C(50))
            DT[NS_EKS].dt.PrimaryKey = new DataColumn[] { DT[NS_EKS].dt.Columns["KEKS"] };
            DT[NS_EKS].Text = "�����������";

            // ������
            DT.Add(NS_SKLAD, new TableDef(NS_SKLAD, new DataColumn[]{
                new DataColumn("KSK", typeof(int)),                     // ��� ������
                new DataColumn("NAME", typeof(string)) }));             // ������������ ������
            DT[NS_SKLAD].dt.PrimaryKey = new DataColumn[] { DT[NS_SKLAD].dt.Columns["KSK"] };
            DT[NS_SKLAD].Text = "������";

            // ������� �������
            DT.Add(NS_SUSK, new TableDef(NS_SUSK, new DataColumn[]{
                new DataColumn("KSK", typeof(int)),                     // ��� ������
                new DataColumn("NUCH", typeof(int)),                    // ��� �������
                new DataColumn("NAME", typeof(string)) }));             // ������������ �������
            DT[NS_SUSK].dt.PrimaryKey = new DataColumn[] { DT[NS_SUSK].dt.Columns["KSK"], DT[NS_SUSK].dt.Columns["NUCH"] };
            DT[NS_SUSK].Text = "������� �������";

            // ���������� ����
            DT.Add(NS_SMEN, new TableDef(NS_SMEN, new DataColumn[]{
                new DataColumn("KSMEN", typeof(string)),                // ��� �����
                new DataColumn("NAME", typeof(string)) }));             // ������������ �����
            DT[NS_SMEN].dt.PrimaryKey = new DataColumn[] { DT[NS_SMEN].dt.Columns["KSMEN"] };
            DT[NS_SMEN].Text = "�����";

            // ���������� ������ �����
            DT.Add(NS_PRPR, new TableDef(NS_PRPR, new DataColumn[]{
                new DataColumn("KPR", typeof(string)),                  // ��� ������� ������
                new DataColumn("KRK", typeof(int)),                     // ��� ������� �������
                //new DataColumn("NAME", typeof(string)),                 // ������������ �������
                new DataColumn("SNM", typeof(string))}));               // ������� ������������ �������
            DT[NS_PRPR].dt.PrimaryKey = new DataColumn[] { DT[NS_PRPR].dt.Columns["KPR"] };
            DT[NS_PRPR].Text = "������� �����";

            // ���������� ���� �����������
            DT.Add(NS_KRUS, new TableDef(NS_KRUS, new DataColumn[]{
                new DataColumn("KMC", typeof(string)),                  // ��� (C(10))
                new DataColumn("EAN13", typeof(string)),                // ��� (C(10))
                new DataColumn("KINT", typeof(string))  }));            // ���������� ���
            DT[NS_KRUS].dt.PrimaryKey = new DataColumn[] { DT[NS_KRUS].dt.Columns["KINT"] };
            DT[NS_KRUS].Text = "���� �����������";

            // �������������� ����������
            DT.Add(NS_AI, new TableDef(NS_AI, new DataColumn[]{
                new DataColumn("KAI", typeof(string)),              // ��� ��������������
                new DataColumn("NAME", typeof(string)),             // ������������
                new DataColumn("TYPE", typeof(string)),             // ��� ������
                new DataColumn("MAXL", typeof(int)),                // ����� ������
                new DataColumn("VARLEN", typeof(int)),              // ������� ���������� �����
                new DataColumn("DECP", typeof(int)),                // ������� ���������� �����
                new DataColumn("PROP", typeof(string)),             // ����
                new DataColumn("KED", typeof(string)) }));          // ��� �������
            DT[NS_AI].dt.PrimaryKey = new DataColumn[] { DT[NS_AI].dt.Columns["KAI"] };
            DT[NS_AI].nType = TBLTYPE.INTERN | TBLTYPE.NSI;
            DT[NS_AI].nState = DT_STATE_INIT;
            DT[NS_AI].Text = "�������������� ����������";

            // �������� ������� ��� � �����
            DT.Add(NS_ADR, new TableDef(NS_ADR, new DataColumn[]{
                new DataColumn("KADR", typeof(string)),             // ����� ������-����
                new DataColumn("NAME", typeof(string)),             // ������������
                new DataColumn("TYPE", typeof(int)) }));            // ���
            DT[NS_ADR].dt.PrimaryKey = new DataColumn[] { DT[NS_ADR].dt.Columns["KADR"] };
            //DT[NS_ADR].nType = TBLTYPE.INTERN | TBLTYPE.NSI;
            //DT[NS_ADR].nState = DT_STATE_INIT;
            DT[NS_ADR].Text = "������";
            // ������� ��� ����������� ������ NameAdr(nSklad, sAdr);


            // ��������� ����������
            DT.Add(BD_DOCOUT, new TableDef(BD_DOCOUT, new DataColumn[]{
                new DataColumn("TD", typeof(int)),                      // ��� ��������� (N(2))
                new DataColumn("KRKPP", typeof(int)),                   // ��� ���������� (N(4))
                new DataColumn("KSMEN", typeof(string)),                // ��� ����� (C(3))
                new DataColumn("DT", typeof(string)),                   // ���� (C(8))
                new DataColumn("KSK", typeof(int)),                     // ��� ������ (N(3))
                new DataColumn("NUCH", typeof(int)),                    // ����� ������� (N(3))
                new DataColumn("KEKS", typeof(int)),                    // ��� ����������� (N(5))
                new DataColumn("NOMD", typeof(string)),                 // ����� ��������� (C(10))
                new DataColumn("SYSN", typeof(int)),                    // ID ��� (N(9))
                new DataColumn("SOURCE", typeof(int)),                  // ������������� N(2))
                new DataColumn("DIFF", typeof(int)),                    // ���������� �� ������
                new DataColumn("EXPR_DT", typeof(string)),              // ��������� ��� ����
                new DataColumn("EXPR_SRC", typeof(string)),             // ��������� ��� �������������

                new DataColumn("CHKSSCC", typeof(int)),                 // ��� �������� SSCC

                new DataColumn("PP_NAME", typeof(string)),              // ������������ ����������
                new DataColumn("EKS_NAME", typeof(string)),             // ������������ �����������
                new DataColumn("MEST", typeof(int)),                    // ���������� ����(N(3))
                new DataColumn("MESTZ", typeof(int)),                   // ���������� ���� �� ������(N(3))
                new DataColumn("KOLE", typeof(FRACT)),                  // ���������� ������ (N(10,3))
                new DataColumn("TYPOP", typeof(int)),                   // ��� �������� (�������, ��������, ...)

                new DataColumn("LSTUCH", typeof(string)),               // ������ ��������
                new DataColumn("LSTNPD", typeof(string)),               // ������ ������� ��������
                new DataColumn("CONFSCAN", typeof(int)),                // ����� ������������� ������������(�����)

                new DataColumn("SSCCONLY", typeof(int)),                // 1 - ����� ����� - ������ SSCC
                new DataColumn("PICTURE", typeof(string)),              // ���� �������
                
                new DataColumn("TIMECR", typeof(DateTime)),             // ����-����� ��������
                new DataColumn("ID_LOAD", typeof(string))  }));         // ��� ������������ ��������� (C(10))

            DT[BD_DOCOUT].dt.Columns["EXPR_DT"].Expression = "substring(DT,7,2) + '.' + substring(DT,5,2)";
            DT[BD_DOCOUT].dt.Columns["EXPR_SRC"].Expression = "iif(SOURCE=1,'����', iif(SOURCE=2,'����','����'))";

            DT[BD_DOCOUT].dt.Columns["DIFF"].DefaultValue = NSI.DOCCTRL.UNKNOWN;
            DT[BD_DOCOUT].dt.Columns["MEST"].DefaultValue = 0;
            DT[BD_DOCOUT].dt.Columns["MESTZ"].DefaultValue = 0;
            DT[BD_DOCOUT].dt.Columns["TIMECR"].DefaultValue = DateTime.Now; ;
            DT[BD_DOCOUT].dt.Columns["TYPOP"].DefaultValue = AppC.TYPOP_PRMK;
            DT[BD_DOCOUT].dt.Columns["CONFSCAN"].DefaultValue = 0;
            DT[BD_DOCOUT].dt.Columns["EKS_NAME"].DefaultValue = "";

            DT[BD_DOCOUT].dt.PrimaryKey = new DataColumn[] { DT[BD_DOCOUT].dt.Columns["SYSN"] };
            DT[BD_DOCOUT].dt.Columns["SYSN"].AutoIncrement = true;
            DT[BD_DOCOUT].dt.Columns["SYSN"].AutoIncrementSeed = -1;
            DT[BD_DOCOUT].dt.Columns["SYSN"].AutoIncrementStep = -1;
            DT[BD_DOCOUT].nType = TBLTYPE.BD;

            // ��������� ������ (���������)
            DT.Add(BD_DOUTD, new TableDef(BD_DOUTD, new DataColumn[]{
                new DataColumn("KRKMC", typeof(int)),                   // ������� ��� (N(4))
                new DataColumn("SNM", typeof(string)),                  // ����������� (C(30))
                new DataColumn("KOLM", typeof(int)),                    // ���������� ���� (N(4))
                new DataColumn("KOLE", typeof(FRACT)),                  // ����� (������ ��� ���) (N(10,3))
                new DataColumn("EMK", typeof(FRACT)),                   // �������   (N(?))

                new DataColumn("NP", typeof(string)),                   // � ������ (N(4))

                new DataColumn("DVR", typeof(string)),                  // ���� ��������� (D(8))
                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("SYSN", typeof(int)),                    // ���� ��������� (N(9))
                new DataColumn("SRP", typeof(int)),                     // ������� �������� (1-�������) (N(3))
                new DataColumn("GKMC", typeof(string)),                 // ��������� ��� (C(10))
                new DataColumn("KRKT", typeof(string)),                 // ��� ����(C(10))
                new DataColumn("KTARA", typeof(string)),                // ��� ����(C(10))

                new DataColumn("VES", typeof(FRACT)),                   // ����� (������ ��� ���) (N(10,3))
                new DataColumn("KOLG", typeof(int)),                    // ����� ������ ��������������� ���� (N(10,3))
                
                new DataColumn("KOLSH", typeof(int)),                   // �� ����������� ��������-����/�������� (N(2))
                new DataColumn("DEST", typeof(int)),                    // ���������� ������
                new DataColumn("ID", typeof(int)),                      // ID ������

                new DataColumn("NPODDZ", typeof(int)),                  // � ������� �� ������
                new DataColumn("ADRFROM", typeof(string)),              // ����� �����������
                new DataColumn("ADRTO", typeof(string)),                // ����� ���������
                new DataColumn("NPODD", typeof(int)),                   // � ������� ������ ������
                new DataColumn("NMESTA", typeof(int)),                  // � �����
                new DataColumn("SSCC", typeof(string)),                 // ID �������
                new DataColumn("SSCCINT", typeof(string)),              // ���������� SSCC �������

                new DataColumn("SYSPRD", typeof(int)),                  // SYSN ������������

                new DataColumn("USER", typeof(string)),                 // ��� ������������
                
                new DataColumn("SRC", typeof(int)),                     // ������������� ������
                new DataColumn("TIMECR", typeof(DateTime)),             // ����-����� ��������
                new DataColumn("TIMEOV", typeof(DateTime)),             // ����-����� ��������
                new DataColumn("STATE", typeof(int)),                   // ��������� ������
                
                new DataColumn("NPP_ZVK", typeof(int)),                 // ID ������-������
                new DataColumn("KMC", typeof(string)) }));              // ��� (C(10))
            DT[BD_DOUTD].dt.PrimaryKey = new DataColumn[] { DT[BD_DOUTD].dt.Columns["SYSN"], 
                DT[BD_DOUTD].dt.Columns["KRKMC"], 
                DT[BD_DOUTD].dt.Columns["EMK"], 
                DT[BD_DOUTD].dt.Columns["NP"],
                DT[BD_DOUTD].dt.Columns["ID"] };

            DT[BD_DOUTD].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_DOUTD].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_DOUTD].dt.Columns["ID"].AutoIncrementStep = -1;

            DT[BD_DOUTD].dt.Columns["NP"].DefaultValue = "";
            DT[BD_DOUTD].dt.Columns["NP"].AllowDBNull = false;

            DT[BD_DOUTD].dt.Columns["DEST"].DefaultValue = DESTINPROD.USER;
            DT[BD_DOUTD].dt.Columns["SRC"].DefaultValue = SRCDET.HANDS;
            DT[BD_DOUTD].dt.Columns["TIMECR"].DefaultValue = DateTime.Now;
            DT[BD_DOUTD].dt.Columns["VES"].DefaultValue = 0;

            DT[BD_DOUTD].dt.Columns["ADRFROM"].DefaultValue = "";
            DT[BD_DOUTD].dt.Columns["ADRTO"].DefaultValue = "";
            DT[BD_DOUTD].dt.Columns["NPODD"].DefaultValue = 0;
            DT[BD_DOUTD].dt.Columns["NMESTA"].DefaultValue = 0;
            DT[BD_DOUTD].dt.Columns["NPODDZ"].DefaultValue = 0;
            DT[BD_DOUTD].dt.Columns["STATE"].DefaultValue = AppC.OPR_STATE.OPR_EMPTY;
            DT[BD_DOUTD].nType = TBLTYPE.BD;

            // ��������� ������ ������
            DT.Add(BD_DIND, new TableDef(BD_DIND, new DataColumn[]{
                new DataColumn("KRKMC", typeof(int)),                   // ������� ��� (N(4))
                new DataColumn("SNM", typeof(string)),                  // ����������� (C(30))
                new DataColumn("KOLM", typeof(int)),                    // ���������� ���� (N(4))
                new DataColumn("KOLE", typeof(FRACT)),                  // ����� (������ ��� ���) (N(10,3))
                new DataColumn("EMK", typeof(FRACT)),                   // �������   (N(?))

                //new DataColumn("NP", typeof(int)),                      // � ������ (N(4))
                new DataColumn("NP", typeof(string)),                   // � ������ (N(4))

                new DataColumn("DVR", typeof(string)),                  // ���� ��������� (D(8))
                new DataColumn("DTG", typeof(string)),                  // ���� �������� (D(8))

                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("SYSN", typeof(int)),                    // ���� ��������� (N(9))
                new DataColumn("SRP", typeof(int)),                     // ������� �������� (1-�������) (N(3))
                new DataColumn("GKMC", typeof(string)),                 // ��������� ��� (C(10))
                //new DataColumn("KRKT", typeof(string)),                 // ��� ����(C(10))
                new DataColumn("KTARA", typeof(string)),                // ��� ����(C(10))

                new DataColumn("COND", typeof(int)),                    // ������� �� ������
                new DataColumn("READYZ", typeof(int)),                  // ���������� ������ �� ���������

                new DataColumn("NPODDZ", typeof(int)),                  // � �������
                new DataColumn("NPP", typeof(int)),                     // � ������� �/� ��� ������� �������

                new DataColumn("ADRFROM", typeof(string)),              // ����� �����������
                new DataColumn("ADRTO", typeof(string)),                // ����� ���������

                new DataColumn("SSCC", typeof(string)),                 // ID �������
                new DataColumn("SSCCINT", typeof(string)),              // ���������� SSCC �������

                new DataColumn("KRKPP", typeof(string)),                // ��� ���������� (N(5))
                new DataColumn("ID", typeof(int)),                      // ID ������

                new DataColumn("KMC", typeof(string)) }));              // ��� (C(10))

            DT[BD_DIND].dt.Columns["NP"].DefaultValue = "";
            DT[BD_DIND].dt.Columns["NP"].AllowDBNull = false;
            DT[BD_DIND].dt.Columns["COND"].DefaultValue = SPECCOND.NO;
            DT[BD_DIND].dt.Columns["READYZ"].DefaultValue = READINESS.NO;
            DT[BD_DIND].dt.Columns["SSCC"].DefaultValue = "";
            DT[BD_DIND].dt.Columns["SSCCINT"].DefaultValue = "";

            DT[BD_DIND].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_DIND].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_DIND].dt.Columns["ID"].AutoIncrementStep = -1;

            DT[BD_DIND].nType = TBLTYPE.BD | TBLTYPE.LOAD;

            // ������ ����� � ���������
            DT.Add(BD_SPMC, new TableDef(BD_SPMC, new DataColumn[]{
                new DataColumn("SYSN", typeof(int)),                    // ���� ��������� (N(9))
                new DataColumn("ID", typeof(int)),                      // ID ������ ���������
                new DataColumn("IDB", typeof(int)),                     // ID ������ �����
                new DataColumn("SNM", typeof(string)),                  // ������������ �������
                new DataColumn("KOLM", typeof(int)),                    // ���������� ���� (N(4))
                new DataColumn("KOLE", typeof(FRACT)),                  // ����� (������ ��� ���) (N(10,3))
                new DataColumn("KRK", typeof(int)),                     // ��� ������� �������
                new DataColumn("KPR", typeof(string)),                  // ��� ������� ������
                new DataColumn("TIMECR", typeof(DateTime))}));          // ����-����� ��������
            //DT[BD_SPMC].dt.PrimaryKey = new DataColumn[] { DT[BD_SPMC].dt.Columns["SYSN"], 
            //    DT[BD_SPMC].dt.Columns["ID"], 
            //    DT[BD_SPMC].dt.Columns["IDB"]};
            DT[BD_SPMC].dt.PrimaryKey = new DataColumn[] { DT[BD_SPMC].dt.Columns["ID"], 
                DT[BD_SPMC].dt.Columns["IDB"]};

            DT[BD_SPMC].dt.Columns["IDB"].AutoIncrement = true;
            DT[BD_SPMC].dt.Columns["IDB"].AutoIncrementSeed = -1;
            DT[BD_SPMC].dt.Columns["IDB"].AutoIncrementStep = -1;

            DT[BD_SPMC].dt.Columns["KOLM"].DefaultValue = 0;
            DT[BD_SPMC].dt.Columns["KOLE"].DefaultValue = 0;
            DT[BD_SPMC].dt.Columns["SNM"].DefaultValue = "";

            DT[BD_SPMC].nType = TBLTYPE.BD;
            DT[BD_SPMC].Text = "������ �����";

            // ������ SSCC ��� ���������
            DT.Add(BD_SSCC, new TableDef(BD_SSCC, new DataColumn[]{
                new DataColumn("SYSN",      typeof(int)),               // ���� ��������� (N(9))
                new DataColumn("NPODDZ",    typeof(int)),               // � �������
                new DataColumn("SSCC",      typeof(string)),            // SSCC �������
                new DataColumn("KOLM",      typeof(int)),               // ����
                new DataColumn("KOLE",      typeof(FRACT)),             // ������

                new DataColumn("MONO",      typeof(int)),               // ���� �����������

                new DataColumn("IN_ZVK",    typeof(int)),               // 1 - �������� � ������� (��� ������)
                new DataColumn("IN_TTN",    typeof(int)),               // 1 - �����������������������
                new DataColumn("STATE",     typeof(int)),               // ���������
                new DataColumn("ID",        typeof(int)) }));           // ID ������
            DT[BD_SSCC].nType = TBLTYPE.BD;
            DT[BD_SSCC].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_SSCC].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_SSCC].dt.Columns["ID"].AutoIncrementStep = -1;
            DT[BD_SSCC].dt.Columns["IN_ZVK"].DefaultValue = 0;
            DT[BD_SSCC].dt.Columns["IN_TTN"].DefaultValue = 0;

            // ������ ���� �������� ��� ���������
            DT.Add(BD_PICT, new TableDef(BD_PICT, new DataColumn[]{
                new DataColumn("SYSN",      typeof(int)),               // ���� ��������� (N(9))
                new DataColumn("NPODDZ",    typeof(int)),               // � �������
                new DataColumn("NPP",       typeof(int)),               // � ����
                new DataColumn("PICTURE",   typeof(string)),            // ���� �������
                new DataColumn("ID",        typeof(int)) }));           // ID ������
            DT[BD_PICT].nType = TBLTYPE.BD;
            DT[BD_PICT].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_PICT].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_PICT].dt.Columns["ID"].AutoIncrementStep = -1;

            // ������ ���� ��� ������
            DT.Add(BD_SOTG, new TableDef(BD_SOTG, new DataColumn[]{
                new DataColumn("SYSN", typeof(int)),                    // ���� ��������� (N(9))
                new DataColumn("NPP", typeof(int)),                     // ���� ��������� (N(9))
                new DataColumn("ID", typeof(int)),                      // ID ������
                new DataColumn("KSMEN", typeof(string)),                // ��� ����� (C(3))
                new DataColumn("DTP", typeof(string)),                // ����/����� ��������
                new DataColumn("DTU", typeof(string)),                // ����/����� ������
                new DataColumn("NSH", typeof(int)),                     // � �����
                new DataColumn("KEKS", typeof(int)),                    // ��� ����������� (N(5))
                new DataColumn("KAVT", typeof(string)),                 // � ����
                new DataColumn("NPL", typeof(int)),                     // � ��������
                new DataColumn("ND", typeof(int)),                      // � ���������
                new DataColumn("ROUTE", typeof(string)),                // �������� ��������
                new DataColumn("STATE", typeof(int))}));                // ���������

            DT[BD_SOTG].dt.PrimaryKey = new DataColumn[] { DT[BD_SOTG].dt.Columns["ID"] };

            DT[BD_SOTG].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_SOTG].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_SOTG].dt.Columns["ID"].AutoIncrementStep = -1;

            DT[BD_SOTG].dt.Columns["STATE"].DefaultValue = 0;
            DT[BD_SOTG].nType = TBLTYPE.BD;
            DT[BD_SOTG].Text = "������ ����";

            // ��������� ������� �� ������������
            DT.Add(BD_KMPL, new TableDef(BD_KMPL, new DataColumn[]{
                new DataColumn("TD", typeof(int)),                      // ��� ��������� (N(2))
                new DataColumn("KRKPP", typeof(int)),                   // ��� ���������� (N(4))
                new DataColumn("KSMEN", typeof(string)),                // ��� ����� (C(3))
                new DataColumn("DT", typeof(string)),                   // ���� (C(8))
                new DataColumn("KSK", typeof(int)),                     // ��� ������ (N(3))
                new DataColumn("NUCH", typeof(string)),                 // ������ ��������
                new DataColumn("KEKS", typeof(int)),                    // ��� ����������� (N(5))
                new DataColumn("NOMD", typeof(string)),                 // ����� ��������� (C(10))
                new DataColumn("SYSN", typeof(long)),                    // ID ��� (N(9))
                new DataColumn("KOLPODD", typeof(int)),                 // �������� ��� ���������

                new DataColumn("EXPR_DT", typeof(string)),              // ��������� ��� ����
                
                new DataColumn("PP_NAME", typeof(string)),              // ������������ ����������
                new DataColumn("TYPOP", typeof(int)),                   // ��� �������� (�������, ��������, ...)
                
                new DataColumn("KOBJ", typeof(string))  }));            // ��� ������� (C(10))

            DT[BD_KMPL].dt.Columns["EXPR_DT"].Expression = "substring(DT,7,2) + '.' + substring(DT,5,2)";

            DT[BD_KMPL].dt.Columns["TYPOP"].DefaultValue = AppC.TYPOP_KMPL;

            DT[BD_KMPL].dt.PrimaryKey = new DataColumn[] { DT[BD_KMPL].dt.Columns["SYSN"] };
            DT[BD_KMPL].dt.Columns["SYSN"].AutoIncrement = true;
            DT[BD_KMPL].dt.Columns["SYSN"].AutoIncrementSeed = -1;
            DT[BD_KMPL].dt.Columns["SYSN"].AutoIncrementStep = -1;
            DT[BD_KMPL].nType = TBLTYPE.BD;

            // ������ �� ����� ����������
            DT.Add(NS_BLANK, new TableDef(NS_BLANK, new DataColumn[]{
                new DataColumn("TD",        typeof(int)),               // ��� ��������
                new DataColumn("KBL",       typeof(string)),            // ��� ������
                new DataColumn("NAME",      typeof(string)),            // ������������ ������
                new DataColumn("PS",        typeof(int)),               // �������� ��������� �����
                new DataColumn("BCT",       typeof(string)),            // ���� ���� ��� ������
                new DataColumn("NPARS",     typeof(int)) }));           // ���������� �������������� ����������
            DT[NS_BLANK].dt.PrimaryKey = new DataColumn[] { 
                DT[NS_BLANK].dt.Columns["TD"],
                DT[NS_BLANK].dt.Columns["KBL"]};
            DT[NS_BLANK].Text = "������ ����������";

            // ��������� ������� (���� ����� �����)
            DT.Add(NS_SBLK, new TableDef(NS_SBLK, new DataColumn[]{
                new DataColumn("KBL",       typeof(string)),            // ��� ������
                new DataColumn("NPP",       typeof(int)),               // � �/�
                new DataColumn("KPAR",      typeof(string)),            // ������������ ���������
                new DataColumn("NAME",      typeof(string)),            // ���������� ���������
                new DataColumn("TPAR",      typeof(string)),            // ��� ���������
                new DataColumn("VALUE",     typeof(string)),            // �������� ���������
                new DataColumn("PRFX",      typeof(string)),            // ������� ���������
                new DataColumn("PARS",      typeof(string)),            // ��� ���� ���������
                new DataColumn("BEGS",      typeof(int)),               // �������� � ������ �� (1,...
                new DataColumn("LENS",      typeof(int)),               // �������� � ������ �� (1,...
                new DataColumn("FUNC",      typeof(string)),            // ������� ����� Validate
                new DataColumn("DSOURCE",   typeof(string)),            // DataSource
                new DataColumn("DISPLAY",   typeof(string)),            // DisplayMember
                new DataColumn("RESULT",    typeof(string)),            // ValueMember
                new DataColumn("TBCODE",    typeof(string)),            // ��� ���������
                new DataColumn("PERCAPT",   typeof(int)),               // �������� � ������ �� (1,...
                new DataColumn("BCT",       typeof(string)),            // ���� ���� ��� EXPR
                new DataColumn("FORMAT",    typeof(string)) }));        // ������ ��������� �����
            DT[NS_SBLK].dt.PrimaryKey = new DataColumn[] { 
                DT[NS_SBLK].dt.Columns["KBL"], DT[NS_SBLK].dt.Columns["KPAR"]};
            //DT[NS_SBLK].nType = TBLTYPE.INTERN | TBLTYPE.NSI;

            DT[NS_SBLK].dt.Columns["DSOURCE"].DefaultValue = "";
            DT[NS_SBLK].dt.Columns["DISPLAY"].DefaultValue = "";
            DT[NS_SBLK].dt.Columns["RESULT"].DefaultValue = "";

            DT[NS_SBLK].Text = "������ ���������� ������";
        }

        // �������� ������ ��������� ������
        public void ConnDTGrid(DataGrid dgDoc, DataGrid dgDet)
        {
            dgDoc.SuspendLayout();
            DT[BD_DOCOUT].dg = dgDoc;
            dgDoc.DataSource = DT[BD_DOCOUT].dt;
            CreateTableStyles(DT[BD_DOCOUT].dg);
            ChgGridStyle(BD_DOCOUT, GDOC_VNT);
            dgDoc.ResumeLayout();

            // �������� ��������� �����
            dgDet.SuspendLayout();
            DT[BD_DOUTD].dg = dgDet;
            // � ������ - ��� �� Grid
            DT[BD_DIND].dg = dgDet;
            CreateTableStylesDet(dgDet);
            ChgGridStyle(BD_DIND, GDET_ZVK);
            //ChgGridStyle(BD_DIND, GDET_ZVK_KMPL);
            // �� ��������� - �������� ���
            dgDet.DataSource = dsM.Relations[0].ChildTable;
            ChgGridStyle(BD_DOUTD, GDET_SCAN);
            dgDet.ResumeLayout();
        }

        // ����� ��������� ������� ���������� � �����
        private void CreateTableStyles(DataGrid dg)
        {
            DataGridTableStyle 
                ts,
                tss;
            // ����������� ����� ��� ����������� ��������
            System.Drawing.Color 
                colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            ServClass.DGTBoxColorColumnDoc
                sC;

            dg.TableStyles.Clear();

            // ��� �����������
            ts = new DataGridTableStyle();
            ts.MappingName = GDOC_VNT.ToString();

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_DT";
            sC.HeaderText = "����";
            sC.Width = 31;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "TD";
            sC.HeaderText = "T";
            sC.Width = 10;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KSK";
            sC.HeaderText = "�����";
            sC.Width = 35;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_SRC";
            sC.HeaderText = "����";
            sC.Width = 32;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KRKPP";
            sC.HeaderText = "�-��";
            sC.Width = 33;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.ReadOnly = true;
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "MEST";
            sC.HeaderText = "����";
            sC.NullText = "";
            sC.Width = 36;
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "NOMD";
            sC.HeaderText = "� ���";
            sC.Width = 55;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);

            // ��� ��������������
            DataGridTableStyle tsi = new DataGridTableStyle();
            tsi.MappingName = GDOC_INV.ToString();

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_DT";
            sC.HeaderText = "����";
            sC.Width = 31;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "TD";
            sC.HeaderText = "T";
            sC.Width = 10;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KSMEN";
            sC.HeaderText = "�����";
            sC.Width = 35;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "NUCH";
            sC.HeaderText = "��";
            sC.Width = 20;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KEKS";
            sC.HeaderText = "����";
            sC.Width = 33;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_SRC";
            sC.HeaderText = "����";
            sC.Width = 32;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "MEST";
            sC.HeaderText = "����";
            sC.NullText = "";
            sC.Width = 36;
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "MESTZ";
            sC.HeaderText = "�����";
            sC.Width = 40;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "DIFF";
            sC.HeaderText = "��";
            sC.Width = 18;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KOLE";
            sC.HeaderText = "�����";
            sC.Width = 36;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(tsi);

            // ��� ������������
            tss = new DataGridTableStyle();
            tss.MappingName = GDOC_CENTR.ToString();

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "PP_NAME";
            sC.HeaderText = "����������";
            sC.Width = 130;
            sC.NullText = "";
            tss.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "MEST";
            sC.HeaderText = "����";
            sC.Width = 40;
            sC.NullText = "";
            tss.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "DIFF";
            sC.HeaderText = "���";
            sC.Width = 35;
            sC.NullText = "";
            tss.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(tss);
        }


        private Color
            C_READY_ZVK = Color.LightGreen,                 // ��������� ������ ���������
            C_READY_TTN = Color.Lavender,                   // ��������� ��� ������ � ��������
            C_TNSFD_TTN = Color.LightGreen;                 // ��������� ��� �������� �� ������

        // ����� ������� ��������� ����� (��� � ������)
        private void CreateTableStylesDet(DataGrid dg)
        {
            DataGridTableStyle
                tsKV,
                tsK,
                ts;
            DataGridTextBoxColumn 
                colTB;
            ServClass.DGTBoxColorColumn 
                sC;
            Color 
                colSpec = Color.PaleGoldenrod,
                colGreen = Color.LightGreen;

            dg.TableStyles.Clear();

            ts = new DataGridTableStyle();                                  // ��� ����������� ������������ (���)
            ts.MappingName = GDET_SCAN.ToString();

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KRKMC";
            sC.HeaderText = "���";
            sC.Width = 27;
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "SNM";
            sC.HeaderText = "������������";
            sC.Width = 136;
            sC.AlternatingBackColor = colGreen;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KOLM";
            sC.HeaderText = "�-�";
            sC.AlternatingBackColor = colSpec;
            sC.Width = 26;
            ts.GridColumnStyles.Add(sC);

            colTB = new DataGridTextBoxColumn();
            colTB.MappingName = "EMK";
            colTB.HeaderText = "���";
            colTB.Width = 35;
            ts.GridColumnStyles.Add(colTB);

            colTB = new DataGridTextBoxColumn();
            colTB.MappingName = "NP";
            colTB.HeaderText = "���";
            colTB.Width = 35;
            colTB.NullText = "";
            ts.GridColumnStyles.Add(colTB);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "DVR";
            sC.HeaderText = "����";
            sC.Width = 36;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KOLE";
            sC.HeaderText = "��.";
            sC.Width = 43;
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            //colTB = new DataGridTextBoxColumn();
            //colTB.MappingName = "NPODD";
            //colTB.HeaderText = "���";
            //colTB.Width = 25;
            //colTB.NullText = "";
            //ts.GridColumnStyles.Add(colTB);

            colTB = new DataGridTextBoxColumn();
            colTB.MappingName = "NPODDZ";
            colTB.HeaderText = "���";
            colTB.Width = 25;
            colTB.NullText = "";
            ts.GridColumnStyles.Add(colTB);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "SSCC";
            sC.HeaderText = "SSCC";
            sC.Width = 128;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KTARA";
            sC.HeaderText = "����";
            sC.Width = 35;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);

            /// *************************** ��� ������ ************************
            ts = new DataGridTableStyle();                                      // � ������ �������� ���������
            ts.MappingName = GDET_ZVK.ToString();

            tsK = new DataGridTableStyle();                                     // � ������ ������������
            tsK.MappingName = GDET_ZVK_KMPL.ToString();

            tsKV = new DataGridTableStyle();                                     // � ������ ������������
            tsKV.MappingName = GDET_ZVK_KMPLV.ToString();

            //sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            //sC.MappingName = "NPODDZ";
            //sC.HeaderText = "��";
            //sC.Width = 22;
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "NPP";
            sC.HeaderText = "�";
            sC.Width = 30;
            sC.Alignment = HorizontalAlignment.Right;
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "KRKMC";
            sC.HeaderText = "���";
            sC.Width = 30;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "SNM";
            sC.HeaderText = "������������";
            sC.Width = 136;
            ts.GridColumnStyles.Add(sC);
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "EMK";
            sC.HeaderText = "���";
            sC.Width = 28;
            sC.Alignment = HorizontalAlignment.Right;
            //ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = Color.Azure;
            sC.MappingName = "KOLM";
            sC.HeaderText = "����";
            sC.Width = 36;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "KOLE";
            sC.HeaderText = "��.";
            sC.Width = 45;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "EMK";
            sC.HeaderText = "���";
            sC.Width = 28;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "DVR";
            sC.HeaderText = "����";
            sC.Width = 34;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "NP";
            sC.HeaderText = "� ��";
            sC.Width = 36;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);


            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "KRKPP";
            sC.HeaderText = "�-��";
            sC.Width = 36;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Left;
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "DTG";
            sC.HeaderText = "�����";
            sC.Width = 34;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "KOLE";
            sC.HeaderText = "��.";
            sC.Width = 45;
            sC.Alignment = HorizontalAlignment.Right;
            //ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            //c.MappingName = "KRKT";
            sC.MappingName = "KTARA";
            sC.HeaderText = "����";
            sC.Width = 35;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);
            dg.TableStyles.Add(tsK);
            dg.TableStyles.Add(tsKV);
        }


        public string SortDet(bool bIsTTN, NSI x, ref string sNSort)
        {
            int nCurSort;
            string nI, sRet = "";

            nI = (bIsTTN == true)?BD_DOUTD:BD_DIND;

            nCurSort = ((int)x.DT[nI].TSort) + 1;

            if (nCurSort == (int)TABLESORT.MAXDET)
                nCurSort = 0;
            x.DT[nI].TSort = nCurSort;
            sRet = SortName(bIsTTN, ref sNSort);

            x.DT[nI].sTSort = sRet;
            return (sRet);
        }

        public string SortName(bool bIsTTN, ref string sNSort)
        {
            string 
                sT = (bIsTTN)?BD_DOUTD:BD_DIND,
                sRet = "";
            switch ((NSI.TABLESORT)DT[sT].TSort)
            {
                case TABLESORT.NO:
                    sRet = "";
                    sNSort = "- - ";
                    break;
                case TABLESORT.KODMC:
                    sRet = "KRKMC";
                    sNSort = "- K - ";
                    break;
                case TABLESORT.NAMEMC:
                    sRet = "SNM";
                    sNSort = "- H - ";
                    break;
                case TABLESORT.RECSTATE:
                    sRet = (bIsTTN == true) ? "DEST" : "READYZ";
                    sNSort = "- B - ";
                    break;
            }
            sNSort += "\xAF";
            if (DT[sT].sTFilt != "")
                sNSort += "F";
            return (sRet);
        }

        // ����� ����� �������
        // nSt - ��������� �����
        public void ChgGridStyle(string iT, int nSt)
        {
            if (DT[iT].nGrdStyle != -1)
            {                                                           // �� ��������� ���������
                int nOld = DT[iT].nGrdStyle;
                string sCurStyle = DT[iT].dg.TableStyles[nOld].MappingName;

                // ������� �������
                DT[iT].dg.TableStyles[nOld].MappingName = nOld.ToString();
                if (nSt == GDOC_NEXT)
                {                                                       // ����������� �����
                    nSt = ((nOld + 1) == DT[iT].dg.TableStyles.Count) ? 0 : nOld + 1;
                }
            }
            DT[iT].nGrdStyle = nSt;
            DT[iT].dg.TableStyles[nSt].MappingName = DT[iT].dt.TableName;
        }

        //public static string GrdDocStyleName(int i)
        //{
        //    string ret = "";
        //    if (i == GDOC_CENTR)
        //        ret = "���";
        //    else if (i == GDOC_INV)
        //        ret = "���";
        //    else if (i == GDOC_SAM)
        //        ret = "���";
        //    return (ret);
        //}


        // �������������� ��� ����� �������
        public DataRow BD_TINF_RW(string sTName)
        {
            DataRow 
                dr = DT[NSI.BD_TINF].dt.Rows.Find(new object[] { sTName });
            if (dr == null)
            {
                dr = DT[NSI.BD_TINF].dt.NewRow();
                dr["DT_NAME"] = sTName;
                dr["MD5"] = "";
                dr["LASTLOAD"] = DateTime.MinValue;
                dr["LOAD_HOST"] = "";
                dr["LOAD_PORT"] = 0;
                dr["FLAG_LOAD"] = "";

                DT[NSI.BD_TINF].dt.Rows.Add(dr);
            }
            return (dr);
        }

        private DataView
            dvEmk = null;
        // �������� �� ��������� �������� ����������� � ������
        // ����� ������������� ����� �� ������
        public int AfterLoadNSI(string sTName, bool bFromSrv, string sFileFromSrv)
        {
            int 
                nKr = 0,
                nRet = AppC.RC_OK;

            switch (sTName)
            {
                case NSI.NS_TYPD:
                    foreach (DataRow dr in DT[NS_TYPD].dt.Rows)
                    {
                        try
                        {
                            nKr = (int)dr["KOD"];
                            //if (!DocPars.dicTypD.ContainsKey(nKr))
                            //    DocPars.dicTypD.Add(nKr, (string)dr["NAME"]);
                            //else
                            //    DocPars.dicTypD[nKr] = (string)dr["NAME"];

                            if (AppC.xDocTInf.ContainsKey(nKr))
                                AppC.xDocTInf[nKr].Name = (string)dr["NAME"];
                            else
                                AppC.xDocTInf.Add(nKr, new DocTypeInf(nKr,(string)dr["NAME"], AppC.MOVTYPE.RASHOD));

                        }
                        catch { }
                    }
                    break;
                case NSI.NS_MC:
                    DT[NS_KREAN].dt.BeginLoadData();
                    DT[NS_KREAN].dt.Clear();
                    try
                    {
                        foreach (DataRow dr in DT[NS_MC].dt.Rows)
                        {
                            if (!(dr["GRADUS"] is string))
                                dr["GRADUS"] = "";
                            if (dr["KRKMC"] is int)
                                if ((int)dr["KRKMC"] > 0)
                                    DT[NS_KREAN].dt.Rows.Add(new object[] { dr["EAN13"], dr["KMC"], dr["KRKMC"] });
                        }
                        DT[NS_KREAN].dt.EndLoadData();
                    }
                    catch(Exception e)
                    {
                        nKr = 28;
                    }
                    finally
                    {
                    }
                    break;
                case NSI.NS_SEMK:
                    if (sFileFromSrv.Length > 0)
                    {// ���������� ������ �� �������
                        try
                        {
                            foreach (DataRow dr in DT[NS_SEMK].dt.Rows)
                            {
                                if (!(dr["KTARA"] is string))
                                    dr["KTARA"] = "";
                                if (!(dr["GTIN"] is string))
                                    dr["GTIN"] = "";
                                if (!(dr["PR"] is int))
                                    dr["PR"] = 0;
                            }
                        }
                        catch { nKr = 28; }
                    }
                    else
                    {// ��������� �������� (���������� ������)
                        DT[NSI.NS_SEMK].SetAddSort("KMC");
                        DT[NSI.NS_SEMK].SetAddSort("GTIN");
                    }
                    break;
                case NSI.NS_PRPR:
                    try
                    {
                        DataRow[] xMax = DT[NSI.NS_PRPR].dt.Select("KRK=MAX(KRK)");
                        if (xMax.Length > 0)
                            nKr = (int)xMax[0]["KRK"];
                    }
                    catch { }

                    foreach (DataRow drp in DT[NSI.NS_PRPR].dt.Rows)
                    {
                        if ((drp["KRK"] == DBNull.Value) || (((int)drp["KRK"] <= 0)))
                            drp["KRK"] = ++nKr;
                    }
                    if (sFileFromSrv.Length > 0)
                    {
                        DT[sTName].dt.WriteXml(sPathNSI + DT[sTName].sXML);
                        File.Delete(sFileFromSrv);
                        nRet = AppC.RC_BADTABLE;
                    }
                    break;
                case NSI.BD_PASPORT:
                    ExprDll.Expr xE = null;
                    Srv.LoadInterCode(out xE, xFF.xExpDic, DT[NSI.BD_PASPORT]);
                    xFF.xGExpr = xE;
                    MainF.AddrInfo.xR = (xFF.xGExpr.run.FindFunc(AppC.FEXT_ADR_NAME) is ExprDll.Action) ? xFF.xGExpr.run : null;
                    break;
            }
            MainF.AddrInfo.dtA = DT[NSI.NS_ADR].dt;
            return (nRet);
        }

        internal bool GetMCDataOnEAN(string sEAN, ref PSC_Types.ScDat s, bool bShowErr)
        {
            bool 
                ret = false;
            DataTable 
                dt = DT[NS_MC].dt;
            DataRow 
                dr = null;

            if (DT[NS_MC].nState > DT_STATE_INIT)  // ���������� ��������
            {
                string sss = 
                    String.Format("EAN13 LIKE '{0}%'", sEAN);
                DataView 
                    xRowDView = new DataView(DT[NSI.NS_MC].dt, String.Format("EAN13 LIKE '{0}%'", sEAN), "", DataViewRowState.CurrentRows);
                if (xRowDView.Count > 0)
                {
                    if (xRowDView.Count > 1)
                    {
                        if (bShowErr)
                            Srv.ErrorMsg(String.Format("EAN={0}\n���������� ITF({1})", sEAN, s.ci.ToString()), "���������������!", true);
                        return (false);
                    }
                    else
                        dr = xRowDView[0].Row;
                    ret = s.GetFromNSI(s.s, dr, DT[NSI.NS_MC].dt, bShowErr);
                    if (!ret)
                    {
                        if (bShowErr)
                            Srv.ErrorMsg(String.Format("EAN={0}", sEAN), "�� ������� � ���!", true);
                    }
                }
            }
            return (ret);
        }


        public bool GetMCData(string sKMCFull, ref PSC_Types.ScDat s, int nKrKMC, bool bShowErr)
        {
            bool
                ret = false;
            DataTable
                dt = DT[NS_MC].dt;
            DataRow
                dr = null;

            if (DT[NS_MC].nState > DT_STATE_INIT)  // ���������� ��������
            {
                if ((sKMCFull.Length == 0) && (nKrKMC > 0))
                {
                    dr = DT[NS_KREAN].dt.Rows.Find(new object[] { nKrKMC });
                    if (dr != null)
                        sKMCFull = (string)dr["KMC"];
                }
                if (sKMCFull.Length == 0)              // ��� �� Code128, EAN13 � ������� ����� 
                {
                    DataRow[] dra;
                    dra = dt.Select(String.Format("KRKMC={0}", s.nKrKMC));
                    if (dra != null)
                        dr = dra[0];
                }
                else
                {
                    dr = DT[NS_MC].dt.Rows.Find(new object[] { sKMCFull });
                }
                ret = s.GetFromNSI(s.s, dr, DT[NSI.NS_MC].dt);
                if (!ret)
                {
                    if (bShowErr)
                        Srv.ErrorMsg(String.Format("KMC={0}\n���={1}", sKMCFull, nKrKMC), "�� ������� � ���!", true);
                }
                //else
                //{
                //    s.xEmks = new Srv.Collect4Show<StrAndInt>(xFF.GetEmk4KMC(ref s, dr, true, out nDefEmk));
                //    if ((s.xEmks.Count > 0) && (s.fEmk == 0))
                //    {
                //        s.xEmks.CurrIndex = nDefEmk;
                //        s.fEmk = s.fEmk_s = ((StrAndInt)s.xEmks.Current).DecDat;
                //    }
                //}

            }

            return (ret);
        }


        // ����� ����� ���������� ����� �����������
        internal bool IsAlien(string sEAN13, ref PSC_Types.ScDat s)
        {
            string 
                sKMC = "",
                sFind;
            bool 
                ret = false;
            int 
                nL;
            DataRow dr = null;
            DataRow[] dra = null;

            if (DT[NS_KRUS].nState > DT_STATE_INIT)     // ���������� ��������
            {
                //sFind = sEAN13.Substring(0, 7);


                //dra = DT[NS_KRUS].dt.Select("[KINT] LIKE '" + sEAN13 + "'");
                dra = DT[NS_KRUS].dt.Select("[KINT] LIKE '" + sEAN13.Substring(0, 3) + "%'");
                foreach (DataRow drr in dra)
                {
                    sFind = (string)drr["KINT"];
                    nL = sFind.Length;
                    if ( (sFind == sEAN13.Substring(0, nL)))
                    {
                        dr = drr;
                        s.sIntKod = sFind;
                        sKMC = (string)drr["KMC"];
                        break;
                    }
                }
                //DT[NS_KRUS].dt.DefaultView.RowFilter = String.Format("[KINT] LIKE '{0}%'", sV);

                //dr = DT[NS_KRUS].dt.Rows.Find(new object[] { sFind });
                if (dr != null)
                {
                    dr = DT[NS_MC].dt.Rows.Find(new object[] { sKMC });
                    if (dr != null)
                    {
                        //ret = s.GetFromNSI(s.s, dr);
                        ret = s.GetFromNSI(s.s, dr, DT[NSI.NS_MC].dt);
                        //if (ret)
                        //    s.xEmks = new Srv.Collect4Show<StrAndInt>(xFF.GetEmk4KMC(ref s, dr, true, out nL));
                    }
                }
                DT[NS_KRUS].dt.DefaultView.RowFilter = "";
            }
            return (ret);
        }




        private void FillPoddonlLst(CurDoc xD)
        {
            DataTable 
                dt = DT[NSI.BD_DIND].dt,
                dtD = DT[NSI.BD_DOUTD].dt;

            // ������ ������� �������� �� ���������
            DataView dv1 = new DataView(dtD, xD.DefDetFilter(), "", DataViewRowState.CurrentRows);
            DataTable dtN1 = dv1.ToTable(true, "NPODDZ");

            xD.xNPs.Clear();

            if (xD.xDocP.TypOper == AppC.TYPOP_KMPL)
            {
                // ������ ������� �������� �� ������
                DataView dv = new DataView(dt, xD.DefDetFilter(), "", DataViewRowState.CurrentRows);
                DataTable dtN = dv.ToTable(true, "NPODDZ");

                // ��� ��������� ������������?
                if ((dtN.Rows.Count == 1) && ((int)(dtN.Rows[0]["NPODDZ"]) > 0))
                {
                    xD.bFreeKMPL = true;
                }
                else
                    xD.bFreeKMPL = false;

                foreach (DataRow dr in dtN.Rows)
                {
                    if (!xD.xNPs.ContainsKey((int)dr["NPODDZ"]))
                        xD.xNPs.Add((int)dr["NPODDZ"], new PoddonInfo());
                }
            }

            foreach (DataRow dr in dtN1.Rows)
            {
                if (!xD.xNPs.ContainsKey((int)dr["NPODDZ"]))
                    xD.xNPs.Add((int)dr["NPODDZ"], new PoddonInfo());
            }
        }

        //public string AdrName(string sA)
        //{
        //    string 
        //        sR = "";
        //    try
        //    {
        //        DataRow dr = DT[NS_ADR].dt.Rows.Find(new object[] { sA });
        //        if (dr != null)
        //            sR = (string)dr["NAME"];
        //    }
        //    catch { sR = ""; }
        //    return (sR);
        //}




        // ������ ������� ������ � ������ ������ ����������
        public bool InitCurDoc(CurDoc xD, Smena xS)
        {
            bool ret = false;
            //int i;

            if (xD.drCurRow != null)
            {
                try
                {
                    //int i = xD.nCurRec;
                    DocPars x = xD.xDocP;

                    //DataRow dr = DT[NSI.BD_DOCOUT].dt.Rows[i];
                    DataRow dr = xD.drCurRow;

                    xD.nId = (int)((dr["SYSN"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["SYSN"]);
                    xD.nDocSrc = (int)((dr["SOURCE"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["SOURCE"]);
                    xD.ID_DocLoad = (dr["ID_LOAD"] is string) ? (string)dr["ID_LOAD"] : "";

                    x.nTypD = (int)((dr["TD"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["TD"]);
                    x.sNomDoc = ((dr["NOMD"] == System.DBNull.Value) ? "" : dr["NOMD"].ToString());
                    x.nPol = (int)((dr["KRKPP"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["KRKPP"]);
                    x.sSmena = ((dr["KSMEN"] == System.DBNull.Value) ? "" : dr["KSMEN"].ToString());
                    x.nSklad = (int)((dr["KSK"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["KSK"]);
                    x.nUch = (int)((dr["NUCH"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["NUCH"]);
                    x.nEks = (int)((dr["KEKS"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["KEKS"]);

                    x.sEks = (dr["EKS_NAME"] is string)?(string)dr["EKS_NAME"] : "";
                    x.sPol = (dr["PP_NAME"] is string) ? (string)dr["PP_NAME"] : "";

                    try
                    {
                        x.dDatDoc = DateTime.ParseExact(dr["DT"].ToString(), "yyyyMMdd", null);
                    }
                    catch
                    {
                        x.dDatDoc = DateTime.MinValue;
                    }

                    xD.xDocP.TypOper = (int)((dr["TYPOP"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["TYPOP"]);

                    //i = (int)((dr["CONFSCAN"] == System.DBNull.Value) ? 0 : dr["CONFSCAN"]);
                    xD.bConfScan = (xFF.ConfScanOrNot(dr, xFF.xPars.ConfScan) > 0) ? true : false;
                    

                    xD.sLstUchNoms = (string)((dr["LSTUCH"] == System.DBNull.Value) ? "" : dr["LSTUCH"]);

                    xD.xNPs = new PoddonList();
                    FillPoddonlLst(xD);
                    if (xD.xDocP.TypOper == AppC.TYPOP_KMPL)
                    {
                        xS.FilterTTN = FILTRDET.UNFILTERED;
                        DT[NSI.BD_DOUTD].sTFilt = "";
                    }
                    xD.xDocP.lSysN = (int)((dr["DIFF"] == System.DBNull.Value) ? AppC.EMPTY_INT : dr["DIFF"]);
                    ret = true;
                }
                catch
                {

                }
            }
            return (ret);
        }



        // ���������� ������ ������� ����������
        public bool UpdateDocRec(DataRow dr, CurDoc xD)
        {
            bool 
                ret = true;
            DocPars 
                x = xD.xDocP;
            try
            {
                if (x.nSklad != AppC.EMPTY_INT)
                    dr["KSK"] = x.nSklad;
                else
                    dr["KSK"] = System.DBNull.Value;

                if (x.nUch != AppC.EMPTY_INT)
                    dr["NUCH"] = x.nUch;
                else
                    dr["NUCH"] = System.DBNull.Value;


                dr["DT"] = x.dDatDoc.ToString("yyyyMMdd");

                dr["KSMEN"] = x.sSmena;

                if (x.nTypD != AppC.EMPTY_INT)
                {
                    dr["TD"] = x.nTypD;
                }
                else
                    dr["TD"] = System.DBNull.Value;


                dr["NOMD"] = x.sNomDoc;

                if (x.nPol != AppC.EMPTY_INT)
                {
                    dr["KRKPP"] = x.nPol;
                    dr["PP_NAME"] = x.sPol;
                }
                else
                {
                    dr["KRKPP"] = System.DBNull.Value;
                    dr["PP_NAME"] = System.DBNull.Value;
                }

                if (x.nEks != AppC.EMPTY_INT)
                {
                    dr["KEKS"] = x.nEks;
                    dr["EKS_NAME"] = x.sEks;
                }
                else
                {
                    dr["KEKS"] = System.DBNull.Value;
                    dr["EKS_NAME"] = System.DBNull.Value;
                }

                //if ((xD.xDocP.TypOper != AppC.TYPOP_DOCUM) && (nRegApp == AppC.REG_DOC))
                //    xD.xDocP.TypOper = AppC.TYPOP_DOCUM;

                dr["TYPOP"] = xD.xDocP.TypOper;
                //dr["CONFSCAN"] = xFF.ConfScanOrNot(dr, xFF.xPars.ConfScan);
                dr["DIFF"] = (int)(xD.xDocP.lSysN);

                dr["ID_LOAD"] = xD.ID_DocLoad;
            }
            catch
            {
                ret = false;
            }
            return(ret);
        }

        // ���������� ����� ������ � ���������
        public bool AddDocRec(CurDoc xD)
        {
            bool 
                ret = true;

            try
            {
                DataRow dr = DT[NSI.BD_DOCOUT].dt.NewRow();
                dr["SOURCE"] = DOCSRC_CRTD;
                dr["TIMECR"] = DateTime.Now;

                ret = UpdateDocRec(dr, xD);
                if (ret == true)
                {
                    DT[NSI.BD_DOCOUT].dt.Rows.Add(dr);
                    xD.nId = (int)dr["SYSN"];
                    xD.drCurRow = dr;
                }
            }
            catch {
                ret = false;
            }
            return (ret);
        }

        // ������ ������� ��������� ������
        public bool InitCurProd(ref PSC_Types.ScDat scD, DataRow drD)
        {
            bool 
                ret = false;
            string 
                s;
            MainF.AddrInfo
                xA;

            if (drD != null)
            {
                try
                {
                    scD.nKrKMC = (int)((drD["KRKMC"] == System.DBNull.Value) ? AppC.EMPTY_INT : drD["KRKMC"]);
                    scD.sN = (string)((drD["SNM"] == System.DBNull.Value) ? "" : drD["SNM"]);
                    scD.nMest = (int)((drD["KOLM"] == System.DBNull.Value) ? AppC.EMPTY_INT : drD["KOLM"]);
                    scD.fVsego = (FRACT)((drD["KOLE"] == System.DBNull.Value) ? 0 : drD["KOLE"]);
                    scD.fEmk = (FRACT)((drD["EMK"] == System.DBNull.Value) ? 0 : drD["EMK"]);
                    scD.nParty = (drD["NP"] is string) ? (string)drD["NP"]:"";
                    try
                    {
                        string sD = drD["DVR"].ToString();
                        scD.dDataIzg = DateTime.ParseExact(sD, "yyyyMMdd", null);
                        scD.sDataIzg = sD.Substring(6, 2) + "." + sD.Substring(4, 2) + "." + sD.Substring(2, 2);
                    }
                    catch
                    {
                        scD.dDataIzg = DateTime.MinValue;
                        scD.sDataIzg = "";
                    }
                    scD.sEAN = (string)((drD["EAN13"] == System.DBNull.Value) ? "" : drD["EAN13"]);
                    //scD.nTara = (int)((drD["KRKT"] == System.DBNull.Value) ? AppC.EMPTY_INT : drD["KRKT"]);
                    //scD.nTara = (drD["KRKT"] is string) ? (string)drD["KRKT"] : "";
                    scD.nTara = (drD["KTARA"] is string) ? (string)drD["KTARA"] : "";

                    scD.nKolSht = (int)((drD["KOLSH"] is int) ? drD["KOLSH"]:0);
                    scD.nKolG = (int)((drD["KOLG"] is int) ? drD["KOLG"] : 0);

                    int i = (int)((drD["SRP"] == System.DBNull.Value) ? AppC.EMPTY_INT : drD["SRP"]);
                    scD.bVes = (i == 1) ? true : false;

                    scD.sKMC = (string)((drD["KMC"] == System.DBNull.Value) ? "" : drD["KMC"]);
                    scD.nDest = (NSI.DESTINPROD)((drD["DEST"] == System.DBNull.Value) ? DESTINPROD.USER : drD["DEST"]);

                    scD.sGrK = (string)((drD["GKMC"] == System.DBNull.Value) ? "" : drD["GKMC"]);

                    scD.nRecSrc = (int)((drD["SRC"] == System.DBNull.Value) ? SRCDET.SCAN : drD["SRC"]);
                    scD.dtScan = (DateTime)((drD["TIMECR"] == System.DBNull.Value) ? DateTime.MinValue : drD["TIMECR"]);

                    //s = (string)((drD["ADRFROM"] == System.DBNull.Value) ? "" : drD["ADRFROM"]);
                    //scD.xOp.xAdrSrc = new MainF.AddrInfo(s, AdrName(s));
                    try
                    {
                        xA = new MainF.AddrInfo((string)drD["ADRFROM"], xFF.xSm.nSklad);
                    }
                    catch { xA = null; }
                    scD.xOp.SetOperSrc(xA, xFF.xCDoc.xDocP.nTypD, true);

                    //s = (string)((drD["ADRTO"] == System.DBNull.Value) ? "" : drD["ADRTO"]);
                    //scD.xOp.xAdrDst = new MainF.AddrInfo(s, AdrName(s));

                    try
                    {
                        xA = new MainF.AddrInfo((string)drD["ADRTO"], xFF.xSm.nSklad);
                    }
                    catch { xA = null; }
                    scD.xOp.SetOperDst(xA, xFF.xCDoc.xDocP.nTypD, true);

                    scD.nNPredMT = (drD["SYSPRD"] is int) ? ((int)drD["SYSPRD"]) : 0;

                    ret = true;
                }
                catch{}
            }
            return (ret);
        }



        public DataRow AddDet(PSC_Types.ScDat s, CurDoc xCDoc, DataRow drOld)
        {
            return (AddDet(s, xCDoc, drOld, true));
        }



        // ���������� ������� ��������� ������
        public DataRow AddDet(PSC_Types.ScDat s, CurDoc xCurrentDoc, DataRow drOld, bool bAddNew)
        {
            int
                nKey = (int)xCurrentDoc.drCurRow["SYSN"],
                nPodz = 0;
            DateTime
                dtCr;
            DataRow 
                ret = drOld;

            if (drOld == null)
            {
                try
                {
                    ret = DT[NSI.BD_DOUTD].dt.NewRow();

                    ret["KRKMC"] = s.nKrKMC;
                    ret["SNM"] = s.sN;
                    ret["KOLM"] = s.nMest;
                    ret["KOLE"] = s.fVsego;
                    ret["EMK"] = s.fEmk;
                    ret["NP"] = s.nParty;
                    ret["DVR"] = s.dDataIzg.ToString("yyyyMMdd");
                    ret["EAN13"] = s.sEAN;
                    ret["SYSN"] = nKey;
                    ret["SRP"] = (s.bVes == true) ? 1 : 0;
                    ret["GKMC"] = s.sGrK;

                    //if (s.nTara == AppC.EMPTY_INT)
                    //    ret["KRKT"] = System.DBNull.Value;
                    //else
                    //    ret["KRKT"] = s.nTara;
                    //ret["KRKT"] = s.nTara;

                    ret["KTARA"] = s.nTara;
                    
                    ret["KOLSH"] = s.nKolSht;

                    ret["VES"] = s.fVes;
                    ret["KOLG"] = s.nKolG;

                    ret["DEST"] = s.nDest;
                    ret["KMC"] = s.sKMC;

                    ret["SRC"] = s.nRecSrc;

                    dtCr = s.dtScan;
                    if (xCurrentDoc.xDocP.TypOper == AppC.TYPOP_MOVE)
                    {
                        if (xCurrentDoc.xOper.IsFillSrc())
                            dtCr = xCurrentDoc.xOper.xAdrSrc.ScanDT;
                    }
                    ret["TIMECR"] = dtCr;

                    ret["NPODD"] = s.nNomPodd;
                    ret["NMESTA"] = s.nNomMesta;

                    s.xOp = xCurrentDoc.xOper;
                    ret["ADRFROM"] = s.xOp.GetSrc(false);
                    ret["ADRTO"] = s.xOp.GetDst(false);

                    // ������� ����� ������
                    ret["GKMC"] = s.s;

                    /// changed 11.04.17
                    // ret["SSCC"] = xCurrentDoc.xOper.SSCC;
                    ret["SSCC"] = s.sSSCC;

                    try
                    {
                        if ((xCurrentDoc.xDocP.TypOper == AppC.TYPOP_KMPL) || (xCurrentDoc.xDocP.TypOper == AppC.TYPOP_OTGR))
                            nPodz = xCurrentDoc.xNPs.Current;
                        else
                            nPodz = 0;
                    }
                    catch
                    {
                        nPodz = 0;
                    }
                    ret["NPODDZ"] = nPodz;

                    //if (xCDoc.xOper != null)
                    //{
                    //    if (xCDoc.xOper.IsFillSrc())
                    //        ret["ADRFROM"] = xCDoc.xOper.xAdrSrc.Addr;
                    //    if (xCDoc.xOper.IsFillDst())
                    //        ret["ADRTO"] = xCDoc.xOper.xAdrDst.Addr;
                    //}
                    ret["SYSPRD"] = s.nNPredMT;


                    if (bAddNew)
                        DT[NSI.BD_DOUTD].dt.Rows.Add(ret);
                }
                catch
                {
                    Srv.ErrorMsg("������ ���������� ���������!");
                }
            }
            else
            {
                drOld["KOLM"] = (int)drOld["KOLM"] + s.nMest;
                drOld["KOLE"] = (FRACT)drOld["KOLE"] + s.fVsego;
                drOld["VES"] = (FRACT)drOld["VES"] + s.fVes;

                drOld["NPODD"] = s.nNomPodd;
                drOld["NMESTA"] = s.nNomMesta;
            }
            return(ret);
        }

        // ���������� DataSet ��� ��������
        //public DataSet MakeWorkDataSet_(DataTable dtM, DataTable dtD, DataRow[] drA, Smena xSm, CurUpLoad xCU)
        //{

        //    DataTable dtMastNew = dtM.Clone();
        //    DataTable dtDetNew = dtD.Clone();
        //    DataTable dtBNew = DT[BD_SPMC].dt.Clone();
        //    DataRow[] aDR, childRows;
        //    bool bNeedRow;
        //    string sS;

        //    DataRelation myRelation = dtM.ChildRelations[REL2TTN];


        //    foreach (DataRow dr in drA)
        //    {
        //        //DataRow drm = dtMastNew.NewRow();
        //        //drm.ItemArray = dr.ItemArray;
        //        //dtMastNew.Rows.Add(drm);
        //        dtMastNew.LoadDataRow(dr.ItemArray, true);

        //        if (xCU.bOnlyCurRow)
        //            // �������������� �������� ����� ������ �� ��������� ��������
        //            childRows = new DataRow[]{ xCU.drForUpl };
        //        else
        //            childRows = dr.GetChildRows(myRelation);

        //        foreach (DataRow chRow in childRows)
        //        {
        //            //DataRow drd = dtDetNew.NewRow();
        //            //drd.ItemArray = chRow.ItemArray;
        //            //dtDetNew.Rows.Add(drd);

        //            //if ((xSm.RegApp == AppC.REG_OPR) && ((AppC.OPR_STATE)chRow["STATE"] == AppC.OPR_STATE.OPR_UPL))
        //            try
        //            {
        //                if (((int)dr["TD"] == AppC.TYPD_OPR) &&
        //                    ((AppC.OPR_STATE)chRow["STATE"] == AppC.OPR_STATE.OPR_UPL))
        //                    bNeedRow = false;
        //                else
        //                {
        //                    sS = (chRow["SSCC"] == System.DBNull.Value)?"":"1";
        //                    sS += (chRow["SSCCINT"] == System.DBNull.Value)?"":"2";
        //                    if (((int)dr["TYPOP"] == AppC.TYPOP_MARK) && (sS.Length == 0))
        //                        bNeedRow = false;
        //                    else
        //                        bNeedRow = true;
        //                }
        //                if (xCU.sCurUplCommand == AppC.COM_PRNDOC)
        //                {
        //                }
        //            }
        //            catch
        //            {
        //                bNeedRow = false;
        //            }
        //            if (bNeedRow)
        //            {
        //                dtDetNew.LoadDataRow(chRow.ItemArray, true);

        //                aDR = chRow.GetChildRows(REL2BRK);
        //                foreach (DataRow bR in aDR)
        //                {
        //                    //r = dtBNew.NewRow();
        //                    //r.ItemArray = bR.ItemArray;
        //                    //dtBNew.Rows.Add(r);

        //                    dtBNew.LoadDataRow(bR.ItemArray, true);
        //                }
        //            }
        //        }
        //    }

        //    DataSet ds1Rec = new DataSet("dsMOne");
        //    ds1Rec.Tables.Add(dtMastNew);
        //    ds1Rec.Tables.Add(dtDetNew);
        //    ds1Rec.Tables.Add(dtBNew);
        //    return (ds1Rec);
        //}





        // ���������� DataSet ��� ��������
        public DataSet MakeWorkDataSet(DataTable dtM, DataTable dtD, DataRow[] drA, DataRow[] drDetReady, 
            Smena xSm, CurUpLoad xCU)
        {
            DataRow
                drAdded;
            DataTable 
                dtMastNew = dtM.Clone(),
                dtDetNew = dtD.Clone(),
                dtBNew = DT[BD_SPMC].dt.Clone(),
                dtSCNew = DT[BD_SSCC].dt.Clone();
            DataRow[] 
                aDR, childRows;
            bool 
                bNeedRow;
            string 
                sS;
            int
                nOldTyp;

            DataRelation myRelation = dtM.ChildRelations[REL2TTN];


            foreach (DataRow dr in drA)
            {
                //nOldTyp = (int)dr["TD"];
                //if ((int)dr["TD"] == AppC.TYPD_PRIH)
                //    dr["TD"] = AppC.TYPD_VPER;
                dtMastNew.LoadDataRow(dr.ItemArray, true);
                //dr["TD"] = nOldTyp;

                //if (xCU.sCurUplCommand == AppC.COM_ZSC2LST)
                //    break;
                // ��� SSCC ��������� ������ ���������
                if ((xCU.sCurUplCommand == AppC.COM_ZSC2LST) ||
                     (xCU.sCurUplCommand == AppC.COM_ADR2CNT) )
                    break;


                if (drDetReady == null)
                {// ������ ��������� ����� ��� �� �����
                    if (xCU.bOnlyCurRow)
                        // �������������� �������� ����� ������ �� ��������� ��������
                        childRows = new DataRow[] { xCU.drForUpl };
                    else
                        childRows = dr.GetChildRows(myRelation);
                }
                else
                    childRows = drDetReady;

                dtDetNew.BeginLoadData();
                foreach (DataRow chRow in childRows)
                {
                    try
                    {
                        bNeedRow = true;
                        #region ������������� ��������� ��������� ������
                        do
                        {
                            if (drDetReady != null)
                                // ��� �������������� ������ ������ � ��������
                                break;

                            if ((int)dr["TYPOP"] != AppC.TYPOP_DOCUM)
                            {// ��� ������������� ������ ����� ���� ��������...
                                if ((AppC.OPR_STATE)chRow["STATE"] == AppC.OPR_STATE.OPR_TRANSFERED)
                                {// �������� ��� �����������
                                    bNeedRow = false;
                                }
                                else
                                {
                                    if ((int)dr["TYPOP"] == AppC.TYPOP_MOVE)
                                    {
                                        sS = (chRow["ADRFROM"] == System.DBNull.Value) ? "" : (string)chRow["ADRFROM"];
                                        if ((sS.Length > 0) && (xCU.sCurUplCommand != AppC.COM_CKCELL))
                                        {
                                            sS = (chRow["ADRTO"] == System.DBNull.Value) ? "" : (string)chRow["ADRTO"];
                                        }
                                        if (sS.Length == 0)
                                            bNeedRow = false;
                                    }
                                    else
                                    {
                                        sS = (chRow["SSCC"] == System.DBNull.Value) ? "" : "1";
                                        sS += (chRow["SSCCINT"] == System.DBNull.Value) ? "" : "2";
                                        if (sS.Length == 0)
                                        {// ����������������� ��������� 
                                            if (((int)dr["TYPOP"] == AppC.TYPOP_MARK) ||
                                                ((int)dr["TYPOP"] == AppC.TYPOP_KMPL))
                                                bNeedRow = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                            }


                        } while (false);
                        #endregion
                    }
                    catch
                    {
                        bNeedRow = false;
                    }

                    if (bNeedRow)
                    {
                        drAdded = dtDetNew.LoadDataRow(chRow.ItemArray, true);
                        //if (((int)drAdded["SYSPRD"] < 0) && ((SRCDET)drAdded["SRC"] == NSI.SRCDET.SCAN))
                        //{// ��� ��������
                        //    drAdded["NP"] = "";
                        //}

                        if ((int)drAdded["SYSPRD"] < 0) 
                        {// ��� ��������
                            drAdded["NP"] = "";
                        }

                        aDR = chRow.GetChildRows(REL2BRK);
                        foreach (DataRow bR in aDR)
                        {
                            //r = dtBNew.NewRow();
                            //r.ItemArray = bR.ItemArray;
                            //dtBNew.Rows.Add(r);

                            dtBNew.LoadDataRow(bR.ItemArray, true);
                        }

                    }
                }
                dtDetNew.EndLoadData();

                if ((xCU.sCurUplCommand == AppC.COM_VTTN) ||
                     (xCU.sCurUplCommand == AppC.COM_VVPER) ||
                     (xCU.sCurUplCommand == AppC.COM_VINV))
                {
                    aDR = dr.GetChildRows(REL2SSCC);
                    foreach (DataRow bR in aDR)
                    {
                        if ((int)(bR["IN_TTN"]) >= 1)
                            dtSCNew.LoadDataRow(bR.ItemArray, true);
                    }
                }

            }

            DataSet ds1Rec = new DataSet("dsMOne");
            ds1Rec.Tables.Add(dtMastNew);
            ds1Rec.Tables.Add(dtDetNew);
            ds1Rec.Tables.Add(dtBNew);
            ds1Rec.Tables.Add(dtSCNew);
            return (ds1Rec);
        }





        public DataTable MakeTempDOUTD(DataTable dtResult)
        {
            return (MakeTempDOUTD(dtResult, dtResult.TableName));
        }

        public DataTable MakeTempDOUTD(DataTable dtResult, string sDTName)
        {
            int
                i = 0;

            DataColumn[] dcl = new DataColumn[dtResult.Columns.Count];
            foreach (DataColumn dc in dtResult.Columns)
                dcl[i++] = new DataColumn(dc.ColumnName, dc.DataType);
            DataTable dtOut = new DataTable(sDTName);
            dtOut.Columns.AddRange(dcl);
            return (dtOut);
        }

        // ������� ��� ������
        public DataSet MakeDataSetForLoad(DataTable dtM, DataTable dtD, DataTable dtSSCC, string sDSName)
        {

            DataTable
                dtMastNew = MakeTempDOUTD(dtM),
                dtDetNew = MakeTempDOUTD(dtD),
                dtPictNew = MakeTempDOUTD(DT[BD_PICT].dt),
                dtSSCCNew = MakeTempDOUTD(dtSSCC);

            DataSet 
                dsWZvk = new DataSet(sDSName);

            DataColumn 
                dcDocHeader = dtMastNew.Columns["SYSN"],
                dcDet = dtDetNew.Columns["SYSN"],
                dcSC = dtSSCCNew.Columns["SYSN"];

            dsWZvk.Tables.Add(dtMastNew);
            dsWZvk.Tables.Add(dtDetNew);
            dsWZvk.Tables.Add(dtSSCCNew);
            dsWZvk.Tables.Add(dtPictNew);

            if (sDSName == "dsZ")
            {// �������� ���������
                dtMastNew.TableName = BD_ZDOC;
                dtDetNew.TableName = BD_ZDET;

                dsWZvk.Relations.Add(REL2ZVK, dcDocHeader, dcDet);
                dsWZvk.Relations.Add(REL2SSCC, dcDocHeader, dcSC);
                dsWZvk.Relations.Add(REL2PIC, dcDocHeader, dtPictNew.Columns["SYSN"]);
            }
            else
            {
                //dtDetNew.PrimaryKey = null;
                //dtSSCCNew.PrimaryKey = null;
            }

            return (dsWZvk);
        }


        private string sP_CSDat = "CSDat.xml";

        /// ������������ ����������� ������
        public int DSRestore(string sDS, DateTime dCur, int nMaxD, bool bControlDate)
        {
            int i = 0,
                nRet = AppC.RC_OK;
            DateTime 
                dD;
            TimeSpan 
                tsD;
            DataSet
                dsTMP;

            dsM.AcceptChanges();
            dsTMP = dsM.Copy();
            try
            {
                dsM.BeginInit();
                dsM.EnforceConstraints = false;
                dsM.Clear();
                try
                {
                    dsM.ReadXml(sDS + sP_CSDat);
                    if (bControlDate)
                    {
                        if (nMaxD > 0)
                        {
                            while (i < DT[NSI.BD_DOCOUT].dt.Rows.Count)
                            {
                                dD = DateTime.ParseExact((string)(DT[NSI.BD_DOCOUT].dt.Rows[i]["DT"]), "yyyyMMdd", null);
                                tsD = dCur.Subtract(dD);
                                if (tsD.Days > nMaxD)
                                    DT[NSI.BD_DOCOUT].dt.Rows.RemoveAt(i);
                                else
                                    i++;
                            }
                        }
                    }
                }
                catch (Exception ee)
                {// ��, ������, �� ���� 
                    nRet = AppC.RC_CANCEL;
                }

                dsM.EnforceConstraints = true;
                dsM.EndInit();
            }
            catch
            {
                nRet = AppC.RC_NOFILE;
            }
            if (nRet != AppC.RC_OK)
            {
                dsM.BeginInit();
                dsM.EnforceConstraints = false;
                dsM.Clear();
                dsM.Merge(dsTMP);
                dsM.EnforceConstraints = true;
                dsM.EndInit();
            }

            return (nRet);
        }

        public int DSSave(string sF)
        {
            int ret = AppC.RC_OK;

            try
            {
                ClearPics();
                dsM.WriteXml(sF + sP_CSDat);
            }
            catch
            {
                ret = AppC.RC_CANCEL;
            }

            return (ret);
        }

        private void ClearPics()
        {
            foreach(DataRow dr in DT[NSI.BD_DOCOUT].dt.Rows)
            {
                dr["PICTURE"] = System.DBNull.Value;
            }
            dsM.Tables[BD_PICT].Clear();
        }


    }
}
