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
using System.Diagnostics;
using System.Security;
using System.IO;
using System.Text;
using System.Net;
using System.Data.SqlClient;
using System.Configuration;
using SafeNetWS.login;
using SafeNetWS.creditcard;
using SafeNetWS.log;
using System.Runtime.InteropServices;


namespace SafeNetWS.utils
{

    /// <summary>
    /// A collection of utilities.
    /// </summary>
    public class Util
    {

        /// <summary>
        /// Giving back a nano date
        /// a 1/10 nano should be enaugh
        /// </summary>
        /// <returns>String in the ticks representation</returns>
        public static string GetNanoDateTimeNow()
        {
            string str = DateTime.Now.Ticks.ToString();
            // Let's return a 1/10 nano
            return str.Substring(0, str.Length - 3);
        }

        /// <summary>
        /// Calculate a checksum for an input string
        /// this function is used for the Luhn check
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="number">Chaine sur laquelle sera effectuée le calcul du checksum</param>
        /// <returns>Checksum (int)</returns>
        public static int CalculChecksum(UserInfo user, string number)
        {
            int checksum = 0;
            try
            {
                int[] DELTAS = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
                char[] chars = number.ToCharArray();
                int NrChars = chars.Length;
                for (int i = NrChars - 1; i > -1; i--)
                {
                    int j = ((int)chars[i]) - 48;
                    checksum += j;
                    if (((i - NrChars) % 2) == 0) checksum += DELTAS[j];
                }
             }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Util.CheckSum.Error", e.Message, true));
            }
            return checksum;
        }


        /// <summary>
        /// Un checksum est calculé pour le token
        /// Le calcul est celui effectué pour le test de Luhn
        /// On prend uniquement le premier digit
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="FormattedDateTime">Chaine de calcul</param>
        /// <returns>Checksum (string)</returns>
        public static string CalculChecksumForToken(UserInfo user, string FormattedDateTime)
        {
            string checksum=CalculChecksum(user, FormattedDateTime).ToString();
            // On returne le premier digit
            return checksum.Substring(0, 1);
        }


        /// <summary>
        /// Calcul d'un token BackOffice
        /// Le calcul est effectué à partir de la date et heure courante
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Checksum (long)</returns>
        public static long GenerateBOToken(UserInfo user)
        {
            long Token = Const.EmptyBoToken;
            try
            {
                // On récupérere la représentation en Nano 
                // de la date actuelle
                string FormattedDateTime = GetNanoDateTimeNow();
                // On calcul un checksum de la valeur
                // Ce check est utilisé pour le test de Luhn
                string Checksum = CalculChecksumForToken(user, FormattedDateTime);
                // On cancatène les 2 valeurs
                string TokenString = FormattedDateTime + Checksum;
                // On convertie la valeur en long
                Token= Util.ConvertStringToToken(TokenString);
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Util.GenerateToken.Error", e.Message, true));
            }
            return Token;
        }


        /// <summary>
        /// Calcul de la référence carte suivante
        /// pour les cartes corporates et privates
        /// </summary>
        /// <param name="refcard">Dernière référence carte</param>
        /// <param name="sharingType">Sharing type (private ou corporate)</param>
        /// <returns>Référence carte</returns>
        public static string GetNextReferenceProfilCard(string refcard, int sharingType)
        {
            string nexRefCard = null;
            if (sharingType == CardInfos.NavisionSharingTypePrivate)
            {
                if (String.IsNullOrEmpty(refcard)) return "1";
                // Carte privée
                // on commence à 1 et on incrémente de +1 ....
                int refPrivate = 1;
                try
                {
                    refPrivate = int.Parse(refcard) + 1;
                }
                catch (Exception) { } // ignorer cette exception
                nexRefCard = refPrivate.ToString();
            }
            else
            {
                if (String.IsNullOrEmpty(refcard)) return "A";
                // Carte corporate
                // Cela va de A à Z puis Z1, Z2, ...
                switch (refcard.ToUpper())
                {
                    case "A":
                        nexRefCard = "B";
                        break;
                    case "B":
                        nexRefCard = "C";
                        break;
                    case "C":
                        nexRefCard = "D";
                        break;
                    case "D":
                        nexRefCard = "E";
                        break;
                    case "E":
                        nexRefCard = "F";
                        break;
                    case "F":
                        nexRefCard = "G";
                        break;
                    case "G":
                        nexRefCard = "H";
                        break;
                    case "H":
                        nexRefCard = "I";
                        break;
                    case "I":
                        nexRefCard = "J";
                        break;
                    case "J":
                        nexRefCard = "K";
                        break;
                    case "K":
                        nexRefCard = "L";
                        break;
                    case "L":
                        nexRefCard = "M";
                        break;
                    case "M":
                        nexRefCard = "N";
                        break;
                    case "N":
                        nexRefCard = "O";
                        break;
                    case "O":
                        nexRefCard = "P";
                        break;
                    case "P":
                        nexRefCard = "Q";
                        break;
                    case "Q":
                        nexRefCard = "R";
                        break;
                    case "R":
                        nexRefCard = "S";
                        break;
                    case "S":
                        nexRefCard = "T";
                        break;
                    case "T":
                        nexRefCard = "U";
                        break;
                    case "U":
                        nexRefCard = "V";
                        break;
                    case "V":
                        nexRefCard = "W";
                        break;
                    case "W":
                        nexRefCard = "X";
                        break;
                    case "X":
                        nexRefCard = "Y";
                        break;
                    case "Y":
                        nexRefCard = "Z";
                        break;
                    default:
                        // On suppose Z...
                        int refCoporate = 1;
                        try
                        {
                            refCoporate = int.Parse(refcard.Substring(1,refcard.Length-1)) + 1;
                        }
                        catch (Exception) { } // ignorer cette exception
                        nexRefCard = "Z" + refCoporate.ToString();
                        break;
                }
            }


            return nexRefCard;
        }


