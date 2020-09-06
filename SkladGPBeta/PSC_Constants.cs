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

        // Общий код для авторизации
        public const string GUEST = "000";
        //public const int RC_ADM_ONLY = 1;

        // для обмена
        public const int RC_NODATA = 301;

        // в режиме редактирования
        public const int RC_EDITEND = 6;
        public const int RC_WARN = 7;
        public const int RC_SAMEVES = 8;

        public const int RC_NOEAN       = 10;           // продукция отсутствует
        public const int RC_NOEANEMK    = 11;           // продукция данной емкости отсутствует
        public const int RC_ALREADY    = 12;           // заявка по продукции выполнена
        public const int RC_ZVKONLY     = 13;           // только если есть заявки
        public const int RC_BADTABLE    = 14;           // неподходящая таблица
        public const int RC_BADSCAN     = 15;           // продукция не подходит к заявке
        public const int RC_BADPODD     = 16;           // продукция на другом поддоне
        public const int RC_UNVISPOD    = 17;           // продукция не принадлежит текущему View
        public const int RC_NOAUTO      = 18;           // продукция не подбирается автоматом
        //public const int RC_BADDVR      = 19;           // срок годности не подходит
        public const int RC_NOSSCC      = 20;           // SSCC отсутствует
        public const int RC_MANYEAN     = 21;           // SSCC преобразовался в список
        public const int RC_BADPARTY    = 22;           // не та партия, что в заявке
        public const int RC_BADDATEXCT  = 23;           // конкретная дата не подходит
        public const int RC_NOTADR      = 24;           // SSCC не используется как адрес
        public const int RC_BADDATE     = 25;           // дата годности (или выработки) не подходит
        public const int RC_NOTALLDATA  = 26;           // нехватка данных

        public const int RC_OPNOTREADY  = 30;           // операция еще не готова

        public const int RC_CONTINUE    = 40;           // продолжить обработку

        // коды возврата сервера
        public const int RC_NEEDPARS    = 50;           // для печати требуется установить параметры
        public const int RC_FILLPARS    = 52;           // параметры не пересоздаются, а заполняются
        public const int RC_UNCHGPARS   = 54;           // параметры не изменялись
        public const int RC_HALFOK      = 99;           // не все в порядке, будет результат контроля

        // режимы работы с таблицей

        public const int DT_ESC = -1;       // отмена текущего

        public const int DT_SHOW = 0;
        public const int DT_ADDNEW = 1;
        public const int DT_CHANGE = 2;

        public const int DT_LOAD_DOC = 10;  // загрузка документов
        public const int DT_UPLD_DOC = 20;  // выгрузка документов

        // Режимы работы приложения
        public const int REG_DOC        = 1;    // документальный
        //public const int REG_OPR        = 2;    // операционный
        public const int REG_MARK       = 2;    // маркировка
        public const string 
            TOTAL_AVAIL                 = "*+", // разрешены все режимы
            TOTAL_RESTRICT              = "*-"; // запрещены все режимы

        // типы документов
        public const int TYPD_SAM       = 0;    // самовывоз
        public const int TYPD_CVYV      = 1;    // центровывоз
        public const int TYPD_SVOD      = 2;    // свод
        public const int TYPD_VPER      = 3;    // внутреннее перемещение
        public const int TYPD_SCHT      = 4;    // счет
        public const int TYPD_INV       = 5;    // инвентаризация
        public const int TYPD_OPR       = 6;    // операции типа приемка поддонов
        public const int TYPD_BRK       = 7;    // акт брака
        public const int TYPD_PRIH      = 8;    // приходный ордер
        public const int TYPD_ZKZ       = 9;    // заказ на комплектацию

        // типы операций
        public const int TYPOP_PRMK     = 1;    // приемка с производства
        public const int TYPOP_MARK     = 2;    // маркировка
        public const int TYPOP_OTGR     = 3;    // отгрузка
        public const int TYPOP_MOVE     = 4;    // перемещение на складе
        public const int TYPOP_DOCUM    = 5;    // работа через документы (обычный)
        public const int TYPOP_KMPL     = 6;    // комплектация
        public const int TYPOP_KMSN     = 7;    // комиссионирование
        public const int TYPOP_INVENT   = 8;    // инвентаризация по адресам

        // Функции
        public const int F_CTRLDOC      = 9;    // контроль документа
        public const int F_CHG_LIST   = 10;   // смена стиля грид
        public const int F_ADD_SCAN     = 11;   // редактирование отсканированных данных
        public const int F_MAINPAGE     = 12;   // переход на главную вкладку
        public const int F_NEXTDOC      = 22;   // следующий документ
        public const int F_PREVDOC      = 23;   // предыдущий документ
        public const int F_CHG_SORT     = 24;   // смена сортировки
        public const int F_TOT_MEST     = 25;   // всего мест
        public const int F_SAMEKMC      = 26;   // такой же код в ТТН/заявке
        public const int F_LASTHELP     = 27;   // просмотр последней инфы
        public const int F_CHGSCR       = 28;   // смена представления экрана
        public const int F_FLTVYP       = 29;   // фильтр выполненных заявок
        public const int F_EASYEDIT     = 31;   // легкий ввод
        public const int F_PODD         = 32;   // ввод поддонов
        //public const int F_PODDPLUS     = 33;   // увеличить мест_на_поддоне
        //public const int F_PODDMIN      = 34;   // увеличить мест_на_поддоне
        public const int F_ZVK2TTN      = 37;   // перенос продукции из заявки в отгруженные
        public const int F_BRAKED       = 38;   // ввод брака
        public const int F_SHLYUZ       = 39;   // ввод сведений о прибытий под загрузку
        public const int F_OPROVER      = 44;   // подтверждение завершения операции
        public const int F_LOADKPL      = 45;   // загрузка комплектации
        public const int F_SETPODD      = 46;   // установка номера/ID поддона
        public const int F_LOADOTG      = 47;   // загрузка отгрузки

        public const int F_SETADRZONE   = 48;   // установка фиксированного адреса

        public const int F_PRNDOC       = 49;   // печать документа
        public const int F_SETPRN       = 50;   // установка текущего принтера

        public const int F_KMCINF       = 51;   // информация о размещении продукции
        public const int F_CELLINF      = 52;   // информация о содержимом ячейки
        public const int F_PRNBLK       = 53;   // печать бланка
        public const int F_CONFSCAN     = 54;   // смена режима
        public const int F_EXLDPALL     = 55;   // убрать позицию из поддона
        public const int F_SETPODDCUR   = 56;   // установка номера/ID поддона для текущей позиции
        public const int F_MARKWMS      = 57;   // серверу - сведения по SSCC
        public const int F_STARTQ1ST    = 58;   // с какого количества начинать

        public const int F_A4MOVE       = 59;   // по адресу ячейки получить содержимое для перемещения
        public const int F_ADR2CNT      = 60;   // по адресу ячейки получить содержимое
        public const int F_JOINPCS      = 61;   // объединить единицы с ящиками
        public const int F_TMPMOV       = 62;   // временный выход для перемещения

        public const int F_GENFUNC      = 63;   // выбор произвольной функции
        public const int F_REFILL       = 64;   // пополнение адреса
        public const int F_GENSCAN      = 65;   // просто сканирование

        public const int F_NEWOPER      = 66;   // новая операция
        public const int F_CLRCELL      = 67;   // очистка содержимого ячейки
        public const int F_CNTSSCC      = 68;   // содержимое SSCC
        public const int F_ZZKZ1        = 69;   // загрузка одного заказа для кладовщика

        public const int F_FLTSSCC      = 70;   // фильтр по SSCC
        public const int F_SSCCSH       = 71;   // авто-содержимое SSCC
        public const int F_NEXTPL       = 72;   // перейти на следующий паллет
        public const int F_CHKSSCC      = 73;   // контроль SSCC
        public const int F_SHOWPIC      = 74;   // схема поддона
        public const int F_LOAD4CHK     = 75;   // загрузка документа для окончательного контроля
        public const int F_CHG_VIEW     = 76;   // смена стиля грид

        public const int F_VES_CONF     = 150;  // подтверждение веса
        public const int F_LOGOFF       = 200;  // выход пользователя
        public const int F_SIMSCAN        = 500;  // для отладки

        // тоже функции, но внутренние
        public const int F_INITREG = 99999; // инициализация функции/режима
        public const int F_INITRUN = 99988; // инициализация и запуск подфункции/режима
        public const int F_OVERREG = 88888; // завершение функции/режима

        // Комбинации клавиш (KeyValue из KeyEventArgs для KeyDown)
        // --- Документы

        //public const int K_ESC = 0x1B; // 27 - (Esc)
        //public const int K_ENTER = 0x0D; // 13 - (Enter)
        //public const int K_LOAD_DOC = 0x38; // 56 - (*)

        //public const int K_ADD_DOC_S = 115;          //115 - (F4)
        //public const int K_VIEW_DOC_S = 119;          //119 - (F8) - просмотр детальных
        //public const int K_ADD_DOC = 0xBB;         //187 - (+ или = (нет Shift))

        //public const int K_CHG_DOC = 0xBD;  //189 - (-)
        //public const int K_HOME = 0xC3;  //195 - (Home)
        //public const int K_QUIT = 0xC4;  //196 - (FN2-Esc)

        // Номера допустимых команд
        internal const string   COM_ZSPR    = "ZSPR";       // загрузка справочников
        internal const string   COM_VINV    = "VINV";       // выгрузка инвентаризации
        internal const string   COM_VVPER   = "VOTV";       // выгрузка внутреннее перемещение
        internal const string   COM_ZZVK    = "ZTTN";       // загрузка заявок
        internal const string   COM_VTTN    = "VTTN";       // выгрузка ТТН

        public const string     COM_ZOTG    = "ZOTG";       // загрузка сведений о прибытии/убытии
        public const string     COM_VOTG    = "VOTG";       // выгрузка сведений о прибытии/убытии

        public const string     COM_ZPRP    = "ZPRP";       // выгрузка сведений о прибытии/убытии (пропуск)
        public const string     COM_CCELL   = "CLRCELL";    // очистка ячейки
        public const string     COM_CELLI   = "CELLINF";    // информация о ячейке
        public const string     COM_KMCI    = "KMCINF";     // информация о размещении продукции
        public const string     COM_CKCELL  = "CELLCHK";    // проверка корректного размещения в ячейке
        public const string     COM_A4MOVE  = "CELLMOV";    // по адресу ячейки получить содержимое для перемещения

        public const string     COM_ADR2CNT = "CELLCTNT";   // по адресу ячейки получить содержимое

        public const string     COM_VOPR    = "VOPR";       // выгрузка операций
        public const string     COM_VMRK    = "VMRK";       // выгрузка маркировки

        internal const string   COM_ZKMPLST = "ZLSTZKZ";    // загрузка списка заказов на комплектацию для выбора
        internal const string   COM_ZKMPD   = "ZZKZ";       // загрузка заявки на комплектацию
        internal const string   COM_VKMPL   = "VZKZ";       // выгрузка заявки на комплектацию
        internal const string   COM_UNLDZKZ = "UNLDZKZ";    // отказ от резервирования заказа на комплектацию
        public const string     COM_ZSC2LST = "SSCC2LST";   // загрузка списка продукции по SSCC

        internal const string   COM_PRNDOC  = "DOCPRN";     // печать списка продукции
        internal const string   COM_GETPRN  = "GETPRN";     // получить список доступных принтеров
        public const string     COM_PRNBLK  = "BLKPRN";     // печать произвольного документа
        public const string     COM_UNKBC   = "UNKBC";      // получен неопознанный штрихкод

        public const string     COM_GENFUNC = "GENFUNC";    //  обращение к серверу произвольного документа

        internal const string   COM_CHKSCAN = "CONFSCAN";   // запрос сервера на допустимость данных
        public  const string    COM_MARKWMS = "PRMARK";     // серверу - сведения по SSCC
        public  const string    COM_REFILL  = "REFILL";     // пополнение адреса

        public const string     COM_LOGON   = "LOGON";      // запрос сервера на авторизацию

        // Терминатор для команды
        public static byte[] baTermCom = { 13, 10 };
        // Терминатор для передаваемых данных
        public static byte[] baTermMsg = { 13, 10, 0x2E, 13, 10 };

        // типы продукции
        public const int PRODTYPE_SHT = 0;                    // штучный
        public const int PRODTYPE_VES = 1;                    // весовой

        // типы ввесового товара
        //internal const int TYP_VES_UNK = 0;
        //internal const int TYP_VES_1ED = 1;
        //internal const int TYP_VES_TUP = 2;
        //internal const int TYP_VES_PAL = 3;

        // типы штрихкодов
        internal const int TYP_BC_OLD   = 1;
        internal const int TYP_BC_NEW = 2;

        internal const int TYP_BC_PALET = 11;

        [Flags]
        public enum TYP_TARA
        {
            UNKNOWN,                                    // не определена
            TARA_POTREB,                                  // скопировано из заявки
            TARA_TRANSP,                                  // скопировано из заявки
            TARA_PODDON                                  // скопировано из заявки
        }


        // типы движения для документов
        public enum MOVTYPE : int
        {
            PRIHOD = 1,        // Приход
            RASHOD = 2,        // Расход 
            AVAIL = 3,        // Остаток
            MOVEMENT = 4         // внутреннее
        }


        //public enum OPR_STATE : int
        //{
        //    OPR_EMPTY   = 0,                                    // операция еще не начиналась
        //    OPR_START   = 1,                                    // операция начата
        //    OPR_OVER    = 2,                                    // операция окончена
        //    OPR_UPL     = 3                                     // операция выгружена
        //}


        [Flags]
        public enum OPR_STATE : int
        {
            OPR_EMPTY       = 0,                            // операция еще не начиналась
            OPR_SRC_SET     = 1,                            // источник установлен
            OPR_DST_SET     = 2,                            // приемник установлен
            OPR_SRV_SET     = 4,                            // адрес с сервера установлен
            OPR_OBJ_SET     = 8,                            // операция начата
            OPR_READY       = 16,                           // операция окончена
            OPR_TRANSFERED  = 32,                           // операция выгружена
            OPR_EDITING     = 64                            // операция редактируется
        }

        // Режимы переключения таблиц
        public enum REG_SWITCH : int
        {
            SW_NEXT     = 0,                                // следующий по порядку
            SW_CLEAR    = 1,                                // принудительно сброс
            SW_SET      = 2                                 // принудительно установка
        }

        // тип объекта в детальной строке
        [Flags]
        public enum OBJ_IN_DROW : int
        {
            OBJ_NONE,                                    // не определена
            OBJ_EAN,                                    // EAN
            OBJ_SSCCINT,                                    //
            OBJ_SSCC                                    // 
        }

        // коды возврата при контроле значений полей
        public enum VALNSI : int
        {
            NO_NSI          = 0,
            ANY_AVAIL       = 1,
            EMPTY_NOT_AVAIL = 11,
            UNKNOWN_CODE    = 21
        }

        //// Режимы установки флага стрейчевания
        //public enum WRP_MODES : int
        //{
        //    WRP_BY_NSI      = 1,                            // по справочнику
        //    WRP_ASK_EVERY   = 2,                            // всегда спрашивать
        //    WRP_ALW_SET     = 4,                            // всегда установлен
        //    WRP_ALW_RESET   = 8                             // всегда сброшен
        //}

        // Режимы установки флага стрейчевания
        public class WRAP_MODES
        {
            public const int
            WRP_BY_NSI = 1,                            // по справочнику
            WRP_ASK_EVERY = 2,                            // всегда спрашивать
            WRP_ALW_SET = 4,                            // всегда установлен
            WRP_ALW_RESET = 8;                             // всегда сброшен

            private int
                m_Cur = WRP_BY_NSI;

            // Текущий тип документа
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
                        s = "По справочнику";
                        break;
                    case WRP_ASK_EVERY:
                        s = "По запросу";
                        break;
                    case WRP_ALW_SET:
                        s = "Застрейчеван";
                        break;
                    case WRP_ALW_RESET:
                        s = "Без стрейча";
                        break;
                    default:
                        s = "Неизвестно";
                        break;
                }
                return s;
            }
        }



        // способ завершения ввода операции
        public const int OPOV_SCPROD = 1;                    // сканирование продукции
        public const int OPOV_SCADR2 = 2;                    // сканирование получателя


        public const int KRKMC_MIX   = 69;                    // краткий код для сборной паллетты

        // 
        internal const string 
                sIDTmp = "TMPOPR";

        // Имена функций интерпретатора
        internal const string DOC_CONTROL = "ControlDoc";
        /// Контроль указанного документа
        /// object xRet = xDocControl.run.ExecFunc
        /// параметры: DOC_CONTROL, 
        /// new object[] { dr, childRowsZVK, childRowsTTN, lstStr }, actDocControl);
        /// nRet = (int)xRet;
        internal const string SCAN_OVER = "ScanOver";
        /// После получения результатов сканирования
        /// object xRet = xDocControl.run.ExecFunc(DOC_CONTROL, new object[] { dr, childRowsZVK, childRowsTTN, 
        /// параметры: SCAN_OVER
        ///                                        lstStr }, actDocControl);
        /// nRet = (int)xRet;
        /// 
        internal const string 
            FEXT_ADR_NAME       = "NameAdr",                // Визуальное представление адреса
            FEXT_CONF_SCAN      = "ConfScan";               // Подтверждение скана на сервере

        // режимы фиксации
        public const int FX_PRPSK = 1;         // по пропуску
        public const int FX_PTLST = 2;         // по путевому листу

        // режим работы формы обмена с сервером
        public const int R_BLANK = 1;           // выбор бланка
        public const int R_PARS = 2;            // установка параметров от сервера

        // описание типов документов
        public static Dictionary<int, SkladGP.DocTypeInf>
                        xDocTInf;

    }
}

