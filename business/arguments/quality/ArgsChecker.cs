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
using System.Text.RegularExpressions;
using SafeNetWS.utils;
using SafeNetWS.log;
using SafeNetWS.login;
using SafeNetWS.www;

namespace SafeNetWS.business.arguments.quality
{

    /// <summary>
    /// Cette classe contient des fonctions
    /// permettant le contrôle des arguments
    /// des différentes méthodes
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class ArgsChecker
    {

        public const int TOKEN_UNKNOWN = -1;
        public const int TOKEN_BO = 0;
        public const string TOKEN_TYPE_BO = "TokenBO";

        public const int TOKEN_FO = 1;
        public const string TOKEN_TYPE_FO = "TokenFO";

        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);


        /// <summary>
        /// Check if string represents a valid token
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="Token">Value to test</param>
        /// <returns></returns>
        public static bool IsValidToken(UserInfo user, string Token)
        {
            try
            {
                // Is it a valid BO token?
                ValidateBOToken(user, Token);
                return true;
            }catch
            {
                try
                {
                    // Is it a valid FO ToKen?
                    ValidateFOToken(user, Token);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

        }


        /// <summary>
        /// Vérification de la validité d'un token
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="Token">Token à verifier</param>
        public static void ValidateBOToken(UserInfo user, string Token)
        {
            if (String.IsNullOrEmpty(Token))
            {
                // Le token ne peut être vide!
                throw new Exception(user.GetMessages().GetString("EmptyToken", true));
            }

            if(!Util.IsDigit(Token))
            {
                // Le token est un nombre!
                throw new Exception(user.GetMessages().GetString("TokenIsNumeric", true));
            }

            int Len = Token.Length;
            if (Len < 3)
            {
                // Le token doit avoir une longueur minimale
                throw new Exception(user.GetMessages().GetString("TokenToShort", Token, true));
            }
 
            bool ValidToken=false;
            try
            {
                // Normalement un token est constitué de deux partie
                // On récupère la partie correspondant à la date et heure
                string DateTimeString = Token.Substring(0, Len - 1);

                // On convertie en date
                // 1 ticks correspond à 100 nano
                // comme on est au 1/10 de nano pour construire le token
                // on va donc multiplier par 1000
                DateTime d = new DateTime(Util.ConvertStringToToken(DateTimeString) * 1000);

                // et vérifie bien l'année
                if (d.Year > 2005)
                {
                   // On a date une date valide
                   // On récupère le dernier digit (checksum)
                   string ChecksumString = Token.Substring(Len - 1, 1);
                   // Le dernier digit est le résultat du checksum de la première partie
                   ValidToken = (Util.CalculChecksumForToken(user, DateTimeString).Equals(ChecksumString));
                }
            }
            catch (Exception e)
            {
                // Erreur en effectuant le traitement
                throw new Exception(user.GetMessages().GetString("TokenErrorChecking", Token, e.Message, true));
            }
            if (!ValidToken)
            {
                // Le token n'est pas valide!
                throw new Exception(user.GetMessages().GetString("TokenIsNotValid", Token, true));
            }

        }


  
        /// <summary>
        /// Vérification du compte utilisateur et du mot de passe
        /// qui désire utiliser les méthodes
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        /// <returns>Informations utilisateur</returns>
        public static UserInfo CheckLogin(UserInfo useri)
        {
            UserInfo user = null;
            try
            {
                // On récupère les informations sur le compte client
                // depuis le serveur LDAP
                // On va en outre lire tous les groupes auxquels il 
                // appartient et dresser une matrice de droits
                user = new UserInfo(useri);
            }
            catch (Exception e)
            {
                // Erreur lors de l'identification de l'utilisateur
                // Erreur de connexion au serveur LDAP, user introuvable ou
                // mot de passe erroné ou tout simplement soucis LDAP
                throw new Exception(e.Message);
            }

            // A-t-on besoin de restreindre l'accès à cette méthode
            // à certaines adresses IP?
            string message = CheckSourceIP(user);

            if (message != null)
            {
                // On garde une trace de cette exception
                // L'utilisateur n'a pas les droits suffisants pour effectuer
                // l'action ...
                Logger.WriteErrorToLog(message);
                throw new Exception(message);
            }

            if (user.GetRequiredRigth() == UserInfo.RightNA)
            {
                // No need to check specific right
                return user;
            }


            bool checkDisplayCardLimit = false;
      
            // On vérifie les droits d'appel de la méthode sollicitée
            // On va comparer les droits requis par l'application
            // et ceux dont dispose l'utilsateur 

            switch (user.GetRequiredRigth())
            {
                case UserInfo.RightCreateAProfilCard:
                    if (!user.CanCreateAProfilCard())
                    {
                        message = user.GetMessages().GetString("UserCanNotCreateProfilCard", user.GetLogin(), true);
                    }
                    break;
                case UserInfo.RightCreateATransactionalCard:
                    if (!user.CanCreateATransactionalCard())
                    {
                        message = user.GetMessages().GetString("UserCanNotCreateTransactionalCard", user.GetLogin(), true);
                    }
                    break;
                case UserInfo.RightEncryptCard:
                    if (!user.CanEncryptCard())
                    {
                        message = user.GetMessages().GetString("UserCanNotEncryptCard", user.GetLogin(), true);
                    }
                    break;
                case UserInfo.RightEncryptFOCard:
                    if (!user.CanEncryptFOCard())
                    {
                        message = user.GetMessages().GetString("UserCanNotEncryptFOCard", user.GetLogin(), true);
                    }
                    break;
                case UserInfo.RightDisplayACardInLookupTool:
                case UserInfo.RightProcessALookupInLookupTool:
                    if (!user.CanProcessALookupInLookupTool() && !user.CanDisplayACardInLookupTool())
                    {
                        message = user.GetMessages().GetString("UserCanNotProcessALookupInLookupTool", user.GetLogin(), true);
                    }
                    else
                    {
                        // On contrôlera que le nombre de visualisation des cartes n'est pas atteint
                        checkDisplayCardLimit = true;
                    }
                    break;
                case UserInfo.RightProcessAReverseLookup:
                    if (!user.CanProcessAResverseLookup())
                    {
                        message = user.GetMessages().GetString("UserCanNotProcessAReverseLookup", user.GetLogin(), true);
                    }
                    break;
                case UserInfo.RightUpdateTokenAfterKeyRotation:
                    if (!user.CanUpdateTokenAfterKeyRotation())
                    {
                        message = user.GetMessages().GetString("UserCanNotUpdateTokenAfterKeyRotation", user.GetLogin(), true);
                    }
                    break;
                default:
                    break;
            }
            


            if (message != null)
            {
                // On garde une trace de cette exception (vers Syslog)
                // L'utilisateur n'a pas les droits suffisants pour effectuer
                // l'action ...
                Logger.WriteErrorToLog(message);
                throw new Exception(message);
            }
            
            // On contrôle que le nombre de visualisation des cartes n'est pas atteint
            // Ce contrôle est uniquement activé pour les clients non robots (depuis l'interface UI)
            if (!user.IsRobot() && checkDisplayCardLimit && !user.IsDisplayCardsCountLimitNotReached())
            {
                message = user.GetLogin() + ", you have reached display card limit (" + user.GetDisplayCardsCount() + ")! Please try tomorrow.";
                // On garde une trace de cette exception (vers Syslog)
                Logger.WriteErrorToLog(message);
                // et biensur on lève une exception pour informer le client
                throw new Exception(user.GetMessages().GetString("UserDisplayCardLimitReached", user.GetLogin(), user.GetDisplayCardsCount(), true));
            }
            

            // Si on est là
            // L'utilisateur est valide
            // et a tout à fait les droits suffisants pour 
            // effectuer l'action souhaitée
            return user;
        }

       

        /// <summary>
        /// Vérification de l'adresse IP source
        /// Si la méthode sollicité limite les adresses IP
        /// source, le client doit impérativement avoir une adresse IP
        /// dans cette liste
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <returns>Message en cas d'adresse IP refusée</returns>
        private static string CheckSourceIP(UserInfo user)
        {
            string message = null;
            bool checkIP = false;
            bool iPInList = false;

            // A-t-on besoin de restreindre l'accès à cette méthode
            // à certaines adresses IP?
            // On va rechercher si un paramètre a été défini
            // pour la méthode sollicitée par l'utilisateur
            string ipsString = Util.GetLimitAccessForIPsSetting(UserInfo.GetApplicationName(user.GetApplication()));

            if (!String.IsNullOrEmpty(ipsString))
            {
                // Il a une liste d'adresse IPs qui sont autorisées à avoir accès à cette méthode
                // On peut avoir une liste d'adresses séparées par une vigule
                // mais on peut aussi avoir une plage d'adresses séparées par un moins -
                // 192.168.1.1,192.168.0.1-192.168.0.22
                checkIP = true;

                // Il faut bien vérifier que cette liste est valide
                // Les différentes adresses IPs sont séparées par une virgule
                string[] allowedIPs = ipsString.Split(',');
                int NrIPs = allowedIPs.Length;
                for (int i = 0; i < NrIPs && !iPInList; i++)
                {
                    string ip = allowedIPs[i].Trim();
                    if (!String.IsNullOrEmpty(ip))
                    {
                        // On a une adresse ou une plage d'adresses
                        if (ip.IndexOf('-') > 0)
                        {
                            string[] ipsrange = ip.Split('-');
                            if (ipsrange.Length == 2)
                            {
                                // On a une plage d'adresses IP
                                // adresse de debut - adresse de fin
                                iPInList = Util.IPIsInRange(user.GetClientIP(), ipsrange[0].Trim(), ipsrange[1].Trim());
                            }
                        }
                        else
                        {
                            // Une seule adresse IP
                            if (ip.Equals(user.GetClientIP()))
                            {
                                // On a trouvé l'adresse IP du client en cours
                                // dans la liste des adresses autorisées
                                iPInList = true;
                            }
                        }
                    }
                }
            }

            if (checkIP && !iPInList)
            {
                // Cet adresse n'est pas dans la liste 
                // des IPs autorisées à solliciter cette méthode
                // Nous devons bloquer ce compte
                // en lui retournant un refus
                message = user.GetMessages().GetString("SourceHostCantCallMethod", user.GetLogin(), user.GetClientIP(), true);
            }
            return message;
        }



        /// <summary>
        /// Vérification de la validité d'un token FrontOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="Token">Token à verifier</param>
        public static void ValidateFOToken(UserInfo user, string Token)
        {
            try
            {
                if (String.IsNullOrEmpty(Token))
                {
                    // Le token ne peut être vide!
                    throw new Exception(user.GetMessages().GetString("EmptyToken", true));
                }

                if (Token.Length<36)
                {
                    // Le token est trop court
                    throw new Exception(user.GetMessages().GetString("TokenIsNotValid", Token, true));
                }

                // Un token doit avoir deux parties
                // Le numero du serveur et le GUID
                // nrserveur_guid
                string[] parts = Token.Split('_');

                if (parts.Length < 2)
                {
                    // On doit avoir deux parties
                    throw new Exception(user.GetMessages().GetString("TokenIsNotValid", Token, true));
                }
                // Extraction du numero du serveur
                string part1 = parts[0];
                if (!Util.IsDigit(part1))
                {
                    throw new Exception(user.GetMessages().GetString("TokenIsNotValid", Token, true));
                }

                // Extraction du GUID
                string guid = parts[1];

                if (guid.Length!=36)
                {
                    // la longueur d'un GUID est de 38 avec les {}
                    // donc en realite 36
                    throw new Exception(user.GetMessages().GetString("TokenIsNotValid", Token, true));
                }
                if (!IsGuid(guid))
                {
                    // Le GUID n'est pas valide
                    throw new Exception(user.GetMessages().GetString("TokenIsNotValid", Token, true));
                }
            }
            catch (Exception e)
            {
                // Erreur en effectuant le traitement
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Validation d'un ID Vpayment (Amex)
        /// Un ID VPayment a 
        ///  - 15 caractères
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="id">ID Vpayment</param>
        public static void ValidateVPaymentID(UserInfo user, string id)
        {
            if(String.IsNullOrEmpty(id)) 
            {
                // L'ID ne peut être vide!
                throw new Exception(user.GetMessages().GetString("VPaymentIDEmpty", true));   
            }
            if (id.Length != 15)
            {
                // La longueur de l'id est 15
                throw new Exception(user.GetMessages().GetString("VPaymentIDLenError", true));   
            }
            // L'ID se décompose de 2 parties
            // Un GUID (12 caractères)
            // Un checksum (3 caractères)

            // On extrait le GUID
            string guid = id.Substring(0, 12);

            // les 3 caractères suivants sont le checksum
            string checksumInId = id.Substring(12, 3);

            // On va calculer le checksum pour la chaine extraite depuis l'ID
            string checksum = Util.CalculateAlphaNumberWithPadding(guid);

            // On compare les 2 checksums
            if (!checksumInId.Equals(checksum))
            {
                throw new Exception(user.GetMessages().GetString("VPaymentIDInvalid", true));  
            }
            
        }
                
        /// <summary>
        /// Validation d'un GUID
        /// Cette méthode est utilisée pour
        /// valider le token FrontOffice
        /// </summary>
        /// <param name="candidate">Guid</param>
        /// <returns>VRAI ou FAUX</returns>
        private static bool IsGuid(string candidate)
        {
            if (candidate != null)
            {
                if (isGuid.IsMatch(candidate))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retourne le type de token
        /// FrontOffice 
        /// BackOffice
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="token">value (token BO, FO ou PAN)</param>
        /// <returns></returns>
        public static int GuessTokenType(UserInfo user, string value)
        {
            try
            {
                // Nous allons vérifier si c'est un token Back
                ValidateBOToken(user, value);
                // C'est un token Back Office
                return TOKEN_BO;
            }
            catch (Exception)
            {
                // Ce n'est visiblement pas un token Back
                // est-ce un token Front Office?
                try
                {
                    ValidateFOToken(user, value);
                    // C'est un token Front Office
                    return TOKEN_FO;
                }
                catch (Exception){}
            }
            // Ce n'est ni un token Front Office
            // ni un token Back
            return TOKEN_UNKNOWN;
        }

        /// <summary>
        /// Sanity check for comcode
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="value">value to check</param>
        /// <param name="mandatory">Value is mandatory</param>
        public static void ValidateComCode(UserInfo user, string value, bool mandatory)
        {
            if(mandatory && String.IsNullOrEmpty(value))
            {
                // The comcode is mandatory
                throw new Exception(user.GetMessages().GetString("ComcodeEmpty", true));
            }
            if (!String.IsNullOrEmpty(value) && !Util.IsNumeric(value))
            {
                // the value is not numeric
                throw new Exception(user.GetMessages().GetString("ComcodeInvalid", value, true));
            }
        }


        /// <summary>
        /// Sanity check for POS
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="value">Value to check</param>
       public static void ValidatePOS(UserInfo user, string value)
       {

            // Nous devons avoir un POS
            if (String.IsNullOrEmpty(value))
            {
                throw new Exception(user.GetMessages().GetString("PosEmpty", true));
            }
        }




        



        /// <summary>
        /// Sanity check for percode
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="value">value to check</param>
        /// <param name="mandatory">Value is mandatory</param>
        public static void ValidatePerCode(UserInfo user, string value, bool mandatory)
        {
            if (mandatory && String.IsNullOrEmpty(value))
            {
                // The comcode is mandatory
                throw new Exception(user.GetMessages().GetString("PercodeEmpty", true));
            }
            if (!String.IsNullOrEmpty(value) && !Util.IsNumeric(value))
            {
                // the value is not numeric
                throw new Exception(user.GetMessages().GetString("PercodeInvalid", value, true));
            }
        }


        /// <summary>
        /// Sanity check for cost Center id
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="value">value to check</param>
        /// <param name="mandatory">Value is mandatory</param>
        public static void ValidateCostCenterId(UserInfo user, string value, bool mandatory)
        {
            if (mandatory && String.IsNullOrEmpty(value))
            {
                // The comcode is mandatory
                throw new Exception(user.GetMessages().GetString("CostCenterEmpty", true));
            }
            if (!String.IsNullOrEmpty(value) && !Util.IsNumeric(value))
            {
                // the value is not numeric
                throw new Exception(user.GetMessages().GetString("CostCenterInvalid", value, true));
            }
        }

        public static void ValidateContextSource(UserInfo user, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new Exception("Context is empty!");
            }
            if(!value.Equals(Const.Context_Source_PreSales) && !value.Equals(Const.Context_Source_BU) && !value.Equals(Const.Context_Source_CLE))
            {
                throw new Exception("Context unknown!");
            }

        }

    }

}
