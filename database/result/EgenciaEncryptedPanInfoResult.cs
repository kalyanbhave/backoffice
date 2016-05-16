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
    public class EgenciaEncryptedPanInfoResult
    {
        private string EncryptedPAN;
        private string EncryptedCSC;

        public EgenciaEncryptedPanInfoResult()
        {
            // Initialisation
        }

        /// <summary>
        /// Affectation des valeurs
        /// </summary>
        /// <param name="encryptedPAN">Numéro de carte masqué</param>
        /// <param name="encryptedCSC">Expiration date</param>
        public void SetValues(string encryptedPAN, string encryptedCSC)
        {
            this.EncryptedPAN = encryptedPAN;
            this.EncryptedCSC = encryptedCSC;
        }

        /// <summary>
        /// Retourne la date d'expiration
        /// </summary>
        /// <returns>Date expiration</returns>
        public string GetEncryptedCSC()
        {
            return this.EncryptedCSC;
        }

        /// <summary>
        /// Retourne le numéro de carte masqué 
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetEncryptedPAN()
        {
            return this.EncryptedPAN;
        }

        /// <summary>
        /// Indicateur le token existe
        /// </summary>
        /// <returns>True ou False</returns>
        public bool IsTokenExists()
        {
            return (GetEncryptedPAN() != null);
        }
    }
}