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

namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de mise à jour des cryptogrammes des cartes
     * après rotation de clé
     * La réponse est structurée de la manière suivante :
     * 
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *    <Duration>Valeur de retour</Duration>
     *   <Value>    
     *      <Total>Valeur de retour</Total>    
     *      <Success>Valeur de retour</Success>
     *      <Error>Valeur de retour</Error>
     *      <FORemainingCards>Valeur de retour</FORemainingCards>    
     *      <ClearedBOBibitCacheEntries>Valeur de retour</ClearedBOBibitCacheEntries>
     *      <ClearedFOBibitCacheEntries>Valeur de retour</ClearedFOBibitCacheEntries>
            <EgenciaCardsTotal>Valeur de retour</EgenciaCardsTotal>    
     *      <EgenciaCardsSuccess>Valeur de retour</EgenciaCardsSuccess>
     *      <EgenciaCardsError>Valeur de retour</EgenciaCardsError>
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
    public class KeyRotationResponse
    {
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";

        // Value to return (serialized into string)
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Count to return (serialized into string)
        private const string Xml_Count_Open_Tag = "<Total>";
        private const string Xml_Count_Close_Tag = "</Total>";
        // SuccessCount to return (serialized into string)
        private const string Xml_SuccessCount_Open_Tag = "<Success>";
        private const string Xml_SuccessCount_Close_Tag = "</Success>";
        // ErrorCount to return (serialized into string)
        private const string Xml_ErrorCount_Open_Tag = "<Error>";
        private const string Xml_ErrorCount_Close_Tag = "</Error>";

        // EgenciaCards Count to return (serialized into string)
        private const string Xml_EgenciaCardsCount_Open_Tag = "<EgenciaCardsTotal>";
        private const string Xml_EgenciaCardsCount_Close_Tag = "</EgenciaCardsTotal>";
        // EgenciaCards SuccessCount to return (serialized into string)
        private const string Xml_EgenciaCardsSuccessCount_Open_Tag = "<EgenciaCardsSuccess>";
        private const string Xml_EgenciaCardsSuccessCount_Close_Tag = "</EgenciaCardsSuccess>";
        // EgenciaCards ErrorCount to return (serialized into string)
        private const string Xml_EgenciaCardsErrorCount_Open_Tag = "<EgenciaCardsError>";
        private const string Xml_EgenciaCardsErrorCount_Close_Tag = "</EgenciaCardsError>";

        // FORemainingCards to return (serialized into string)
        private const string Xml_FORemainingCards_Open_Tag = "<FORemainingCards>";
        private const string Xml_FORemainingCards_Close_Tag = "</FORemainingCards>";
        // ClearedBOBibitCacheEntries to return (serialized into string)
        private const string Xml_ClearedBOBibitCacheEntries_Open_Tag = "<ClearedBOBibitCacheEntries>";
        private const string Xml_ClearedBOBibitCacheEntries_Close_Tag = "</ClearedBOBibitCacheEntries>";
        // ClearedFOBibitCacheEntries to return (serialized into string)
        private const string Xml_ClearedFOBibitCacheEntries_Open_Tag = "<ClearedFOBibitCacheEntries>";
        private const string Xml_ClearedFOBibitCacheEntries_Close_Tag = "</ClearedFOBibitCacheEntries>";
        


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


        private int Count;
        private int SuccessCount;
        private int ErrorCount;
        private int RemainingFOCards;
        private int ClearedBOBibitCacheEntries;
        private int ClearedFOBibitCacheEntries;
        // egencia card
        private int EgenciaCardsCount;
        private int EgenciaCardsSuccessCount;
        private int EgenciaCardsErrorCount;

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo user;
        private DateTime StartDate;

        public KeyRotationResponse()
        {
            // Initialisation
            this.StartDate = DateTime.Now;
        }
        public void SetUser(UserInfo useri)
        {
            this.user = useri;
        }
        public UserInfo GetUser()
        {
            return this.user;
        }
        public void SetValues(UserInfo useri, KeyRotationResult res)
        {
            SetUser(useri);
            this.Count = res.GetCount();
            this.SuccessCount = res.GetSuccessCount();
            this.RemainingFOCards = res.GetRemainingFOCards();
            this.ErrorCount = res.GetErrorCount();
            this.ClearedBOBibitCacheEntries = res.GetClearedBOBibitCacheEntries();
            this.ClearedFOBibitCacheEntries = res.GetClearedFOBibitCacheEntries();
            this.EgenciaCardsCount = res.GetEgenciaCardsCount();
            this.EgenciaCardsSuccessCount = res.GetEgenciaCardsSuccessCount();
            this.EgenciaCardsErrorCount = res.GetEgenciaCardsErrorCount();
        }

        private void SetExceptionCount(int Counti)
        {
            SetUser(user);
            this.ExceptionCount = Counti;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }

        public void SetException(UserInfo user, string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(user, 1);
        }
        private void SetExceptionCount(UserInfo useri, int Count)
        {
            SetUser(useri);
            this.ExceptionCount = Count;
        }
        public void SetException(UserInfo useri, Exception exception)
        {
            SetException(useri, exception.Message);
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
            return String.Format("Total = {0}, Success = {1}, Error = {2}", this.Count, this.SuccessCount, this.ErrorCount);
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
            string strData = Const.XmlHeader
                + Xml_Response_Open_Tag
                    +Xml_Response_Duration_Open_Tag
                        + GetDuration()
                    + Xml_Response_Duration_Close_Tag;
                if (!IsError())
                {
                    // Pas d'exception
                    // Tout va bien
                    strData +=
                       Xml_Count_Open_Tag
                        + this.Count
                     + Xml_Count_Close_Tag
                     + Xml_SuccessCount_Open_Tag
                        + this.SuccessCount
                     + Xml_SuccessCount_Close_Tag
                     + Xml_ErrorCount_Open_Tag
                        + this.ErrorCount
                     + Xml_ErrorCount_Close_Tag
                     + Xml_FORemainingCards_Open_Tag
                        + this.RemainingFOCards
                     + Xml_FORemainingCards_Close_Tag
                     + Xml_ClearedBOBibitCacheEntries_Open_Tag
                        + this.ClearedBOBibitCacheEntries
                     + Xml_ClearedBOBibitCacheEntries_Close_Tag
                     + Xml_ClearedFOBibitCacheEntries_Open_Tag
                        + this.ClearedFOBibitCacheEntries
                     + Xml_ClearedFOBibitCacheEntries_Close_Tag
                     + Xml_EgenciaCardsCount_Open_Tag
                        + this.EgenciaCardsCount
                     + Xml_EgenciaCardsCount_Close_Tag
                     + Xml_EgenciaCardsSuccessCount_Open_Tag
                        + this.EgenciaCardsSuccessCount
                     + Xml_EgenciaCardsSuccessCount_Close_Tag
                     + Xml_EgenciaCardsErrorCount_Open_Tag
                        + this.EgenciaCardsErrorCount
                     + Xml_EgenciaCardsErrorCount_Close_Tag;
                }
                else
                {
                    // On a une exception
                    strData +=
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
                    strData +=
                 Xml_Response_Close_Tag;
            return strData;
        }


       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(this.user,
            null,
            String.Format("The following values were returned to user : {0}", GetValueMessage()),
            String.Format("Unfortunately, the process failed for the following reason: {0}", GetExceptionMessage()),
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