        /// <summary>
        /// Récupération de la date sans l'heure
        /// pour une date passée en paramètre
        /// </summary>
        /// <param name="inDate">Date à traiter</param>
        /// <returns>Date sans heure</returns>
        public static DateTime getDateOnly(DateTime inDate)
        {
            string dateString = inDate.ToString(Const.DateFormat_yyyyMMdd);
            if(dateString.StartsWith("1900")) dateString="1753-01-01";

            return DateTime.ParseExact(dateString, Const.DateFormat_yyyyMMdd, null);
        }

        /// <summary>
        /// Retourne la date vide en Navision
        /// </summary>
        /// <returns>Date vide</returns>
        public static DateTime GetNavisionEmptyDate()
        {
            return Util.ConvertStringToDate(Const.NavisionEmptyDateString, Const.DateFormat_ddMMyyyyHHmmss);
        }


        /// <summary>
        /// Récupération du dernier 
        /// jour du mois de la date fournie
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Dernier jour du mois</returns>
        public static DateTime GetLastDayOfThisMonth(DateTime date)
        {
            int nbDays = DateTime.DaysInMonth(date.Year, date.Month);

            return new DateTime(date.Year, date.Month, nbDays);
        }

        /// <summary>
        /// Récupération du mois et de l'année d'une date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>MM/yy</returns>
        public static string GetShortExpirationDate(DateTime date)
        {
            return date.ToString(Const.DateFormat_MMyy);
        }

        /// <summary>
        /// Calcul de la référence carte suivante
        /// pour les cartes transactionnelle
        /// Le grain sera la nanoseconde (la milliseconde serait amplement suffisant)
        /// afin d'insérer plusieurs cartes pour le même traveller dans la même minute.
        /// </summary>
        /// <param name="traveller">Identifiant voyageur (percode)</param>
        /// <returns>Référence carte</returns>
        public static string GetNextTransactionalCardReference(string traveller)
        {
            return Const.TransactionalCardReferenceStart + Util.GetNanoDateTimeNow();
        }

        /// <summary>
        /// Sanity check on the POS
        /// </summary>
        /// <param name="user">usern</param>
        /// <param name="pos">Pos navision</param>
        /// <returns>Pos corrigé</returns>
        public static string CorrectPos(UserInfo user,string pos)
        {
            if (String.IsNullOrEmpty(pos))
            {
                // pos code is missing, we can not continue
                return null;// throw new Exception(user.GetMessages().GetString("PosEmpty", true));
            }
            string retval = pos;
            switch (pos.ToUpper())
            {
                case "FRANCE":
                case "FRENCH":
                case "FR":
                case "FRA":
                case "FR-FR":
                    retval = Const.PosFrance;
                    break;
                case "ENGLISH":
                case "UK":
                case "GB":
                case "GBR":
                case "EN":
                case "EN-US":
                case "EN-GB":
                    retval = Const.PosUK;
                    break;
                case "BELGIUM":
                case "BE":
                case "BEL":
                    retval = Const.PosBelgium;
                    break;
                case "GERMANY":
                case "GE":
                case "DE":
                case "DEU":
                case "DE-DE":
                case "GERMAN":
                    retval = Const.PosGermany;
                    break;
                case "SPAIN":
                case "SP":
                case "ES":
                case "ES-ES":
                case "ESP":
                    retval = Const.PosSpain;
                    break;
                case "IRELAND":
                case "IE":
                case "IR":
                case "IRL":
                    retval = Const.PosIreland;
                    break;
                case "ITALY":
                case "IT":
                case "ITA":
                    retval = Const.PosItaly;
                    break;
                case "NETHERLANDS":
                case "NL":
                case "NL-NL":
                case "NLD":
                    retval = Const.PosNetherlands;
                    break;
                case "SWITZERLAND":
                case "CH":
                case "CHE":
                    retval = Const.PosSwitzerland;
                    break;
                case "SWEDEN":
                case "SE":
                case "SE-SE":
                case "SWE":
                    retval = Const.PosSweden;
                    break;
                case "AUSTRALIA":
                case "AU":
                case "AUS":
                    retval = Const.PosAustralia;
                    break;
                case "CHINA":
                case "CN":
                case "CHN":
                    retval = Const.PosChina;
                    break;
                case "INDIA":
                case "IN":
                case "IND":
                    retval = Const.PosIndia;
                    break;
                case "CZECH REPUBLIC":
                case "CZECH":
                case "CZ":
                case "CZE":
                    retval = Const.PosCzechRepublic;
                    break;
                case "POLAND":
                case "PL":
                case "POL":
                    retval = Const.PosPoland;
                    break;
                case "DENMARK":
                case "DK":
                case "DNK":
                    retval = Const.PosDenmark;
                    break;
                case "FINLAND":
                case "FI":
                case "FIN":
                    retval = Const.PosFinland;
                    break;
                case "NORWAY":
                case "NO":
                case "NOR":
                    retval = Const.PosNorway;
                    break;
                case "TURKEY":
                case "TR":
                case "TUR":
                    retval = Const.PosTurkey;
                    break;
                case "SG":
                case "SGP":
                case "SINGAPORE":
                    retval = Const.PosSingapore;
                    break;
                case "PH":
                case "PHL":
                case "PHILIPPINES":
                    retval = Const.PosPhilippines;
                    break;
                case "HK":
                case "HKG":
                case "HONG KONG":
                    retval = Const.PosHongKong;
                    break;
                default:
                   throw new Exception(user.GetMessages().GetString("UnknowPOS", pos, true));
            }
            return retval;
        }



        /// <summary>
        /// Correction de la langue
        /// </summary>
        /// <param name="lang">Langue</param>
        /// <returns>Langue corrigée</returns>
        public static string CorrectLang(string lang)
        {
            string retval = lang;
            switch (lang.ToLower())
            {
                case "fr":
                case "fr_fr":
                case "fr-fr":
                case "france":
                case "french":
                case "français":
                    retval = "fr_FR";
                    break;
                case "de":
                case "de_de":
                case "de-de":
                case "germany":
                case "ge":
                    retval = "de_DE";
                    break;
                case "spain":
                case "sp":
                case "es":
                case "spanish":
                case "es_es":
                case "es-es":
                    retval = "es_ES";
                    break;
                case "italy":
                case "it":
                case "it_it":
                case "it-it":
                    retval = "it_IT";
                    break;
                case "netherlands":
                case "nl":
                case "nl_nl":
                case "nl-nl":
                    retval = "nl_NL";
                    break;
                case "sweden":
                case "se":
                case "se_se":
                case "se-se":
                case "sw":
                case "sv_se":
                    retval = "se_SE";
                    break;
                case "portuguese":
                case "portugal":
                case "pt":
                case "pt_pt":
                case "pt-pt":
                    retval = "pt_PT";
                    break;
                default:
                    retval = "en_US";
                    break;
            }
            return retval;
        }