namespace SkladGP
{
    
    public sealed class AppPars
    {
        public static int MAXDocType = 9;

        // имеются штучный и весовой
        public static int MAXProductsType = 2;
        public static int MAXFields = 7;

        public struct ParsForMType
        {
            public bool bMestConfirm;
            public bool bMAX_Kol_EQ_Poddon;
            public int nDefEmkVar;
            public bool b1stPoddon;
        }

        // Параметры для типа продукции (весовой или штучный)
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

        // Параметры для типа документа
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

        // Параметры для операции
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
        // Путь к резервной копии
        private string m_AppStore;
        // Путь к НСИ
        private string m_NSIPath;
        // Путь к данным
        private string m_DataPath;

        // HOST-m_Name сервера
        private string m_Host;
        // № порта сервера (обмен данными)
        private int m_SrvPort;
        // № порта сервера (обмен сообщениями)
        private int m_SrvPortM;
        // NTP-сервер
        private string m_NTP;

        // Вкл/выкл обмен сообщениями с сервером
        private bool m_WaitSock;
        // Автосохранение
        private bool m_AutoSave = false;
        // Группа серверов
        private bool m_UseSrvG = false;

        private string
            m_AppAvailModes = "1+";

        //-----*****-----*****-----
        // Текущее поле (индекс)
        private int m_CurField;
        // Текущий тип материала (индекс)
        private int m_CurVesType;
        // Запрос на завершение ввода для нового сканирования
        private bool m_WarnNewScan = false;

        
        private int
            m_CurDocType,                   // Текущий тип документа
            m_Days2Save,                    // Дней хранения документов
            m_DebugLevel = 0,               // Уровень отладки
            m_ReLogon;                      // Таймаут повторного логона (минут)

        
        private bool
            m_HidUpl;                       // Скрывать выгруженные документы

