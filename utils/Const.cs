//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Samatar Hassan.
//
//===================================================================

using System;
using System.Web;
using System.IO;
using System.Text;
using System.Globalization;
using SafeNetWS.log;
using SafeNetWS.login;
using System.Configuration;
using System.Collections;
using System.Reflection;

namespace SafeNetWS.utils
{

    /// <summary>
    /// Cette classe contient des constantes "genérales" utiles
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>

    public class Const
    {
        public const string ENettChannelMobile = "Mobile";
        public const string StatusAuthorised = "AUTHORISED";
        public const string StatusRefused = "REFUSED";
        public const string StatusTimeOut = "TIME_OUT";
        public const string StatusConnectionError = "CONNECTION ERROR";

        public const string Success = "OK";
        public const string Failed = "KO";

        // Source contexte mise à jour des cartes transactionnelles

        // Le contexte provient de la préfacture (le contexte est le numéro de préfacture)
        public const string Context_Source_PreSales = "SH";
        // Le contexte vient du billing unit (le contexte est le numéro du billet)
        public const string Context_Source_BU = "BU";
        // Le contexte vient du CLE (Le contexte est le numéro d'entrée)
        public const string Context_Source_CLE = "CLE";


        // Application
        private const string ApplicationName = "CEE";


        // Envoi d'un mail au support en cas d'erreur
        // au niveau de la base de données
        public const int SupportAlertSourceCreditCardConnLog = 0;
        public const string SupportAlertSourceCreditCardConnLogMsg = "insert rejected card in rejections table";
        // au niveau de SafeNet
        public const int SupportAlertSourceSafeNetSession = 1;
        public const string SupportAlertSourceSafeNetSessionMsg = "open SafeNet session";
        // au niveau du cache RBS BO
        public const int SupportAlertSourceRBSBOCache = 2;
        public const string SupportAlertSourceRBSBOCacheSaveMsg = "RBS BO cache save";
        public const string SupportAlertSourceRBSBOCacheRetrieveMsg = "RBS BO cache retrieve";
        // au niveau du cache RBS FO
        public const int SupportAlertSourceRBSFOCache = 3;
        public const string SupportAlertSourceRBSFOCacheSaveMsg = "RBS FO cache save";
        public const string SupportAlertSourceRBSFOCacheRetrieveMsg = "RBS FO cache retrieve";

        public const string On = "ON";

        public static DateTime NavisionEmptyDate = Util.GetNavisionEmptyDate();

        public const string NavisionEmptyDateString = "01/01/1753 00:00:00";
        public const string CRLF = "\r\n";
        public const string EmptyUserLog = "A user called the service.";

        // Global Cache
        public const int StatusCacheSuccess = 1;
        public const int StatusCacheFailed = 0;

        public static DateTime EmptyDate = DateTime.MinValue;
        public const long EmptyBoToken = -1;
        // Pos
        public const string PosFrance="FRANCE";
        public const string PosUK="UK";
        public const string PosBelgium="BELGIUM";
        public const string PosGermany="GERMANY";
        public const string PosSpain="SPAIN";
        public const string PosIreland="IRELAND";
        public const string PosItaly="ITALY";
        public const string PosNetherlands= "NETHERLANDS";
        public const string PosSwitzerland="SWITZERLAND";
        public const string PosSweden="SWEDEN";
        public const string PosAustralia = "AUSTRALIA";
        public const string PosChina = "CHINA";
        public const string PosIndia = "INDIA";
        public const string PosCzechRepublic = "CZECH REPUBLIC";
        public const string PosPoland = "POLAND";
        public const string PosFinland = "FINLAND";
        public const string PosNorway = "NORWAY";
        public const string PosDenmark = "DENMARK";
        public const string PosTurkey = "TURKEY";

        public const string PosSingapore = "SINGAPORE";
        public const string PosHongKong = "HONG KONG";
        public const string PosPhilippines = "PHILIPPINES";

        // Devises
        public const string CurrencyEuro = "EUR";
        public const string CurrencyGBP = "GBP";
        public const string CurrencySEK = "SEK";
        public const string CurrencyCHF = "CHF";
        public const string CurrencyAUD = "AUD";
        public const string CurrencyYuan = "CNY";
        public const string CurrencyIndiaRoupie = "INR";
        public const string CurrencyCzechRepublicKoruna = "CZK";
        public const string CurrencyPolandZloty  = "PLN";
        public const string CurrencyNorwayNOK = "NOK";
        public const string CurrencyDenmarDKK = "DKK";
        public const string CurrencyTurkeyLira= "TRY";

        public const string CurrencySingaporeDollar = "SGD";
        public const string CurrencyHongKongDollar = "HKD";
        public const string CurrencyPhilippinesPeso = "PHP";

        // Historisation des changements dans la table des cartes Navision
        public const string HistCardOperationInsert = "INSERT";
        public const string HistCardOperationUpdate = "UPDATE";
        public const string HistCardOperationDelete = "DELETE";

