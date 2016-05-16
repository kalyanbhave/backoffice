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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using Ingrian.Security.Cryptography;

using SafeNetWS.login;
using SafeNetWS.messages;
using SafeNetWS.log;
using SafeNetWS.utils.cache;
using SafeNetWS.business;

namespace SafeNetWS.utils.crypting.safenet
{
 
    /// <summary>
    /// Cette classe définie une session Ingrian (NAESession)
    /// permettant d'encrypter ou decrypter des données
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class SafeNetSession
    {

        // Types d'appel
        // BO ou FO
        public const int CallerTypeBO = 0;
        public const int CallerTypeFO = 1;
        // Type de client (BO ou FO)
        private int callerType;

        // Compte de connexion à Ingrian
        private string username;
        // Mot de passe validant le compte utilisateur
        private string password;
        // Session NAE
        private NAESession session;
        // Clé permettant le cryptage/ décryptage
        private SymmetricAlgorithm key;
        // Encodage
        private UTF8Encoding encoding;
        // Compte utilisateur applicatif
        private UserInfo user;

        // Nombre de tentative d'ouverture de session
        // en cas d'erreur initialisation de la librairie
        //private int nrTryAgain;
        // Erreur d'initialisation de la librairie
        //private const string ErrorInitMessage = "Library Initialization Failed";


        /// <summary>
        /// Ouverture d'une session NASession
        /// Si une session est déjà est déjà ouverte pour ce compte
        /// utilisateur, alors aucune
        /// nouvelle session ne sera ouverte
        /// </summary>
        /// <param name="useri">Compte utilisateur (connexion service)</param>
        /// <param name="usernamei">Compte utilisateur SafeNet</param>
        /// <param name="passwordi">Mot de passe</param>
        /// <param name="callerTypei">Type d'appel (BO ou FO)</param>
        public SafeNetSession(UserInfo useri, string usernamei, string passwordi, int callerTypei)
        {
            //this.nrTryAgain = 0;
            // On met à jour les variables
            // pour cette session
            this.user = useri;
            this.encoding = new UTF8Encoding();
            this.username = usernamei;
            this.password = passwordi;
            this.callerType = callerTypei;

            // On va vérifier si la session est déjà disponible au niveau du cache
            // pour ce compte ingrian
            // on va s'en servir et ainsi éviter d'ouvrir une nouvelle
            // session Ingrian
            //RetrieveSessionFromCache();

            //if (GetSession() == null)
            //{
                // Nous n'avons pas de session ouverte pour ce compte utilisateur
                // dans le cache
                // On a besoin d'en ouvrir une nouvelle
                OpenSession();
            //}
            // On a une session,
            // On va maintenant récupérer la clé
            // pour encrypter/décrypter
            RetrieveKey();
        }

        /// <summary>
        /// Retourne l'encodage
        /// </summary>
        /// <returns>Encodage</returns>
        private UTF8Encoding GetEncoding()
        {
            return this.encoding;
        }

        /// <summary>
        /// Ouverture d'une nouvelle session Ingrian
        /// La session sera ouverte et chargée dans le cache
        /// </summary>
        private void OpenSession()
        {
            try
            {
                // Ouverture d'une nouvelle session 
                SetSession(new NAESession(GetUsername(), GetPassword()));
               
            }
            catch (TypeInitializationException t)
            {
                throw new Exception(GetMessages().GetString("SafeNet.Error.Init", GetUsername(), t.ToString(), true));
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("SafeNet.Error.OpeningSession", GetUsername(), e.ToString(), true));
            }
        }

        /// <summary>
        /// Affectation session NAE
        /// </summary>
        /// <param name="naeSession">Session NAE</param>
        private void SetSession(NAESession naeSession)
        {
            this.session = naeSession;
        }

