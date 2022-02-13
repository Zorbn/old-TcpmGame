using Raylib_cs;

namespace Client;

public static class SpriteShader
{
    public static Shader Shader;
    private static int FlashAmountLoc;
    private static int TextureSizeLoc;
    
    public static void LoadSpriteShader()
    {
        Shader = Raylib.LoadShader(null, "Resources/SpriteShader.frag");

        float outlineSize = 1f;
        float[] outlineColor = { 0f, 0f, 0f, 1f };

        int outlineSizeLoc = Raylib.GetShaderLocation(Shader, "outlineSize");
        int outlineColorLoc = Raylib.GetShaderLocation(Shader, "outlineColor");

        Raylib.SetShaderValue(Shader, outlineSizeLoc, outlineSize, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        Raylib.SetShaderValue(Shader, outlineColorLoc, outlineColor, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        
        FlashAmountLoc = Raylib.GetShaderLocation(Shader, "flashAmount");
        TextureSizeLoc = Raylib.GetShaderLocation(Shader, "textureSize");
    }

    public static void BeginSpriteShaderMode(float width, float height, float flashAmount)
    {
        Raylib.SetShaderValue(Shader, FlashAmountLoc, flashAmount,
            ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        Raylib.SetShaderValue(Shader, TextureSizeLoc,
            new[] { width, height },
            ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            
        Raylib.BeginShaderMode(Shader);
    }
}