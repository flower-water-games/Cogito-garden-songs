shader_type canvas_item;

uniform sampler2D albedo_texture;
uniform sampler2D normal_texture;
uniform sampler2D roughness_texture;
uniform sampler2D metallic_texture;
uniform sampler2D emission_texture;
uniform sampler2D height_texture;
uniform sampler2D ambient_occlusion_texture;

uniform float rotation_speed : hint_range(0, 360) = 90.0;

void fragment() {
    // Calculate the rotation angle
    float angle = radians(rotation_speed * TIME);

    // Calculate the center of rotation
    vec2 center = vec2(0.5, 0.5);

    // Calculate the rotated texture coordinates
    vec2 tex_coords_rotated = vec2(
        cos(angle) * (FRAGCOORD.x - center.x) - sin(angle) * (FRAGCOORD.y - center.y) + center.x,
        sin(angle) * (FRAGCOORD.x - center.x) + cos(angle) * (FRAGCOORD.y - center.y) + center.y
    );

    // Sample from the textures
    vec3 albedo = texture(albedo_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).rgb;
    vec3 normal = texture(normal_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).rgb;
    float roughness = texture(roughness_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).r;
    float metallic = texture(metallic_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).r;
    vec3 emission = texture(emission_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).rgb;
    float height = texture(height_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).r;
    float ambient_occlusion = texture(ambient_occlusion_texture, tex_coords_rotated / SCREEN_PIXEL_SIZE).r;

    // Combine material properties (you can adjust this based on your needs)
    vec3 final_color = albedo;

    // Output the final color
    COLOR.rgb = final_color;
    COLOR.a = 1.0;
}
