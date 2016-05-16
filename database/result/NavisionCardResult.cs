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
using System.Data.SqlClient;
using SafeNetWS.utils;

namespace SafeNetWS.database.result
{

    /// <summary>
    /// Cette classe permet de définir les informations d'une carte
    /// dans Navision
    /// Date : 22 septembre 2009
    /// Auteur : Samatar
    /// </summary>
    public class NavisionCardResult
    {
        private string CustomerNo;
        private string TravellerNo;
        private string CardReference;
        private int SharingType;
        private string CardNo1;
        private string CardNo2;
        private string CardNo3;
        private string CardNo4;
        private string CardNo5;
        private DateTime ExpirationDate;
        private string ExtendedNo;
        private int CardType;
        private int Status;
        private int TransactionsEntries;
        private decimal TransactionsAmount;
        private string Bank;
        private string Description;
        //--> EGE-62034 : Revamp - CCE - Change Financial flow update
        //private string Card;
        private string Financialflow;
        private string EnhancedFlow;
        //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
        private int PaymentBTA;
        private int LodgedCard;
        private int PaymentAIRPLUS;
        private string AnalyticalCode1;
        private int ServiceGroup;
        private DateTime CreationDate;
        private DateTime ModificationDate;
        private int FictiveBta;
        private string CVC;
        private string FirstTransactionCode;
        private DateTime DateSentCVC;
        private string UserCode;
        private long Token;
        private string ModificationOrder;
        private int Blocked;

        //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
        private string FirstCardReference;
        //<< EGE-79826 - [BO] Lodge Card - First card Enhancement

        public NavisionCardResult()
        {
            // Initialisation
        }

        public void SetValues(SqlDataReader dr)
        {
            this.CustomerNo= dr["Customer No_"].ToString();
            this.TravellerNo = dr["Traveller No_"].ToString();
            this.CardReference= dr ["Card Reference"].ToString();
            this.SharingType = Util.GetSQLInt(dr, "Sharing Type");
            this.CardNo1 = dr["Card No_ 1"].ToString();
            this.CardNo2 = dr["Card No_ 2"].ToString();
            this.CardNo3 = dr["Card No_ 3"].ToString();
            this.CardNo4 = dr["Card No_ 4"].ToString();
            this.CardNo5 = dr["Card No_ 5"].ToString();
            this.ExpirationDate = Util.GetSQLDataTime(dr, "Expiration Date");
            this.ExtendedNo= dr["Extended No_"].ToString();
            this.CardType = Util.GetSQLInt(dr, "Card Type");
            this.Status = Util.GetSQLInt(dr, "Status");
            this.TransactionsEntries = Util.GetSQLInt(dr, "Transactions Entries");
            this.TransactionsAmount=  Util.GetSQLDecimal(dr, "Transactions Amount");
            this.Bank= dr["Bank"].ToString();
            this.Description= dr["Description"].ToString();
            //--> EGE-62034 : Revamp - CCE - Change Financial flow update
            //this.Card= dr["Card"].ToString();
            this.Financialflow = dr["Card"].ToString();
            this.EnhancedFlow = dr["Enhanced Flow"].ToString();
            //<-- EGE-62034 : Revamp - CCE - Change Financial flow update
            this.PaymentBTA = Util.ConvertStringToInt(dr["Payment BTA"].ToString());
            this.LodgedCard = Util.ConvertStringToInt(dr["LodgedCard"].ToString());
            this.PaymentAIRPLUS = Util.ConvertStringToInt(dr["PaymentAIRPLUS"].ToString());
            this.AnalyticalCode1= dr["Analytical Code 1"].ToString();
            this.ServiceGroup = Util.GetSQLInt(dr, "Service Group");
            this.CreationDate = Util.GetSQLDataTime(dr, "CreationDate");
            this.ModificationDate = Util.GetSQLDataTime(dr, "ModificationDate");
            this.FictiveBta = Util.ConvertStringToInt(dr["FictiveBta"].ToString());
            this.CVC= dr["CVC"].ToString();
            this.FirstTransactionCode= dr["first transaction code"].ToString();
            this.DateSentCVC = Util.GetSQLDataTime(dr, "date sent CVC");
            this.UserCode= dr["user code"].ToString();
            this.Token= Util.ConvertStringToToken(dr["Token"].ToString());
            this.ModificationOrder = string.Empty;
            this.Blocked = 0;
            //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
            this.FirstCardReference = dr["FirstCardReference"].ToString();
            //<< EGE-79826 - [BO] Lodge Card - First card Enhancement
        }


        //>> EGE-79826 - [BO] Lodge Card - First card Enhancement
        public string GetFirstCardReference()
        {
            return this.FirstCardReference;
        }
        //<< EGE-79826 - [BO] Lodge Card - First card Enhancement

        public string GetModificationOrder()
        {
            return this.ModificationOrder;
        }
        public int GetBlocked()
        {
            return this.Blocked;
        }
        public string GetCustomerNo()
        {
            return this.CustomerNo;
        }
        public string GetTravellerNo()
        {
            return this.TravellerNo;
        }
        public string GetCardReference()
        {
            return this.CardReference;
        }
        public int GetSharingType()
        {
            return this.SharingType;
        }
        public string GetCardNo1()
        {
            return this.CardNo1;
        }
        public string GetCardNo2()
        {
            return this.CardNo2;
        }
        public string GetCardNo3()
        {
            return this.CardNo3;
        }
        public string GetCardNo4()
        {
            return this.CardNo4;
        }
        public string GetCardNo5()
        {
            return this.CardNo5;
        }
        public DateTime GetExpirationDate()
        {
            return this.ExpirationDate;
        }
        public string GetExtendedNo()
        {
            return this.ExtendedNo;
        }
        public int GetCardType()
        {
            return this.CardType;
        }
        public int GetStatus()
        {
            return this.Status;
        }
        public int GetTransactionsEntries()
        {
            return this.TransactionsEntries;
        }
        public decimal GetTransactionsAmount()
        {
            return this.TransactionsAmount;
        }
        public string GetBank()
        {
            return this.Bank;
        }
        public string GetDescription()
        {
            return this.Description;
        }
        //--> EGE-62034 : Revamp - CCE - Change Financial flow update
        //public string GetCard()
        //{
        //    return this.Card;
        //}
        public string GetFinancialflow()
        {
            return this.Financialflow;
        }
        public string GetEnhancedFlow()
        {
            return this.EnhancedFlow;
        }
        //--> EGE-62034 : Revamp - CCE - Change Financial flow update
        public int GetPaymentBTA()
        {
            return this.PaymentBTA;
        }
        public int GetPaymentAIRPLUS()
        {
            return this.PaymentAIRPLUS;
        }
        public int GetLodgedCard()
        {
            return this.LodgedCard;
        }
        public string GetAnalyticalCode1()
        {
            return this.AnalyticalCode1;
        }
        public int GetServiceGroup()
        {
            return this.ServiceGroup;
        }
        public DateTime GetCreationDate()
        {
            return this.CreationDate;
        }
        public DateTime GetModificationDate()
        {
            return this.ModificationDate;
        }
        public int GetFictiveBta()
        {
            return this.FictiveBta;
        }
        public string GetCVC()
        {
            return this.CVC;
        }
        public string GetFirstTransactionCode()
        {
            return this.FirstTransactionCode;
        }
        public DateTime GetDateSentCVC()
        {
            return this.DateSentCVC;
        }
        public string GetUserCode()
        {
            return this.UserCode;
        }
        public long GetToken()
        {
            return this.Token;
        }

        public bool IsTransactional()
        {
            return (Status == 6);
        }
    }
}