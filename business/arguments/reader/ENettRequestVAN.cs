//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Sunil Kumar Pidugu (from Sonata Hyderabad).
// Code was reviewed by Samatar Hassan
//===================================================================

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

namespace SafeNetWS.business.arguments.reader
{
    /// <summary>
    /// <RequestVAN>
    ///      <ECN>223227</ECN>
    ///      <Market>DE</Market>
    ///      <MinAuthAmount>0</MinAuthAmount>
    ///      <MaxAuthAmount>100</MaxAuthAmount>
    ///      <ActivationDate>24/10/2013</ActivationDate>
    ///      <ExpiryDate>29/10/2013</ExpiryDate>
    ///      <IsMultiUse>true</IsMultiUse>
    ///      <MultiUseClosePercentage>100</MultiUseClosePercentage>
    ///      <MerchantCategory>Rail</MerchantCategory>
    ///      <Currency>EUR</Currency>
    ///      <Product>Rail</Product>
    ///      <SupplierName><![CDATA[Deutsche Bahn]]></SupplierName>
    ///      <BookingDate>22/10/2013</BookingDate>
    ///      <UserName>Website</UserName>
    ///      <Travellers>
    ///          <Traveller>
    ///               <IsMainTraveller>true</IsMainTraveller>
    ///               <Name>Laurent Greter</Name>
    ///               <Percode>119775</Percode>
    ///               <CC1>test500</CC1>
    ///               <CC2>TESTCC2</CC2>
    ///               <CC3>testCC3</CC3>
    ///               <TravellerOrder>0</TravellerOrder>
    ///           </Traveller>
    ///      </Travellers>
    ///      <ComCode>4703</ComCode>
    ///      <BookerPerCode>119775</BookerPerCode>
    ///      <AgentLogIn>saurabhkumar</AgentLogIn>
    ///      <Channel>ONLINE</Channel>
    ///      <MdCode>110788728</MdCode>
    ///      <RefDossierType>SNCF_TC</RefDossierType>
    ///      <RefDossierCode>4876574</RefDossierCode>
    ///      <TripName><![CDATA[Hamburg Airport_10/01/2014]]></TripName>
    ///      <OriginDate>10/01/2014</OriginDate>
    ///      <OriginLocationName>Berlin</OriginLocationName>
    ///      <OriginLocationCode>BER</OriginLocationCode>
    ///      <OriginCountryCode>DEU</OriginCountryCode>
    ///      <EndDate>11/01/2014</EndDate>
    ///      <EndLocationName>Hamburg Flughafen</EndLocationName>
    ///      <EndLocationCode>8096020</EndLocationCode>
    ///      <EndCountryCode>DEU</EndCountryCode>
    ///     <AdultsCount>1</AdultsCount>
    ///     <FreeText1></FreeText1>
    ///     <FreeText2></FreeText2>
    ///     <FreeText3></FreeText3>
    ///     <HotelID></HotelID>
    ///  
    /// </RequestVAN>
    /// </summary>
    public class ENettRequestVAN
    {
	
        private string ECNfield = string.Empty;
        private string Marketfield = string.Empty;
        private string MinAuthAmountfield = string.Empty;
        private string MaxAuthAmountfield = string.Empty;
        private string ActivationDatefield ;
        private string ExpiryDatefield = string.Empty;
        private string IsMultiUsefield = string.Empty;
        private string MultiUseClosePercentagefield = string.Empty;
        private string MerchantCategoryfield = string.Empty;
        private string Currencyfield = string.Empty;
        private string Productfield = string.Empty;
        private string SupplierNamefield = string.Empty;
        private string BookingDatefield = string.Empty;
        private string UserNamefield = string.Empty;

        private string ComCodefield = string.Empty;
        private string BookerPerCodefield = string.Empty;
        private string AgentLogInfield = string.Empty;
        private string Channelfield = string.Empty;
        private string MdCodefield = string.Empty;
        private string RefDossierTypefield = string.Empty;
        private string RefDossierCodefield = string.Empty;
        private string TripNamefield = string.Empty;
        private string OriginDatefield = string.Empty;
        private string OriginLocationNamefield = string.Empty;
        private string OriginLocationCodefield = string.Empty;
        private string OriginCountryCodefield = string.Empty;
        private string EndDatefield = string.Empty;
        private string EndLocationNamefield = string.Empty;
        private string EndLocationCodefield = string.Empty;
        private string EndCountryCodefield = string.Empty;
        private string AdultsCountfield = string.Empty;
        private string FreeText1field = string.Empty;
        private string FreeText2field = string.Empty;
        private string FreeText3field = string.Empty;
        // Phase 1 of Enett implementation
        private string TravellerNamefield = string.Empty;
        // Phase 1 of Enett implementation
        private string PerCodefield = string.Empty;

        private string HotelIDfield = string.Empty;

        //>> EGE-60949
        private string PosCountryCodefield = string.Empty;
        private string PosCurrencyfield = string.Empty;
        //<< EGE-60949

        public List<Traveller> Travellers { get; set; }
        public class Traveller
        {
            public int TravellerOrder { get; set; }
            public string IsMainTraveller { get; set; }
            public string Name { get; set; }
            public string Percode { get; set; }
            public string CC1 { get; set; }
            public string CC2 { get; set; }
            public string CC3 { get; set; }
        }

