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
using SafeNetWS.creditcard;
using SafeNetWS.login;
using SafeNetWS.exception;

namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de récupération du PAN d'une carte
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <Pan>Valeur de retour</Pan>
     *      <TruncatedPAN>Valeur de retour</TruncatedPAN>
     *      <CardType>Valeur de retour</CardType>
     *      <ShortCardType>Valeur de retour</ShortCardType>
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
    public class PanResponse
    {
        
        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value PAN to return (serialized into string)
        private const string Xml_Response_PAN_Open_Tag = "<Pan>";
        private const string Xml_Response_PAN_Close_Tag = "</Pan>";
        // Value truncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_Open_Tag = "<TruncatedPAN>";
        private const string Xml_Response_TruncatedPAN_Close_Tag = "</TruncatedPAN>";
        // Value cardType to return (serialized into string)
        private const string Xml_Response_CardType_Open_Tag = "<CardType>";
        private const string Xml_Response_CardType_Close_Tag = "</CardType>";
        // Value ShortCardType to return (serialized into string)
        private const string Xml_Response_ShortCardType_Open_Tag = "<ShortCardType>";
        private const string Xml_Response_ShortCardType_Close_Tag = "</ShortCardType>";
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

        private string PAN;
        private string TruncatedPAN;
        private string cardType;
        private string shortCardType;

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;

        private string InputToken;

        public PanResponse(string input_token)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            // On garde en mémoire le token
            // que le client souhaite envoyer
            this.InputToken = input_token;
        }

        public void SetValues(UserInfo useri, string pan)
        {
            SetUser(useri);
            this.PAN = pan;

            // On vérifier le numéro de carte (test de Luhn)
            CardInfos ci = Services.CheckCreditCard(useri, pan, Const.EmptyDate, null, null, string.Empty, false,
            //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 
                   // false, null, null, null, null, false, 0);
                    false, null, null, null, null, 0, string.Empty);
            //>>EGE-76833 : [BO] Lodged Card - First card rejected on 1€ check 

            this.TruncatedPAN = ci.GetTruncatedPAN();//CreditCardVerifier.TruncatePan(pan);
            this.shortCardType = ci.GetShortCardType();
            this.cardType = ci.GetCardType();
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

        private bool IsError()
        {
            return (GetExceptionCount() > 0);
        }
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }
        private string GetValueMessage()
        {
            return "TruncatedPAN = " + GetTruncatedPAN();
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
                    strData+= 
                      Xml_Response_Value_Open_Tag
                        +Xml_Response_PAN_Open_Tag
                            + GetPan()
                        + Xml_Response_PAN_Close_Tag
                        + Xml_Response_TruncatedPAN_Open_Tag
                            + GetTruncatedPAN()
                        + Xml_Response_TruncatedPAN_Close_Tag
                        + Xml_Response_CardType_Open_Tag
                            + GetCardType()
                        + Xml_Response_CardType_Close_Tag
                        + Xml_Response_ShortCardType_Open_Tag
                            + GetShortCardType()
                        + Xml_Response_ShortCardType_Close_Tag
                    + Xml_Response_Value_Close_Tag;
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
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(),
             String.Format(" and provided Token={0}", Util.Nvl(this.InputToken, string.Empty)),
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
       /// Retourne le numéro de carte
       /// </summary>
       /// <returns>Numéro de carte</returns>
       private string GetPan()
       {
           return this.PAN;
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
       /// Retourne le type de carte
       /// </summary>
       /// <returns>Type de carte</returns>
       private string GetCardType()
       {
           return this.cardType;
       }

       /// <summary>
       /// Retourne le type de carte court
       /// </summary>
       /// <returns>Type de carte court</returns>
       private string GetShortCardType()
       {
           return this.shortCardType;
       }
    }
}
