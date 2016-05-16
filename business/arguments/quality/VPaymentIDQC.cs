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
using SafeNetWS.business.response.writer;
using SafeNetWS.exception;
using SafeNetWS.login;
using SafeNetWS.business.arguments.reader;
using SafeNetWS.utils;

namespace SafeNetWS.business.arguments.quality
{
    public class VPaymentIDQC
    {

        public static void ValidCommunValues(UserInfo user, List<CEEException> exceptions, string pos, string travelerCode, string travelerName,
            string cc1, string cc2, string bookingDate)
        {

            // Point de vente
            if (String.IsNullOrEmpty(pos))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_POS",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingPOS", false)));
            }

            // Code voyageur
            if (String.IsNullOrEmpty(travelerCode))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_TRAVELER_CODE",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingTravelerCode", false)));
            }

            // Nom voyageur
            if (String.IsNullOrEmpty(travelerName))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_TRAVELER_NAME",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingTravelerName", false)));
            }
            /*
            // CC1
            if (String.IsNullOrEmpty(cc1))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_MAIN_COST_CENTER",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingCC1", false)));
            }

            // CC2
            if (String.IsNullOrEmpty(cc2))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_SECONDARY_COST_CENTER",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingCC2", false)));
            }
            */
        }

        /// <summary>
        /// Quality control
        /// Validation des arguments passés
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="args">Liste arguments</param>
        /// <param name="exceptions">Liste d'exceptions à mettre à jour</param>
        public static void ValidateForLC(UserInfo user, ArgsForVPaymentIDLC args, List<CEEException> exceptions)
        {
            // On fait à ce niveau des contrôles
            // pour les valeurs communes
            ValidCommunValues(user, exceptions, args.GetPOS(), args.GetTravelerCode(), args.GetTravelerName(),
                args.GetCC1(), args.GetCC2(), args.GetBookingDateString());

            // On corrige le Pos
            args.SetPOS(Util.CorrectPos(user, args.GetPOS()));
            
            if (!String.IsNullOrEmpty(args.GetTravelerCode()))
            {
                // On corrige leTravelerCode
                args.SetTravelerCode(args.GetTravelerCode().ToUpper());
            }
            if (!String.IsNullOrEmpty(args.GetCC1()))
            {
                // On corrige le CC1
                args.SetCC1(args.GetCC1().ToUpper());
            }
            if (!String.IsNullOrEmpty(args.GetCC2()))
            {
                // On corrige le CC2
                args.SetCC2(args.GetCC2().ToUpper());
            }
            // AIR Company
            if (String.IsNullOrEmpty(args.GetCompany()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_AIR_COMPANY",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingAIRCompany", false)));
            }

             // Trip tip
            if (String.IsNullOrEmpty(args.GetTripType()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_TRIP_TYPE",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingTripType", false)));
            }

            // Departure from
            if (String.IsNullOrEmpty(args.GetDepartureFrom()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_DEPARTURE_FROM",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingDepartureFrom", false)));
            }

            // Going from
            if (String.IsNullOrEmpty(args.GetGoingTo()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_GOING_TO",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
               user.GetMessages().GetString("VPaymentGeneration.Error.MissingGoingTo", false)));
            }

            // Date de départ
            if (String.IsNullOrEmpty(args.GetDepartureDateString()))
            {
                // la date de réservation n'a pas été renseignée
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_DEPARTURE_DATE",
                   CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                   CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                   user.GetMessages().GetString("VPaymentGeneration.Error.DepartureDateMissing", false)));
            }
            else
            {
                // La date de départ a été renseigné
                // il faut la convertir en date
                try
                {
                    args.SetDepartureDate();
                }
                catch (Exception)
                {
                    exceptions.Add(CCEExceptionUtil.BuildCCEException("INVALID_DEPARTURE_DATE",
                        CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                        CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                        user.GetMessages().GetString("VPaymentGeneration.Error.DepartureDateInvalid", false)));
                }
            }

            // Date de retour
            if (args.GetTripType().Equals("RETURN"))
            {
                if (String.IsNullOrEmpty(args.GetReturnDateString()))
                {
                    // la date de retour n'a pas été renseignée
                    exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_RETURN_DATE",
                       CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                       CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                       user.GetMessages().GetString("VPaymentGeneration.Error.ReturnDateMissing", false)));
                }
                else
                {
                    // La date de retour a été renseigné
                    // il faut la convertir en date
                    try
                    {
                        args.SetReturnDate();
                    }
                    catch (Exception)
                    {
                        exceptions.Add(CCEExceptionUtil.BuildCCEException("INVALID_RETURN_DATE",
                            CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                            CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                            user.GetMessages().GetString("VPaymentGeneration.Error.ReturnDateInvalid", false)));
                    }
                }
            }
            else
            {
                // One way trip
                args.SetReturnDate(Util.GetNavisionEmptyDate());
            }       

            // Date de booking
            if (String.IsNullOrEmpty(args.GetBookingDateString()))
            {
                // la date de réservation n'a pas été renseignée
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_BOOKING",
                   CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                   CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                   user.GetMessages().GetString("VPaymentGeneration.Error.BookingDateMissing", false)));
            }
            else
            {
                // La date de départ a été renseigné
                // il faut la convertir en date
                try
                {
                    args.SetBookingDate();
                }
                catch (Exception)
                {
                    exceptions.Add(CCEExceptionUtil.BuildCCEException("INVALID_BOOKING_DATE",
                        CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                        CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                        user.GetMessages().GetString("VPaymentGeneration.Error.BookingDateInvalid", false)));
                }
            }
        }

        /// <summary>
        /// Quality control
        /// Validation des arguments passés
        /// </summary>
        /// <param name="user">Compte utilisateur</param>
        /// <param name="args">Liste arguments</param>
        /// <param name="exceptions">Liste d'exceptions à mettre à jour</param>
        public static void ValidateForHotel(UserInfo user, ArgsForVPaymentIDHotel args, List<CEEException> exceptions)
        {
            // On fait à ce niveau des contrôles
            // avant de rentrer dans le vif du sujet, on va vérifier les dates

            // pour les valeurs communes
            ValidCommunValues(user, exceptions, args.GetPOS(), args.GetTravelerCode(), args.GetTravelerName(),
                args.GetCC1(), args.GetCC2(), args.GetBookingDateString());

            if (!String.IsNullOrEmpty(args.GetPOS()))
            {
                // On corrige le Pos
                args.SetPOS(Util.CorrectPos(user,args.GetPOS()));
            }
            if (!String.IsNullOrEmpty(args.GetTravelerCode()))
            {
                // On corrige leTravelerCode
                args.SetTravelerCode(args.GetTravelerCode().ToUpper());
            }
            if (!String.IsNullOrEmpty(args.GetCC1()))
            {
                // On corrige le CC1
                args.SetCC1(args.GetCC1().ToUpper());
            }
            if (!String.IsNullOrEmpty(args.GetCC2()))
            {
                // On corrige le CC2
                args.SetCC2(args.GetCC2().ToUpper());
            }
            // Type d'hotel
            if (String.IsNullOrEmpty(args.GetHotelType()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_TYPE",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingHotelType", false)));
            }

            // Nom de l'hotel
            if (String.IsNullOrEmpty(args.GetHotelName()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_NAME",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingHotelName", false)));
            }

            // Ville de l'hotel
            if (String.IsNullOrEmpty(args.GetCity()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_CITY",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingHotelCity", false)));
            }

            // Code postal de l'hotel
            if (String.IsNullOrEmpty(args.GetZipCode()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_ZIP_CODE",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingHotelZipCode", false)));
            }

            // Pays
            if (String.IsNullOrEmpty(args.GetCountry()))
            {
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_COUNTRY",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.MissingHotelCountry", false)));
            }
            else
            {
                args.SetPOS(args.GetPOS().ToUpper());
            }

            // Date d'arrivée
            if (String.IsNullOrEmpty(args.GetArrivalDateString()))
            {
                // la date d'arrivée n'a pas été renseignée
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_ARRIVAL_DATE",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.HotelArrivalDateMissing", false)));
            }
            else
            {
                // La date a été renseignée
                // Nous devons la convertir en date
                try
                {
                    args.SetArrivalDate();
                }
                catch (Exception)
                {
                    exceptions.Add(CCEExceptionUtil.BuildCCEException("INVALID_ARRIVAL_DATE",
                        CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                        CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                        user.GetMessages().GetString("VPaymentGeneration.Error.HotelArrivalDateInvalid", false)));
                }
            }


            
            // Date de départ
            if (String.IsNullOrEmpty(args.GetDepartureDateString()))
            {
                // la date de départ n'a pas été renseignée
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_HOTEL_DEPARTURE_DATE",
                    CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                    CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                    user.GetMessages().GetString("VPaymentGeneration.Error.HotelDepartureDateMissing", false)));
            }
            else
            {
                // La date de départ a été renseigné
                // il faut la convertir en date
                try
                {
                    args.SetDepartureDate();
                }
                catch (Exception)
                {
                    exceptions.Add(CCEExceptionUtil.BuildCCEException("INVALID_DEPARTURE_DATE",
                        CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                        CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                        user.GetMessages().GetString("VPaymentGeneration.Error.HotelDepartureDateInvalid", false)));
                }
            }

            // Date de booking
            if (String.IsNullOrEmpty(args.GetBookingDateString()))
            {
                // la date de réservation n'a pas été renseignée
                exceptions.Add(CCEExceptionUtil.BuildCCEException("MISSING_BOOKING",
                   CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                   CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                   user.GetMessages().GetString("VPaymentGeneration.Error.BookingDateMissing", false)));
            }
            else
            {
                // La date de départ a été renseigné
                // il faut la convertir en date
                try
                {
                    args.SetBookingDate();
                }
                catch (Exception)
                {
                    exceptions.Add(CCEExceptionUtil.BuildCCEException("INVALID_BOOKING_DATE",
                        CCEExceptionMap.EXCEPTION_TYPE_FONCTIONAL,
                        CCEExceptionMap.EXCEPTION_SEVERITY_ERROR,
                        user.GetMessages().GetString("VPaymentGeneration.Error.BookingDateInvalid", false)));
                }
            }
        }


    }
}