        //===***===
        private bool 
            m_OpAutoUpl = true,                     // Авто-выгрузка для операций
            m_UseFixAddr = false,                   // Использовать фиксированные адреса
            m_OpChkAdr = true,                      // Проверка ячейки для операции
            m_UseAdr4DocMode = false,               // Использование адресов в документальном режиме

            m_ConfScan = true,                      // Запрашивать сервер после сканирования
            m_Ask4biddScan = true,                  // Реакция на запрещенное сканирование
            m_BadPartyForbidd = true,               // Строгий запрет на плохие партии/даты

            m_SendTG2WMS = true,                    // птичка при маркировке "Приход"
            m_CanEditIDNum = false;                 // Ввод № ID-точки ручками

        //private AppC.WRP_MODES
        //    m_WrapMode = AppC.WRP_MODES.WRP_BY_NSI; // значение при маркировке "Застрейчеван"

        private AppC.WRAP_MODES
            m_WrapMode = new AppC.WRAP_MODES();    // значение при маркировке "Застрейчеван"

        // Способ завершения операций
        private int 
            m_OpOver = AppC.OPOV_SCPROD;


        // Клавиши управления курсором только с Shift
        public static bool 
            bArrowsWithShift = true;



        /// Панель ввода
        /// 

        #region Будут установлены при каждом запуске
        
