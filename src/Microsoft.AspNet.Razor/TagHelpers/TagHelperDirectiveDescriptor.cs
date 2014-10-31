// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Contains information needed to resolve <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public class TagHelperDirectiveDescriptor
    {
        /// <summary>
        /// A <see cref="string"/> used to find tag helper <see cref="System.Type"/>s.
        /// </summary>
        public string LookupText { get; set; }

        /// <summary>
        /// The <see cref="TagHelperDirectiveType"/> of this directive.
        /// </summary>
        public TagHelperDirectiveType DirectiveType { get; set;  }
    }
}