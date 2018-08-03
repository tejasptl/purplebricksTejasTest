using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace SNS.CodeLibrary.SharedCode
{
    public static class ExportUtils
    {

        public static string PutString(string target, object src, int offset, int length = 0, char padChar = ' ')
        {
            string res = target ?? "";

            string val = "";
            if (src == null)
                val = "";
            else if (src is string)
                val = (string)src;
            else if (src is DateTime)
                val = ExportUtils.FormatDate((DateTime)src);
            else
                val = src.ToString();

            if (res.Length < offset)
                res = res.PadRight(offset, padChar);

            if (length > 0)
            {
                if (val.Length < length)
                    val = val.PadRight(length, padChar);
                else if (val.Length > length)
                    val = val.Substring(0, length);
            }

            res = res.Substring(0, offset) + val;
            if (target.Length > offset + val.Length)
                res += target.Substring(offset + val.Length);

            return res;
        }

        public static string FormatDate(DateTime? dt)
        {
            if (dt == null) return "        ";
            return ((DateTime)dt).ToString("yyyyMMdd");
        }

        public static string FormatMoney(decimal amount, int length = 0)
        {
            //string res = (amount * 100).ToString(); 

            decimal roundVal = Math.Floor(amount);
            string res = "" + (int)(Math.Round(amount - roundVal, 2) * 100);
            res = roundVal.ToString() + res.PadLeft(2, '0') + "0";
            if (length > 0)
                res = res.PadLeft(length, '0');

            return res;
        }
    }

    /// <summary>
    /// Utils class miscellaneous utilities for use within the SNS
    /// December 2012 : Initial Creation By Frank Perez
    /// </summary>
    public static class Utils
    {
        #region "Properties"

        public static DateTime MinDateUtc = new DateTime(0L, DateTimeKind.Utc);

        /// <summary>
        /// Returns the database connection string from the config file.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["IrisDatabaseConnection"].ToString();
            }
        }

        #endregion

        /// <summary>
        /// Validates whether the given string is in valid IP version 4 format.
        /// </summary>
        /// <param name="IPAddress">IPAdress string to check</param>
        /// <returns>True if IPv4 address, else false</returns>
        public static bool CheckIPv4AddressFormat(string IPAddress)
        {
            string pattern = @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][‌​0-9]?)\b";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(IPAddress);
        }

        /// <summary>
        /// Retrieves the IP version 4 address of the given host name or address
        /// </summary>
        /// <returns>IPv4 Address</returns>
        public static string GetIP4Address(string IPAddress)
        {
            string IP4Address = String.Empty;

            if (string.IsNullOrWhiteSpace(IPAddress))
            {
                throw new ArgumentNullException("IPAddress");
            }

            //if already in IPv4 format, return the IPAddress passed in
            if (CheckIPv4AddressFormat(IPAddress))
            {
                return IPAddress;
            }

            //not in IPv4, try to get IPv4 address
            foreach (IPAddress IPA in Dns.GetHostAddresses(IPAddress))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(IP4Address))
            {
                return IP4Address;
            }

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            return IP4Address;
        }

        /// <summary>
        /// Returns true if if passed in DataSet is null or contains no data rows.
        /// </summary>
        /// <param name="ds">DataSet object to check.</param>
        /// <returns>True or False</returns>
        public static bool IsDataSetEmpty(DataSet ds)
        {
            bool isEmpty = true;
            //if null or no tables, empty

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataTable dt in ds.Tables)
                {
                    if (dt.Rows.Count > 0)
                    {
                        isEmpty = false;
                        break;
                    }
                }
            }

            return isEmpty;
        }

        /// <summary>
        /// Returns whether or not the passed in string array contains any of the string 
        /// parameters passed in.
        /// </summary>
        /// <param name="items">String array of items that will be checked against. Defined as an object to alow Session data to be directly passed in.</param>
        /// <param name="checks">Parameter list of strings to check array for.</param>
        /// <returns>True or False</returns>
        public static bool CheckStringArrayContains(object items, params string[] checks)
        {
            bool isContained = false;
            string[] arrItems = items as string[];

            if (arrItems != null && arrItems.Length > 0)
            {
                foreach (string check in checks)
                {
                    if (arrItems.Contains<string>(check))
                    {
                        isContained = true;
                        break;
                    }
                }
            }
            return isContained;
        }

        /// <summary>
        /// Uses the rows in the passed in DataTable to build a list of LisItems.
        /// </summary>
        /// <param name="table">DataTabel containing list data</param>
        /// <param name="displayField">Column name of display field</param>
        /// <param name="valueField">Column name of value field</param>
        /// <returns>List collection of ListItems objects</returns>
        public static List<ListItems> DeriveListItemsFromDataTable(DataTable table, string displayField, string valueField)
        {
            ListItems item = null;
            List<ListItems> list = null;
            
            if (table == null || table.Rows.Count == 0)
            {
                return null;
            }

            if (!table.Columns.Contains(displayField) || !table.Columns.Contains(valueField))
            {
                throw new ApplicationException(string.Format("The given display ('{0}') and/or value ('{1}') fields do not exist in this table.", displayField, valueField));
            }

            list = new List<ListItems>();
            foreach (DataRow row in table.Rows)
            {
                item = new ListItems(row[valueField].ToString(), row[displayField].ToString());
                list.Add(item);
            }
            return list;
        }

        public static IDbDataParameter AddCommandOutputParameter(IDbCommand cmd, string fieldName, DbType? type = null)
        {
            IDbDataParameter res = cmd.CreateParameter();
            res.ParameterName = fieldName;
            res.Direction = ParameterDirection.Output;
            if (type == null || type == DbType.String)
            {
                res.DbType = DbType.String;
                res.Size = 4000;
            }
            else
            {
                res.DbType = (DbType)type;
            }
            cmd.Parameters.Add(res);
            return res;
        }
        public static IDbDataParameter BuildCommandParameter(IDbDataParameter res, string fieldName, object value, string offValue = "")
        {
            
            //IDbDataParameter res = cmd.CreateParameter();
            res.ParameterName = fieldName;
            if (value is Nullable<bool>)
            {
                bool? v = value as bool?;
                if (v == null)
                {
                    res.Value = "";
                }
                else
                {
                    res.Value = ((bool)v) ? "Y" : offValue;
                }
            }
            else if (value == null)
            {
                res.Value = offValue;
                //res = cmd.Parameters.Add(cmd.CreateParameter(fieldName, offValue));
            }
            else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(DescribedEnum<>))
            {
                string descVal = (string)value.GetType().GetProperty("Code").GetValue(value, null);
                res.Value = descVal;
                //res = cmd.Parameters.AddWithValue(fieldName, descVal);
            }
            else if (value.GetType().IsEnum)
            {
                res.Value = DescribedEnum.GetDatabaseCode(value);
            }
            else if (value is Boolean)
            {
                bool? v = value as bool?;
                res.Value = ((bool)v) ? "Y" : offValue;
                //res = cmd.Parameters.AddWithValue(fieldName, ((bool)v) ? "Y" : offValue);
            }
            else if (value is DateTime && (((DateTime)value) == DateTime.MinValue || ((DateTime)value) == Utils.MinDateUtc))
            {
                res.Value = offValue;
                //res = cmd.Parameters.AddWithValue(fieldName, offValue);
            }
            else if (value is int && (int)value == int.MinValue)
            {
                res.Value = offValue;
                //res = cmd.Parameters.AddWithValue(fieldName, offValue);
            }
            else if (value is decimal && (decimal)value == decimal.MinValue)
            {
                res.Value = offValue;
            }
            else if (value.GetType().IsValueType)
            {
                res.Value = value;
                //res = cmd.Parameters.AddWithValue(fieldName, value);
            }
            else
            {
                res.Value = value.ToString();
            }
            return res;
        }

        public static IDbDataParameter AddCommandParameter(IDbCommand cmd, string fieldName, object value, string offValue = "")
        {

            IDbDataParameter p = cmd.CreateParameter();
            BuildCommandParameter(p, fieldName, value, offValue);
            cmd.Parameters.Add(p);

            return p;
        }

        public static OleDbParameter AsOleDbParameter(IDbDataParameter p)
        {
            if (p == null) return null;
            if (p is OleDbParameter) return (OleDbParameter)p;
            OleDbParameter p2 = null;
            if (p.Value == null)
            {
                p2 = new OleDbParameter(p.ParameterName, p.DbType);
            }
            else
            {
                p2 = new OleDbParameter(p.ParameterName, p.Value);
            }
            return p2;
        }
        public static T GetReaderField<T>(IDataReader reader, string fieldName, T nullValue)
        {
            return (T)GetReaderField(typeof(T), reader, fieldName, nullValue);
        }

        public static object GetReaderField(Type T, IDataReader reader, string fieldName, object nullValue)
        {
            int fieldPos = -1;
            if (fieldName == null) return null;
            /*
            for (int pInd = 0; pInd < reader.FieldCount; pInd++)
            {
                if (reader.GetName(pInd).ToUpper() == fieldName.ToUpper())
                {
                    fieldPos = pInd;
                    break;
                }
            }
            if (fieldPos < 0) return nullValue;
             */
            try
            {
                fieldPos = reader.GetOrdinal(fieldName);
            }
            catch (IndexOutOfRangeException)
            {
                return nullValue;
            }
            if (fieldPos < 0)
            {
                return nullValue;
            }
            return SNSConvertType(T, reader.GetValue(fieldPos), nullValue);
        }

        public static T SNSConvertType<T>(object val, T nullValue = default(T))
        {
            return (T)SNSConvertType(typeof(T), val, nullValue);
        }

        public static object SNSConvertType(Type T, object val, object nullValue)
        {
            if (val is Newtonsoft.Json.Linq.JValue)
            {
                val = ((Newtonsoft.Json.Linq.JValue)val).Value;
            }

            if (val == null || val is DBNull)
            {
                return nullValue;
            }

            string valStr = "";

            if (T.IsAssignableFrom(val.GetType()))
            {
                if (val is Decimal)
                {
                    val = ((decimal)val / 1.00000000000m);
                }
                return val;
            }

            if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (val is string && (string)val == "")
                {
                    return nullValue;
                }
                T = T.GetGenericArguments()[0];
            }


            TypeCode tCode = Type.GetTypeCode(T);
            if (val is IConvertible 
                && tCode != TypeCode.Object 
                && tCode != TypeCode.DateTime
                && !T.IsGenericType
                && !T.IsEnum
                && !(val is string))
            {
                return Convert.ChangeType(val, T);
            }


            if (val == null)
            {
                valStr = "";
            }
            else if (val is bool)
            {
                valStr = ((bool)val) ? "Y" : "";
            }
            else if (val is string)
            {
                valStr = (string)val;
            }
            else
            {
                valStr = val.ToString() ?? "";
            }

            if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(DescribedEnum<>))
            {
                object descRes = T.GetConstructor(new Type[] { typeof(string) }).Invoke(new object[] { valStr });

                return descRes;
            }


            if (T.Equals(typeof(bool)))
            {
                if (valStr == "Y")
                {
                    object conv = true;
                    return conv;
                }
                else
                {
                    return false;
                }
            }

            if (T.Equals(typeof(String)))
            {
                return valStr;
            }

            if (T.IsEnum)
            {
                if (typeof(int).IsAssignableFrom(val.GetType()))
                {
                    return (int)val;
                }
                if (DescribedEnum.IsValidCode(T, valStr))
                {
                    return DescribedEnum.GetEnumValue(T, valStr);
                }
                try
                {
                    return Enum.Parse(T, valStr);
                }
                catch (ArgumentException)
                {
                }
            }

            if (T.IsAssignableFrom(typeof(DateTime)) && val.GetType().IsAssignableFrom(typeof(String)))
            {
                //object conv = DateTime.Parse(valStr);
                object conv = Utils.ParseDate((string)val, null);
                return conv;
            }


            if (T.IsAssignableFrom(typeof(Double)))
            {
                if (val is bool)
                {
                    return ((bool)val) ? 1.0 : 0.0;
                }
                object conv = Double.Parse(valStr);
                return conv;
            }
            if (T.IsAssignableFrom(typeof(Decimal)))
            {
                if (val is bool)
                {
                    return ((bool)val) ? 1m : 0m;
                }
                decimal conv = Decimal.Parse(valStr);
                return conv / 1.000000000000000000000000m;
            }
            if (T.IsAssignableFrom(typeof(Int64)))
            {
                if (val is bool)
                {
                    return ((bool)val) ? 1L : 0L;
                }
                object conv = Int64.Parse(valStr);
                return conv;
            }
            if (T.IsAssignableFrom(typeof(UInt64)))
            {
                if (val is bool)
                {
                    return (UInt64)(((bool)val) ? 1 : 0);
                }
                object conv = UInt64.Parse(valStr);
                return conv;
            }
            if (T.IsAssignableFrom(typeof(Int32)))
            {
                if (val is bool)
                {
                    return ((bool)val) ? 1 : 0;
                }
                object conv = Int32.Parse(valStr);
                return conv;
            }
            if (T.IsAssignableFrom(typeof(UInt32)))
            {
                if (val is bool)
                {
                    return (UInt32)(((bool)val) ? 1 : 0);
                }
                object conv = UInt32.Parse(valStr);
                return conv;
            }
            ConstructorInfo ci = T.GetConstructor(new Type[] { typeof(string) });
            if (ci != null)
            {
                return ci.Invoke(new object[] { valStr });
            }
            return nullValue;
        }

        public static object GetReaderField(Type T, IDataReader reader, int fieldPos, object nullValue)
        {
            return SNSConvertType(T, reader.GetValue(fieldPos), nullValue);
        }

        public static string AttrString<T, AT>(string fieldName, string defaultValue = null)
             where AT : Attribute
        {
            string res = defaultValue; // default(AT);
            FieldInfo fi = typeof(T).GetField(fieldName);
            PropertyInfo pi = typeof(T).GetProperty(fieldName);
            if (fi != null)
            {
                return AttrString<AT>(fi, defaultValue);
            }
            else if (pi != null)
            {
                return AttrString<AT>(pi, defaultValue);
            }
            return res;
        }

        private static Dictionary<string, bool> AttrDefinedCache = new Dictionary<string, bool>();
        private static System.Threading.ReaderWriterLockSlim AttrDefinedCacheLock = new System.Threading.ReaderWriterLockSlim();
        public static bool AttrIsDefined(MemberInfo fi, Type attr)
        {
            string key = fi.ReflectedType.FullName + ":" + fi.Name + ":" + attr.FullName;
            bool res = false;

            try
            {
                if (AttrDefinedCacheLock.TryEnterReadLock(100))
                {
                    if (AttrDefinedCache.TryGetValue(key, out res))
                    {
                        return res;
                    }
                    AttrDefinedCacheLock.ExitReadLock();
                }
                res = Attribute.IsDefined(fi, attr);
                if (AttrDefinedCacheLock.TryEnterWriteLock(100))
                {
                    AttrDefinedCache[key] = res;
                }
                return res;
            }
            finally
            {
                if (AttrDefinedCacheLock.IsReadLockHeld)
                {
                    AttrDefinedCacheLock.ExitReadLock();
                }
                if (AttrDefinedCacheLock.IsWriteLockHeld)
                {
                    AttrDefinedCacheLock.ExitWriteLock();
                }
            }
        }

        public static string AttrString<AT>(MemberInfo fi, string defaultValue = null)
            where AT : Attribute
        {
            string res = defaultValue; // default(AT);
            //if (Attribute.IsDefined(fi, typeof(AT)))
            if (AttrIsDefined(fi, typeof(AT)))
            {
                AT resObj = (AT)Attribute.GetCustomAttribute(fi, typeof(AT));
                if (resObj != null)
                {
                    res = resObj.ToString();
                }
            }
            return res;
        }

        public static DataTable FieldsToDataTable<T>()
        {
            DataTable tab = new DataTable(typeof(T).Name);
            MemberInfo[] allMembers = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (MemberInfo mi in allMembers)
            {
                if (!mi.IsDefined(typeof(DatabaseCodeAttribute), true)) continue;

                if (mi is FieldInfo)
                {
                    tab.Columns.Add(mi.Name, ((FieldInfo)mi).FieldType);
                }
                else if (mi is PropertyInfo)
                {
                    tab.Columns.Add(mi.Name, ((PropertyInfo)mi).PropertyType);
                }
            }
            return tab;            
        }

        public static DataTable ReaderToDataTable(IDataReader reader, string name = null)
        {
            DataTable tab = String.IsNullOrWhiteSpace(name)? new DataTable() : new DataTable(name);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (tab.Columns[reader.GetName(i)] == null)
                {
                    tab.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }
            }
            return tab;
        }

        public static void ReaderToDataRow(IDataReader reader, DataRow row)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader[i];
            }
        }

        public static void ReadFields<T>(T tuple, IDataReader reader, string prefix = null)
        {
//            List<MemberInfo> allFieldsList = new List<MemberInfo>();
//            allFieldsList.AddRange(typeof(T).GetFields());
//            allFieldsList.AddRange(typeof(T).GetProperties());
//
//            MemberInfo[] allFields = allFieldsList.ToArray();
            MemberInfo[] allFields = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public);

            foreach (MemberInfo fi in allFields)
            {
                if (fi.MemberType != MemberTypes.Field && fi.MemberType != MemberTypes.Property)
                {
                    continue;
                }

                string dbName = Utils.AttrString<DatabaseCodeAttribute>(fi, null);
                int pos = -1;
                if (!String.IsNullOrWhiteSpace(dbName))
                {
                    try
                    {
                        pos = reader.GetOrdinal(prefix + dbName);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        pos = -1;
                    }
                    if (pos < 0)
                    {
                        throw new ApplicationException("Invalid database field " + prefix + dbName + " for the property " + typeof(T).Name + "." + fi.Name);
                    }

                    
                    Type fiType = null;
                    if (fi is FieldInfo)
                    {
                        fiType = ((FieldInfo)fi).FieldType;
                    }
                    else
                    {
                        fiType = ((PropertyInfo)fi).PropertyType;
                    }
                    try
                    {
                        object objVal = GetReaderField(fiType, reader, pos, null);

                        if (fi is FieldInfo)
                        {
                            ((FieldInfo)fi).SetValue(tuple, objVal);
                        }
                        else
                        {
                            ((PropertyInfo)fi).SetValue(tuple, objVal, null);
                        }
                    }
                    catch (Exception rex)
                    {
                        throw new ApplicationException("Error reading property " + fi.Name + ": " + rex.Message, rex);
                    }
                }
            }
        }

        /// <summary>
        /// Builds a list of a DML statement parameters in the given mode (SELECT, UPDATE, INSERT, INSERTVALUES).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tuple"></param>
        /// <param name="mode"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="cmdParams"></param>
        public static string BuildSQLString<T>(T tuple, string mode, string tablePrefix = null, List<IDbDataParameter> cmdParams = null)
        {
            List<MemberInfo> allFields = new List<MemberInfo>();
            allFields.AddRange(typeof(T).GetFields());
            allFields.AddRange(typeof(T).GetProperties());

            return BuildSQLString(tuple, allFields, mode, tablePrefix, cmdParams);
        }

        public static string BuildSQLString(object tuple, List<MemberInfo> fields, string mode, string tablePrefix = null, List<IDbDataParameter> cmdParams = null)
        {
            string sql = null;
            string fieldPrefix = null;
            string paramPrefix = null;
            bool readonlyMode = (mode == "SELECT");
            if (String.IsNullOrWhiteSpace(tablePrefix))
            {
                fieldPrefix = null;
                paramPrefix = null;
            }
            else if (tablePrefix.EndsWith("."))
            {
                fieldPrefix = tablePrefix;
                paramPrefix = tablePrefix.TrimEnd('.') + "_";
            }
            else
            {
                fieldPrefix = tablePrefix + ".";
                paramPrefix = tablePrefix + "_";
            }

            List<MemberInfo> allFields = fields;
            foreach (MemberInfo fi in allFields.OrderBy(q => q.Name))
            {
                Type fiType = null;
                object  fiValue = null;
                if (fi is FieldInfo)
                {
                    fiType = ((FieldInfo)fi).FieldType;
                    if (tuple != null)
                    {
                        fiValue = ((FieldInfo)fi).GetValue(tuple);
                    }
                }
                else
                {
                    fiType = ((PropertyInfo)fi).PropertyType;
                    if (tuple != null)
                    {
                        fiValue = ((PropertyInfo)fi).GetValue(tuple, null);
                    }
                }
                DatabaseCodeAttribute dbAttr = (DatabaseCodeAttribute)Attribute.GetCustomAttribute(fi, typeof(DatabaseCodeAttribute));
                //string dbName = Utils.AttrString<DatabaseCodeAttribute>(fi, null);
                string dbName = null;
                string offVal = "";
                if (dbAttr != null && (readonlyMode || !dbAttr.Readonly))
                {
                    dbName = dbAttr.Code;
                    offVal = dbAttr.OffValue;
                }

                if (dbName != null)
                {
                    if (sql != null) sql += ", ";
                    if (mode == "SELECT")
                    {
                        sql += fieldPrefix + dbName;
                    }
                    else if (mode == "INSERT")
                    {
                        sql += dbName;
                    }
                    else if (mode == "UPDATE")
                    {
                        sql += fieldPrefix + dbName + " = ? ";
                        if (cmdParams != null)
                        {
                            IDbDataParameter p = Utils.BuildCommandParameter(new OleDbParameter(), paramPrefix + dbName, fiValue, offVal);
                            cmdParams.Add(p);                            
                        }
                    }
                    else if (mode == "INSERTVALUES")
                    {
                        sql += " ?";
                        if (cmdParams != null)
                        {
                            IDbDataParameter p = Utils.BuildCommandParameter(new OleDbParameter(), paramPrefix + dbName, fiValue, offVal);
                            cmdParams.Add(p);
                        }
                    }
                }
            }
            return sql;
        }

        /// <summary>
        /// Returns True if the passed in email string is in a valid format for an email address
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>True or False</returns>
        public static bool IsValidEmailFormat(string email)
        {
            if(string.IsNullOrWhiteSpace(email))
                return false;

            Regex emailRegex = new Regex(@"^([\w-\']+[\w-\.\']*@[\w-]+[\w-\.]*\.[A-z]{2,5})$");
            Regex negateRegex = new Regex(@"^([\w-\.]+\.@[\w-\.]+)$");

            return emailRegex.IsMatch(email) && !negateRegex.IsMatch(email);
        }

        /// <summary>
        /// Returns True if the passed in phone number contains only numbers and spaces, with an optional + sign at the start.
        /// </summary>
        /// <param name="phone">Phone number to check</param>
        /// <returns>True or False</returns>
        public static bool IsValidPhoneNumber(string phone)
        {
            if(string.IsNullOrWhiteSpace(phone))
                return false;

            Regex phoneRegex = new Regex(@"^(?:\+?)(\s*\d+)+$");
            return phoneRegex.IsMatch(phone);
        }

        /// <summary>
        /// Returns True if the passed in BSB number is in the valid format, ###-#### or ######.
        /// </summary>
        public static bool IsValidBsbNumber(string bsbNumber)
        {
            if(string.IsNullOrWhiteSpace(bsbNumber))
                return false;

            Regex bsbRegex = new Regex(@"^\d{3}-?\d{3}$");
            return bsbRegex.IsMatch(bsbNumber);
        }

        public static bool IsValidABN(string abnNumber)
        {
            if (string.IsNullOrWhiteSpace(abnNumber))
                return false;

            int[] weightFactor ={10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19};
            Regex abnRegex = new Regex(@"\s+/g");
            abnNumber = abnRegex.Replace(abnNumber.Trim(),""); // Removing whitespaces in from he input.
            abnRegex = new Regex(@"^[1-9]\d{10}$");              // Only numbers. Maximum 11 digits and and first digit can not be zero.
            if (abnRegex.IsMatch(abnNumber)) 
            {
                string first = abnNumber.Substring(0,1);
                int intFirst = int.Parse(first);
                int[] digitArray = abnNumber.Select(x => ((int)x)-'0').ToArray();
                digitArray[0] = intFirst - 1;
                var sum = 0;
                for (int i = 0; i < 11; i++) {
                    sum += (digitArray[i] * weightFactor[i]); // Multiply each digit by weight factor
                }
                if (sum != 0) {
                    if ((sum % 89) == 0) {         // Dividing sum my 89 gives zero as remainder denotes valid ABN.
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Takes a url in the form ~/folder/file and returns the full url i.e. http://host/virtualdir/folder/file.
        /// Used to set references to content files as being full urls.
        /// </summary>
        /// <param name="path">The application path to content, starting with ~</param>
        /// <returns>Full URL string</returns>
        public static string AbsoluteContent(string path)
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);

            //If the URI is not already absolute, rebuild it based on the current request.
            if (!uri.IsAbsoluteUri)
            {
                Uri requestUrl = HttpContext.Current.Request.Url;
                UriBuilder builder = new UriBuilder(requestUrl.Scheme, requestUrl.Host, requestUrl.Port);

                builder.Path = VirtualPathUtility.ToAbsolute(path);
                uri = builder.Uri;
            }

            return uri.ToString();
        }

        /// <summary>
        /// Returns null or optional null parameter if input is null, else returns input trimmed of leading and trailing whitespaces.
        /// </summary>
        public static string TrimString(string input, string nullValue = null)
        {
            if (input == null)
                return nullValue;
            else
                return input.Trim();
        }

        public static List<T> SplitString<T>(string from, string separator)
        {
            List<T> res = new List<T>();
            if (String.IsNullOrEmpty(from))
            {
                //return res;
            }
            else if (String.IsNullOrEmpty(separator))
            {
                res.Add(Utils.SNSConvertType<T>(from));
            }
            else
            {
                foreach (string s in from.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries))
                {
                    res.Add(Utils.SNSConvertType<T>(s));
                }
            }
            return res;
        }

        public static string SafeSubstring(string str, int startIndex, int length = 0)
        {
            if (str == null) return "";
            int strLen = str.Length;
            int pos = startIndex;
            int len = length;

            if (startIndex >= strLen || startIndex < -strLen) return "";

            if (startIndex < 0)
            {
                pos = strLen + startIndex;
            }

            if (length == 0) return str.Substring(pos);

            return str.Substring(pos, Math.Min(length, strLen - pos));
        }
        /// <summary>
        /// compares two strings, null-safe:
        /// * null and empty strings are considered equal (Oracle style)
        /// * if withTrim is set, then strings are trimmed before comparision
        /// * if ignoreCase is set, then strings are compared with InvariantCultureIgnoreCase
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="withTrim"></param>
        /// <returns></returns>
        public static bool StringsEqual(string a, string b, bool ignoreCase = false, bool withTrim = false)
        {
            if (a == null && b == null) return true;
            string aVal = (a ?? "");
            string bVal = (b ?? "");
            if (withTrim)
            {
                aVal = aVal.Trim();
                bVal = bVal.Trim();
            }
            if (ignoreCase)
            {
                return String.Equals(aVal, bVal, StringComparison.InvariantCultureIgnoreCase);
            }
            return String.Equals(aVal, bVal, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Adds a parameter to the Url's query string
        /// </summary>
        /// <param name="Url">Url to add query parameter. Must be absolute URL.</param>
        /// <param name="Name">Parameter name</param>
        /// <param name="Value">Parameter value</param>
        /// <returns>Null if URL is null or empty or is badly formed URL string.</returns>
        public static string AddQueryToUrl(string Url, string Name, string Value)
        {
            if (string.IsNullOrWhiteSpace(Url) || !Uri.IsWellFormedUriString(Url, UriKind.Absolute))
                return null;
            
            Uri uri = new Uri(Url);
            NameValueCollection qs = HttpUtility.ParseQueryString(uri.Query);
            qs.Set(Name, Value);
            return uri.GetLeftPart(UriPartial.Path) + "?" + qs.ToString();
        }

        public static string AddQueryToUrl(string Url, NameValueCollection coll)
        {
            if (string.IsNullOrWhiteSpace(Url) || !Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                return null;
            }
            if (coll == null || coll.Count == 0)
            {
                return Url;
            }

            Uri uri = new Uri(Url);
            NameValueCollection qs = HttpUtility.ParseQueryString(uri.Query); // note that this is a special internal class HttpValueCollection, not a simple NameValue
            foreach (string k in coll.AllKeys)
            {
                qs.Add(k, coll[k]);
            }
            return uri.GetLeftPart(UriPartial.Path) + "?" + qs.ToString(); 
        }

        /// <summary>
        /// Strips certain characters from the input string so that it doesn't break the format of a CSV text file.
        /// Any embedded commas or double-quote characters will be quoted.
        /// Each of the embedded double-quote characters will be represented by a pair of double-quote characters.
        /// Carriage Return and Line Feeds converted to commas.
        /// </summary>
        public static string SanitizeCSVText(string Input)
        {
            if (string.IsNullOrWhiteSpace(Input))
                return Input;

            string output = Input;
            
            output = output.Replace("\n\r",";");
            output = output.Replace("\r\n",";");
            output = output.Replace("\n", ";");
            output = output.Replace("\r", ";");
            if (output.Contains(",") || output.Contains("\"")) //Any embedded commas or double-quote characters must be quoted.
            {
                output = output.Replace("\"", "\"\"");  //Each of the embedded double-quote characters must be represented by a pair of double-quote characters.
                output = "\"" + output + "\""; //Any embedded commas or double-quote characters must be quoted.
            }

            return output;
        }

        /// <summary>
        /// Replaces passed in characters from Input line with empty string, hence stripping them out.
        /// </summary>
        public static string StripText(string Input, params string[] ReplaceChars)
        {
            if(string.IsNullOrWhiteSpace(Input) || ReplaceChars == null || ReplaceChars.Count() == 0)
                return Input;

            string output = Input;

            foreach(string rem in ReplaceChars)
            {
                output = output.Replace(rem, string.Empty);
            }

            return output;
        }


        public static string ToSQLSafe(string val)
        {
            if (val == null) return "";
            return val
                .Replace("'", "''")
                .Replace("&", "'||CHR(38)||'")
            ;
        }

        public static string ToJSSafe(string val)
        {
            if (val == null) return "";
            return val
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                //.Replace("\\u", "\\\\u")
                .Replace("\n", "\\n")
            ;
        }

        /// <summary>
        /// Converts our "asterisk" syntax for string search to the SQL 'like' syntax.
        /// If forceWildcard is true and there are no wildcards in the given string, appends one '%' to the right end of the string.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="forceWildcard"></param>
        /// <returns></returns>
        public static string WildcardizeString(string src, bool forceWildcard = false, bool convertToSearch = false)
        {
            if (src == null)
                return "";
            bool hasWildcards = src.Contains("*") || src.Contains("?");
            string res = src.Replace("%", "\\%")
                            .Replace("_", "\\_")
                            .Replace("*", "%")
                            .Replace("?", "_")
            ;

            if (!hasWildcards && forceWildcard)
            {
                res += "%";
            }
            if (convertToSearch)
            {
                res = ConvertToSearch(res);
            }
            return res;
        }

        /// <summary>
        /// This is a C# equivalent of our Oracle INTEGRITYPACKAGE.CONVERTTOSEARCH function.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ConvertToSearch(string src)
        {
            /*   upperstring := UPPER (normalcolumn);

      --We must now strip any symbols out of the given value.
      strippedstring :=
         TRANSLATE (
            upperstring,
            '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ*_ -!@#$^&()*''+=,<.>/?',
            '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ%_ ');
  */           
            if (String.IsNullOrWhiteSpace(src)) return "";
            return Regex.Replace(src.Trim().ToUpper().Replace('*', '%'), "[-!@#$^&()'+=,<.>/?]", "");
        }

        public static string FormatMoney(decimal? val, bool withDollar = false)
        {
            if (val == null) return "";
            if (withDollar) return val.Value.ToString("C");

            return val.Value.ToString("F2");
        }

        public static string FormatMoney(double? val, bool withDollar = false)
        {
            if (val == null) return "";
            if (withDollar) return val.Value.ToString("C");

            return val.Value.ToString("F2");
        }

        public static String FormatDate(DateTime? dt, string nullValue = "")
        {
            if (dt == null || dt <= DateTime.MinValue || dt <= Utils.MinDateUtc) return nullValue;

            return ((DateTime)dt).ToString("dd/MM/yyyy");
        }

        public static String FormatDateTime(DateTime? dt, string nullValue = "")
        {
            if (dt == null || dt <= DateTime.MinValue || dt <= Utils.MinDateUtc) return nullValue;

            return ((DateTime)dt).ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static DateTime? ParseDate(String strDt, DateTime? nullValue = null)
        {
            if (String.IsNullOrWhiteSpace(strDt)) return nullValue;
            DateTime tryRes;
            DateTime? res = null;
            if (!DateTime.TryParseExact(strDt, "d/M/yyyy", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                && !DateTime.TryParseExact(strDt, "d-M-yyyy", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                && !DateTime.TryParseExact(strDt, "d/M/y", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                && !DateTime.TryParseExact(strDt, "d-M-y", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                && !DateTime.TryParse(strDt, out tryRes)
            )
            {
                res = nullValue;
            }
            else
            {
                res = tryRes;
            }
            return res;
        }

        public static TimeSpan ParseTime(string strTime)
        {
            TimeSpan res = new TimeSpan(0);
            DateTime tryRes;
            if (DateTime.TryParseExact(strTime, "H:m", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "H:m:s", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "h:m tt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "h:m:s tt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "h:mtt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "h:m:stt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "Hmm", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "Hmmss", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "hmm tt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "hmmss tt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "hmmtt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                || DateTime.TryParseExact(strTime, "hmmsstt", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out tryRes)
                )
            {
                return tryRes.TimeOfDay;
            }
            return res;
        }

        public static DateTime? ParseDateTime(string strDateTime, DateTime? nullValue = null)
        {
            if (String.IsNullOrWhiteSpace(strDateTime))
            {
                return nullValue;
            }
            int whitePos = strDateTime.IndexOf(" ");
            if (whitePos < 0)
            {
                return ParseDate(strDateTime);
            }

            DateTime? part1 = ParseDate(strDateTime.Substring(0, whitePos), null);
            if (part1 == null)
            {
                return nullValue;
            }

            TimeSpan part2 = ParseTime(strDateTime.Substring(whitePos + 1));

            return part1.Value + part2;
        }

        /// <summary>
        /// returns (d2 - d1), null-safe. If absolute = true, the result will be positive
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="absolute"></param>
        /// <returns></returns>
        public static TimeSpan TimeDiff(DateTime? d1, DateTime? d2, bool absolute = true)
        {
            TimeSpan res;
            if (d1 == null && d2 == null) res = new TimeSpan(0);
            else if (d1 == null) res = new TimeSpan(d2.Value.Ticks);
            else if (d2 == null) res = new TimeSpan(-d1.Value.Ticks);
            else res = d2.Value - d1.Value;

            if (absolute && res.Ticks < 0) res = res.Negate();
            return res;
        }

        public static string FormatFileSize(long size, int precision = 1)
        {
            string[] sizes = { " B", " KB", " MB", " GB" };
            int order = 0;
            decimal len = size;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            len = Math.Round(len, precision);
            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            //return result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return len + sizes[order];

        }

        public static long ParseFileSize(string size)
        {
            long res = 0;
            string[] sizes = { "B", "KB", "MB", "GB" };
            if (String.IsNullOrWhiteSpace(size)) return 0;
            string trimSize = size.Trim().ToUpper();
            int i = sizes.Length - 1;
            for (; i >= 0; i++)
            {
                if (trimSize.EndsWith(sizes[i]))
                {
                    break;
                }
            }
            if (i < 0)
            {
                if (!Int64.TryParse(trimSize, out res))
                {
                    throw new ApplicationException("Invalid FileSize string: " + size);
                }
            }
            else
            {
                decimal d;
                if (!Decimal.TryParse(trimSize.Substring(0, trimSize.Length - sizes[i].Length), out d))
                {
                    throw new ApplicationException("Invalid FileSize string: " + size);
                }
                res = (long)(d * (decimal)Math.Pow(1024, i));
            }
            return res;
        }

        public static string DataToJSON(object data, bool jsEscape = false)
        {
            if (data == null)
            {
                return "{}";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(ms);
            System.Runtime.Serialization.Json.DataContractJsonSerializer jsonWriter = new System.Runtime.Serialization.Json.DataContractJsonSerializer(data.GetType());
            jsonWriter.WriteObject(ms, data);
            ms.Position = 0;

            string res = sr.ReadToEnd() ?? "";
            //res = res.Replace("'", "\\'");
            if (jsEscape)
            {
                res = Utils.ToJSSafe(res);
            }
            return res;
        }

        public static string ToXmlString(object obj)
        {
            if (obj == null) return "";

            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            System.Xml.XmlWriterSettings xws = new System.Xml.XmlWriterSettings();
            xws.Encoding = System.Text.Encoding.UTF8;
            System.Xml.XmlWriter xw = System.Xml.XmlWriter.Create(ms, xws);
            ser.Serialize(xw, obj);
            ms.Flush();
            string res = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());

            return res;
        }

        public static T FromXmlString<T>(string xml)
        {
            if (String.IsNullOrWhiteSpace(xml)) return default(T);

            XmlReaderSettings xrs = new XmlReaderSettings();            
            //xrs.XmlResolver.
            //System.IO.StringReader sr = new System.IO.StringReader(xml);
            //XmlReader xr = XmlReader.Create(sr);

            byte[] xmlBytes = System.Text.Encoding.UTF8.GetBytes(xml.TrimEnd('\0'));
            System.IO.MemoryStream ms = new System.IO.MemoryStream(xmlBytes);
            XmlReader xr = XmlReader.Create(ms);
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            T res = default(T);
            try
            {
                res = (T)ser.Deserialize(xr);
            }
            catch
            {
                res = default(T);
            }

            return res;
        }

        /// <summary>
        /// Checks the parameter Value if it is null, empty string or whitespace only, if so returns the second parameter DefaultValue,
        /// else returns Value.
        /// </summary>
        public static string EmptyToValue(string Value, string DefaultValue)
        {
            if (string.IsNullOrWhiteSpace(Value))
                return DefaultValue;

            return Value;
        }

        /// <summary>
        /// Copies all _public_ fields and properties of type T from src to dest.
        /// Everything is copied by value, as if you were doing dest.MyField = src = MyField.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void ShallowCopy<T>(T dest, T src)
        {
            if (src != null)
            {
                FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                /*
                FieldInfo[] fields;
                if (typeof(T1).IsAssignableFrom(typeof(T2)))
                {
                    fields = typeof(T1).GetFields(BindingFlags.Instance | BindingFlags.Public);
                }
                else
                {
                    fields = typeof(T2).GetFields(BindingFlags.Instance | BindingFlags.Public);
                }
                 */
                foreach (FieldInfo fi in fields)
                {
                    fi.SetValue(dest, fi.GetValue(src));
                }

                PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                foreach (PropertyInfo prop in props)
                {
                    if (prop.CanWrite)
                    {
                        prop.SetValue(dest, prop.GetValue(src, null), null);
                    }
                }
            }
        }

        public static bool ShallowCompare<T>(T dest, T src)
        {
            if (dest == null && src == null) return true;
            if (dest == null || src == null) return false;

            if (typeof(T).Equals(typeof(object))) return dest.Equals(src);

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo fi in fields)
            {
                object destVal = fi.GetValue(dest);
                object srcVal = fi.GetValue(src);

                if (!ShallowCompare<object>(destVal, srcVal)) return false;
            }

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (PropertyInfo prop in props)
            {
                object destVal = prop.GetValue(dest, null);
                object srcVal = prop.GetValue(src, null);

                if (!ShallowCompare<object>(destVal, srcVal)) return false;
            }
            return true;
        }

        public static string Coalesce(params string[] vals)
        {
            foreach (string s in vals)
            {
                if (!String.IsNullOrWhiteSpace(s)) return s;
            }
            return "";
        }

        public static Type DescribedEnumCoreType(Type type)
        {
            if (type == null) return null;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name.IndexOf("DescribedEnum") >= 0)
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        /// Returns object string with given format if passed in.
        /// </summary>
        public static string DisplayString(object Value, string Format = null)
        {
            if(Value == null || Value == DBNull.Value)
                return string.Empty;
            else
            {
                if(!string.IsNullOrWhiteSpace(Format))
                    return string.Format("{0:" + Format + "}", Value);
                else
                    return Value.ToString();
            }
        }

        /// <summary>
        /// This method will load an assembly and create an object for the given type with provided constructor parameters,
        /// using the first matching type it finds in the assembly.
        /// If there are more than ony mathing type, one of them will be used (whichever comes up first in the Assembly.GetTypes list).
        /// If anything goes wrong, the method will throw and exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T CreateFromLibrary<T>(string filePath, params object[] args)
        {
            Assembly lib = Assembly.LoadFrom(filePath);
            Type requiredType = typeof(T);
            foreach (Type t in lib.GetTypes())
            {
                if (requiredType.IsAssignableFrom(t))
                {
                    //T res = (T)Activator.CreateInstance(t, args);
                    T res = (T)lib.CreateInstance(t.FullName, false, BindingFlags.CreateInstance, null, args, null, null);
                    return res;
                }
            }

            throw new ApplicationException("No matching type found for " + requiredType.Name + " in " + filePath);
        }

        /// <summary>
        /// Converts boolean to char
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string boolToChar(bool value)
        {
            if (value) return "Y"; else return "N";
        }

        /// <summary>
        /// Builds a 'IN' clause where count of vals is more than 1000
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="field"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static string BuildInCond(IDbCommand cmd, string field, List<string> vals)
        {
            if (vals == null || vals.Count == 0) { return "1=0"; }

            List<string> valsNew = new List<string>();

            foreach(var val in vals)
            {
                if (!string.IsNullOrEmpty(val))
                { valsNew.Add(string.Format("(1, '{0}')", val)); }
            }

            return string.Format("(1, {0}) in ({1})", field, string.Join(",", valsNew.ToArray()));
        }

        /// <summary>
        /// This will replace the previous line of the console with the given text line.
        /// </summary>
        public static void UpdateConsoleProgress(string line)
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(line);
        }

        public static string NameValueCollectionToString(NameValueCollection coll)
        {
            if (coll == null || coll.Count == 0)
            {
                return "";
            }
            string res = "";
            foreach (string key in coll.Keys)
            {
                string val = (coll[key] ?? "")
                    .Replace("&", "&amp;")
                    .Replace("=", "&eq;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;")
                    .Replace("|", "&vert;");
                res += "|" + key + "=" + val;
            }

            return res.Substring(1);
        }

        public static NameValueCollection NameValueCollectionFromString(string from)
        {
            NameValueCollection res = new NameValueCollection();
            if (String.IsNullOrWhiteSpace(from))
            {
                return res;
            }

            foreach (string pair in from.Split('|'))
            {
                int valPos = pair.IndexOf('=');
                if (valPos < 0)
                {
                    res[pair] = "";
                }
                else
                {
                    string val = pair.Substring(valPos + 1)
                    .Replace("&eq;", "=")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Replace("&quot;", "\"")
                    .Replace("&apos;", "'")
                    .Replace("&vert;", "|")
                    .Replace("&amp;", "&")
                    ;

                    res[pair.Substring(0, valPos)] = val;
                }
            }
            return res;
        }

        /// <summary>
        /// Copies all pairs from the "from" collection into the "to" collection, overwriting already existing pairs.
        /// Note that the common NameValueCollection.Add() method does not overwrite existing pairs, it combines the values instead.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static int NameValueCollectionMerge(NameValueCollection to, NameValueCollection from)
        {
            if (to == null || from == null) return 0;
            int count = 0;
            foreach (string key in from.AllKeys)
            {
                to.Set(key, from[key]);
                count++;
            }
            return count;
        }


        /// <summary>
        /// Compare two strings.
        /// </summary>
        /// <param name="input1">The input string 1</param>
        /// <param name="input2">The input string 2</param>
        /// <param name="ignoreCase">The ignore case option</param>
        /// <returns>Returns true if both null or both equal according to the ignore case option.</returns>
        public static bool CompareString(string input1, string input2, bool ignoreCase = true)
        {
            //Trim if its not empty
            if (!string.IsNullOrEmpty(input1))
                input1 = input1.Trim();

            //Trim if its not empty
            if (!string.IsNullOrEmpty(input2))
                input2 = input2.Trim();

            if (input1 == input2)
            {
                // if both in same case and equal or both null
                return true;
            }
            else if (input1 == null || input2 == null)
            {
                // one string is null
                return false;
            }
            else if (ignoreCase)
            {
                // compare after converting them to uppder case
                return input1.ToUpper(CultureInfo.InvariantCulture) == input2.ToUpper(CultureInfo.InvariantCulture);
            }

            // if not ignore case
            return input1 == input2;
        }

        /// <summary>
        /// Calculate Age based on passed in DateofBirth. If passed in DateofBirth is null or DateTime.MinValue, then it returns null
        /// </summary>
        /// <param name="birthDate"></param>
        /// <returns></returns>
        public static int? GetAge(DateTime birthDate)
        {
            if (birthDate == null || birthDate <= DateTime.MinValue) return null;
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (today.Month < birthDate.Month || (today.Month == birthDate.Month && today.Day < birthDate.Day))
                age--;
            return age;
        }

        public static int? Abs(int? val)
        {
            if (val == null) return null;
            if (val.Value < 0) return -val.Value;

            return val;
        }

        public static decimal? Abs(decimal? val)
        {
            if (val == null) return null;
            if (val.Value < 0) return -val.Value;

            return val;
        }

        public static double? Abs(double? val)
        {
            if (val == null) return null;
            if (val.Value < 0) return -val.Value;

            return val;
        }

        public static bool InList<T>(T a, params T[] list)
        {
            if (list == null || list.Length == 0)
            {
                return false;
            }
            else if (a == null)
            {
                foreach (object b in list)
                {
                    if (b == null) return true;
                }
                return false;
            }
            else
            {
                foreach (object b in list)
                {
                    if (a.Equals(b))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Returns count of list, zero if null.
        /// </summary>
        public static int ListCount(IEnumerable<object> list)
        {
            int count = 0;
            if(list != null)
                count = Enumerable.Count<object>(list);

            return count;
        }

        public static bool IsEmptyValue<T>(T value)
        {
            bool returnValue = true;

            if (value == null || value is DBNull)
            {
                returnValue = false;
            }
            else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(DescribedEnum<>))
            {
                string descVal = (string)value.GetType().GetProperty("Code").GetValue(value, null);
                if (string.IsNullOrWhiteSpace(descVal))
                {
                    returnValue = false;
                }
            }
            else if (typeof(T).IsEnum)
            {
                if (string.IsNullOrWhiteSpace(DescribedEnum.GetDatabaseCode(value)))
                {
                    returnValue = false;
                }
            }
            else if (value is DateTime)
            {
                DateTime? d = value as DateTime?;
                if (d == DateTime.MinValue || d == Utils.MinDateUtc)
                {
                    returnValue = false;
                }
            }
            else if (value is int)
            {
                int? i = value as int?;
                if (i == int.MinValue)
                {
                    returnValue = false;
                }
            }
            else if (value is decimal)
            {
                decimal? d = value as decimal?;
                if (d == decimal.MinValue)
                {
                    returnValue = false;
                }
            }
            else if (value is string)
            {
                string s = value as string;
                if (String.IsNullOrEmpty(s))
                {
                    returnValue = false;
                }
            }
            return !returnValue;
        }
        /// <summary>
        /// Returns the value if it is not null or default value such as string empty, describe enum default value, DateTime Min value, otherwise return the null replacement value
        /// </summary>
        public static T NVL<T>(T value, params T[] nullValue)
        {
            if (nullValue == null || nullValue.Length == 0)
            {
                return value;
            }

            if (!IsEmptyValue(value))
            {
                return value;
            }
            else if (nullValue.Length == 1)
            {
                return nullValue[0];
            }
            else
            {
                foreach (T v in nullValue)
                {
                    if (!IsEmptyValue(v))
                    {
                        return v;
                    }
                }
                return nullValue[nullValue.Length - 1];
            }
        }

        public static bool TypeIsNumeric(Type t)
        {
            if (t == null) return false;
            TypeCode tc = Type.GetTypeCode(t);
            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
            
        }
    } //end class Utils

    public class DatabaseCodeAttribute : Attribute
    {
        private string _value = null;
        private bool _readonly = false;
        private string _offValue = "";
        public DatabaseCodeAttribute(string code)
        {
            _value = code;
        }

        public DatabaseCodeAttribute(string code, bool readOnly = false, string offValue = "")
        {
            _value = code;
            _readonly = readOnly;
            _offValue = offValue;
        }

        public bool Readonly { get { return _readonly; } }
        public string Code { get { return _value; } }
        public string OffValue { get { return _offValue; } }

        public override string ToString() { return _value; }
    }

    public static class StaticUtils
    {
        /// <summary>
        /// This extension method will make the string safe for injection into an SQL expression.
        /// It will also insert apostrophes (as in "my' || 'string") when replacing special symbols with their CHR codes.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToSQLSafe(this string val)
        {
            if (val == null) return "";
            return val
                .Replace("'", "''")
                .Replace("&", "'||CHR(38)||'")
            ;
        }

        public static string ToJSSafe(this string val)
        {
            if (val == null) return "";
            return val
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\\u", "\\\\u")
                .Replace("\n", "\\n")
            ;
        }

    }

    public static class DataSetExtensions
    {
        /// <summary>
        /// Return String value from DataColumn
        /// </summary>
        /// <param name="columnName">Column Name</param>
        /// <returns>Decimal</returns>
        public static string GetString(this DataRow dr, string columnName)
        {
            try
            {
                return dr[columnName].ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Return Int value from DataColumn
        /// </summary>
        /// <param name="columnName">Column Name</param>
        /// <param name="defaultValue">Default Value returns if passed datacolumn value is null or invalid</param>
        /// <returns>int</returns>
        public static int GetInt(this DataRow dr, string columnName, int defaultValue)
        {
            try
            {
                return (System.DBNull.Value.Equals(dr[columnName])) ? defaultValue : int.Parse(dr[columnName].ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Return Decimal value from DataColumn
        /// </summary>
        /// <param name="columnName">Column Name</param>
        /// <param name="defaultValue">Default Value returns if passed datacolumn value is null or invalid</param>
        /// <returns>Decimal</returns>
        public static Decimal GetDecimal(this DataRow dr, string columnName, Decimal defaultValue)
        {
            try
            {
                return (System.DBNull.Value.Equals(dr[columnName])) ? defaultValue : Decimal.Parse(dr[columnName].ToString());
            }
            catch
            {
                return defaultValue;
            }
        }       
    }

}//end namespace