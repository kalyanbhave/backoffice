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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Resources;
using System.Web;
using SafeNetWS.utils;
using SafeNetWS.exception;

namespace SafeNetWS.messages
{
    /// <summary>
    /// Cette classe permet de charger les entrées
    /// de traduction
    /// </summary>
    public class Messages
    {
        private string Lang;
        private Dictionary<string, string> dictionary;


        public Messages(string lang)
        {
            SetLang(lang);

            //Initialisation du tableau qui va contenir
            // les entrées clé/valeur correspondant aux valeurs tranduites
            this.dictionary = new Dictionary<string, string>();

            InitResourceFile();
        }

        /// <summary>
        /// Chargement du fichier correspond à la bonne langue
        /// sélectionné par le client
        /// </summary>
        protected void InitResourceFile()
        {
            // On récupère la langue que le client souhaite
            string mylang = Util.CorrectLang(GetLang());
            // On pointe vers le bon fichier de traduction
            string fileName=HttpContext.Current.Server.MapPath(String.Format("messages/messages_{0}.properties", mylang));
            if (!Util.FileExists(fileName))
            {
                // le fichier pour la local est introuvable
                // On va passer en anglais (language de référence)
                SetLang(Const.LangEN);
                fileName = HttpContext.Current.Server.MapPath("messages/messages_en_US.properties");
            }

            // On lit le fichier properties et on charge
            // les entrées pour la locale désirée
            foreach (string line in File.ReadAllLines(fileName, Encoding.Default)) 
            { 
                if ((!string.IsNullOrEmpty(line)) && (!line.StartsWith(";")) && (!line.StartsWith("#")) && (!line.StartsWith("'")) && (line.Contains("="))) 
                { 
                    int index = line.IndexOf('='); 
                    string key = line.Substring(0, index).Trim(); 
                    string value = line.Substring(index + 1).Trim(); 
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'"))) 
                    { 
                        value = value.Substring(1, value.Length - 2); 
                    } 
                    dictionary.Add(key, value); 
                } 
            }
        }

        
        public string GetString(string resKey, bool exception)
        {
            if (exception)
            {
                return CCEExceptionUtil.GetEnhancedMessage(resKey, dictionary[resKey], dictionary[resKey]);
            }
            else
            {
                return dictionary[resKey];
            }
        }

        public string GetString(string resKey, string arg, bool exception)
        {
            if (exception)
            {
                return CCEExceptionUtil.GetEnhancedMessage(resKey, arg, String.Format(dictionary[resKey], new String[] { arg }));
            }
            else
            {
                return String.Format(dictionary[resKey], new String[] { arg });
            }
        }
        public string GetString(string resKey, int arg, bool exception)
        {
            return GetString(resKey, arg.ToString(), exception);
        }
        public string GetString(string resKey, string arg1, string arg2, bool exception)
        {
            if (exception)
            {
                return CCEExceptionUtil.GetEnhancedMessage(resKey, arg2, String.Format(dictionary[resKey], new String[] { arg1, arg2 }));
            }
            else
            {
                return String.Format(dictionary[resKey], new String[] { arg1, arg2 });
            }
        }
        public string GetString(string resKey, string arg1, string arg2, string arg3, bool exception)
        {
            if (exception)
            {
                return CCEExceptionUtil.GetEnhancedMessage(resKey, arg3, String.Format(dictionary[resKey], new String[] { arg1, arg2, arg3 }));
            }
            else
            {
                return String.Format(dictionary[resKey], new String[] { arg1, arg2, arg3});
            }
        }
        public string GetString(string resKey, long arg1, string arg2, string arg3, bool exception)
        {
            return GetString(resKey, arg1.ToString(), arg2, arg3, exception);
        }
        public string GetString(string resKey, long arg1, string arg2, string arg3, string arg4, bool exception)
        {
            return GetString(resKey, arg1.ToString(), arg2, arg3, arg4, exception);
        }
        public string GetString(string resKey, string arg1, string arg2, string arg3, string arg4, Boolean exception)
        {
            if (exception)
            {
                return CCEExceptionUtil.GetEnhancedMessage(resKey, arg4, String.Format(dictionary[resKey], new String[] { arg1, arg2, arg3, arg4 }));
            }
            else
            {
                return String.Format(dictionary[resKey], new String[] { arg1, arg2, arg3, arg4 });
            }
        }
        public string GetString(string resKey, string arg1, string arg2, string arg3, string arg4, string arg5, bool exception)
        {
            if (exception)
            {
                return CCEExceptionUtil.GetEnhancedMessage(resKey, arg5, String.Format(dictionary[resKey], new String[] { arg1, arg2, arg3, arg4, arg5 }));
            }
            else
            {
                return String.Format(dictionary[resKey], new String[] { arg1, arg2, arg3, arg4, arg5 });
            }
        }
        public string GetString(string resKey, string arg1, int arg2, bool exception)
        {
            return GetString(resKey, arg1, arg2.ToString(), exception);
        }
        public string GetString(string resKey, long arg1, string arg2, Exception arg3, bool exception)
        {
            return GetString(resKey, arg1.ToString(), arg2, arg3.Message, exception);
        }
        public string GetString(string resKey, string arg1, string arg2, Exception arg3, bool exception)
        {
            return GetString(resKey, arg1, arg2, arg3.Message, exception);
        }
        public string GetString(string resKey, long arg1, string arg2, bool exception)
        {
            return GetString(resKey, arg1.ToString(), arg2, exception);
        }
        public string GetString(string resKey, long arg, bool exception)
        {
            return GetString(resKey, arg.ToString(), exception);
        }

        /// <summary>
        /// Retour de la langue
        /// </summary>
        /// <returns>Langue</returns>
        public string GetLang()
        {
            return this.Lang;
        }

        /// <summary>
        /// Affectation de la langue
        /// </summary>
        /// <param name="lang">Langue</param>
        public void SetLang(string lang)
        {
            this.Lang = lang;
        }

    }
}