        public static bool
            bVesNeedConfirm = true,                         // Подтверждение мест для весового товара
            ShowSSCC = true,
            bUseHours = false;                              // использовать часы в сроках реализации

        #endregion

        // Добавление записи для весового товара
        //public bool parVvodVESNewRec = true;

        // Добавление записи для штучного товара
        //public bool parVvodSHTNewRec = false;

        // Отображение кодов конкретных партий в заявке
        public bool parVvodShowExact = true;

        /// Панель работы с документами
        // Контроль документов при перемещении в грид
        //public bool parDocControl = false;



        // таблица с параметрами
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

            // Параметры по типам материалов
            //--- штучный
            aParsTypes[AppC.PRODTYPE_SHT].bMestConfirm = true;
            aParsTypes[AppC.PRODTYPE_SHT].bMAX_Kol_EQ_Poddon = true;
            aParsTypes[AppC.PRODTYPE_SHT].nDefEmkVar = 0;
            aParsTypes[AppC.PRODTYPE_SHT].b1stPoddon = false;

            aParsTypes[AppC.PRODTYPE_VES].bMestConfirm = true;
            aParsTypes[AppC.PRODTYPE_VES].bMAX_Kol_EQ_Poddon = true;
            aParsTypes[AppC.PRODTYPE_VES].nDefEmkVar = 20;
            aParsTypes[AppC.PRODTYPE_VES].b1stPoddon = false;

            m_WarnNewScan = true;

            // Параметры по типам документов
            SetArrDoc(ref this.aDocPars);

            m_Days2Save = 100;
            ReLogon = -1;
            m_HidUpl = false;
            OpAutoUpl = true;
        }

        //---***---***---
        #region Общие параметры

        // Путь к резервной копии
        public string sAppStore
        {
            get { return m_AppStore; }
            set { m_AppStore = value; }
        }
        // Путь к НСИ
        public string sNSIPath
        {
            get { return m_NSIPath; }
            set { m_NSIPath = value; }
        }
        // Путь к данным
        public string sDataPath
        {
            get { return m_DataPath; }
            set { m_DataPath = value; }
        }

        // HOST-m_Name сервера
        public string sHostSrv
        {
            get { return m_Host; }
            set { m_Host = value; }
        }
        // № порта сервера (обмен данными)
        public int nSrvPort
        {
            get { return m_SrvPort; }
            set { m_SrvPort = value; }
        }

        // NTP-сервер
        public string NTPSrv
        {
            get { return m_NTP; }
            set { m_NTP = value; }
        }


        // № порта сервера (обмен сообщениями)
        public int nSrvPortM
        {
            get { return m_SrvPortM; }
            set { m_SrvPortM = value; }
        }
        // Вкл/выкл обмен сообщениями с сервером
        public bool bWaitSock
        {
            get { return m_WaitSock; }
            set { m_WaitSock = value; }
        }
        // Автосохранение
        public bool bAutoSave
        {
            get { return m_AutoSave; }
            set { m_AutoSave = value; }
        }
        // Группа серверов
        public bool bUseSrvG
        {
            get { return m_UseSrvG; }
            set { m_UseSrvG = value; }
        }

        // Допустимые режимы работы приложения
        public string AppAvailModes
        {
            get { return m_AppAvailModes; }
            set { m_AppAvailModes = value; }
        }


        // Получить по адресу его содержимое с сервера
        public bool GetAdrContentFromSrv
        {
            get { return m_UseAdr4DocMode; }
            set { m_UseAdr4DocMode = value; }
        }

        // Использование адресов в документальном режиме
        public bool UseAdr4DocMode 
        {
            get { return m_UseAdr4DocMode ; }
            set { m_UseAdr4DocMode = value; }
        }

        // Использование адресов в документальном режиме
        public int DebugLevel
        {
            get { return m_DebugLevel; }
            set { m_DebugLevel = value; }
        }


        #endregion
        //-----*****-----*****-----
        #region Параметры ввода

        // Параметы ввода данных для типов материала
        public ParsForMType[] aParsTypes = new ParsForMType[MAXProductsType];

        // Параметы ввода данных для полей
        public FieldDef[] aFields = new FieldDef[MAXFields];

        // Текущее поле
        public int CurField
        {
            get { return m_CurField; }
            set { m_CurField = value; }
        }
        // Текущее тип
        public int CurVesType
        {
            get { return m_CurVesType; }
            set { m_CurVesType = value; }
        }

        // Ввод материала новой строкой (режим с добавлением)
        //public bool bAddNewRow
        //{
        //    get { return aParsTypes[CurVesType].bAddNewRow; }
        //    set { aParsTypes[CurVesType].bAddNewRow = value; }
        //}

        // Подтверждение мест при вводе
        public bool bConfMest
        {
            get { return aParsTypes[CurVesType].bMestConfirm; }
            set { aParsTypes[CurVesType].bMestConfirm = value; }
        }
        // Максимальное количество - поддон
        public bool bMaxKolEQPodd
        {
            get { return aParsTypes[CurVesType].bMAX_Kol_EQ_Poddon; }
            set { aParsTypes[CurVesType].bMAX_Kol_EQ_Poddon = value; }
        }

        // Процент отклонения веса одного места
        public int MaxVesVar
        {
            get { return aParsTypes[CurVesType].nDefEmkVar; }
            set { aParsTypes[CurVesType].nDefEmkVar = value; }
        }

        // С какого количества начинать (false - остаток от поддона, true - целого поддона)
        public bool bStart1stPoddon
        {
            get { return aParsTypes[CurVesType].b1stPoddon; }
            set { aParsTypes[CurVesType].b1stPoddon = value; }
        }


