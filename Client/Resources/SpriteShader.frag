#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform vec2 textureSize;
uniform float outlineSize;
uniform vec4 outlineColor;

uniform float flashAmount;

// Output fragment color
out vec4 finalColor;

void main()
{
    vec4 texel = texture(texture0, fragTexCoord);   // Get texel color
    
    vec2 texelScale = vec2(0.0);
    texelScale.x = outlineSize/textureSize.x;
    texelScale.y = outlineSize/textureSize.y;

    
    vec4 currentOutlineColor = outlineColor;
    currentOutlineColor.rgb *= currentOutlineColor.a;

    float myAlpha = texel.a;
    float upAlpha = texture(texture0, fragTexCoord + vec2(0, texelScale.y)).a;
    
    float downAlpha = texture(texture0, fragTexCoord - vec2(0, texelScale.y)).a;
    float rightAlpha = texture(texture0, fragTexCoord + vec2(texelScale.x, 0)).a;
    float leftAlpha = texture(texture0, fragTexCoord - vec2(texelScale.x, 0)).a;
    
    float upRightAlpha = texture(texture0, fragTexCoord + vec2(texelScale.x, texelScale.y)).a;
    float upLeftAlpha = texture(texture0, fragTexCoord + vec2(-texelScale.x, texelScale.y)).a;
    float downLeftAlpha = texture(texture0, fragTexCoord - vec2(texelScale.x, texelScale.y)).a;
    float downRightAlpha = texture(texture0, fragTexCoord - vec2(-texelScale.x, texelScale.y)).a;
    
    finalColor = mix(texel, currentOutlineColor, ceil(clamp(downAlpha + upAlpha + leftAlpha + rightAlpha + upRightAlpha + downLeftAlpha + upLeftAlpha + downRightAlpha, 0, 1)) - ceil(myAlpha));
    finalColor = mix(finalColor, vec4(1.0, 1.0, 1.0, finalColor.a), flashAmount);
}