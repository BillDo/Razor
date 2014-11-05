// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers.Internal
{
    public static class TagHelperBlockRewriter
    {
        public static TagHelperBlockBuilder Rewrite(string tagName,
                                                    bool validStructure,
                                                    Block tag,
                                                    IEnumerable<TagHelperDescriptor> descriptors,
                                                    RewritingContext context)
        {
            // There will always be at least one child for the '<'.
            var start = tag.Children.First().Start;
            var attributes = GetTagAttributes(tagName, validStructure, tag, context);

            return new TagHelperBlockBuilder(tagName, start, attributes, descriptors);
        }

        private static IDictionary<string, SyntaxTreeNode> GetTagAttributes(string tagName,
                                                                            bool validStructure, 
                                                                            Block tagBlock, 
                                                                            RewritingContext context)
        {
            var attributes = new Dictionary<string, SyntaxTreeNode>(StringComparer.OrdinalIgnoreCase);

            // We skip the first child "<tagname" and take everything up to the ending portion of the tag ">" or "/>".
            // The -2 accounts for both the start and end tags. If the tag does not have a valid structure then there's
            // no end tag to ignore.
            var symbolOffset = validStructure ? 2 : 1;
            var attributeChildren = tagBlock.Children.Skip(1).Take(tagBlock.Children.Count() - symbolOffset);

            foreach (var child in attributeChildren)
            {
                KeyValuePair<string, SyntaxTreeNode>? attribute;

                if (child.IsBlock)
                {
                    attribute = ParseBlock(tagName, (Block)child, context);
                }
                else
                {
                    attribute = ParseSpan((Span)child);

                    var wrapperBlock = new BlockBuilder
                    {
                        Type = BlockType.Markup,
                        CodeGenerator = BlockCodeGenerator.Null
                    };

                    wrapperBlock.Children.Add(attribute.Value.Value);
                    attribute = new KeyValuePair<string, SyntaxTreeNode>(attribute.Value.Key, wrapperBlock.Build());
                }

                // Attribute will not have a value if the attribute was unable to be parsed
                if (attribute.HasValue)
                {
                    attributes.Add(attribute.Value.Key, attribute.Value.Value);
                }
            }

            return attributes;
        }

        // This method handles cases when the attribute is a simple span attribute such as
        // class="something moresomething".  This does not handle complex attributes such as
        // class="@myclass". Therefore the span.Content is equivalent to the entire attribute.
        private static KeyValuePair<string, SyntaxTreeNode>? ParseSpan(Span span)
        {
            var afterEquals = false;
            var builder = new SpanBuilder
            {
                CodeGenerator = span.CodeGenerator,
                EditHandler = span.EditHandler,
                Kind = span.Kind
            };
            var htmlSymbols = span.Symbols.OfType<HtmlSymbol>().ToArray();
            var symbolOffset = 1;
            string name = null;

            // Iterate down through the symbols to find the name and the start of the value.
            // We subtract the symbolOffset so we don't accept an ending quote of a span.
            for (var i = 0; i < htmlSymbols.Length - symbolOffset; i++)
            {
                var symbol = htmlSymbols[i];

                if (name == null && symbol.Type == HtmlSymbolType.Text)
                {
                    name = symbol.Content;
                }
                else if (afterEquals)
                {
                    builder.Accept(symbol);
                }
                else if (symbol.Type == HtmlSymbolType.Equals)
                {
                    // We've found an '=' symbol, this means that the coming symbols will either be a quote
                    // or value (in the case that the value is unquoted).
                    // Spaces after/before the equal symbol are not yet supported: 
                    // https://github.com/aspnet/Razor/issues/123

                    // TODO: Support no attribute values: https://github.com/aspnet/Razor/issues/220

                    // Check for attribute start values, aka single or double quote
                    if (IsQuote(htmlSymbols[i + 1]))
                    {
                        // Move past the attribute start so we can accept the true value.
                        i++;
                    }
                    else
                    {
                        // Set the symbol offset to 0 so we don't attempt to skip an end quote that doesn't exist.
                        symbolOffset = 0;
                    }

                    afterEquals = true;
                }
            }

            return new KeyValuePair<string, SyntaxTreeNode>(name, builder.Build());
        }

        private static KeyValuePair<string, SyntaxTreeNode>? ParseBlock(string tagName, 
                                                                        Block block, 
                                                                        RewritingContext context)
        {
            // TODO: Accept more than just spans: https://github.com/aspnet/Razor/issues/96.
            // The first child will only ever NOT be a Span if a user is doing something like:
            // <input @checked />

            var childSpan = block.Children.First() as Span;

            if (childSpan == null || childSpan.Kind != SpanKind.Markup)
            {
                context.OnError(block.Children.First().Start, 
                                RazorResources.FormatTagHelpers_CannotHaveCSharpInTagDeclaration(tagName));

                return null;
            }

            var builder = new BlockBuilder(block);

            // If there's only 1 child it means that it's plain text inside of the attribute.
            // i.e. <div class="plain text in attribute">
            if (builder.Children.Count == 1)
            {
                return ParseSpan(childSpan);
            }

            var textSymbol = childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text);
            var name = textSymbol != null ? textSymbol.Content : null;

            if (name == null)
            {
                context.OnError(childSpan.Start, RazorResources.FormatTagHelpers_AttributesMustHaveAName(tagName));

                return null;
            }

            // TODO: Support no attribute values: https://github.com/aspnet/Razor/issues/220

            // Remove first child i.e. foo="
            builder.Children.RemoveAt(0);

            // Grabbing last child to check if the attribute value is quoted.
            var endNode = block.Children.Last();
            if (!endNode.IsBlock)
            {
                var endSpan = (Span)endNode;
                var endSymbol = (HtmlSymbol)endSpan.Symbols.Last();

                // Checking to see if it's a quoted attribute, if so we should remove end quote
                if (IsQuote(endSymbol))
                {
                    builder.Children.RemoveAt(builder.Children.Count - 1);
                }
            }

            // We need to rebuild the code generators of the builder and its children (this is needed to
            // ensure we don't do special attribute code generation since this is a tag helper).
            block = RebuildCodeGenerators(builder.Build());

            return new KeyValuePair<string, SyntaxTreeNode>(name, block);
        }

        private static Block RebuildCodeGenerators(Block block)
        {
            var builder = new BlockBuilder(block);

            var isDynamic = builder.CodeGenerator is DynamicAttributeBlockCodeGenerator;

            // We don't want any attribute specific logic here, null out the block code generator.
            if (isDynamic || builder.CodeGenerator is AttributeBlockCodeGenerator)
            {
                builder.CodeGenerator = BlockCodeGenerator.Null;
            }

            for (var i = 0; i < builder.Children.Count; i++)
            {
                var child = builder.Children[i];

                if (child.IsBlock)
                {
                    // The child is a block, recurse down into the block to rebuild its children
                    builder.Children[i] = RebuildCodeGenerators((Block)child);
                }
                else
                {
                    var childSpan = (Span)child;
                    ISpanCodeGenerator newCodeGenerator = null;
                    var literalGenerator = childSpan.CodeGenerator as LiteralAttributeCodeGenerator;

                    if (literalGenerator != null)
                    {
                        if (literalGenerator.ValueGenerator == null || literalGenerator.ValueGenerator.Value == null)
                        {
                            newCodeGenerator = new MarkupCodeGenerator();
                        }
                        else
                        {
                            newCodeGenerator = literalGenerator.ValueGenerator.Value;
                        }
                    }
                    else if (isDynamic && childSpan.CodeGenerator == SpanCodeGenerator.Null)
                    {
                        // Usually the dynamic code generator handles rendering the null code generators underneath
                        // it. This doesn't make sense in terms of tag helpers though, we need to change null code 
                        // generators to markup code generators.

                        newCodeGenerator = new MarkupCodeGenerator();
                    }

                    // If we have a new code generator we'll need to re-build the child
                    if (newCodeGenerator != null)
                    {
                        var childSpanBuilder = new SpanBuilder(childSpan)
                        {
                            CodeGenerator = newCodeGenerator
                        };

                        builder.Children[i] = childSpanBuilder.Build();
                    }
                }
            }

            return builder.Build();
        }

        private static bool IsQuote(HtmlSymbol htmlSymbol)
        {
            return htmlSymbol.Type == HtmlSymbolType.DoubleQuote ||
                   htmlSymbol.Type == HtmlSymbolType.SingleQuote;
        }
    }
}