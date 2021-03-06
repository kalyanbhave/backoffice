﻿//==================================================================== 
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
using SafeNetWS.database.result;
using SafeNetWS.exception;

namespace SafeNetWS.business.response.writer
{
    /// <summary>
    /// Cette classe permet de construire la réponse apportée
    /// par la méthode de récupération du PAN d'une carte
    /// depuis la base hébergée par FrontOffice
    /// La réponse est structurée de la manière suivante :
    /// <?xml version="1.0" encoding="ISO-8859-1"?>
    /// <Response>
    ///   <Duration>Valeur de retour</Duration>
    ///   <Value>
    ///      <BackToken>Valeur de retour</BackToken>
    ///      <TruncatedPAN>Valeur de retour</TruncatedPAN>
    ///      <ExpirationDate>Valeur de retour</ExpirationDate>
    ///      <CardType>Valeur de retour</CardType>
    ///      <ShortCardType>Valeur de retour</ShortCardType>
    ///      <NavisionCardLabel>Valeur de retour</NavisionCardLabel>
    ///      <NavisionCardType>Valeur de retour</NavisionCardType>
    ///      <NavisionPaymentAirplus>Valeur de retour</NavisionPaymentAirplus>
    ///      <NavisionPaymentBTA>Valeur de retour</NavisionPaymentBTA>
    ///      <NavisionPaymentDiners>Valeur de retour</NavisionPaymentDiners>
    ///   </Value>
    ///   <Exception>
    ///      <Count>0</Count>
    ///      <Code></Code>
    ///      <Severity></Severity>
    ///      <Type></Type>
    ///      <Message></Message>
    ///  </Exception>
    /// </Response>
    /// 
    /// Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
    /// 
    /// Date : 13/10/2009
    /// Auteur : Samatar HASSAN
    ///  
    /// </summary>

    public class FOCardResponse
    {

        private const string Xml_Response_Open_Tag = "<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value BackToken to return (serialized into string)
        private const string Xml_Response_BackToken_Open_Tag = "<BackToken>";
        private const string Xml_Response_BackToken_Close_Tag = "</BackToken>";
        // Value truncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_Open_Tag = "<TruncatedPAN>";
        private const string Xml_Response_TruncatedPAN_Close_Tag = "</TruncatedPAN>";
        // Value ExpirationDate to return (serialized into string)
        private const string Xml_Response_ExpirationDate_Open_Tag = "<ExpirationDate>";
        private const string Xml_Response_ExpirationDate_Close_Tag = "</ExpirationDate>";
        // Value CardType to return (serialized into string)
        private const string Xml_Response_CardType_Open_Tag = "<CardType>";
        private const string Xml_Response_CardType_Close_Tag = "</CardType>";
        // Value ShortCardType to return (serialized into string)
        private const string Xml_Response_ShortCardType_Open_Tag = "<ShortCardType>";
        private const string Xml_Response_ShortCardType_Close_Tag = "</ShortCardType>";
        // Value NavisionCardLabel to return (serialized into string)
        private const string Xml_Response_NavisionCardLabel_Open_Tag = "<NavisionCardLabel>";
        private const string Xml_Response_NavisionCardLabel_Close_Tag = "</NavisionCardLabel>";
        // Value NavisionCardType to return (serialized into string)
        private const string Xml_Response_NavisionCardType_Open_Tag = "<NavisionCardType>";
        private const string Xml_Response_NavisionCardType_Close_Tag = "</NavisionCardType>";
        // Value NavisionPaymentAirplus to return (serialized into string)
        private const string Xml_Response_NavisionPaymentAirplus_Open_Tag = "<NavisionPaymentAirplus>";
        private const string Xml_Response_NavisionPaymentAirplus_Close_Tag = "</NavisionPaymentAirplus>";
        // Value NavisionPaymentBTA to return (serialized into string)
        private const string Xml_Response_NavisionPaymentBTA_Open_Tag = "<NavisionPaymentBTA>";
        private const string Xml_Response_NavisionPaymentBTA_Close_Tag = "</NavisionPaymentBTA>";
        // Value NavisionPaymentDiners to return (serialized into string)
        private const string Xml_Response_NavisionPaymentDiners_Open_Tag = "<NavisionPaymentDiners>";
        private const string Xml_Response_NavisionPaymentDiners_Close_Tag = "</NavisionPaymentDiners>";

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

