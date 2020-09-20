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

        // таблицы для обмена
        public const string BD_ZDOC     = "BD_ZTTN";
        public const string BD_ZDET     = "BD_STTN";

        public const string BD_KMPL     = "BD_KMPL";

        // связи между таблицами
        public const string REL2TTN     = "DOC2TTN";
        public const string REL2ZVK     = "DOC2ZVK";
        public const string REL2PIC     = "DOC2PIC";

        public const string REL2BRK     = "TTN2BRK";

        public const string REL2EMK     = "KMC2EMK";
        public const string REL2SSCC    = "DOC2SSCC";

        // коды сортировки детальных строк
        public new enum TABLESORT : int
        {
            NO = 0,                            // без сортировки
            KODMC = 1,                            // по краткому коду
            NAMEMC = 2,                            // по наименованию
            RECSTATE = 3,                            // по статусу записи
            MAXDET = 4                             // максимальное значение
        }


        // статус заявки
        public enum READINESS : int
        {
            NO          = 0,
            PART_READY  = 20,                           // частично выполнена
            FULL_READY = 100                            // полностью выполнена
        }

        //// дополнительные условия по объекту заявки
        //public enum SPECCOND : int
        //{
        //    NO              = 0,
        //    DATE_SET        = 20,                           // не раньше указанной даты выработки
        //    DATE_SET_EXT    = 50,                           // точное соответствие указанной дате выработки
        //    PARTY_SET       = 100,                          // точная партия с датой выработки
        //    SSCC_INT        = 200,
        //    SSCC            = 500
        //}

        // дополнительные условия по объекту заявки
        public enum SPECCOND : int
        {
            NO              = 0,
            DATE_V_SET      = 4,                            // не раньше указанной даты выработки
            DATE_G_SET      = 16,                           // не раньше указанной даты годности
            DATE_SET        = 32,                           // что-то из двух дат задали
            DATE_SET_EXACT  = 64,                           // точное совпадение даты
            PARTY_SET       = 128,                          // точная партия с датой выработки
            SSCC_INT        = 256,
            SSCC            = 512
        }


        // разрешения ввода детальных строк
        public enum DESTINPROD: int
        {
            UNKNOWN = 0,
            GENCASE = 1,                                // общий случай
            TOTALZ  = 2,                                // точное соответствие заявке (EAN-EMK-NP)
            PARTZ   = 3,                                // частичное соответствие заявке
            USER    = 10,                               // подтвердил User
        }

        // происхождение детальных строк
        public enum SRCDET : int
        {
            SCAN            = 1,                        // отсканировали
            FROMZ           = 2,                        // скопировано из заявки
            HANDS           = 3,                        // введено вручную
            SSCCT           = 4,                        // загрузили через SSCC
            FROMADR         = 5,                        // загрузили через адрес
            FROMADR_BUTTON  = 6,                        // загрузили через адрес
            CR4CTRL         = 7                         // созданог при контроле документа
        }

        // фильтрация детальных строк
        [Flags]
        public enum FILTRDET
        {
            UNFILTERED = 0,                             // без фильтра
            READYZ,                                     // по готовности заявок
            NPODD,                                      // по номерам поддонов
            SSCC                                        // по SSCC поддонов
        }

        // результат контроля документа
        public enum DOCCTRL : int
        {
            UNKNOWN = 0,                                // контроль не выполнялся
            OK = 1,                                     // точное соответствие заявке
            WARNS = 2,                                  // есть предупреждения
            ERRS = 3                                    // есть ошибки
        }


        // индексы стилей для гридов
        internal const int GDOC_VNT         = 0;        // для внутреннего
        internal const int GDOC_INV         = 1;        // для инвентаризации
        internal const int GDOC_CENTR       = 2;        // для центровывоза

        internal const int GDOC_NEXT        = 999;      // следующую по списку

        // индексы стилей для детальных строк
        internal const int GDET_SCAN        = 0;        // для сканированных
        internal const int GDET_ZVK         = 1;        // для заявок
        internal const int GDET_ZVK_KMPL    = 2;        // для инвентаризации
        internal const int GDET_ZVK_KMPLV   = 3;        // для комплектации с весовым

        // происхождение документа
        internal const int DOCSRC_LOAD = 1;             // загружен
        internal const int DOCSRC_CRTD = 2;             // создан вручную
        internal const int DOCSRC_UPLD = 3;             // выгружен
       

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

        // загрузка НСИ на терминале (локальных) NEW!!!
        // nReg - LOAD_EMPTY или LOAD_ANY (грузить по-любому)
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




        // создание таблиц
        private void CreateTables()
        {
            DT = new Dictionary<string, TableDef>();

            // информация о справочниках
            DT.Add(BD_TINF, new TableDef(BD_TINF, new DataColumn[]{
                new DataColumn("DT_NAME", typeof(string)),              // имя таблицы
                new DataColumn("LASTLOAD", typeof(DateTime)),           // Дата последней удачной загрузки
                new DataColumn("LOAD_HOST", typeof(string)),            // Host (IP) сервера загрузки
                new DataColumn("LOAD_PORT", typeof(int)),               // Порт сервера загрузки
                new DataColumn("FLAG_LOAD", typeof(string)),            // Режим загрузки с сервера
                new DataColumn("MD5", typeof(string)) }));              // контрольная сумма MD5
            DT[BD_TINF].dt.PrimaryKey = new DataColumn[] { DT[BD_TINF].dt.Columns["DT_NAME"] };
            DT[BD_TINF].nType = TBLTYPE.NSI | TBLTYPE.INTERN;           // создаю сам
            DT[BD_TINF].dt.Columns["LOAD_HOST"].DefaultValue = "";
            DT[BD_TINF].dt.Columns["LOAD_PORT"].DefaultValue = 0;
            DT[BD_TINF].dt.Columns["FLAG_LOAD"].DefaultValue = "";
            DT[BD_TINF].dt.Columns["MD5"].DefaultValue = "";

            // связь EAH-KMC
            DT.Add(NS_KREAN, new TableDef(NS_KREAN, new DataColumn[]{
                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("KMC", typeof(string)),                  // Код (C(10))
                new DataColumn("KRKMC", typeof(int)) }));               // краткий код (N(4))
            DT[NS_KREAN].dt.PrimaryKey = new DataColumn[] { DT[NS_KREAN].dt.Columns["KRKMC"] };
            DT[NS_KREAN].nType = TBLTYPE.CREATE | TBLTYPE.NSI;          // создаю сам

            // типы документов
            DT.Add(NS_TYPD, new TableDef(NS_TYPD, new DataColumn[]{
                new DataColumn("KOD", typeof(int)),                     // тип документаEAN13 (C(13))
                new DataColumn("NAME", typeof(string)) }));             // наименование типа
            DT[NS_TYPD].dt.PrimaryKey = new DataColumn[] { DT[NS_TYPD].dt.Columns["KOD"] };
            DT[NS_TYPD].nType = TBLTYPE.INTERN | TBLTYPE.NSI;

            // паспорт задачи
            DT.Add(BD_PASPORT, new TableDef(BD_PASPORT, new DataColumn[]{
                new DataColumn("KD", typeof(string)),                   // Код данных
                new DataColumn("TD", typeof(string)),                   // Тип данных
                new DataColumn("NAME", typeof(string)),                 // Наименование
                new DataColumn("SD", typeof(string)),                   // значение
                new DataColumn("MD", typeof(string)) }));               // значение
            DT[BD_PASPORT].dt.PrimaryKey = new DataColumn[] { DT[BD_PASPORT].dt.Columns["KD"] };
            //DT[BD_PASPORT].nType = TBLTYPE.BD | TBLTYPE.PASPORT | TBLTYPE.LOAD;
            DT[BD_PASPORT].nType = TBLTYPE.PASPORT | TBLTYPE.NSI | TBLTYPE.LOAD;
            DT[BD_PASPORT].Text = "Паспорт";

            // справочник пользователей
            DT.Add(NS_USER, new TableDef(NS_USER, new DataColumn[]{
                new DataColumn("KP", typeof(string)),                   // код пользователя
                new DataColumn("NMP", typeof(string)),                  // имя пользователя
                new DataColumn("PP", typeof(string)),                   // пароль
                new DataColumn("TABN", typeof(string)) }));             // табельный номер
            DT[NS_USER].dt.PrimaryKey = new DataColumn[] { DT[NS_USER].dt.Columns["KP"] };
            DT[NS_USER].Text = "Пользователи";

            // матценности
            DT.Add(NS_MC, new TableDef(NS_MC, new DataColumn[]{
                new DataColumn("KMC", typeof(string)),                  // Код (C(10))
                new DataColumn("SNM", typeof(string)),                  // Обозначение (C(30))
                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("SRR", typeof(int)),                     // срок реализации (часы) (N(6))
                new DataColumn("SRP", typeof(int)),                     // признак весового (1-весовой) (N(3))
                new DataColumn("KRKMC", typeof(int)),                   // краткий код (N(4))
                new DataColumn("WRAPP", typeof(string)),                // код программы стрейчевания
                new DataColumn("GRADUS", typeof(string)),               // температурный режим для продукции
                new DataColumn("GKMC", typeof(string))  }));            // групповой код (C(10))
            //DT[NS_MC].dt.PrimaryKey = new DataColumn[] { DT[NS_MC].dt.Columns["EAN13"] };
            DT[NS_MC].dt.PrimaryKey = new DataColumn[] { DT[NS_MC].dt.Columns["KMC"] };
            DT[NS_MC].dt.Columns["SRP"].AllowDBNull = false;
            DT[NS_MC].Text = "Мат. ценности";

            // справочник емкостей
            DT.Add(NS_SEMK, new TableDef(NS_SEMK, new DataColumn[]{
                    new DataColumn("KMC", typeof(string)),              // Код продукта(C(10))
                    new DataColumn("KT", typeof(string)),               // код тары(C(10))
                    new DataColumn("KTARA", typeof(string)),            // код тары(C(10))
                    new DataColumn("EMK", typeof(FRACT)),               // емкость/вес   (N(?))
                    new DataColumn("EMKPOD", typeof(int)),              // емкость поддона в тарных местах
                    new DataColumn("KRK", typeof(int)),                 // количество штук (N(5))
                    new DataColumn("GTIN", typeof(string)),             // GTIN (C(14))
                    new DataColumn("WRAPP", typeof(string)),            // код программы стрейчевания
                    new DataColumn("PR", typeof(int)) }));              // приоритет (N(4))
            DT[NS_SEMK].Text = "Емкости";
            DT[NS_SEMK].dt.Columns["EMK"].DefaultValue = 0;
            DT[NS_SEMK].dt.Columns["EMKPOD"].DefaultValue = 0;
            //DT[NS_SEMK].dt.PrimaryKey = new DataColumn[] { 
                //DT[NS_SEMK].dt.Columns["GTIN"] 
                //DT[NS_SEMK].dt.Columns["KMC"],
            //};

            // плательщики / получатели
            DT.Add(NS_PP, new TableDef(NS_PP, new DataColumn[]{
                new DataColumn("KPL", typeof(string)),                  // код плательщика (C(8))
                new DataColumn("KPP", typeof(string)),                  // полный код получателя
                new DataColumn("KRKPP", typeof(int)),                   // Код (N(4))
                new DataColumn("NAME", typeof(string)) }));             // Наименование (C(50))
            DT[NS_PP].dt.PrimaryKey = new DataColumn[] { DT[NS_PP].dt.Columns["KRKPP"] };
            DT[NS_PP].Text = "Получатели-плательщики";

            // экспедиторы
            DT.Add(NS_EKS, new TableDef(NS_EKS, new DataColumn[]{
                new DataColumn("KEKS", typeof(int)),                    // код экспедитора (N(5))
                new DataColumn("FIO", typeof(string)) }));              // ФИО экспедитора (C(50))
            DT[NS_EKS].dt.PrimaryKey = new DataColumn[] { DT[NS_EKS].dt.Columns["KEKS"] };
            DT[NS_EKS].Text = "Экспедиторы";

            // склады
            DT.Add(NS_SKLAD, new TableDef(NS_SKLAD, new DataColumn[]{
                new DataColumn("KSK", typeof(int)),                     // код склада
                new DataColumn("NAME", typeof(string)) }));             // наименование склада
            DT[NS_SKLAD].dt.PrimaryKey = new DataColumn[] { DT[NS_SKLAD].dt.Columns["KSK"] };
            DT[NS_SKLAD].Text = "Склады";

            // участки складов
            DT.Add(NS_SUSK, new TableDef(NS_SUSK, new DataColumn[]{
                new DataColumn("KSK", typeof(int)),                     // код склада
                new DataColumn("NUCH", typeof(int)),                    // код участка
                new DataColumn("NAME", typeof(string)) }));             // наименование участка
            DT[NS_SUSK].dt.PrimaryKey = new DataColumn[] { DT[NS_SUSK].dt.Columns["KSK"], DT[NS_SUSK].dt.Columns["NUCH"] };
            DT[NS_SUSK].Text = "Участки складов";

            // справочник смен
            DT.Add(NS_SMEN, new TableDef(NS_SMEN, new DataColumn[]{
                new DataColumn("KSMEN", typeof(string)),                // код смены
                new DataColumn("NAME", typeof(string)) }));             // наименование смены
            DT[NS_SMEN].dt.PrimaryKey = new DataColumn[] { DT[NS_SMEN].dt.Columns["KSMEN"] };
            DT[NS_SMEN].Text = "Смены";

            // справочник причин брака
            DT.Add(NS_PRPR, new TableDef(NS_PRPR, new DataColumn[]{
                new DataColumn("KPR", typeof(string)),                  // код причины полный
                new DataColumn("KRK", typeof(int)),                     // код причины краткий
                //new DataColumn("NAME", typeof(string)),                 // наименование причины
                new DataColumn("SNM", typeof(string))}));               // краткое наименование причины
            DT[NS_PRPR].dt.PrimaryKey = new DataColumn[] { DT[NS_PRPR].dt.Columns["KPR"] };
            DT[NS_PRPR].Text = "Причины брака";

            // внутренние коды получателей
            DT.Add(NS_KRUS, new TableDef(NS_KRUS, new DataColumn[]{
                new DataColumn("KMC", typeof(string)),                  // Код (C(10))
                new DataColumn("EAN13", typeof(string)),                // Код (C(10))
                new DataColumn("KINT", typeof(string))  }));            // внутренний код
            DT[NS_KRUS].dt.PrimaryKey = new DataColumn[] { DT[NS_KRUS].dt.Columns["KINT"] };
            DT[NS_KRUS].Text = "Коды получателей";

            // идентификаторы применения
            DT.Add(NS_AI, new TableDef(NS_AI, new DataColumn[]{
                new DataColumn("KAI", typeof(string)),              // Код идентификатора
                new DataColumn("NAME", typeof(string)),             // Наименование
                new DataColumn("TYPE", typeof(string)),             // Тип данных
                new DataColumn("MAXL", typeof(int)),                // Длина данных
                new DataColumn("VARLEN", typeof(int)),              // Признак переменной длины
                new DataColumn("DECP", typeof(int)),                // Позиция десятичной точки
                new DataColumn("PROP", typeof(string)),             // Поле
                new DataColumn("KED", typeof(string)) }));          // Код единицы
            DT[NS_AI].dt.PrimaryKey = new DataColumn[] { DT[NS_AI].dt.Columns["KAI"] };
            DT[NS_AI].nType = TBLTYPE.INTERN | TBLTYPE.NSI;
            DT[NS_AI].nState = DT_STATE_INIT;
            DT[NS_AI].Text = "Идентификаторы применения";

            // описание адресов зон и ячеек
            DT.Add(NS_ADR, new TableDef(NS_ADR, new DataColumn[]{
                new DataColumn("KADR", typeof(string)),             // адрес ячейки-зоны
                new DataColumn("NAME", typeof(string)),             // Наименование
                new DataColumn("TYPE", typeof(int)) }));            // Тип
            DT[NS_ADR].dt.PrimaryKey = new DataColumn[] { DT[NS_ADR].dt.Columns["KADR"] };
            //DT[NS_ADR].nType = TBLTYPE.INTERN | TBLTYPE.NSI;
            //DT[NS_ADR].nState = DT_STATE_INIT;
            DT[NS_ADR].Text = "Адреса";
            // Функция для отображения адреса NameAdr(nSklad, sAdr);


            // заголовки документов
            DT.Add(BD_DOCOUT, new TableDef(BD_DOCOUT, new DataColumn[]{
                new DataColumn("TD", typeof(int)),                      // Тип документа (N(2))
                new DataColumn("KRKPP", typeof(int)),                   // Код получателя (N(4))
                new DataColumn("KSMEN", typeof(string)),                // Код смены (C(3))
                new DataColumn("DT", typeof(string)),                   // Дата (C(8))
                new DataColumn("KSK", typeof(int)),                     // Код склада (N(3))
                new DataColumn("NUCH", typeof(int)),                    // Номер участка (N(3))
                new DataColumn("KEKS", typeof(int)),                    // Код экспедитора (N(5))
                new DataColumn("NOMD", typeof(string)),                 // Номер документа (C(10))
                new DataColumn("SYSN", typeof(int)),                    // ID Код (N(9))
                new DataColumn("SOURCE", typeof(int)),                  // Происхождение N(2))
                new DataColumn("DIFF", typeof(int)),                    // Отклонение от заявки
                new DataColumn("EXPR_DT", typeof(string)),              // выражение для даты
                new DataColumn("EXPR_SRC", typeof(string)),             // выражение для происхождения

                new DataColumn("CHKSSCC", typeof(int)),                 // Для контроля SSCC

                new DataColumn("PP_NAME", typeof(string)),              // Наименование получателя
                new DataColumn("EKS_NAME", typeof(string)),             // Наименование экспедитора
                new DataColumn("MEST", typeof(int)),                    // Количество мест(N(3))
                new DataColumn("MESTZ", typeof(int)),                   // Количество мест по заявке(N(3))
                new DataColumn("KOLE", typeof(FRACT)),                  // Количество единиц (N(10,3))
                new DataColumn("TYPOP", typeof(int)),                   // Тип операции (приемка, отгрузка, ...)

                new DataColumn("LSTUCH", typeof(string)),               // Список участков
                new DataColumn("LSTNPD", typeof(string)),               // Список номеров поддонов
                new DataColumn("CONFSCAN", typeof(int)),                // Режим подтверждения сканирования(ввода)

                new DataColumn("SSCCONLY", typeof(int)),                // 1 - Режим ввода - только SSCC
                new DataColumn("PICTURE", typeof(string)),              // фото поддона
                new DataColumn("DTPRIB", typeof(string)),               // время прибытия под загрузку в формате dd.MM.yyyy HH:mm
                
                new DataColumn("TIMECR", typeof(DateTime)),             // дата-время создания
                new DataColumn("ID_LOAD", typeof(string))  }));         // Код загруженного документа (C(10))

            DT[BD_DOCOUT].dt.Columns["EXPR_DT"].Expression = "substring(DT,7,2) + '.' + substring(DT,5,2)";
            DT[BD_DOCOUT].dt.Columns["EXPR_SRC"].Expression = "iif(SOURCE=1,'Загр', iif(SOURCE=2,'Ввод','Выгр'))";

            DT[BD_DOCOUT].dt.Columns["DIFF"].DefaultValue = NSI.DOCCTRL.UNKNOWN;
            DT[BD_DOCOUT].dt.Columns["MEST"].DefaultValue = 0;
            DT[BD_DOCOUT].dt.Columns["MESTZ"].DefaultValue = 0;
            DT[BD_DOCOUT].dt.Columns["TIMECR"].DefaultValue = DateTime.Now; ;
            DT[BD_DOCOUT].dt.Columns["TYPOP"].DefaultValue = AppC.TYPOP_PRMK;
            DT[BD_DOCOUT].dt.Columns["CONFSCAN"].DefaultValue = 0;
            DT[BD_DOCOUT].dt.Columns["EKS_NAME"].DefaultValue = "";
            DT[BD_DOCOUT].dt.Columns["DTPRIB"].DefaultValue = "";
            DT[BD_DOCOUT].dt.Columns["DTPRIB"].AllowDBNull = false;

            DT[BD_DOCOUT].dt.PrimaryKey = new DataColumn[] { DT[BD_DOCOUT].dt.Columns["SYSN"] };
            DT[BD_DOCOUT].dt.Columns["SYSN"].AutoIncrement = true;
            DT[BD_DOCOUT].dt.Columns["SYSN"].AutoIncrementSeed = -1;
            DT[BD_DOCOUT].dt.Columns["SYSN"].AutoIncrementStep = -1;
            DT[BD_DOCOUT].nType = TBLTYPE.BD;

            // детальные строки (введенные)
            DT.Add(BD_DOUTD, new TableDef(BD_DOUTD, new DataColumn[]{
                new DataColumn("KRKMC", typeof(int)),                   // краткий код (N(4))
                new DataColumn("SNM", typeof(string)),                  // Обозначение (C(30))
                new DataColumn("KOLM", typeof(int)),                    // количество мест (N(4))
                new DataColumn("KOLE", typeof(FRACT)),                  // всего (единиц или вес) (N(10,3))
                new DataColumn("EMK", typeof(FRACT)),                   // емкость   (N(?))

                new DataColumn("NP", typeof(string)),                   // № партии (N(4))

                new DataColumn("DVR", typeof(string)),                  // дата выработки (D(8))
                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("SYSN", typeof(int)),                    // ключ документа (N(9))
                new DataColumn("SRP", typeof(int)),                     // признак весового (1-весовой) (N(3))
                new DataColumn("GKMC", typeof(string)),                 // групповой код (C(10))
                new DataColumn("KRKT", typeof(string)),                 // код тары(C(10))
                new DataColumn("KTARA", typeof(string)),                // код тары(C(10))

                new DataColumn("VES", typeof(FRACT)),                   // всего (единиц или вес) (N(10,3))
                new DataColumn("KOLG", typeof(int)),                    // всего единиц потребительской тары (N(10,3))
                
                new DataColumn("KOLSH", typeof(int)),                   // из справочника емкостей-штук/упаковку (N(2))
                new DataColumn("DEST", typeof(int)),                    // назначение строки
                new DataColumn("ID", typeof(int)),                      // ID строки

                new DataColumn("NPODDZ", typeof(int)),                  // № поддона из заявки
                new DataColumn("ADRFROM", typeof(string)),              // адрес отправления
                new DataColumn("ADRTO", typeof(string)),                // адрес получения
                new DataColumn("NPODD", typeof(int)),                   // № поддона внутри партии
                new DataColumn("NMESTA", typeof(int)),                  // № места
                new DataColumn("SSCC", typeof(string)),                 // ID поддона
                new DataColumn("SSCCINT", typeof(string)),              // внутренний SSCC поддона

                new DataColumn("SYSPRD", typeof(int)),                  // SYSN предъявления

                new DataColumn("USER", typeof(string)),                 // код пользователя
                
                new DataColumn("SRC", typeof(int)),                     // происхождение строки
                new DataColumn("TIMECR", typeof(DateTime)),             // дата-время создания
                new DataColumn("TIMEOV", typeof(DateTime)),             // дата-время создания
                new DataColumn("STATE", typeof(int)),                   // состояние строки
                
                new DataColumn("NPP_ZVK", typeof(int)),                 // ID строки-заявки
                new DataColumn("KMC", typeof(string)) }));              // Код (C(10))
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

            // детальные строки заявки
            DT.Add(BD_DIND, new TableDef(BD_DIND, new DataColumn[]{
                new DataColumn("KRKMC", typeof(int)),                   // краткий код (N(4))
                new DataColumn("SNM", typeof(string)),                  // Обозначение (C(30))
                new DataColumn("KOLM", typeof(int)),                    // количество мест (N(4))
                new DataColumn("KOLE", typeof(FRACT)),                  // всего (единиц или вес) (N(10,3))
                new DataColumn("EMK", typeof(FRACT)),                   // емкость   (N(?))

                //new DataColumn("NP", typeof(int)),                      // № партии (N(4))
                new DataColumn("NP", typeof(string)),                   // № партии (N(4))

                new DataColumn("DVR", typeof(string)),                  // дата выработки (D(8))
                new DataColumn("DTG", typeof(string)),                  // дата годности (D(8))

                new DataColumn("EAN13", typeof(string)),                // EAN13 (C(13))
                new DataColumn("SYSN", typeof(int)),                    // ключ документа (N(9))
                new DataColumn("SRP", typeof(int)),                     // признак весового (1-весовой) (N(3))
                new DataColumn("GKMC", typeof(string)),                 // групповой код (C(10))
                //new DataColumn("KRKT", typeof(string)),                 // код тары(C(10))
                new DataColumn("KTARA", typeof(string)),                // код тары(C(10))

                new DataColumn("COND", typeof(int)),                    // условия по заявке
                new DataColumn("READYZ", typeof(int)),                  // готовность заявки по продукции

                new DataColumn("NPODDZ", typeof(int)),                  // № поддона
                new DataColumn("NPP", typeof(int)),                     // № поддона п/п для укладки поддона

                new DataColumn("ADRFROM", typeof(string)),              // адрес отправления
                new DataColumn("ADRTO", typeof(string)),                // адрес получения

                new DataColumn("SSCC", typeof(string)),                 // ID поддона
                new DataColumn("SSCCINT", typeof(string)),              // внутренний SSCC поддона

                new DataColumn("KRKPP", typeof(string)),                // Код получателя (N(5))
                new DataColumn("ID", typeof(int)),                      // ID строки

                new DataColumn("KMC", typeof(string)) }));              // Код (C(10))

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

            // список брака к документу
            DT.Add(BD_SPMC, new TableDef(BD_SPMC, new DataColumn[]{
                new DataColumn("SYSN", typeof(int)),                    // ключ документа (N(9))
                new DataColumn("ID", typeof(int)),                      // ID строки продукции
                new DataColumn("IDB", typeof(int)),                     // ID строки брака
                new DataColumn("SNM", typeof(string)),                  // наименование причины
                new DataColumn("KOLM", typeof(int)),                    // количество мест (N(4))
                new DataColumn("KOLE", typeof(FRACT)),                  // всего (единиц или вес) (N(10,3))
                new DataColumn("KRK", typeof(int)),                     // код причины краткий
                new DataColumn("KPR", typeof(string)),                  // код причины полный
                new DataColumn("TIMECR", typeof(DateTime))}));          // дата-время создания
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
            DT[BD_SPMC].Text = "Список брака";

            // список SSCC для документа
            DT.Add(BD_SSCC, new TableDef(BD_SSCC, new DataColumn[]{
                new DataColumn("SYSN",      typeof(int)),               // ключ документа (N(9))
                new DataColumn("NPODDZ",    typeof(int)),               // № поддона
                new DataColumn("SSCC",      typeof(string)),            // SSCC поддона
                new DataColumn("KOLM",      typeof(int)),               // мест
                new DataColumn("KOLE",      typeof(FRACT)),             // единиц

                new DataColumn("MONO",      typeof(int)),               // флаг монопаллеты

                new DataColumn("IN_ZVK",    typeof(int)),               // 1 - получено с сервера (как заявка)
                new DataColumn("IN_TTN",    typeof(int)),               // 1 - отсканированотерминалом
                new DataColumn("STATE",     typeof(int)),               // состояние
                new DataColumn("ID",        typeof(int)) }));           // ID строки
            DT[BD_SSCC].nType = TBLTYPE.BD;
            DT[BD_SSCC].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_SSCC].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_SSCC].dt.Columns["ID"].AutoIncrementStep = -1;
            DT[BD_SSCC].dt.Columns["IN_ZVK"].DefaultValue = 0;
            DT[BD_SSCC].dt.Columns["IN_TTN"].DefaultValue = 0;

            // список схем поддонов для документа
            DT.Add(BD_PICT, new TableDef(BD_PICT, new DataColumn[]{
                new DataColumn("SYSN",      typeof(int)),               // ключ документа (N(9))
                new DataColumn("NPODDZ",    typeof(int)),               // № поддона
                new DataColumn("NPP",       typeof(int)),               // № фото
                new DataColumn("PICTURE",   typeof(string)),            // фото поддона
                new DataColumn("ID",        typeof(int)) }));           // ID строки
            DT[BD_PICT].nType = TBLTYPE.BD;
            DT[BD_PICT].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_PICT].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_PICT].dt.Columns["ID"].AutoIncrementStep = -1;

            // список авто для выбора
            DT.Add(BD_SOTG, new TableDef(BD_SOTG, new DataColumn[]{
                new DataColumn("SYSN", typeof(int)),                    // ключ документа (N(9))
                new DataColumn("NPP", typeof(int)),                     // ключ документа (N(9))
                new DataColumn("ID", typeof(int)),                      // ID строки
                new DataColumn("KSMEN", typeof(string)),                // Код смены (C(3))
                new DataColumn("DTP", typeof(string)),                // Дата/время прибытия
                new DataColumn("DTU", typeof(string)),                // Дата/время убытия
                new DataColumn("NSH", typeof(int)),                     // № шлюза
                new DataColumn("KEKS", typeof(int)),                    // Код экспедитора (N(5))
                new DataColumn("KAVT", typeof(string)),                 // № авто
                new DataColumn("NPL", typeof(int)),                     // № путевого
                new DataColumn("ND", typeof(int)),                      // № документа
                new DataColumn("ROUTE", typeof(string)),                // описание маршрута
                new DataColumn("STATE", typeof(int))}));                // состояние

            DT[BD_SOTG].dt.PrimaryKey = new DataColumn[] { DT[BD_SOTG].dt.Columns["ID"] };

            DT[BD_SOTG].dt.Columns["ID"].AutoIncrement = true;
            DT[BD_SOTG].dt.Columns["ID"].AutoIncrementSeed = -1;
            DT[BD_SOTG].dt.Columns["ID"].AutoIncrementStep = -1;

            DT[BD_SOTG].dt.Columns["STATE"].DefaultValue = 0;
            DT[BD_SOTG].nType = TBLTYPE.BD;
            DT[BD_SOTG].Text = "Список авто";

            // заголовки заказов на комплектацию
            DT.Add(BD_KMPL, new TableDef(BD_KMPL, new DataColumn[]{
                new DataColumn("TD", typeof(int)),                      // Тип документа (N(2))
                new DataColumn("KRKPP", typeof(int)),                   // Код получателя (N(4))
                new DataColumn("KSMEN", typeof(string)),                // Код смены (C(3))
                new DataColumn("DT", typeof(string)),                   // Дата (C(8))
                new DataColumn("KSK", typeof(int)),                     // Код склада (N(3))
                new DataColumn("NUCH", typeof(string)),                 // Список участков
                new DataColumn("KEKS", typeof(int)),                    // Код экспедитора (N(5))
                new DataColumn("NOMD", typeof(string)),                 // Номер документа (C(10))
                new DataColumn("SYSN", typeof(long)),                    // ID Код (N(9))
                new DataColumn("KOLPODD", typeof(int)),                 // Поддонов для документа

                new DataColumn("EXPR_DT", typeof(string)),              // выражение для даты
                
                new DataColumn("PP_NAME", typeof(string)),              // Наименование получателя
                new DataColumn("TYPOP", typeof(int)),                   // Тип операции (приемка, отгрузка, ...)
                
                new DataColumn("KOBJ", typeof(string))  }));            // Код объекта (C(10))

            DT[BD_KMPL].dt.Columns["EXPR_DT"].Expression = "substring(DT,7,2) + '.' + substring(DT,5,2)";

            DT[BD_KMPL].dt.Columns["TYPOP"].DefaultValue = AppC.TYPOP_KMPL;

            DT[BD_KMPL].dt.PrimaryKey = new DataColumn[] { DT[BD_KMPL].dt.Columns["SYSN"] };
            DT[BD_KMPL].dt.Columns["SYSN"].AutoIncrement = true;
            DT[BD_KMPL].dt.Columns["SYSN"].AutoIncrementSeed = -1;
            DT[BD_KMPL].dt.Columns["SYSN"].AutoIncrementStep = -1;
            DT[BD_KMPL].nType = TBLTYPE.BD;

            // бланки по типам документов
            DT.Add(NS_BLANK, new TableDef(NS_BLANK, new DataColumn[]{
                new DataColumn("TD",        typeof(int)),               // тип доумента
                new DataColumn("KBL",       typeof(string)),            // код бланка
                new DataColumn("NAME",      typeof(string)),            // Наименование бланка
                new DataColumn("PS",        typeof(int)),               // Выгрузка детальных строк
                new DataColumn("BCT",       typeof(string)),            // Блок кода для бланка
                new DataColumn("NPARS",     typeof(int)) }));           // Количество дополнительных параметров
            DT[NS_BLANK].dt.PrimaryKey = new DataColumn[] { 
                DT[NS_BLANK].dt.Columns["TD"],
                DT[NS_BLANK].dt.Columns["KBL"]};
            DT[NS_BLANK].Text = "Бланки документов";

            // параметры бланков (поля ввода формы)
            DT.Add(NS_SBLK, new TableDef(NS_SBLK, new DataColumn[]{
                new DataColumn("KBL",       typeof(string)),            // код бланка
                new DataColumn("NPP",       typeof(int)),               // № п/п
                new DataColumn("KPAR",      typeof(string)),            // Наименование параметра
                new DataColumn("NAME",      typeof(string)),            // Назначение параметра
                new DataColumn("TPAR",      typeof(string)),            // Тип параметра
                new DataColumn("VALUE",     typeof(string)),            // значение параметра
                new DataColumn("PRFX",      typeof(string)),            // префикс штрихкода
                new DataColumn("PARS",      typeof(string)),            // код поля штрихкода
                new DataColumn("BEGS",      typeof(int)),               // смещение в строке ШК (1,...
                new DataColumn("LENS",      typeof(int)),               // смещение в строке ШК (1,...
                new DataColumn("FUNC",      typeof(string)),            // функция после Validate
                new DataColumn("DSOURCE",   typeof(string)),            // DataSource
                new DataColumn("DISPLAY",   typeof(string)),            // DisplayMember
                new DataColumn("RESULT",    typeof(string)),            // ValueMember
                new DataColumn("TBCODE",    typeof(string)),            // тип штрихкода
                new DataColumn("PERCAPT",   typeof(int)),               // смещение в строке ШК (1,...
                new DataColumn("BCT",       typeof(string)),            // Блок кода для EXPR
                new DataColumn("FORMAT",    typeof(string)) }));        // формат текстовых полей
            DT[NS_SBLK].dt.PrimaryKey = new DataColumn[] { 
                DT[NS_SBLK].dt.Columns["KBL"], DT[NS_SBLK].dt.Columns["KPAR"]};
            //DT[NS_SBLK].nType = TBLTYPE.INTERN | TBLTYPE.NSI;

            DT[NS_SBLK].dt.Columns["DSOURCE"].DefaultValue = "";
            DT[NS_SBLK].dt.Columns["DISPLAY"].DefaultValue = "";
            DT[NS_SBLK].dt.Columns["RESULT"].DefaultValue = "";

            DT[NS_SBLK].Text = "Список параметров бланка";
        }

        // создание стилей просмотра таблиц
        public void ConnDTGrid(DataGrid dgDoc, DataGrid dgDet)
        {
            dgDoc.SuspendLayout();
            DT[BD_DOCOUT].dg = dgDoc;
            dgDoc.DataSource = DT[BD_DOCOUT].dt;
            CreateTableStyles(DT[BD_DOCOUT].dg);
            ChgGridStyle(BD_DOCOUT, GDOC_VNT);
            dgDoc.ResumeLayout();

            // Просмотр детальных строк
            dgDet.SuspendLayout();
            DT[BD_DOUTD].dg = dgDet;
            // у заявок - тот же Grid
            DT[BD_DIND].dg = dgDet;
            CreateTableStylesDet(dgDet);
            ChgGridStyle(BD_DIND, GDET_ZVK);
            //ChgGridStyle(BD_DIND, GDET_ZVK_KMPL);
            // по умолчанию - просмотр ТТН
            dgDet.DataSource = dsM.Relations[0].ChildTable;
            ChgGridStyle(BD_DOUTD, GDET_SCAN);
            dgDet.ResumeLayout();
        }

        // стили просмотра таблицы документов в гриде
        private void CreateTableStyles(DataGrid dg)
        {
            DataGridTableStyle 
                ts,
                tss;
            // специальные цвета для результатов контроля
            System.Drawing.Color 
                colForFullAuto = System.Drawing.Color.LightGreen,
                colSpec = System.Drawing.Color.PaleGoldenrod;

            ServClass.DGTBoxColorColumnDoc
                sC;

            dg.TableStyles.Clear();

            // для внутреннего
            ts = new DataGridTableStyle();
            ts.MappingName = GDOC_VNT.ToString();

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_DT";
            sC.HeaderText = "Дата";
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
            sC.HeaderText = "Склад";
            sC.Width = 35;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_SRC";
            sC.HeaderText = "Загр";
            sC.Width = 32;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KRKPP";
            sC.HeaderText = "П-ль";
            sC.Width = 33;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.ReadOnly = true;
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "MEST";
            sC.HeaderText = "Мест";
            sC.NullText = "";
            sC.Width = 36;
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "NOMD";
            sC.HeaderText = "№ док";
            sC.Width = 55;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);

            // Для инвентаризации
            DataGridTableStyle tsi = new DataGridTableStyle();
            tsi.MappingName = GDOC_INV.ToString();

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_DT";
            sC.HeaderText = "Дата";
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
            sC.HeaderText = "Смена";
            sC.Width = 35;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "NUCH";
            sC.HeaderText = "Уч";
            sC.Width = 20;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KEKS";
            sC.HeaderText = "Эксп";
            sC.Width = 33;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "EXPR_SRC";
            sC.HeaderText = "Загр";
            sC.Width = 32;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.AlternatingBackColor = colForFullAuto;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "MEST";
            sC.HeaderText = "Мест";
            sC.NullText = "";
            sC.Width = 36;
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "MESTZ";
            sC.HeaderText = "МестЗ";
            sC.Width = 40;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "DIFF";
            sC.HeaderText = "Ст";
            sC.Width = 18;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "KOLE";
            sC.HeaderText = "Всего";
            sC.Width = 36;
            sC.NullText = "";
            sC.AlternatingBackColorSpec = colSpec;
            tsi.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(tsi);

            // для центровывоза
            tss = new DataGridTableStyle();
            tss.MappingName = GDOC_CENTR.ToString();

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "PP_NAME";
            sC.HeaderText = "Плательщик";
            sC.Width = 130;
            sC.NullText = "";
            tss.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "MEST";
            sC.HeaderText = "Мест";
            sC.Width = 40;
            sC.NullText = "";
            tss.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumnDoc(dg, BD_DOCOUT);
            sC.MappingName = "DIFF";
            sC.HeaderText = "Гот";
            sC.Width = 35;
            sC.NullText = "";
            tss.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(tss);
        }


        private Color
            C_READY_ZVK = Color.LightGreen,                 // детальная Заявка выполнена
            C_READY_TTN = Color.Lavender,                   // детальная ТТН готова к передаче
            C_TNSFD_TTN = Color.LightGreen;                 // детальная ТТН передана на сервер

        // стили таблицы детальных строк (ТТН и Заявки)
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

            ts = new DataGridTableStyle();                                  // Для результатов сканирования (ТТН)
            ts.MappingName = GDET_SCAN.ToString();

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KRKMC";
            sC.HeaderText = "Код";
            sC.Width = 27;
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "SNM";
            sC.HeaderText = "Наименование";
            sC.Width = 136;
            sC.AlternatingBackColor = colGreen;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KOLM";
            sC.HeaderText = "М-т";
            sC.AlternatingBackColor = colSpec;
            sC.Width = 26;
            ts.GridColumnStyles.Add(sC);

            colTB = new DataGridTextBoxColumn();
            colTB.MappingName = "EMK";
            colTB.HeaderText = "Емк";
            colTB.Width = 35;
            ts.GridColumnStyles.Add(colTB);

            colTB = new DataGridTextBoxColumn();
            colTB.MappingName = "NP";
            colTB.HeaderText = "Прт";
            colTB.Width = 35;
            colTB.NullText = "";
            ts.GridColumnStyles.Add(colTB);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "DVR";
            sC.HeaderText = "Двыр";
            sC.Width = 36;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DOUTD);
            sC.MappingName = "KOLE";
            sC.HeaderText = "Ед.";
            sC.Width = 43;
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            ts.GridColumnStyles.Add(sC);

            //colTB = new DataGridTextBoxColumn();
            //colTB.MappingName = "NPODD";
            //colTB.HeaderText = "№Пд";
            //colTB.Width = 25;
            //colTB.NullText = "";
            //ts.GridColumnStyles.Add(colTB);

            colTB = new DataGridTextBoxColumn();
            colTB.MappingName = "NPODDZ";
            colTB.HeaderText = "ПдЗ";
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
            sC.HeaderText = "Тара";
            sC.Width = 35;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);

            dg.TableStyles.Add(ts);

            /// *************************** для заявок ************************
            ts = new DataGridTableStyle();                                      // в режиме обычного просмотра
            ts.MappingName = GDET_ZVK.ToString();

            tsK = new DataGridTableStyle();                                     // в режиме комплектации
            tsK.MappingName = GDET_ZVK_KMPL.ToString();

            tsKV = new DataGridTableStyle();                                     // в режиме комплектации
            tsKV.MappingName = GDET_ZVK_KMPLV.ToString();

            //sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            //sC.MappingName = "NPODDZ";
            //sC.HeaderText = "Пд";
            //sC.Width = 22;
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "NPP";
            sC.HeaderText = "№";
            sC.Width = 30;
            sC.Alignment = HorizontalAlignment.Right;
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "KRKMC";
            sC.HeaderText = "Код";
            sC.Width = 30;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "SNM";
            sC.HeaderText = "Наименование";
            sC.Width = 136;
            ts.GridColumnStyles.Add(sC);
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "EMK";
            sC.HeaderText = "Емк";
            sC.Width = 28;
            sC.Alignment = HorizontalAlignment.Right;
            //ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = Color.Azure;
            sC.MappingName = "KOLM";
            sC.HeaderText = "Мест";
            sC.Width = 36;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "KOLE";
            sC.HeaderText = "Ед.";
            sC.Width = 45;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "EMK";
            sC.HeaderText = "Емк";
            sC.Width = 28;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            //tsK.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "DVR";
            sC.HeaderText = "Дврб";
            sC.Width = 34;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "NP";
            sC.HeaderText = "№ Пт";
            sC.Width = 36;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);


            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.MappingName = "KRKPP";
            sC.HeaderText = "П-ль";
            sC.Width = 36;
            sC.NullText = "";
            sC.Alignment = HorizontalAlignment.Left;
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "DTG";
            sC.HeaderText = "Дгодн";
            sC.Width = 34;
            sC.Alignment = HorizontalAlignment.Right;
            ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            sC.AlternatingBackColor = colGreen;
            sC.AlternatingBackColorSpec = colSpec;
            sC.MappingName = "KOLE";
            sC.HeaderText = "Ед.";
            sC.Width = 45;
            sC.Alignment = HorizontalAlignment.Right;
            //ts.GridColumnStyles.Add(sC);
            tsK.GridColumnStyles.Add(sC);
            tsKV.GridColumnStyles.Add(sC);

            sC = new ServClass.DGTBoxColorColumn(dg, NSI.BD_DIND);
            //c.MappingName = "KRKT";
            sC.MappingName = "KTARA";
            sC.HeaderText = "Тара";
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

        // смена стиля таблицы
        // nSt - требуемый стиль
        public void ChgGridStyle(string iT, int nSt)
        {
            if (DT[iT].nGrdStyle != -1)
            {                                                           // НЕ первичная установка
                int nOld = DT[iT].nGrdStyle;
                string sCurStyle = DT[iT].dg.TableStyles[nOld].MappingName;

                // очистка текущей
                DT[iT].dg.TableStyles[nOld].MappingName = nOld.ToString();
                if (nSt == GDOC_NEXT)
                {                                                       // циклическая смена
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
        //        ret = "Цтр";
        //    else if (i == GDOC_INV)
        //        ret = "Инв";
        //    else if (i == GDOC_SAM)
        //        ret = "Сам";
        //    return (ret);
        //}


        // характеристики для одной таблицы
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
        // действия по окончании загрузки справочника в память
        // после инициализации любой из таблиц
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
                    {// справочник пришел от сервера
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
                    {// локальная загрузка (подготовка поиска)
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

            if (DT[NS_MC].nState > DT_STATE_INIT)  // справочник загружен
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
                            Srv.ErrorMsg(String.Format("EAN={0}\nСканируйте ITF({1})", sEAN, s.ci.ToString()), "Неоднозначность!", true);
                        return (false);
                    }
                    else
                        dr = xRowDView[0].Row;
                    ret = s.GetFromNSI(s.s, dr, DT[NSI.NS_MC].dt, bShowErr);
                    if (!ret)
                    {
                        if (bShowErr)
                            Srv.ErrorMsg(String.Format("EAN={0}", sEAN), "Не найдено в НСИ!", true);
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

            if (DT[NS_MC].nState > DT_STATE_INIT)  // справочник загружен
            {
                if ((sKMCFull.Length == 0) && (nKrKMC > 0))
                {
                    dr = DT[NS_KREAN].dt.Rows.Find(new object[] { nKrKMC });
                    if (dr != null)
                        sKMCFull = (string)dr["KMC"];
                }
                if (sKMCFull.Length == 0)              // это не Code128, EAN13 с кратким кодом 
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
                        Srv.ErrorMsg(String.Format("KMC={0}\nКод={1}", sKMCFull, nKrKMC), "Не найдено в НСИ!", true);
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


        // поиск среди внутренних кодов получателей
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

            if (DT[NS_KRUS].nState > DT_STATE_INIT)     // справочник загружен
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

            // список номеров поддонов из накладных
            DataView dv1 = new DataView(dtD, xD.DefDetFilter(), "", DataViewRowState.CurrentRows);
            DataTable dtN1 = dv1.ToTable(true, "NPODDZ");

            xD.xNPs.Clear();

            if (xD.xDocP.TypOper == AppC.TYPOP_KMPL)
            {
                // список номеров поддонов из заявок
                DataView dv = new DataView(dt, xD.DefDetFilter(), "", DataViewRowState.CurrentRows);
                DataTable dtN = dv.ToTable(true, "NPODDZ");

                // это свободная комплектация?
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




        // чтение текущей строки в объект панели документов
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



        // заполнение строки таблицы документов
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

        // добавление новой записи в документы
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

        // чтение текущей детальной строки
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



        // сохранение текущей детальной строки
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

                    // отладка чудес всяких
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
                    Srv.ErrorMsg("Ошибка добавления продукции!");
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

        // подготовка DataSet для выгрузки
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
        //            // автоматическая выгрузка одной строки по окончании операции
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





        // подготовка DataSet для выгрузки
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
                // для SSCC выгружаем только заголовок
                if ((xCU.sCurUplCommand == AppC.COM_ZSC2LST) ||
                     (xCU.sCurUplCommand == AppC.COM_ADR2CNT) )
                    break;


                if (drDetReady == null)
                {// массив детальных строк еще не готов
                    if (xCU.bOnlyCurRow)
                        // автоматическая выгрузка одной строки по окончании операции
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
                        #region Необходимость включения детальной строки
                        do
                        {
                            if (drDetReady != null)
                                // все подготовленные строки войдут в выгрузку
                                break;

                            if ((int)dr["TYPOP"] != AppC.TYPOP_DOCUM)
                            {// для операционного режима могут быть варианты...
                                if ((AppC.OPR_STATE)chRow["STATE"] == AppC.OPR_STATE.OPR_TRANSFERED)
                                {// операция уже выгружалась
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
                                        {// неотмаркированная продукция 
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
                        //{// это материал
                        //    drAdded["NP"] = "";
                        //}

                        if ((int)drAdded["SYSPRD"] < 0) 
                        {// это материал
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

        // датасет для заявок
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
            {// загрузка документа
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

        /// восстановить сохраненные данные
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
                {// ну, значит, не было 
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
