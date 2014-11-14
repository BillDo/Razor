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

        private static Type Valid_PlainTagHelperType { get { return typeof(Valid_PlainTagHelper); } }

        private static Type Valid_InheritedTagHelperType { get { return typeof(Valid_InheritedTagHelper); } }

        [Theory]
        [InlineData("myType, myAssembly", "myAssembly")]
        [InlineData("myAssembly2", "myAssembly2")]
        public void Resolve_AllowsOverridenResolveDescriptorsInAssembly(string lookupText, string expectedAssemblyName)
        {
            // Arrange
            var tagHelperDescriptorResolver = new OverriddenResolveAssemblyDescriptorResolver();
            var context = new TagHelperDescriptorResolutionContext
            {
                DirectiveDescriptors = new[]
                {
                    new TagHelperDirectiveDescriptor
                    {
                        DirectiveType = TagHelperDirectiveType.AddTagHelper,
                        LookupText = lookupText
                    }
                }
            };

            // Act
            tagHelperDescriptorResolver.Resolve(context);

            // Assert
            Assert.Equal(expectedAssemblyName, tagHelperDescriptorResolver.CalledWithAssemblyName);
        }

        public static TheoryData ResolveDirectiveDescriptorsData
        {
            get
            {
                var assemblyA = AssemblyName;
                var stringType = typeof(string);

                var assemblyB = stringType.GetTypeInfo().Assembly.GetName().Name;
                var stringTagHelperDescriptor = 
                    new TagHelperDescriptor("string", 
                                            "System.String", 
                                            assemblyB, 
                                            ContentBehavior.None);

                return new TheoryData<Dictionary<string, IEnumerable<Type>>, // descriptorAssemblyLookups
                                      IEnumerable<TagHelperDirectiveDescriptor>, // directiveDescriptors
                                      IEnumerable<TagHelperDescriptor>> // expectedDescriptors
                {
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            }
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType } },
                            { assemblyB, new [] { stringType } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyB
                            }
                        },
                        new [] { Valid_PlainTagHelperDescriptor, stringTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType } },
                            { assemblyB, new [] { stringType } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = assemblyB
                            }
                        },
                        new [] { Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                            { assemblyB, new [] { stringType } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyB
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = assemblyA
                            }
                        },
                        new [] { stringTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = Valid_PlainTagHelperType.FullName + ", " + assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            }
                        },
                        new [] { Valid_PlainTagHelperDescriptor, Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } }
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_PlainTagHelperType.FullName + ", " + assemblyA
                            }
                        },
                        new [] { Valid_InheritedTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_PlainTagHelperType.FullName + ", " + assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            }
                        },
                        new [] { Valid_InheritedTagHelperDescriptor, Valid_PlainTagHelperDescriptor }
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
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
            Assert.Equal(expectedDescriptors.Count(), descriptors.Count());

            foreach (var expectedDescriptor in expectedDescriptors)
            {
                Assert.Contains(expectedDescriptor, descriptors, TagHelperDescriptorComparer.Default);
            }
        }

        public static TheoryData ResolveDirectiveDescriptorsData_EmptyResult
        {
            get
            {
                var assemblyA = AssemblyName;
                var stringType = typeof(string);

                var assemblyB = stringType.GetTypeInfo().Assembly.GetName().Name;
                var stringTagHelperDescriptor =
                    new TagHelperDescriptor("string",
                                            "System.String",
                                            assemblyB,
                                            ContentBehavior.None);

                return new TheoryData<Dictionary<string, IEnumerable<Type>>, // descriptorAssemblyLookups
                                      IEnumerable<TagHelperDirectiveDescriptor>, // directiveDescriptors
                                      IEnumerable<TagHelperDescriptor>> // expectedDescriptors
                {
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = assemblyA
                            },
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_PlainTagHelperType.FullName + ", " + assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_InheritedTagHelperType.FullName + ", " + assemblyA
                            }
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                            { assemblyB, new [] { stringType } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyB
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = assemblyB
                            }
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>
                        {
                            { assemblyA, new [] { Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                            { assemblyB, new [] { stringType } },
                        },
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.AddTagHelper,
                                LookupText = assemblyB
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_PlainTagHelperType.FullName + ", " + assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_InheritedTagHelperType.FullName + ", " + assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = stringType.FullName + ", " + assemblyB
                            }
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    },
                    {
                        new Dictionary<string, IEnumerable<Type>>(),
                        new []
                        {
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = assemblyA
                            },
                            new TagHelperDirectiveDescriptor
                            {
                                DirectiveType = TagHelperDirectiveType.RemoveTagHelper,
                                LookupText = Valid_PlainTagHelperType.FullName + ", " + assemblyA
                            },
                        },
                        Enumerable.Empty<TagHelperDescriptor>()
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ResolveDirectiveDescriptorsData_EmptyResult))]
        public void Resolve_CanReturnEmptyDescriptorsBasedOnDirectiveDescriptors(
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
            Assert.Empty(descriptors);
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
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_AbstractTagHelper, " + AssemblyName,
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_GenericTagHelper`, " + AssemblyName,
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_NestedPublicTagHelper, " + AssemblyName,
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_NestedInternalTagHelper, " + AssemblyName,
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_PrivateTagHelper, " + AssemblyName,
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_ProtectedTagHelper, " + AssemblyName,
                "Microsoft.AspNet.Razor.Runtime.Test.TagHelpers.Invalid_InternalTagHelper, " + AssemblyName);

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
            Assert.Empty(descriptors);
        }

        [Fact]
        public void DescriptorResolver_ResolvesOnlyTypeResolverProvidedTypes()
        {
            // Arrange
            var resolver = new TestTagHelperDescriptorResolver(
                new LookupBasedTagHelperTypeResolver(
                    new Dictionary<string, IEnumerable<Type>>(StringComparer.OrdinalIgnoreCase)
                    {
                        { AssemblyName, ValidTestableTagHelpers },
                        {
                            Valid_PlainTagHelperType.FullName + ", " + AssemblyName,
                            new Type[] { Valid_PlainTagHelperType }
                        }
                    }));

            // Act
            var descriptors = resolver.Resolve(Valid_PlainTagHelperType + ", " + AssemblyName);

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
                        { AssemblyName, new Type[]{ Valid_PlainTagHelperType, Valid_InheritedTagHelperType } },
                    }));
            var expectedDescriptors = new TagHelperDescriptor[]
            {
                Valid_PlainTagHelperDescriptor,
                Valid_InheritedTagHelperDescriptor
            };

            // Act
            var descriptors = resolver.Resolve(AssemblyName).ToArray();

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
                        { AssemblyName, ValidTestableTagHelpers },
                        {
                            Valid_PlainTagHelperType.FullName + ", " + AssemblyName,
                            new Type[]{ Valid_PlainTagHelperType }
                        }
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
                        DirectiveDescriptors = lookupTexts.Select(
                            lookupText => new TagHelperDirectiveDescriptor
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

            internal override bool IsTagHelper(TypeInfo typeInfo)
            {
                return true;
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