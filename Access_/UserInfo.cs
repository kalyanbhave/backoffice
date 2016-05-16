using System;
using System.IO;

namespace SafeNetWS
{
    /*
     * Cette classe permet de vérifier les droits d'exécution des méthodes
     * par les users
     * Date : 22 septembre 2009
     * Auteur : Samatar
     * 
     */

    public class UserInfo
    {
        private string login;
        private string password;

        private DateTime loginDate;

        public UserInfo(string login, string password)
        {
            this.login = login;
            this.password = password;
            this.loginDate = DateTime.Now;
        }

        public string getLogin()
        {
            return this.login;
        }

        public bool canInsertCard()
        {
            return true;
        }

        public bool canSeeCard()
        {
            return true;
        }

    }
}