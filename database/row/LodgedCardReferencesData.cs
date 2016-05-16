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
using SafeNetWS.database.row.value;
using System.Data.SqlClient;

namespace SafeNetWS.database.row
{

    /// <summary>
    /// This class allows to handle lodged card references
    /// for a specific customer
    /// </summary>
    public class LodgedCardReferencesData
    {
        // Lodged card references container
        private Hashtable references;
        // Number oj references
        private int nr;

        /// <summary>
        /// Define a new lodged card references
        /// </summary>
        public LodgedCardReferencesData()
        {
            this.references = new Hashtable();
            this.nr = 0;
        }

        /// <summary>
        /// Add a lodged card references to container
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        public void AddReference(SqlDataReader dr)
        {
            LodgedCardReferenceValue value = new LodgedCardReferenceValue(dr);
            this.references.Add(nr, value);
            nr++;
        }

        /// <summary>
        /// Returns the number of lodged card references
        /// </summary>
        /// <returns>Number of references</returns>
        public int GetSize()
        {
            return this.nr;
        }

        /// <summary>
        /// Returns the references container enumerator
        /// 
        /// </summary>
        /// <returns>references container enumerator</returns>
        public IDictionaryEnumerator GetReferences()
       {
           return this.references.GetEnumerator();
       }
    }
}