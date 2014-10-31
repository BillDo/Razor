// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelperDescriptorResolverTest : TagHelperTypeResolverTest
    {
        private static readonly string AssemblyName =
            typeof(TagHelperDescriptorFactoryTest).GetTypeInfo().Assembly.GetName().Name;

        private static TagHelperDescriptor Valid_PlainTagHelperDescriptor
        {
            get
            {
                return new TagHelperDescriptor("Valid_Plain",
                                               typeof(Valid_PlainTagHelper).FullName,
                                               AssemblyName,
                                               ContentBehavior.None);
            }
        }

        private static TagHelperDescriptor Valid_InheritedTagHelperDescriptor
        {
            get
            {
                return new TagHelperDescriptor("Valid_Inherited",
                                               typeof(Valid_InheritedTagHelper).FullName,
                                               AssemblyName,
                                               ContentBehavior.None);
            }
        }

        [Theory]
        [InlineData("myType, myAssembly", "myAssembly")]
        [InlineData("myAssembly2", "myAssembly2")]
        public void Resolve_AllowsOverridenResolveDescriptorsInAssembly(string lookupText, string expectedAssemblyName)
        {
            // Arrange
            var tagHelperDescriptorResolver = new OverriddenResolveAssemblyDescriptorResolver();

            // Act
            tagHelperDescriptorResolver.Resolve(lookupText);

            // Assert
            Assert.Equal(expectedAssemblyName, tagHelperDescriptorResolver.CalledWithAssemblyName);
        }

        public static TheoryData ResolveDirectiveDescriptorsData
        {
            get
            {
                return new TheoryData<Dictionary<string, IEnumerable<Type>>, // descriptorAssemblyLookups
                                      IEnumerable<TagHelperDirectiveDescriptor>, // directiveDescriptors
                                      IEnumerable<TagHelperDescriptor>> // expectedDescriptors
                {
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper) } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" }
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper) } },
                            { "lookup2", new [] { typeof(Valid_InheritedTagHelper) } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup2" }
                        },
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper) } },
                            { "lookup2", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup2" }
                        },
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                            { "lookup2", new [] { typeof(Valid_PlainTagHelper) } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.RemoveTagHelper, LookupText = "lookup2" }
                        },
                        new [] { Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                            { "lookup2", new [] { typeof(Valid_PlainTagHelper) } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.RemoveTagHelper, LookupText = "lookup2" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup2" }
                        },
                        new [] { Valid_InheritedTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper) } },
                            { "lookup2", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.RemoveTagHelper, LookupText = "lookup2" },
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.RemoveTagHelper, LookupText = "lookup1" },
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                            { "lookup2", new [] { typeof(Valid_PlainTagHelper) } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.RemoveTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.RemoveTagHelper, LookupText = "lookup2" },
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { "lookup1", new [] { typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                            new TagHelperDirectiveDescriptor { DirectiveType = TagHelperDirectiveType.AddTagHelper, LookupText = "lookup1" },
                        },
                        new [] { Valid_InheritedTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ResolveDirectiveDescriptorsData))]
        public void Resolve_ReturnsDescriptorsBasedOnDirectiveDescriptors(
            Dictionary<string, IEnumerable<Type>> descriptorAssemblyLookups,
            IEnumerable<TagHelperDirectiveDescriptor> directiveDescriptors,
            IEnumerable<TagHelperDescriptor> expectedDescriptors)
        {
            // Arrange
            var tagHelperDescriptorResolver =
                new TestTagHelperDescriptorResolver(
                    new LookupBasedTagHelperTypeResolver(descriptorAssemblyLookups));
            var resolutionContext = new TagHelperDescriptorResolutionContext
            {
                DirectiveDescriptors = directiveDescriptors
            };

            // Act
            var descriptors = tagHelperDescriptorResolver.Resolve(resolutionContext);

            // Assert
            if (expectedDescriptors.Any())
            {
                foreach (var expectedDescriptor in expectedDescriptors)
                {
                    Assert.Contains(expectedDescriptor, descriptors, TagHelperDescriptorComparer.Default);
                }
            }
            else
            {
                Assert.Empty(descriptors);
            }
        }

        [Fact]
        public void DescriptorResolver_DoesNotReturnInvalidTagHelpersWhenSpecified()
        {
            // Arrange
            var tagHelperDescriptorResolver =
                new TestTagHelperDescriptorResolver(
                    new TestTagHelperTypeResolver(TestableTagHelpers));

            // Act
            var descriptors = tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_AbstractTagHelper, MyAssembly");
            descriptors = descriptors.Concat(tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_GenericTagHelper`, MyAssembly"));
            descriptors = descriptors.Concat(tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_NestedPublicTagHelper, MyAssembly"));
            descriptors = descriptors.Concat(tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_NestedInternalTagHelper, MyAssembly"));
            descriptors = descriptors.Concat(tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_PrivateTagHelper, MyAssembly"));
            descriptors = descriptors.Concat(tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_ProtectedTagHelper, MyAssembly"));
            descriptors = descriptors.Concat(tagHelperDescriptorResolver.Resolve(
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_InternalTagHelper, MyAssembly"));

            // Assert
            Assert.Empty(descriptors);
        }

        [Theory]
        [InlineData("Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper,MyAssembly")]
        [InlineData("    Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper,MyAssembly")]
        [InlineData("Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper    ,MyAssembly")]
        [InlineData("    Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper    ,MyAssembly")]
        [InlineData("Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper,    MyAssembly")]
        [InlineData("Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper,MyAssembly    ")]
        [InlineData("Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper,    MyAssembly    ")]
        [InlineData("    Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper,    MyAssembly    ")]
        [InlineData("    Microsoft.AspNet.Razor.Runtime.TagHelpers.Valid_PlainTagHelper    ,    MyAssembly    ")]
        public void DescriptorResolver_IgnoresSpaces(string lookupText)
        {
            // Arrange
            var tagHelperTypeResolver = new TestTagHelperTypeResolver(TestableTagHelpers)
            {
                OnGetLibraryDefinedTypes = (assemblyName) =>
                {
                    Assert.Equal("MyAssembly", assemblyName.Name);
                }
            };
            var tagHelperDescriptorResolver = new TestTagHelperDescriptorResolver(tagHelperTypeResolver);

            // Act
            var descriptors = tagHelperDescriptorResolver.Resolve(lookupText);

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(Valid_PlainTagHelperDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void DescriptorResolver_ResolvesOnlyTypeResolverProvidedTypes()
        {
            // Arrange
            var resolver = new TestTagHelperDescriptorResolver(
                new LookupBasedTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText1", ValidTestableTagHelpers },
                        { "lookupText2", new Type[]{ typeof(Valid_PlainTagHelper) } }
                    }));

            // Act
            var descriptors = resolver.Resolve("lookupText2");

            // Assert
            var descriptor = Assert.Single(descriptors);
            Assert.Equal(Valid_PlainTagHelperDescriptor, descriptor, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void DescriptorResolver_ResolvesMultipleTypes()
        {
            // Arrange
            var resolver = new TestTagHelperDescriptorResolver(
                new LookupBasedTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText", new Type[]{ typeof(Valid_PlainTagHelper), typeof(Valid_InheritedTagHelper) } },
                    }));
            var expectedDescriptors = new TagHelperDescriptor[]
            {
                Valid_PlainTagHelperDescriptor,
                Valid_InheritedTagHelperDescriptor
            };

            // Act
            var descriptors = resolver.Resolve("lookupText").ToArray();

            // Assert
            Assert.Equal(descriptors.Length, 2);
            Assert.Equal(expectedDescriptors, descriptors, CompleteTagHelperDescriptorComparer.Default);
        }

        [Fact]
        public void DescriptorResolver_DoesNotResolveTypesForNoTypeResolvingLookupText()
        {
            // Arrange
            var resolver = new TestTagHelperDescriptorResolver(
                new LookupBasedTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "lookupText1", ValidTestableTagHelpers },
                        { "lookupText2", new Type[]{ typeof(Valid_PlainTagHelper) } }
                    }));

            // Act
            var descriptors = resolver.Resolve("lookupText").ToArray();

            // Assert
            Assert.Empty(descriptors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void DescriptorResolver_ResolveThrowsIfNullOrEmptyLookupText(string lookupText)
        {
            // Arrange
            var tagHelperDescriptorResolver =
                new TestTagHelperDescriptorResolver(
                    new TestTagHelperTypeResolver(InvalidTestableTagHelpers));

            var expectedMessage =
                Resources.FormatTagHelperDescriptorResolver_InvalidTagHelperLookupText(lookupText) +
                Environment.NewLine +
                "Parameter name: lookupText";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(nameof(lookupText),
            () =>
            {
                tagHelperDescriptorResolver.Resolve(lookupText);
            });

            Assert.Equal(expectedMessage, ex.Message);
        }

        private class TestTagHelperDescriptorResolver : TagHelperDescriptorResolver
        {
            public TestTagHelperDescriptorResolver(TagHelperTypeResolver typeResolver)
                : base(typeResolver)
            {
            }

            public IEnumerable<TagHelperDescriptor> Resolve(params string[] lookupTexts)
            {
                return Resolve(
                    new TagHelperDescriptorResolutionContext
                    {
                        DirectiveDescriptors = lookupTexts.Select(lookupText =>
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = lookupText
                            })
                    });
            }
        }

        private class LookupBasedTagHelperTypeResolver : TagHelperTypeResolver
        {
            private Dictionary<string, IEnumerable<Type>> _lookupValues;

            public LookupBasedTagHelperTypeResolver(Dictionary<string, IEnumerable<Type>> lookupValues)
            {
                _lookupValues = lookupValues;
            }

            internal override IEnumerable<TypeInfo> GetLibraryDefinedTypes(AssemblyName assemblyName)
            {
                IEnumerable<Type> types;

                _lookupValues.TryGetValue(assemblyName.Name, out types);

                return types?.Select(type => type.GetTypeInfo()) ?? Enumerable.Empty<TypeInfo>();
            }
        }

        private class OverriddenResolveAssemblyDescriptorResolver : TagHelperDescriptorResolver
        {
            public string CalledWithAssemblyName { get; set; }

            protected override IEnumerable<TagHelperDescriptor> ResolveDescriptorsInAssembly(string assemblyName)
            {
                CalledWithAssemblyName = assemblyName;

                return Enumerable.Empty<TagHelperDescriptor>();
            }
        }
    }
}