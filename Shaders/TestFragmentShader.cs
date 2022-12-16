using ShaderGen;
using System.Numerics;

namespace TestShaders
{
    public class FragmentShader
    {
        [FragmentShader]
        public Vector4 FS(VertexOutput input) => input.Color;

        public struct VertexOutput
        {
            [VertexSemantic(SemanticType.SystemPosition)]
            public Vector4 Position;
            [VertexSemantic(SemanticType.Color)]
            public Vector4 Color;
        }
    }
}