        /// <summary>
        /// get, set travellerName
        /// Phase 1 of ENett implementation
        /// </summary>
        public string TravellerName
        {
            get { return TravellerNamefield; }
            set { TravellerNamefield = value; }
        }

        /// <summary>
        /// get, set PerCode
        /// Phase 1 of ENett implementation
        /// </summary>
        public string PerCode
        {
            get { return PerCodefield; }
            set { PerCodefield = value; }
        }

        //>> EGE-60949.
        /// <summary>
        /// get, set PosCountryCode 
        /// </summary>
        public string PosCountryCode 
        {
            get { return PosCountryCodefield; }
            set { PosCountryCodefield = value; }
        }
        /// <summary>
        /// get, set PosCurrency 
        /// </summary>
        public string PosCurrency 
        {
            get { return PosCurrencyfield; }
            set { PosCurrencyfield = value; }
        }
        //<< EGE-60949

        public string HotelID
        {
            get { return HotelIDfield; }
            set { HotelIDfield = value; }
        }
        public string FreeText1
        {
            get { return FreeText1field; }
            set { FreeText1field = value; }
        }
        public string FreeText2
        {
            get { return FreeText2field; }
            set { FreeText2field = value; }
        }
        public string FreeText3
        {
            get { return FreeText3field; }
            set { FreeText3field = value; }
        }
        public string ECN
        {
            get { return ECNfield; }
            set { ECNfield = value; }
        }
        public string Market
        {
            get { return Marketfield; }
            set { Marketfield = value; }
        }

        public string MinAuthAmount
        {
            get { return MinAuthAmountfield; }
            set { MinAuthAmountfield = value; }
        }

        public string MaxAuthAmount
        {
            get { return MaxAuthAmountfield; }
            set { MaxAuthAmountfield = value; }
        }
        public string SupplierName
        {
            get { return SupplierNamefield; }
            set { SupplierNamefield = value; }
        }
        public string ActivationDate
        {
            get { return ActivationDatefield; }
            set { ActivationDatefield = value; }
        }
        public string Currency
        {
            get { return Currencyfield; }
            set { Currencyfield = value; }
        }
 
        public string Product
        {
            get { return Productfield; }
            set { Productfield = value; }
        }
        public string BookingDate
        {
            get { return BookingDatefield; }
            set { BookingDatefield = value; }
        }
        public string UserName
        {
            get { return UserNamefield; }
            set { UserNamefield = value; }
        }
       
        public string MerchantCategory
        {
            get { return MerchantCategoryfield; }
            set { MerchantCategoryfield = value; }
        }
        public string ExpiryDate
        {
            get { return ExpiryDatefield; }
            set { ExpiryDatefield = value; }
        }
        public string IsMultiUse
        {
            get { return IsMultiUsefield; }
            set { IsMultiUsefield = value; }
        }
        public string MultiUseClosePercentage
        {
            get { return MultiUseClosePercentagefield; }
            set { MultiUseClosePercentagefield = value; }
        }


        public string ComCode
        {
            get { return ComCodefield; }
            set { ComCodefield = value; }
        }


        public string BookerPerCode
        {
            get { return BookerPerCodefield; }
            set { BookerPerCodefield = value; }
        }

        public string AgentLogIn
        {
            get { return AgentLogInfield; }
            set { AgentLogInfield = value; }
        }


        public string Channel
        {
            get { return Channelfield; }
            set { Channelfield = value; }
        }


        public string MdCode
        {
            get { return MdCodefield; }
            set { MdCodefield = value; }
        }


        public string RefDossierType
        {
            get { return RefDossierTypefield; }
            set { RefDossierTypefield = value; }
        }

        public string RefDossierCode
        {
            get { return RefDossierCodefield; }
            set { RefDossierCodefield = value; }
        }

        public string TripName
        {
            get { return TripNamefield; }
            set { TripNamefield = value; }
        }


        public string OriginDate
        {
            get { return OriginDatefield; }
            set { OriginDatefield = value; }
        }
        public string OriginLocationName
        {
            get { return OriginLocationNamefield; }
            set { OriginLocationNamefield = value; }
        }

        public string OriginLocationCode
        {
            get { return OriginLocationCodefield; }
            set { OriginLocationCodefield = value; }
        }
        
        public string OriginCountryCode
        {
            get { return OriginCountryCodefield; }
            set { OriginCountryCodefield = value; }
        }
        
        public string EndLocationName
        {
            get { return EndLocationNamefield; }
            set { EndLocationNamefield = value; }
        }

        public string EndDate
        {
            get { return EndDatefield; }
            set { EndDatefield = value; }
        }

        public string EndLocationCode
        {
            get { return EndLocationCodefield; }
            set { EndLocationCodefield = value; }
        }
  
        public string EndCountryCode
        {
            get { return EndCountryCodefield; }
            set { EndCountryCodefield = value; }
        }

        public string AdultsCount
        {
            get { return AdultsCountfield; }
            set { AdultsCountfield = value; }
        }
  

    }

   
}
