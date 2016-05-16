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

namespace SafeNetWS.www
{
    /// <summary>
    /// Cette classe enregistre le nom et 
    /// l'adresse IP de l'hote distant (client)
    /// </summary>
    public class RemoteHost
    {
        private const string SslOn = "ON";
        private const string SslOff = "OFF";

        private string Addr;
        private string Host;
        private bool Https;

        public RemoteHost(string remoteAddr, string remoteHost, string https)
        {
            this.Addr = remoteAddr;
            this.Host = remoteHost;
            this.Https = https.Equals(SslOn) ? true : false;
        }

        /// <summary>
        /// Retourne l'adresse IP
        /// de l'hote distant
        /// </summary>
        /// <returns>Adresse IP de l'hote</returns>
        public string GetAddr()
        {
            return this.Addr;
        }

        /// <summary>
        /// Retourne le nom de l'hote
        /// distant (client)
        /// </summary>
        /// <returns>Nom de l'hote</returns>
        public string GetHost()
        {
            return this.Host;
        }
        /// <summary>
        /// Retourne retourne TRUE si la requête 
        /// arrive au moyen d'un canal de sécurité SSL
        /// sinon OFF.
        /// </summary>
        /// <returns>SSL</returns>
        public bool IsHttps()
        {
            return this.Https;
        }

        /// <summary>
        /// Retourne les informations sur
        /// l'hote distant
        /// </summary>
        /// <returns>Informations sur l'hote distant</returns>
        public string GetRemoteInfos()
        {
            return String.Format("Host={0} IP={1} SSL={2}", GetHost(), GetAddr(), IsHttps() ? SslOn : SslOff);
        }
    }
}
