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
using SafeNetWS.business.response.writer;
using SafeNetWS.utils;

namespace SafeNetWS.business.arguments.reader
{
    public class ArgsForVPaymentIDLC
    {

        // Champs communs
        private string Pos;
        private string TravelerCode;
        private string TravelerName;
        private string CC1;
        private string CC2;

        // Champs spécifiques
        private string Company;
        private string TripType;
        private string DepartureFrom;
        private string GoingTo;
        private string DepartureDateString;
        private DateTime DepartureDate;
        private string returnDateString;
        private DateTime returnDate;
        private string BookingDateString;
        private DateTime BookingDate;



        public ArgsForVPaymentIDLC(string pos, string travelerCode, string travelerName, string cc1, string cc2, string tripType,
            string departureFrom, string goingTo, string departureDate, string returnDate, string company, string bookingDate)
        {
            this.Pos = pos;
            this.TravelerCode = travelerCode;
            this.TravelerName = travelerName;
            this.CC1 = cc1;
            this.CC2 = cc2;
            this.TripType=tripType;
            this.DepartureFrom=departureFrom;
            this.GoingTo=goingTo;
            this.DepartureDateString=departureDate;
            this.returnDateString=returnDate;
            this.Company=company;
            this.BookingDateString=bookingDate;
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

        public string GetPOS()
        {
            return this.Pos;
        }
        public void SetPOS(string value)
        {
            this.Pos=value;
        }

        public string GetCC1()
        {
            return this.CC1;
        }
        public void SetCC1(string value)
        {
            this.CC1 = value;
        }

        public string GetCC2()
        {
            return this.CC2;
        }
        public void SetCC2(string value)
        {
            this.CC2 = value;
        }

        public string GetCompany()
        {
            return this.Company;
        }

        public string GetTripType()
        {
            return this.TripType;
        }


        public string GetDepartureFrom()
        {
            return this.DepartureFrom;
        }
        public string GetGoingTo()
        {
            return this.GoingTo;
        }

        public string GetDepartureDateString()
        {
            return this.DepartureDateString;
        }
        public void SetDepartureDate()
        {
            this.DepartureDate = Util.ConvertStringToDate(GetDepartureDateString(), Const.DateFormat_yyyyMMddHHmmss);
        }
        public DateTime GetDepartureDate()
        {
            return this.DepartureDate;
        }
        public string GetBookingType()
        {
            return VPaymentIDResponse.BookingTypeLC;
        }
        public string GetReturnDateString()
        {
            return this.returnDateString;
        }
        public void SetReturnDate()
        {
            this.returnDate = Util.ConvertStringToDate(GetReturnDateString(), Const.DateFormat_yyyyMMddHHmmss);
        }
        public void SetReturnDate(DateTime value)
        {
            this.returnDate = value;
        }
        public DateTime GetReturnDate()
        {
            return this.returnDate;
        }


        public string GetBookingDateString()
        {
            return this.BookingDateString;
        }

   
        public void SetBookingDate()
        {
            this.BookingDate = Util.ConvertStringToDate(GetBookingDateString(), Const.DateFormat_yyyyMMddHHmmss);
        }
        public DateTime GetBookingDate()
        {
            return this.BookingDate;
        }

        public string GetValue()
        {
            return String.Format("Pos={0} Traveller code={1}, Traveler name={2}", GetPOS(), GetTravelerCode(), GetTravelerName());
        }
    }
}
