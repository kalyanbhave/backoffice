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
using SafeNetWS.business.arguments.quality;
using SafeNetWS.creditcard;


namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de récupération d'informations hiérarchique
     * relatives aux cartes
     * La réponse est structurée de la manière suivante :
     * <ECTEGetUserBookingPaymentRS xmlns:xsi = "http://www.w3.org/2001/XMLSchema-instance\">
     * <ContextRS>
     *     <Status>""</Status>
     *     <ErrorCode>-1</ErrorCode>
     *     <SeverityLevel>0</SeverityLevel>
     *     <Message>Message</Message>
     *     <Duration>Valeur de retour</Duration>
     * </ContextRS>
     * <Response>
     *     <PaymentCard Service = "AIR">
     *          <CardType>Amex</CardType>
     *          <ShortCardType>AX</ShortCardType>
     *          <MII>Valeur de retour</MII>
     *          <MIIIssuerCategory>Valeur de retour</MIIIssuerCategory>
     *          <CardToken>Valeur de retour</CardToken>
     *          <CardNumber>Valeur de retour</CardNumber>
     *          <TruncatedCardNumber>Valeur de retour</TruncatedCardNumber>
     *          <StartDate>maDate</StartDate>
     *          <ExpiryDate>maDate</ExpiryDate>
     *          <ShortExpiryDate>maDate</ShortExpiryDate>
     *          <FormOfPayment>Valeur de retour</FormOfPayment>
     *          <CVV2>Cvv2</CVV2>
     *          <Origin>Origin</Origin>
     *          <FinancialFlow>Origin</FinancialFlow>
     *     </PaymentCard>
     * </Response>
     * </ECTEGetUserBookingPaymentRS>
     * 
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class UserBookingPaymentResponse
    {
        // ECTEGetUserBookingPaymentRS
        private const string Xml_ECTEGetUserBookingPaymentRS_Open_Tag = "<ECTEGetUserBookingPaymentRS xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
        private const string Xml_ECTEGetUserBookingPaymentRS_Close_Tag = "</ECTEGetUserBookingPaymentRS>";

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
        // --> PaymentCard
        private const string Xml_PaymentCard_Open_Tag = "<PaymentCard Service=\"";
        private const string Xml_PaymentCard_Close_Tag = "</PaymentCard>";
        // --> CardType
        private const string Xml_CardType_Open_Tag = "<CardType>";
        private const string Xml_CardType_Close_Tag = "</CardType>";
        // --> ShortCardType
        private const string Xml_ShortCardType_Open_Tag = "<ShortCardType>";
        private const string Xml_ShortCardType_Close_Tag = "</ShortCardType>";
        // Value MII to return (serialized into string)
        private const string Xml_MII_Open_Tag = "<MII>";
        private const string Xml_MII_Close_Tag = "</MII>";
        // Value MII to return (serialized into string)
        private const string Xml_MIIIssuerCategory_Open_Tag = "<MIIIssuerCategory>";
        private const string Xml_MIIIssuerCategory_Close_Tag = "</MIIIssuerCategory>";
        // --> CardToken
        private const string Xml_CardToken_Open_Tag = "<CardToken>";
        private const string Xml_CardToken_Close_Tag = "</CardToken>";
        // --> CardTokenType
        private const string Xml_CardTokenType_Open_Tag = "<CardTokenType>";
        private const string Xml_CardTokenType_Close_Tag = "</CardTokenType>";
        
        // --> CardNumber
        private const string Xml_CardNumber_Open_Tag = "<CardNumber>";
        private const string Xml_CardNumber_Close_Tag = "</CardNumber>";
        // --> TruncatedCardNumber
        private const string Xml_TruncatedCardNumber_Open_Tag = "<TruncatedCardNumber>";
        private const string Xml_TruncatedCardNumber_Close_Tag = "</TruncatedCardNumber>";
        // --> StartDate
        private const string Xml_StartDate_Open_Tag = "<StartDate>";
        private const string Xml_StartDate_Close_Tag = "</StartDate>";
        // --> ExpiryDate
        private const string Xml_ExpiryDate_Open_Tag = "<ExpiryDate>";
        private const string Xml_ExpiryDate_Close_Tag = "</ExpiryDate>";
        // Value ShortExpirationDate to return (serialized into string)
        private const string Xml_ShortExpirationDate_Open_Tag = "<ShortExpirationDate>";
        private const string Xml_ShortExpirationDate_Close_Tag = "</ShortExpirationDate>";
        // --> CVV2
        private const string Xml_CVV2_Open_Tag = "<CVV2>";
        private const string Xml_CVV2_Close_Tag = "</CVV2>";
        // --> Origin
        private const string Xml_Origin_Open_Tag = "<Origin>";
        private const string Xml_Origin_Close_Tag = "</Origin>";
        // Value FormOfPayment to return (serialized into string)
        private const string Xml_Response_FormOfPayment_Open_Tag = "<FormOfPayment>";
        private const string Xml_Response_FormOfPayment_Close_Tag = "</FormOfPayment>";
        // Value FinancialFlow to return (serialized into string)
        private const string Xml_FinancialFlow_Open_Tag = "<FinancialFlow>";
        private const string Xml_FinancialFlow_Close_Tag = "</FinancialFlow>";

        // Values
        private string Service;
        private string CardType;
        private string CardType2;
        private string MII;
        private string MIIIssuerCategory;
        private long CardToken;
        private string CardNumber;
        private string TruncatedCardNumber;
        private string StartDate;
        private string ShortExpirationDate;
        private string ExpiryDate;
        private string CVV2;
        private string Origin;
        private string CardTokenType;
        private string FormOfPayment;
        private int LodgedCard;
        private string FinancialFlow;

        private string ErrorCode;
        private string ExceptionSeverity;
        private string ExceptionType;
        private string ExceptionMessage;
        private int ExceptionCount;


        private UserInfo user;
        private DateTime BeginDate;


        private string InputValue;

        public UserBookingPaymentResponse(string inputValue)
        {
            // Initialisation
            this.BeginDate = DateTime.Now;
            this.CardToken = -1;
            this.ErrorCode = "0";
            this.CardTokenType = ArgsChecker.TOKEN_TYPE_BO;
            this.LodgedCard = 0;
            this.InputValue = Util.RemoveCRLFTAB(inputValue);
        }

        /// <summary>
        /// Affectation des informations sur la carte
        /// </summary>
        /// <param name="user">Informations sur le client</param>
        /// <param name="pan">Numéro de carte</param>
        /// <param name="rs">Informations hiérarchiques extraites de Navision</param>
        public void SetValues(UserInfo user, string pan, UserBookingPaymentRSResult rs)
        {
            SetUser(user);
            CardInfos ci=Services.CheckCreditCard(GetUser(), pan);
            this.Service = rs.GetService();
            this.CardType = rs.GetCardType();
            this.CardType2 = rs.GetCardType2();
            this.MII = Util.ConvertTokenToString(ci.GetMII());
            this.MIIIssuerCategory = ci.GetMIIIssuerCategory();
            this.CardToken = rs.GetToken();
            this.CardNumber = pan;
            this.TruncatedCardNumber = rs.GetTruncatedPAN();
            this.StartDate = Util.ConvertDateToString(rs.GetCreationDate(), Const.DateFormat_ddMMyyyyHHmmss);
            this.ExpiryDate = Util.ConvertDateToString(rs.GetExpirationDate(), Const.DateFormat_ddMMyyyyHHmmss);
            this.ShortExpirationDate = rs.GetShortExpirationDate();
            this.CVV2 = rs.GetCvv2();
            this.Origin = rs.GetOrigin();
            this.LodgedCard = rs.IsLodgedCard();
            this.FinancialFlow = rs.GetFinancialFlow();
            SetFormOfPayment(); 
        }

        /// <summary>
        /// Affectation des informations sur la carte
        /// </summary>
        /// <param name="user">Informations sur le client</param>
        /// <param name="service">Service</param>
        /// <param name="info">Informations retournées depuis le FrontOffice</param>
        public void SetValues(UserInfo user, string service, FOPanInfoResult info)
        {
            SetUser(user);
            CardInfos ci = Services.CheckCreditCard(GetUser(), info.GetPAN());
            this.CardTokenType = ArgsChecker.TOKEN_TYPE_FO;
            this.Service = service;
            this.CardType = info.GetCardType();
            this.CardType2 = info.GetShortCardType();
            this.MII = Util.ConvertTokenToString(ci.GetMII());
            this.MIIIssuerCategory = ci.GetMIIIssuerCategory();
            this.CardToken = info.GetBOToken();
            this.CardNumber = info.GetPAN();
            this.TruncatedCardNumber = info.GetTruncatedPAN();
            this.StartDate = null;
            this.ExpiryDate =  Util.ConvertExpirationDateToString(info.GetExpirationDate());
            this.ShortExpirationDate = info.GetShortExpirationDate();
            this.CVV2 = null;
            this.Origin = "traveller";
            this.FinancialFlow = null;
            SetFormOfPayment(); 

        }
        /// <summary>
        /// Affectation des informations sur la carte
        /// </summary>
        /// <param name="user">Informations sur le client</param>
        /// <param name="service">Service</param>
        /// <param name="pi">Informations carte depuis Navision</param>
        public void SetValues(UserInfo user, string service, ExtendedPanInfoResult pi)
        {
            SetUser(user);
            this.Service = service;
            this.CardType = pi.GetCardType();
            this.CardType2 = pi.GetShortCardType();
            this.MII = Util.ConvertTokenToString(pi.GetMII());
            this.MIIIssuerCategory = pi.GetMIIIssuerCategory();
            this.CardToken = pi.GetToken();
            this.CardNumber = pi.GetPan();
            this.TruncatedCardNumber = pi.GetTruncatedPan();
            this.StartDate = null;
            this.ExpiryDate = pi.GetExpirationDate();
            this.ShortExpirationDate = pi.GetShortExpirationDate();
            this.CVV2 = null;
            this.Origin = "traveller";
            this.FinancialFlow = pi.GetMerchantFlow();
            SetFormOfPayment(); 

        }

        /// <summary>
        /// Affectation du nombre d'exception
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        /// <param name="count">Nombre d'erreur</param>
        private void SetExceptionCount(UserInfo useri,int count)
        {
            SetUser(useri);
            this.ErrorCode = "-1";
            this.ExceptionCount = count;
            // Ok, on a construire la réponse
            // mais avant on va extraire les différents informations
            // depuis le message d'exception
            SplitException();
        }

        /// <summary>
        /// Affectation de l'exception
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        /// <param name="message">Message d'exception</param>
        public void SetException(UserInfo useri, string message)
        {
            this.ExceptionMessage = message;
            SetExceptionCount(useri, 1);
        }

        /// <summary>
        /// Affectation de l'exception
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        /// <param name="exception">Exception</param>
        public void SetException(UserInfo useri,Exception exception)
        {
            SetException(useri, exception.Message);
        }

        /// <summary>
        /// Affectation de la forme de paiement
        /// </summary>
        private void SetFormOfPayment()
        {
            this.FormOfPayment = String.Format("FP CC{0}{1}/{2}", GetCardType2(), GetCardNumber(), GetShortExpirationDate().Replace("/", string.Empty));
        }

        /// <summary>
        /// Retourne la durée du traitement
        /// </summary>
        /// <returns>Durée (en ms)</returns>
        private string GetDuration()
        {
            return Util.GetDuration(this.BeginDate).ToString();
        }

        /// <summary>
        /// Returns financial flow
        /// </summary>
        /// <returns>Financial flow</returns>
        private string GetFinancialFlow()
        {
            return this.FinancialFlow;
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
            + Xml_ECTEGetUserBookingPaymentRS_Open_Tag
            +
                Xml_Context_Open_Tag
                    + Xml_Context_Status_Open_Tag
                        + string.Empty
                    + Xml_Context_Status_Close_Tag
                    + Xml_Context_ErrorCode_Open_Tag
                        + GetErrorCode()
                    + Xml_Context_ErrorCode_Close_Tag
                    + Xml_Context_SeverityLevel_Open_Tag
                        + "0"
                    + Xml_Context_SeverityLevel_Close_Tag
                    + Xml_Context_Message_Open_Tag
                        + GetExceptionMessage()
                    + Xml_Context_Message_Close_Tag
                    + Xml_Context_Duration_Open_Tag
                        + GetDuration()
                    + Xml_Context_Duration_Close_Tag
                + Xml_Context_Close_Tag;

            if (!IsError())
            {
                // On va ajouter les informations sur la carte
                strData +=
                Xml_Response_Open_Tag
                    + Xml_PaymentCard_Open_Tag
                            + GetService() + "\">"
                        + Xml_CardType_Open_Tag
                            + GetCardType()
                        + Xml_CardType_Close_Tag
                        + Xml_ShortCardType_Open_Tag
                            + GetCardType2()
                        + Xml_ShortCardType_Close_Tag;
                    if (!String.IsNullOrEmpty(GetMII()))
                    {
                        strData +=
                        Xml_MII_Open_Tag
                           + GetMII()
                       + Xml_MII_Close_Tag
                       + Xml_MIIIssuerCategory_Open_Tag
                          + GetMIIIssuerCategory()
                       + Xml_MIIIssuerCategory_Close_Tag;
                    }
                        strData +=
                         Xml_CardToken_Open_Tag
                            + Util.ConvertTokenToString(GetCardToken())
                        + Xml_CardToken_Close_Tag
                        + Xml_CardTokenType_Open_Tag
                            + GetCardTokenType()
                        + Xml_CardTokenType_Close_Tag
                        + Xml_CardNumber_Open_Tag
                            + GetCardNumber()
                        + Xml_CardNumber_Close_Tag
                        + Xml_TruncatedCardNumber_Open_Tag
                            + GetTruncatedCardNumber()
                        + Xml_TruncatedCardNumber_Close_Tag;
                    if (!String.IsNullOrEmpty(this.StartDate))
                    {
                        strData +=
                        Xml_StartDate_Open_Tag
                             + this.StartDate
                        + Xml_StartDate_Close_Tag;
                    }
                    if (!String.IsNullOrEmpty(GetExpirationDate()))
                    {
                        strData +=
                        Xml_ExpiryDate_Open_Tag
                             + GetExpirationDate()
                        + Xml_ExpiryDate_Close_Tag
                        + Xml_ShortExpirationDate_Open_Tag
                            + GetShortExpirationDate()
                        + Xml_ShortExpirationDate_Close_Tag;
                    }
                    strData +=
                        Xml_Response_FormOfPayment_Open_Tag
                            + GetFormOfPayment()
                        + Xml_Response_FormOfPayment_Close_Tag;
                    if (!String.IsNullOrEmpty(GetCVV2()))
                    {
                        strData +=
                        Xml_CVV2_Open_Tag
                             + GetCVV2()
                        + Xml_CVV2_Close_Tag;
                    }
                    if (!String.IsNullOrEmpty(GetOrigin()))
                    {
                        strData +=
                        Xml_Origin_Open_Tag
                             + GetOrigin()
                        + Xml_Origin_Close_Tag;
                    }

                    if (!String.IsNullOrEmpty(GetFinancialFlow()))
                    {
                        strData +=
                        Xml_FinancialFlow_Open_Tag
                             + GetFinancialFlow()
                        + Xml_FinancialFlow_Close_Tag;
                    }
                    strData +=
                    Xml_PaymentCard_Close_Tag
               + Xml_Response_Close_Tag;
            }
            
            strData += Xml_ECTEGetUserBookingPaymentRS_Close_Tag;
            return Util.HtmlEncode(strData);
        }

        /// <summary>
        /// Retourne l'origine
        /// de la carte
        /// </summary>
        /// <returns>Origine de la carte</returns>
        private string GetOrigin()
        {
            return this.Origin;
        }

        /// <summary>
        /// Retourne le type de carte
        /// </summary>
        /// <returns>Type de carte</returns>
        private string GetCardTokenType()
        {
            return CardTokenType;
        }

        /// <summary>
        /// Retourne le service
        /// </summary>
        /// <returns>Service</returns>
        private string GetService()
        {
            return this.Service;
        }

        /// <summary>
        /// Retourne la MII
        /// </summary>
        /// <returns>MII</returns>
        public string GetMII()
        {
            return this.MII;
        }

        /// <summary>
        /// Return 1 if it's  lodged card
        /// </summary>
        /// <returns>1 or 0</returns>
        public int IsLodgedCard()
        {
            return this.LodgedCard;
        }

        /// <summary>
        /// Retourne la categorie MII Issuer
        /// </summary>
        /// <returns>MII Issuer Category</returns>
        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }
       /// <summary>
       /// Retourne le moyen de paiement
       /// </summary>
       /// <returns>Moyen de paiement</returns>
        private string GetFormOfPayment()
        {
            return this.FormOfPayment;
        }
        /// <summary>
        /// Retourne le CVC
        /// </summary>
        /// <returns>CVC</returns>
        private string GetCVV2()
        {
            return this.CVV2;
        }


        /// <summary>
        /// Retourne le numéro de carte
        /// </summary>
        /// <returns>Numéro de carte</returns>
        private string GetCardNumber()
        {
            return this.CardNumber;
        }

        /// <summary>
        /// Retourne le numéro de carte masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        private string GetTruncatedCardNumber()
        {
            return this.TruncatedCardNumber;
        }

        /// <summary>
        /// Retourne le numéro de carte
        /// </summary>
        /// <returns>Numéro de carte</returns>
        private long GetCardToken()
        {
            return this.CardToken;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public string GetExpirationDate()
        {
            return this.ExpiryDate;
        }

        /// <summary>
        /// Retourne la date d'expiration courte
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }
        /// <summary>
        /// Retourne le type de carte
        /// </summary>
        /// <returns>Numéro étendu</returns>
        public string GetCardType()
        {
            return this.CardType;
        }

        /// <summary>
        /// Retourne le type de carte court
        /// </summary>
        /// <returns>Type de carte</returns>
        private string GetCardType2()
        {
            return this.CardType2;
        }
 
        /// <summary>
        /// Returne TRUE si la demande
        /// n'a pas été satisfaite
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
       private bool IsError()
       {
           return (this.ExceptionCount > 0 || Util.IsEmptyToken(this.CardToken));
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
        /// Retourne l'information sur la demande
        /// du client
        /// Cette information va être envoyée
        /// vers la trace (syslog)
        /// </summary>
        /// <returns></returns>
       private string GetValueMessage()
       {
           return String.Format("Token ={0} Truncated Pan={1}", GetToken(), GetTruncatedCardNumber()); 
       }
        /// <summary>
        /// Affectation du compte utilisateur
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
       public void SetUser(UserInfo useri)
       {
           this.user = useri;
       }

        /// <summary>
        /// Retourne le token
        /// </summary>
        /// <returns>Token</returns>
       public long GetToken()
       {
           return this.CardToken;
       }

        /// <summary>
        /// Retourne le code erreur
        /// </summary>
        /// <returns>Code erreur</returns>
       private string GetErrorCode()
       {
           return IsError() ? (Util.IsDigit(this.ErrorCode)?this.ErrorCode: "-1") : "0";
       }

       /// <summary>
       /// Retourne le compte utilisateur
       /// </summary>
       /// <returns>Compte utilisateur</returns>
 
       public UserInfo GetUser()
       {
           return this.user;
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


       public string GetShortCardType()
       {
           return this.CardType2;
       }


       public string GetTruncatedPan()
       {
           return this.TruncatedCardNumber;
       }

       public string GetExpiryDate()
       {
           return this.ExpiryDate;
       }


    }
}
