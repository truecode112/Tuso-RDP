﻿using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Immense.RemoteControl.Server.Abstractions
{
    public interface IViewerAuthorizer
    {
        /// <summary>
        /// Example: "/Account/Login"
        /// </summary>
        string UnauthorizedRedirectUrl { get; }
        bool IsAuthorized(AuthorizationFilterContext context);
    }
}
