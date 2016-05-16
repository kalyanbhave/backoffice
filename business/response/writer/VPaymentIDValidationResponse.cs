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
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.business.arguments.quality;
using SafeNetWS.database.row;

namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de validation d'ID VPayment
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <Status>VALID</Status>
     *      <RefusalReason>
     *          <RefusalReasonCode>Value</RefusalReasonCode>
     *          <RefusalReasonMessage>Value</RefusalReasonMessage>
     *      </RefusalReason>
     *      <IDInformation>
     *          <GenerationDate>Value</GenerationDate>
     *          <GenerationUser>Value</GenerationUser>
     *          <GenerationPercode>Value</GenerationPercode>
     *      </IDInformation>
     *   </Value>
     *   <Exceptions>
     *      <Count>0</Count>
     *      <Exception>
     *          <Code></Code>
     *          <Severity></Severity>
     *          <Type></Type>
     *          <Message></Message>
     *      </Exception>
     *    </Exceptions>
     * </Response>
     * 
     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/06/2010
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class VPaymentIDValidationResponse
    {
        public const string Status_Invalid = "INVALID";
        public const string Status_Valid = "VALID";
        
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value Status to return (serialized into string)
        private const string Xml_Response_Status_Open_Tag = "<Status>";
        private const string Xml_Response_Status_Close_Tag = "</Status>";
        // Refusal reason
        private const string Xml_Response_RefusalReason_Open_Tag = "<RefusalReason>";
        private const string Xml_Response_RefusalReason_Close_Tag = "</RefusalReason>";
        // Refusal reason code
        private const string Xml_Response_RefusalReason_Code_Open_Tag = "<RefusalReasonCode>";
        private const string Xml_Response_RefusalReason_Code_Close_Tag = "</RefusalReasonCode>";
        // Refusal reason message
        private const string Xml_Response_RefusalReason_Message_Open_Tag = "<RefusalReasonMessage>";
        private const string Xml_Response_RefusalReason_Message_Close_Tag = "</RefusalReasonMessage>";


        // ID Information
        private const string Xml_Response_IDInformation_Open_Tag = "<IDInformation>";
        private const string Xml_Response_IDInformation_Close_Tag = "</IDInformation>";
        // ID Information generation date
        private const string Xml_Response_IDInformation_GenerationDate_Open_Tag = "<GenerationDate>";
        private const string Xml_Response_IDInformation_GenerationDate_Close_Tag = "</GenerationDate>";
        // ID Information generation user
        private const string Xml_Response_IDInformation_GenerationUser_Open_Tag = "<GenerationUser>";
        private const string Xml_Response_IDInformation_GenerationUser_Close_Tag = "</GenerationUser>";
        // ID Information generation percode
        private const string Xml_Response_IDInformation_GenerationTravelerCode_Open_Tag = "<GenerationTravelerCode>";
        private const string Xml_Response_IDInformation_GenerationTravelerCode_Close_Tag = "</GenerationTravelerCode>";
        // ID Information generation traveler name
        private const string Xml_Response_IDInformation_GenerationTravelerName_Open_Tag = "<GenerationTravelerName>";
        private const string Xml_Response_IDInformation_GenerationTravelerName_Close_Tag = "</GenerationTravelerName>";




        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";

        // Exceptions
        private const string Xml_Response_Exceptions_Open_Tag = "<Exceptions>";
        private const string Xml_Response_Exceptions_Close_Tag = "</Exceptions>";

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


        private string RefusalReasonCode;
        private string RefusalReasonMessage;

        private bool valid;
        private DateTime IDGenerationDate;
        private string IDGenerationUser;
        private string IDGenerationTravelerCode;
        private string IDGenerationTravelerName;


        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;

        private string InputValue;

        public VPaymentIDValidationResponse(string input_value)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            // On garde en mémoire la valeur
            // que le client souhaite envoyer
            this.InputValue = input_value;
        }

        public void SetValid(bool status)
        {
            this.valid = status;
        }

        public bool IsValid()
        {
            return this.valid;
        }

        public void SetRefusalReason(string code, string message)
        {
            this.RefusalReasonCode = code;
            this.RefusalReasonMessage = message;
        }
        public string GetRefusalReasonCode()
        {
            return this.RefusalReasonCode;
        }
        public string GetRefusalReasonMessage()
        {
            return this.RefusalReasonMessage;
        }
        

        public void SetIDInformation(VPaymentIDData value)
        {
            this.IDGenerationDate = value.GetInsertDate();
            this.IDGenerationUser = value.GetUser();
            this.IDGenerationTravelerCode = value.GetTravelerCode();
            this.IDGenerationTravelerName = value.GetTravelerName();
        }

        public DateTime GetIDGenerationDate()
        {
            return this.IDGenerationDate;
        }
        public string GetIDGenerationUser()
        {
            return this.IDGenerationUser;
        }
        public string GetIDGenerationTravelerCode()
        {
            return this.IDGenerationTravelerCode;
        }
        public string GetIDGenerationTravelerName()
        {
            return this.IDGenerationTravelerName;
        }
        public void SetUser(UserInfo useri)
        {
            this.User = useri;
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
        /// Retourne la valeur renseigné par le client
        /// </summary>
        /// <returns>Retourne</returns>
        public string GetInputValue()
        {
            return Util.Nvl(this.InputValue, string.Empty);
        }

        /// <summary>
        /// Retourne TRUE si la réponse a une erreur
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        private bool IsError()
        {
            return (GetExceptionCount() > 0);
        }

        /// <summary>
        /// Retourne le message d'exception
        /// </summary>
        /// <returns>Message d'exception</returns>
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }


        /// <summary>
        /// Retourne les informations à retourner
        /// à la fin du traitement
        /// </summary>
        /// <returns>Valeur à retourner</returns>
        private string GetValueMessage()
        {
            return String.Format("VPaymentID status ={0}", IsValid()?Status_Valid: Status_Invalid);
        }

        /// <summary>
        /// Retour de la réponse structurée en XML
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetResponse()
        {
            // On trace la demande
            LogResponse();
            // On va maintenant contruire la réponse
            string strData = Const.XmlHeader
                + Xml_Response_Open_Tag
                        +Xml_Response_Duration_Open_Tag
                            + GetDuration()
                        + Xml_Response_Duration_Close_Tag;
                if(!IsError())
                 {
                     strData +=
                         Xml_Response_Value_Open_Tag
                            + Xml_Response_Status_Open_Tag
                                + (IsValid() ? Status_Valid : Status_Invalid)
                            + Xml_Response_Status_Close_Tag;
                     if (IsValid())
                     {
                         // Il n'y aucune erreur
                         // On va renvoyer les données
                         // et de ce fait ignorer les tag d'exception

                         strData +=
                             Xml_Response_IDInformation_Open_Tag
                                + Xml_Response_IDInformation_GenerationDate_Open_Tag
                                    + GetIDGenerationDate().ToString()
                                + Xml_Response_IDInformation_GenerationDate_Close_Tag
                                + Xml_Response_IDInformation_GenerationUser_Open_Tag
                                    + GetIDGenerationUser()
                                + Xml_Response_IDInformation_GenerationUser_Close_Tag
                                + Xml_Response_IDInformation_GenerationTravelerCode_Open_Tag
                                    + GetIDGenerationTravelerCode()
                                + Xml_Response_IDInformation_GenerationTravelerCode_Close_Tag
                                + Xml_Response_IDInformation_GenerationTravelerName_Open_Tag
                                    + GetIDGenerationTravelerName()
                                + Xml_Response_IDInformation_GenerationTravelerName_Close_Tag
                             + Xml_Response_IDInformation_Close_Tag;
                     }
                     else
                     {
                         strData +=
                                Xml_Response_RefusalReason_Open_Tag
                                   + Xml_Response_RefusalReason_Code_Open_Tag
                                       + GetRefusalReasonCode()
                                   + Xml_Response_RefusalReason_Code_Close_Tag
                                   + Xml_Response_RefusalReason_Message_Open_Tag
                                       + GetRefusalReasonMessage()
                                   + Xml_Response_RefusalReason_Message_Close_Tag
                                + Xml_Response_RefusalReason_Close_Tag;

                     }
                     strData +=
                       Xml_Response_Value_Close_Tag;
                }
                else
                {
                    // On a une exception
                    // Il faut renvoyer les tags d'exception et
                    // de ce fait ne pas ajouter les tags sur ls données
                    strData+=
                      Xml_Response_Exceptions_Open_Tag
                         + Xml_Response_Exception_Count_Open_Tag
                            + GetExceptionCount()
                         + Xml_Response_Exception_Count_Close_Tag
                         + Xml_Response_Exception_Open_Tag
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
                        + Xml_Response_Exception_Close_Tag
                     + Xml_Response_Exceptions_Close_Tag;
                }
                strData += 
                Xml_Response_Close_Tag;
            return strData;
        }

        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
       public UserInfo GetUser()
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
             String.Format(" and provided VPayment ID {0}", GetInputValue()),
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
       /// Retourne le type d'exception
       /// </summary>
       /// <returns>Type d'exception</returns>
       private string GetExceptionType()
       {
           return this.ExceptionType;
       }

       /// <summary>
       /// Retourne le code d'exception
       /// </summary>
       /// <returns>Code d'exception</returns>
       private string GetExceptionCode()
       {
           return this.ExceptionCode;
       }

       /// <summary>
       /// Retourne la gravité de l'exception
       /// </summary>
       /// <returns>Gravité exception</returns>
       private string GetExceptionSeverity()
       {
           return this.ExceptionSeverity;
       }

       /// <summary>
       /// Retourne le nombre d'erreur
       /// </summary>
       /// <returns>Nombre d'erreurs</returns>
       private int GetExceptionCount()
       {
           return this.ExceptionCount;
       }

 
    }
}
