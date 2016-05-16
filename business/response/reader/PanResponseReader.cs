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
using System.Web;
using System.Web.Security;
using System.Xml;
using System.IO;
using SafeNetWS.utils;

namespace SafeNetWS.business.response.reader
{
    /**
     * Cette classe permet de lire la réponse apportée
     * par la méthode de récupération du token
     * L'entrée réponse est structurée de la manière suivante :
     * <?xml version="1.0" encoding="ISO-8859-1"?>
     * <Response>
     *   <Duration>Valeur de retour</Duration>
     *   <Value>
     *      <Pan>Valeur de retour</Pan>
     *      <truncatedPAN>Valeur de retour</truncatedPAN>
     *   </Value>
     *   <Exception>
     *      <Count>0</Count>
     *      <Message></Message>
     *  </Exception>
     * </Response>
     * 
     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */
    public class PanResponseReader
    {
        // Value PAN to return (serialized into string)
        private const string Xml_Response_PAN_TagName = "Pan";
        // Value truncatedPAN to return (serialized into string)
        private const string Xml_Response_TruncatedPAN_TagName = "truncatedPAN";
        // Value Duration In Milliseconds to return (serialized into string)
        private const string Xml_Response_Duration_TagName = "Duration";

        private string InputResponse;
        // Valeurs de retour
        private string PAN;
        private string truncatedPan;
        private double Duration;

        private int ExceptionCount;
        private string ExceptionMessage;


        // Exception 
        // Exception code (0 = no error otherwise 1)
        private const string Xml_Response_Exception_Code_TagName = "Count";
        // Exception message
        private const string Xml_Response_Exception_Message_TagName = "Message";


        public PanResponseReader(string inputResponse)
        {
            this.InputResponse = inputResponse;

            // On va lire l'entrée
            ParseResponse();
           
        }

        private void ParseResponse()
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                //The XmlResolver property is set to null. External resources are not resolved.
                doc.XmlResolver = null;

                // On charge la réponse
                doc.Load(new StringReader(InputResponse));
                   
                try
                {
                    // On récupère en premier l'état d'exception
                    this.ExceptionCount = Util.ConvertStringToInt(doc.GetElementsByTagName(Xml_Response_Exception_Code_TagName)[0].InnerXml);

                    if (this.ExceptionCount > 0)
                    {
                        this.ExceptionMessage = doc.GetElementsByTagName(Xml_Response_Exception_Message_TagName)[0].InnerXml;
                    }
                }
                catch (Exception)
                {
                    // Visiblement pas d'exception
                    // 
                }
                
                // Pas d'exception, on récupère les valeurs
                if (this.ExceptionCount == 0)
                {
                    // On récupère directement la valeur de la node <Value></Value>
                    // Il ne doit y avoir qu'une node de ce type
                    this.PAN = doc.GetElementsByTagName(Xml_Response_PAN_TagName)[0].InnerXml;
                    this.truncatedPan = doc.GetElementsByTagName(Xml_Response_TruncatedPAN_TagName)[0].InnerXml;
                    this.Duration = Util.ConvertStringToDouble(doc.GetElementsByTagName(Xml_Response_Duration_TagName)[0].InnerXml);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la lecture de la réponse! Erreur :" + e.Message);
            }
            
        }

  
       public string GetPan()
       {
           return this.PAN;
       }

       public string GetTruncatedPan()
       {
           return this.truncatedPan;
       }
 
       public string GetExceptionMessage()
       {
           return this.ExceptionMessage;
       }
       public bool IsError()
       {
           return (this.ExceptionCount > 0);
       }
       public double GetDuration()
       {
           return this.Duration;
       }
    }
}
