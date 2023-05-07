shader_type canvas_item;
uniform float scroll_speed;
uniform vec4 tint : source_color;

void fragment() {
	vec2 shifteduv = UV;
    shifteduv.x += TIME * scroll_speed;
    shifteduv.y -= TIME * scroll_speed;
    vec4 color = texture(TEXTURE, shifteduv);
    color *= tint;
    COLOR = color;
}
