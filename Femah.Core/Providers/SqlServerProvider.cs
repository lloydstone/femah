using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Femah.Core.FeatureSwitchTypes;

namespace Femah.Core.Providers
{
    /// <summary>
    /// A feature switch provider that stores feature switches in a SQL Server database.
    /// TODO: Autogenerate table when it doesn't exist.
    /// TODO: Allow consumer to override table & column names.
    /// Current required schema is: 
    /// CREATE TABLE [dbo].[femahSwitches](
    /// 	[id] [int] IDENTITY(1,1) NOT NULL,
    /// 	[name] [varchar](255) NOT NULL,
    /// 	[isEnabled] [bit] NOT NULL,
    /// 	[switchXml] [xml] NULL,
    /// 	[assemblyName] [varchar](512) NULL,
    /// 	[typeName] [varchar](128) NULL,
    /// CONSTRAINT [PK_femahSwitches] PRIMARY KEY CLUSTERED 
    /// (
    /// 	[id] ASC
    /// ) WITH (
    ///     PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
    /// ) ON [PRIMARY]
    /// </summary>
    public class SqlServerProvider : IFeatureSwitchProvider
    {
        private const string _tableName = "femahSwitches";
        static List<Type> _featureSwitchtypes = null;

        public string ConnectionString { get; private set; }

        /// <summary>
        /// Configure the SqlServerProvider.
        /// </summary>
        /// <param name="connString">The connection string to use to connect to the SQL Server.</param>
        public void Configure(string connString)
        {
            ConnectionString = connString;
        }

        #region IFeatureSwitchProvider Implementation

        /// <summary>
        /// Initialise the provider, given the names of the feature switches.
        /// </summary>
        /// <param name="featureSwitches">Names of the feature switches in the application.</param>
        public void Initialise(IEnumerable<string> featureSwitches)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Ensure all listed feature switches exist in the database.
                foreach (var featureSwitchName in featureSwitches)
                {
                    var featureSwitch = this.Get(featureSwitchName);
                    if (featureSwitch == null)
                    {
                        featureSwitch = new SimpleFeatureSwitch { Name = featureSwitchName, IsEnabled = false };
                    }
                    this.SaveOrUpdateSwitch(featureSwitch, conn);
                }

