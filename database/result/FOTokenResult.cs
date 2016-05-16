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
    /// Cette classe permet de définir une strcuture de la table
    /// des tokens hébergés par le FrontOffice
    /// Cette structure se définie :
    /// - Un token (chaîne de caractères)
    /// - Date d'expiration
    ///
    ///  Date : 26 mars 2010
    ///  Auteur : Samatar
    /// </summary>
    public class FOTokenResult
    {
        // Token FrontOffice
        private string Token;
        // Date d'expiration de la carte
        private DateTime ExpirationDate;

        public FOTokenResult()
        {
            // Initialisation des valeurs
            this.Token = null;
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="expirationDate">Date d'expiration</param>
        public void SetValues(string token, DateTime expirationDate)
        {
            SetToken(token);
            SetExpirationDate(expirationDate);
        }
        /// <summary>
        /// Affectation du token
        /// </summary>
        /// <param name="token">Token</param>
        public void SetToken(string token)
        {
            this.Token = token;
        }

        /// <summary>
        /// Retourne le token
        /// </summary>
        /// <returns>Token</returns>
        public string GetToken()
        {
            return this.Token;
        }
        /// <summary>
        /// Affectation de la date d'expiration
        /// </summary>
        /// <param name="expirationDate">Date d'expiration</param>
        public void SetExpirationDate(DateTime expirationDate)
        {
            this.ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public DateTime GetExpirationDate()
        {
            return this.ExpirationDate;
        }

        /// <summary>
        /// Retourne TRUE si le token
        /// a été defini
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool isFound()
        {
            return (GetToken() != null);
        }
    }
}