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
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections;
using System.DirectoryServices;
using SafeNetWS.messages;
using SafeNetWS.utils.crypting;
using SafeNetWS.utils;
using SafeNetWS.utils.cache;
using Microsoft.Security.Application;


/// <summary>
/// Description résumée de LdapAuthentication.
/// Fournir des méthodes d'authentification via LDAP
/// </summary>
/// 
namespace SafeNetWS.login.ldap
{
    public class LdapAuthentication : IDisposable
    {
        private DirectoryEntry de;
        private DirectorySearcher deSearch;
        private string displayname;
        private Messages messages;

        private bool anonymous;

        /// <summary>
        /// Connexion à l'active directory
        /// pour vérifier les comptes utilisateurs
        /// qui souhaitent utiliser l'application
        /// </summary>
        /// <param name="messagesi">Message langue</param>
        public LdapAuthentication(Messages messagesi)
        {
            SetMessages(messagesi);
            this.anonymous = false;
            this.de = null;
        }

        /// <summary>
        /// Connexion à l'AD
        /// </summary>
        /// <param name="userName">Compte utilisateur</param>
        /// <param name="password">Mot de passe</param>
        private void Connect(string userName, string password)
        {
            if (IsAnonymous())
            {
                // On recherche la valeur dans le cache
                this.de = Global.GetLDAPDirectoryEntry();
            }
            if (GetDirectoryEntry()==null)
            {
                // On va utiliser un compte pour se connecter à l'AD   
                this.de = new DirectoryEntry(ConfigurationManager.AppSettings["LDAPPath"], userName, password);

                if (GetDirectoryEntry().NativeObject.Equals(null))
                {
                    throw new COMException("Unknown user or wrong password!");
                }
            }
            if (IsAnonymous())
            {
                // On va stoquer la variable dans le cache
                Global.SetLDAPDirectoryEntry(GetDirectoryEntry());
            }
        }
        
        /// <summary>
        /// Connexion à l'AD
        /// avec le compte générique
        /// </summary>
        private void Connect()
        {
            SetAnonymous(true);
            Connect(ConfigurationManager.AppSettings["LDAPUserName"],
                EncDec.DecryptPassword(ConfigurationManager.AppSettings["LDAPPassword"]));
        }


        /// <summary>
        /// Retourne l'entrée LDAP courante
        /// </summary>
        /// <returns>DirectoryEntry</returns>
        private DirectoryEntry GetDirectoryEntry()
        {
            return this.de;
        }


        /// <summary>
        /// Check method for AD connectivity
        /// Generic username will be send
        /// </summary>
        public void Test()
        {
             // On va passer par la connexion avec le compte
             // générique afin de faire des recherches (groupes)
             Connect();
        }
        

        /// <summary>
        /// Vérification si un utilisateur existe dans l'AD
        /// </summary>
        /// <param name="userName">Compte utilisateur à vérifier</param>
        /// <param name="password">Mot de passe</param>
        /// <returns></returns>
        public bool UserExists(string userName, string password)
        {
            try
            {
                if (password != null)
                {
                    // On va se connecter avec le compte utilisateur
                    // fournie par le client
                    Connect(userName, password);
                }
                else
                {
                    // On va passer par la connexion avec le compte
                    // générique afin de faire des recherches (groupes)
                    Connect();
                }

                // Ce compte existe et le mot de passe est bon
                // On va rechercher le compte utilisateur
                // Il n'y a pas de vérification de mot de passe
                this.deSearch = new DirectorySearcher();
                this.deSearch.SearchRoot = GetDirectoryEntry();

                // On modifie le filtre pour ne chercher que le user
                // LdapFilterEncode encodes input according to RFC4515 where unsafe values are converted to \XX where XX is the representation of the unsafe character.
                // Set filter
                this.deSearch.Filter = String.Format("(SAMAccountName={0})", Encoder.LdapFilterEncode(userName));
                this.deSearch.PropertiesToLoad.Add("displayName");

                // Pas de boucle foreach car on ne cherche qu'un user
                SearchResult result = this.deSearch.FindOne();
                if (result != null)
                {
                    // On récupère le nom
                    SetDisplayName(result.Properties["displayname"].ToString());

                    return true;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return false;
        }
        /// <summary>
        /// Affectation du nom du
        /// compte utilisateur
        /// </summary>
        /// <param name="name">Nom</param>
        private void SetDisplayName(string name)
        {
            this.displayname = name;
        }


        /// <summary>
        /// Récupération des groupes
        /// auquelles appartient un user
        /// </summary>
        /// <returns>Hash contenant la liste des groupes</returns>
        public Hashtable GetGroups()
        {
            this.deSearch.PropertiesToLoad.Add("memberOf");
            // les noms de groupe vont être stoqués
            // dans un Hash
            Hashtable groupNames = new Hashtable();
            try
            {
                SearchResult result = this.deSearch.FindOne();
                int propertyCount = result.Properties["memberOf"].Count;

                String dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (String)result.Properties["memberOf"][propertyCounter];
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }
                    // On a un groupe 
                    // On l'ajouter au Hash
                    groupNames.Add(propertyCounter, dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(messages.GetString("LdapAuthentication.GetGroups.Error", ex.Message, true));
            }

            return groupNames;
        }


        /// <summary>
        /// Fermeture de la connexion
        /// et libération des ressources
        /// Date : 22 septembre 2009
        /// Auteur : Samatar
        /// </summary>
        public void Disconnect()
        {
            if (!IsAnonymous())
            {
                try
                {
                      Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(messages.GetString("LdapAuthentication.Disconnect", e.Message, true));
                }
            }
        }


        /// <summary>
        /// Récupération du nom de l'utilisateur
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName()
        {
            return this.displayname;
        }

        /// <summary>
        /// Affectation des messages
        /// pour la traduction
        /// </summary>
        /// <param name="messagesin">Messages</param>
        private void SetMessages(Messages messagesin)
        {
            this.messages = messagesin;
        }

        /// <summary>
        /// Retourne TRUE si le compte
        /// utilisé pour l'authentification
        /// est un compte générique
        /// </summary>
        /// <returns>True ou False</returns>
        private bool IsAnonymous()
        {
            return this.anonymous;
        }

        /// <summary>
        /// Affectation de l'utilisation
        /// d'un compte anonymous
        /// </summary>
        /// <param name="value">True ou False</param>
        private void SetAnonymous(bool value)
        {
            this.anonymous = value;
        }

         #region IDisposable implementation
 
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
 
    private bool m_Disposed = false;
 
    protected virtual void Dispose(bool disposing)
    {
        if (!m_Disposed)
        {
            if (disposing)
            {
                de.Dispose();
                deSearch.Dispose();
            }
 
            // Unmanaged resources are released here.
            m_Disposed = true;
        }
    }

    ~LdapAuthentication()    
    {        
        Dispose(false);
    }
 
    #endregion

    }
}
