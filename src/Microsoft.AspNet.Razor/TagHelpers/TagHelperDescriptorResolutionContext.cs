// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Contains information needed to resolve <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public class TagHelperDescriptorResolutionContext
    {
        /// <summary>
        /// <see cref="TagHelperDirectiveDescriptor"/>s used to resolve <see cref="TagHelperDescriptor"/>s.
        /// </summary>        
        public IEnumerable<TagHelperDirectiveDescriptor> DirectiveDescriptors { get; set; }
    }
}