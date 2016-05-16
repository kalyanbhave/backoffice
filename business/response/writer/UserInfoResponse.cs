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
using SafeNetWS.exception;


namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode qui se connecte à l'AD re récupère les informations sur un user
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *       <Login>Valeur de retour</Login>
     *       <ClientIP>Valeur de retour</ClientIP>
     *       <DisplayName>Valeur de retour</DisplayName>
     *       <DisplayCardsCount>Valeur de retour</DisplayCardsCount>
     *       <LoginDate>Valeur de retour</LoginDate>
     *       <DisplayACardInLookupTool>Valeur de retour</DisplayACardInLookupTool>
     *       <ProcessALookupInLookupTool>Valeur de retour</ProcessALookupInLookupTool>
     *       <ProcessAResverseLookup>Valeur de retour</ProcessAResverseLookup>
     *       <CreateATransactionalCard>Valeur de retour</CreateATransactionalCard>
     *       <CreateAProfilCard>Valeur de retour</CreateAProfilCard>
     *       <EncryptACard>Valeur de retour</EncryptACard>
     *       <EncryptAFOCard>Valeur de retour</EncryptAFOCard>
     *       <CanUpdateTokenAfterKeyRotation>Valeur de retour</CanUpdateTokenAfterKeyRotation>  
     *       <IsARobot>Valeur de retour</IsARobot>      
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
    public class UserInfoResponse
    {
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value Login to return (serialized into string)
        private const string Xml_Response_Login_Open_Tag = "<Login>";
        private const string Xml_Response_Login_Close_Tag = "</Login>";
        // Value Lang to return (serialized into string)
        private const string Xml_Response_Lang_Open_Tag = "<Lang>";
        private const string Xml_Response_Lang_Close_Tag = "</Lang>";
        // Value ClientIP to return (serialized into string)
        private const string Xml_Response_ClientIP_Open_Tag = "<ClientIP>";
        private const string Xml_Response_ClientIP_Close_Tag = "</ClientIP>";
        // Value DisplayName to return (serialized into string)
        private const string Xml_Response_DisplayName_Open_Tag = "<DisplayName>";
        private const string Xml_Response_DisplayName_Close_Tag = "</DisplayName>";
        // Value displayCardsCount to return (serialized into string)
        private const string Xml_Response_DisplayCardsCount_Open_Tag = "<DisplayCardsCount>";
        private const string Xml_Response_DisplayCardsCount_Close_Tag = "</DisplayCardsCount>";
        // Value loginDate to return (serialized into string)
        private const string Xml_Response_LoginDate_Open_Tag = "<LoginDate>";
        private const string Xml_Response_LoginDate_Close_Tag = "</LoginDate>";
        // Value DisplayACardInLookupTool to return (serialized into string)
        private const string Xml_Response_DisplayACardInLookupTool_Open_Tag = "<DisplayACardInLookupTool>";
        private const string Xml_Response_DisplayACardInLookupTool_Close_Tag = "</DisplayACardInLookupTool>";
        // Value ProcessALookupInLookupTool to return (serialized into string)
        private const string Xml_Response_ProcessALookupInLookupTool_Open_Tag = "<ProcessALookupInLookupTool>";
        private const string Xml_Response_ProcessALookupInLookupTool_Close_Tag = "</ProcessALookupInLookupTool>";
        // Value ProcessAResverseLookup to return (serialized into string)
        private const string Xml_Response_ProcessAResverseLookup_Open_Tag = "<ProcessAResverseLookup>";
        private const string Xml_Response_ProcessAResverseLookup_Close_Tag = "</ProcessAResverseLookup>";
        // Value CreateATransactionalCard to return (serialized into string)
        private const string Xml_Response_CreateATransactionalCard_Open_Tag = "<CreateATransactionalCard>";
        private const string Xml_Response_CreateATransactionalCard_Close_Tag = "</CreateATransactionalCard>";
        // Value CreateAProfilCard to return (serialized into string)
        private const string Xml_Response_CreateAProfilCard_Open_Tag = "<CreateAProfilCard>";
        private const string Xml_Response_CreateAProfilCard_Close_Tag = "</CreateAProfilCard>";
        // Value EncryptACard to return (serialized into string)
        private const string Xml_Response_EncryptACard_Open_Tag = "<EncryptACard>";
        private const string Xml_Response_EncryptACard_Close_Tag = "</EncryptACard>";
        // Value EncryptAFOCard to return (serialized into string)
        private const string Xml_Response_EncryptAFOCard_Open_Tag = "<EncryptAFOCard>";
        private const string Xml_Response_EncryptAFOCard_Close_Tag = "</EncryptAFOCard>";
        // Value UpdateTokenAfterKeyRotation to return (serialized into string)
        private const string Xml_Response_UpdateTokenAfterKeyRotation_Open_Tag = "<UpdateTokenAfterKeyRotation>";
        private const string Xml_Response_UpdateTokenAfterKeyRotation_Close_Tag = "</UpdateTokenAfterKeyRotation>";
        // Value IsARobot to return (serialized into string)
        private const string Xml_Response_IsARobot_Open_Tag = "<IsARobot>";
        private const string Xml_Response_IsARobot_Close_Tag = "</IsARobot>";


        
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";


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

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private DateTime StartDate;
        private UserInfo User;

        public UserInfoResponse()
        {
            // Initialisation
            this.StartDate = DateTime.Now;
        }

        public void SetValue(UserInfo useri)
        {
            this.User = useri;
        }
        public UserInfo GetValue()
        {
            return this.User;
        }
        private string ConvertBoolToInt(bool value)
        {
            return value == true ? "1" : "0";   
        }
        private void SetExceptionCount(int count)
        {
            this.ExceptionCount = count;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }
        public void SetException(string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(1);
        }

        public void SetException(Exception exception)
        {
            SetException(exception.Message);
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
            // on va tracer cette demande
            LogResponse();
            // Ok, maintenant on va construire la réponse
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
                strData+=
                Xml_Response_Value_Open_Tag
                    + Xml_Response_Login_Open_Tag
                        + GetUser().GetLogin()
                    + Xml_Response_Login_Close_Tag
                    + Xml_Response_Lang_Open_Tag
                        + GetUser().GetMessages().GetLang()
                    + Xml_Response_Lang_Close_Tag
                    + Xml_Response_ClientIP_Open_Tag
                        + GetUser().GetClientIP()
                    + Xml_Response_ClientIP_Close_Tag
                    + Xml_Response_DisplayName_Open_Tag
                        + GetUser().GetDisplayName()
                    + Xml_Response_DisplayName_Close_Tag
                    + Xml_Response_DisplayCardsCount_Open_Tag
                        + GetUser().GetDisplayCardsCount().ToString()
                    + Xml_Response_DisplayCardsCount_Close_Tag
                    + Xml_Response_LoginDate_Open_Tag
                        + Util.ConvertDateToString(GetUser().GetLoginDate(), Const.DateFormat_ddMMyyyyHHmmss)
                    + Xml_Response_LoginDate_Close_Tag
                    + Xml_Response_DisplayACardInLookupTool_Open_Tag
                        + ConvertBoolToInt(GetUser().CanDisplayACardInLookupTool())
                    + Xml_Response_DisplayACardInLookupTool_Close_Tag
                    + Xml_Response_ProcessALookupInLookupTool_Open_Tag
                        +  ConvertBoolToInt(GetUser().CanProcessALookupInLookupTool())
                    + Xml_Response_ProcessALookupInLookupTool_Close_Tag
                    + Xml_Response_ProcessAResverseLookup_Open_Tag
                        +  ConvertBoolToInt(GetUser().CanProcessAResverseLookup())
                    + Xml_Response_ProcessAResverseLookup_Close_Tag
                    + Xml_Response_CreateAProfilCard_Open_Tag
                        + ConvertBoolToInt(GetUser().CanCreateAProfilCard())
                    + Xml_Response_CreateAProfilCard_Close_Tag
                    + Xml_Response_CreateATransactionalCard_Open_Tag
                        + ConvertBoolToInt(GetUser().CanCreateATransactionalCard())
                    + Xml_Response_CreateATransactionalCard_Close_Tag
                    + Xml_Response_EncryptACard_Open_Tag
                        + ConvertBoolToInt(GetUser().CanEncryptCard())
                    + Xml_Response_EncryptACard_Close_Tag
                    + Xml_Response_EncryptAFOCard_Open_Tag
                        + ConvertBoolToInt(GetUser().CanEncryptFOCard())
                    + Xml_Response_EncryptAFOCard_Close_Tag
                    + Xml_Response_UpdateTokenAfterKeyRotation_Open_Tag
                        + ConvertBoolToInt(GetUser().CanUpdateTokenAfterKeyRotation())
                    + Xml_Response_UpdateTokenAfterKeyRotation_Close_Tag
                    + Xml_Response_IsARobot_Open_Tag
                        + ConvertBoolToInt(GetUser().IsRobot())
                    + Xml_Response_IsARobot_Close_Tag
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
       /// <summary>
       /// Retourne le compte utilisateur
       /// </summary>
       /// <returns>Compte utilisateur</returns>
       private UserInfo GetUser()
       {
           return this.User;
       }
       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(),
             null,
             GetValueMessage(),
             String.Format("The connection failed for the following reason: {0}", GetExceptionMessage()),
             IsError(),
             GetDuration());

       }
       private string GetValueMessage()
       {
           return String.Format(".User {0}({1}) connected to {2} from {3}", GetUser().GetLogin(),
               GetUser().GetDisplayName(), UserInfo.GetApplicationName(GetUser().GetApplication()), GetUser().GetClientIP());
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
