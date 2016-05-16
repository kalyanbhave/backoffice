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
using System.Data;
using System.Configuration;
using SafeNetWS.utils;

namespace SafeNetWS.creditcard.creditcardvalidator
{
    public class CachedValidationResult
    {
        // Différents statut
        // L'entrée introuvable dans le cache
        // L'entrée a été trouvée dans le cache (Valide)
        // L'entrée a été trouvée dans le cache mais elle a expirée
        public enum CacheStatus { NotFound, FoundValid, FoundExpired };

        private CacheStatus Status;
        private DateTime LastAccessTime;

        public CachedValidationResult()
        {
            SetStatus(CacheStatus.NotFound);
            SetLastAccessTime(Const.EmptyDate);
        }


        /// <summary>
        /// Retourne le statut
        /// CacheStatus.NotFound si l'entrée est introuvable
        /// CacheStatus.FoundValid si l'entrée dans le cache
        /// CacheStatus.FoundExpired si l'entrée est dans le cache mais a expirée
        /// </summary>
        /// <returns>Statut</returns>
        public CacheStatus GetStatus()
        {
            return this.Status;
        }

        /// <summary>
        /// Affectation de la présence ou non
        /// d'une entrée dans le cache
        /// </summary>
        /// <param name="status">Statut</param>
        public void SetStatus(CacheStatus status)
        {
            this.Status = status;
        }

        /// <summary>
        /// Retourne la date de dernière écriture
        /// dans le cache pour une entrée
        /// </summary>
        /// <returns>Date</returns>
        public DateTime GetLastAccessTime()
        {
            return this.LastAccessTime;
        }


        /// <summary>
        /// Affectation de la date d'accès de l'entrée
        /// </summary>
        /// <param name="date">Date</param>
        public void SetLastAccessTime(DateTime date)
        {
            this.LastAccessTime = date;
        }


    }
}