                // Remove any feature switches from database that aren't valid feature switches.
                DeleteUnlistedSwitches(featureSwitches, conn);
            }
        }


        /// <summary>
        /// Get a feature switch.
        /// </summary>
        /// <param name="name">The name of the feature switch to get</param>
        /// <returns>An instance of IFeatureSwitch if found, otherwise null</returns>
        public IFeatureSwitch Get(string name)
        {
            IFeatureSwitch featureSwitch;

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                featureSwitch = this.GetSwitch(name, conn);
            }

            return featureSwitch;
        }

        /// <summary>
        /// Save a feature switch.
        /// </summary>
        /// <param name="featureSwitch">The feature to be saved</param>
        public void Save(IFeatureSwitch featureSwitch)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                this.SaveOrUpdateSwitch(featureSwitch, conn);
            }
        }

        /// <summary>
        /// Return all feature switches.
        /// </summary>
        /// <returns>A list of zero or more instances of IFeatureSwitch</returns>
        public List<IFeatureSwitch> AllFeatureSwitches()
        {
            List<IFeatureSwitch> featureSwitches;

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                featureSwitches = this.GetAllSwitches(conn);
            }

            return featureSwitches;
        }

        public List<Type> AllFeatureSwitchTypes()
        {
            return _featureSwitchtypes;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get all feature switches from the datbase.
        /// </summary>
        /// <param name="conn">An open connection to the database.</param>
        /// <returns>A list of feature switches</returns>
        protected List<IFeatureSwitch> GetAllSwitches(SqlConnection conn)
        {
            var cmd = new SqlCommand(this.SelectAllSwitchesSql, conn);

            var reader = cmd.ExecuteReader();
            return this.ReadAllSwitches(reader);
        }


        /// <summary>
        /// Get a particular feature switch from the database.
        /// </summary>
        /// <param name="name">The name of the feature switch to get</param>
        /// <param name="conn">An open connection to the dataase</param>
        /// <returns>The feature switch if found, or null if not found</returns>
        protected IFeatureSwitch GetSwitch(string name, SqlConnection conn)
        {
            IFeatureSwitch featureSwitch;

            var cmd = new SqlCommand(this.SelectSwitchSql, conn);
            var param = new SqlParameter("@NameParam", SqlDbType.NVarChar);
            param.Value = name;
            cmd.Parameters.Add(param);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    return null;
                }

                reader.Read();
                featureSwitch = ReadSwitch(reader);
            }

            return featureSwitch;
        }

        /// <summary>
        /// Read all feature switches from the provided SQL data reader.
        /// </summary>
        /// <param name="reader">A SqlDataReader from which the feature switches should be read.</param>
        /// <returns>A list of zero or more feature switches.</returns>
        protected List<IFeatureSwitch> ReadAllSwitches(SqlDataReader reader)
        {
            var switches = new List<IFeatureSwitch>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    switches.Add(this.ReadSwitch(reader));
                }
            }

            switches.RemoveAll(s => s == null);

            return switches;
        }

        /// <summary>
        /// Read a single feature switch from an SQL data reader.
        /// </summary>
        /// <param name="reader">The SqlDataReader from which the switch should be read.</param>
        /// <returns>A feature switch.</returns>
        protected IFeatureSwitch ReadSwitch(SqlDataReader reader)
        {
            if (reader["assemblyName"] == DBNull.Value || reader["typeName"] == DBNull.Value)
            {
                return new SimpleFeatureSwitch
                {
                    IsEnabled = (Boolean)(reader["isEnabled"]),
                    Name = reader["name"].ToString()
                };
            }

            // Instantiate an instance of the specified type.
            string assemblyName = reader["assemblyName"].ToString();
            string typeName = reader["typeName"].ToString();
            IFeatureSwitch featureSwitch;
            try
            {
                var switchObj = Activator.CreateInstance(assemblyName, typeName);
                featureSwitch = switchObj.Unwrap() as IFeatureSwitch;
            }
            catch (TypeLoadException)
            {
                // Unable to load type.  Possibly the type in the db no longer exists.
                return null;
            }

            featureSwitch.Name = reader["name"].ToString();
            featureSwitch.IsEnabled = (Boolean)reader["isEnabled"];

            if (reader["switchXml"] != DBNull.Value)
            {
                StringReader xmlStringReader = new StringReader(reader["switchXml"].ToString());
                var xmlReader = XmlReader.Create(xmlStringReader);
                var deserializer = new XmlSerializer(featureSwitch.GetType());
                if (deserializer.CanDeserialize(xmlReader))
                {
                    featureSwitch = (IFeatureSwitch)deserializer.Deserialize(xmlReader);
                }
            }

            return featureSwitch;
        }

        /// <summary>
        /// Save the feature switch to the database.  If the switch already exists in the db, it
        /// is updated, otherwise inserted.
        /// </summary>
        /// <param name="featureSwitch">The feature switch to be written to the database</param>
        /// <param name="conn">An open connection to the database</param>
        protected void SaveOrUpdateSwitch(IFeatureSwitch featureSwitch, SqlConnection conn)
        {
            var nameParam = new SqlParameter("@Param1", SqlDbType.NVarChar);
            nameParam.Value = featureSwitch.Name;
            var cmd = new SqlCommand(this.SwitchCountSql, conn);
            cmd.Parameters.Add(nameParam);

            Int32 rowCount = (Int32) cmd.ExecuteScalar();
            if (rowCount >= 1)
            {
                // Update.
                cmd = new SqlCommand(this.UpdateSwitchSql, conn);
            }
            else
            {
                // Insert.
                cmd = new SqlCommand(this.InsertSwitchSql, conn);
            }

            nameParam = new SqlParameter("@SwitchName", SqlDbType.NVarChar);
            nameParam.Value = featureSwitch.Name;
            cmd.Parameters.Add(nameParam);

            var param = new SqlParameter("@IsEnabled", SqlDbType.Bit);
            param.Value = featureSwitch.IsEnabled;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@AssemblyName", SqlDbType.NVarChar);
            param.Value = featureSwitch.GetType().Assembly.FullName;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@TypeName", SqlDbType.NVarChar);
            param.Value = featureSwitch.GetType().FullName;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@SwitchXml", SqlDbType.Xml);
            XmlSerializer serializer = new XmlSerializer(featureSwitch.GetType());
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            serializer.Serialize(writer, featureSwitch);
            param.Value = sb.ToString();
            cmd.Parameters.Add(param);
                
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete a feature switch from the database.
        /// </summary>
        /// <param name="name">Name of the feature switch to delete.</param>
        /// <param name="conn">An open connection to a SQL Server database.</param>
        protected void DeleteSwitch(string name, SqlConnection conn)
        {
            var cmd = new SqlCommand(this.DeleteSwitchSql, conn);

            var param = new SqlParameter("@SwitchName", SqlDbType.NVarChar);
            param.Value = name;
            cmd.Parameters.Add(param);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete any feature switches from database that aren't specified in a given list.
        /// </summary>
        /// <param name="switchList">The names of feature switches NOT to be deleted.</param>
        /// <param name="conn">An open connection to a SQL Server database.</param>
        private void DeleteUnlistedSwitches(IEnumerable<string> switchList, SqlConnection conn)
        {
            var cmd = new SqlCommand(this.SelectAllSwitchesSql, conn);

            var switchesToDelete = new List<string>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader["name"].ToString();
                    if (!switchList.Contains(name))
                    {
                        switchesToDelete.Add(reader["name"].ToString());
                    }
                }
            }

            foreach (var name in switchesToDelete)
            {
                DeleteSwitch(name, conn);
            }
        }

        #endregion 

        #region SQL Strings

        /// <summary>
        /// SQL statement to select all feature switches in the database.
        /// </summary>
        private string SelectAllSwitchesSql
        {
            get
            {
                return String.Format("SELECT id, name, isEnabled, assemblyName, typeName, switchXml FROM {0}", _tableName);
            }
        }

        /// <summary>
        /// SQL statement to select the feature switch with a given name, to be provided in a parameter named @NameParam.
        /// </summary>
        private string SelectSwitchSql
        {
            get
            {
                return String.Format("SELECT id, name, isEnabled, assemblyName, typeName, switchXml FROM {0} WHERE name = @NameParam", _tableName);
            }
        }

        /// <summary>
        /// SQL statement to select count of feature switches in the database.
        /// </summary>
        private string SwitchCountSql
        {
            get
            {
                return String.Format("SELECT COUNT(*) FROM {0} WHERE name = @Param1", _tableName);
            }
        }

        /// <summary>
        /// SQL statement to update the feature switch with name @NameParam with values @IsEnabled, @AssemblyName, @TypeName, and @SwitchXml.
        /// </summary>
        private string UpdateSwitchSql
        {
            get
            {
                return String.Format("UPDATE {0} SET isEnabled = @IsEnabled, assemblyName = @AssemblyName, typeName = @TypeName, switchXml = @SwitchXml WHERE name = @SwitchName", _tableName);
            }
        }

        /// <summary>
        /// SQL statement to insert a feature switch with values @FeatureName, @IsEnabled, @AssemblyName, @TypeName, and @SwitchXml.
        /// </summary>
        private string InsertSwitchSql
        {
            get
            {
                return String.Format("INSERT INTO {0} (name, isEnabled, assemblyName, typeName, switchXml ) VALUES (@SwitchName, @IsEnabled, @AssemblyName, @TypeName, @SwitchXml)", _tableName);
            }
        }

        /// <summary>
        /// SQL statement to delete the row for the feature switch with name @NameParam.
        /// </summary>
        private string DeleteSwitchSql
        {
            get
            {
                return String.Format("DELETE FROM {0} WHERE name = @SwitchName", _tableName);
            }
        }

        #endregion
    }
}
