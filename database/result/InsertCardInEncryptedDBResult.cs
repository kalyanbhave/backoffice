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
    ///  d'insertion des cartes dans la base des données encryptées
    ///  Date : 22 septembre 2009
    ///  Auteur : Samatar
    /// </summary>
    public class InsertCardInEncryptedDBResult
    {
        private long Token;
        private string TruncatedPan;
        private string CardType;
        private string ShortCardType;


        public InsertCardInEncryptedDBResult()
        {
            // Initialisation
            this.Token = -1;
        }

        /// <summary>
        /// Affectation du token
        /// </summary>
        /// <param name="token">Token</param>
        public void SetToken(long token)
        {
            this.Token = token; 
        }

        /// <summary>
        /// Affectation du type de carte
        /// </summary>
        /// <param name="cardType">Type de carte</param>
        public void SetCardType(string cardType)
        {
            this.CardType = cardType;
        }

        /// <summary>
        /// Affectation du type de carte court
        /// </summary>
        /// <param name="shortCardType">Type de carte court</param>
        public void SetShortCardType(string shortCardType)
        {
            this.ShortCardType = shortCardType;
        }

        /// <summary>
        /// Affectation du numéro de carte masqué
        /// </summary>
        /// <param name="truncatedPan">Numéro de carte masqué</param>
        public void SetTruncatedPan(string truncatedPan)
        {
            this.TruncatedPan = truncatedPan;
        }

        /// <summary>
        /// Retourne le token
        /// </summary>
        /// <returns>Token</returns>
        public long GetToken()
        {
            return this.Token;
        }

        /// <summary>
        /// Retourne le numéro de carte masqué
        /// </summary>
        /// <returns>Numéro de carte masqué</returns>
        public string GetTruncatedPan()
        {
            return this.TruncatedPan;
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
        /// Retourne le type de carte court
        /// </summary>
        /// <returns>Type de carte court</returns>
        public string GetShortCardType()
        {
            return this.ShortCardType;
        }
    }
}