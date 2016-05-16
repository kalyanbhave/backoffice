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
using SafeNetWS.log;

namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de vérification des numéros de cartes
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <Token>Valeur de retour</Token>
     *      <TruncatedPAN>Valeur de retour</TruncatedPAN>
     *      <Status>AUTHORISED<Status>
     *      <Information>
     *          <Code>Valeur</Code>
     *          <Message>Valeur></Message>
     *      </Information>
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
     * Date : 13/06/2010
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class CreditCardValidationResponse
    {
        
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value Token to return (serialized into string)
        private const string Xml_Response_Token_Open_Tag = "<Token>";
        private const string Xml_Response_Token_Close_Tag = "</Token>";
        // Value CardType to return (serialized into string)
        private const string Xml_Response_CardType_Open_Tag = "<CardType>";
        private const string Xml_Response_CardType_Close_Tag = "</CardType>";
        // Value truncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_Open_Tag = "<TruncatedPAN>";
        private const string Xml_Response_TruncatedPAN_Close_Tag = "</TruncatedPAN>";
        // Value status to return (serialized into string)
        private const string Xml_Response_Status_Open_Tag = "<Status>";
        private const string Xml_Response_Status_Close_Tag = "</Status>";
        private const string Xml_Response_Status_Message_Open_Tag = "<StatusMessage>";
        private const string Xml_Response_Status_Message_Close_Tag = "</StatusMessage>";
        // Value Information to return (serialized into string)
        private const string Xml_Response_Information_Open_Tag = "<Information>";
        private const string Xml_Response_Information_Close_Tag = "</Information>";
        // Value Information to return (serialized into string)
        private const string Xml_Response_Information_Code_Open_Tag = "<Code>";
        private const string Xml_Response_Information_Code_Close_Tag = "</Code>";
        // Value Information to return (serialized into string)
        private const string Xml_Response_Information_Message_Open_Tag = "<Message>";
        private const string Xml_Response_Information_Message_Close_Tag = "</Message>";
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";

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

        private string Token;
        private string TruncatedPAN;
        private string CardType;
        private string Status;
        private string InformationCode;
        private string InformationMessage;

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;

        private string InputValue;

        private bool requestLogged;

        public CreditCardValidationResponse(string input_value, bool requestLogged)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            // On garde en mémoire la valeur
            // que le client souhaite envoyer
            this.InputValue = input_value;

            this.requestLogged = requestLogged;
        }

        public void SetValues(UserInfo useri, string token, string pan, string cardType, string status, string code, string message)
        {
            SetUser(useri);
            this.TruncatedPAN = CreditCardVerifier.TruncatePan(pan);
            this.Status = status;
            this.CardType = cardType;
            this.InformationCode = code;
            this.InformationMessage = message;
            this.Token = token;

        }
   
        public void SetUser(UserInfo useri)
        {
            this.User = useri;
            // log first call
            LogRequest();
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
            return String.Format("TruncatedPAN ={0}, Status ={1}", GetTruncatedPAN(), GetStatus());
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
                    // Il n'y aucune erreur
                    // On va renvoyer les données
                    // et de ce fait ignorer les tag d'exception
                     strData +=
                       Xml_Response_Value_Open_Tag
                         /*+ Xml_Response_Token_Open_Tag
                             + GetToken()
                         + Xml_Response_Token_Close_Tag*/
                         + Xml_Response_Status_Open_Tag
                             + GetStatus()
                         + Xml_Response_Status_Close_Tag
                         + Xml_Response_Status_Message_Open_Tag
                            + GetUser().GetMessages().GetString("ValidateCCNumber.PANValid", false)
                         + Xml_Response_Status_Message_Close_Tag
                         + Xml_Response_TruncatedPAN_Open_Tag
                             + GetTruncatedPAN()
                         + Xml_Response_TruncatedPAN_Close_Tag
                         + Xml_Response_CardType_Open_Tag
                             + GetCardType()
                         + Xml_Response_CardType_Close_Tag;

                     if (!String.IsNullOrEmpty(GetInformationCode()))
                     {
                         strData +=
                          Xml_Response_Information_Open_Tag
                             + Xml_Response_Information_Code_Open_Tag
                                 + GetInformationCode()
                             + Xml_Response_Information_Code_Close_Tag
                             + Xml_Response_Information_Message_Open_Tag
                                 + GetInformationMessage()
                             + Xml_Response_Information_Message_Close_Tag
                         + Xml_Response_Information_Close_Tag;
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
                strData += 
                Xml_Response_Close_Tag;
            return strData;
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
             String.Format(" and provided {0}.", ArgsChecker.GuessTokenType(GetUser(), GetInputValue()) == ArgsChecker.TOKEN_UNKNOWN ? CreditCardVerifier.TruncatePan(GetInputValue()) : GetInputValue()),
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
       /// Retourne le token de la carte
       /// </summary>
       /// <returns>Token carte</returns>
       private string GetToken()
       {
           return this.Token;
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
       /// <summary>
       /// Retourne le numéro de carte masqué
       /// </summary>
       /// <returns>Numéro de carte masqué</returns>
       private string GetTruncatedPAN()
       {
           return this.TruncatedPAN;
       }

       /// <summary>
       /// Retourne le statut
       /// </summary>
       /// <returns>Statut</returns>
       private string GetStatus()
       {
           return this.Status;
       }
       private string GetCardType()
       {
           return this.CardType;
       }
       public void SetInformationCode(string value)
       {
           this.InformationCode = value;
       }
       public string GetInformationCode()
       {
           return this.InformationCode;
       }
       public void SetInformationMessage(string value)
       {
           this.InformationMessage = value;
       }
       public string GetInformationMessage()
       {
           return this.InformationMessage;
       }
      

       private void LogRequest()
       {
           if (!this.requestLogged)
           {
               Logger.WriteInformationToLog(String.Format("(login = {0}, IP¨= {1}) is calling {2} and provided {3}",
                   GetUser().GetLogin(), GetUser().GetClientIP(), UserInfo.GetApplicationName(GetUser().GetApplication()),
                   ArgsChecker.IsValidToken(GetUser(), GetInputValue()) ? GetInputValue() : CreditCardVerifier.TruncatePan(GetInputValue())));

               // request logged
               this.requestLogged = true;
           }
       }  
    }
}
