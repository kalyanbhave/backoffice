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
using System.Data.SqlClient;
using System.Configuration;
using SafeNetWS.utils;
using SafeNetWS.messages;
using SafeNetWS.database.row;
using SafeNetWS.login;
using SafeNetWS.utils.crypting;
using SafeNetWS.database.result;
using SafeNetWS.creditcard.creditcardvalidator;
using SafeNetWS.creditcard.creditcardvalidator.bibit;
using SafeNetWS.utils.cache;
using SafeNetWS.business.arguments.reader;

namespace SafeNetWS.database

{
    /// <summary>
    /// Cette classe définie une connection SQL à la base de données
    /// encryptées hébergée par FrontOffice
    /// Date : 26 mars 2010
    /// Author : Samatar
    ///</summary>


    public class EncryptedFODataConnection
    {

        // Connexion SQL
        private SqlConnection myConn;

        // Compte utilisateur qui appel le service
        private UserInfo user;

        /// <summary>
        /// Définition d'une nouvelle connexion
        /// </summary>
        /// <param name="useri">Comte utilisateur</param>
        public EncryptedFODataConnection(UserInfo useri)
        {
            // On garde le compte utilisateur qui a appelé
            // afin de ne pas perdre sa langue
            SetUser(useri);

            // On définit une nouvelle connexion
            SetConnection(new SqlConnection(GetConnString()));
        }

        /// <summary>
        /// Retourne la chaine de connexion
        /// à la base de données
        /// </summary>
        /// <returns>Chaine de connexion</returns>
        private string GetConnString()
        {
            // On recherche la chaine de connexion
            // dans le cache
            string connString = Global.GetConnStrinEncrFo();
            if (connString != null)
            {
                // La chaine de connexion est en cache
                return connString;
            }

            // On construit la chaine de connexion
            connString = Util.BuildSQLConnectionString(EncDec.DecryptPassword(ConfigurationManager.ConnectionStrings["EncryptedFODataConnectionString"].ConnectionString),
                ConfigurationManager.AppSettings["EncryptedDataConnectionMaxPoolSize"],
                ConfigurationManager.AppSettings["EncryptedDataConnectionMinPoolSize"]);

            // On sauvegarde la chaine de connexion
            Global.SetConnStringEncrFo(connString);

            return connString;
        }

        /// <summary>
        /// Ouverture d'une connection vers la
        /// base de données contenant les données encryptées
        /// </summary>
        public void Open()
        {
            try
            {
                this.myConn.Open();
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.ErrorConnecting", GetConnection().Database, e.Message, true));
            }
        }



        /// <summary>
        /// Fermeture de la connection vers la
        /// base de données contenant les données encryptées
        /// Si un pool de connexions est utilisé, la connexion
        /// sera rendue dans le pool
        /// </summary>
        public void Close()
        {
            if (GetConnection() != null)
            {
                // Fermeture et libération des ressources
                try
                {
                    this.myConn.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(GetMessages().GetString("EncryptedDB.ErrorClosingConnection", e.Message, true));
                }
            }

        }


