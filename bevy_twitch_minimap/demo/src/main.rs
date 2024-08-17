use std::time::Duration;

use bevy::prelude::*;
use bevy::sprite::{MaterialMesh2dBundle, Mesh2dHandle};
use bevy_twitch_minimap::*;

const CHANNEL: &str = "vivax";

#[derive(Component)]
struct Player;

fn main() {
    App::new()
        .add_plugins(DefaultPlugins)
        .add_plugins(TwitchMinimapPlugin {
            send_interval: Duration::from_millis(100),
            world: WorldInfo {
                size: Vec2::new(200.0, 200.0),
                origin: Vec2::new(-100.0, -100.0),
            },
            auto_connect: Some(Connect::new_with_default_host(CHANNEL.into())),
        })
        .add_systems(Startup, (setup,))
        .add_systems(Update, (print_client_events, move_player, update_color))
        .run();
}

fn setup(
    mut commands: Commands,
    mut meshes: ResMut<Assets<Mesh>>,
    mut materials: ResMut<Assets<ColorMaterial>>,
) {
    commands.spawn(Camera2dBundle::default());
    commands.spawn((
        MaterialMesh2dBundle {
            mesh: Mesh2dHandle(meshes.add(Rectangle::new(20.0, 20.0))),
            material: materials.add(Color::srgb_u8(255, 0, 0)),
            ..default()
        },
        OnMinimap {
            kind: "Square".into(),
            color: Color::srgb_u8(0, 255, 0),
            ..default()
        },
        Player,
    ));
}

fn print_client_events(mut events: EventReader<ClickEvent>) {
    for event in events.read() {
        println!("{} clicked on {}, {}", event.user_id, event.x, event.y);
    }
}

fn move_player(
    mut query: Query<&mut Transform, With<Player>>,
    keyboard: Res<ButtonInput<KeyCode>>,
    time: Res<Time>,
) {
    let Ok(mut trans) = query.get_single_mut() else {
        return;
    };

    let mut dir = Vec2::ZERO;
    if keyboard.pressed(KeyCode::KeyD) {
        dir.x += 1.;
    }
    if keyboard.pressed(KeyCode::KeyA) {
        dir.x -= 1.;
    }
    if keyboard.pressed(KeyCode::KeyW) {
        dir.y += 1.;
    }
    if keyboard.pressed(KeyCode::KeyS) {
        dir.y -= 1.;
    }

    let delta = dir * 200.0 * time.delta_seconds();
    trans.translation += delta.extend(0.0);
}

fn update_color(
    mut query: Query<&mut OnMinimap, With<Player>>,
    mouse: Res<ButtonInput<MouseButton>>,
) {
    let Ok(mut data) = query.get_single_mut() else {
        return;
    };

    if mouse.just_pressed(MouseButton::Left) {
        println!("UPDATING COLOR");
        data.color = data.color.rotate_hue(60.0);
    }
}
