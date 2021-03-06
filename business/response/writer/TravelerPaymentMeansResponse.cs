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
using SafeNetWS.database.result;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.NavService;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.business.arguments.quality;


namespace SafeNetWS.business.response.writer
{
/// <summary>
///  Cette classe permet de construire la réponse apportée
///   par la méthode de récupération d'informations hiérarchique
///   relatives au mode paiement pour un voyageur
///   La réponse est structurée de la manière suivante :
///   
/// <?xml version="1.0" encoding="ISO-8859-1"?>
/// <Response>
///      <Duration>284</Duration>
///      <Value>
///           <POS>Fr</POS>
///           <Customer>2</Customer>
///           <CostCenter>TEST</CostCenter>
///           <Percode>122</Percode>
///           <Services>
///                <RequestedService  RequestedValue  = "Air">
///                     <PaymentType>CC</PaymentType>
///                     <Origin>customer</Origin>
///                     <Service>AIR</Service>
///                     <Card>
///                          <CardType>Visa</CardType>
///                          <ShortCardType>VI</ShortCardType>
///                          <CardToken>6343872467102947</CardToken>
///                          <TruncatedCardNumber>492957XXXXXX4263</TruncatedCardNumber>
///                          <FormOfPayment>FP CCVI492957XXXXXX4263/1216</FormOfPayment>
///                          <LodgedCard>1</LodgedCard>
///                          <ExpirationDate>31/12/2016 00:00:00</ExpirationDate>
///                          <ShortExpirationDate>12/16</ShortExpirationDate>
///                          <MII></MII>
///                          <MIIIssuerCategory></MIIIssuerCategory>
///                     </Card>
///                </RequestedService>
///                <RequestedService RequestedValue = "Hotel">
///                     <PaymentType>CC</PaymentType>
///                     <Origin>customer</Origin>
///                     <Service>HOTEL</Service>
///                     <Card>
///                          <CardType>Eurocard/Mastercard</CardType>
///                          <ShortCardType>CA</ShortCardType>
///                          <CardToken>6343845789444969</CardToken>
///                          <TruncatedCardNumber>524808XXXXXX5907</TruncatedCardNumber>
///                          <FormOfPayment>FP CCCA524808XXXXXX5907/1015</FormOfPayment>
///                          <LodgedCard>1</LodgedCard>
///                          <ExpirationDate>31/10/2015 00:00:00</ExpirationDate>
///                          <ShortExpirationDate>10/15</ShortExpirationDate>
///                          <MII></MII>
///                          <MIIIssuerCategory></MIIIssuerCategory>
///                     </Card>
///                </RequestedService>
///           </Services>
///      </Value>
/// </Response>
/// 
/// 
/// </summary>
    public class TravelerPaymentMeansResponse
    {


        // Response
        private const string Xml_Response_Open_Tag = "<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";

        // Value to return (serialized into string)
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // --> POS
        private const string Xml_POS_Open_Tag = "<POS>";
        private const string Xml_POS_Close_Tag = "</POS>";
        // --> Customer
        private const string Xml_Customer_Open_Tag = "<Customer>";
        private const string Xml_Customer_Close_Tag = "</Customer>";
        // --> CostCenter
        private const string Xml_CostCenter_Open_Tag = "<CostCenter>";
        private const string Xml_CostCenter_Close_Tag = "</CostCenter>";
        // --> Percode
        private const string Xml_Percode_Open_Tag = "<Percode>";
        private const string Xml_Percode_Close_Tag = "</Percode>";
        // --> PaymentType
        private const string Xml_PaymentType_Open_Tag = "<PaymentType>";
        private const string Xml_PaymentType_Close_Tag = "</PaymentType>";
        // --> Origin
        private const string Xml_Origin_Open_Tag = "<Origin>";
        private const string Xml_Origin_Close_Tag = "</Origin>";
        // --> Services
        private const string Xml_Services_Open_Tag = "<Services>";
        private const string Xml_Services_Close_Tag = "</Services>";
        // --> Service
        private const string Xml_Service_Open_Tag = "<Service>";
        private const string Xml_Service_Close_Tag = "</Service>";
        // --> Service
        private const string Xml_Requested_Service_Close_Tag = "</RequestedService>";