        /// <summary>
        /// Convert a string to a date
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="value">Representation of Expiration date</param>
        /// <returns>DateTime</returns>
        public static DateTime ConvertExpirationDateToDate(UserInfo user, string value)
        {

            // Quelques contrôles sur la date
            // Au moins 3 caractères
            if (value.Length < 3) throw new Exception(user.GetMessages().GetString("UnvalidExpirationDate", true));

            int IndexOfSep=value.IndexOf('/');
            if (IndexOfSep == -1) throw new Exception(user.GetMessages().GetString("UnvalidExpirationDate", true));

            int LastIndexOfSep = value.LastIndexOf('/');
            
            string ExpDateString = value;

            if (LastIndexOfSep == IndexOfSep)
            {
                // On a la date sous la forme mois/annee
                // MM/yyyy ou M/yyyy ...
                // Extraction du mois et de l'année
                // 
                string MonthPart = value.Substring(0, IndexOfSep);
                IndexOfSep++;
                string YearPart = value.Substring(IndexOfSep, value.Length - IndexOfSep);
                if (MonthPart.Length == 1)
                {
                    MonthPart = "0" + MonthPart;
                }
                if (YearPart.Length == 2)
                {
                    YearPart = "20" + YearPart;
                }
                ExpDateString = "01/" + MonthPart + "/" + YearPart;
            }
            else
            {
                // On a une date sous le format dd/MM/yyyy
                string DayPart = value.Substring(0, IndexOfSep);
                IndexOfSep++;
                string MonthPart = value.Substring(IndexOfSep, value.Length-IndexOfSep-LastIndexOfSep);
                LastIndexOfSep++;
                string YearPart = value.Substring(LastIndexOfSep, value.Length - LastIndexOfSep);

                if (DayPart.Length == 1)
                {
                    DayPart = "0" + DayPart;
                }
                if (MonthPart.Length == 1)
                {
                    MonthPart = "0" + MonthPart;
                }

                if (YearPart.Length == 2)
                {
                    YearPart = "20" + YearPart;
                }

                ExpDateString = DayPart + "/" + MonthPart + "/" + YearPart;
            }
            try
            {
                // On converti la chaine en date
                // et on retourne le dernier jour du mois
                return GetLastDayOfThisMonth(DateTime.ParseExact(ExpDateString, Const.ExpirationDateFormat, Const.GetCulture()));
            }
            catch (Exception)
            {
                throw new Exception(user.GetMessages().GetString("UnvalidExpirationDate", true));
            }
        }
  
        /// <summary>
        /// Encodage HTML
        /// </summary>
        /// <param name="text">Texte</param>
        /// <returns>Texte encodé (& --> &amp;)</returns>
        public static string HtmlEncode(string text)
        {
            return text.Replace("&", "&amp;");
        }

        /// <summary>
        /// Décodage HMTL
        /// </summary>
        /// <param name="text">Texte encodé</param>
        /// <returns>Texte décodé (&amp; --> &)</returns>
        public static string HtmlDecode(string text)
        {
            return text.Replace("&amp;", "&");
        }

        /// <summary>
        /// Build filename (combine with folder)
        /// </summary>
        /// <param name="folderName">Foldername</param>
        /// <param name="fileName">Short filename</param>
        /// <returns>Full filename</returns>
        public static string BuildFileName(string folderName, string fileName)
        {
            return Path.Combine(folderName, fileName);
        }


        /// <summary>
        /// Compute the duration between the initial and the end time. 
        /// and return the result in milliseconds.
        /// </summary>
        /// <param name="startDate">Initial date</param>
        /// <returns>Duration in milliseconds</returns>
        public static double GetDuration(DateTime startDate)
        {
            TimeSpan duration = DateTime.Now - startDate;
            return duration.TotalMilliseconds;
        }


        /// <summary>
        /// Converts a string to Token (long). 
        /// </summary>
        /// <param name="value">Token (string)</param>
        /// <returns>Token (long)</returns>
        public static long ConvertStringToToken(string value)
        {
            return Int64.Parse(value, Const.GetCulture());
        }

