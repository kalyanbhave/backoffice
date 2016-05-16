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
using System.Threading;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using Ingrian.Security.Cryptography;
using SafeNetWS.login;
using SafeNetWS.utils.crypting;
using SafeNetWS.utils;
using SafeNetWS.log;

using System.Reflection;
using System.Diagnostics;
using System.DirectoryServices;
using System.Collections.Generic;


namespace SafeNetWS
{
    public class Global : System.Web.HttpApplication
    {

        // Ecriture de la trace dans fichier
        // On ouvre le fichier et on le maintient ouvert
        private static Filelog LogFile;

        // Chaines de connexion
        // Navision Settings database
        private static string ConnStringNavSettings;

        // Base des cartes encryptées BO
        private static string ConnStringEncr;

        // Base des cartes encryptées FO
        private static string ConnStringEncrFO;

        // Base des traces cartes rejetées
        private static string ConnStringRejectedCCLog;

        // Base des traces demandes vCard
        private static string ConnStringVCardLog;

        // Entrée LDAP anonymous
        private static DirectoryEntry LDAPDirectoryEntry;

        // Navision  connections container
        private static Dictionary<string, string> NavConns;


        /// <summary>
        /// Returns Navision connection string
        /// for a specific pos
        /// </summary>
        /// <param name="pos">point of sale</param>
        /// <returns></returns>
        public static string GetConnStringNav(string pos)
        {
            // First get the connections container
            // from the cache
            Dictionary<string, string> d= MyThread.VolatileRead(ref NavConns);
            if (d == null)
            {
                // The container is missing
                return null;
            }
            // We have a container
            // let's return the connection for this pos
            return d[pos];
        }

        /// <summary>
        /// Set connection Navision string
        /// </summary>
        /// <param name="pos">point of sale</param>
        /// <param name="value">connection string</param>
        public static void SetConnStringNav(string pos, string value)
        {
            // Get the connections container
            Dictionary<string, string> d = MyThread.VolatileRead(ref NavConns);
            if (d == null)
            {
                d = new Dictionary<string, string>();
            }
            if (d.ContainsKey(pos))
            {
                // remove key
                d.Remove(pos);
            }
            d.Add(pos, value);

            Interlocked.Exchange(ref NavConns, d);
        }

        /// <summary>
        /// Retourne la valeur
        /// chaine de connexion
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        public static string GetConnStringNavSettings()
        {
            return MyThread.VolatileRead(ref ConnStringNavSettings);
        }

        /// <summary>
        /// Affectation de la chaine de connexion
        /// </summary>
        /// <param name="value">Chaine de connexion</param>
        public static void SetConnStringNavSettings(string value)
        {
            Interlocked.Exchange(ref ConnStringNavSettings, value);
        }




        /// <summary>
        /// Retourne la valeur
        /// chaine de connexion
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        public static string GetConnStrinEncr()
        {
            return MyThread.VolatileRead(ref ConnStringEncr);
        }

        /// <summary>
        /// Affectation de la chaine de connexion
        /// </summary>
        /// <param name="value">Chaine de connexion</param>
        public static void SetConnStringEncr(string value)
        {
            Interlocked.Exchange(ref ConnStringEncr, value);
        }

        /// <summary>
        /// Retourne la valeur
        /// chaine de connexion
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        public static string GetConnStrinEncrFo()
        {
            return MyThread.VolatileRead(ref ConnStringEncrFO);
        }

        /// <summary>
        /// Affectation de la chaine de connexion
        /// </summary>
        /// <param name="value">Chaine de connexion</param>
        public static void SetConnStringEncrFo(string value)
        {
            Interlocked.Exchange(ref ConnStringEncrFO, value);
        }

        /// <summary>
        /// Retourne la valeur
        /// chaine de connexion
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        public static string GetConnStringRejectedCCLog()
        {
            return MyThread.VolatileRead(ref ConnStringRejectedCCLog);
        }

                /// <summary>
        /// Retourne la valeur
        /// chaine de connexion
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        public static string GetConnStringVCardLog()
        {
            return MyThread.VolatileRead(ref ConnStringVCardLog);
        }

        

        /// <summary>
        /// Affectation de la chaine de connexion
        /// </summary>
        /// <param name="value">Chaine de connexion</param>
        public static void SetConnStringRejectedCCLog(string value)
        {
            Interlocked.Exchange(ref ConnStringRejectedCCLog, value);
        }
              /// <summary>
        /// Affectation de la chaine de connexion
        /// </summary>
        /// <param name="value">Chaine de connexion</param>
        public static void SetConnStringVCardLog(string value)
        {
            Interlocked.Exchange(ref ConnStringVCardLog, value);
        }

        

        /// <summary>
        /// Instantiation d'un objet
        /// FileLog pour l'ecriture
        /// de la trace dans un fichier
        /// </summary>
        public static void InitFileLog()
        {
            if (Filelog.IsUseLogFile())
            {
                // On va ouvrir une référence vers le fichier
                // et la maintenir ouverte
                // Si un fichier est déjà ouvert
                // alors on le ferme
                if (LogFile != null) LogFile.Dispose();
                // et on ouvre un nouveau
                LogFile = new Filelog();

                //Remove old log files if needed
                //Logger.DeleteOldFiles();
            }
        }

        /// <summary>
        /// Retourne l'instance de FileLog
        /// </summary>
        /// <returns>FileLog</returns>
        public static Filelog GetFileLog()
        {
            return MyThread.VolatileRead(ref LogFile);
        }

        
        /// <summary>
        /// Retourne la connexion LDAP anonymous
        /// </summary>
        /// <returns>Connexion LDAP</returns>
        public static DirectoryEntry GetLDAPDirectoryEntry()
        {
            return MyThread.VolatileRead(ref LDAPDirectoryEntry);
        }

        /// <summary>
        /// Affectation de la connexion LDAP
        /// </summary>
        /// <param name="value">Connexion LDAP</param>
        public static void SetLDAPDirectoryEntry(DirectoryEntry value)
        {
            Interlocked.Exchange(ref LDAPDirectoryEntry, value);
        }



    }
}