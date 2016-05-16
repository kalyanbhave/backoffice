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
using SafeNetWS.utils;
using SafeNetWS.login;
using SafeNetWS.database.result;
using SafeNetWS.exception;
using SafeNetWS.creditcard.creditcardvalidator;


namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode d'insertion des cartes Egencia dans la base des données encryptées
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <Token>Valeur de retour</Token>
     *      <TruncatedPAN>Valeur de retour</TruncatedPAN>
     *      <CardType>Valeur de retour</CardType>
     *      <ShortCardType>Valeur de retour</ShortCardType>
     *      <MII>Valeur de retour</MII>
     *      <MIIIssuerCategory>Valeur de retour</MIIIssuerCategory>
     *   </Value>
     *   <Exception>
     *      <Count>0</Count>
     *      <Code></Code>
     *      <Severity></Severity>
     *      <Type></Type>
     *      <Message></Message>
     *  </Exception>
     * </Response>
     * 
     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class InsertEgenciaCardResponse
    {
        
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value Token to return (serialized into string)
        private const string Xml_Response_Token_Open_Tag = "<Token>";
        private const string Xml_Response_Token_Close_Tag = "</Token>";
        // Value TruncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_Open_Tag = "<TruncatedPAN>";
        private const string Xml_Response_TruncatedPAN_Close_Tag = "</TruncatedPAN>";
        // Value ExpirationDate to return (serialized into string)
        private const string Xml_Response_ExpirationDate_Open_Tag = "<ExpirationDate>";
        private const string Xml_Response_ExpirationDate_Close_Tag = "</ExpirationDate>";
        // Value ShortExpirationDate to return (serialized into string)
        private const string Xml_Response_ShortExpirationDate_Open_Tag = "<ShortExpirationDate>";
        private const string Xml_Response_ShortExpirationDate_Close_Tag = "</ShortExpirationDate>";
        // Value CardType to return (serialized into string)
        private const string Xml_Response_CardType_Open_Tag = "<CardType>";
        private const string Xml_Response_CardType_Close_Tag = "</CardType>";
        // Value ShortCardType to return (serialized into string)
        private const string Xml_Response_ShortCardType_Open_Tag = "<ShortCardType>";
        private const string Xml_Response_ShortCardType_Close_Tag = "</ShortCardType>";
        // Value MII to return (serialized into string)
        private const string Xml_Response_MII_Open_Tag = "<MII>";
        private const string Xml_Response_MII_Close_Tag = "</MII>";
        // Value MII to return (serialized into string)
        private const string Xml_Response_MIIIssuerCategory_Open_Tag = "<MIIIssuerCategory>";
        private const string Xml_Response_MIIIssuerCategory_Close_Tag = "</MIIIssuerCategory>";

        // Exception 
        private const string Xml_Response_Exception_Open_Tag = "<Exception>";
        private const string Xml_Response_Exception_Close_Tag = "</Exception>";
        // Exception count (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Count_Open_Tag = "<Count>";
        private const string Xml_Response_Exception_Count_Close_Tag = "</Count>";
        // Exception code
        private const string Xml_Response_Exception_Code_Open_Tag = "<Code>";
        private const string Xml_Response_Exception_Code_Close_Tag = "</Code>";
        // Exception severity
        private const string Xml_Response_Exception_Severity_Open_Tag = "<Severity>";
        private const string Xml_Response_Exception_Severity_Close_Tag = "</Severity>";
        // Exception type
        private const string Xml_Response_Exception_Type_Open_Tag = "<Type>";
        private const string Xml_Response_Exception_Type_Close_Tag = "</Type>";
        // Exception message
        private const string Xml_Response_Exception_Message_Open_Tag = "<Message>";
        private const string Xml_Response_Exception_Message_Close_Tag = "</Message>";
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";

        private string Token;
        private string TruncatedPAN;
        private string ExpirationDate;
        private string ShortExpirationDate;
        private string CardType;
        private string ShortCardType;
        private string MII;
        private string MIIIssuerCategory;

        private int ExceptionCount;
        private string ExceptionMessage;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;

        private UserInfo user;
        private DateTime StartDate;
        private string InputPan;

        public InsertEgenciaCardResponse(string pan)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            // On garde en mémoire le Pan
            // que le client souhaite envoyer
            this.InputPan = CreditCardVerifier.TruncatePan(pan);
        }
        public void SetUser(UserInfo useri)
        {
            this.user = useri;
        }
        public UserInfo GetUser()
        {
            return this.user;
        }
        public void SetValues(UserInfo useri, InsertEgenciaCardInEncryptedDBResult res)
        {
            SetUser(useri);
            this.Token = res.GetToken();
            this.TruncatedPAN = res.GetTruncatedPan();
            this.ExpirationDate = res.GetExpirationDate();
            this.ShortExpirationDate = res.GetShortExpirationDate();
            this.CardType = res.GetCardType();
            this.ShortCardType = res.GetShortCardType();
            this.MII = res.GetMII();
            this.MIIIssuerCategory = res.GetMIIIssuerCategory();
        }
        private void SetExceptionCount(UserInfo useri, int count)
        {
            SetUser(useri);
            this.ExceptionCount = count;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }
        public void SetException(UserInfo useri, string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(useri, 1);
        }
        public void SetException(UserInfo useri, Exception exception)
        {
            SetException(useri, exception.Message);
        }



        /// <summary>
        /// Retourne la durée du traitement
        /// en ms
        /// </summary>
        /// <returns>Durée (ms)</returns>
        private string GetDuration()
        {
            return Util.GetDuration(this.StartDate).ToString();
        }

        /// <summary>
        /// Retour de la réponse structurée en XML
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetResponse()
        {
            // On trace la demande
            LogResponse();

            string strData = 
            Const.XmlHeader
            + Xml_Response_Open_Tag
                +Xml_Response_Duration_Open_Tag
                    + GetDuration()
                + Xml_Response_Duration_Close_Tag;
            if (!IsError())
            {
                // Il n'y a aucune erreur
                // On va retourner la valeur
                // sans les tags d'exception
                strData +=
                Xml_Response_Value_Open_Tag
                    + Xml_Response_Token_Open_Tag
                        + this.Token
                    + Xml_Response_Token_Close_Tag
                    + Xml_Response_TruncatedPAN_Open_Tag
                        + this.TruncatedPAN
                    + Xml_Response_TruncatedPAN_Close_Tag
                    + Xml_Response_CardType_Open_Tag
                        + this.CardType
                    + Xml_Response_CardType_Close_Tag
                    + Xml_Response_ShortCardType_Open_Tag
                        + this.ShortCardType
                    + Xml_Response_ShortCardType_Close_Tag
                    + Xml_Response_ExpirationDate_Open_Tag
                        + this.ExpirationDate
                    + Xml_Response_ExpirationDate_Close_Tag
                    + Xml_Response_ShortExpirationDate_Open_Tag
                        + this.ShortExpirationDate
                    + Xml_Response_ShortExpirationDate_Close_Tag
                    + Xml_Response_MII_Open_Tag
                        + this.MII
                    + Xml_Response_MII_Close_Tag
                    + Xml_Response_MIIIssuerCategory_Open_Tag
                        + this.MIIIssuerCategory
                    + Xml_Response_MIIIssuerCategory_Close_Tag

                   + Xml_Response_Value_Close_Tag;
            }
            else
            {
                // On a rencontré une erreur
                // On va retourner les tags d'exception
                // sans les tags de valeur
               strData+=
               Xml_Response_Exception_Open_Tag
                   + Xml_Response_Exception_Count_Open_Tag
                       + this.ExceptionCount
                   + Xml_Response_Exception_Count_Close_Tag
                   + Xml_Response_Exception_Code_Open_Tag
                       + this.ExceptionCode
                   + Xml_Response_Exception_Code_Close_Tag
                   + Xml_Response_Exception_Severity_Open_Tag
                       + this.ExceptionSeverity
                   + Xml_Response_Exception_Severity_Close_Tag
                   + Xml_Response_Exception_Type_Open_Tag
                       + this.ExceptionType
                   + Xml_Response_Exception_Type_Close_Tag
                   + Xml_Response_Exception_Message_Open_Tag
                       + this.ExceptionMessage
                   + Xml_Response_Exception_Message_Close_Tag
               + Xml_Response_Exception_Close_Tag;
            }
            strData+= Xml_Response_Close_Tag;
            return strData;
        }
       
       private bool IsError()
       {
           return (this.ExceptionCount > 0);
       }
       private string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }
       private string GetToken()
       {
           return this.Token;
       }
       private string GetValueMessage()
       {
           return String.Format("Token ={0}", GetToken());
       }

       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(this.user,
            String.Format(" and provided Pan={0}", Util.Nvl(this.InputPan, string.Empty)),
            String.Format(".The following values were returned to user : {0}", GetValueMessage()),
            String.Format(".Unfortunately, the process failed for the following reason: {0}", GetExceptionMessage()),
            IsError(),
            GetDuration());

       }

        /// <summary>
        /// Décomposition de l'exception si cette dernière est enrichie
        /// On va extraire le code de l'exception
        /// le degré de sévérité de l'exception
        /// le type d'exception
        /// </summary>
       private void SplitException()
       {
           if (GetExceptionMessage().StartsWith(CCEExceptionUtil.EXCEPTION_TAG_OPEN))
           {
               // Ce message est enrichi
               // par le code, le type et la sévérité du message
               this.ExceptionCode = CCEExceptionUtil.GetExceptionCode(GetExceptionMessage());
               this.ExceptionSeverity = CCEExceptionUtil.GetExceptionSeverity(GetExceptionMessage());
               this.ExceptionType = CCEExceptionUtil.GetExceptionType(GetExceptionMessage());
               this.ExceptionMessage = CCEExceptionUtil.GetExceptionOnlyMessage(GetExceptionMessage());
           }
           else
           {
               // Cette exception n'est pas enrichie
               // On va mettre les valeurs par défaut
               this.ExceptionCode = CCEExceptionMap.EXCEPTION_CODE_DEFAULT;
               this.ExceptionSeverity = CCEExceptionMap.EXCEPTION_SEVERITY_DEFAULT;
               this.ExceptionType = CCEExceptionMap.EXCEPTION_TYPE_SYSTEM;
           }
        }


    }
}