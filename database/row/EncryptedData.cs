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

namespace SafeNetWS.database.row
{

    /// <summary>
    /// Cette classe permet de stoquer les enregistrements
    /// correspondant aux cryptogrammes cartes
    /// après extraction de la table des données encryptées
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class EncryptedData
    {
        // Hash qui contient les enregistrements
        // (token, cryptogramme)
        private Hashtable Tokens;


        public EncryptedData()
        {
            this.Tokens = new Hashtable();
        }

        public void AddData(long token, string encryptedData)
        {
            this.Tokens.Add(token, encryptedData);
        }

        public int GetSize()
        {
            return this.Tokens.Count;
        }
        public Hashtable GetTokens()
       {
           return this.Tokens;
       }
    }
}