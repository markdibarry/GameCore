vec3 get_contrast(vec3 color, float contrast_amount) {
    return ((color - 0.5) * max(contrast_amount + 1.0, 0)) + 0.5;
}

vec3 get_brightness(vec3 color, float brightness_amount) {
    return color + brightness_amount;
}

vec3 get_saturation(vec3 color, float saturation_amount) {
    return mix(vec3(dot(vec3(1.0), color) * 0.33333), color, saturation_amount + 1.0);
}

vec3 get_tint(vec3 color, vec4 tint_color, float tint_amount) {
    vec3 c2 = color * tint_color.rgb;
    return mix(color, c2, tint_amount);
}

vec3 adjust_color(vec3 c, vec4 tint_color, float tint, float contrast, float brightness, float saturation) {
    vec3 color = c;
    color = get_tint(color, tint_color, tint);
    color = get_contrast(color, contrast);
    color = get_saturation(color, saturation);
    color = get_brightness(color, brightness);
    return color;
}