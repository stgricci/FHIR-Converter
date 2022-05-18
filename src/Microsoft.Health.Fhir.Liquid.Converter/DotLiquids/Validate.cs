﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using DotLiquid.Util;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using RenderException = Microsoft.Health.Fhir.Liquid.Converter.Exceptions.RenderException;

namespace Microsoft.Health.Fhir.Liquid.Converter.DotLiquids
{
    public class Validate : Block
    {
        private static readonly Regex Syntax = R.B(@"({0}+)\s$", DotLiquid.Liquid.QuotedFragment);

        private string _schemaFileName;
        private List<object> _validateBlock;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                NodeList = _validateBlock = new List<object>();
                var fullSchemaFileName = syntaxMatch.Groups[1].Value;
                _schemaFileName = fullSchemaFileName.Substring(1, fullSchemaFileName.Length - 2);
            }
            else
            {
                throw new SyntaxException(Resources.MergeDiffTagSyntaxError);
            }

            // Initialize block content into variable 'Nodelist'
            // Variable '_diffBlock' will also be initialized since they refer to the same object.
            base.Initialize(tagName, markup, tokens);
        }

        public override void Render(Context context, TextWriter result)
        {
            JSchema validateSchema = LoadValidateSchema(context);

            if (context is JsonContext jsonContext)
            {
                jsonContext.ValidateSchemas.Add(validateSchema);
            }

            using StringWriter writer = new StringWriter();

            context.Stack(() =>
            {
                RenderAll(_validateBlock, context, writer);
            });

            var validateObject = JObject.Parse(writer.ToString());

            // JsonConvert.DeserializeObject<Dictionary<string, object>>(writer.ToString());

            IList<string> messages;
            bool isValid = validateObject.IsValid(validateSchema, out messages);

            if (!isValid)
            {
                throw new RenderException(FhirConverterErrorCode.InvalidValidateBlockContent, string.Join(";", messages));
            }

            result.Write(JsonConvert.SerializeObject(validateObject));
        }

        private JSchema LoadValidateSchema(Context context)
        {
            IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
            string schemaContent = fileSystem.ReadTemplateFile(context, _schemaFileName);

            return JSchema.Parse(schemaContent);
        }
    }
}
