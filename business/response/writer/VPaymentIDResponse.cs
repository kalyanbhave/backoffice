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
using System.Collections;
using SafeNetWS.utils;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.login;
using SafeNetWS.exception;
using SafeNetWS.business.arguments.quality;
using SafeNetWS.business.arguments.reader;

namespace SafeNetWS.business.response.writer
{
    /**
     * Cette classe permet de construire la réponse apportée
     * par la méthode de génération d'ID VPayment
     * La réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <VPaymentID>XXXXX</VPaymentID>
     *   </Value>
     *    <Exceptions>
     *    <Count>0</Count>
     *      <Exception>
     *          <Code></Code>
     *          <Severity></Severity>
     *          <Type></Type>
     *          <Message></Message>
     *      </Exception>
     *   </Exceptions>
     * </Response>
     * 
     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/06/2010
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class VPaymentIDResponse
    {
        public const string BookingTypeHotel = "HOTEL";
        public const string BookingTypeLC = "LOW COST";

        private const string Xml_Response_Open_Tag="<Response>";
        private const string Xml_Response_Close_Tag = "</Response>";
        private const string Xml_Response_Value_Open_Tag = "<Value>";
        private const string Xml_Response_Value_Close_Tag = "</Value>";
        // Value VPayment to return (serialized into string)
        private const string Xml_Response_VPaymentID_Open_Tag = "<VPaymentID>";
        private const string Xml_Response_VPaymentID_Close_Tag = "</VPaymentID>";
       
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_Open_Tag = "<Duration>";
        private const string Xml_Response_Duration_Close_Tag = "</Duration>";

        // Exceptions 
        private const string Xml_Response_Exceptions_Open_Tag = "<Exceptions>";
        private const string Xml_Response_Exceptions_Close_Tag = "</Exceptions>";
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Count_Open_Tag = "<Count>";
        private const string Xml_Response_Exception_Count_Close_Tag = "</Count>";
        // Exception 
        private const string Xml_Response_Exception_Open_Tag = "<Exception>";
        private const string Xml_Response_Exception_Close_Tag = "</Exception>";
        // Exception code
        private const string Xml_Response_Exception_Code_Open_Tag = "<Code>";
        private const string Xml_Response_Exception_Code_Close_Tag = "</Code>";
        // Exception severity
        private const string Xml_Response_Exception_Severity_Open_Tag = "<Severity>";
        private const string Xml_Response_Exception_Severity_Close_Tag = "</Severity>";
        // Exception type
        private const string Xml_Response_Exception_Type_Open_Tag = "<Type>";
        private const string Xml_Response_Exception_Type_Close_Tag = "</Type>";
        // Exception message
        private const string Xml_Response_Exception_Message_Open_Tag = "<Message>";
        private const string Xml_Response_Exception_Message_Close_Tag = "</Message>";


        private string VPaymentID;

        // Liste des exceptions
        private List<CEEException> Exceptions;

        private UserInfo User;
        private DateTime StartDate;

        private ArgsForVPaymentIDLC LCArgs;
        private ArgsForVPaymentIDHotel HotelArgs;

        private string bookingType;

        public VPaymentIDResponse(string bookingType)
        {
            // Initialisation
            this.StartDate = DateTime.Now;
            // On initialise les exceptions
            this.Exceptions = new List<CEEException>();
            // Le type de booking
            // Hotel ou Low Cost
            this.bookingType = bookingType;
        }

        public void SetArguments(ArgsForVPaymentIDLC args)
        {
            this.LCArgs = args;
        }
        public void SetArguments(ArgsForVPaymentIDHotel args)
        {
            this.HotelArgs = args;
        }

        public string GetBookingType()
        {
            return this.bookingType;
        }


        public void SetValues(UserInfo useri, string id)
        {
            SetUser(useri);
            this.VPaymentID = id;
        }


        public void Validate()
        {
            switch (GetBookingType())
            {
                case BookingTypeHotel:
                    // On initialise le quality control pour les hotels
                    VPaymentIDQC.ValidateForHotel(GetUser(), GetHotelArguments(), GetExceptions());
                    break;
                case BookingTypeLC:
                    // On initialise le quality control pour les low costs
                    VPaymentIDQC.ValidateForLC(GetUser(), GetLCArguments(), GetExceptions());
                    break;
                default: break;
            }

        }

        public void SetUser(UserInfo useri)
        {
            this.User = useri;
        }

        public void AddException(UserInfo useri, string message)
        {
            CEEException value = new CEEException(message);
            AddException(useri, value);
        }
        public void AddException(UserInfo useri, CEEException exception)
        {
            this.Exceptions.Add(exception);
            SetUser(useri);
        }

        public void AddException(UserInfo useri, Exception exception)
        {
            AddException(useri, exception.Message);
        }


        /// <summary>
        /// Retourne la valeur renseigné par le client
        /// </summary>
        /// <returns>Retourne</returns>
        public ArgsForVPaymentIDLC GetLCArguments()
        {
            return this.LCArgs;
        }

        /// <summary>
        /// Retourne la valeur renseigné par le client
        /// </summary>
        /// <returns>Retourne</returns>
        public ArgsForVPaymentIDHotel GetHotelArguments()
        {
            return this.HotelArgs;
        }


        /// <summary>
        /// Retourne TRUE si la réponse a une erreur
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public bool IsError()
        {
            return (GetExceptionCount() > 0);
        }


        private List<CEEException> GetExceptions()
        {
            return this.Exceptions;
        }

        /// <summary>
        /// Retourne les informations à retourner
        /// à la fin du traitement
        /// </summary>
        /// <returns>Valeur à retourner</returns>
        private string GetValueMessage()
        {
            return String.Format("VPaymentID ={0}", GetVPaymentID());
        }

        private string GetAllExceptions()
        {
            string retval = string.Empty;
            if (IsError())
            {
                CEEException[] exs = this.Exceptions.ToArray();
                int nr=exs.Length;
                for (int i = 0; i < nr; i++)
                {
                    // On récupère l'exception
                    CEEException ex = exs[i];
                    retval+=". " +ex.GetExceptionMessage();
                }
            }
            return retval;
        }

        /// <summary>
        /// Retour de la réponse structurée en XML
        /// </summary>
        /// <returns>Réponse (XML)</returns>
        public string GetResponse()
        {
            // On trace la demande
            LogResponse();
            // On va maintenant contruire la réponse
            string strData = Const.XmlHeader
                + Xml_Response_Open_Tag
                        +Xml_Response_Duration_Open_Tag
                            + GetDuration()
                        + Xml_Response_Duration_Close_Tag;
                if(!IsError())
                 {
                    // Il n'y aucune erreur
                    // On va renvoyer les données
                    // et de ce fait ignorer les tag d'exception
                     strData +=
                       Xml_Response_Value_Open_Tag
                         + Xml_Response_VPaymentID_Open_Tag
                             + GetVPaymentID()
                         + Xml_Response_VPaymentID_Close_Tag
                     + Xml_Response_Value_Close_Tag;
                }
                else
                {
                    // On a une exception
                    // Il faut renvoyer les tags d'exception et
                    // de ce fait ne pas ajouter les tags sur ls données
                    strData +=
                     Xml_Response_Exceptions_Open_Tag
                        + Xml_Response_Exception_Count_Open_Tag
                            + GetExceptionCount()
                        + Xml_Response_Exception_Count_Close_Tag;
                        // On parcourir les exceptions
                        CEEException[] exs = this.Exceptions.ToArray();
                        int nr=exs.Length;
                        for(int i=0; i<nr; i++)
                        {
                            // On récupère l'exception
                            CEEException ex= exs[i];
                            strData +=
                            Xml_Response_Exception_Open_Tag
                                + Xml_Response_Exception_Code_Open_Tag
                                    + ex.GetExceptionCode()
                                + Xml_Response_Exception_Code_Close_Tag
                                + Xml_Response_Exception_Severity_Open_Tag
                                     + ex.GetExceptionSeverity()
                                + Xml_Response_Exception_Severity_Close_Tag
                                + Xml_Response_Exception_Type_Open_Tag
                                      + ex.GetExceptionType()
                                + Xml_Response_Exception_Type_Close_Tag
                                + Xml_Response_Exception_Message_Open_Tag
                                      + ex.GetExceptionMessage()
                                + Xml_Response_Exception_Message_Close_Tag
                           + Xml_Response_Exception_Close_Tag;
                        }
                        strData +=
                        Xml_Response_Exceptions_Close_Tag;
                }
                strData += 
                Xml_Response_Close_Tag;
            return strData;
        }

        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
       public UserInfo GetUser()
       {
           return this.User;
       }
       /// <summary>
       /// On va répondre au client
       /// mais avant, nous devons tracer cette demande
       /// en informant Syslog
       /// </summary>
       private void LogResponse()
       {
           Services.WriteOperationStatusToLog(GetUser(),
               String.Format(" and provided {0}", GetBookingType().Equals(BookingTypeHotel) ? GetHotelArguments().GetValue() : GetLCArguments().GetValue()),
             String.Format(".The following values were returned to user : {0}", GetValueMessage()),
             String.Format(".Unfortunately, the process failed for the following reason: {0}", GetAllExceptions()),
             IsError(),
             GetDuration());
       }
   

       /// <summary>
       /// Retourne la durée du traitement
       /// en ms
       /// </summary>
       /// <returns>Durée (ms)</returns>
       private string GetDuration()
       {
           return Util.GetDuration(this.StartDate).ToString();
       }


       /// <summary>
       /// Retourne le nombre d'erreur
       /// </summary>
       /// <returns>Nombre d'erreurs</returns>
       private int GetExceptionCount()
       {
           return GetExceptions().Count;
       }
       /// <summary>
       /// Retourne l'ID VPayment
       /// </summary>
       /// <returns>ID VPayment</returns>
       private string GetVPaymentID()
       {
           return this.VPaymentID;
       }

 
    }
}
