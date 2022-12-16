using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using ShaderGen;
using ShaderGen.Glsl;
using ShaderGen.Hlsl;
using ShaderGen.Metal;

namespace Vent.Engine.Generators;

[Generator]
public class ShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilation = context.CompilationProvider.Select(static (c, _) => c);

        context.RegisterSourceOutput(compilation, static (c, compilation) =>
        {
            var         hlsl        = new HlslBackend(compilation);
            var         glsl330     = new Glsl330Backend(compilation);
            var         glsles300   = new GlslEs300Backend(compilation);
            var         glsl450     = new Glsl450Backend(compilation);
            var         metal       = new MetalBackend(compilation);
            
            var languages = new LanguageBackend[]
            {
                hlsl,
                glsl330,
                glsles300,
                glsl450,
                metal,
            };
                
            var generator = new ShaderGen.ShaderGenerator(compilation, languages);
            var result    = generator.GenerateShaders();
            
            foreach (var language in languages)
            {
                var extension = BackendExtension(language);
                var    sets      = result.GetOutput(language);
            
                foreach (var set in sets)
                {
                    var name = set.Name;
                    if (set.VertexShaderCode != null)
                    {
                        name = name + "-vertex." + extension;
                        c.AddSource(name, ToCommented(set.VertexShaderCode));
                    }
                    if (set.FragmentShaderCode != null)
                    {
                        name = name + "-fragment." + extension;
                        c.AddSource(name, ToCommented(set.FragmentShaderCode));
                    }
                    if (set.ComputeShaderCode != null)
                    {
                        name = name + "-compute." + extension;
                        c.AddSource(name, ToCommented(set.ComputeShaderCode));
                    }
                }
            }
        });
    }

    private static string ToCommented(string source) =>
        string.Join(
            "\n",
            source
                .Split(new[] {Environment.NewLine}, StringSplitOptions.None)
                .Select(line => $"// {line}")
                .ToArray());

    private static string BackendExtension(LanguageBackend language)
    {
        if (language.GetType() == typeof(HlslBackend))
            return "hlsl";
        if (language.GetType() == typeof(Glsl330Backend))
            return "330.glsl";
        if (language.GetType() == typeof(GlslEs300Backend))
            return "300.glsles";
        if (language.GetType() == typeof(Glsl450Backend))
            return "450.glsl";
        if (language.GetType() == typeof(MetalBackend))
            return "metal";

        throw new InvalidOperationException("Invalid backend type: " + language.GetType().Name);
    }
}