﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DotLiquid;
using Newtonsoft.Json.Schema;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    public class JsonContext : Context
    {
        public JsonContext(List<Hash> environments, Hash outerScope, Hash registers, ErrorsOutputMode errorsOutputMode, int maxIterations, int timeout, IFormatProvider formatProvider)
             : base(environments, outerScope, registers, errorsOutputMode, maxIterations, timeout, formatProvider)
        {
        }

        public List<JSchema> ValidateSchemas { get; set; }
    }
}