        /// <summary>
        /// Récupération du numéro de carte encrypté
        /// et de la carte d'expiration
        /// à partir du token
        /// </summary>
        /// <param name="token">identifiant l'enregistrement unique dans la table</param>
        /// <returns>FOEncryptedPanInfoResult (encyptedPAN et ExpirationDate)</returns>
        public FOEncryptedPanInfoResult GetEncryptedPANAndExpirationDate(string token)
        {
            FOEncryptedPanInfoResult retval = new FOEncryptedPanInfoResult();
            // On prépare la requête
            string request = "SELECT EncryptedPAN, ExpirationDate FROM Cards (NOLOCK) WHERE Token= @token";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@token", SqlDbType.VarChar, 50);
                command.Parameters["@token"].Value = token;

                dr = command.ExecuteReader();

                if (dr.Read())
                {
                    // On a trouvé une ligne pour le token
                    // on récupère le cryptogramme et la date d'expiration
                    retval.SetValues(dr["EncryptedPAN"].ToString(), Util.GetSQLDataTime(dr, "ExpirationDate"));
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetEncryptedPAN.ErrorGettingPAN", token, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;

        }


        /// <summary>
        /// Récupération du token identifiant l'enregistrement
        /// unique dans la table à partir du numéro de carte encrypté
        /// </summary>
        /// <param name="encryptedPan">numéro de carte encrypté</param>
        /// <returns>token et date d'expiration</returns>
        public FOTokenResult GetToken(string encryptedPan)
        {
            FOTokenResult retval = new FOTokenResult();

            string request = "SELECT Token, ExpirationDate FROM Cards (NOLOCK) WHERE EncryptedPAN = @encryptedPAN";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                command.Parameters["@encryptedPAN"].Value = encryptedPan;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    // On a trouvé un token pour le cryptogramme
                    retval.SetValues(dr["Token"].ToString(), Util.GetSQLDataTime(dr, "ExpirationDate"));
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetToken.ErrorGettingToken", e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;

        }

        /// <summary>
        /// Mise à jour de la date d'expiration
        /// dans la base des cartes encryptées par 
        /// le FrontOffice
        /// </summary>
        /// <param name="token">Token (+ nouvelle date d'expiration)</param>
        public void UpdateEncryptedCard(FOTokenResult token)
        {
            string request = "UPDATE Cards SET ExpirationDate = @expirationDate WHERE Token = @token";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@expirationDate", SqlDbType.DateTime);
                command.Parameters.Add("@token", SqlDbType.VarChar, 50);
                // Affectation des variables bindées
                command.Parameters["@expirationDate"].Value = token.GetExpirationDate();
                command.Parameters["@token"].Value = token.GetToken();
                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("UpdateEncryptedCard.UpdateEncryptedCard.Error", token.GetToken(), e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Suppression d'une carte dans la base des cartes encryptées
        /// hébergées par FrontOffice
        /// </summary>
        /// <param name="token">Token de la carte à supprimer</param>
        public void DeleteCard(string token)
        {
            string request = "DELETE FROM Cards WHERE Token=@token";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@token", SqlDbType.VarChar, 50);
                // Affectation des variables bindées
                command.Parameters["@token"].Value = token;

                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("EncryptedFODB.DeleteCard.Error", token, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }
        /// <summary>
        /// Récupération de tous les tokens depuis la table
        /// dans la base des données encryptées côté FrontOffice
        /// Il faut récupérer tous les tokens
        /// qui reste dans cette base
        /// </summary>
        /// <returns>Tableaux de cartes</returns>
        public FORemainingEncryptedData GetAllRemainingTokens()
        {
            // On prépare le conteneur des enregistrements à retourner
            FORemainingEncryptedData retval = new FORemainingEncryptedData();

            string request = "SELECT Token, EncryptedPAN, ExpirationDate FROM Cards (NOLOCK)";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            command.CommandType = CommandType.Text;

            SqlDataReader dr = null;

            try
            {
                dr = command.ExecuteReader();

                while (dr.Read())
                {
                    // La table contient des cartes
                    // On va récupérer les cartes
                    // les unes après les autres
                    retval.AddData(dr["Token"].ToString(), dr["EncryptedData"].ToString(),
                        Util.GetSQLDataTime(dr, "ExpirationDate"));
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetAllTokens.Error", e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;

        }

        /// <summary>
        /// Insertion du statut de la réponse de RBS
        /// </summary>
        /// <param name="encryptedPan">Cryptogramme du PAN</param>
        public void InsertBibitResponseStatus(string encryptedPan)
        {
            string request = "INSERT INTO db_bibitf.BibitCachef (EncryptedPAN,LastAccessTime) VALUES (@encryptedPan, @lastAccessTime)";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // On prépare l'instruction en passant les variables
                command.Parameters.Add("@encryptedPan", SqlDbType.VarChar, 48);
                command.Parameters.Add("@lastAccessTime", SqlDbType.DateTime);

                command.Parameters["@encryptedPan"].Value = encryptedPan;
                command.Parameters["@lastAccessTime"].Value = DateTime.Now;

                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.CacheBibitResponseStatus.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Mise à jour du statut de la réponse de RBS
        /// </summary>
        /// <param name="encryptedPan">Cryptogramme du PAN</param>
        public void UpdateBibitResponseStatus(string encryptedPan)
        {
            string request = "UPDATE db_bibitf.BibitCachef SET LastAccessTime= @lastAccessTime WHERE EncryptedPAN= @encryptedPan";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // On prépare l'instruction en passant les variables
                command.Parameters.Add("@lastAccessTime", SqlDbType.DateTime);
                command.Parameters.Add("@encryptedPan", SqlDbType.VarChar, 48);

                command.Parameters["@lastAccessTime"].Value = DateTime.Now;
                command.Parameters["@encryptedPan"].Value = encryptedPan;

                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.UpdateBibitResponseStatus.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }


        /// <summary>
        /// Récupération du statut de la dernière validation
        /// d'une carte via le service RBS
        /// </summary>
        /// <param name="encryptedPan">Cryptogramme du PAN</param>
        /// <returns>Statut</returns>
        public CachedValidationResult GetCachedBibitResponseStatus(string encryptedPan)
        {
            CachedValidationResult retval = new CachedValidationResult();

            string request = "SELECT LastAccessTime AS lastAccessTime FROM db_bibitf.BibitCachef (NOLOCK) WHERE EncryptedPAN = @encryptedPan";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@encryptedPan", SqlDbType.VarChar, 48);
                command.Parameters["@encryptedPan"].Value = encryptedPan;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    // On a trouvé une entrée
                    DateTime lastCheckTime = Util.GetSQLDataTime(dr, "lastAccessTime");
                    if (Util.DateDiffInDays(lastCheckTime, DateTime.Now) <= 1)
                    {
                        // La carte a été testé (et accepté) il y a moin de 24h
                        retval.SetStatus(CachedValidationResult.CacheStatus.FoundValid);
                        retval.SetLastAccessTime(lastCheckTime);
                    }
                    else
                    {
                        // Ce test a expiré
                        retval.SetStatus(CachedValidationResult.CacheStatus.FoundExpired);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetCachedBibitResponseStatus.Error", e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;
        }

        /// <summary>
        /// Retire toutes les entrées du cache BIBIT
        /// </summary>
        /// <returns>Nombre d'entrées retirées du cache</returns>
        public int ClearBibitCache()
        {
            int retval = 0;
            string request = "DELETE FROM db_bibitf.BibitCachef";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Exécution de la requête
                retval = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.ClearBibitCache.Error", e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
            return retval;
        }

        /// <summary>
        /// Récupération du token identifiant l'enregistrement
        /// unique dans la table à partir du numéro de carte Egencia encrypté
        /// </summary>
        /// <param name="encryptedPan">numéro de carte encrypté</param>
        /// <returns>token</returns>
        public EgenciaCardTokenResult GetEgenciaCardToken(string encryptedPan)
        {
            EgenciaCardTokenResult retval = new EgenciaCardTokenResult();

            // On prépare la requête d'extraction
            string request = "SELECT Token FROM EgenciaCards (NOLOCK) WHERE EncryptedPAN=@encryptedPAN";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                command.Parameters["@encryptedPAN"].Value = encryptedPan;

                dr = command.ExecuteReader();

                // Normalement une seule ligne sera renvoyée
                if (dr.Read())
                {
                    // On a trouvé un token pour le cryptogramme
                    retval.SetToken(dr["token"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetToken.ErrorGettingToken", e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;

        }


        /// <summary>
        /// Insertion d'une carte encryptée
        /// dans la base des cartes encryptés
        /// </summary>
        /// <param name="encryptedPan">numéro de carte encrypté</param>
        /// <param name="expirationDate">Date d'expiration de la carte</param>
        /// <returns>token de la valeur insérée</returns>
        public string InsertEncryptedCard(string encryptedPan, DateTime expirationDate)
        {
            string token = null;
            string request = "INSERT INTO Cards (Token,EncryptedPAN,ExpirationDate,CreationDate,CreationUser) VALUES (@token, @encryptedPAN, @expirationDate,@creationDate, @creationUser)";


            // Initilisation de l'indicateur d'insertion
            int InsertValueStatus = EncryptedDataConnection.ValueNotYetInserted;

            while (InsertValueStatus == EncryptedDataConnection.ValueNotYetInserted)
            {
                // On passe dans cette boucle dés lors que la carte
                // n'a pas été insérée et qu'il n'y a pas d'erreur (autre que la violation de clé)
                // On va donc réessayer à chaque fois avec un nouveau token

                // objet command
                SqlCommand command = new SqlCommand(request, GetConnection());

                // On va générer un token
                token = Util.GenerateFOToken(GetUser());

                try
                {
                    // add parameters
                    command.Parameters.Add("@token", SqlDbType.VarChar, 50);
                    command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                    command.Parameters.Add("@expirationDate", SqlDbType.DateTime);
                    command.Parameters.Add("@creationDate", SqlDbType.DateTime);
                    command.Parameters.Add("@creationUser", SqlDbType.VarChar, 100);

                    command.Parameters["@token"].Value = token;
                    command.Parameters["@encryptedPAN"].Value = encryptedPan;
                    command.Parameters["@expirationDate"].Value = expirationDate;
                    command.Parameters["@creationDate"].Value = DateTime.Now;
                    command.Parameters["@creationUser"].Value = GetLogin();

                    // Exécution de la requête
                    command.ExecuteNonQuery();

                    // Le token avec le cryptogramme a été correctement insérée
                    InsertValueStatus = EncryptedDataConnection.ValueInserted;
                }
                catch (SqlException s)
                {
                    if (s.Number == EncryptedDataConnection.KeyViolationErrorNumber)
                    {
                        // Erreur de violation de clé lors de l'insertion
                        // Cela veut dire que le token généré existe déjà dans la base
                        // On doit être dans une configuration avec au moins 2 serveurs
                        // désynchronisés
                        // Dans ce cas, on va réessayer avec un nouveau token jusqu'à insertion
                    }
                    else
                    {
                        // Une exception a été levée lors de l'insertion
                        InsertValueStatus = EncryptedDataConnection.ValueError;
                        // On retourne l'erreur
                        throw new Exception(GetMessages().GetString("EncryptedDB.InsertEncryptedCard.Error", s.Message, true));
                    }

                }
                catch (Exception e)
                {
                    // Une exception a été levée lors de l'insertion
                    InsertValueStatus = EncryptedDataConnection.ValueError;
                    // On retourne l'erreur
                    throw new Exception(GetMessages().GetString("EncryptedDB.InsertEncryptedCard.Error", e.Message, true));
                }
                finally
                {
                    DisposeCommand(command);
                }
            }
            return token;
        }

        /// <summary>
        /// Insertion de numéro de carte et du CSC Egencia encryptée
        /// dans la base des cartes encryptées par 
        /// le FrontOffice
        /// </summary>
        /// <param name="encryptedPan">Numéro de carte encrypté</param>
        /// <param name="encryptedCSC">CSC encrypté</param>
        public string InsertEgenciaEncryptedCard(string encryptedPan, string encryptedCSC)
        {
            string request = "INSERT INTO EgenciaCards (Token, EncryptedPAN, EncryptedCSC, CreationDate, CreationUser, "
                + "LastUpdateDate, LastUpdateUser, InternalUse) VALUES (@token, @encryptedPAN, @encryptedCSC, @creationDate, @creationUser, @lastUpdateDateTime, @lastUpdateUser, @rotated)";


            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            // On va générer un token
            string token = Util.GenerateEgenciaToken(GetUser());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@token", SqlDbType.VarChar, 50);
                command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                command.Parameters.Add("@encryptedCSC", SqlDbType.VarChar, 48);
                command.Parameters.Add("@creationDate", SqlDbType.DateTime);
                command.Parameters.Add("@creationUser", SqlDbType.VarChar, 100);
                command.Parameters.Add("@lastUpdateDateTime", SqlDbType.DateTime);
                command.Parameters.Add("@lastUpdateUser", SqlDbType.VarChar, 100);
                command.Parameters.Add("@rotated", SqlDbType.Int);
                // Affectation des variables bindées
                command.Parameters["@token"].Value = token;
                command.Parameters["@encryptedPAN"].Value = encryptedPan;
                command.Parameters["@encryptedCSC"].Value = encryptedCSC;
                command.Parameters["@creationDate"].Value = DateTime.Now;
                command.Parameters["@creationUser"].Value = GetLogin();
                command.Parameters["@lastUpdateDateTime"].Value = DateTime.Now;
                command.Parameters["@lastUpdateUser"].Value = GetLogin();
                command.Parameters["@rotated"].Value = 0;
                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("InsertEgenciaEncryptedCard.Error", token, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
            return token;
        }

        /// <summary>
        /// Récupération de tous les tokens Egencia depuis la table
        /// dans la base des données encryptées
        /// Il faut récupérer tous les tokens dont le
        /// cryptogramme n'a pas été encore mis à jour
        /// </summary>
        /// <returns>Tableaux de Tokens</returns>
        public EncryptedEgenciaData GetAllEgenciaTokens()
        {

            EncryptedEgenciaData retval = new EncryptedEgenciaData();

            string request = "SELECT Token, EncryptedPAN, EncryptedCSC FROM EgenciaCards (NOLOCK) WHERE InternalUse =0";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            DataTable DT = new DataTable();
            SqlDataAdapter adapter = null;
            try
            {
                command.CommandType = CommandType.Text;

                adapter = new SqlDataAdapter(command);
                adapter.Fill(DT);
                if (DT.Rows.Count > 0)
                {
                    foreach (DataRow ds in DT.Rows)
                    {
                        retval.AddData(ds["Token"].ToString(), ds["EncryptedPAN"].ToString(), ds["EncryptedCSC"].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetAllTokens.Error", e.Message, true));
            }
            finally
            {
                DisposeSqlDataAdapter(adapter);
                CloseDataTable(DT, command);
            }
            return retval;

        }


        /// <summary>
        /// Mise à jour d'un cryptogramme de cartes Egencia
        /// Ce process est utilisé par la rotation de clés
        /// </summary>
        /// <param name="token">Token à mettre à jour</param>
        /// <param name="param">Cryptogramme PAN (après rotation)</param>
        /// <param name="encryptedCSC">Cryptogramme CSC (après rotation)</param>
        public void UpdateEncryptedEgenciaCard(string token, string encryptedPAN, string encryptedCSC)
        {
            string request = "UPDATE EgenciaCards SET EncryptedPAN = @encryptedPAN, EncryptedCSC = @encryptedCSC, InternalUse = @rotated, "
                + "LastUpdateDate = @lastUpdateDateTime, LastUpdateUser = @lastUpdateUser WHERE token=@token";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@encryptedPAN", SqlDbType.VarChar, 48);
                command.Parameters.Add("@encryptedCSC", SqlDbType.VarChar, 48);
                command.Parameters.Add("@rotated", SqlDbType.Int);
                command.Parameters.Add("@lastUpdateDateTime", SqlDbType.DateTime);
                command.Parameters.Add("@lastUpdateUser", SqlDbType.VarChar, 100);
                command.Parameters.Add("@token", SqlDbType.VarChar, 100);
                // Affectation des variables bindées
                command.Parameters["@encryptedPAN"].Value = encryptedPAN;
                command.Parameters["@encryptedCSC"].Value = encryptedCSC;
                command.Parameters["@rotated"].Value = 1;
                command.Parameters["@lastUpdateDateTime"].Value = DateTime.Now;
                command.Parameters["@lastUpdateUser"].Value = GetLogin();
                command.Parameters["@token"].Value = token;
                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("EncryptedDB.UpdateEncryptedCard.Error", token, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }


        /// <summary>
        /// Mise à jour d'un cryptogramme de cartes Egencia
        /// Ce process est utilisé pour la rotation de clés
        /// Si le process de mise à jour des cartes s'est achevé avec succès
        /// On remet l'indicateur pour tous les Tokens à 0
        /// </summary>
        public void SetRotationEndedForEgenciaCard()
        {
            string request = "UPDATE EgenciaCards SET InternalUse=@rotated WHERE InternalUse=@notrotated";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Préparation des variables bindées
                command.Parameters.Add("@rotated", SqlDbType.Int);
                command.Parameters.Add("@notrotated", SqlDbType.Int);
                // Affectation des variables bindées
                command.Parameters["@rotated"].Value = 0;
                command.Parameters["@notrotated"].Value = 1;

                // Exécution de la requête
                command.ExecuteNonQuery();
                // Tout s'est bien passé
            }
            catch (Exception e)
            {
                // Une exception a été levée lors de l'insertion
                throw new Exception(GetMessages().GetString("EncryptedDB.SetRotationEnded.Error", e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }
        }

        /// <summary>
        /// Retourne les informations carte Egencia depuis un token
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Informations carte</returns>
        public EgenciaEncryptedPanInfoResult GetEgenciaEncryptedPANAndCSC(string token)
        {
            EgenciaEncryptedPanInfoResult retval = new EgenciaEncryptedPanInfoResult();
            // On prépare la requête
            string request = "SELECT EncryptedPAN, EncryptedCSC FROM EgenciaCards (NOLOCK) WHERE Token= @token";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@token", SqlDbType.VarChar, 100);
                command.Parameters["@token"].Value = token;

                dr = command.ExecuteReader();

                if (dr.Read())
                {
                    // On a trouvé une ligne pour le token
                    // on récupère le cryptogramme et la date d'expiration
                    retval.SetValues(dr["EncryptedPAN"].ToString(), dr["EncryptedCSC"].ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.GetEncryptedPAN.ErrorGettingPAN", token, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return retval;
        }


        /// <summary>
        /// Insert ENett ENC credentials to database
        /// </summary>
        /// <param name="ENettECNRequestorAccess">Requestor ECN</param>
        public void InsertENettECNDetails(ENettECNRequestorAccess access)
        {

            string request = string.Format("INSERT INTO ENettENCDetails (RequestorECN, IntegratorCode, "
                + "IntegratorAccessKey, ClientAccessKey, CreationDate, CreationUser) "
                + "VALUES (@requestorECN, @integratorCode, @integratorAccessKey, @clientAccessKey, @creationDate, @creationUser)");

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Prepare statement
                command.Parameters.Add("@requestorECN", SqlDbType.Int);
                command.Parameters.Add("@integratorCode", SqlDbType.VarChar, 50);
                command.Parameters.Add("@integratorAccessKey", SqlDbType.VarChar, 50);
                command.Parameters.Add("@clientAccessKey", SqlDbType.VarChar, 50);
                command.Parameters.Add("@creationDate", SqlDbType.DateTime);
                command.Parameters.Add("@creationUser", SqlDbType.VarChar, 100);

                command.Parameters["@requestorECN"].Value = access.GetRequestorECN();
                command.Parameters["@integratorCode"].Value = access.GetIntegratorCode();
                command.Parameters["@integratorAccessKey"].Value = access.GetIntegratorAccessKey();
                command.Parameters["@clientAccessKey"].Value = access.GetClientAccessKey();
                command.Parameters["@creationDate"].Value = DateTime.Now;
                command.Parameters["@creationUser"].Value = GetLogin();

                // Exécution de la requête
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                // On retourne l'erreur
                throw new Exception(GetMessages().GetString("EncryptedDB.ENettECNSave.Error", access.GetRequestorECN(), e.Message, true));
            }
            finally
            {
                // On libère les ressources
                DisposeCommand(command);
            }
        }

     

        /// <summary>
        /// Check ENett ENC in the database
        /// </summary>
        /// <param name="RequestorECN">Requestor ENC</param>
        /// <returns>TRUE or FALSE</returns>
        public bool CheckENettECN(int RequestorECN)
        {
            string request = string.Format("SELECT IntegratorCode FROM ENettENCDetails WHERE RequestorECN= @requestorECN");

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());
            SqlDataReader dr = null;

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@requestorECN", SqlDbType.Int);
                command.Parameters["@requestorECN"].Value = RequestorECN;

                dr = command.ExecuteReader();

                // Let's check if we have an entry for the ECN
                if (dr.Read())
                {
                    // We have something
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.CheckENettECN.Error", RequestorECN, e.Message, true));
            }
            finally
            {
                CloseDataReader(dr, command);
            }
            return false;
        }

        /// <summary>
        /// Delete Requestor ECN access information
        /// from the database
        /// </summary>
        /// <param name="RequestorECN">Requestor ECN</param>
        public void DeleteENettECNDetails(int RequestorECN)
        {
            string request = "DELETE FROM ENettENCDetails WHERE RequestorECN= @requestorECN";

            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Paramètres (bind variable)
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@requestorECN", SqlDbType.Int);
                command.Parameters["@requestorECN"].Value = RequestorECN;
                
                // Run the statement
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(GetMessages().GetString("EncryptedDB.DeleteENettECN.Error", RequestorECN, e.Message, true));
            }
            finally
            {
                DisposeCommand(command);
            }

        }



        /// Test function Connection
        /// This is a dummy function to check that we can connect to database
        /// </summary>
        /// <exception cref="Exception"/>
        public void Test()
        {
            string request = "SELECT 1";
            // objet command
            SqlCommand command = new SqlCommand(request, GetConnection());

            try
            {
                // Run the statement
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                // Faced exception
                throw new Exception(GetMessages().GetString("CreditCardLogConnection.Test.Error", e.Message, false));
            }
            finally
            {
                DisposeCommand(command);
            }
        }



        /// <summary>
        /// Renvoi du message
        /// correspondant à la langue
        /// de l'utilisateur
        /// </summary>
        /// <returns>Message (langue)</returns>
        private Messages GetMessages()
        {
            return GetUser().GetMessages();
        }


        /// <summary>
        /// Retourne la connexion SQL
        /// courante
        /// </summary>
        /// <returns>Connexion SQL</returns>
        private SqlConnection GetConnection()
        {
            return this.myConn;
        }

        /// <summary>
        /// Mise à jour de la connexion SQL
        /// </summary>
        /// <param name="conn">Connexion SQL</param>
        private void SetConnection(SqlConnection conn)
        {
            this.myConn = conn;
        }
        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        private UserInfo GetUser()
        {
            return this.user;
        }

        /// <summary>
        /// Mise à jour du compte utilisateur
        /// </summary>
        /// <param name="useri">Compte utilisateur</param>
        private void SetUser(UserInfo useri)
        {
            this.user = useri;
        }

        /// <summary>
        /// Fermeture du DataReader et SQLCommand
        /// et libération des ressources
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <param name="command">SQLCommand</param>
        private void CloseDataReader(SqlDataReader dr, SqlCommand command)
        {
            if (dr != null)
            {
                dr.Dispose();
            }
            DisposeCommand(command);
        }
        /// <summary>
        /// Fermeture du DataTable et SQLCommand
        /// et libération des ressources
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="command">SQLCommand</param>
        private void CloseDataTable(DataTable dt, SqlCommand command)
        {
            if (dt != null)
            {
                dt.Dispose();
            }
            DisposeCommand(command);
        }

        /// <summary>
        /// Libération des ressources SqlCommand
        /// </summary>
        /// <param name="command">SqlCommand</param>
        private void DisposeCommand(SqlCommand command)
        {
            if (command != null) command.Dispose();
        }
        /// <summary>
        /// Libération des ressources SqlDataAdapter
        /// </summary>
        /// <param name="adap">SqlDataAdapter</param>
        private void DisposeSqlDataAdapter(SqlDataAdapter adap)
        {
            if (adap != null) adap.Dispose();
        }

        /// <summary>
        /// Retourne le compte utilisateur
        /// </summary>
        /// <returns>Compte utilisateur</returns>
        private string GetLogin()
        {
            return GetUser().GetLogin();
        }
    }

}