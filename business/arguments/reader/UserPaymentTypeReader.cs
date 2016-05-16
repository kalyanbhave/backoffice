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
     * par les méthodes de recherche du moyen de paiement
     * L'entrée réponse est structurée de la manière suivante :
     * <ECTEGetUserPaymentTypeRQ>
     *    <ContextRQ>
     *       <Language>FR</Language>
     *       <Application>RTL</Application>
     *       <UserAgent>MidOffice</UserAgent>
     *    </ContextRQ>
     *    <Parameters>
     *       <PerCode>844325</PerCode>
     *       <ComCode>4794</ComCode>
     *       <Service>AIR</Service>
     *    </Parameters>
     * </ECTEGetUserPaymentTypeRQ>
     * 
     * 
     * Date : 13/10/2009
     * Auteur : Samatar HASSAN
     * --------------------------
     * MAJ: 06/12/2011
     * Auteur : Samatar HASSAN
     * Possibilité d'affecter les valeurs directement sans passer
     * par la valeur XML
     * 
     */
    public class UserPaymentTypeReader
    {

        private string inputString;
        // Valeurs de retour
        private string language;
        private string comcode;
        private string costCenter;
        private string percode;
        private string service;
        private string poscode;

        // Value Language
        private const string Xml_Input_Language_TagName = "Language";
        // Value PosCodeCode
        private const string Xml_Input_PosCode_TagName = "PosCode";
        // Value ComCode
        private const string Xml_Input_ComCode_TagName = "ComCode";
        // Value PerCode
        private const string Xml_Input_PerCode_TagName = "PerCode";
        // Value Service
        private const string Xml_Input_Service_TagName = "Service";

        public UserPaymentTypeReader()
        {
            // Initialisation
        }

        public UserPaymentTypeReader(string pos, string comcode, string costCenter, string percode, string service)
        {
            SetPosCode(pos);
            SetComcode(comcode);
            SetCostCenter(costCenter);
            SetPerCode(percode);
            SetService(service);
        }

        /// <summary>
        /// Extraction des arguments depuis l'entrée XML
        /// </summary>
        public void ParseInput(string value)
        {
            // On affecte la valeur à lire
            SetInputString(Util.HtmlEncode(value));

            XmlDocument doc = null;

            try
            {
                doc = new XmlDocument();
                //The XmlResolver property is set to null. External resources are not resolved.
                doc.XmlResolver = null;
                // On charge la réponse
                doc.Load(new StringReader(GetInputStream()));

                // Get argument poscode
                SetPosCode(doc.GetElementsByTagName(Xml_Input_PosCode_TagName)[0].InnerXml);

                // Get argument percode
                SetPerCode(doc.GetElementsByTagName(Xml_Input_PerCode_TagName)[0].InnerXml);

                // Set empty cost center
                SetCostCenter(string.Empty);
                
                try
                {
                    // Get language
                    SetLanguage(doc.GetElementsByTagName(Xml_Input_Language_TagName)[0].InnerXml);
                    if (!String.IsNullOrEmpty(GetLanguage())) SetLanguage(GetLanguage().ToLower());

                    // Get argument comcode
                    SetComcode(doc.GetElementsByTagName(Xml_Input_ComCode_TagName)[0].InnerXml);

                    // Get argument service
                    SetService(doc.GetElementsByTagName(Xml_Input_Service_TagName)[0].InnerXml);
                    if (!String.IsNullOrEmpty(GetService()))
                    {
                        // Elimination des caractères spéciaux
                        SetService(Util.HtmlDecode(GetService()));
                    }
                }
                catch (Exception) { }
            }
            catch (Exception e)
            {
                // On ignore cette erreur
                SetPerCode(null);
                throw new Exception(e.Message);
            }
            
        }

        /// <summary>
        /// Affectation de la valeur envoyee
        /// </summary>
        /// <param name="value">Valeur</param>
        public void SetInputString(string value)
        {
            this.inputString = value;
        }


        /// <summary>
        /// Retourne la valeur envoyee
        /// </summary>
        /// <returns>Valeur envoyee</returns>
        public string GetInputStream()
        {
            return this.inputString;
        }

       /// <summary>
       /// Retourne le code de la companie
       /// </summary>
       /// <returns>Code companie</returns>
       public string GetComCode()
       {
           return this.comcode;
       }

       /// <summary>
       /// Returns Cost Center
       /// </summary>
       /// <returns>Cost Center</returns>
       public string GetCostCenter()
       {
           return this.costCenter;
       }

        /// <summary>
        /// Affectation du comcode
        /// </summary>
        /// <param name="value">comcode</param>
       private void SetComcode(string value)
       {
           this.comcode = value;
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
       /// return market
       /// </summary>
       /// <returns>Market</returns>
       public string GetPosCode()
       {
           return this.poscode;
       }

       /// <summary>
       /// Affectation du pos
       /// </summary>
       /// <param name="value">percode</param>
       private void SetPosCode(string value)
       {
           this.poscode = value;
       }

       /// <summary>
       /// Affectation du percode
       /// </summary>
       /// <param name="value">percode</param>
       private void SetPerCode(string value)
       {
           this.percode = value;
       }

       /// <summary>
       /// Affectation du Centre de cout
       /// </summary>
       /// <param name="value">cc1</param>
       private void SetCostCenter(string value)
       {
           this.costCenter = value;
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
       /// Affectation du service
       /// </summary>
       /// <param name="value">comcode</param>
       private void SetService(string value)
       {
           this.service = value;
       }

       /// <summary>
       /// Retourne la langue
       /// </summary>
       /// <returns>Langue</returns>
       public string GetLanguage()
       {
           return this.language;
       }

       /// <summary>
       /// Affectation de la langue
       /// </summary>
       /// <param name="value">comcode</param>
       public void SetLanguage(string value)
       {
           this.language = value;
       }
       
    }
}
