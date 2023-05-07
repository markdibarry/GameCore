shader_type canvas_item;
const float PALETTE_UNIT = 0.0625;
uniform sampler2D palette1 : filter_nearest;
uniform sampler2D palette2 : filter_nearest;
uniform vec4 flash_color : source_color = vec4(1.0);
uniform float flash_mix : hint_range(0.0, 1.0) = 0.0;
uniform float speed : hint_range(0.0, 2.0) = 0.0;
uniform float cycle_start : hint_range(0.0, 16.0, 1.0) = 0.0;
uniform float cycle_end : hint_range(0.0, 16.0, 1.0) = 0.0;
uniform float cycle_direction : hint_range(-1.0, 1.0, 2.0) = 1.0;
uniform bool use_palette1;
uniform bool use_palette2;

vec3 flash(vec3 color) {
    return mix(color.rgb, flash_color.rgb, flash_mix);
}

vec3 applyPalette(vec3 color) {
    float start = PALETTE_UNIT * cycle_start;
    float end = PALETTE_UNIT * cycle_end;
    float color_pos = color.r;
    float stepped_time = floor(TIME * speed * 10.0) * PALETTE_UNIT  * cycle_direction + color_pos;

    if (start < color_pos && color_pos < end) {
        color_pos = mod(stepped_time, end - start) + start;
        if (use_palette2)
            return texture(palette2, vec2(color_pos, 0)).rgb;
    }
    return texture(palette1, vec2(color_pos, 0)).rgb;
}

void fragment() {
	vec4 color = texture(TEXTURE, UV);
    if (use_palette1)
        color.rgb = applyPalette(color.rgb);
    color.rgb = flash(color.rgb);
    COLOR = color;
}
