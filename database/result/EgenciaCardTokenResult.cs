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
    /// des tokens Egencia hébergés par le FrontOffice
    /// Cette strunture se défini :
    /// - Un token (chaîne de caractères)
    ///
    ///  Date : 26 mars 2010
    ///  Auteur : Samatar
    /// </summary>
    public class EgenciaCardTokenResult
    {
        // Token Egencia card
        private string Token;


        public EgenciaCardTokenResult()
        {
            // Initialisation des valeurs
            this.Token = null;
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