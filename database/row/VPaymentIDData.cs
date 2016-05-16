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
using System.Collections.Generic;
using System.Web;

namespace SafeNetWS.database.row
{
    public class VPaymentIDData
    {
        // ID VPayment
        private string ID;
        // Code voyageur
        private string TravelerCode;
        // Nom du voyageur
        private string TravelerName;
        // Compte utilisateur qui a généré l'ID
        private string User;
        // Date de création de l'id
        private DateTime InsertDate;


        public VPaymentIDData()
        {

        }

        public string GetID()
        {
            return this.ID;
        }

        public void SetID(string value)
        {
            this.ID = value;
        }

        public string GetTravelerCode()
        {
            return this.TravelerCode;
        }

        public void SetTravelerCode(string value)
        {
            this.TravelerCode = value;
        }


        public string GetTravelerName()
        {
            return this.TravelerName;
        }

        public void SetTravelerName(string value)
        {
            this.TravelerName = value;
        }
        public DateTime GetInsertDate()
        {
            return this.InsertDate;
        }

        public void SetInsertDate(DateTime value)
        {
            this.InsertDate = value;
        }

        public string GetUser()
        {
            return this.User;
        }

        public void SetUser(string value)
        {
            this.User = value;
        }



    }
}
