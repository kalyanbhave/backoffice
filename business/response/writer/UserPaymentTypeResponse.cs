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
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.log;



namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de récupération d'informations hiérarchique
     * relatives au mode paiement
     * La réponse est structurée de la manière suivante :
     * <ECTEGetUserPaymentTypeServiceRS xmlns:xsi = "http://www.w3.org/2001/XMLSchema-instance\">
     * <ContextRS>
     *     <Status>""</Status>
     *     <ErrorCode>-1</ErrorCode>
     *     <SeverityLevel>0</SeverityLevel>
     *     <Message>Message</Message>
     *     <Duration>Valeur de retour</Duration>
     * </ContextRS>
     * <Response>
     *     <PaymentType>EC</PaymentType>
     *     <Origin>Origin</Origin>
     *     <Service>AIR</Service>
     * </Response>
     * </ECTEGetUserPaymentTypeServiceRS>

     * 
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class UserPaymentTypeResponse
    {


        // ECTEGetUserBookingPaymentRS
        private const string Xml_ECTEGetUserPaymentType_Open_Tag = "<ECTEGetUserPaymentTypeServiceRS xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
        private const string Xml_ECTEGetUserPaymentType_Close_Tag = "</ECTEGetUserPaymentTypeServiceRS>";

        // ContextRS
        private const string Xml_Context_Open_Tag = "<ContextRS>";
        private const string Xml_Context_Close_Tag = "</ContextRS>";
        // --> Status
        private const string Xml_Context_Status_Open_Tag = "<Status>";
        private const string Xml_Context_Status_Close_Tag = "</Status>";
        // --> ErrorCode
        private const string Xml_Context_ErrorCode_Open_Tag = "<ErrorCode>";
        private const string Xml_Context_ErrorCode_Close_Tag = "</ErrorCode>";
        // --> SeverityLevel
        private const string Xml_Context_SeverityLevel_Open_Tag = "<SeverityLevel>";
        private const string Xml_Context_SeverityLevel_Close_Tag = "</SeverityLevel>";
        // --> Message
        private const string Xml_Context_Message_Open_Tag = "<Message>";
        private const string Xml_Context_Message_Close_Tag = "</Message>";
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Context_Duration_Open_Tag = "<Duration>";
        private const string Xml_Context_Duration_Close_Tag = "</Duration>";


        // Response
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        // --> PaymentType
        private const string Xml_PaymentType_Open_Tag = "<PaymentType>";
        private const string Xml_PaymentType_Close_Tag = "</PaymentType>";
        // --> Origin
        private const string Xml_Origin_Open_Tag = "<Origin>";
        private const string Xml_Origin_Close_Tag = "</Origin>";
        // --> Service
        private const string Xml_Service_Open_Tag = "<Service>";
        private const string Xml_Service_Close_Tag = "</Service>";


        // Values
        private string PaymentType;
        private string Origin;
        private string Service;

        private string ErrorCode;
        private string ExceptionMessage;
        private string ExceptionType;
        private string ExceptionSeverity;
        private int ExceptionCount;


        private UserInfo user;
        private DateTime StartDate;

        private string InputValue;

        public UserPaymentTypeResponse(string inputValue)
        {
            // initialisation
            this.StartDate = DateTime.Now;
            this.ErrorCode = "0";
            this.InputValue = Util.RemoveCRLFTAB(inputValue);
        }

        public void SetValues(UserInfo user, UserPaymentTypeResult rs)
        {
            SetUser(user);
            this.Service = rs.GetService();
            this.PaymentType = rs.GetPaymentType();
            this.Origin = rs.GetOrigin();
        }

        private void SetExceptionCount(UserInfo useri,int count)
        {
            SetUser(useri);
            this.ErrorCode = "-1";
            this.ExceptionCount = count;
        }
        public void SetException(UserInfo useri, string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(useri, 1);
        }

        public void SetException(UserInfo useri,Exception exception)
        {
            SetException(useri, exception.Message);
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
            + Xml_ECTEGetUserPaymentType_Open_Tag
            +
                Xml_Context_Open_Tag
                    + Xml_Context_Status_Open_Tag
                        + string.Empty
                    + Xml_Context_Status_Close_Tag
                    + Xml_Context_ErrorCode_Open_Tag
                        + this.ErrorCode
                    + Xml_Context_ErrorCode_Close_Tag
                    + Xml_Context_SeverityLevel_Open_Tag
                        + "0"
                    + Xml_Context_SeverityLevel_Close_Tag
                    + Xml_Context_Message_Open_Tag
                        + this.ExceptionMessage
                    + Xml_Context_Message_Close_Tag
                    + Xml_Context_Duration_Open_Tag
                        + GetDuration()
                    + Xml_Context_Duration_Close_Tag
                + Xml_Context_Close_Tag;

            if (!IsError() && !String.IsNullOrEmpty(this.PaymentType))
            {
                // On va ajouter les informations sur la carte
             
                strData +=
                Xml_Response_Open_Tag
                + Xml_PaymentType_Open_Tag
                    + this.PaymentType
                + Xml_PaymentType_Close_Tag;
                if (!String.IsNullOrEmpty(this.Origin))
                {
                    strData +=
                     Xml_Origin_Open_Tag
                        + this.Origin
                    + Xml_Origin_Close_Tag;
                }
                if (!String.IsNullOrEmpty(this.Service))
                {
                    strData +=
                     Xml_Service_Open_Tag
                        + this.Service
                    + Xml_Service_Close_Tag;
                }
                strData +=
                 Xml_Response_Close_Tag;
                
            }

            strData += Xml_ECTEGetUserPaymentType_Close_Tag;
            return Util.HtmlEncode(strData);
        }
       
       private bool IsError()
       {
           return (this.ExceptionCount > 0);
       }
       private string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }
       private string GetValueMessage()
       {
           return "PaymentType =" + this.PaymentType + ", Service =" + this.Service + 
               ", Origin =" + this.Origin;
       }
       public void SetUser(UserInfo useri)
       {
           this.user = useri;
       }

       private string GetInputValue()
       {
           return this.InputValue;
       }

       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(),
            String.Format(" and provided {0}.", GetInputValue()),
            String.Format("The following values were returned to user : {0}", GetValueMessage()),
            String.Format("Unfortunately, the process failed for the following reason: {0}", GetExceptionMessage()),
            IsError(),
            GetDuration());
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
       /// Retourne le compte utilisateur
       /// </summary>
       /// <returns>Compte utilisateur</returns>
       public UserInfo GetUser()
       {
           return this.user;
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
               this.ErrorCode = CCEExceptionUtil.GetExceptionCode(GetExceptionMessage());
               this.ExceptionSeverity = CCEExceptionUtil.GetExceptionSeverity(GetExceptionMessage());
               this.ExceptionType = CCEExceptionUtil.GetExceptionType(GetExceptionMessage());
               this.ExceptionMessage = CCEExceptionUtil.GetExceptionOnlyMessage(GetExceptionMessage());
           }
           else
           {
               // Cette exception n'est pas enrichie
               // On va mettre les valeurs par défaut
               this.ErrorCode = CCEExceptionMap.EXCEPTION_CODE_DEFAULT;
               this.ExceptionSeverity = CCEExceptionMap.EXCEPTION_SEVERITY_DEFAULT;
               this.ExceptionType = CCEExceptionMap.EXCEPTION_TYPE_SYSTEM;
           }
       }
    }
}
