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
    public class ArgsForVPaymentIDHotel
    {
        // Champs communs
        private string Pos;
        private string TravelerCode;
        private string TravelerName;
        private string CC1;
        private string CC2;

        // Champs spécifiques
        private string HotelType;
        private string HotelName;
        private string City;
        private string ZipCode;
        private string ArrivalDateString;
        private DateTime ArrivalDate;
        private string DepartureDateString;
        private DateTime DepartureDate;
        private string Country;
        private string BookingDateString;
        private DateTime BookingDate;


        public ArgsForVPaymentIDHotel(string pos, string travelerCode, string travelerName, string cc1, string cc2,
            string hotelType, string hotelName, string city, string zipCode, string arrivalDate, string departureDate,
            string country, string bookingDate)
        {
            this.Pos = pos;
            this.TravelerCode = travelerCode;
            this.TravelerName = travelerName;
            this.CC1 = cc1;
            this.CC2 = cc2;
            this.HotelType = hotelType;
            this.HotelName = hotelName;
            this.City = city;
            this.ZipCode = zipCode;
            this.Country = country;
            this.ArrivalDateString = arrivalDate;
            this.DepartureDateString = departureDate;
            this.BookingDateString = bookingDate;
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
            this.CC1=value;
        }

        public string GetCC2()
        {
            return this.CC2;
        }
        public void SetCC2(string value)
        {
            this.CC2 = value;
        }


        public string GetHotelType()
        {
            return this.HotelType;
        }

        public string GetHotelName()
        {
            return this.HotelName;
        }
        public string GetBookingType()
        {
            return VPaymentIDResponse.BookingTypeHotel;
        }

        public string GetCity()
        {
            return this.City;
        }

        public string GetZipCode()
        {
            return this.ZipCode;
        }
        public string GetCountry()
        {
            return this.Country;
        }

        public void SetCountry(string value)
        {
            this.Country = value;
        }

        public string GetDepartureDateString()
        {
            return this.DepartureDateString;
        }
        public DateTime GetDepartureDate()
        {
            return this.DepartureDate;
        }
        public void SetDepartureDate()
        {
            this.DepartureDate = Util.ConvertStringToDate(GetDepartureDateString(), Const.DateFormat_yyyyMMddHHmmss);
        }
        public string GetArrivalDateString()
        {
            return this.ArrivalDateString;
        }
        public DateTime GetArrivalDate()
        {
            return this.ArrivalDate;
        }
        public void SetArrivalDate()
        {
            this.ArrivalDate = Util.ConvertStringToDate(GetArrivalDateString(), Const.DateFormat_yyyyMMddHHmmss);
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
            return String.Format("Pos={0}, Traveller code={1}, Traveler name={2}", GetPOS(), GetTravelerCode(), GetTravelerName());
        }
    }
}