        // If Payment type = CC
        // We need to take care of card informations
        // --> Service
        private const string Xml_Card_Open_Tag = "<Card>";
        private const string Xml_Card_Close_Tag = "</Card>";

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
        // --> TruncatedCardNumber
        private const string Xml_TruncatedCardNumber_Open_Tag = "<TruncatedCardNumber>";
        private const string Xml_TruncatedCardNumber_Close_Tag = "</TruncatedCardNumber>";
        // --> ExpiryDate
        private const string Xml_ExpirationDate_Open_Tag = "<ExpirationDate>";
        private const string Xml_ExpirationDate_Close_Tag = "</ExpirationDate>";
        // Value ShortExpirationDate to return (serialized into string)
        private const string Xml_ShortExpirationDate_Open_Tag = "<ShortExpirationDate>";
        private const string Xml_ShortExpirationDate_Close_Tag = "</ShortExpirationDate>";
        // Value FormOfPayment to return (serialized into string)
        private const string Xml_FormOfPayment_Open_Tag = "<FormOfPayment>";
        private const string Xml_FormOfPayment_Close_Tag = "</FormOfPayment>";

        // Value LodgedCard to return (serialized into string)
        private const string Xml_LodgedCard_Open_Tag = "<LodgedCard>";
        private const string Xml_LodgedCard_Close_Tag = "</LodgedCard>";

        // Value MerchantFlow to return (serialized into string)
        private const string Xml_MerchantFlow_Open_Tag = "<MerchantFlow>";
        private const string Xml_MerchantFlow_Close_Tag = "</MerchantFlow>";
        // Value EnhancedFlow to return (serialized into string)
        private const string Xml_EnhancedFlow_Open_Tag = "<EnhancedFlow>";
        private const string Xml_EnhancedFlow_Close_Tag = "</EnhancedFlow>";



        // Handle exceptions

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

        private int ExceptionCount;
        private string ExceptionCode;
        private string ExceptionType;
        private string ExceptionSeverity;
        private string ExceptionMessage;

        private UserInfo user;
        private DateTime StartDate;


        // Arguments
        private string argPos;
        private string argComcode;
        private string argCostCenter;
        private string argPercode;
        private string argServiceslist;

        // Result from Navision WS
        private Nav_PaymentMeans pm;

        public TravelerPaymentMeansResponse(string pos, string comcode, string costCenter, string percode, string argServiceslist)
        {
            // initialisation
            this.StartDate = DateTime.Now;
            this.argPos = String.IsNullOrEmpty(pos) ? pos : ((String.Compare(pos, "null", true) == 0) ? String.Empty : pos);
            this.argCostCenter = String.IsNullOrEmpty(costCenter) ? costCenter : ((String.Compare(costCenter, "null", true) == 0) ? String.Empty : costCenter);
            this.argComcode = String.IsNullOrEmpty(comcode) ? comcode : ((String.Compare(comcode, "null", true) == 0) ? String.Empty : comcode);
            this.argPercode = String.IsNullOrEmpty(percode) ? percode : ((String.Compare(percode, "null", true) == 0) ? String.Empty : percode);
            this.argServiceslist = String.IsNullOrEmpty(argServiceslist) ? argServiceslist : ((String.Compare(argServiceslist, "null", true) == 0) ? String.Empty : argServiceslist); 
        }
        
        /// <summary>
        /// Validate arguments
        /// We need at least POS and Comcocode
        /// </summary>
        public void ValidateArguments()
        {
            // Correct POS
            this.argPos = Util.CorrectPos(GetUser(), GetArgPos());

            // sanity check for comcode
            // it's mandatory
            ArgsChecker.ValidateComCode(GetUser(), GetArgComcode(), true);

            // Sanity check for traveler, not mandatory
            ArgsChecker.ValidateCostCenterId(GetUser(), GetArgCostCenter(), false);

            // Sanity check for Cost center id, not mandatory
            ArgsChecker.ValidatePerCode(GetUser(), GetArgPercode(), false);

            // validate service list
            Util.CorrectServices(GetUser(), GetArgServicesList());
        }


