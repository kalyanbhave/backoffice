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


namespace SafeNetWS.creditcard.creditcardvalidator
{
    /// <summary>
    /// Classe permettant de gérer le retour de la validation
    /// des cartes BIBIT depuis le service RBS
    /// </summary>
    public class ProviderVerifierResult
    {
        // Statut de la carte
        private bool valid;
        // Message Warning
        // dans le cas de timeout
        // ou d'erreur au niveau du service RBS
        private string InformationCode;
        private string InformationMessage;

        // Résponse complète en XML
        private string CompleteResponse;

        //Numéro de la transaction
        private string OrderCode;

        public ProviderVerifierResult()
        {
            SetSuccess(true);
        }
        /// <summary>
        /// Retourne la réponse
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetCompleteResponse()
        {
            return this.CompleteResponse;
        }

        /// <summary>
        /// Retourne TRUE si la carte
        /// a été validée par le service BIBIT
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsSuccess()
        {
            return this.valid;
        }

        /// <summary>
        /// Affectation du statut de la validation
        /// de la carte
        /// </summary>
        /// <param name="value">TRUE ou FALSE</param>
        public void SetSuccess(bool value)
        {
            this.valid = value;
        }

        /// <summary>
        /// Affectation de la réponse complète
        /// du service RBS
        /// </summary>
        /// <param name="value">Réponse complète (XML)</param>
        public void SetCompleteResponse(string value)
        {
            this.CompleteResponse = value;
        }

        /// <summary>
        /// Affectation du code statut
        /// </summary>
        /// <param name="value">Code statut</param>
        public void SetInformationCode(string value)
        {
            this.InformationCode = value;
        }

        /// <summary>
        /// Retourne le code statut
        /// </summary>
        /// <returns>Code statut</returns>
        public string GetInformationCode()
        {
            return this.InformationCode;
        }

        /// <summary>
        /// Affectation du message complet
        /// </summary>
        /// <param name="value">Message complet</param>
        public void SetInformationMessage(string value)
        {
            this.InformationMessage = value;
        }

        /// <summary>
        /// Retourne le message d'information complet
        /// </summary>
        /// <returns>Message d'information complet</returns>
        public string GetInformationMessage()
        {
            return this.InformationMessage;
        }

        /// <summary>
        /// Retourne le numéro de la transaction
        /// </summary>
        /// <returns>Numéro de la transaction</returns>
        public string GetOrderCode()
        {
            return this.OrderCode;
        }

        /// <summary>
        /// Affectation du numéro de la
        /// transaction envoyée à RBS
        /// </summary>
        /// <param name="value">Numéro de transaction</param>
        public void SetOrderCode(string value)
        {
            this.OrderCode = value;
        }
    }
}
