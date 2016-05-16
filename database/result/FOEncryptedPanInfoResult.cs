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

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir le retour de la méthode
    /// de récupération des informations sur les cartes dans FrontOffice
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class FOEncryptedPanInfoResult
    {
        private string EncryptedPAN;
        private DateTime ExpirationDate;

        public FOEncryptedPanInfoResult()
        {
            // Initialisation
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="encryptedPAN">Numéro de carte masqué</param>
        /// <param name="expirationDate">Expiration date</param>
        public void SetValues(string encryptedPAN, DateTime expirationDate)
        {
            this.EncryptedPAN = encryptedPAN;
            this.ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date expiration</returns>
        public DateTime GetExpirationDate()
        {
            return this.ExpirationDate;
        }

        /// <summary>
        /// Retourne le numéro de carte masqué 
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetEncryptedPAN()
        {
            return this.EncryptedPAN;
        }
    }
}