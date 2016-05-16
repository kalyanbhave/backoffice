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
using System.Collections.Generic;

namespace SafeNetWS.exception
{
    public class CCEExceptionMap
    {
        // Sevérité
        public const string EXCEPTION_SEVERITY_FATAL = "FATAL";
        public const string EXCEPTION_SEVERITY_ERROR = "ERROR";
        public const string EXCEPTION_SEVERITY_WARNING = "WARNING";

        // TYPE
        public const string EXCEPTION_TYPE_UNKNWON = "UNKNOWN";
        public const string EXCEPTION_TYPE_SYSTEM = "SYSTEM";
        public const string EXCEPTION_TYPE_FONCTIONAL = "FUNCTIONAL";

        // Valeurs par défaut
        public const string EXCEPTION_CODE_DEFAULT = EXCEPTION_TYPE_UNKNWON;
        public const string EXCEPTION_TYPE_DEFAULT = EXCEPTION_TYPE_SYSTEM;
        public const string EXCEPTION_SEVERITY_DEFAULT = EXCEPTION_SEVERITY_ERROR;

        // Tableau des exceptions
        private static Dictionary<string, CCEExceptionInfo> dictionary;

        public static void LoadMap()
        {
            if (dictionary == null)
            {
                // Chargement des codes d'exception dépendant de la clé de traduction
                dictionary = new Dictionary<string, CCEExceptionInfo>();
        
                dictionary.Add("SourceHostCantCallMethod", new CCEExceptionInfo("NOT_ALLOWED_IP", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("Services.GetPanFromFOToken.FOTokenUnknow", new CCEExceptionInfo("UNKNOWN_FO_TOKEN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                // LDAP      
                dictionary.Add("EmptyPassword", new CCEExceptionInfo("EMPTY_PASSWORD", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("EmptyLogin", new CCEExceptionInfo("EMPTY_LOGIN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("LDAPUnknownUserOrWrongPassword", new CCEExceptionInfo("UNKNOWN_USER_OR_WRONG_PASSWORD", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("LDAPConnectionError", new CCEExceptionInfo("LDAP_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("UserCanNotEncryptFOCard", new CCEExceptionInfo("LDAP_CAN_NOT_ENCRYPT_FO_CARD", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("LdapAuthentication.GetGroups.Error", new CCEExceptionInfo("LDAP_GROUPS_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("LdapAuthentication.Init.Error", new CCEExceptionInfo("LDAP_CONNECTION_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("LDAPUnknownUser", new CCEExceptionInfo("LDAP_UNKNOWN_USER", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("LdapAuthentication.Disconnect", new CCEExceptionInfo("LDAP_CLOSE", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("LDAPUserNotBelongToGroup", new CCEExceptionInfo("LDAP_USER_NOT_BELONG_TO_GROUP", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR)); 
               
                
                // SafeNet
                dictionary.Add("SafeNet.CanNotFindKey", new CCEExceptionInfo("SAFENET_KEY_NOT_EXIST", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("SafeNet.NotAESKey", new CCEExceptionInfo("SAFENET_KEY_NOT_AES", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("SafeNet.UnexpectedError", new CCEExceptionInfo("SAFENET_KEY_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("SafeNet.Error.Init", new CCEExceptionInfo("SAFENET_INIT", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("SafeNet.Error.ClosingSession", new CCEExceptionInfo("SAFENET_CLOSE", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("SafeNet.Error.OpeningSession", new CCEExceptionInfo("SAFENET_OPEN", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("SafeNet.Error.Encrypting", new CCEExceptionInfo("SAFENET_CRYPT", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("SafeNet.Error.Decrypting", new CCEExceptionInfo("SAFENET_DECRYPT", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                // Token
                dictionary.Add("TokenIsNotValid", new CCEExceptionInfo("BO_TOKEN_NOT_VALID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("EmptyToken", new CCEExceptionInfo("BO_TOKEN_EMPTY", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("TokenIsNumeric", new CCEExceptionInfo("BO_TOKEN_NOT_NUMERIC", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("TokenToShort", new CCEExceptionInfo("BO_TOKEN_TOO_SHORT", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("TokenErrorChecking", new CCEExceptionInfo("BO_TOKEN_ERROR_CHECKING", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("NoPANFoundForToken", new CCEExceptionInfo("BO_TOKEN_UNKNOWN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("UpdateEncryptedCard.UpdateEncryptedCard.Error", new CCEExceptionInfo("ERROR_EXPDATE_UPDATE", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL));
                dictionary.Add("UnknownToken", new CCEExceptionInfo("TOKEN_UNKNOWN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                
                
                // PAN
                dictionary.Add("EmptyPAN", new CCEExceptionInfo("PAN_EMPTY", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("PANNotDigits", new CCEExceptionInfo("PAN_NOT_DIGITS", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("UnvalidPAN", new CCEExceptionInfo("PAN_NOT_VALID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("UnknownCardType", new CCEExceptionInfo("CARD_TYPE_UNKNOWN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("LunhValidateError", new CCEExceptionInfo("ERROR_CHEKING_CARD", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("ExpiredPAN", new CCEExceptionInfo("CARD_EXPIRED", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("UnvalidExpirationDate", new CCEExceptionInfo("CARD_UNVALID_EXP_DATE", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("InvalidExpirationDate", new CCEExceptionInfo("CARD_UNVALID_EXP_DATE", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CardTypeNotAllowedByNavision", new CCEExceptionInfo("CARD_TYPE_NOT_ALLOWED", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
  
                // Service
                dictionary.Add("GetNavisionServiceGroup.UnknownService", new CCEExceptionInfo("UNKNOWN_SERVICE", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                // HTTP
                dictionary.Add("HTTP.Error.ResponseStatusKO", new CCEExceptionInfo("BIBIT_TIMEOUT", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL)); 
                dictionary.Add("HTTP.Error.NoResponse", new CCEExceptionInfo("BIBIT_NO_RESPONSE", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_FATAL)); 
               
                // Credit card log
                dictionary.Add("CreditCardLogConnection.LogCard.Error", new CCEExceptionInfo("CREDIT_CARD_LOG_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CreditCardLogConnection.ErrorConnecting", new CCEExceptionInfo("CREDIT_CARD_LOG_CONNECTION_OPEN_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CreditCardLogConnection.ErrorClosingConnection", new CCEExceptionInfo("CREDIT_CARD_LOG_CONNECTION_CLOSE_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                
                // BIBIT
                dictionary.Add("Bibit.RejectedCard", new CCEExceptionInfo("REFUSED", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                
                // Credit card validation
                dictionary.Add("CreditCardValidationResponse.InputValueEmpty", new CCEExceptionInfo("INPUT_VALUE_MISSING", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
 
                // VPayment
                dictionary.Add("CheckVPaymentForCorporation.VPaymentNotAllowedForCorporation", new CCEExceptionInfo("VPAYMENT_NOT_ALLOWED", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CheckVPaymentForCorporation.CanNotFindCorporation", new CCEExceptionInfo("COMPANY_NOT_EXIST", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("VPaymentIDEmpty", new CCEExceptionInfo("EMPTY_VPAYMENT_ID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("VPaymentIDLenError", new CCEExceptionInfo("WRONG_LEN_VPAYMENT_ID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("VPaymentIDInvalid", new CCEExceptionInfo("INVALID_VPAYMENT_ID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
 
                // Amadeus Alignment profil
                dictionary.Add("UserBookingPaymentResponse.NoPaymentCardFound", new CCEExceptionInfo("-2", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));

                // Navision delete payment card
                dictionary.Add("NavisionDbConnection.GetCardProvider.Error", new CCEExceptionInfo("CARD_PROVIDER_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("DeleteCardInNavision.EmptyCardReference", new CCEExceptionInfo("EMPTY_CARD_REFERENCE", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("DeleteCardInNavision.EmptyCustomer", new CCEExceptionInfo("EMPTY_CUSTOMER", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("NavisionDbConnection.DeleteCard.Error", new CCEExceptionInfo("DELETE_CARD_ERROR", EXCEPTION_TYPE_SYSTEM, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("NavisionDbConnection.DeleteCard.CardNotFound.Error", new CCEExceptionInfo("CARD_NOT_EXIST", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                
                // Corporation payment mode
                dictionary.Add("CheckGDSPaymentTypeForCorporation.CanNotFindCorporation", new CCEExceptionInfo("COMPANY_NOT_EXIST", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CorporationEmpty", new CCEExceptionInfo("CORPORATION_CODE_EMPTY", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("PosEmpty", new CCEExceptionInfo("POS_EMPTY", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));


                // ENett
                dictionary.Add("Services.SaveENettECN.Error", new CCEExceptionInfo("ALREADY_EXIST", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("Services.GetENettECN.UnknowECN", new CCEExceptionInfo("UNKNOW_ECN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));

                dictionary.Add("PosUnknown", new CCEExceptionInfo("POS_UNKNOWN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));

                dictionary.Add("ComcodeInvalid", new CCEExceptionInfo("COMCODE_INVALID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("ComcodeEmpty", new CCEExceptionInfo("COMCODE_MISSING", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CostCenterInvalid", new CCEExceptionInfo("COSTCENTER_INVALID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("CostCenterEmpty", new CCEExceptionInfo("COSTCENTER_MISSING", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("PercodeInvalid", new CCEExceptionInfo("PERCODE_INVALID", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                dictionary.Add("PercodeEmpty", new CCEExceptionInfo("PERCODE_MISSING", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));

                dictionary.Add("UnknowPOS", new CCEExceptionInfo("POS_UNKNOWN", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));

                dictionary.Add("CheckCustomerForPos.CanNotFindCustomer", new CCEExceptionInfo("COMCODE_NOT_EXIST", EXCEPTION_TYPE_FONCTIONAL, EXCEPTION_SEVERITY_ERROR));
                
                
            }
        }

        /// <summary>
        /// Récupération des informations d'une exception
        /// </summary>
        /// <param name="key">Clé de traduction</param>
        /// <returns>Informations sur l'exception</returns>
        public static CCEExceptionInfo GetExceptionInfo(string key)
        {
            // chargement du tableau des exceptions si nécéssaire
            LoadMap();
            if (dictionary.ContainsKey(key))
            {
                // la clé est dans le dictionnaire
                // il suffit donc de la retourner
                return dictionary[key];
            }
            else
            {
                // La clé n'existe pas dans le dictionnaire
                // On va retourner la valeur par défaut
                return new CCEExceptionInfo(EXCEPTION_CODE_DEFAULT, EXCEPTION_TYPE_DEFAULT, EXCEPTION_SEVERITY_DEFAULT); 
            }
        }
    }
}
