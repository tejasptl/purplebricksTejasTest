using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SNS.CodeLibrary.SharedCode
{

    public struct DescribedEnum
    {
        public static AT AttrValue<AT>(object val)
             where AT : Attribute
        {
            AT res = null; // default(AT);
            if (val == null)
            {
                return null;
            }

            Type T = val.GetType();
            if (!T.IsEnum)
            {
                return null;
            }
            //FieldInfo fi = typeof(T).GetField(_code);
            FieldInfo fi = T.GetField(val.ToString());
            if (fi != null && Attribute.IsDefined(fi, typeof(AT)))
            {
                res = (AT)Attribute.GetCustomAttribute(fi, typeof(AT));
            }
            return res;
        }

        public static bool IsDescribedEnum(Type T)
        {
            return (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(DescribedEnum<>));
        }

        public static object GetDefaultValue(Type E)
        {
            if (E == null || !E.IsEnum)
            {
                return null;
            }
            foreach (string name in Enum.GetNames(E))
            {
                object[] attr = E.GetField(name).GetCustomAttributes(typeof(DefaultValueAttribute), false);
                if (attr.Length == 1
                    && (((DefaultValueAttribute)attr[0]).Value as Boolean? == true))
                {
                    return Enum.Parse(E, name);
                }
            }
            return Enum.GetValues(E).GetValue(0);
        }


        public static string GetEnumDescription(object val)
        {
            if (val == null) return "";

            DescriptionAttribute res = AttrValue<DescriptionAttribute>(val);

            if (res != null)
            {
                return res.Description;
            }

            return val.ToString();
        }

        public static string GetEnumFlagsDescription(object val, string separator = ",")
        {
            if (val == null) return "";
            if (!(val is Enum)) return val.ToString();

            int count = 0;

            Array allVals = Enum.GetValues(val.GetType());
            string[] valDesc = new string[allVals.Length];
            ulong testVal = Convert.ToUInt64(val);

            foreach (object v in allVals)
            {
                if ((Convert.ToUInt64(v) & testVal) != 0)
                {
                    valDesc[count] = GetEnumDescription(v);
                    count++;
                }
            }

            if (count == 0) return "";

            return String.Join(separator, valDesc, 0, count);
        }

        public static string GetDatabaseCode(object val)
        {
            if (val == null) return "";

            DatabaseCodeAttribute ca = AttrValue<DatabaseCodeAttribute>(val);

            if (ca != null)
            {
                return ca.Code;
            }

            return val.ToString();
        }

        public static object GetEnumValue(Type T, string code)
        {
            if (!T.IsEnum)
            {
                return null;
            }
            string res = code;
            bool foundIt = false;
            foreach (object s in Enum.GetValues(T))
            {
                DatabaseCodeAttribute ca = AttrValue<DatabaseCodeAttribute>(s);
                if (ca != null && ca.Code == code)
                {
                    res = s.ToString();
                    foundIt = true;
                    break;
                }
            }
            if (!foundIt && String.IsNullOrEmpty(code))
            {
                return GetDefaultValue(T);
            }
            return Enum.Parse(T, res);
        }

        public static bool IsValidCode(Type T, string code)
        {
            if (!T.IsEnum || code == null)
            {
                return false;
            }
            string res = code;
            foreach (object s in Enum.GetValues(T))
            {
                DatabaseCodeAttribute ca = AttrValue<DatabaseCodeAttribute>(s);
                string sName = s.ToString();
                if (ca != null && ca.Code == code)
                {
                    return true;
                }
                else if (ca == null && code == sName)
                {
                    return true;
                }
            }
            return false;
        }

        public static T GetEnumValue<T>( string code)
        {
            if (!typeof(T).IsEnum)
            {
                return default(T);
            }
            return (T)GetEnumValue(typeof(T), code);
        }

        public static Dictionary<string, string> DictionaryOfValues(Type T, bool excludeDefault = true)
        {
            if (IsDescribedEnum(T))
            {
                return DictionaryOfValues(T.GetGenericArguments()[0], excludeDefault);
            }

            Dictionary<string, string> res = new Dictionary<string, string>();
            if (!T.IsEnum)
            {
                return res;
            }
            foreach (object s in Enum.GetValues(T))
            {
                if (excludeDefault)
                {
                    DefaultValueAttribute va = AttrValue<DefaultValueAttribute>(s);
                    if (va != null && ((va.Value as Boolean?) ?? false))
                    {
                        continue;
                    }
                }
                DatabaseCodeAttribute ca = AttrValue<DatabaseCodeAttribute>(s);
                DescriptionAttribute da = AttrValue<DescriptionAttribute>(s);
                
                string code = (ca == null)? s.ToString() : ca.Code;
                string name = (da == null)? s.ToString() : da.Description;
                res.Add(code, name);
            }
            return res;
        }


    }
    /// <summary>
    /// DescribedEnum is a wrapper for enums that enables specifying free text descriptions to enum values.
    /// It supports the following meta-attributes:
    /// 1) DefaultValue(true) - conversion from a null or empty string produces this value;
    /// 2) Description("some text") - the ToString() method returns this text;
    /// 3) DatabaseCode("some code") - when converting from a string, this code will be used for matching instead of the standard enum name.
    /// 
    /// The wrapper is immutable, meaning that all its properties are read-only.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct DescribedEnum<T> : IComparable<T>, IComparable<DescribedEnum<T>>
        where T : struct
    {
        private T? _value;
        private string _code;
        private static string m_defaultValue = null;

        public static readonly IEnumerable<T> AllValues = Enum.GetValues(typeof(T)).OfType<T>();

        public DescribedEnum(T? value)
        {
            _value = value;
            _code = "";
            if(_value != null)
            {
                _code = AttrString<DatabaseCodeAttribute>(_value.ToString());
            }
        }

        /// <summary>
        /// Conversion from a number follows the same priorities as from a string:
        /// 1) database code
        /// 2) (enum code is not applicable, since enum codes cannot be numbers)
        /// 3) numeric value of enum.
        /// If no match is found, an ArgumentException is thrown
        /// </summary>
        public DescribedEnum(int value)
        {/*
            _value = null;
            _code = "";
            foreach (T val in Enum.GetValues(typeof(T)))
            {
                int testVal = (int)Convert.ChangeType(val, typeof(int));
                if (value == testVal)
                {
                    _value = val;
                    _code = AttrString<DatabaseCodeAttribute>(_value.ToString());
                    break;
                }
            }
          */
            _value = ParseString("" + value);
            if(_value == null)
            {
                throw new ArgumentException("Invalid enum value " + value + " assigned to type " + typeof(T).Name);
            }
            else
            {
                //_code = strValue;
                _code = "";
                _code = AttrString<DatabaseCodeAttribute>(_value.ToString());
            }

        }

        private static T? ParseString(string strValue)
        {
            T? res = null;
            foreach(string name in Enum.GetNames(typeof(T)))
            {
                FieldInfo fi = typeof(T).GetField(name);
                if(Attribute.IsDefined(fi, typeof(DatabaseCodeAttribute))
                    && strValue == Attribute.GetCustomAttribute(fi, typeof(DatabaseCodeAttribute)).ToString())
                {
                    res = (T)Enum.Parse(typeof(T), name);
                }
            }
            if(res == null)
            {
                //_value = (T)Enum.Parse(typeof(T), strValue);
                T testVal;
                int intTestVal;
                if(Enum.IsDefined(typeof(T), strValue) && Enum.TryParse<T>(strValue, true, out testVal))
                {
                    res = testVal;
                }
                else if(Int32.TryParse(strValue, out intTestVal) && Enum.IsDefined(typeof(T), intTestVal))
                {
                    res = (T)Enum.Parse(typeof(T), strValue);
                }
            }
            return res;
        }

        /// <summary>
        /// String is converted into DescribedEnum in the following order of priorities:
        /// 1) database code (DatabaseCode attribute if it is defined);
        /// 2) string enum code;
        /// 3) numeric value of enum.
        /// If no match is found, an ArgumentException is thrown
        /// </summary>
        public DescribedEnum(string strValue)
        {
            strValue = string.IsNullOrWhiteSpace(strValue) ? strValue : strValue.Trim();

            if(strValue != null) // empty string is a valid code value, but null is not.
            {
                _value = ParseString(strValue);
            }
            else
            {
                _value = null;
            }
            if(_value == null)
            {
                if(String.IsNullOrEmpty(strValue))
                {
                    _value = null;
                    _code = "";
                }
                else
                {
                    throw new ArgumentException("Invalid enum value " + strValue + " assigned to type " + typeof(T).Name);
                }
            }
            else
            {
                //_code = strValue;
                _code = "";
                _code = AttrString<DatabaseCodeAttribute>(_value.ToString());
            }
        }

        public static readonly DescribedEnum<T> NullValue = new DescribedEnum<T>();

        public static DescribedEnum<T> DefaultValue
        {
            get
            {
                if(m_defaultValue == null)
                {

                    foreach(string name in Enum.GetNames(typeof(T)))
                    {
                        object[] attr = typeof(T).GetField(name).GetCustomAttributes(typeof(DefaultValueAttribute), false);
                        if(attr.Length == 1
                            && (((DefaultValueAttribute)attr[0]).Value as Boolean? == true))
                        {
                            m_defaultValue = name;
                        }
                    }
                    if(m_defaultValue == null)
                    {
                        m_defaultValue = Enum.GetNames(typeof(T))[0];
                    }
                }
                return (DescribedEnum<T>)m_defaultValue;
            }

        }

        public T? Value { get { return this._value; } }
        public string Code
        {
            get
            {
                return _code;
            }
            /*
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _value = null;
                    _code = "";
                }
                else
                {
                    _value = (T)Enum.Parse(typeof(T), value);
                    _code = value;
                }
            }
             */
        }


        public string Description
        {
            get { return this.ToString(); }
        }

        public static string GetDescription(string s)
        {
            if (!IsValidCode(s))
            {
                return s;
            }
            return ((DescribedEnum<T>)s).Description;
        }

        public override string ToString()
        {
            if(_code == null) return ""; // DefaultValue.ToString();

            DescriptionAttribute res = AttrValue<DescriptionAttribute>();
            if(res == null) return _code;
            return res.Description;
        }

        public AT AttrValue<AT>()
             where AT : Attribute
        {
            AT res = null; // default(AT);
            if(_value != null)
            {
                //FieldInfo fi = typeof(T).GetField(_code);
                FieldInfo fi = typeof(T).GetField(_value.Value.ToString());
                if(Attribute.IsDefined(fi, typeof(AT)))
                {
                    res = (AT)Attribute.GetCustomAttribute(fi, typeof(AT));
                }
                /*
                object[] attr =
                    typeof(T).GetField(_code)
                    .GetCustomAttributes(AT, false);
                if (attr.Length == 1 && attr[0] != null)
                {
                    res = (AT)attr[0];
                }
                 */
            }
            return res;
        }

        public string AttrString<AT>(string defaultValue)
             where AT : Attribute
        {
            string res = defaultValue; // default(AT);
            if(_value != null)
            {
                FieldInfo fi = typeof(T).GetField(_value.Value.ToString());
                if (fi == null)
                {
                    Console.Out.WriteLine("Ooops");
                }
                // (fi == null) means it's an invalid enum value (as any number can be assigned to an enum, we can have invalid enum variables)
                if(fi != null && Attribute.IsDefined(fi, typeof(AT)))
                {
                    AT resObj = (AT)Attribute.GetCustomAttribute(fi, typeof(AT));
                    if(resObj != null)
                    {
                        res = resObj.ToString();
                    }
                }
            }
            return res;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return _value == null;
            }
            else if(obj is DescribedEnum<T>)
            {
                DescribedEnum<T> dobj = (DescribedEnum<T>)obj;
                if(_value == null) return dobj._value == null;
                else return _value.Equals(dobj._value);
            }
            else if(obj is string)
            {
                if(_value == null) return ((string)obj == "");
                else return _value.ToString() == (string)obj;
            }
            else if(obj is T)
            {
                if(_value == null)
                {
                    return ((T)obj).Equals((T)DefaultValue);
                }
                return _value.Equals(obj);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if(_value == null)
                return base.GetHashCode();
            return _value.GetHashCode();
        }

        /// <summary>
        /// This returns all Enum members that have the attribute attributeType defined.
        /// If the fieldName parameter is not null, only the members where (attributeType.fieldName = value) are returned.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<string> ListOfCodesByAttribute(Type attributeType, string fieldName, object value)
        {
            System.Reflection.FieldInfo testField = attributeType.GetField(fieldName);

            List<string> res = new List<string>();
            if(testField != null)
            {
                foreach(string s in Enum.GetNames(typeof(T)))
                {
                    object[] attr =
                        typeof(T).GetField(s)
                        .GetCustomAttributes(attributeType, false);
                    if(attr.Length == 1)
                    {
                        object attrVal = testField.GetValue(attr[0]);
                        if(Object.Equals(attrVal, value))
                        {
                            DescribedEnum<T> el = (DescribedEnum<T>)s;
                            res.Add(el.Code);
                        }
                    }
                }
            }
            return res;
        }

        public static Dictionary<string, string> ListOfCodesByAttribute(Type attributeType, string value)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            foreach(string s in Enum.GetNames(typeof(T)))
            {
                object[] attr =
                    typeof(T).GetField(s)
                    .GetCustomAttributes(attributeType, false);
                if(attr.Length == 1)
                {
                    if((attr[0] == null && value == null)
                        || (attr[0].ToString() == value))
                    {
                        DescribedEnum<T> el = (DescribedEnum<T>)s;
                        res.Add(el.Code, el);
                    }
                }
            }
            return res;
        }

        public static List<string> ListOfCodes(bool excludeDefault = true)
        {
            string defaultValueName = DefaultValue.Code;
            List<string> res = new List<string>();
            foreach(string s in Enum.GetNames(typeof(T)))
            {
                DescribedEnum<T> el = (DescribedEnum<T>)s;
                if (!excludeDefault || el.Code != defaultValueName)
                {
                    res.Add(el.Code);
                }
            }
            return res;
        }

        public static Dictionary<string, string> DictionaryOfValues(bool excludeDefault = true, bool codeAsNumber = false)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            string defaultValueName = null;
            if(DefaultValue.Value != null)
            {
                defaultValueName = ((T)DefaultValue.Value).ToString();
            }
            foreach(string s in Enum.GetNames(typeof(T)))
            {
                if(!excludeDefault || s != defaultValueName)
                {
                    DescribedEnum<T> el = (DescribedEnum<T>)s;
                    if (codeAsNumber)
                    {
                        if (el.Value == null)
                        {
                            res.Add("", el);
                        }
                        else
                        {
                            int t = Convert.ToInt32(el.Value);
                            res.Add("" + t, el);
                        }
                    }
                    else
                    {
                        res.Add(el.Code, el);
                    }
                }
            }
            return res;
        }

        public static bool IsValidCode(string value)
        {
            if(String.IsNullOrEmpty(value))
                return true;

            return (ParseString(value) != null);
        }

        public static implicit operator DescribedEnum<T>(T value)
        {
            return new DescribedEnum<T>(value);
        }

        public static string GetCode(T? value)
        {
            if (value == null) return DefaultValue.Code;

            return ((DescribedEnum<T>)(T)value).Code;
        }

        public static implicit operator T(DescribedEnum<T> value)
        {
            if(value._value == null) return (T)(DescribedEnum<T>.DefaultValue._value);
            return (T)value._value;
        }

        public static implicit operator string(DescribedEnum<T> value)
        {
            return value.ToString();
        }

        public static implicit operator int(DescribedEnum<T> value)
        {
            if(value._value == null) return (int)Convert.ChangeType(DescribedEnum<T>.DefaultValue._value, typeof(int));
            return (int)Convert.ChangeType(value._value, typeof(int));
        }

        public static implicit operator DescribedEnum<T>(string value)
        {
            if(String.IsNullOrEmpty(value))
                return DefaultValue;
            //return (T)Enum.Parse(typeof(T), value);
            return new DescribedEnum<T>(value);
        }

        public static implicit operator DescribedEnum<T>(int value)
        {
            //T valT;
            //
            //if (!Enum.TryParse<T>(value.ToString(), out valT))
            //    return DefaultValue;
            //return valT;
            return new DescribedEnum<T>(value);
        }

        public static bool operator ==(DescribedEnum<T> obj, object value)
        {
            if((object)obj == null) return value == null;
            return obj.Equals(value);
        }
        public static bool operator !=(DescribedEnum<T> obj, object value)
        {
            if((object)obj == null) return value != null;
            return !obj.Equals(value);
        }

        public int CompareTo(T other)
        {
            if (this.Value == null) return -1;
            return ((IComparable)this.Value.Value).CompareTo(other);
        }

        public int CompareTo(DescribedEnum<T> other)
        {
            if (other == null || other.Value == null) return 1;
            return CompareTo(other.Value.Value);
        }
    }

}//end namespace

