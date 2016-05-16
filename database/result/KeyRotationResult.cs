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
    /// Cette classe permet de construire la réponse apportée
    /// par la méthode de mise à jour des cryptogrammes des cartes
    /// après rotation de clé
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class KeyRotationResult
    {
        private int Count;
        private int SuccessCount;
        private int RemainingFOCards;
        private int ErrorCount;
        // cartes Egencia
        private int EgenciaCardsCount;
        private int EgenciaCardsSuccessCount;
        private int EgenciaCardsErrorCount;

        private int ClearedBOBibitCacheEntries;
        private int ClearedFOBibitCacheEntries;

        public KeyRotationResult()
        {
            // Initialisation
        }

        public void SetValues(int count, int successCount, int errorCount, int remainingFOCards,
            int clearedBOBibitCacheEntries, int clearedFOBibitCacheEntries)
        {
            this.Count = count;
            this.SuccessCount = successCount;
            this.RemainingFOCards = remainingFOCards;
            this.ErrorCount = errorCount;
            this.ClearedBOBibitCacheEntries = clearedBOBibitCacheEntries;
            this.ClearedFOBibitCacheEntries = clearedFOBibitCacheEntries;
        }
        public void SetEgenciaCardsValues(int egenciaCardsCount, int egenciaCardsSuccessCount, int egenciaCardsErrorCount)
        {
            this.EgenciaCardsCount = egenciaCardsCount;
            this.EgenciaCardsSuccessCount = egenciaCardsSuccessCount;
            this.EgenciaCardsErrorCount = egenciaCardsErrorCount;
        }

       
        public void SetValues(int count, int successCount, int errorCount)
        {
            this.Count = count;
            this.SuccessCount = successCount;
            this.ErrorCount = errorCount;
        }

        public int GetCount()
        {
            return this.Count;
        }
        public int GetSuccessCount()
        {
            return this.SuccessCount;
        }
        public int GetRemainingFOCards()
        {
            return this.RemainingFOCards;
        }
        public int GetErrorCount()
        {
            return this.ErrorCount;
        }
        public int GetEgenciaCardsCount()
        {
            return this.EgenciaCardsCount;
        }
        public int GetEgenciaCardsSuccessCount()
        {
            return this.EgenciaCardsSuccessCount;
        }
        public int GetEgenciaCardsErrorCount()
        {
            return this.EgenciaCardsErrorCount;
        }
        public int GetClearedBOBibitCacheEntries()
        {
            return this.ClearedBOBibitCacheEntries;
        }

        public int GetClearedFOBibitCacheEntries()
        {
            return this.ClearedFOBibitCacheEntries;
        }
    }
}