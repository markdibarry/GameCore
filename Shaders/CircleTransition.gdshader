shader_type canvas_item;

uniform vec4 in_color: source_color;
uniform vec4 out_color: source_color;
uniform float in_out: hint_range(0.0, 1.0) = 0.0;
uniform float position: hint_range(-1.5, 1.0) = 1.0;
uniform vec2 size = vec2(16.0, 16.0);

void fragment() {
    vec2 a = (1.0 / SCREEN_PIXEL_SIZE) / size;

    vec2 uv = UV * a;
    vec2 i_uv = floor(uv);
    vec2 f_uv = fract(uv);

    float wave = max(0.0, i_uv.x / (a.x) - position);
    vec2 center = f_uv * 2.0 - 1.0;
    float circle = length(center);
    circle = 1.0 - step(wave, circle);

    vec4 color = texture(TEXTURE, UV);

    COLOR = vec4(circle) * color;
}