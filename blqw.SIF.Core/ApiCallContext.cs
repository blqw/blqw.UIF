﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace blqw.SIF
{
    /// <summary>
    /// 当前Api调用上下文
    /// </summary>
    public class ApiCallContext : IResultProvider
    {
        private readonly IResultProvider _resultProvider;
        public ApiCallContext(
            IResultProvider resultProvider,
            object instance,
            IDictionary<string, object> parameters,
            IDictionary<string, object> properties)
        {
            _resultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
            Parameters = parameters ?? new SafeStringDictionary();
            Properties = properties ?? new SafeStringDictionary();
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Data = new SafeStringDictionary();
        }

        public IDictionary<string, object> Parameters { get; }
        public IDictionary<string, object> Properties { get; }
        public IDictionary<string, object> Data { get; }

        public object Instance { get; }
        public MethodInfo Method { get; }
        public object Result => _resultProvider.Result;
        public Exception Exception => _resultProvider.Exception;

        public bool IsError => _resultProvider.IsError;
        
    }
}