        public Nav_PaymentMeans GetNav_PaymentMeans()
        {
            return this.pm;
        }

        /// <summary>
        /// Set result from Navision ws
        /// </summary>
        /// <param name="value">NavPaymentMeans</param>
        public void SetValue(Nav_PaymentMeans value)
        {
            this.pm = value;
            // Let's check if we have an exception code
            string exceptionCode = value.NavException == null ? null : value.NavException[0].NavExceptionCode[0];

            if (!String.IsNullOrEmpty(exceptionCode))
            {
                // We have an exception here
                this.ExceptionCount = 1;
                // set exception code and message
                this.ExceptionCode = exceptionCode;
                this.ExceptionMessage = value.NavException[0].NavExceptionDesc[0];
                this.ExceptionSeverity = CCEExceptionMap.EXCEPTION_SEVERITY_ERROR;
                this.ExceptionType = CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL;
            }
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
           // On va maintenant contruire la réponse
           string strData = Const.XmlHeader
                + Xml_Response_Open_Tag
                        + Xml_Response_Duration_Open_Tag
                            + GetDuration()
                        + Xml_Response_Duration_Close_Tag;
            if (!IsError())
            {
                // Il n'y aucune erreur
                // On va renvoyer les données
                // et de ce fait ignorer les tag d'exception
                strData +=
                  Xml_Response_Value_Open_Tag
                  + Xml_POS_Open_Tag
                    + GetArgPos()
                  + Xml_POS_Close_Tag
                  + Xml_Customer_Open_Tag
                    + GetArgComcode()
                  + Xml_Customer_Close_Tag
                  + Xml_CostCenter_Open_Tag
                    + Util.Nvl(GetArgCostCenter(), string.Empty)
                  + Xml_CostCenter_Close_Tag
                  + Xml_Percode_Open_Tag
                    + GetArgPercode()
                  + Xml_Percode_Close_Tag
                  + Xml_Services_Open_Tag;

                foreach (RequestedService rs in this.pm.RequestedService)
                {
                    strData += "<RequestedService RequestedValue=\"" + Util.Nvl(rs.RequestedValue, Const.ServiceALL) + "\">"
                          + Xml_PaymentType_Open_Tag + rs.PaymentType + Xml_PaymentType_Close_Tag
                          + Xml_Origin_Open_Tag + rs.Origin + Xml_Origin_Close_Tag
                          + Xml_Service_Open_Tag + rs.Service[0] + Xml_Service_Close_Tag;

                    if (!String.IsNullOrEmpty(rs.PaymentType) && rs.PaymentType.Equals(Const.PaymentTypeCreditCardShort) && !String.IsNullOrEmpty(rs.Card[0].CardToken))
                    {
                        // We have a credit card
                        // prepare expiration date
                        DateTime expDate=Util.GetLastDayOfThisMonth(Util.ConvertStringToDate(rs.Card[0].ExpirationDate, Const.DateFormat_MMyyyy));
                        string shortExpirationDate = Util.ConvertDateToString(expDate, Const.DateFormat_MMyy);
                        // Set card type
                        string cardType = rs.Card[0].CardType[0];
                        string shortCardType = CreditCardVerifier.GetShortCardTypeFromNavisionCardType(rs.Card[0].CardType[0]);
                        // Return the MII (first digit)
                        int mii = String.IsNullOrEmpty(rs.Card[0].TruncatedPAN) ? -1 : Util.ConvertStringToInt(rs.Card[0].TruncatedPAN.Substring(0, 1));

                        strData += Xml_Card_Open_Tag
                            + Xml_CardType_Open_Tag + cardType + Xml_CardType_Close_Tag
                            + Xml_ShortCardType_Open_Tag + CreditCardVerifier.GetShortCardTypeFromNavisionCardType(cardType) + Xml_ShortCardType_Close_Tag
                            + Xml_CardToken_Open_Tag + rs.Card[0].CardToken + Xml_CardToken_Close_Tag
                            + Xml_TruncatedCardNumber_Open_Tag + rs.Card[0].TruncatedPAN + Xml_TruncatedCardNumber_Close_Tag
                            + Xml_FormOfPayment_Open_Tag +
                                String.Format("FP CC{0}{1}/{2}", shortCardType, rs.Card[0].TruncatedPAN, shortExpirationDate.Replace("/", string.Empty))
                            + Xml_FormOfPayment_Close_Tag
                            + Xml_ExpirationDate_Open_Tag + Util.ConvertExpirationDateToString(expDate) + Xml_ExpirationDate_Close_Tag
                            + Xml_ShortExpirationDate_Open_Tag + shortExpirationDate + Xml_ShortExpirationDate_Close_Tag
                            + Xml_LodgedCard_Open_Tag + (rs.Card[0].LodgedCard[0].Equals("true") ? "1" : "0") + Xml_LodgedCard_Close_Tag
                            + Xml_MII_Open_Tag + mii.ToString() + Xml_MII_Close_Tag
                            + Xml_MIIIssuerCategory_Open_Tag + CreditCardVerifier.GetMIIIssuerCategory(mii) + Xml_MIIIssuerCategory_Close_Tag
                            //+ Xml_MerchantFlow_Open_Tag + rs.Card[0].MerchandFlow + Xml_MerchantFlow_Close_Tag
                            //+ Xml_EnhancedFlow_Open_Tag + rs.Card[0].EnhanceFlow + Xml_EnhancedFlow_Close_Tag

                       + Xml_Card_Close_Tag; 
                    }

                    strData += Xml_Requested_Service_Close_Tag;
                }
               
                strData += 
                Xml_Services_Close_Tag
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
       /// Il y a t-il des exceptions dans le traitement
       /// Si on a au  moins une exception
       /// alors la réponse va être flaggée en erreur
       /// </summary>
       /// <returns>TRUE ou FALSE</returns>
       private bool IsError()
       {
           return (this.ExceptionCount > 0);
       }

        /// <summary>
        /// Retourne le message d'exception
        /// Cette méthode est appelée lorsque la fonction
        /// IsError return e TRUE
        /// </summary>
        /// <returns>Message d'exception</returns>
       private string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }
       private string GetValueMessage()
       {
           return String.Format("Payment means for all services");
       }
       public void SetUser(UserInfo useri)
       {
           this.user = useri;
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
       /// Retourne les informatiosn fournies
       /// par le client lors de l'appel
       /// </summary>
       /// <returns>Informations fournies</returns>
       private string GetInputValue()
       {
           return String.Format("pos = {0}, comcode = {1}, percode = {2}, services = {3}", GetArgPos(),GetArgComcode(), GetArgPercode(), GetArgServicesList());
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
       /// Retourne le nombre d'erreur
       /// </summary>
       /// <returns>Nombre d'erreurs</returns>
       private int GetExceptionCount()
       {
           return this.ExceptionCount;
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
       /// Retourne le code client
       /// </summary>
       /// <returns>Code client</returns>
       public string GetArgComcode()
       {
           return this.argComcode;
       }

       /// <summary>
       /// Retourne le code voyageur
       /// </summary>
       /// <returns>Code voyageur</returns>
       public string GetArgPercode()
       {
           return this.argPercode;
       }

        /// <summary>
       /// Retourne le centre de cout
       /// </summary>
       /// <returns>cc1</returns>
       public string GetArgCostCenter()
       {
           return this.argCostCenter;
       }

        
       /// <summary>
       /// Retourne le marché
       /// </summary>
       /// <returns>Marché</returns>
       public string GetArgPos()
       {
           return this.argPos;
       }

       /// <summary>
       /// Retourne le marché
       /// </summary>
       /// <returns>Marché</returns>
       public string GetArgServicesList()
       {
           return this.argServiceslist;
       }

    }
}
