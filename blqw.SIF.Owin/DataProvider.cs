﻿using blqw.SIF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using blqw.SIF.Descriptor;
using Microsoft.Owin;
using System.IO;
using blqw.IOC;
using System.Diagnostics;
using System.Collections;

namespace blqw.SIF.Owin
{
    class DataProvider : IApiDataProvider
    {
        IReadableStringCollection _query;
        IReadableStringCollection _body;
        IHeaderDictionary _header;
        IOwinContext _context;

        public DataProvider(IOwinContext context)
        {
            _context = _context ?? throw new ArgumentNullException(nameof(_context));
            _query = _context.Request.Query;
            _header = _context.Request.Headers;
            _body = ParseBody(_context);
        }

        /// <summary>
        /// 解析正文
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IReadableStringCollection ParseBody(IOwinContext context)
        {
            var contentType = context.Request.ContentType;
            if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                return context.Request.ReadFormAsync().Result;
            }
            if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                //throw new NotSupportedException("不支持的ContentType");
                return context.Request.ReadFormAsync().Result;
            }

            var format = GetFormat(context.Request.ContentType)?.ToLowerInvariant();
            if (format == "json")
            {
                var bytes = ReadAll(context.Request.Body);
                var encoding = GetEncoding(context.Request.ContentType) ?? Encoding.UTF8;
                var json = encoding.GetString(bytes);
                var obj = Json.ToObject(json);
                return new InnerBody(obj);
            }
            throw new NotSupportedException("不支持的ContentType");
        }


        private class InnerBody : IReadableStringCollection
        {
            private object _obj;
            private IDictionary _dict;

            public InnerBody(object obj)
            {
                _obj = obj;
                _dict = obj as IDictionary;
            }

            public string this[string key]
            {
                get
                {
                    if (_dict.Contains(key))
                    {
                        return _dict[key]?.ToString();
                    }
                    return null;
                }
            }

            public string Get(string key)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public IList<string> GetValues(string key)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        static char[] _ContentTypeSeparator = new[] { ' ', ';' };
        private string GetFormat(string contentType)
        {
            if (contentType == null)
            {
                return null;
            }
            var i = contentType.IndexOf('/');
            if (i < 0)
            {
                return null;
            }
            i++;
            var e = contentType.IndexOfAny(_ContentTypeSeparator);
            if (e < 0)
            {
                e = contentType.Length;
            }
            return contentType.Substring(i, e - i);
        }

        private Encoding GetEncoding(string contentType)
        {
            if (contentType == null)
            {
                return null;
            }
            var i = contentType.IndexOf("charset=");
            if (i < 0)
            {
                return null;
            }
            i++;
            var e = contentType.IndexOfAny(_ContentTypeSeparator);
            if (e < 0)
            {
                e = contentType.Length;
            }
            try
            {
                return Encoding.GetEncoding(contentType.Substring(i, e - i));
            }
            catch (Exception ex)
            {
                Logger.Source.Write(TraceEventType.Error, $"未知的编码名称'{contentType.Substring(i, e - i)}'", ex);
                throw;
            }
        }

        /// <summary>
        /// 字符串缓冲
        /// </summary>
        [ThreadStatic]
        private static byte[] _Buffer;
        /// <summary>
        /// 读取流中的所有字节
        /// </summary>
        /// <param name="stream"> </param>
        private static byte[] ReadAll(Stream stream)
        {
            var length = 1024;
            if (_Buffer == null)
            {
                _Buffer = new byte[length];
            }
            length = _Buffer.Length;
            var bytes = new List<byte>();
            int count;
            do
            {
                if ((count = stream.Read(_Buffer, 0, length)) == length)
                {
                    bytes.AddRange(_Buffer);
                }
                else
                {
                    bytes.AddRange(_Buffer.Take(count));
                }
            } while (count > 0);
            return bytes.ToArray();
        }

        public ApiData GetParameter(ApiParameterDescriptor parameter)
        {
            var value = (object)_body.Get(parameter.Name) ?? _query.Get(parameter.Name);
            if (value == null)
            {
                return ApiData.NotFound;
            }
            try
            {
                value = value.ChangeType(parameter.ParameterType);
                return new ApiData(value);
            }
            catch (Exception ex)
            {
                return new ApiData(ex);
            }
        }

        public ApiData GetProperty(ApiPropertyDescriptor property)
        {
            var value = (object)_header.Get(property.Name);
            if (value == null)
            {
                return ApiData.NotFound;
            }
            try
            {
                value = value.ChangeType(property.PropertyType);
                return new ApiData(value);
            }
            catch (Exception ex)
            {
                return new ApiData(ex);
            }
        }

    }
}