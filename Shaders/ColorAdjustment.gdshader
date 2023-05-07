shader_type canvas_item;
render_mode blend_mix,unshaded;

#include "res://GameCore/Shaders/ColorAdjustment.gdshaderinc"

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float _brightness : hint_range(-1, 1) = 0;
uniform float _contrast : hint_range(-1, 1) = 0;
uniform float _saturation : hint_range(-1, 2) = 0;
uniform vec4 _tint_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float _tint_amount : hint_range(0, 1) = 0;

void fragment() {
	vec3 c = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
    c = adjust_color(c, _tint_color, _tint_amount, _contrast, _brightness, _saturation);
    COLOR.rgb = c;
}