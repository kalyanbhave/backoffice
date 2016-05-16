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
using System.Collections.Generic;
using SafeNetWS.database.row.value;

namespace SafeNetWS.database.row
{

    /// <summary>
    /// Cette classe permet de stoquer les enregistrements
    /// correspondant aux cryptogrammes cartes (plus CSC) Egencia
    /// après extraction de la table des données encryptées
    /// Date : 22 mai 2012
    /// Auteur : Samatar
    /// </summary>
    public class EncryptedEgenciaData
    {
        // Hash qui contient les enregistrements
        // (token, cryptogramme pan, cryptogramme csc)
        private Dictionary<string, EncryptedEgenciaValue> Tokens;


        public EncryptedEgenciaData()
        {
            this.Tokens = new Dictionary<string, EncryptedEgenciaValue>();
        }

        /// <summary>
        /// Ajout d'une nouvelle carte
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="encryptedPAN">PAN crypté</param>
        /// <param name="encryptedCSC">CSC crypté</param>
        public void AddData(string token, string encryptedPAN, string encryptedCSC)
        {
            this.Tokens.Add(token, new EncryptedEgenciaValue(encryptedPAN, encryptedCSC));
        }

        /// <summary>
        /// Retourne le nombre d'éléménts
        /// </summary>
        /// <returns>Nombre de cartes</returns>
        public int GetSize()
        {
            return this.Tokens.Count;
        }

        /// <summary>
        /// Retourne les éléments
        /// </summary>
        /// <returns>Eléménts</returns>
        public Dictionary<string, EncryptedEgenciaValue> GetTokens()
        { 
           return this.Tokens;
        }
    }
}