        // доступность поля после сканирования
        public bool bAfterScan
        {
            get { return aFields[CurField].aVes[CurVesType].bScan; }
            set { aFields[CurField].aVes[CurVesType].bScan = value; }
        }
        // доступность поля для редактирования
        public bool bEdit
        {
            get { return aFields[CurField].aVes[CurVesType].bEdit; }
            set { aFields[CurField].aVes[CurVesType].bEdit = value; }
        }
        // доступность поля для ввода
        public bool bManual
        {
            get { return aFields[CurField].aVes[CurVesType].bVvod; }
            set { aFields[CurField].aVes[CurVesType].bVvod = value; }
        }
        // Запрос на завершение ввода для нового сканирования
        public bool WarnNewScan
        {
            get { return m_WarnNewScan; }
            set { m_WarnNewScan = value; }
        }

        // Реакция на запрещенное сканирование
        public bool Ask4biddScan 
        {
            get { return m_Ask4biddScan; }
            set { m_Ask4biddScan = value; }
        }

        // Строгий запрет на плохие партии/даты
        public bool BadPartyForbidd
        {
            get { return m_BadPartyForbidd; }
            set { m_BadPartyForbidd = value; }
        }


        #endregion

        #region Параметры документов

        // Параметы для типов документа
        //public ParsForDoc[] aDocPars = new ParsForDoc[MAXDocType + 1];
        public ParsForDoc[] aDocPars = new ParsForDoc[0];

        // Текущий тип документа
        public int CurDocType
        {
            get { return m_CurDocType; }
            set { m_CurDocType = value; }
        }

        // Количество по умолчанию из заявки
        public bool bKolFromZvk
        {
            get { return aDocPars[CurDocType].bShowFromZ; }
            set { aDocPars[CurDocType].bShowFromZ = value; }
        }

        // Контроль документа перед выгрузкой
        public bool bTestBeforeUpload
        {
            get { return aDocPars[CurDocType].bTestBefUpload; }
            set { aDocPars[CurDocType].bTestBefUpload = value; }
        }

        // Суммировать весовую продукцию
        public bool bSumVesProd
        {
            get { return aDocPars[CurDocType].bSumVes; }
            set { aDocPars[CurDocType].bSumVes = value; }
        }

        // Дней хранения документа
        public int Days2Save
        {
            get { return m_Days2Save; }
            set { m_Days2Save = value; }
        }

        // Таймаут повторного логона (минут)
        public int ReLogon
        {
            get { return m_ReLogon; }
            set { m_ReLogon = value; }
        }

        // Скрывать выгруженные документы
        public bool bHideUploaded
        {
            get { return m_HidUpl; }
            set { m_HidUpl = value; }
        }

        // Запрашивать сервер после сканирования
        public bool ConfScan
        {
            get { return m_ConfScan; }
            set { m_ConfScan = value; }
        }

        // Ввод № ID-точки ручками
        public bool CanEditIDNum 
        {
            get { return m_CanEditIDNum ; }
            set { m_CanEditIDNum = value; }
        }

        // птичка при маркировке "Приход"
        public bool SendTG2WMS
        {
            get { return m_SendTG2WMS ; }
            set { m_SendTG2WMS = value; }
        }

        // птичка при маркировке "Застрейчеван"
        //public AppC.WRP_MODES WrapMode
        //{
        //    get { return m_WrapMode; }
        //    set { m_WrapMode = value; }
        //}

        // птичка при маркировке "Застрейчеван"
        public AppC.WRAP_MODES WrapMode
        {
            get { return m_WrapMode; }
            set { m_WrapMode = value; }
        }


        #endregion


        #region Параметры операционного режима
        //===***===
        // Авто-выгрузка для операций
        public bool OpAutoUpl
        {
            get { return m_OpAutoUpl; }
            set { m_OpAutoUpl = value; }
        }

        // Способ завершения операций
        public int OpOver
        {
            get { return m_OpOver; }
            set { m_OpOver = value; }
        }

        // Проверка ячейки для операции
        public bool OpChkAdr
        {
            get { return m_OpChkAdr; }
            set { m_OpChkAdr = value; }
        }

        // Использовать фиксированные адреса
        public bool UseFixAddr 
        {
            get { return m_UseFixAddr ; }
            set { m_UseFixAddr = value; }
        }

        #endregion



        #region Параметры серверов