        private string BackToken;
        private string TruncatedPAN;
        private string CardType;
        private string ShortCardType;
        private string ExpirationDate;
        private string NavisionCardLabel;
        private string NavisionCardType;
        private string NavisionPaymentAirplus;
        private string NavisionPaymentBTA;
        private string NavisionPaymentDiners;

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo User;
        private DateTime StartDate;

        private string InputToken;

        public FOCardResponse(string input_token)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            // On garde en mémoire le token
            // que le client souhaite envoyer
            this.InputToken = input_token;
        }

        public void SetValues(UserInfo useri, FOPanInfoResult res)
        {
            SetUser(useri);
            this.BackToken = Util.ConvertTokenToString(res.GetBOToken());
            this.TruncatedPAN = res.GetTruncatedPAN();
            this.CardType = res.GetCardType();
            this.ShortCardType = res.GetShortCardType();
            // We need to return date in format dd/MM/yyyy HH:mm:ss (for example : 31/12/2015 00:00:00)
            this.ExpirationDate = Util.ConvertDateToString(res.GetExpirationDate(), Const.DateFormat_ddMMyyyyHHmmss);
            this.NavisionCardLabel = res.GetNavisionCardLabel();
            this.NavisionCardType = res.GetNavisionCardType().ToString();
            this.NavisionPaymentAirplus = res.GetNavisionPaymentAirPlus().ToString();
            this.NavisionPaymentBTA = res.GetNavisionPaymentBTA().ToString();
            this.NavisionPaymentDiners = res.GetNavisionLodgedCard().ToString();
        }

        public void SetUser(UserInfo useri)
        {
            this.User = useri;
        }
        public UserInfo GetUser()
        {
            return this.User;
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
            return (this.ExceptionCount > 0);
        }
        private string GetExceptionMessage()
        {
            return this.ExceptionMessage;
        }
        private string GetValueMessage()
        {
            return String.Format("BackToken= {0}, TruncatedPAN= {1}", this.BackToken, this.TruncatedPAN);
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
            // On va maintenant contruire la réponse
            string strData = Const.XmlHeader
                + Xml_Response_Open_Tag
                        + Xml_Response_Duration_Open_Tag
                            + GetDuration()
                        + Xml_Response_Duration_Close_Tag;
            if (this.ExceptionCount == 0)
            {
                // Il n'y aucune erreur
                // On va renvoyer les données
                // et de ce fait ignorer les tag d'exception
                strData +=
                  Xml_Response_Value_Open_Tag
                    + Xml_Response_BackToken_Open_Tag
                        + this.BackToken
                    + Xml_Response_BackToken_Close_Tag
                    + Xml_Response_TruncatedPAN_Open_Tag
                        + this.TruncatedPAN
                    + Xml_Response_TruncatedPAN_Close_Tag
                    + Xml_Response_ExpirationDate_Open_Tag
                        + this.ExpirationDate
                    + Xml_Response_ExpirationDate_Close_Tag
                    + Xml_Response_CardType_Open_Tag
                        + this.CardType
                    + Xml_Response_CardType_Close_Tag
                    + Xml_Response_ShortCardType_Open_Tag
                        + this.ShortCardType
                    + Xml_Response_ShortCardType_Close_Tag
                     + Xml_Response_NavisionCardLabel_Open_Tag
                        + this.NavisionCardLabel
                    + Xml_Response_NavisionCardLabel_Close_Tag
                    + Xml_Response_NavisionCardType_Open_Tag
                        + this.NavisionCardType
                    + Xml_Response_NavisionCardType_Close_Tag
                    + Xml_Response_NavisionPaymentAirplus_Open_Tag
                        + this.NavisionPaymentAirplus
                    + Xml_Response_NavisionPaymentAirplus_Close_Tag
                    + Xml_Response_NavisionPaymentBTA_Open_Tag
                        + this.NavisionPaymentBTA
                    + Xml_Response_NavisionPaymentBTA_Close_Tag
                    + Xml_Response_NavisionPaymentDiners_Open_Tag
                        + this.NavisionPaymentDiners
                    + Xml_Response_NavisionPaymentDiners_Close_Tag
                + Xml_Response_Value_Close_Tag;
            }
            else
            {
                // On a une exception
                // Il faut renvoyer les tags d'exception et
                // de ce fait ne pas ajouter les tags sur ls données
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
            Services.WriteOperationStatusToLog(this.User,
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

    }
}