        public const string HistCardCategoryBefore = "BEFORE";
        public const string HistCardCategoryAfter = "AFTER";

        // Services group Navision
        public const int ServiceGroupUNKNOWN = -1;
        public const int ServiceGroupALL = 0;
        public const int ServiceGroupAIR = 1;
        public const int ServiceGroupRAIL = 2;
        public const int ServiceGroupSEA = 3;
        public const int ServiceGroupHOTEL = 4;
        public const int ServiceGroupCAR = 5;
        public const int ServiceGroupLOWCOST = 6;
        public const int ServiceGroupOTHER = 7;
        public const int ServiceGroupVISA = 8;
        public const int ServiceGroupGROUND = 9;
        

        // Description des services supportés dans Navision
        public const string ServiceALL = "All";
        public const string ServiceAIR = "Air";
        public const string ServiceRAIL = "Rail";
        public const string ServiceSEA = "Sea";
        public const string ServiceHOTEL = "Hotel";
        public const string ServiceCAR = "Car";
        public const string ServiceLOWCOST = "Low Cost";
        public const string ServiceOTHER = "Other";
        public const string ServiceVisa = "Visa";
        public const string ServiceGround = "Ground";

        public static string[] Services = new string[] { ServiceALL, ServiceAIR, ServiceRAIL, ServiceSEA, ServiceHOTEL, ServiceCAR, ServiceLOWCOST, ServiceOTHER, ServiceVisa, ServiceGround };
        public static int ServicesCount = 10;

        // Formats de dates
        public const string DateFormat_yyyysMMsdd = "yyyy/MM/dd";
        public const string DateFormat_yyyyMMddXSD = "yyyy-MM-dd";
        public const string DateFormat_yyyyMMddHHmmss = "yyyyMMdd HH:mm:ss";
        public const string DateFormat_yyyyMMdd = "yyyyMMdd";
        public const string DateFormat_ddMMyyyyHHmmss = "dd/MM/yyyy HH:mm:ss";
        public const string DateFormat_ddMMyyyy = "ddMMyyyy";
        public const string DateFormat_MMyy = "MM/yy";
        public const string DateFormat_MMyyyy = "MM/yyyy";
        public const string DateFormat_MM = "MM";
        public const string DateFormat_YYYY = "yyyy";
        public const string ExpirationDateFormat = "dd/MM/yyyy";

        public const string LimitAccessForIPs = "LimitAccessForIPs";


        // Trace
        // Durée du traitement
        public const string Log_Duration = " - [Duration ms= {0}]";

        // Statut de la réponse
        public const string Status_Success = " - [Success] [-]";
        public const string Status_Failed = " - [Failed] [-]";
        public const string Status_Info = " - [Info] [-]";

        // Locale
        public const string LangEN = "en";
        public const string LangFR = "fr";

        // En-tête des réponses XML
        public const string XmlHeader = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";

        public const string ManualTransactional = "Manual transactional";
        public const string TransactionalCardReferenceStart = "T_";

        public const string LastAutomaticRunDateFileName = "lastAutomaticRunDate";

        public const string Space = " ";
        public const char Tab = '\u0009';
        public const string CR = "\r";
        public const string LF = "\n";

        // Les différents moyen de paiement des companies aériennes
        public enum CorporationPaymentType
        {
            ALL = 0,
            CASH = 1,
            CREDIT_CARD = 2,
        };

        public const string PaymentTypeStringALL = "ALL";
        public const string PaymentTypeStringCASH = "CASH";
        public const string PaymentTypeStringCASH_FR = "EC";
        public const string PaymentTypeStringCreditCard = "CREDIT_CARD";
        public const string PaymentTypeCreditCardShort = "CC";

        public static string[] CorporationPaymentTypeString =
            new string[]
        {
            PaymentTypeStringALL, 
            PaymentTypeStringCASH,
            PaymentTypeStringCreditCard
        };

        /// <summary>
        /// Returns invariante culture
        /// </summary>
        /// <returns>Invariante culture</returns>
        public static CultureInfo GetCulture()
        {
            return CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Returns webservice version
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            Assembly thisAssem = typeof(WSS).Assembly;
            return thisAssem.GetName().Version.ToString();
        }

        /// <summary>
        /// Retourne le nom de l'application
        /// </summary>
        /// <returns>Nom de l'application</returns>
        public static string GetApplicationName()
        {
            string nrServer=Util.Nvl(ConfigurationManager.AppSettings["FOTokenPrefixNrHost"], string.Empty);

            return String.Format("{0}_{1}_{2}", (nrServer.Equals("1")?"FO":"BO"), ApplicationName, GetVersion());
        }


        /// <summary>
        /// Retourne le nom du serveur
        /// </summary>
        /// <returns>Nom du serveur</returns>
        public static string GetServerName()
        {
            return HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
        }
    }

}
