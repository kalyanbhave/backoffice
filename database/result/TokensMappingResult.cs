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
    ///  de recherche d'un token dans la table de mapping des Tokens
    ///  Date : 26 mars 2010
    ///  Auteur : Samatar
    /// </summary>
    public class TokensMappingResult
    {
        // Token BackOffice
        private long BOToken;
        // Token FrontOffice
        private string FOToken;
        // Date d'expiration de la carte
        private DateTime ExpirationDate;


        public TokensMappingResult()
        {
            // Initialisation des valeurs
            this.BOToken = -1;
        }

        public void SetBOToken(long token)
        {
            this.BOToken = token; 
        }

        public long GetBOToken()
        {
            return this.BOToken;
        }
        public void SetFOToken(string token)
        {
            this.FOToken = token;
        }

        public string GetFOToken()
        {
            return this.FOToken;
        }
        public void SetExpirationDate(DateTime expirationDate)
        {
            this.ExpirationDate = expirationDate;
        }

        public DateTime GetExpirationDate()
        {
            return this.ExpirationDate;
        }
    }
}