        /// <summary>
        /// Récupération de la clé
        /// depuis la session
        /// </summary>
        private void RetrieveKey()
        {
            try
            {
                // Récupération de la clé secrète
                // Set the IV, Padding, and Mode 
                this.key = (Rijndael) GetSession().GetKey(ConfigurationManager.AppSettings["SafeNetKeyName"]);
                // Set the IV, Padding, and Mode 
                byte[] iv = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 };
                this.key.IV = iv;
                this.key.Padding = PaddingMode.PKCS7;
                this.key.Mode = CipherMode.CBC;
            }
            catch (NAEException e)
            {
                // La clé est probablement introuvable!
                throw new Exception(GetMessages().GetString("SafeNet.CanNotFindKey", ConfigurationManager.AppSettings["SafeNetKeyName"], e.Message, true));
            }
            catch (InvalidCastException i)
            {
                // On met à jour le statut
                throw new Exception(GetMessages().GetString("SafeNet.NotAESKey", ConfigurationManager.AppSettings["SafeNetKeyName"], i.Message, true));
            }
            catch (Exception e)
            {
                // On met à jour le statut
                throw new Exception(GetMessages().GetString("SafeNet.UnexpectedError", ConfigurationManager.AppSettings["SafeNetKeyName"], e.Message, true));
            }
        }

        /// <summary>
        /// Encryptage d'un numéro de carte bancaire
        /// </summary>
        /// <param name="stringtoencrypt">numéro de carte en clair</param>
        /// <returns>numéro de carte encrypté</returns>
        public string Encrypt(string stringtoencrypt)
        {
            string retval = null;
            MemoryStream memstr=null;

            try
            {
                // On extrait la chaine à encrypter
                byte[] inputBytes = GetEncoding().GetBytes(stringtoencrypt);
                memstr = new MemoryStream();

                ICryptoTransform encryptor = null;
                CryptoStream encrstr = null;
                try
                {
                    // Create a crypto transform 
                    encryptor = GetKey().CreateEncryptor();

                    // Create a crypto stream and encrypt data 
                    encrstr = new CryptoStream(memstr, encryptor, CryptoStreamMode.Write);
                    encrstr.Write(inputBytes, 0, inputBytes.Length);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    // On ferme le flux
                    CloseCryptoStream(encrstr);
                    DisposeICryptoTransform(encryptor);
                }
        
                byte[] encrBytes = memstr.ToArray();

                // retour du resultat encrypté
                retval = Convert.ToBase64String(encrBytes);
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("SafeNet.Error.Encrypting", e.Message, true));
            }
            finally
            {
                // On libère la mémoire
                CloseMemoryStream(memstr);
            }
            return retval;
        }



        /// <summary>
        /// Décryptage d'un numéro de carte bancaire encrypté
        /// </summary>
        /// <param name="encryptedSring">numéro de carte encrypté</param>
        /// <returns>numéro de carte en clair</returns>
        public string Decrypt(string encryptedSring)
        {
            string retval = null;
            MemoryStream memstr = null;
 
            try
            {
                byte[] encrBytes = Convert.FromBase64String(encryptedSring);
                memstr = new MemoryStream();

                ICryptoTransform decryptor = null;
                CryptoStream decrstr = null;

                try
                {
                    // Create a crypto transform 
                    decryptor = GetKey().CreateDecryptor();
                    
                    // Create a crypto stream and decrypt data 
                    decrstr = new CryptoStream(memstr, decryptor, CryptoStreamMode.Write);
                    decrstr.Write(encrBytes, 0, encrBytes.Length);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    // On ferme le flux
                    CloseCryptoStream(decrstr);
                    DisposeICryptoTransform(decryptor);
                }

                byte[] decrBytes = memstr.ToArray();

                // retour du resultat décrypté
                retval = new String(GetEncoding().GetChars(decrBytes));

            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("SafeNet.Error.Decrypting", e.Message, true));
            }
            finally
            {
                // On libère la mémoire
                CloseMemoryStream(memstr);
            }
            return retval;
        }


     
        /// <summary>
        /// Fermeture de la session
        /// <remarks>ATTENTION, il ne faut pas fermer
        /// la connexion si on réutilise la session
        /// </remarks>
        /// </summary>
        public void CloseSession()
        {
            // On ne ferme de connexion car une session 
            // est ouverte par compte utilisateur et est
            // maintenue ouverte dans le cache

            if (GetSession() != null)
            {
                try
                {
                    // On ferme la session
                   this.session.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(GetMessages().GetString("SafeNet.Error.ClosingSession", e.Message, true));
                }
            }
        
        }

        /// <summary>
        /// Fermeture d'un flux mémoire
        /// </summary>
        /// <param name="mem">Flux mémoire</param>
        private void CloseMemoryStream(MemoryStream mem)
        {
            if (mem != null)
            {
                mem.Dispose();
            }
        }
        
            
        /// <summary>
        /// Fermeture d'un CryptoStream
        /// </summary>
        /// <param name="mem">CryptoStream</param>
        private void CloseCryptoStream(CryptoStream crypt)
        {
            if (crypt != null)
            {
                crypt.Dispose();
            }
        }
        
        /// <summary>
        /// Libération de la mémoire d'un ICryptoTransform
        /// </summary>
        /// <param name="mem">CryptoStream</param>
        private void DisposeICryptoTransform(ICryptoTransform icrypt)
        {
            if (icrypt != null)
            {
                icrypt.Dispose();
            }
        }

        /// <summary>
        /// Renvoi le compte utilisateur SafeNet
        /// </summary>
        /// <returns>Compte utilisateur SafeNet</returns>
        public string GetUsername()
        {
            return this.username;
        }

        /// <summary>
        /// Renvoi le mot de passe utilisé
        /// avec la compte SafeNet
        /// </summary>
        /// <returns>Mot de passe</returns>
        private string GetPassword()
        {
            return this.password;
        }

        /// <summary>
        /// Renvoi la clé symmétrique
        /// </summary>
        /// <returns>SymmetricAlgorithm</returns>
        private SymmetricAlgorithm GetKey()
        {
            return this.key;
        }


        /// <summary>
        /// Renvoi la session e cours
        /// </summary>
        /// <returns>NAESession</returns>
        private NAESession GetSession()
        {
            return this.session;
        }

        /// <summary>
        /// Retourne le compte utilisateur d'appel
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        public UserInfo GetUser()
        {
            return this.user;
        }


        /// <summary>
        /// Retourne les messages associés
        /// pour une locale
        /// </summary>
        /// <returns>Messages</returns>
        public Messages GetMessages()
        {
            return GetUser().GetMessages();
        }
    }

}