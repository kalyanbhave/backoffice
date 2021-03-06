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
using System.Configuration;
using SafeNetWS.utils;
using SafeNetWS.login;


namespace SafeNetWS.creditcard.creditcardvalidator.bibit
{

    /// <summary>
    /// Cette classer permet de construire une requête d'intérogation
    /// du service de paiement de RBS (Royal Bank of Scotland) pour valider
    /// les cartes BIBIT
    /// La requête est sous la forme suivante:
    /// 
    /// <?xml version="1.0"?>
    /// <!DOCTYPE paymentService PUBLIC "-//RBS WorldPay//DTD RBS WorldPay PaymentService v1//EN" "http://dtd.wp3.rbsworldpay.com/paymentService_v1.dtd">
    /// <paymentService version = "1.4" merchantCode = "EXECTAUTH">
    ///    <submit>
    ///       <order orderCode = "T0211010">
    ///          <description>20 English Roses from MYMERCHANT Webshops</description>
    ///          <amount value = "1400" currencyCode = "GBP" exponent = "2"/>
    ///          <paymentDetails>
    ///                  <VISA-SSL>
    ///                      <cardNumber>4444333322221111</cardNumber>
    ///                       <expiryDate>
    ///                          <date month = "09" year = "2009"/>
    ///                       </expiryDate>
    ///                       <cardHolderName>J. Shopper</cardHolderName>
    ///                   </VISA-SSL>
    ///                   <session shopperIPAddress = "194.98.196.2" id = "02l5ui8ib1"/>
    ///           </paymentDetails>
    ///       </order>
    ///   </submit>
    /// </paymentService>
    /// 
    /// 
    /// 
    /// </summary>
    public class RBSPaymentServiceRequest
    {
        private const string DocType = "<!DOCTYPE paymentService PUBLIC \"-//RBS WorldPay//DTD RBS WorldPay PaymentService v1//EN\" \"http://dtd.wp3.rbsworldpay.com/paymentService_v1.dtd\">";
        private const string Xml_PaymentService_Open = "<paymentService version = \"1.4\" merchantCode = \"{0}\">";
        private const string Xml_PaymentService_Submit =
           "<submit>"
            + "<order orderCode = \"{0}\">"
                + "<description>{1}</description>"
                + "<amount value = \"{2}\" currencyCode = \"{3}\" exponent = \"{4}\"/>"
                + "<paymentDetails>"
                    + "<{5}>"   // VISA ou MASTERCARD
                        + "<cardNumber>{6}</cardNumber>"
                        + "<expiryDate>"
                            + "<date month = \"{7}\" year = \"{8}\"/>"
                        + "</expiryDate>"
                        + "<cardHolderName>{9}</cardHolderName>"
                    + "</{5}>"
                    + "<session shopperIPAddress = \"{10}\" id = \"{11}\"/>"
                + "</paymentDetails>"
            + "</order>"
          + "</submit>";
        private const string Xml_PaymentService_Close = "</paymentService>";



        private string MerchandCode;
        private string OrderCode;
        private string Description;
        private decimal Amount;
        private string CurrencyCode;
        private int Exponent;
        // Détail paiement
        private string PaymentMethod;
        private string CardNumber;
        private string ExpirationDateMonth;
        private string ExpirationDateYear;
        private string CardHolderName;
        private string ShopperIPAddress;
        private string SessionId;

        public RBSPaymentServiceRequest(UserInfo user, CardInfos card, bool sendZeroAmount)
        {
            // Initialisation
            SetValues(user, card.GetPOS(), card.GetCardNumber(), card.GetExpirationDate(), card.GetHolderName(),
                card.GetRBSPaymentMethod(), sendZeroAmount);
        }

        public RBSPaymentServiceRequest(UserInfo user, string pos, string crediCardNumber, DateTime expirationDate,
            string holderName, string paymentMethod, bool sendZeroAmount)
        {
            // Initialisation
            SetValues(user, pos, crediCardNumber, expirationDate, holderName,
                paymentMethod, sendZeroAmount);
        }

