using System;
using System.ComponentModel;
using System.Windows.Forms;

using ExprDll;
using ScannerAll;

using PDA.OS;
using PDA.Service;

using FRACT = System.Decimal;

namespace SkladGP
{
    public partial class MainF : Form
    {




        private void SetMainFuncDict(ScannerAll.TERM_TYPE ttX, string sExeDir)
        {
            string[] sH = new string[0];

            xFuncs = new FuncDic(sExeDir + "KeyMap.xml"); 
            if (xFuncs.Loaded == false)
            {
                xFuncs.SetDefaultFunc();
                // �������� ������� �������-������� (Datalogic Skorpio - 38-keys)

                xFuncs.SetNewFunc(W32.VK_F2,        Keys.Shift,     AppC.F_CTRLDOC,     "SHIFT-F2",     " - �������� ���������");
                xFuncs.SetNewFunc(W32.VK_F3,        Keys.Shift,     AppC.F_CHGSCR,      "SHIFT-F3",     " - ������������� �����");
                xFuncs.SetNewFunc(W32.VK_F4,        Keys.None,      AppC.F_CHG_REC,     "F4",           " - �������� ������");
                xFuncs.SetNewFunc(W32.VK_F4,        Keys.Shift,     AppC.F_FLTVYP,      "SHIFT-F4",     " - ������");
                xFuncs.SetNewFunc(W32.VK_F6,        Keys.None,      AppC.F_DEL_REC,     "F6",           " - ������� �������");
                xFuncs.SetNewFunc(W32.VK_F6,        Keys.Shift,     AppC.F_DEL_ALLREC,  "SHIFT-F6",     " - ������� ���");
                xFuncs.SetNewFunc(W32.VK_F8,        Keys.None,      AppC.F_ADD_REC,     "F8",           " - ����� �������");
                xFuncs.SetNewFunc(W32.VK_F9,        Keys.Shift,     AppC.F_VES_CONF,    "SHIFT-F9",     " - ������������� ENT");
                xFuncs.SetNewFunc(W32.VK_F10,       Keys.None,      AppC.F_CHG_LIST,    "F10",          " - ���/������/��� ������");
                xFuncs.SetNewFunc(W32.VK_ENTER,     Keys.Shift,     AppC.F_VIEW_DOC,    "SHIFT-ENT",    " - ���������");
                xFuncs.SetNewFunc(W32.VK_ENTER,     Keys.Control,   AppC.F_NEXTDOC,     "CTRL-ENT",     " - ��������� ��������");
                xFuncs.SetNewFunc(W32.VK_ENTER,     Keys.Alt,       AppC.F_PREVDOC,     "ALT-ENT",      " - ���������� ��������");
                xFuncs.SetNewFunc(W32.VK_UP,        Keys.Control,   AppC.F_GOFIRST,     "CTRL-^",       " - �� ������ ������");
                xFuncs.SetNewFunc(W32.VK_DOWN,      Keys.Control,   AppC.F_GOLAST,      "CTRL-v",       " - �� ��������� ������");
                //xFuncs.SetNewFunc(W32.VK_F11,       Keys.None,      AppC.F_DEBUG,       "F11",          "");

                xFuncs.SetNewFunc(W32.VK_F1,        Keys.Alt,       AppC.F_SIMSCAN,       "ALT-F1", "");
                xFuncs.AddNewFunc(W32.VK_F1,        Keys.Control,   AppC.F_SIMSCAN,       "CTRL-F1", "");

                xFuncs.SetNewFunc(W32.VK_F5,        Keys.Control,   AppC.F_CHG_SORT,    "CTRL-F5",      " - ����������");
                xFuncs.SetNewFunc(W32.VK_F5,        Keys.Shift,     AppC.F_EASYEDIT,    "SHIFT-F5",     " - ���������� ����");
                xFuncs.SetNewFunc(W32.VK_F8,        Keys.Shift,     AppC.F_TOT_MEST,    "SHIFT-F8",     " - ����� ����, ���");

                xFuncs.SetNewFunc(W32.VK_F1,        Keys.Shift,     AppC.F_LASTHELP,    "SHIFT-F1",     " - �������� ���������");
                //xFuncs.SetNewFunc(W32.VK_F1,        Keys.Shift,     AppC.F_ADR2CNT,     "",             " - ���������� �� ������");


                //xFuncs.SetNewFunc(W32.VK_D1,        Keys.Control,   AppC.F_PODDMIN,     "Ctl-1",        " - ������� ������� -");
                //xFuncs.SetNewFunc(W32.VK_D3,        Keys.Control,   AppC.F_PODDPLUS,    "Ctl-3",        " - ������� ������� +");
                xFuncs.SetNewFunc(W32.VK_RIGHT,     Keys.Control,   AppC.F_NEXTPAGE,    "CTRL-->",      " - ��������� �������");
                xFuncs.SetNewFunc(W32.VK_LEFT,      Keys.Control,   AppC.F_PREVPAGE,    "CTRL-<-",      " - ���������� �������");
                xFuncs.SetNewFunc(W32.VK_SPACE,     Keys.Shift,     AppC.F_PODD,        "SHIFT-SPC",    " - �������->�����");
                xFuncs.SetNewFunc(W32.VK_F10,       Keys.Shift,     AppC.F_SAMEKMC,     "SHIFT-F10",    " - KMC � ������ ������");

                //xFuncs.SetNewFunc(W32.VK_F10,       Keys.Control,   AppC.F_ZVK2TTN,     "",             " - ������� � ���");

                xFuncs.SetNewFunc(W32.VK_F10,       Keys.Control,   AppC.F_CHG_VIEW,    "CTRL-F10",     " - ��� ������");


                xFuncs.SetNewFunc(W32.VK_F7,        Keys.None,      AppC.F_BRAKED,      "F7",           " - ���� �����");
                xFuncs.SetNewFunc(W32.VK_F7,        Keys.Shift,     AppC.F_SHLYUZ,      "SHIFT-F7",     " - �������/������");
                xFuncs.SetNewFunc(W32.VK_D2,        Keys.None,      AppC.F_OPROVER,     "2",            " - �������� ��������");
                xFuncs.SetNewFunc(W32.VK_PERIOD,    Keys.None,      AppC.F_SETPODD,     ".",            " - ��������� SSCC");
                xFuncs.SetNewFunc(W32.VK_PERIOD,    Keys.Control,   AppC.F_SETPODDCUR,  "CTRL-.",       " - ��������� SSCC(�����)");

                xFuncs.SetNewFunc(W32.VK_F3,        Keys.Control,   AppC.F_LOADKPL,     "CTRL-F3",      " - ����� �������");
                xFuncs.SetNewFunc(W32.VK_D3,        Keys.None,      AppC.F_LOADOTG,     "3",            " - ����� ��������");

                if (xPars.UseFixAddr)
                    xFuncs.SetNewFunc(W32.VK_D0,        Keys.Control,   AppC.F_SETADRZONE,  "CTRL-0",       " - ������������� �����");
                else
                    xFuncs.SetNewFunc(W32.VK_D0,        Keys.Control,   AppC.F_SETADRZONE,  "", " - ������������� �����");

                xFuncs.SetNewFunc(W32.VK_D8,        Keys.Control,   AppC.F_SETPRN,      "CTRL-8",       " - ����� ��������");
                xFuncs.SetNewFunc(W32.VK_D9,        Keys.Control,   AppC.F_PRNDOC,      "CTRL-9",       " - ��������� ��������");
                xFuncs.SetNewFunc(W32.VK_D7,        Keys.Control,   AppC.F_EXLDPALL,    "CTRL-7",       " - ���.�/����.�� �������");

                xFuncs.SetNewFunc(W32.VK_D1,        Keys.Control,   AppC.F_KMCINF,      "CTRL-1",       " - ��� �������� ���������");
                xFuncs.SetNewFunc(W32.VK_D3,        Keys.Control,   AppC.F_CELLINF,     "CTRL-3",       " - ���������� ������");
                xFuncs.SetNewFunc(W32.VK_D2,        Keys.Control,   AppC.F_PRNBLK,      "CTRL-2",       " - ������ ���������");
                xFuncs.SetNewFunc(W32.VK_D4,        Keys.Control,   AppC.F_CONFSCAN,    "CTRL-4",       " - ������������� ����");

                xFuncs.SetNewFunc(W32.VK_D5,        Keys.Control,   AppC.F_STARTQ1ST,   "CTRL-5",       " - ������� �������");
                xFuncs.SetNewFunc(W32.VK_D6,        Keys.Control,   AppC.F_JOINPCS,     "CTRL-6",       " - ������� ������");
                xFuncs.SetNewFunc(W32.VK_D1,        Keys.Shift,     AppC.F_MARKWMS,     "SHIFT-1",      " - SSCC ��� WMS");
                xFuncs.SetNewFunc(W32.VK_F5,        Keys.None,      AppC.F_A4MOVE,      "F5",           " - ������ �� ������");
                xFuncs.SetNewFunc(W32.VK_F5,        Keys.Alt,       AppC.F_TMPMOV,      "ALT-F5",       " - ����������� 1 ������ ");
                xFuncs.SetNewFunc(W32.VK_D3,        Keys.Alt,       AppC.F_REFILL,      "ALT-3",        " - ��������� �����");

                xFuncs.SetNewFunc(W32.VK_D8,        Keys.Alt,       AppC.F_NEWOPER,     "ALT-8",        " - ����� ��������");
                xFuncs.SetNewFunc(W32.VK_D3,        Keys.Shift,     AppC.F_CNTSSCC,     "SHIFT-3",      " - ���������� SSCC");

                xFuncs.SetNewFunc(W32.VK_F3,        Keys.Alt,       AppC.F_ZZKZ1,       "ALT-F3",       " - �������� 1 ������");
                xFuncs.SetNewFunc(W32.VK_SPACE,     Keys.None,      AppC.F_FLTSSCC,     "SPC",          " - ������ �� SSCC");
                xFuncs.SetNewFunc(W32.VK_D1,        Keys.Alt,       AppC.F_SSCCSH,      "ALT-1",        " - ����-���������� SSCC");

                xFuncs.SetNewFunc(W32.VK_RIGHT,     Keys.Shift,     AppC.F_NEXTPL,      "SHIFT-->",     " - ��������� ������");
                xFuncs.SetNewFunc(W32.VK_F2,        Keys.Alt,       AppC.F_CHKSSCC,     "ALT-F2",       " - �������� SSCC");

                xFuncs.SetNewFunc(W32.VK_F10,       Keys.Alt,       AppC.F_SHOWPIC,     "ALT-F10",      " - ����� �������");

                xFuncs.SetNewFunc(W32.VK_F2,        Keys.Control,   AppC.F_LOAD4CHK,    "CTRL-F2",      " - �������� ��� ��������");


                xFuncs.SetNewFunc(W32.VK_D9, Keys.Shift, AppC.F_GENFUNC, "", " - �������");
                //xFuncs.AddNewFunc(W32.VK_F9_PC,     Keys.None,      AppC.F_MENU,        "F9",           "");

                switch (ttX)
                {
                    case TERM_TYPE.NRDMERLIN:
#if NRDMERLIN
                        xFuncs.SetNewFunc(W32.VK_HYPHEN,    Keys.Alt,   AppC.F_NEXTDOC,     "Alt- - ", " - ��������� ��������");
                        xFuncs.SetNewFunc(W32.VK_PERIOD,    Keys.Alt,   AppC.F_PREVDOC,     "Alt-.", " - ���������� ��������");

                        xFuncs.SetNewFunc(W32.VK_D1, Keys.Alt, AppC.F_KMCINF, "Alt-1", " - ��� �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_D3, Keys.Alt, AppC.F_CELLINF, "Alt-3", " - ���������� ������");
                        xFuncs.SetNewFunc(W32.VK_D2, Keys.Alt, AppC.F_PRNBLK, "ALT-2", " - ������ ���������");
                        xFuncs.SetNewFunc(W32.VK_D8, Keys.Alt, AppC.F_SETPRN, "ALT-8", " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_D9, Keys.Alt, AppC.F_PRNDOC, "ALT-9", " - ��������� ��������");
                        xFuncs.SetNewFunc(W32.VK_D4, Keys.Alt, AppC.F_CONFSCAN, "ALT-4", " - ������������� ����");
                        xFuncs.SetNewFunc(W32.VK_D7, Keys.Alt, AppC.F_EXLDPALL, "ALT-7", " - ���. �/����. �� ������");

                        xFuncs.SetNewFunc(W32.VK_D3, Keys.None, AppC.F_LOADKPL, "3", " - ����� �������");
                        xFuncs.SetNewFunc(W32.VK_D1, Keys.Shift, AppC.F_MARKWMS, "Shift-1", " - ���������� SSCC");
                        //xFuncs.SetNewFunc(W32.VK_F3, Keys.Control, AppC.F_LOADOTG, "CTRL-F3", " - ����� ��������");


#endif
                        break;
                    case TERM_TYPE.SYMBOL:
                        xFuncs.SetNewFunc(W32.VK_ASCII_Y, Keys.None, AppC.F_CHG_SORT, "Y", " - ����������");
                        break;
                    case TERM_TYPE.HWELL6100:
#if HWELL6100
		                 
                        xFuncs.Clear();
                        xFuncs.SetNewFunc(W32.VK_F1,        Keys.None,      AppC.F_HELP,            "F1",   "");
                        xFuncs.SetNewFunc(W32.VK_F2,        Keys.None,      AppC.F_UPLD_DOC,        "F2",   " - ���������");
                        xFuncs.SetNewFunc(W32.VK_F3,        Keys.None,      AppC.F_LOAD_DOC,        "F3",   " - ���������");
                        xFuncs.SetNewFunc(W32.VK_F4,        Keys.None,      AppC.F_CHG_REC,         "F4",   " - �������� ������");
                        xFuncs.SetNewFunc(W32.VK_F5,        Keys.None,      AppC.F_EASYEDIT,        "Func-1",   " - ���������� ����");
                        xFuncs.SetNewFunc(W32.VK_F6,        Keys.None,      AppC.F_MENU,            "Func-2",   " - ����");
                        xFuncs.SetNewFunc(W32.VK_F7,        Keys.None,      AppC.F_QUIT,            "Func-3",   " - �����");

                        xFuncs.SetNewFunc(W32.VK_D1,        Keys.None,      AppC.F_CHG_SORT,        " 1",   " - ����������");
                        xFuncs.SetNewFunc(W32.VK_D2,        Keys.None,      AppC.F_FLTVYP,          " 2",   " - ������");
                        xFuncs.SetNewFunc(W32.VK_D3,        Keys.None,      AppC.F_CHGSCR,          " 3",   " - ������������� �����");
                        xFuncs.SetNewFunc(W32.VK_D4,        Keys.None,      AppC.F_CTRLDOC,         " 4",   " - �������� ���������");
                        //xFuncs.SetNewFunc(W32.VK_D5,        Keys.None,      AppC.F_LASTHELP,        " 5",   " - �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_D5,        Keys.None,      AppC.F_TOT_MEST,        " 5",   " - ����� ����, ���");
                        xFuncs.SetNewFunc(W32.VK_D6,        Keys.None,      AppC.F_VIEW_DOC,        " 6",   " - ���������");
                        xFuncs.SetNewFunc(W32.VK_D7,        Keys.None,      AppC.F_GOFIRST,         " 7",   " - �� ������ ������");
                        xFuncs.SetNewFunc(W32.VK_D9,        Keys.None,      AppC.F_GOLAST,          " 9",   " - �� ��������� ������");
                        xFuncs.SetNewFunc(W32.VK_D8,        Keys.None,      AppC.F_ADD_REC,         " 8",   " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_D0,        Keys.None,      AppC.F_CHG_GSTYLE,      " 0",   " - ���/������");

                        //xFuncs.SetNewFunc(W32.VK_FUNC_F1,   Keys.None,      AppC.F_DEL_REC,         "Func-F1", " - �������");
                        xFuncs.SetNewFunc(W32.VK_BACK,      Keys.None,      AppC.F_DEL_REC,         "BKSP", " - �������");
                        xFuncs.SetNewFunc(0, Keys.None, -1, "-><-", " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_SPACE,     Keys.None,      AppC.F_NEXTDOC,         "SP",   " - ��������� ��������");
                        xFuncs.SetNewFunc(W32.VK_PERIOD,    Keys.None,      AppC.F_PREVDOC,         ".",    " - ���������� ��������");
                        xFuncs.SetNewFunc(W32.VK_DEL,       Keys.None,      AppC.F_DEL_ALLREC,      "Func-0",  " - ������� ���");
                        xFuncs.SetNewFunc(W32.VK_TAB,       Keys.None,      AppC.F_NEXTPAGE,        "Func-7",  " - ������� ������");

                        //xFuncs.SetNewFunc(W32.VK_D1,        Keys.Shift,     AppC.F_LOADKPL,         "Ctrl-1", " - ����� �������");
                        xFuncs.SetNewFunc(W32.VK_D2,        Keys.Shift,     AppC.F_OPROVER,         "Ctrl-2", " - �������� ��������");
                        xFuncs.SetNewFunc(W32.VK_D3,        Keys.Shift,     AppC.F_LOADOTG,         "Ctrl-3", " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_D4,        Keys.Shift,     AppC.F_SAMEKMC,         "Ctrl-4", " - ����� ���/��������");
                        xFuncs.SetNewFunc(W32.VK_D5,        Keys.Shift,     AppC.F_SHLYUZ,          "Ctrl-5", " - ��������/������");
                        xFuncs.SetNewFunc(W32.VK_D6,        Keys.Shift,     AppC.F_VES_CONF,        "Ctrl-6", " - ������������� Ent");
                        xFuncs.SetNewFunc(W32.VK_D7,        Keys.Shift,     AppC.F_BRAKED,          "Ctrl-7", " - ���� �����");
                        xFuncs.SetNewFunc(W32.VK_D9,        Keys.Shift,     AppC.F_SETPRN,          "Ctrl-8", " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_D0,        Keys.Shift,     AppC.F_PRNDOC,          "Ctrl-9", " - ������ ��������");

                        //xFuncs.SetNewFunc(W32.VK_D8,        Keys.Shift,     AppC.F_SETADRZONE,      "Ctrl-0", " - ������������� �����");

                        xFuncs.SetNewFunc(W32.VK_D8,        Keys.Shift,     AppC.F_PRNBLK,          "Ctrl-0", " - ������ ���������");
                        xFuncs.SetNewFunc(W32.VK_HYPHEN, Keys.None, AppC.F_SETPODD, "Ctrl-.", " - ���������� ������");
                        /*
                        */

                        xFuncs.SetNewFunc(W32.VK_FUNC_F2,   Keys.None,       AppC.F_KMCINF,          "Func-F2"," - ��� �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_FUNC_F3,   Keys.None,       AppC.F_CELLINF,         "Func-F3"," - ���������� ������");

                        xFuncs.SetNewFunc(W32.VK_D9, Keys.Control, AppC.F_GENFUNC, "", " - �������");
                        xFuncs.SetNewFunc(W32.VK_FUNC_F4, Keys.None, AppC.F_CONFSCAN, "Func-F4", " - ������������� ����");
                        //xFuncs.SetNewFunc(W32.VK_D1,       Keys.Shift,      AppC.F_MARKWMS,     "Ctrl-1",            " - ���������� SSCC");
                        xFuncs.SetNewFunc(W32.VK_FUNC_F1,   Keys.None,      AppC.F_MARKWMS,         "Func-F1", " - ���������� SSCC");


                        xFuncs.AddNewFunc(W32.VK_F9_PC, Keys.None, AppC.F_MENU, "", " - ����");
                        xFuncs.AddNewFunc(W32.VK_F1_PC, Keys.None, AppC.F_HELP, "F1", "");
                        xFuncs.AddNewFunc(W32.VK_F2_PC, Keys.None, AppC.F_UPLD_DOC, "F2", "");
                        xFuncs.AddNewFunc(W32.VK_F8_PC, Keys.None, AppC.F_DEL_REC, "F2", "");
#endif
                        break;
                    case TERM_TYPE.DOLPH7850:
#if DOLPH7850
                        xFuncs.SetNewFunc(W32.VK_F5,        Keys.None,  AppC.F_EASYEDIT,    "F5",       " - ���������� ����");
                        xFuncs.SetNewFunc(W32.VK_F6,        Keys.None,  AppC.F_CTRLDOC,     "F6",       " - �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_F7,        Keys.None,  AppC.F_CHGSCR,      "F7",       " - ������������� �����");
                        xFuncs.SetNewFunc(W32.VK_DEL,       Keys.None,  AppC.F_DEL_REC,     "DEL",      " - ������� �������");
                        xFuncs.SetNewFunc(W32.VK_HYPHEN,    Keys.Shift, AppC.F_DEL_ALLREC,  "SFT--",    " - ������� ���");
                        xFuncs.SetNewFunc(W32.VK_MONSIGN,   Keys.None,  AppC.F_FLTVYP,      "#",        " - ������");
                        xFuncs.SetNewFunc(W32.VK_QUOTE,     Keys.None,  AppC.F_CHG_SORT,    "''",       " - ����������");
                        xFuncs.SetNewFunc(W32.VK_D2,        Keys.Shift, AppC.F_VES_CONF,    "SFT-2",    " - ������������� ����������");
                        xFuncs.SetNewFunc(W32.VK_MULTIPLY,  Keys.None,  AppC.F_PODD,        "*",        " - �������->�����");
                        xFuncs.SetNewFunc(W32.VK_RIGHT,     Keys.Shift, AppC.F_SAMEKMC,     "SFT-->",   " - ��� �� ��� � ������");
                        xFuncs.SetNewFunc(W32.VK_LEFT,      Keys.Shift, AppC.F_ZVK2TTN,     "SFT-<-",   " - ������� � ���");
                        xFuncs.SetNewFunc(W32.VK_UP,        Keys.Shift, AppC.F_GOFIRST,     "SFT-^",    " - �� ������ ������");
                        xFuncs.SetNewFunc(W32.VK_DOWN,      Keys.Shift, AppC.F_GOLAST,      "SFT-v",    " - �� ��������� ������");
                        xFuncs.SetNewFunc(W32.VK_COMMA,     Keys.None,  AppC.F_NEXTDOC,     ".",        " - ��������� ��������");
                        xFuncs.SetNewFunc(W32.VK_PERIOD,    Keys.None,  AppC.F_PREVDOC,     ",",        " - ���������� ��������");
                        xFuncs.SetNewFunc(W32.VK_EQUAL,     Keys.None,  AppC.F_TOT_MEST,    "=",        " - ����� ����, ���");
                        xFuncs.SetNewFunc(W32.VK_BACK,      Keys.None,  AppC.F_PODDMIN,     "BKSP",     " - ������� ������� -");
                        xFuncs.SetNewFunc(W32.VK_SPACE,     Keys.None,  AppC.F_PODDPLUS,    "SPC",      " - ������� ������� +");
                        xFuncs.SetNewFunc(W32.VK_TAB,       Keys.None,  AppC.F_NEXTPAGE,    "TAB",      " - ��������� �������");
                        xFuncs.SetNewFunc(W32.VK_TAB,       Keys.Shift, AppC.F_PREVPAGE,    "SFT-TAB",  " - ���������� �������");
                        xFuncs.SetNewFunc(W32.VK_D1,        Keys.Shift, AppC.F_LASTHELP,    "SFT-1",    " - �������� ���������");

                        xFuncs.SetNewFunc(W32.VK_D7,        Keys.None,  AppC.F_BRAKED,      "7",        " - ���� �����");
                        xFuncs.SetNewFunc(W32.VK_D7,        Keys.Shift, AppC.F_SHLYUZ,      "SFT-7",    " - �������/������");
#endif
                        break;
                    case TERM_TYPE.DOLPH9950:
#if DOLPH9950
                        xFuncs.Clear();
                        xFuncs.SetNewFunc(W32.VK_F1,        Keys.None,      AppC.F_HELP,        "F1", "");
                        xFuncs.SetNewFunc(W32.VK_F2,        Keys.None,      AppC.F_UPLD_DOC,    "F2",       " - ���������");
                        xFuncs.SetNewFunc(W32.VK_F3,        Keys.None,      AppC.F_LOAD_DOC,    "F3",       " - ���������");
                        xFuncs.SetNewFunc(W32.VK_F4,        Keys.None,      AppC.F_CHG_REC,     "F4",       " - �������� ������");

                        xFuncs.SetNewFunc(W32.VK_F1,        Keys.Shift,     AppC.F_MENU,        "SFT-F1",   " - ����");
                        xFuncs.SetNewFunc(W32.VK_F3,        Keys.Shift,     AppC.F_EASYEDIT,    "SFT-F3",   " - ���������� ����");
                        xFuncs.SetNewFunc(W32.VK_F4,        Keys.Shift,     AppC.F_QUIT,        "SFT-F4",   " - �����");

                        xFuncs.SetNewFunc(W32.VK_D1,        Keys.None,      AppC.F_CHG_SORT,    " 1",       " - ����������");
                        xFuncs.SetNewFunc(W32.VK_D2,        Keys.None,      AppC.F_FLTVYP,      " 2",       " - ������");
                        xFuncs.SetNewFunc(W32.VK_D3,        Keys.None,      AppC.F_CHGSCR,      " 3",       " - ������������� �����");
                        xFuncs.SetNewFunc(W32.VK_D4,        Keys.None,      AppC.F_CTRLDOC,     " 4",       " - �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_D5,        Keys.None,      AppC.F_LASTHELP,    " 5",       " - �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_D6,        Keys.None,      AppC.F_VIEW_DOC,    " 6",       " - ���������");
                        xFuncs.SetNewFunc(W32.VK_D7,        Keys.None,      AppC.F_GOFIRST,     " 7",       " - �� ������ ������");
                        xFuncs.SetNewFunc(W32.VK_D9,        Keys.None,      AppC.F_GOLAST,      " 9",       " - �� ��������� ������");
                        xFuncs.SetNewFunc(W32.VK_D8,        Keys.None,      AppC.F_ADD_REC,     " 8",       " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_D0,        Keys.None,      AppC.F_CHG_GSTYLE,  " 0",       " - ���/������");

                        xFuncs.SetNewFunc(W32.VK_BACK,      Keys.None,      AppC.F_DEL_REC,     "BKSP",     " - �������");
                        xFuncs.SetNewFunc(0,                Keys.None,      -1,                 "-><-",     " - ����� ��������");
                        xFuncs.SetNewFunc(W32.VK_COMMA,     Keys.None,      AppC.F_NEXTDOC,     " ,",       " - ��������� ��������");
                        xFuncs.SetNewFunc(W32.VK_PERIOD,    Keys.None,      AppC.F_PREVDOC,     " .",       " - ���������� ��������");
                        xFuncs.SetNewFunc(W32.VK_DEL,       Keys.None,      AppC.F_DEL_ALLREC,  "DEL",      " - ������� ���");
                        xFuncs.SetNewFunc(W32.VK_TAB,       Keys.None,      AppC.F_NEXTPAGE,    "TAB",      " - ������� ������");
                        xFuncs.AddNewFunc(W32.VK_TAB,       Keys.Shift,     AppC.F_PREVPAGE,    "SFT-TAB",  " - ������� �����");

                        xFuncs.SetNewFunc(W32.VK_F2,        Keys.Shift,     AppC.F_PODD,        "SFT-F2",   " - �������->�����");
                        //xFuncs.SetNewFunc(W32.VK_HYPHEN,    Keys.Control,   AppC.F_PODDMIN,     " -",       " - ������� ������� -");
                        //xFuncs.SetNewFunc(W32.VK_PLUS,      Keys.None,      AppC.F_PODDPLUS,    " +",       " - ������� ������� +");
                        xFuncs.SetNewFunc(W32.VK_SPACE,     Keys.None,      AppC.F_TOT_MEST,    "SP",       " - ����� ����, ���");
                        xFuncs.SetNewFunc(W32.VK_F2,        Keys.Shift,     AppC.F_VES_CONF,    "SFT-2",    " - ������������� Ent");

                        xFuncs.SetNewFunc(W32.VK_FWIN,      Keys.None,      AppC.F_SAMEKMC,     "Start",    " - ��� �� ��� � ������");
                        xFuncs.SetNewFunc(W32.VK_SEND,      Keys.None,      AppC.F_ZVK2TTN,     "SEND",     " - ������� � ���");

                        xFuncs.AddNewFunc(W32.VK_F2_PC,     Keys.None,      AppC.F_UPLD_DOC,    "F2",   "");
                        xFuncs.AddNewFunc(W32.VK_F9_PC,     Keys.None,      AppC.F_MENU,        "",     " - ����");
                        xFuncs.AddNewFunc(W32.VK_ESC,       Keys.Shift, AppC.F_QUIT,        "",         " - �����");
#endif
                        break;
                    case TERM_TYPE.PSC4410:
#if PSC4410
                        xFuncs.SetNewFunc(W32.VK_F10, Keys.None, AppC.F_QUIT, "F10", " - �����");
                        xFuncs.SetNewFunc(W32.VK_D7, Keys.None, AppC.F_CTRLDOC, "7", " - �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_D8, Keys.None, AppC.F_LASTHELP, "8", " - �������� ���������");
                        xFuncs.SetNewFunc(W32.VK_D4, Keys.None, AppC.F_EASYEDIT, "4", " - ���������� ����");
                        xFuncs.SetNewFunc(W32.VK_D5, Keys.None, AppC.F_CHGSCR, "5", " - ������������� �����");
                        xFuncs.SetNewFunc(W32.VK_D6, Keys.None, AppC.F_FLTVYP, "6", " - ������");
                        xFuncs.SetNewFunc(W32.VK_D0, Keys.None, AppC.F_CHG_SORT, "0", " - ����������");
                        xFuncs.SetNewFunc(W32.VK_F7, Keys.None, AppC.F_DEL_ALLREC, "F7", " - ������� ���");
                        xFuncs.SetNewFunc(W32.VK_F5, Keys.None, AppC.F_TOT_MEST, "F5", " - ����� ����, ���");
                        //xFuncs.SetNewFunc(W32.VK_D1, Keys.None, AppC.F_PODDMIN, "1", " - ������� ������� -");
                        //xFuncs.SetNewFunc(W32.VK_D3, Keys.None, AppC.F_PODDPLUS, "3", " - ������� ������� +");
                        xFuncs.SetNewFunc(W32.VK_BACK, Keys.None, AppC.F_VES_CONF, "BKSP", " - ������������� ����������");
#endif
                        break;
                    case TERM_TYPE.PSC4220:
                        xFuncs.SetNewFunc(W32.VK_D4, Keys.Shift, AppC.F_HELP, "Fn1-$", "");
                        xFuncs.SetNewFunc(W32.VK_D7, Keys.Shift, AppC.F_DEL_REC, "Fn1-&", " - ������� �������");
                        xFuncs.SetNewFunc(W32.VK_USER_QUIT, Keys.None, AppC.F_QUIT, "Fn2-ESC", " - �����");
                        break;
                }
                xFuncs.SetDefaultHelp();
            }
        }

        private bool AlpHandle(bool bNewAlp)
        {
            bool ret = bNewAlp;

            //string sS;
            //char[] chArr = { '\u2191', '\u2193' };
            //string sCyf = "25";

            //byte[] bUTF8 = Encoding.GetEncoding(1252).GetBytes(chArr);
            //byte[] bUTF81 = Encoding.GetEncoding(65001).GetBytes(chArr);
            //string sTypDoc = Encoding.UTF8.GetString(bUTF8, 0, bUTF8.Length);
            //string ss = String.Format(

            if (AppPars.bArrowsWithShift == true)
            {
                if (((nCurDocFunc != AppC.DT_SHOW) && (tcMain.SelectedIndex == PG_DOC)) ||
                ((nCurVvodState != AppC.DT_SHOW) && (tcMain.SelectedIndex == PG_SCAN)) || (bEditMode == true))
                {// ��� ��������������
                    if (bNewAlp == true)
                        ret = false;
                }
                else
                {
                    if (bNewAlp == false)
                        ret = true;
                }

            }

            string sS = (ret == true) ? "\xAD\xAF" : "25";
            tDocAlpState.Text = sS;
            tDocAlpState.Refresh();
            //tCurrPoddon.Text = sS;
            tCurrPoddon.Refresh();

            return (ret);
        }








    }

    public partial class ServClass
    {
        private static byte IsArrow(KeyEventArgs e)
        {
            byte ret = 0;
            switch (e.KeyValue)
            {
                case 56:                // Up
                    ret = 38;
                    break;
                case 50:                // Down
                    ret = 40;
                    break;
                case 52:                // Left
                    ret = 37;
                    break;
                case 54:                // Right
                    ret = 39;
                    break;
                case 53:
                    ret = 13;
                    break;
            }
            return (ret);
        }

        public static bool HandleSpecMode(KeyEventArgs e, bool bEdit, ScannerAll.BarcodeScanner xSc)
        {
            bool ret = false;

            if (AppPars.bArrowsWithShift == true)
            {// ������� �������� ������ � Shift
                if (e.Shift == true)
                {
#if SYMBOL
                    if ((null != xSc) && (xSc.nTermType == ScannerAll.TERM_TYPE.SYMBOL) && (xSc.nKeys == 48))
                    {
                        byte newKey = IsArrow(e);
                        if (newKey > 0)
                        {// ��� ��������� �������
                            if (bEdit == true)
                            {// ������ ����� �������
                                if (e.Handled != true)
                                {
                                    //int sc = GetKeyState(W32.VK_CONTROL);
                                    //int ss = GetKeyState(W32.VK_SHIFT);
                                    ((ScannerAll.Symbol.SymbolBarcodeScanner)xSc).SetShiftOff();
                                    TimeSpan tsDiff;
                                    int nMSec = 0,
                                        t1 = Environment.TickCount;

                                    while (nMSec < 300)
                                    {
                                        tsDiff = new TimeSpan(0, 0, 0, 0, Environment.TickCount - t1);
                                        nMSec = tsDiff.Milliseconds;
                                    }

                                    //ss = GetKeyState(W32.VK_SHIFT);
                                    //sc = GetKeyState(W32.VK_CONTROL);

                                    //W32.keybd_event(W32.VK_UP, W32.VK_UP, W32.KEYEVENTF_SILENT, 0);
                                    //W32.keybd_event(W32.VK_UP, W32.VK_UP, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
                                    //W32.keybd_event(newKey, newKey, W32.KEYEVENTF_SILENT, 0);
                                    //W32.keybd_event(newKey, newKey, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
                                    W32.keybd_event(newKey, 0, W32.KEYEVENTF_SILENT, 0);
                                    W32.keybd_event(newKey, 0, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
                                    e.Handled = true;
                                }
                                ret = true;
                            }
                        }
                    }
#endif
                }
            }

            return (ret);
        }

    }



}
