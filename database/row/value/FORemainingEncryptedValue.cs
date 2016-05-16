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

namespace SafeNetWS.database.row.value
{

    /// <summary>
    /// Cette classe permet de définir  une valeur
    /// stoquée dans la base FrontOffice
    /// Elle se compose :
    ///  1 - Numéro de carte crypté
    ///  2 - Date d'expiration
    ///
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class FORemainingEncryptedValue
    {
        private string EncryptedPAN;
        private DateTime ExpirationDate;


        public FORemainingEncryptedValue(string encryptedPAN, DateTime expirationDate)
        {
            this.EncryptedPAN = encryptedPAN;
            this.ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Retourne le numéro de carte
        /// masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetEncryptedPAN()
        {
            return EncryptedPAN;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date d'expiration</returns>
        public DateTime GetExpirationDate()
        {
           return this.ExpirationDate;
        }
    }
}