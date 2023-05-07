shader_type canvas_item;
render_mode blend_mix,unshaded;

#include "res://GameCore/Shaders/ColorAdjustment.gdshaderinc"

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float _brightness : hint_range(-1, 1) = 0;
uniform float _contrast : hint_range(-1, 1) = 0;
uniform float _saturation : hint_range(-1, 2) = 0;
uniform vec4 _tint_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float _tint_amount : hint_range(0, 1) = 0;
uniform sampler2D mask_sampler : filter_nearest;
uniform vec3 _mask_transforms[3];

float get_mask_alpha(vec2 tex_size, vec2 tex_UV, vec2 mask_size, vec2 mask_position, float mask_scale) {
    vec2 mask_size_scaled = mask_size * mask_scale;
    vec2 mask_position_adj = (mask_position - mask_size_scaled * 0.5) / tex_size;
    vec2 mask_UV = (tex_UV - mask_position_adj) * (tex_size / mask_size_scaled);
    return texture(mask_sampler, mask_UV).a;
}

void fragment() {
    vec4 orig_color = texture(SCREEN_TEXTURE, SCREEN_UV);
    vec3 c = adjust_color(orig_color.rgb, _tint_color, _tint_amount, _contrast, _brightness, _saturation);
    vec2 texture_size = vec2(480,270);
    vec2 mask_size = vec2(textureSize(mask_sampler,0));
    float mask_alpha = 0.0;
    float size_mod = abs(mod(TIME * 5.0, 2.0) - 1.0);
    size_mod = size_mod * 0.05 + 0.975;
    for (int i = 0; i < 3; i++) {
        mask_alpha += get_mask_alpha(texture_size, UV, mask_size, _mask_transforms[i].xy, _mask_transforms[i].z * size_mod);
    }
    
    COLOR.rgb = c;
    COLOR.a = clamp(orig_color.a - mask_alpha, 0.0, 1.0);
}