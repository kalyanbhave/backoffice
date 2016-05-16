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
using System.Collections;
using SafeNetWS.database.row.value;

namespace SafeNetWS.database.row
{

    /// <summary>
    /// Cette classe permet de stoquer les enregistrements
    /// correspondant aux cryptogrammes cartes
    /// après extraction de la table des données encryptées
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class FORemainingEncryptedData
    {
        // Hash qui contient les enregistrements
        // (token, cryptogramme + date d'expiration)
        public Hashtable Tokens;


        public FORemainingEncryptedData()
        {
            // Initialisation
            this.Tokens = new Hashtable();
        }

        /// <summary>
        /// Ajout de l'enregistrement
        /// dans le Hash
        /// </summary>
        /// <param name="token">TokenFront</param>
        /// <param name="encryptedPAN">Cryptogramme carte</param>
        /// <param name="expirationDate">Date d'expiration</param>
        public void AddData(string token, string encryptedPAN, DateTime expirationDate)
        {
            this.Tokens.Add(token, new FORemainingEncryptedValue(encryptedPAN,expirationDate));
        }

        /// <summary>
        /// Retourne le nombre d'éléments
        /// </summary>
        /// <returns>Nombre d'éléments</returns>
        public int GetSize()
        {
            return this.Tokens.Count;
        }
        
        /// <summary>
        /// Retourne tous les
        /// enregistrements
        /// </summary>
        /// <returns>Hash</returns>
        public Hashtable GetTokens()
        {
           return this.Tokens;
        }
    }
}