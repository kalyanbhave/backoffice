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
using System.Web;
using System.Web.Caching;
using Ingrian.Security.Cryptography;
using SafeNetWS.log;

namespace SafeNetWS.utils.cache
{
    public class NetCache
    {
        // On va ouvrir une connexion que lors du démarrage
        // du service (première fois qu'une encryption/décryptage est demandée)
        // Une session pour BO
        private const string BOSessionNameKey = "BOSession";
        // et une autre sur FO
        private const string FOSessionNameKey = "FOSession";

        // Fonction d'appel lorsqu'une entrée est supprimée du cache
        private static CacheItemRemovedCallback onRemove;

        private const int MetricNotStored = 0;

        public NetCache()
        {
            if (onRemove == null)
            {
                onRemove = new CacheItemRemovedCallback(this.RemovedCallback);
            }
        }

        /// <summary>
        /// Mise en cache d'une session
        /// </summary>
        /// <param name="keyName">Nom de la clé</param>
        /// <param name="session">Session</param>
        private static void SetSession(string keyName, NAESession session)
        {
            HttpRuntime.Cache.Insert(keyName, session, null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                System.Web.Caching.Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                onRemove);
        }

        /// <summary>
        /// Recuperation d'une session en cache
        /// </summary>
        /// <param name="keyName">Nom de la clé</param>
        public static NAESession GetSession(string keyName)
        {
            return (NAESession)HttpRuntime.Cache.Get(keyName);
        }

        /// <summary>
        /// Mise en cache d'une indicateur
        /// </summary>
        /// <param name="keyName">Nom de la clé</param>
        /// <param name="value">Valeur</param>
        private static void SetMetric(string keyName, int value)
        {
            HttpRuntime.Cache.Insert(keyName, value, null,
             System.Web.Caching.Cache.NoAbsoluteExpiration,
             System.Web.Caching.Cache.NoSlidingExpiration,
             CacheItemPriority.High,
             onRemove);
        }

        /// <summary>
        /// Récupération de la valeur d'un indicateur
        /// </summary>
        /// <param name="keyName">Nom de ka cké</param>
        /// <returns>Valeur de l'indicateur</returns>
        private static int GetMetric(string keyName)
        {
            return (int)Util.Nvl(HttpRuntime.Cache.Get(keyName), MetricNotStored);
        }

        /// <summary>
        /// Mise en cache de la session NAESession BO
        /// </summary>
        /// <param name="session">Session NAE</param>
        public static void SetBOSession(NAESession session)
        {
            SetSession( BOSessionNameKey, session);
        }

        /// <summary>
        /// Retourne la session NAE BO
        /// </summary>
        /// <returns>Session NAE</returns>
        public static NAESession GetBOSession()
        {
            return GetSession(BOSessionNameKey);
        }

        /// <summary>
        /// Mise en cache de la session NAESession FO
        /// </summary>
        /// <param name="session">Session NAE</param>
        public static void SetFOSession(NAESession session)
        {
            SetSession(FOSessionNameKey, session);
        }


        /// <summary>
        /// Retourne la session NAE FO
        /// </summary>
        /// <returns>Session NAE</returns>
        public static NAESession GetFOSession()
        {
            return GetSession(FOSessionNameKey);
        }


        /// <summary>
        /// Vidage du cache
        /// On retire toutes les variables
        /// mises en cache par le service
        /// </summary>
        public static void ClearCache()
        {
            ClearSafetNetSessionCache();
        }

        /// <summary>
        /// Vidage du cache
        /// On retire toutes les variables
        /// mises en cache par le service
        /// pour les sessionbs SafeNet
        /// </summary>
        public static void ClearSafetNetSessionCache()
        {
            HttpRuntime.Cache.Remove(FOSessionNameKey);
            HttpRuntime.Cache.Remove(BOSessionNameKey);
        }
      
        /// <summary>
        /// Fonction permettant de fermer une session
        /// qui a été retirée du cache
        /// </summary>
        /// <param name="k">Nom de clé</param>
        /// <param name="v">Session</param>
        /// <param name="r">Raison du retrait</param>
        private void RemovedCallback ( string k, object v, CacheItemRemovedReason r )
        {
            if (!String.IsNullOrEmpty(k))
            {
                switch (k)
                {
                    case BOSessionNameKey:
                    case FOSessionNameKey:
                        NAESession session = (NAESession)v;
                        if (session != null)
                        {
                            // Une session a été extraite du cache
                            // Nous allons la fermer
                            try
                            {
                                session.Dispose();
                            }
                            catch (Exception)
                            {
                                // Ignorer cette exception
                            };
                        }
                        break;
                    default:
                        break;
                }
              
            }
        }

    }
}
