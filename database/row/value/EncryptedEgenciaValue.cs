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
    /// correspondant à une carte Egencia
    /// Elle se compose :
    ///  1 - Numéro de carte crypté
    ///  2 - Numéro de CSC crypté
    ///
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class EncryptedEgenciaValue
    {
        private string EncryptedPAN;
        private string EncryptedCSC;


        public EncryptedEgenciaValue(string encryptedPAN, string encryptedCSC)
        {
            this.EncryptedPAN = encryptedPAN;
            this.EncryptedCSC = encryptedCSC;
        }

        /// <summary>
        /// Retourne le numéro de carte
        /// masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetEncryptedPAN()
        {
            return this.EncryptedPAN;
        }

        /// <summary>
        /// Retourne le CSC
        /// </summary>
        /// <returns>CSC</returns>
        public string GetEncryptedCSC()
        {
            return this.EncryptedCSC;
        }
    }
}