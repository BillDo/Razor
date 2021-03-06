﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingTest : TagHelperTestBase
    {
        private static IEnumerable<TagHelperDescriptor> PAndInputTagHelperDescriptors
        {
            get
            {
                var pAgePropertyInfo = typeof(TestType).GetProperty("Age");
                var inputTypePropertyInfo = typeof(TestType).GetProperty("Type");
                var checkedPropertyInfo = typeof(TestType).GetProperty("Checked");
                return new[]
                {
                    new TagHelperDescriptor("p",
                                            "PTagHelper",
                                            ContentBehavior.None,
                                            new [] {
                                                new TagHelperAttributeDescriptor("age", pAgePropertyInfo)
                                            }),
                    new TagHelperDescriptor("input",
                                            "InputTagHelper",
                                            ContentBehavior.None,
                                            new TagHelperAttributeDescriptor[] {
                                                new TagHelperAttributeDescriptor("type", inputTypePropertyInfo)
                                            }),
                    new TagHelperDescriptor("input",
                                            "InputTagHelper2",
                                            ContentBehavior.None,
                                            new TagHelperAttributeDescriptor[] {
                                                new TagHelperAttributeDescriptor("type", inputTypePropertyInfo),
                                                new TagHelperAttributeDescriptor("checked", checkedPropertyInfo)
                                            })
                };
            }
        }

        private static IEnumerable<TagHelperDescriptor> ContentBehaviorTagHelperDescriptors
        {
            get
            {
                return new[]
                {
                    new TagHelperDescriptor("modify", "ModifyTagHelper", ContentBehavior.Modify),
                    new TagHelperDescriptor("none", "NoneTagHelper", ContentBehavior.None),
                    new TagHelperDescriptor("append", "AppendTagHelper", ContentBehavior.Append),
                    new TagHelperDescriptor("prepend", "PrependTagHelper", ContentBehavior.Prepend),
                    new TagHelperDescriptor("replace", "ReplaceTagHelper", ContentBehavior.Replace)
                };
            }
        }

        public static TheoryData DesignTimeTagHelperTestData
        {
            get
            {
                // Test resource name, baseline resource name, expected TagHelperDescriptors, expected LineMappings
                return new TheoryData<string, string, IEnumerable<TagHelperDescriptor>, List<LineMapping>>
                {
                    {
                        "SingleTagHelper",
                        "SingleTagHelper.DesignTime",
                        PAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(documentAbsoluteIndex: 14,
                                             documentLineIndex: 0,
                                             generatedAbsoluteIndex: 475,
                                             generatedLineIndex: 15,
                                             characterOffsetIndex: 14,
                                             contentLength: 11)
                        }
                    },
                    {
                        "BasicTagHelpers",
                        "BasicTagHelpers.DesignTime",
                        PAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(documentAbsoluteIndex: 14,
                                             documentLineIndex: 0,
                                             generatedAbsoluteIndex: 475,
                                             generatedLineIndex: 15,
                                             characterOffsetIndex: 14,
                                             contentLength: 11)
                        }
                    },
                    {
                        "ContentBehaviorTagHelpers",
                        "ContentBehaviorTagHelpers.DesignTime",
                        ContentBehaviorTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(documentAbsoluteIndex: 14,
                                             documentLineIndex: 0,
                                             generatedAbsoluteIndex: 495,
                                             generatedLineIndex: 15,
                                             characterOffsetIndex: 14,
                                             contentLength: 11)
                        }
                    },
                    {
                        "ComplexTagHelpers",
                        "ComplexTagHelpers.DesignTime",
                        PAndInputTagHelperDescriptors,
                        new List<LineMapping>
                        {
                            BuildLineMapping(14, 0, 479, 15, 14, 11),
                            BuildLineMapping(30, 2, 1, 995, 35, 0, 48),
                            BuildLineMapping(157, 7, 32, 1177, 45, 6, 12),
                            BuildLineMapping(205, 9, 1260, 50, 0, 12),
                            BuildLineMapping(218, 9, 13, 1356, 56, 12, 27),
                            BuildLineMapping(346, 12, 1754, 68, 0, 48),
                            BuildLineMapping(440, 15, 46, 2004, 78, 6, 8),
                            BuildLineMapping(501, 16, 31, 2384, 88, 6, 30),
                            BuildLineMapping(568, 17, 30, 2733, 97, 0, 10),
                            BuildLineMapping(601, 17, 63, 2815, 103, 0, 8),
                            BuildLineMapping(632, 17, 94, 2895, 109, 0, 1),
                            BuildLineMapping(639, 18, 3149, 118, 0, 15),
                            BuildLineMapping(680, 21, 3234, 124, 0, 1)
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(DesignTimeTagHelperTestData))]
        public void TagHelpers_GenerateExpectedDesignTimeOutput(string testName,
                                                                string baseLineName,
                                                                IEnumerable<TagHelperDescriptor> tagHelperDescriptors,
                                                                List<LineMapping> expectedDesignTimePragmas)
        {
            // Act & Assert
            RunTagHelperTest(testName,
                             baseLineName,
                             designTimeMode: true,
                             tagHelperDescriptors: tagHelperDescriptors,
                             expectedDesignTimePragmas: expectedDesignTimePragmas);
        }

        public static TheoryData RuntimeTimeTagHelperTestData
        {
            get
            {
                // Test resource name, expected TagHelperDescriptors
                // Note: The baseline resource name is equivalent to the test resource name.
                return new TheoryData<string, IEnumerable<TagHelperDescriptor>>
                {
                    { "SingleTagHelper", PAndInputTagHelperDescriptors },
                    { "BasicTagHelpers", PAndInputTagHelperDescriptors },
                    { "BasicTagHelpers.RemoveTagHelper", PAndInputTagHelperDescriptors },
                    { "ContentBehaviorTagHelpers", ContentBehaviorTagHelperDescriptors },
                    { "ComplexTagHelpers", PAndInputTagHelperDescriptors },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RuntimeTimeTagHelperTestData))]
        public void TagHelpers_GenerateExpectedRuntimeOutput(string testName,
                                                             IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
        {
            // Act & Assert
            RunTagHelperTest(testName, tagHelperDescriptors: tagHelperDescriptors);
        }

        [Fact]
        public void CSharpCodeGenerator_CorrectlyGeneratesMappings_ForRemoveTagHelperDirective()
        {
            // Act & Assert
            RunTagHelperTest("RemoveTagHelperDirective",
                             designTimeMode: true,
                             expectedDesignTimePragmas: new List<LineMapping>()
                             {
                                    BuildLineMapping(documentAbsoluteIndex: 17,
                                                     documentLineIndex: 0,
                                                     generatedAbsoluteIndex: 442,
                                                     generatedLineIndex: 14,
                                                     characterOffsetIndex: 17,
                                                     contentLength: 11)
                             });
        }

        [Fact]
        public void CSharpCodeGenerator_CorrectlyGeneratesMappings_ForAddTagHelperDirective()
        {
            // Act & Assert
            RunTagHelperTest("AddTagHelperDirective",
                             designTimeMode: true,
                             expectedDesignTimePragmas: new List<LineMapping>()
                             {
                                    BuildLineMapping(documentAbsoluteIndex: 14,
                                                     documentLineIndex: 0,
                                                     generatedAbsoluteIndex: 433,
                                                     generatedLineIndex: 14,
                                                     characterOffsetIndex: 14,
                                                     contentLength: 11)
                             });
        }

        [Fact]
        public void TagHelpers_Directive_GenerateDesignTimeMappings()
        {
            // Act & Assert
            RunTagHelperTest("AddTagHelperDirective",
                             designTimeMode: true,
                             tagHelperDescriptors: new[] {
                                new TagHelperDescriptor("p", "pTagHelper", ContentBehavior.None)
                             });
        }

        [Theory]
        [InlineData("TagHelpersInSection")]
        [InlineData("TagHelpersInHelper")]
        public void TagHelpers_WithinHelpersAndSections_GeneratesExpectedOutput(string testType)
        {
            // Arrange
            var propertyInfo = typeof(TestType).GetProperty("BoundProperty");
            var tagHelperDescriptors = new TagHelperDescriptor[]
            {
                new TagHelperDescriptor("MyTagHelper",
                                        "MyTagHelper",
                                        ContentBehavior.None,
                                        new [] {
                                            new TagHelperAttributeDescriptor("BoundProperty",
                                                                             propertyInfo)
                                        }),
                new TagHelperDescriptor("NestedTagHelper", "NestedTagHelper", ContentBehavior.Modify)
            };

            // Act & Assert
            RunTagHelperTest(testType, tagHelperDescriptors: tagHelperDescriptors);
        }

        private class TestType
        {
            public int Age { get; set; }

            public string Type { get; set; }

            public bool Checked { get; set; }

            public string BoundProperty { get; set; }
        }
    }
}