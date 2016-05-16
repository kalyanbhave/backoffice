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
using System.IO;
using System.Web;
using System.Configuration;
using SafeNetWS.utils;


namespace SafeNetWS.log
{
    /// <summary>
    /// Log files system
    /// log file is rotated once a day
    /// </summary>
    public class Filelog : IDisposable
    {
        // This is the folder for log files
        public static string LogFolder = ConfigurationManager.AppSettings["LogFilesFolder"];
        // This is the log files extension
        private const string File_Extension = "log";
        // This is pattern used in log filename
        private const string Date_Pattern = "{0:dd.MM.yyyy HH:mm:ss} - {1}";

        private StreamWriter LOG;
        private string Filename;

        public Filelog()
        {
            SetFileName();
            LOG = new StreamWriter(File.Open(GetFileName(), FileMode.Append, FileAccess.Write, FileShare.ReadWrite), System.Text.Encoding.Default);
            LOG.AutoFlush = true;
        }

        /// <summary>
        /// Affectation le complet du fichier de trace
        /// </summary>
        private void SetFileName()
        {
            this.Filename = Util.BuildFileName(@LogFolder,
                string.Format("log_{0}.{1}",DateTime.Now.ToString(Const.DateFormat_ddMMyyyy), File_Extension));
        }

        /// <summary>
        /// Retourne le complet du fichier de trace
        /// </summary>
        /// <returns></returns>
        private String GetFileName()
        {
            return this.Filename;
        }

        /// <summary>
        /// Retourne l'indicateur d'activation
        /// de l'écriture dans un fichier de trace
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public static bool IsUseLogFile()
        {
            return (Util.IsOptionOn("UseLogFiles"));
        }

        /// <summary>
        /// Retourne TRUE si le fichier de trace est ouvert
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        private bool IsOpen()
        {
            return (LOG != null && LOG.BaseStream != null);
        }
        

        /// <summary>
        /// Ecriture d'un retour chariot et saut de ligne 
        /// dans le fichier de trace
        /// </summary>
        public void WriteLine()
        {
            Write(Const.CRLF, false);
        }

        /// <summary>
        /// Ecriture d'une information dans le fichier de trace
        /// </summary>
        /// <param name="line">Line à écrire</param>
        public void WriteLine(string line)
        {
            Write(line + Const.CRLF, true);
        }

        /// <summary>
        /// Retourne TRUE si le fichier de trace est ouvert
        /// en ecriture
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        private bool CanWrite()
        {
            return (IsOpen() && LOG.BaseStream.CanWrite);
        }

        /// <summary>
        /// Ecriture d'une information dans le fichier de trace
        /// </summary>
        /// <param name="line">Line à écrire</param>
        /// <param name="addDate">Ajouter la date avant d'écrire a ligne</param>
        private void Write(string line, bool addDate)
        {
            if (CanWrite())
            {
                try
                {
                    if (addDate)
                        LOG.Write(String.Format(Date_Pattern, DateTime.Now, line));
                    else
                        LOG.Write(line);
                }
                catch (Exception)
                {
                    // On ignore les exceptions
                }
            }
        }

        /// <summary>
        /// Retourne la date de création du fichier
        /// </summary>
        /// <returns>Date de création</returns>
        public DateTime GetCreationDate()
        {
            return File.GetCreationTime(GetFileName());
        }


    #region IDisposable implementation
 
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
 
    private bool m_Disposed = false;
 
    protected virtual void Dispose(bool disposing)
    {
        if (!m_Disposed)
        {
            if (disposing)
            {
                LOG.Dispose();
            }
 
            // Unmanaged resources are released here.
 
            m_Disposed = true;
        }
    }

    ~Filelog()    
    {        
        Dispose(false);
    }
 
    #endregion


    }

}