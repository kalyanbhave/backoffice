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
using System.Net;
using System.IO;
using System.Text;
using SafeNetWS.login;
using SafeNetWS.log;

namespace SafeNetWS.www
{
    /// <summary>
    /// Cette classe permet d'envoyer un flux à travers le web
    /// par la méthode POST
    /// </summary>
    public class HttpUtil
    {
        public const string NoTimeOutString = "-1";
        public const int NoTimeOut = -1;
        private const string HttpContentTypeText = "text/xml";
        public const string HttpContentTypeUrlEncoded = "application/x-www-form-urlencoded";
        public const string HttpMethodPost = "POST";

        /// <summary>
        /// Cette méthode permet d'effectuer un POST
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="uri">URL du service</param>
        /// <param name="login">Compte d'authentification</param>
        /// <param name="password">Mot de passe</param>
        /// <param name="data">Flux de donneés à envoyer</param>
        /// <returns>Flux de réponse (chaîne de caractères))</returns>
        public static string HttpPost(UserInfo user, string uri, string login, string password, string data)
        {
            return HttpPost(user, uri, login, password, data, NoTimeOut);
        }

        /// <summary>
        /// Cette méthode permet de construire la requete HTTP
        /// </summary>
        /// <param name="uri">URL du service</param>
        /// <param name="login">Compte d'authentification</param>
        /// <param name="password">Mot de passe</param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static HttpWebRequest BuildRequest(string uri, int timeout, string login, string password)
        {
            //  Accept an invalid SSL certificate programmatically.
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Préparation de la requête HTTP
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            // On établi une connexion persistante            
            request.KeepAlive = false;

            if (timeout != NoTimeOut)
            {
                // Pour spécifier le temps à attendre avant l'expiration d'une opération de lecture ou d'écriture
                // en millisecondes. 
                request.Timeout = timeout * 1000;
            }

            // Définition du compte utilisateur
            request.Credentials = new NetworkCredential(login, password);
            request.PreAuthenticate = true;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = HttpMethodPost;
            // Le type du contenu doit être txt/xml
            request.ContentType = HttpContentTypeText;

            return request;
        }

        /// <summary>
        /// Cette méthode permet d'effectuer un POST
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="uri">URL du service</param>
        /// <param name="login">Compte d'authentification</param>
        /// <param name="password">Mot de passe</param>
        /// <param name="data">Flux de donneés à envoyer</param>
        /// <param name="timeout">Time out (s)</param>
        /// <returns>Flux de réponse (chaîne de caractères))</returns>
        public static string HttpPost(UserInfo user, string uri, string login, string password, string data, int timeout)
        {
            string retval = null;

            // Préparation de la requête HTTP
            HttpWebRequest request = BuildRequest(uri, timeout, login, password);
       
            // On va envoyer les données en octets
            byte[] postBytes = Encoding.ASCII.GetBytes(data);
            // On compte les octets à envoyer
            request.ContentLength = postBytes.Length;

            Stream requestStream = null;
            try
            {
                // On va maintenant envoyer les données
                requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Dispose();
                }
            }

            // Visiblement les données ont été envoyées
            // nous allons maintenant lire la réponse

            HttpWebResponse response = null;
            StreamReader reader = null;
            Stream dataStream = null;
            try
            {
                // Les données ont été envoyés
                // allons lire la réponse
                response = (HttpWebResponse)request.GetResponse();
                if (response == null)
                {
                    throw new Exception(user.GetMessages().GetString("HTTP.Error.NoResponse", true));
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(user.GetMessages().GetString("HTTP.Error.ResponseStatusKO", response.StatusCode.ToString(), true));
                }
                // Get the request stream.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                reader = new StreamReader(dataStream);
                // Read the content.
                retval = reader.ReadToEnd();
            }
            finally
            {
                // Clean up the streams.
                if (response != null)
                {
                    response.Close();
                }
            }

            return retval;
        }

    }
}

