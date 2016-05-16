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
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.IO;
using SafeNetWS.utils;

namespace SafeNetWS.business.response.reader
{
    /**
     * Cette classe permet de lire la réponse apportée
     * par la méthode qui récupère le moyen de paiement
     * d'un voyageur
     * L'entrée réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *    <Duration>Valeur de retour</Duration>
     *    <value>
     *      <PaymentType>EC</PaymentType>
     *      <Origin>Origin</Origin>
     *      <Service>AIR</Service>
     *      <Card>
     *          <CardType>Amex</CardType>
     *          <ShortCardType>AX</ShortCardType>
     *          <MII>Valeur de retour</MII>
     *          <MIIIssuerCategory>Valeur de retour</MIIIssuerCategory>
     *          <CardToken>Valeur de retour</CardToken>
     *          <TruncatedCardNumber>Valeur de retour</TruncatedCardNumber>
     *          <ExpiryDate>maDate</ExpiryDate>
     *          <ShortExpiryDate>maDate</ShortExpiryDate>
     *          <FormOfPayment>Valeur de retour</FormOfPayment>      
     *      </Card>
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
     * Date : 15/12/2011
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class TravelerPaymentMeansResponseReader
    {

        // --> PaymentType
        private const string Xml_PaymentType_Open_Tag = "<PaymentType>";
        // --> Origin
        private const string Xml_Origin_Open_Tag = "<Origin>";
        // --> Service
        private const string Xml_Service_Open_Tag = "<Service>";

        // If Payment type = CC
        // We need to take care of card informations
        // --> Service
        private const string Xml_Card_Open_Tag = "<Card>";

        // --> CardType
        private const string Xml_CardType_Open_Tag = "<CardType>";

        // --> ShortCardType
        private const string Xml_ShortCardType_Open_Tag = "<ShortCardType>";

        // Value MII to return (serialized into string)
        private const string Xml_MII_Open_Tag = "<MII>";

        // Value MII to return (serialized into string)
        private const string Xml_MIIIssuerCategory_Open_Tag = "<MIIIssuerCategory>";

        // --> CardToken
        private const string Xml_CardToken_Open_Tag = "<CardToken>";
 
        // --> TruncatedCardNumber
        private const string Xml_TruncatedCardNumber_Open_Tag = "<TruncatedCardNumber>";
        // --> Expiration Date
        private const string Xml_ExpirationDate_Open_Tag = "<ExpirationDate>";

        // Value ShortExpirationDate to return (serialized into string)
        private const string Xml_ShortExpirationDate_Open_Tag = "<ShortExpirationDate>";

        // Value FormOfPayment to return (serialized into string)
        private const string Xml_FormOfPayment_Open_Tag = "<FormOfPayment>";


        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_TagName = "Duration";


        // Exception 
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Code_TagName = "Count";
        // Exception message
        private const string Xml_Response_Exception_Message_TagName = "Message";

        // Durée du processus
        private double Duration;

        // Exceptions
        private int ExceptionCount;
        private string ExceptionMessage;


        // Values
        private string PaymentType;
        private string Origin;
        private string Service;
        // Card
        private string CardType;
        private string ShortCardType;
        private string MII;
        private string MIIIssuerCategory;
        private string CardToken;
        private string TruncatedCardNumber;
        private string ShortExpirationDate;
        private string ExpirationDate;
        private string FormOfPayment;

        private String InputResponse;

        public TravelerPaymentMeansResponseReader(string inputResponse)
        {
            this.InputResponse = inputResponse;

            // On va lire l'entrée
            ParseResponse();
        }

        /// <summary>
        /// Lecture de la réponse et extraction des
        /// différentes informations
        /// </summary>
        private void ParseResponse()
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                //The XmlResolver property is set to null. External resources are not resolved.
                doc.XmlResolver = null;
                // On charge la réponse
                doc.Load(new StringReader(InputResponse));

                try
                {
                    // On récupère en premier l'état d'exception
                    SetExceptionCount(Util.ConvertStringToInt(doc.GetElementsByTagName(Xml_Response_Exception_Code_TagName)[0].InnerXml));

                    if (IsError())
                    {
                        // Le raitement a échoué...
                        // nous allons récupérer le message d'exception
                        SetExceptionMessage(doc.GetElementsByTagName(Xml_Response_Exception_Message_TagName)[0].InnerXml);
                    }
                }
                catch (Exception)
                {
                    // Visiblement pas d'exception
                    // 
                }

                // Pas d'exception, on récupère les valeurs
                if (!IsError())
                {
                    // On récupère directement la valeur de la node <Value></Value>
                    // Il ne doit y avoir qu'une node de ce type
                    SetDuration(Util.ConvertStringToDouble(doc.GetElementsByTagName(Xml_Response_Duration_TagName)[0].InnerXml));
               
                    SetPaymentType(doc.GetElementsByTagName(Xml_PaymentType_Open_Tag)[0].InnerXml);
                    SetOrigin(doc.GetElementsByTagName(Xml_Origin_Open_Tag)[0].InnerXml);
                    SetService(doc.GetElementsByTagName(Xml_Service_Open_Tag)[0].InnerXml);

                    if (IsPaymentByCreditCard())
                    {
                        // Payment par carte
                        // on aura besoin du détail
                        SetCardType(doc.GetElementsByTagName(Xml_CardType_Open_Tag)[0].InnerXml);
                        SetShortCardType(doc.GetElementsByTagName(Xml_ShortCardType_Open_Tag)[0].InnerXml);
                        SetShortCardType(doc.GetElementsByTagName(Xml_ShortCardType_Open_Tag)[0].InnerXml);
                        SetMII(doc.GetElementsByTagName(Xml_MII_Open_Tag)[0].InnerXml);
                        SetMIIIssuerCategory(doc.GetElementsByTagName(Xml_MIIIssuerCategory_Open_Tag)[0].InnerXml);
                        SetCardToken(doc.GetElementsByTagName(Xml_CardToken_Open_Tag)[0].InnerXml);
                        SetTruncatedCardNumber(doc.GetElementsByTagName(Xml_TruncatedCardNumber_Open_Tag)[0].InnerXml);
                        SetExpirationDate(doc.GetElementsByTagName(Xml_ExpirationDate_Open_Tag)[0].InnerXml);
                        SetShortExpirationDate(doc.GetElementsByTagName(Xml_ShortExpirationDate_Open_Tag)[0].InnerXml);
                        SetFormOfPayment(doc.GetElementsByTagName(Xml_FormOfPayment_Open_Tag)[0].InnerXml);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la lecture de la réponse! Erreur :" + e.Message);
            }

        }

        /// <summary>
        /// Retourne TRUE si le moyen de paiement est en carte
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsPaymentByCreditCard()
        {
            if (String.IsNullOrEmpty(GetPaymentType())) return false;
            return GetPaymentType().Equals(Const.PaymentTypeCreditCardShort);
        }

        /// <summary>
        /// Affectation du moyen de paiement
        /// </summary>
        /// <param name="value">Type de payment</param>
        private void SetPaymentType(string value)
        {
            this.PaymentType = value;
        }
        /// <summary>
        /// Retourne le type de paiement
        /// </summary>
        /// <returns>Type de paiement</returns>
        public string GetPaymentType()
        {
            return this.PaymentType;
        }


        /// <summary>
        /// Retourne l'origine du moyen de paiement
        /// </summary>
        /// <returns>Origine du moyen de paiement</returns>
        public string GetOrigin()
        {
            return this.Origin;
        }


        /// <summary>
        /// Affectation de l'origine du moyen de paiement
        /// </summary>
        /// <param name="value">Origine du moyen de payment</param>
        private void SetOrigin(string value)
        {
            this.Origin = value;
        }

        /// <summary>
        /// Retourne le service
        /// </summary>
        /// <returns>Service</returns>
        public string GetService()
        {
            return this.Service;
        }


        /// <summary>
        /// Affectation du service
        /// </summary>
        /// <param name="value">Service</param>
        private void SetService(string value)
        {
            this.Service = value;
        }

        /// <summary>
        /// Affectation du type de carte
        /// </summary>
        /// <param name="value">Type de carte</param>
        private void SetCardType(string value)
        {
            this.CardType = value;
        }

        /// <summary>
        /// Retourne le type de carte
        /// </summary>
        /// <returns>Type de carte</returns>
        public string GetCardType()
        {
            return this.CardType;
        }

        /// <summary>
        /// Affectation du type de carte
        /// </summary>
        /// <param name="value">Type de carte</param>
        private void SetShortCardType(string value)
        {
            this.ShortCardType = value;
        }

        /// <summary>
        /// Retourne le type de carte
        /// sur deux lettres
        /// </summary>
        /// <returns>Type de carte</returns>
        public string GetShortCardType()
        {
            return this.ShortCardType;
        }

        private void SetMII(string value)
        {
            this.MII = value;
        }
        public string GetMII()
        {
            return this.MII;
        }

        private void SetMIIIssuerCategory(string value)
        {
            this.MIIIssuerCategory = value;
        }

        public string GetMIIIssuerCategory()
        {
            return this.MIIIssuerCategory;
        }

        /// <summary>
        /// Affecatation du numéro de carte masuqé
        /// </summary>
        /// <param name="value">Numéro de carte masqué</param>
        private void SetTruncatedCardNumber(string value)
        {
            this.TruncatedCardNumber = value;
        }

        /// <summary>
        /// Retourne le numéro de carte masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetTruncatedCardNumber()
        {
            return this.TruncatedCardNumber;
        }

        /// <summary>
        /// Affectation de la date d'expiration
        /// </summary>
        /// <param name="value">Date d'expiration</param>
        private void SetShortExpirationDate(string value)
        {
            this.ShortExpirationDate = value;
        }

        /// <summary>
        /// Récupération de la date d'expiration
        /// au format MM/YYYY
        /// </summary>
        /// <returns></returns>
        public string GetShortExpirationDate()
        {
            return this.ShortExpirationDate;
        }

        /// <summary>
        /// Affecatation de la date d'expiration
        /// </summary>
        /// <param name="value">Date d'expiration</param>
        private void SetExpirationDate(string value)
        {
            this.ExpirationDate = value;
        }

        /// <summary>
        /// Récupération de la date d'expiration
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public string GetExpirationDate()
        {
            return this.ExpirationDate;
        }

        /// <summary>
        /// Affectation du token
        /// </summary>
        /// <param name="value">Token</param>
        private void SetCardToken(string value)
        {
            this.CardToken = value;
        }

        /// <summary>
        /// Récupération du token
        /// </summary>
        /// <returns>Token</returns>
        public string GetCardToken()
        {
            return this.CardToken;
        }

       /// <summary>
       /// Retourne le message d'exception
       /// </summary>
       /// <returns>Exception</returns>
       public string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }

       /// <summary>
       /// Affectation du message d'exception
       /// </summary>
       /// <param name="value">Message d'exception</param>
       private void SetExceptionMessage(string value)
       {
           this.ExceptionMessage = value;
       }

       /// <summary>
       /// Indique si la demande a été correctement traitée ou non.
       /// Il faut en premier lieu lire cette méthode.
       /// Si elle retourne 
       ///  TRUE : alors la demande est en erreur et auquel cas, il faut
       ///         appeler la méthode GetExceptionMessage pour retourner l'exception lovcalisée
       /// FALSE : le traitement s'est bien déroulé        
       /// </summary>
       /// <returns>TRUE ou FALSE</returns>
       public bool IsError()
       {
           return (this.ExceptionCount > 0);
       }


       /// <summary>
       /// Affectation du nombre d'erreurs
       /// </summary>
       /// <param name="value">Nombre d'erreurs</param>
       private void SetExceptionCount(int value)
       {
           this.ExceptionCount = value;
       }

       /// <summary>
       /// Retourne la durée du traitement
       /// </summary>
       /// <returns>Durée du traitement</returns>
       public double GetDuration()
       {
           return this.Duration;
       }

       /// <summary>
       // Affecttation de la durée de traitement
       /// </summary>
       /// <param name="value">Durée</param>
       private void SetDuration(double value)
       {
           this.Duration = value;
       }

       /// <summary>
       /// Affectation de la forme de paiement
       /// </summary>
       private void SetFormOfPayment(string value)
       {
           this.FormOfPayment = value;
       }

       /// <summary>
       /// Retourne le moyen de paiement
       /// en une seule chaine
       /// </summary>
       /// <returns>Moyen de paiement</returns>
       public string GetFormOfPayment()
       {
           return this.FormOfPayment;
       }
    }
}