        // Параметы для типов документа
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
                // Параметры по типам документов
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
                // приходный ордер
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
            {// прочитали с диска
                xNew = (AppPars)xx;
                bNeedSave = SetArrDoc(ref xNew.aDocPars);
                // отклонение от емкости по умолчанию для весовых мест
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
                xNew.aSrvG[0].sSrvComment = "Озерцо";
                xNew.aSrvG[0].sSrvHost = "TRESERV";
                xNew.aSrvG[0].nPort = 11010;
                xNew.aSrvG[0].bActive = true;
                xNew.aSrvG[0].ConType = WiFiStat.CONN_TYPE.ACTIVESYNC;

                xNew.aSrvG[1].sSrvComment = "Филиал";
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

    /// набор показателей (с наименованием)
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

    /// объект сканирования-ввода-заявки
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

    // типы адресов

    public class Smena
    {
        public struct ObjInf
        {
            public string ObjName;
        }

        // минимальный таймаут бездействия для релогона
        public static int MIN_TIMEOUT = 2;

        //public static Dictionary<string, ExprAct> xDD = null;


        private string m_OldUser;                       // имя предыдущего пользователя
        //private int m_Sklad;                          // код склада
        //private int m_Uch;                            // код участка


        private string m_SDate = "";                        // дата по умолчанию
        // дата документов
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

        public static DateTime DateDef;                 // дата по умолчанию
        public static int SklDef = 0;                   // код склада
        public static int UchDef = 0;                   // код участка

        public static string 
            EnterPointID = "",
            SmenaDef = "";                              // код смены

        //public static int 
        //    TypDef = AppC.TYPD_SVOD;                    // тип документа


        private static string sXML = "CS.XML";          // имя файла с настройками пользователь/смена

        private DataRow
            m_DocBefTmpMove = null;

        private int
            m_DocType = AppC.TYPD_VPER,                 // тип документа
            m_RegApp = AppC.REG_DOC,                    // режим работы  по умолчанию - с документами
            m_CurrNum4Invent = 0;                       // текущий номер для получения поддона

        // список участков для комплектации
        private List<int> 
            aUch = null;

        // текущий принтер
        //private int m_CurPrn = -1;

        // список принтеров
        //public ObjInf[] aPrn = null;

       
        private string
            //m_DevID = "",
            m_MAC = "000000000000",                     // MAC-адрес
            m_LstUch = "",                              // список участков для комплектации
            //m_FilterTTN = "",                           // текущий фильтр для
            m_CurPrnMOB = "",                           // текущий МОБИЛЬНЫЙ принтер
            m_CurPrnSTC = "";                           // текущий СТАЦИОНАРНЫЙ принтер

            
        public static BindingList<StrAndInt> 
            bl;

        // права пользователей
        public enum USERRIGHTS : int
        {
            USER_KLAD       = 1,                        // кладовщик
            USER_BOSS_SMENA = 10,                       // начальник смены
            USER_BOSS_SKLAD = 100,                      // начальник склада
            USER_ADMIN      = 1000,                     // начальник смены
            USER_SUPER      = 2000                      // 
        }

        // параметры текущей сесси пользователя
        public string sUser = "";                       // код пользователя
        public string sUName = "";                      // ФИО
        public string sUserPass = "";
        public string sUserTabNom = "";
        public USERRIGHTS urCur;                        // текущие права    


        // Время (в минутах) до конца смены (вернет сервер)
        public TimeSpan tMinutes2SmEnd = TimeSpan.FromMinutes(0);
        //public DateTime dtSmEnd;

        // таймер на конец смены для пользователя
        public Timer xtmSmEnd = null;

        // таймер на простой терминала внутри смены
        public Timer xtmTOut = null;

        // длительность таймаута в секундах
        public int nMSecondsTOut = 0;



        // дата-время последней загрузки всех справочников
        //public DateTime dtLoadNS;

        public DateTime dBeg;                           // начало смены
        public DateTime dEnd;                           // окончание смены
        public int nLogins;

        public bool bInLoadUpLoad = false;

        public int nDocs = -1;

        // код склада
        public int nSklad
        {
            get { return SklDef; }
            set { SklDef = value; }
        }

        // код участка
        public int nUch
        {
            get { return UchDef; }
            set
            {
                UchDef = value;
                Uch2Lst(value, true);
            }
        }

        // код смены
        public string DocSmena
        {
            get { return SmenaDef; }
            set { SmenaDef = value; }
        }

        // код типа документа
        public int DocType
        {
            get { return m_DocType; }
            set { m_DocType = value; }
        }


        // режим работы приложения
        public int RegApp
        {
            get { return m_RegApp; }
            set { m_RegApp = value; }
        }

        // список участков для комплектации
        public string LstUchKompl
        {
            get { return m_LstUch; }
            set { m_LstUch = value; }
        }

        public MainF.AddrInfo 
            xAdrForSpec = null,
            xAdrFix1 = null;


        // текущий принтер
        //public int CurPrinter
        //{
        //    get { return m_CurPrn; }
        //    set { m_CurPrn = value; }
        //}

        // имя текущего принтера
        //public string CurPrinterName
        //{
        //    get { return ((m_CurPrn >= 0)?aPrn[m_CurPrn].ObjName : ""); }
        //}

        // имя текущего (МОБИЛЬНОГО) принтера
        public string CurPrinterMOBName
        {
            get { return m_CurPrnMOB; }
            set { m_CurPrnMOB = value; }
        }

        // имя текущего (СТАЦИОНАРНОГО) принтера
        public string CurPrinterSTCName
        {
            get { return m_CurPrnSTC; }
            set { m_CurPrnSTC = value; }
        }


        // код склада
        public int Curr4Invent
        {
            get { return m_CurrNum4Invent; }
            set { m_CurrNum4Invent = value; }
        }

        // MAC-адрес
        public string MACAdr
        {
            get { return m_MAC; }
            set { m_MAC = value; }
        }

        // адрес документа, из которого вызвали временное перемещение
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
            bl.Add(new StrAndInt("Документальный", AppC.REG_DOC));
            //bl.Add(new StrAndInt("Операционный", AppC.REG_OPR));
            bl.Add(new StrAndInt("Маркировка", AppC.REG_MARK));

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

        // состояние
        public NSI.FILTRDET 
            FilterTTN = NSI.FILTRDET.UNFILTERED,
            FilterZVK = NSI.FILTRDET.UNFILTERED;

    }

    // описание типа документа
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

        // имя типа
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        // цифровой код типа (для ввода)
        public int NumCode
        {
            get { return m_NumCode; }
            set { m_NumCode = value; }
        }

        // тип движения по складу
        public AppC.MOVTYPE MoveType
        {
            get { return m_MoveType; }
            set { m_MoveType = value; }
        }

        // обязательность адреса-источника
        public bool AdrFromNeed
        {
            get { return m_AdrFromNeed; }
            set { m_AdrFromNeed = value; }
        }

        // обязательность адреса-приемника
        public bool AdrToNeed
        {
            get { return m_AdrToNeed; }
            set { m_AdrToNeed = value; }
        }

        // получать содержимое из источника
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

        public int nSklad;              // код склада
        public string sSklad;           // наименование склада
        public int nUch;                // код участка
        public DateTime dDatDoc;        // дата документа
        public string sSmena;           // код смены
        //public int nTypD;               // тип документа (код)
        public string sTypD;            // тип документа (наименование)
        public string sNomDoc;          // № документа
        public int nEks;                // код экспедитора
        public string sEks;             // ФИО экспедитора
        public int nPol;                // код получателя 
        public string sPol;             // наименование получателя 

        public long lSysN;              // № документа

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
        // тип документа (код)
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

        // тип операции документа
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
            string s = "Неизвестно";
            switch (nOpr)
            {
                case AppC.TYPOP_PRMK:
                    s = "Прием с производства";
                    break;
                case AppC.TYPOP_MARK:
                    s = "Маркировка";
                    break;
                case AppC.TYPOP_KMPL:
                    s = "Комплектация";
                    break;
                case AppC.TYPOP_OTGR:
                    s = "Отгрузка";
                    break;
                case AppC.TYPOP_MOVE:
                    s = "Перемещение на складе";
                    break;
                case AppC.TYPOP_KMSN:
                    s = "Комиссионирование";
                    break;
                case AppC.TYPOP_INVENT:
                    s = "Инвентаризация";
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

        // попытка перехода на следующее
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

        // диапазон номеров
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
            m_xAdrSrc = null,                           // адрес-источник
            m_xAdrDst_Srv = null,                       // адрес с сервера
            m_xAdrDst = null;                           // адрес-приемник

        private DataRow
            m_drObj = null;                             // объект операции

        private string
            m_SSCC_Src = "",
            m_SSCC_Dst = "";


        // корректировка статуса операции после изменения
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
                    case AppC.TYPD_PRIH:       // документы поступления
                        if ((nOperState & AppC.OPR_STATE.OPR_DST_SET) == AppC.OPR_STATE.OPR_DST_SET)
                            bRet = true;
                        break;
                    case AppC.TYPD_OPR:             // документы перемещения
                        if (IsFillSrc() && IsFillDst())
                            bRet = true;
                        break;
                    default:
                        // для расходных и инвентаризаций нужен источник
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
            xMF = null;                                 // для отображения на форме

        //public bool
        //    bObjOperScanned = false;                    // флаг установки объекта операции

        public AppC.OPR_STATE
            nOperState = AppC.OPR_STATE.OPR_EMPTY;      // текущий статус операции

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
            {// адреса установлены ?
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


        // Адрес-источник, участвующий в операции
        public MainF.AddrInfo xAdrSrc
        {
            get { return m_xAdrSrc; }
        }

        // Установка адреса и попутные вычисления
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

        // Адрес-приемник, участвующий в операции
        public MainF.AddrInfo xAdrDst
        {
            get { return m_xAdrDst; }
        }

        // Установка адреса-приемника и попутные вычисления
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

        // Адрес, рекомендуемый сервером (на статус не влияет)
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

        // Объект (продукция), участвующий в операции
        public DataRow OperObj
        {
            get { return m_drObj; }
        }

        // Установка адреса-приемника и попутные вычисления
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

        // SSCC, участвующий в операции
        public string SSCC
        {
            get { return m_SSCC_Src; }
            set { m_SSCC_Src = value; }
        }

        // SSCC-назначения, участвующий в операции
        public string SSCC_Dst
        {
            get { return m_SSCC_Dst; }
            set { m_SSCC_Dst = value; }
        }



    }


    // текущий документ
    public class CurDoc
    {
        // ID загруженного документа в системе учета
        private string
            m_ID_Load = "";

        public int 
            nId = AppC.EMPTY_INT,                       // код документа
            //nTypOp = AppC.TYPOP_PRMK,                   // тип операции
            nStrokZ,                                    // строк в заявке
            nStrokV,                                    // строк введено
            nDocSrc;                                    // происхождение документа (загружен или введен)

        public string 
            sSSCC,                                      // текущий SSCC поддона
            sLstUchNoms = "";                           // список номеров участков

        public bool 
            bSpecCond,                                  // особые условия для детальных строк
            bEasyEdit = false,
            bTmpEdit = false,
            bConfScan = false,                          // флаг проверки на сервере отсканированных/введенных данных
            bFreeKMPL = false;                          // флаг свободной комплектации

        public DataRow
            drCurSSCC = null,
            drCurRow = null;                            // текущая строка в таблице Документов

        public DocPars 
            xDocP;                                      // параметры документа (тип, склад,...)

        public PoddonList 
            xNPs;                                       //  список номеров поддонов

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


        // ID загруженного документа в системе учета
        public string ID_DocLoad
        {
            get { return m_ID_Load; }
            set { m_ID_Load = value; }
        }


        public void InitNew()
        {
        
            sLstUchNoms = "";                 // список номеров участков
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

    // текущая загрузка
    public class CurLoad
    {
        public IntRegsAvail ilLoad; //режим загрузки

        public bool
            CheckIt = false;

        // команда загрузки
        public int nCommand = 0;

        // параметры фильтра
        public DocPars xLP;


        // результат загрузки
        public DataSet dsZ;

        // результат загрузки (таблица со структурой BD_DOUTD)
        public DataTable dtZ = null;

        public string 
            sComLoad,               // символьная команда для сервера
            sFileFromSrv = "",      // имя временного файла, полученного с сервера
            sSSCC="",               // параметры фильтра загрузки
            sFilt;                  // символьное выражение фильтра

        public DataRow 
            dr1st = null,           // строка с 1-м загруженным документом
            drPars4Load = null;     // строка с параметрами для загрузки

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

    // доступные значения режимов
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
            lRegs.Add(new RegAttr(AppC.UPL_CUR, "Текущий", true));
            lRegs.Add(new RegAttr(AppC.UPL_ALL, "Все", false));
            lRegs.Add(new RegAttr(AppC.UPL_FLT, "По фильтру", false));

            nI = 0;
            CurReg = nSetCur;
        }

        // поиск по заданному значению
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

        // Текущий режим
        public int CurReg {
            get { return (lRegs[nI].RegValue); }
            set
            {
                int nK = FindByVal(value);
                if (nK >= 0)
                    nI = nK;
            }
        }

        // Наименование текущего режима
        public string CurRegName
        {
            get { return (lRegs[nI].RegName); }
        }

        // установить доступность текущего режима
        public bool CurRegAvail
        {
            get { return (lRegs[nI].bRegAvail); }
            set { 
                RegAttr ra = lRegs[nI];
                ra.bRegAvail = value;
                lRegs[nI] = ra;
            }
        }

        // установить следующий/предыдущий доступные режимы
        public string NextReg(bool bUp)
        {
            int nK;

            if (bUp == true)
            {// выбор следующего
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

        // флаг доступности для всех
        public void SetAllAvail(bool bFlag)
        {
            for (int i = 0; i < lRegs.Count; i++ )
            {
                RegAttr ra = lRegs[i];
                ra.bRegAvail = bFlag;
                lRegs[i] = ra;
            }
        }

        // Установить доступность конкретному
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


    // список серверов
    public class GroupServers
    {

        private AppPars xPApp;

        // индекс сервера в списке
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
                        lSrvG.Add("Все");
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

    // текущая выгрузка
    public class CurUpLoad
    {
        //режим выгрузки
        public IntRegsAvail ilUpLoad;

        // индекс сервера в списке
        public int nSrvGind;

        private List<string> lSrvG;
        private AppPars xParsApp;


        // параметры фильтра
        public DocPars xLP;

        public List<int> naComms;

        // текущая команда выгрузки
        public string sCurUplCommand = "";

        // выгрузка только текущей строки (для операций)
        public bool bOnlyCurRow = false;

        public DataRow drForUpl = null;

        // дополнительный объект выгрузки (параметры печати)
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
                        lSrvG.Add("Все");
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
            // результаты сканирования
            public ScannerAll.BCId ci;      // тип штрих-кода
            public string s;                // штрих-код

            // выдрали из штрих-кода
            //public int nParty;              // партия
            public string 
                nParty;                     // партия

            public string sDataIzg;         // дата изготовления (символьно)
            public DateTime 
                dDataGodn,                  // дата годности
                dDataIzg;                   // дата изготовления
            public FRACT fEmk;              // емкость в штуках (для штучного) или 
                                            // вес упаковки (для весового); 0 - единичный товар

            public string
                nTara;                      // код тары(C(10))
            public int
                nKolSht,                    // количество в штуках (для весового)
                nMestPal;                   // количество мест на палетте

            public int
                nKolG,                      // количество голов
                nMest;                      // количество мест

            public FRACT 
                fVes,                       // вес
                fVsego;                     // всего штук /вес

            //public int 
            //    nTypVes;             // тип весового (TYP_VES_TUP,...)

            public int 
                nNPredMT;                   // № предъявления для материала


            // будет нужно -???
            //public int nKolSht;             // количество (штуки)
            //public float nKolVes;           // количество (вес)
            // будет нужно -???

            public bool bFindNSI;           // удалось найти в НСИ

            //--- накопленные данные
            public FRACT fKolE_alr;         // уже введено единиц данного кода (мест = 0)
            public int nKolM_alr;           // уже введено мест данного кода
            public FRACT fMKol_alr;         // уже введено количество продукции (мест != 0)
            //--- накопленные данные (точное совпадение)
            public FRACT fKolE_alrT;        // уже введено единиц данного кода (мест = 0)
            public int
                nMAlr_NPP,                  // уже введено мест данного кода (при комплектации)
                nKolM_alrT;                 // уже введено мест данного кода
            public FRACT
                fVAlr_NPP,                  // уже введено единиц данного кода (при комплектации)
                fMKol_alrT;        // уже введено количество продукции (мест != 0)

            //--- заявка - накопленные данные
            public FRACT fKolE_zvk;         // отдельных единиц данного кода всего
            public int nKolM_zvk;           // мест данного кода и емкости по заявке всего

            // адреса (ТТН)
            public System.Data.DataRow drEd;            // куда суммировать единицы в ТТН
            public System.Data.DataRow drMest;          // куда суммировать места в ТТН

            // строки из заявки
            public System.Data.DataRow drTotKey;        // заявка на места с конкретной партией
            public System.Data.DataRow drPartKey;       // заявка на места с любой партией
            public System.Data.DataRow drTotKeyE;       // заявка на единички с конкретной партией
            public System.Data.DataRow drPartKeyE;      // заявка на единички с любой партией

            public System.Data.DataRow
                //drSEMK,                                 // строка в справочнике емкостей
                drMC;                                   // строка в справочнике матценностей
            // из справочника матценностей
            public string sKMC;             // полный код
            public int nKrKMC;              // краткий код
            public string sN;               // наименование
            public int nSrok;               // срок реализации (часы)

            public bool 
                bEmkByITF,
                bVes;               // признак весового

            public string
                sSSCC,
                sSSCCInt,
                sGTIN,
                sEAN,                       // EAN-код продукции
            
            sGrK;             // групповой код продукции
            public FRACT fEmk_s;            // для восстановления емкости при переключениях мест=0
            //public int EmkPod;

            // назначение строки
            public NSI.DESTINPROD nDest;                // что из заявки закрывается (общая или точная часть)
            // происхождение строки
            public int nRecSrc;
            public DateTime dtScan;

            // результат контроля по данному коду-емкости
            public int nDocCtrlResult;

            // флаг внутреннего кода получателя
            public bool bAlienMC;
            public bool bNewAlienPInf;
            public string sIntKod;

            public int nNomPodd;
            public int nNomMesta;
            public AppC.TYP_TARA tTyp;

            // сообщение об ошибке при сканировании
            public string sErr;

            // выражение фильтра для подсчета ZVK/TTN
            public string sFilt4View;

            // список строк заявки, которые потенциально закрываются текущим сканированием
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
                ci = e.nID;                         // тип штрих-кода
                s = e.Data;                         // штрих-код

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

                fKolE_zvk = 0;       // единиц данного кода всго
                nKolM_zvk = 0;       // мест данного кода  по заявке

                drTotKey = null;     // заявка на места с конкретной партией
                drPartKey = null;    // заявка на места с любой партией
                drTotKeyE = null;    // заявка на единички с конкретной партией
                drPartKeyE = null;   // заявка на единички с любой партией

                sKMC = "";
                nKrKMC = AppC.EMPTY_INT;
                sN = "<Неизвестно>";
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
                nMAlr_NPP = 0;                  // уже введено мест данного кода (при комплектации)
                fVAlr_NPP = 0;                  // уже введено единиц данного кода (при комплектации)

                xEmks = new Srv.Collect4Show<StrAndInt>(new StrAndInt[0]);
            }

            // обнуление расчетных полей для заявки
            public void ZeroZEvals()
            {
                fKolE_zvk = 0;       // единиц данного кода всго
                nKolM_zvk = 0;       // мест данного кода  по заявке

                drTotKey = null;     // заявка на места с конкретной партией
                drPartKey = null;    // заявка на места с любой партией
                drTotKeyE = null;    // заявка на единички с конкретной партией
                drPartKeyE = null;   // заявка на единички с любой партией

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
            //    {// будем по краткому
            //        if (((nKrKMC >= 1) && (nKrKMC <= 8)) ||
            //              (nKrKMC == 46) ||
            //              (nKrKMC == 43) ||
            //              (nKrKMC == 41))
            //        {
            //            ret = true;
            //        }
            //    }
            //    else
            //    {// будем по EAN
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

            // получить данные из справочника по EAN или коду
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
            //                sDataIzg += dReal.ToString("HH").Substring(0, 2) + "ч ";
            //            sDataIzg += dReal.ToString("dd.MM");
            //        }

            //        // поиск емкости по коду продукции и возможно считанному весу
            //        if (drSEMK == null)
            //        {
            //            DataRow[] childRows = drMC.GetChildRows(dtMC.ChildRelations[NSI.REL2EMK]);
            //            if (childRows.Length == 1)
            //            {// подбирать нечего, только одна емкость
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




            // получить данные из справочника по EAN или коду
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
                    {// емкости готовятся для случаев полной информации по продукции

                        if (dDataIzg != DateTime.MinValue)
                        {
                            DateTime dReal = dDataIzg.AddHours((double)nSrok);
                            sDataIzg = dDataIzg.ToString("dd.MM.yy") + "/";
                            if (AppPars.bUseHours == true)
                                sDataIzg += dReal.ToString("HH").Substring(0, 2) + "ч ";
                            sDataIzg += dReal.ToString("dd.MM");
                        }

                        // поиск емкости по коду продукции и возможно считанному весу
                        DataRow[] draEmk = drMC.GetChildRows(dtMC.ChildRelations[NSI.REL2EMK]);
                        xEmks = new Srv.Collect4Show<StrAndInt>(GetEmk4KMC(dr, draEmk, out nDefEmk, out nFoundGTIN));
                        if (xEmks.Count > 0)
                        {
                            if (xEmks.Count == 1)
                            {// подбирать нечего, только одна емкость
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
            //                {// добавляем только несовпадающие емкости
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


            //// построить массив емкостей
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
            //                {// добавляем только несовпадающие емкости
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


            // построить массив емкостей
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
            {// будем по краткому
                if (((nKrKMC >= 1) && (nKrKMC <= 8)) ||
                      (nKrKMC == 46) ||
                      (nKrKMC == 43) ||
                      (nKrKMC == 41))
                {
                    ret = true;
                }
            }
            else
            {// будем по EAN
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
