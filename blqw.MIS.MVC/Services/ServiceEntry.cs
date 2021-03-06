﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using blqw.MIS.Services;

namespace blqw.MIS.MVC.Services
{
    public class ServiceEntry : IServiceEntry
    {
        public ServiceEntry(ApiContainer container, string urlTemplate)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Invoker = new DefaultInvoker();
            Resolver = new Resolver();
            Selector = new Selector(container, urlTemplate);
        }

        /// <summary>
        /// 容器
        /// </summary>
        public ApiContainer Container { get; }

        /// <summary>
        /// API执行器
        /// </summary>
        public IInvoker Invoker { get; }

        /// <summary>
        /// 请求解析器
        /// </summary>
        public IResolver Resolver { get; }

        /// <summary>
        /// API选择器
        /// </summary>
        public ISelector Selector { get; }
    }
}
