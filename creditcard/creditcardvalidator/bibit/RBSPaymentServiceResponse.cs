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
using System.IO;
using System.Xml;
using SafeNetWS.log;
using SafeNetWS.utils;

namespace SafeNetWS.creditcard.creditcardvalidator.bibit
{
    /// <summary>
    /// Cette classer permet de lire la réponse du service des paiements
    /// de RBS (Royal Bank of Scotland)
    /// La réponse est structurée ains :
    /// 
    /// <!DOCTYPE paymentService PUBLIC "-//Bibit//DTD Bibit PaymentService v1//EN" "http://dtd.bibit.com/paymentService_v1.dtd">
    /// <paymentService version = "1.4" merchantCode = "EXECTAUTH">
    ///     <reply>
    ///         <orderStatus orderCode = "T0211011">
    ///             <payment>
    ///                 <paymentMethod>VISA-SSL</paymentMethod>
    ///                   <amount value = "1" currencyCode = "GBP" exponent = "2" debitCreditIndicator = "credit"/>
    ///                   <lastEvent>REFUSED</lastEvent>
    ///                   <ISO8583ReturnCode code = "5" description = "REFUSED"/>
    ///             </payment>
    ///          </orderStatus>
    ///     </reply>
    /// </paymentService>
    /// 
    /// Le tag <lastEvent></lastEvent> retourne le statut de la requête (REFUSED ou AUTHORISED)
    /// C'est cette information qui nous intéresse
    /// </summary>
    public class RBSPaymentServiceResponse
    {
        private const string Xml_Response_LastEvent_TagName = "lastEvent";
        private string InputResponse;
        private bool Status;

        public RBSPaymentServiceResponse(string inputResponse)
        {
            SetInputResponse(inputResponse);
            // On va lire l'entrée
            ParseResponse();
        }
        /// <summary>
        /// Lecture de la réponse
        /// et extraction des différentes informations
        /// </summary>
        private void ParseResponse()
        {
            XmlDocument doc = new XmlDocument();
            try
            {

                // On va ignorer la validation via DTD
                doc.XmlResolver = null;
                // On charge la réponse
                doc.Load(new StringReader(GetInputResponse()));

                // On récupère le statut
                string status = string.Empty;
                try
                {
                    status = doc.GetElementsByTagName(Xml_Response_LastEvent_TagName)[0].InnerXml;
                }
                catch (Exception) 
                { 
                    status = doc.GetElementsByTagName("error")[0].InnerXml;
                }

                SetStatus(status);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Affectation du statut
        /// </summary>
        /// <param name="value">Statut</param>
        private void SetStatus(string value)
        {
            this.Status = (value.Equals(Const.StatusAuthorised));
        }

        /// <summary>
        /// Retourne TRUE si succès
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool isSuccess()
        {
            return this.Status;
        }
        /// <summary>
        /// Retourne la réponse
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetInputResponse()
        {
            return this.InputResponse;
        }
        /// <summary>
        /// Affectation réponse XML
        /// </summary>
        /// <param name="inputResponse">Réponse XML</param>
        private void SetInputResponse(string inputResponse)
        {
            this.InputResponse = inputResponse;
        }
    }
}