        /// <summary>
        /// Converts a string to long. 
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>long)</returns>
        public static long ConvertStringToLong(string value)
        {
            return Int64.Parse(value, Const.GetCulture());
        }

        /// <summary>
        /// Converts a string to long. 
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="defaultValue">long</param>
        /// <returns>long)</returns>
        public static object TryConvertStringToLong(string value, object defaultValue)
        {
            try
            {
                return Int64.Parse(value, Const.GetCulture());
            }
            catch { }
            return defaultValue;
        }

        /// <summary>
        /// Converts a string to long. 
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="defaultValue">int</param>
        /// <returns>int</returns>
        public static object TryConvertStringToInt(string value, object defaultValue)
        {
            try
            {
                return Int32.Parse(value, Const.GetCulture());
            }
            catch { }
            return defaultValue;
        }


        /// <summary>
        /// Converts a string to double.
        /// </summary>
        /// <param name="value">Input value in string</param>
        /// <returns>Value in double</returns>
        public static double ConvertStringToDouble(string value)
        {
            return double.Parse(value, Const.GetCulture());
        }

        /// <summary>
        /// Converts a string to long. 
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="defaultValue">long</param>
        /// <returns>long)</returns>
        public static object TryConvertStringToDouble(string value, object defaultValue)
        {
            try
            {
                return double.Parse(value, Const.GetCulture());
            }
            catch { }
            return defaultValue;
        }



        /// <summary>
        /// Convert a string to int.
        /// </summary>
        /// <param name="value">input string</param>
        /// <returns>returns value in integer</returns>
        public static int ConvertStringToInt(string value)
        {
            return int.Parse(value, Const.GetCulture());
        }


        /// <summary>
        /// Converts a Token to string. 
        /// </summary>
        /// <param name="value">Token in string</param>
        /// <returns>Token in long</returns>
        public static string ConvertTokenToString(long value)
        {
            if (Util.IsEmptyToken(value))
            {
                return string.Empty;
            }
            return value.ToString();
        }

        /// <summary>
        /// Converts an integer to boolean
        /// </summary>
        /// <param name="value">integer</param>
        /// <returns>boolean</returns>
        public static bool ConvertIntToBool(int value)
        {
            return value == 1 ? true : false;
        }

        /// <summary>
        /// Converts a string to boolean
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>boolean</returns>
        public static bool ConvertStringToBool(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                return false;
            }
            return (value.Equals("1") 
                || String.Compare(value, "true", true) == 0
                || String.Compare(value, "on", true) == 0
                || String.Compare(value, "ok", true) == 0
                || String.Compare(value, "1", true) == 0 
                || String.Compare(value, "yes", true) == 0);
        }


        /// <summary>
        /// Retourne du nom complet du fichier dans lequel 
        /// sera enregistrée la date de suppression des fichiers contenant
        /// le nombre de visualisation de numeros de cartes 
        /// </summary>
        /// <returns>Nom du fichier complet</returns>
        public static String GetLastAutomaticRunDateFileName()
        {
            return Const.LastAutomaticRunDateFileName; 
        }

        /// <summary>
        /// Vérification existant de fichier 
        /// </summary>
        /// <param name="fileName">Nom complet du fichier</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool FileExists(string fileName)
        {
            bool retval = false;
            try
            {
                FileInfo infosFile = new FileInfo(Microsoft.Security.Application.Encoder.UrlPathEncode(fileName));
                 if (infosFile.Exists)
                 {
                     retval = true;
                 }
            }
            catch (Exception)
            {
                // On ignore cette exception
                // Impossible de vérifier
            }
            return retval;
        }

        /// <summary>
        /// Cette fonction met à jour la chaine de connexion
        /// en prenant en compte l'utilisation de pool de connexion
        /// et rajoute certaines autres fonctions
        /// </summary>
        /// <param name="ConnString">Chaine de connexion</param>
        /// <param name="MaxPoolSize">Valeur maximale du pool</param>
        /// <param name="MinPoolSize">Valeur minimal du pool</param>
        /// <returns>Chaine de connexion mise à jour</returns>
        public static string BuildSQLConnectionString(string ConnString, string MaxPoolSize, string MinPoolSize)
        {
            string retval = ConnString;
            // On met à jour la chaine de connexion
            // pour prendre en compte le pool de connexion

            // Nombre maximal de connexions dans un pool
            if (!String.IsNullOrEmpty(MaxPoolSize) && !MaxPoolSize.Equals("0"))
            {
                // On doit utiliser un pool de connexion
                retval += String.Format("; max pool size={0}", MaxPoolSize);

                // récupération de la valeur minimale
                // pour définir un pool de connexions
                if (!String.IsNullOrEmpty(MinPoolSize) && !MinPoolSize.Equals("0"))
                {
                    // La valeur minimale a été spécifiée
                    retval += String.Format("; min pool size={0}", MinPoolSize);
                }
                else
                {
                    // la valeur minimale n'a pas été spécifiée
                    retval += String.Format("; min pool size={0}", 0);
                }
            }
            else
            {
                // On n'utilise pas de pool de connexions
                retval += String.Format("; Pooling=False");
            }
   
            
            //On met le nom de l'application
            // Cette information apparait au niveau du profiler SQL
            retval+= String.Format("; {0}{1}","Application Name=", Const.GetApplicationName());

            // On retourne la chaine de connexion mise à jour
            return retval;
        }
    
        /// <summary>
        /// Génération d'un token Front
        /// Le token est en réalité un GUID
        /// que l'on va préfixé avec le numéro de serveur (0, 1, ...)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Token (String)</returns>
        public static string GenerateFOToken(UserInfo user)
        {
            try
            {
                // On génère une valeur unique dans le temps et 
                // dans l'espace
                // Afin d'éviter une collision lorsque le token est généré sur plusieurs
                // serveurs, on va ajouter comme préfixe le numéro du serveur
                return  Util.Nvl(ConfigurationManager.AppSettings["FOTokenPrefixNrHost"], string.Empty)
                    + "_" + GetNewGuid(); 
               
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Util.GenerateToken.Error", e.Message, true));
            }
        }

        /// <summary>
        /// On génère une valeur unique dans le temps et
        /// dans l'espace
        /// </summary>
        /// <returns>Guid (String)</returns>
        public static string GetNewGuidValue()
        {
            return GetNewGuid().ToString();
        }

        /// <summary>
        /// On génère une valeur unique dans le temps et
        /// dans l'espace
        /// </summary>
        /// <returns>Guid</returns>
        public static Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        /// Verification si une adresse IP appartient
        /// à une page d'adresses IP (start, end)
        /// </summary>
        /// <param name="source">Adresse IP à vérifier</param>
        /// <param name="start">Début de la plage d'adresses IP</param>
        /// <param name="end">Fin de la plage d'adresses IP</param>
        /// <returns>TRUE si l'adresse appartient bien à la plage</returns>
        public static bool IPIsInRange(string source, string start, string end)
        {
            try
            {
                IPAddress iPAddressEnd = IPAddress.Parse(end);
                if (IPAddress.IsLoopback(iPAddressEnd) == false)
                {
                    return (IpInUint(source.Split(new char[] { '.' })) >= IpInUint(start.Split(new char[] { '.' })) && IpInUint(source.Split(new char[] { '.' })) <= IpInUint(end.Split(new char[] { '.' })));
                }
                return true;
            }
            catch (Exception)
            {
                // Erreur lors du traitement
                // au lieu de reporter cela, on ignore pas la negative
                return false;
            }
        }

        private static UInt32 IpInUint(string[] s)
        {
            return (Convert.ToUInt32(s[0]) << 24) | (Convert.ToUInt32(s[1]) << 16)  | (Convert.ToUInt32(s[2]) << 8)  | (Convert.ToUInt32(s[3]));
        }

        /// <summary>
        /// Convertion d'une chaîne de caractères en date
        /// </summary>
        /// <param name="value">Date en chaine de caractères</param>
        /// <param name="format">Formattage</param>
        /// <returns>Date</returns>
        public static DateTime ConvertStringToDate(string value, string format)
        {
            return DateTime.ParseExact(value, format, Const.GetCulture());
        }

        

        /// <summary>
        /// Convertie une date en chaînes de caractères
        /// suivant un format
        /// </summary>
        /// <param name="inDate">Date a convertir</param>
        /// <param name="format">Format de conversion</param>
        /// <returns></returns>
        public static string ConvertDateToString(DateTime inDate, string format)
        {
            if (Util.IsEmptyDate(inDate)) return string.Empty;
            return inDate.ToString(format);
        }

        /// <summary>
        /// Retourne une valeur booléenne depuis un champ
        /// </summary>
        /// <param name="dr">SQL data reader</param>
        /// <param name="fieldName">Nom du champ</param>
        /// <returns>boolean</returns>
        public static bool GetSQLBoolean(SqlDataReader dr, string fieldName)
        {
            return dr.GetBoolean(dr.GetOrdinal(fieldName));
        }

        /// <summary>
        /// Retourne la date depuis un champ
        /// </summary>
        /// <param name="dr">SQL data reader</param>
        /// <param name="fieldName">Nom du champ</param>
        /// <returns>DateTime</returns>
        public static DateTime GetSQLDataTime(SqlDataReader dr, string fieldName)
        {
            try
            {
                return dr.GetDateTime(dr.GetOrdinal(fieldName));
            }
            catch (Exception e)
            {
                // On a une erreur
                // Il faut vérifier si la valeur est nulle
                // car dans ce cas il faut renvoyer DateTime.MinValue
                string value = dr[fieldName].ToString();
                if(String.IsNullOrEmpty(value))
                {
                    return Const.EmptyDate;
                }
                // La date n'est pas vide
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Retourne la date depuis un champ
        /// </summary>
        /// <param name="dr">SQL data reader</param>
        /// <param name="fieldName">Nom du champ</param>
        /// <returns>Integer</returns>
        public static int GetSQLInt(SqlDataReader dr, string fieldName)
        {
            return dr.GetInt32(dr.GetOrdinal(fieldName));
        }

        /// <summary>
        /// Retourne la date depuis un champ
        /// </summary>
        /// <param name="dr">SQL data reader</param>
        /// <param name="fieldName">Nom du champ</param>
        /// <returns>Decimal</returns>
        public static decimal GetSQLDecimal(SqlDataReader dr, string fieldName)
        {
            return dr.GetDecimal(dr.GetOrdinal(fieldName));
        }

        /// <summary>
        /// Retourne la date d'expiration en chaîne de caractères
        /// sous le format ddMMyyyy
        /// </summary>
        /// <param name="expirationDate">Date d'expiration (DateTime)</param>
        /// <returns></returns>
        public static string ConvertExpirationDateToString(DateTime expirationDate)
        {
            return Util.ConvertDateToString(expirationDate, Const.DateFormat_ddMMyyyyHHmmss);
        }

        /// <summary>
        /// Différence en jours entre deux dates
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <returns>jours</returns>
        public static int DateDiffInDays(DateTime startDate, DateTime endDate)
        {
            TimeSpan Diff = endDate - startDate;
             return (int)Diff.TotalDays;
        }

        /// <summary>
        /// Différence en jours entre deux dates
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <returns>Annees</returns>
        public static int DateDiffInYears(DateTime startDate, DateTime endDate)
        {
            return endDate.Year - startDate.Year;
        }


        /// <summary>
        /// Cette fonction permet de vérifier si
        /// une option est activée
        /// </summary>
        /// <param name="option">Nom de l'option</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool IsOptionOn(string optionName)
        {
            // On récupère la valeur de l'option
            string option = ConfigurationManager.AppSettings[optionName];

            if (String.IsNullOrEmpty(option)) return false;
            return (String.Compare(option, Const.On, true) == 0);
        }

        /// <summary>
        /// Convert the byte array back to a true string
        /// </summary>
        /// <param name="bytes_Input">Bytes</param>
        /// <returns>Chaine encoding</returns>
        public static string ToHexString(byte[] data)
        {
            byte b;
            int i, j, k;
            int l = data.Length;
            char[] r = new char[l * 2];
            for (i = 0, j = 0; i < l; ++i)
            {
                b = data[i];
                k = b >> 4;
                r[j++] = (char)(k > 9 ? k + 0x37 : k + 0x30);
                k = b & 15;
                r[j++] = (char)(k > 9 ? k + 0x37 : k + 0x30);
            }
            return new string(r);
        } 

        /// <summary>
        /// Protection d'une chaine XML
        /// en effectuant les remplacements suivants
        /// < --> &lt;
        /// > --> &glt;
        /// " --> &quot;
        /// ' --> &apos;
        /// & --> &amp;
        /// </summary>
        /// <param name="inputString">Chaine XML</param>
        /// <returns>Chaine de retour</returns>
        public static string XMLEscape(string inputString)
        {
            if (String.IsNullOrEmpty(inputString)) return inputString;
            return SecurityElement.Escape(inputString);
        }


        /// <summary>
        /// Génération d'un ID unique VPayment
        /// de 15 caractères
        /// 1-2 : Type d'application (BO ou FO)
        /// 3-12 : Chaine aléatoire
        /// 13-15 : Checksum de la chaine de caractères 
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>ID VPayment</returns>
        public static string GenerateVPaymentID()
        {
            // Récupération d'une chaine min GUID de 1é caractères
            string randomString = GetUniqueKey(12).ToUpper(); //RandomString(12, false);
            // On construit l'ID VPayment
            // en ajoutant au GUID une checksum de pondération
            randomString = randomString + CalculateAlphaNumberWithPadding(randomString);
            
            return randomString;
        }

        /// <summary>
        /// Calcul la pondération d'une chaine
        /// et complète le résultat par des 0 jusqu'à 3 digits
        /// </summary>
        /// <param name="randomString">Chaine aléatoires</param>
        /// <returns>Pondération</returns>
        public static string CalculateAlphaNumberWithPadding(string randomString)
        {
            return CalculateAlphaNumber(randomString).ToString().PadLeft(3, '0');
        }


        /// <summary>
        /// Calcul la pondération pour
        /// une chaine de caractères
        /// </summary>
        /// <param name="input">Chaine de caractères</param>
        /// <returns>Poids</returns>
        private static int CalculateAlphaNumber(string input)
        {
            int check = 0;
            if (!String.IsNullOrEmpty(input))
            {
                for (int i = 0; i < input.Length; i++)
                {
                    check += i + GetAlphaNumber(input[i]);
                }
            }
            return check;
        }



        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
       /* private static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch ;
            for(int i=0; i<size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))) ;
                builder.Append(ch); 
            }
            if(lowerCase)
            return builder.ToString().ToLower();
            return builder.ToString();
        }*/


        /// <summary>
        /// Retourne un poids pour chaque lettre
        /// de l'alphabet
        /// </summary>
        /// <param name="input">Lettre</param>
        /// <returns>Poids</returns>
        private static int GetAlphaNumber(char input)
        {
            switch (input)
            {
                case 'A':
                    return 0;
                case 'B':
                    return 1;
                case 'C':
                    return 2;
                case 'D':
                    return 3;
                case 'E':
                    return 4;
                case 'F':
                    return 5;
                case 'G':
                    return 6;
                case 'H':
                    return 7;
                case 'I':
                    return 8;
                case 'J':
                    return 9;
                case 'K':
                    return 10;
                case 'L':
                    return 11;
                case 'M':
                    return 12;
                case 'N':
                    return 13;
                case 'O':
                    return 14;
                case 'P':
                    return 15;
                case 'Q':
                    return 16;
                case 'R':
                    return 17;
                case 'S':
                    return 18;
                case 'T':
                    return 19;
                case 'U':
                    return 20;
                case 'V':
                    return 21;
                case 'W':
                    return 22;
                case 'X':
                    return 23;
                case 'Y':
                    return 24;
                case 'Z':
                    return 25;
                default:
                    return 0;
            }

        }

        /// <summary>/// Gets a unique key based on GUID string hash code.
        /// </summary>
        /// <param name="length">The length of the key returned.</param>
        /// <returns>Unique key returned.</returns>
        public static string GetUniqueKey(int length)
        {    
            string guidResult = string.Empty;        
            while (guidResult.Length < length)    
            {        
                // Get the GUID.        
                guidResult += Guid.NewGuid().ToString().GetHashCode().ToString("x");    
            }    
            // Make sure length is valid.    
            if (length <= 0 || length > guidResult.Length)  throw new ArgumentException("Length must be between 1 and " + guidResult.Length);    
           
            // Return the first length bytes.    
            return guidResult.Substring(0, length);
        }

        /// <summary>
        /// Returns content from a file
        /// </summary>
        /// <param name="filePath">filename path</param>
        /// <param name="removeCRLF">Remove CR and LF</param>
        /// <returns></returns>
        public static string GetContentDispayCardsFilesFolder(string filePath, bool removeCRLF)
        {
            string content = null;
            StreamReader r = null;
            try
            {
                // Get folder from settings
                DirectoryInfo di = new DirectoryInfo(@ConfigurationManager.AppSettings["DisplayCardsFilesFolder"]);
                // get files
                FileInfo[] files = di.GetFiles(filePath);
                // We need only the filt file as it will be one file
                if (files.Length == 0) return content;
                // get file info
                FileInfo infosFile = files[0];

                // Let's open a stream reader 
                r = infosFile.OpenText();
                if (r != null)
                {
                    // stream reader open...
                    // let's read file entil the end...
                    content = r.ReadToEnd();
                    if (removeCRLF)
                    {
                        // We need to remove CR
                        content = content.Replace(Const.CRLF, string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Erreur getting content from file [{0}]!Error: {1}", filePath, e.Message));
            }
            finally
            {
                if (r != null)
                {
                    // et on libère les ressources
                    r.Dispose();
                }
            }
            return content;
        }

        /// <summary>
        /// Save content to a file
        /// </summary>
        /// <param name="filePath">Full filename path</param>
        /// <param name="content"></param>
        public static void SaveContentToFile(string filePath, string content)
        {
            // Get folder from settings
            string filename = BuildFileName(@ConfigurationManager.AppSettings["DisplayCardsFilesFolder"], filePath);

            StreamWriter w = null;
            try
            {
                w = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.Default);
                w.AutoFlush = true;
                w.WriteLine(content);
            }
            catch (Exception e)
            {
                Logger.WriteErrorToLog(String.Format("Error writing to file [{0}]! Error : {1}", filePath, e.Message));
            }
            finally
            {
                if (w != null)
                {
                    // et on libère les ressources
                    w.Dispose();
                }
            }
        }


        /// <summary>
        /// Implements Oracle style NVL function
        /// </summary>
        /// <param name="source">The source argument</param>
        /// <param name="def">The default value in case source is null or the length of the string is 0</param>
        /// <returns>Source if source is not null, otherwise return def</returns>
        public static String Nvl(String source, String def)
        {
            if (String.IsNullOrEmpty(source)) return def;
            return source;
        }

        /// <summary>
        /// Implements Oracle style NVL function
        /// </summary>
        /// <param name="source">The source argument</param>
        /// <param name="def">The default value in case source is null or the length of the string is 0</param>
        /// <returns>Source if source is not null, otherwise return def</returns>
        public static object Nvl(object source, object def)
        {
            if (source == null) return def;
            return source;
        }


        /// <summary>
        /// Retourne le service a partir d'un service Navision
        /// </summary>
        /// <param name="serviceGroup">Service Navision (1, 2, ...)</param>
        /// <returns>Service (Air, Rail , ...)</returns>
        public static string GetService(int serviceGroup)
        {
            string retval = Const.ServiceALL;
            switch (serviceGroup)
            {
                case Const.ServiceGroupAIR: retval = Const.ServiceAIR; break;
                case Const.ServiceGroupRAIL: retval = Const.ServiceRAIL; break;
                case Const.ServiceGroupSEA: retval = Const.ServiceSEA; break;
                case Const.ServiceGroupHOTEL: retval = Const.ServiceHOTEL; break;
                case Const.ServiceGroupCAR: retval = Const.ServiceCAR; break;
                case Const.ServiceGroupLOWCOST: retval = Const.ServiceLOWCOST; break;
                case Const.ServiceGroupOTHER: retval = Const.ServiceOTHER; break;
                default: break;
            }

            return retval;
        }
        /// <summary>
        /// Retourne la devise depuis le POS
        /// </summary>
        /// <param name="pos">Pos</param>
        /// <returns>Devise</returns>
        public static string GetCurrencyFromPos(string pos)
        {
            string retval = Const.CurrencyEuro;
            switch (pos)
            {
                case Const.PosUK:
                    retval = Const.CurrencyGBP;
                    break;
                case Const.PosSwitzerland:
                    retval = Const.CurrencyCHF;
                    break;
                case Const.PosSweden:
                    retval = Const.CurrencySEK;
                    break;
                case Const.PosAustralia:
                    retval = Const.CurrencyAUD;
                    break;
                case Const.PosChina:
                    retval = Const.CurrencyYuan;
                    break;
                case Const.PosIndia:
                    retval = Const.CurrencyIndiaRoupie;
                    break;
                case Const.PosCzechRepublic:
                    retval = Const.CurrencyCzechRepublicKoruna;
                    break;
                case Const.PosPoland:
                    retval = Const.CurrencyPolandZloty;
                    break;
                case Const.PosNorway:
                    retval = Const.CurrencyNorwayNOK;
                    break;
                case Const.PosDenmark:
                    retval = Const.CurrencyDenmarDKK;
                    break;
                case Const.PosTurkey:
                    retval = Const.CurrencyTurkeyLira;
                    break;
                case Const.PosPhilippines:
                    retval = Const.CurrencyPhilippinesPeso;
                    break;
                case Const.PosHongKong:
                    retval = Const.CurrencyHongKongDollar;
                    break;
                case Const.PosSingapore:
                    retval = Const.CurrencySingaporeDollar;
                    break;
                default: break;
            }

            return retval;
        }
        

        /// <summary>
        /// Récupération de la clé permettant de retourner
        /// la liste d'adresses IP autorisées (s'il y en a une)
        /// pour l'application qui sollicite ce service
        /// </summary>
        /// <param name="application"></param>
        /// <returns>clé paramètre</returns>
        public static string GetLimitAccessForIPsSetting(string application)
        {
            if (String.IsNullOrEmpty(application))
            {
                // Le nom de l'application n'a pas ete fourni!
                // Il ne faut pas aller plus loin
                return null;
            }
            return ConfigurationManager.AppSettings[String.Format("{0}:{1}", Const.LimitAccessForIPs, application)];
        }

        /// <summary>
        /// Retourne TRUE si le service est un service principal
        /// AIR
        /// RAIL
        /// HOTEL
        /// LOWCOST
        /// </summary>
        /// <param name="service">Service</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool IsPrincipalService(int service)
        {
            switch (service)
            {
                case Const.ServiceGroupAIR:
                case Const.ServiceGroupRAIL:
                case Const.ServiceGroupHOTEL:
                case Const.ServiceGroupLOWCOST:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Vérification de services
        /// Si un service est invalide, alors
        /// une exception sera levée
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="service">Liste de services (séparée par des virgules)</param>
        /// <returns>Liste des services validés</returns>
        public static string[] CorrectServices(UserInfo user, string serviceList)
        {
            if (String.IsNullOrEmpty(serviceList))
            {
                // On retourne tous les services
                return Const.Services;
            }

            string[] services = serviceList.Split(',');
            int NrServices = services.Length;

            // on prépare la liste des services validés
            string[] retval = new string[NrServices];

            for (int i = 0; i < NrServices; i++)
            {
                if (!String.IsNullOrEmpty(services[i]))
                {
                    // On récupère le service
                    string service = services[i].Trim();
                    // On récupère le service
                    // s'il n'existe pas, alors une exception sera levée
                    retval[i] = CorrectService(user, service);
                }
            }
            return retval;
        }


        /// <summary>
        /// Vérification si le service est valide
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="service">Service</param>
        /// <returns>Service validé</returns>
        public static string CorrectService(UserInfo user, string service)
        {
            if (String.IsNullOrEmpty(service)) return Const.ServiceALL;
            //>> EGE-59351 : CCE- Payment means - Handle "low" and "low cost"
            if (String.Compare(service, "low", true) == 0) return Const.ServiceLOWCOST;
            if (String.Compare(service, "low cost", true) == 0) return Const.ServiceLOWCOST;
            //<< EGE-59351 : CCE- Payment means - Handle "low" and "low cost" 

            for (int i=0; i< Const.ServicesCount; i++)
            {
                if (String.Compare(Const.Services[i], service, true) == 0)
                {
                    return Const.Services[i];
                }
            }
            // Le service est inconnu!
            throw new Exception(user.GetMessages().GetString("GetNavisionServiceGroup.UnknownService", service, true));
        }

        /// <summary>
        /// Retourne le service de Navision
        /// à partir du service (AIR, RAIL, ...)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="service">Service</param>
        /// <returns>Service Navision (1, 2, ...)</returns>
        public static int GetNavisionServiceGroup(UserInfo user, string service)
        {
            int retval = Const.ServiceGroupUNKNOWN;
            switch (Util.Nvl(service, string.Empty).ToUpper())
            {
                case "AIR": retval = Const.ServiceGroupAIR; break;
                case "RAIL": retval = Const.ServiceGroupRAIL; break;
                case "SEA": retval = Const.ServiceGroupSEA; break;
                case "HOTEL": retval = Const.ServiceGroupHOTEL; break;
                case "CAR": retval = Const.ServiceGroupCAR; break;
                case "LOW":
                case "LOWCOST":
                case "LOW COST": retval = Const.ServiceGroupLOWCOST; break;
                case "OTHER": retval = Const.ServiceGroupOTHER; break;
                case "ALL": retval = Const.ServiceGroupALL; break;
                case "": retval = Const.ServiceGroupALL; break;
                default: break;
            }
            if (retval == Const.ServiceGroupUNKNOWN)
            {
                // Le service est inconnu!
                throw new Exception(user.GetMessages().GetString("GetNavisionServiceGroup.UnknownService", service, true));
            }
            return retval;
        }

        /// <summary>
        /// Retourne les services de Navision
        /// à partir d'une liste de services (AIR, RAIL, ...)
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="service">Liste de services (séparée par des virgules)</param>
        /// <returns>Liste service Navision (1, 2, ...)</returns>
        public static int[] GetNavisionServicesGroup(UserInfo user, string serviceList)
        {
            string[] services = serviceList.Split(',');
            int NrServices = services.Length;
            int[] retval = new int[NrServices];

            for (int i = 0; i < NrServices; i++)
            {
                string service = services[i].Trim();
                // On récupère le service
                // s'il n'existe pas, alors une exception sera levée
                retval[i] = GetNavisionServiceGroup(user, service);
            }

            return retval;
        }


        /// <summary>
        /// Check if an expression is numeric
        /// </summary>
        /// <param name="Expression">Expression to ckeck</param>
        /// <returns>True or False</returns>
        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;
            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        /// <summary>
        /// Checks if an expression is a digital
        /// </summary>
        /// <param name="Expression">Express to check</param>
        /// <returns>True or False</returns>
        public static bool IsDigit(object Expression)
        {
            bool isDigit;
            long retDigit;
            isDigit = long.TryParse(Convert.ToString(Expression), out retDigit);
            return isDigit;
        }



        /// <summary>
        /// Retourne TRUE si le token n'est pas spécifié
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool IsEmptyToken(long token)
        {
            return (token.Equals(Const.EmptyBoToken));
        }

        /// <summary>
        /// Retourne TRUE si la date n'est pas spécifiée
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool IsEmptyDate(DateTime date)
        {
            return (date.Equals(Const.EmptyDate));
        }

        /// <summary>
        /// Retourne le message d'erreur generique
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="action">Action</param>
        /// <param name="message">Message</param>
        /// <returns>Message d'erreur</returns>
        public static string BuildErrorMessage(UserInfo user, string action, string message)
        {
            return String.Format("Credit Card Encryption tool failed to {0} !"
                     + "{1}Running on {7}"
                     + "{1}User name: {2} ({3}).{1}User IP address : {4}.{1}Application name : {5}.{1}{1}Exception{1}{6}"
                     + "{1}{1}<img src=\"cid:egenciaImage\"/>",
                        action, "<br>", user.GetDisplayName(), user.GetLogin(), user.GetClientIP(),
                        UserInfo.GetApplicationName(user.GetApplication()), Logger.CleanMessage(message), Const.GetServerName());
        }

        /// <summary>
        /// Remove CR. LF from a string
        /// </summary>
        /// <param name="value">String</param>
        /// <returns>Cleaned string</returns>
        public static string RemoveCRLFTAB(string value)
        {
            string retval = value;
            if (!String.IsNullOrEmpty(retval))
            {
                // Replace CR, LF, TAB by empty string
                retval = value.Replace(Const.CR, string.Empty).Replace(Const.LF, string.Empty).
                    Replace(Const.Tab.ToString(), string.Empty).Trim();

                if (retval.Equals(Const.Space))
                {
                    retval = string.Empty;
                }
            }
            return retval;
        }

        /*public static void RestartIIS()
        {
            Process iisreset = new Process();
            iisreset.StartInfo.FileName = "iisreset.exe";
            //iisreset.StartInfo.Arguments = "localhost";

            iisreset.Start();

        }*/

        /// <summary>
        /// Génération d'un token pour les cartes Egencia
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Token (16 caractères)</returns>
        public static string GenerateEgenciaToken(UserInfo user)
        {
            try
            {
                long i = 1;
                foreach (byte b in GetNewGuid().ToByteArray())
                {
                    i *= ((int)b + 1);
                }
                // On a un guid court de 16 caractères
                string guid=string.Format("{0:x}", i - DateTime.Now.Ticks);

                return guid;
            }
            catch (Exception e)
            {
                throw new Exception(user.GetMessages().GetString("Util.GenerateToken.Error", e.Message, true));
            }
        }

        /// <summary>
        /// Reading a SecureString is more complicated. 
        /// There is no simple ToString method, which is also intended to keep the data secure. 
        /// To read the data C# developers must access the data in memory directly. Luckily the .NET Framework makes it fairly simple:
        /// </summary>
        /// <returns>unsecure string</returns>
        public static string ConvertToUnsecureString(SecureString secureString)
        {

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        /// <summary>
        /// Creating a SecureString is not as simple as a regular string object. 
        /// A SecureString is created one character at a time. 
        /// The class is designed this way to encourage the data to be captured directly as the user types it into an application. 
        /// However some applications will need to copy an existing string into a SecureString, at which point adding a character at a time is sufficient.
        /// </summary>
        /// <param name="unsecureValue">unsecureValue</param>
        public static SecureString ConvertToSecureString(string unsecureValue)
        {
            SecureString secureStr = new SecureString();
            for (int i = 0; i < unsecureValue.Length; i++)
            {
                secureStr.AppendChar(unsecureValue[i]);
            }

            // MakeReadOnly command prevents the SecureString to be edited any further.
            secureStr.MakeReadOnly();
            return secureStr;
        }

        /// <summary>
        /// correct Service for Navision
        /// take care of ALL
        /// </summary>
        /// <param name="value">service</param>
        /// <returns>correct service</returns>
        public static String CorrectServiceForNavision(string value)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            if (String.Compare(value, Const.ServiceALL, true) == 0)
            {
                return string.Empty;
            }
            
            return value;
        }


    }

  

}
