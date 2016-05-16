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
using System.Collections;
using SafeNetWS.utils;
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.creditcard.creditcardvalidator;


namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode d'insertion des cartes transactionnelles dans navision
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <Token>Valeur de retour</Token>
     *      <CardReference>Valeur</CardReference>
     *      <ServiceProvided>Valeur</ServiceProvided>
     *      <ExpirationDate>Valeur de retour</ExpirationDate>
     *      <CardType>Valeur de retour</CardType>
     *      <TruncatedPAN>Valeur de retour</TruncatedPAN>
     *      <CardUsedByAnotherCustomer>Valeur de retour</CardUsedByAnotherCustomer>
     *   </Value>
     *   <Exception>
     *      <Count>0</Count>
     *      <Message></Message>
     *      <Code></Code>
     *      <Severity></Severity>
     *      <Type></Type>
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
    public class InsertTransactCardResponse
    {

        private const string Xml_Response_Open_Tag = "<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value Token to return (serialized into string)
        private const string Xml_Response_Token_Open_Tag = "<Token>";
        private const string Xml_Response_Token_Close_Tag = "</Token>";
        // Value CardReference to return (serialized into string)
        private const string Xml_Response_CardReference_Open_Tag = "<CardReference>";
        private const string Xml_Response_CardReference_Close_Tag = "</CardReference>";
        // Value ServiceProvided to return (serialized into string)
        private const string Xml_Response_ServiceProvided_Open_Tag = "<ServiceProvided>";
        private const string Xml_Response_ServiceProvided_Close_Tag = "</ServiceProvided>";
        // Value ExpirationDate to return (serialized into string)
        private const string Xml_Response_ExpirationDate_Open_Tag = "<ExpirationDate>";
        private const string Xml_Response_ExpirationDate_Close_Tag = "</ExpirationDate>";
        // Value CardType to return (serialized into string)
        private const string Xml_Response_CardType_Open_Tag = "<CardType>";
        private const string Xml_Response_CardType_Close_Tag = "</CardType>";
        // Value TruncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_Open_Tag = "<TruncatedPAN>";
        private const string Xml_Response_TruncatedPAN_Close_Tag = "</TruncatedPAN>";
        // Value CardUsedByAnotherCustomer to return (serialized into string)
        private const string Xml_Response_CardUsedByAnotherCustomer_Open_Tag = "<CardUsedByAnotherCustomer>";
        private const string Xml_Response_CardUsedByAnotherCustomer_Close_Tag = "</CardUsedByAnotherCustomer>";


        // Exception 
        private const string Xml_Response_Exception_Open_Tag = "<Exception>";
        private const string Xml_Response_Exception_Close_Tag = "</Exception>";
        // Exception code (0 = no error otherwise 1)
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
        private string CardReference;
        private string ServiceProvided;
        private string ExpirationDate;
        private string CardType;
        private string TruncatedPAN;
        private string CardUsedByAnotherCustomer;


        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo user;
        private DateTime StartDate;
        private string InputPan;

        public InsertTransactCardResponse(string pan)
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
        public void SetValues(UserInfo useri, InsertCardResult res)
        {
            SetUser(useri);
            this.Token = res.GetToken().ToString();
            if (this.Token.Equals("-1")) this.Token = null;
            this.CardReference = res.GetCardReference();
            this.ServiceProvided = res.GetCardService();
            this.ExpirationDate = res.GetExpirationDate();
            this.CardType = res.GetCardType();
            this.TruncatedPAN = res.GetTruncatedPAN();
            this.CardUsedByAnotherCustomer = res.GetCardUsedByAnotherCustomer();
        }


        private string GetCardReference()
        {
            return this.CardReference;
        }
        private string GetServiceProvided()
        {
            return this.ServiceProvided;
        }
        private DateTime GetStartDate()
        {
            return this.StartDate;
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
        /// Retourne la durée de traitement
        /// </summary>
        /// <returns>Durée de traitement (ms)</returns>
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
            // Ok, on a construire la réponse
            string strData =
            Const.XmlHeader
            + Xml_Response_Open_Tag
                + Xml_Response_Duration_Open_Tag
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
                        + GetToken()
                    + Xml_Response_Token_Close_Tag
                    + Xml_Response_CardReference_Open_Tag
                        + GetCardReference()
                    + Xml_Response_CardReference_Close_Tag
                    + Xml_Response_ServiceProvided_Open_Tag
                        + GetServiceProvided()
                    + Xml_Response_ServiceProvided_Close_Tag
                    + Xml_Response_ExpirationDate_Open_Tag
                        + GetExpirationDate()
                    + Xml_Response_ExpirationDate_Close_Tag
                    + Xml_Response_CardType_Open_Tag
                        + GetCardType()
                    + Xml_Response_CardType_Close_Tag
                    + Xml_Response_TruncatedPAN_Open_Tag
                        + GetTruncatedPAN()
                    + Xml_Response_TruncatedPAN_Close_Tag;
                if (!String.IsNullOrEmpty(GetCardUsedByAnotherCustomer()))
                {
                    strData +=
                     Xml_Response_CardUsedByAnotherCustomer_Open_Tag
                        + GetCardUsedByAnotherCustomer()
                    + Xml_Response_CardUsedByAnotherCustomer_Close_Tag;
                }

                strData += Xml_Response_Value_Close_Tag;
            }
            else
            {
                // On a rencontré une erreur
                // On va retourner les tags d'exception
                // sans les tags de valeur
                strData +=
                Xml_Response_Exception_Open_Tag
                    + Xml_Response_Exception_Count_Open_Tag
                        + GetExceptionCount()
                + Xml_Response_Exception_Count_Close_Tag
                + Xml_Response_Exception_Code_Open_Tag
                     + GetExceptionCode()
                + Xml_Response_Exception_Code_Close_Tag
                + Xml_Response_Exception_Severity_Open_Tag
                     + GetExceptionSeverity()
                + Xml_Response_Exception_Severity_Close_Tag
                + Xml_Response_Exception_Type_Open_Tag
                      + GetExceptionType()
                + Xml_Response_Exception_Type_Close_Tag
                + Xml_Response_Exception_Message_Open_Tag
                      + GetExceptionMessage()
                   + Xml_Response_Exception_Message_Close_Tag
               + Xml_Response_Exception_Close_Tag;
            }
            strData += Xml_Response_Close_Tag;
            return strData;
        }
        private int GetExceptionCount()
        {
            return this.ExceptionCount;
        }
        private string GetExceptionCode()
        {
            return this.ExceptionCode;
        }
        private string GetExceptionSeverity()
        {
            return this.ExceptionSeverity;
        }
        private string GetExceptionType()
        {
            return this.ExceptionType;
        }
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }
        private bool IsError()
        {
            return (GetExceptionCount() > 0);
        }

        private string GetToken()
        {
            return this.Token;
        }
        private string GetExpirationDate()
        {
            return this.ExpirationDate;
        }
        private string GetTruncatedPAN()
        {
            return this.TruncatedPAN;
        }
        private string GetCardType()
        {
            return this.CardType;
        }
        private string GetCardUsedByAnotherCustomer()
        {
            return this.CardUsedByAnotherCustomer;
        }
        private string GetValueMessage()
        {
            return String.Format("Card reference={0}, token ={1}", GetCardReference(), GetToken());
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