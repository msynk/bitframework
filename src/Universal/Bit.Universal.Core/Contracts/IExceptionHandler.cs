﻿using System;
using System.Collections.Generic;

namespace Bit.Core.Contracts
{
    public interface IExceptionHandler
    {
        void OnExceptionReceived(Exception exp, IDictionary<string, string?>? properties = null);

        void OnExceptionReceived(Exception exp, params (string key, string? value)[] properties);
    }
}