        private void SetValues(UserInfo user,string pos, string crediCardNumber, DateTime expirationDate,
            string holderName, string paymentMethod, bool sendZeroAmount)
        {
            // Initialisation
            SetMerchandCode(BibitVerifier.RBSServiceLogin);
            SetAmount(sendZeroAmount?000:100);
            SetExponent(2);
            SetShopperIPAddress(BibitVerifier.RBSRequestShopperIPAddress);
            SetSessionId(BibitVerifier.RBSRequestSessionId);
            SetOrderCode(Util.GetNewGuidValue());
            SetDescription(BibitVerifier.RBSRequestDescription);
            SetCardNumber(crediCardNumber);
            SetExpirationDate(expirationDate);
            SetCurrencyCode(Util.GetCurrencyFromPos(Util.CorrectPos(user, pos)));
            SetCardHolderName(Util.Nvl(holderName, BibitVerifier.RBSRequestDefaultHolderName));
            SetPaymentMethod(paymentMethod);
        }

        /// <summary>
        /// Affectation de la date d'expiration date
        /// </summary>
        /// <param name="expirationDate">Date d'expiration</param>
        public void SetExpirationDate(DateTime expirationDate)
        {
            this.ExpirationDateMonth = expirationDate.ToString(Const.DateFormat_MM);
            this.ExpirationDateYear = expirationDate.ToString(Const.DateFormat_YYYY);
        }

        /// <summary>
        /// Affectation du numéro de la carte
        /// </summary>
        /// <param name="cardnumber">Numéro de carte</param>
        public void SetCardNumber(string cardnumber)
        {
            this.CardNumber = cardnumber;
        }
        /// <summary>
        /// Affectation de la devise
        /// </summary>
        /// <param name="cardnumber">Devise</param>
        public void SetCurrencyCode(string currencyCode)
        {
            this.CurrencyCode = currencyCode;
        }
        /// <summary>
        /// Affectation du card holder
        /// </summary>
        /// <param name="cardnumber">Card holder</param>
        public void SetCardHolderName(string cardHolderName)
        {
            this.CardHolderName = cardHolderName;
        }
        /// <summary>
        /// Affectation du exponent
        /// </summary>
        /// <param name="exponent">exponent</param>
        public void SetExponent(int exponent)
        {
            this.Exponent = exponent;
        }
        /// <summary>
        /// Retourne le code marchand
        /// </summary>
        /// <returns>Code marchand</returns>
        public string GetMerchandCode()
        {
            return MerchandCode;
        }

        /// <summary>
        /// Affectation du code marchand
        /// </summary>
        /// <param name="merchandCode">Code marchand</param>
        public void SetMerchandCode(string merchandCode)
        {
            this.MerchandCode = merchandCode;
        }
        public void SetAmount(decimal amount)
        {
            this.Amount = amount;
        }
        public void SetShopperIPAddress(string shopperIPAddress)
        {
            this.ShopperIPAddress = shopperIPAddress;
        }
        public void SetSessionId(string sessionId)
        {
            this.SessionId = sessionId;
        }
        public void SetOrderCode(string orderCode)
        {
            this.OrderCode = orderCode;
        }
        public string GetOrderCode()
        {
            return this.OrderCode;
        }
        public void SetDescription(string description)
        {
            this.Description = description;
        }
 
        public string GetDescription()
        {
            return this.Description;
        }
        public string GetAmount()
        {
            return this.Amount.ToString();
        }
        public string GetCurrencyCode()
        {
            return this.CurrencyCode;
        }
        public string GetExponent()
        {
            return this.Exponent.ToString();
        }
        public string GetCardNumber()
        {
            return this.CardNumber;
        }
        public string GetExpirationDateMonth()
        {
            return this.ExpirationDateMonth;
        }
        public string GetExpirationDateYear()
        {
            return this.ExpirationDateYear;
        }
        public string GetCardHolderName()
        {
            return this.CardHolderName;
        }
        public string GetShopperIPAddress()
        {
            return this.ShopperIPAddress;
        }
        public string GetSessionId()
        {
            return this.SessionId;
        }
        public string GetPaymentMethod()
        {
            return this.PaymentMethod;
        }
        public void SetPaymentMethod(string value)
        {
            this.PaymentMethod = value;
        }
        /// <summary>
        /// Retour de la réponse structurée en XML
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetXML()
        {
            // Ok, maintenant on va construire la requête XML
            return Const.XmlHeader
            + DocType
            + String.Format(Xml_PaymentService_Open, GetMerchandCode())
             + String.Format(Xml_PaymentService_Submit, GetOrderCode(), Util.XMLEscape(GetDescription()), GetAmount(), GetCurrencyCode(),
                    GetExponent(), GetPaymentMethod(), GetCardNumber(), GetExpirationDateMonth(), GetExpirationDateYear(), Util.XMLEscape(GetCardHolderName()),
                    GetShopperIPAddress(), GetSessionId())
            + Xml_PaymentService_Close;
        }

    }
}
