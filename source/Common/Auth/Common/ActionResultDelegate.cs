﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Common.Auth.Common
{
    public class ActionResultDelegate : IHttpActionResult
    {
        private readonly IHttpActionResult _next;
        private readonly Func<CancellationToken, IHttpActionResult, Task<HttpResponseMessage>> _func;

        public ActionResultDelegate(
            IHttpActionResult next,
            Func<CancellationToken, IHttpActionResult, Task<HttpResponseMessage>> func)
        {
            _next = next;
            _func = func;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return _func(cancellationToken, _next);
        }
    }
}
