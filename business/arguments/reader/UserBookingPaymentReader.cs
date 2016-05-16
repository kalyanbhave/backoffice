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
using System.Xml;
using System.IO;
using SafeNetWS.utils;

namespace SafeNetWS.business.arguments.reader
{
    /**
     * Cette classe permet de lire les arguments apportée
     * par les méthodes de recherche de la carte de paiement
     * avec recherche hiérarchique 
     * L'entrée réponse est structurée de la manière suivante :
     * <ECTEGetUserBookingPaymentRQ>
     * <ContextRQ>
     * <Language>FR</Language>
     * <Application>RTL</Application>
     * <UserAgent>MidOffice</UserAgent>
     * </ContextRQ>
     * <Parameters>
     * <PosCode>FR</PosCode>
     * <PerCode>816985</PerCode>
     * <ComCode>820277</ComCode>
     * <BillingEntity>
     *    <Value>Droege & Comp GmbH</Value>
     *    <Code>CC1</Code>
     * </BillingEntity>
     * <Service>AIR</Service>
     * <Token>xxx</Token>
     * </Parameters>
     * </ECTEGetUserBookingPaymentRQ>

     * Le client doit parser cet XML et extraire en premier le tag "Exception/Count"
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * 
     * 
     */

    public class UserBookingPaymentReader
    {

        private string InputString;
        // Valeurs de retour
        private string Lang;
        private string comcode;
        private string cc1;
        private string percode;
        private string service;
        private string Token;
        private string poscode;



        // Value Language
        private const string Xml_Input_Language_TagName = "Language";
        // Value Pos
        private const string Xml_Input_ComCode_TagName = "ComCode";
        // Value ComCode
        private const string Xml_Input_PosCode_TagName = "PosCode";
        // Value PerCode
        private const string Xml_Input_PerCode_TagName = "PerCode";
        // Value CC1
        private const string Xml_Input_Cc1_TagName = "Value";
        // Value Service
        private const string Xml_Input_Service_TagName = "Service";
        // Value Token
        private const string Xml_Input_Token_TagName = "Token";

        public UserBookingPaymentReader(string inputString)
        {
            this.InputString = Util.HtmlEncode(inputString);

            // On va lire l'entrée
            ParseInput();
        }
        
        public UserBookingPaymentReader(string lang, string poscode, string comcode, string cc1, string percode
         , string service)
        {
            this.InputString = String.Format("PosCode={0}, Comcode={1}, CC1={2}, Percode={3}", poscode, comcode, percode, service);
            this.poscode = poscode;
            this.comcode = comcode;
            this.cc1 = cc1;
            this.percode = percode;
            this.service = service;
        }


        private void ParseInput()
        {
            // On se prépare à parser les arguments
            XmlDocument doc = null;
         
            try
            {
                doc = new XmlDocument();
                //The XmlResolver property is set to null. External resources are not resolved.
                doc.XmlResolver = null;

                // On charge la réponse
                doc.Load(new StringReader(this.InputString));

                // Get language
                this.Lang = doc.GetElementsByTagName(Xml_Input_Language_TagName)[0].InnerXml;
                if (!String.IsNullOrEmpty(this.Lang)) this.Lang = this.Lang.ToLower();
                // Get argument pos
                this.poscode = doc.GetElementsByTagName(Xml_Input_PosCode_TagName)[0].InnerXml;
                // Get argument comcode
                this.comcode = doc.GetElementsByTagName(Xml_Input_ComCode_TagName)[0].InnerXml;
                // Get argument percode
                this.percode = doc.GetElementsByTagName(Xml_Input_PerCode_TagName)[0].InnerXml;

                // Get argument cc1
                this.cc1 = doc.GetElementsByTagName(Xml_Input_Cc1_TagName)[0].InnerXml;
                if (!String.IsNullOrEmpty(this.cc1))
                {
                    // Elimination des caractères spéciaux
                    this.cc1 = Util.HtmlDecode(this.cc1);
                }

                // Get argument service
                this.service = doc.GetElementsByTagName(Xml_Input_Service_TagName)[0].InnerXml;
                if (!String.IsNullOrEmpty(this.service))
                {
                    // Elimination des caractères spéciaux
                    this.service = Util.HtmlDecode(this.service);
                }

                // On va maintenant extraire le token
                // ATTENTION ce tag n'est pas obligatoire
                // donc l'information peut ne pas être présente
                try
                {
                    // Get argument Token
                    this.Token = doc.GetElementsByTagName(Xml_Input_Token_TagName)[0].InnerXml;
                }catch(Exception){};  // On ignore cette erreur
            }
            catch (Exception e)
            {
                this.poscode = null;
                this.comcode = null;
                this.percode = null;
                throw new Exception(e.Message);
            }
            
        }

        

            
       /// <summary>
       /// Retourne les arguments d'appel
       /// </summary>
       /// <returns>Arguments d'appel/returns>
        public string GetInputString()
       {
           return this.InputString;
       }


       /// <summary>
       /// Retourne le code companie
       /// </summary>
       /// <returns>Code companie</returns>
       public string GetComCode()
       {
           return this.comcode;
       }

       /// <summary>
       /// Retourne le code voyageur
       /// </summary>
       /// <returns>Code voyageur</returns>
       public string GetPerCode()
       {
           return this.percode;
       }

       /// <summary>
       /// Retourne le service
       /// </summary>
       /// <returns>Service</returns>
       public string GetService()
       {
           return this.service;
       }

       /// <summary>
       /// Retourne le POS
       /// </summary>
       /// <returns>POS</returns>
       public string GetPos()
       {
           return this.poscode;
       }


       /// <summary>
       /// Retourne le centre de cout 1
       /// </summary>
       /// <returns>Centre de cout 1</returns>
       public string GetCc1()
       {
           return this.cc1;
       }

       /// <summary>
       /// Retourne la langue
       /// </summary>
       /// <returns>Langue</returns>
       public string GetLang()
       {
           return this.Lang;
       }

       /// <summary>
       /// Retourne le token 
       /// </summary>
       /// <returns>Token</returns>
       public string GetToken()
       {
           return this.Token;
       }
    }
}
