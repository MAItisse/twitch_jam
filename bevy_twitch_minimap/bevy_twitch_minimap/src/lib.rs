use std::net::TcpStream;
use std::sync::{mpsc, Mutex};
use std::thread;
use std::time::Duration;

use bevy::prelude::*;
use serde::{Deserialize, Serialize};
use websocket::sync::{self as ws, Reader, Writer};
use websocket::ws::dataframe::DataFrame;
use websocket::OwnedMessage;

const HOST: &str = "websocket.matissetec.dev";

#[derive(Serialize, Debug, Clone)]
pub struct Unit {
    pub id: u32,
    pub kind: String,
    pub x: f32,
    pub y: f32,
}

#[derive(Serialize, Debug, Clone)]
#[serde(rename_all = "lowercase")]
pub enum ServerData {
    Css(String),
    Reset(()),
    #[serde(untagged)]
    Units(Vec<Unit>),
}

#[derive(Serialize, Event, Debug, Clone)]
pub struct ServerEvent {
    pub data: ServerData,
}

#[derive(Deserialize, Event, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct ClickEvent {
    pub x: f32,
    pub y: f32,
    pub user_id: String,
    pub bubble_color: String,
    pub bubble_size: f32,
}

#[derive(Deserialize, Event, Debug)]
#[serde(untagged)]
pub enum ClientEvent {
    Click(ClickEvent),
}

#[derive(Component, Debug)]
pub struct OnMinimap {
    pub kind: String,
    pub color: Color,
    pub extra_css: Option<String>,
}

impl Default for OnMinimap {
    fn default() -> Self {
        Self {
            kind: String::from("Sphere"),
            color: Color::WHITE,
            extra_css: None,
        }
    }
}

#[derive(Event, Clone, Debug)]
pub struct Connect {
    pub host: String,
    pub channel: String,
}

impl Connect {
    pub fn new_with_default_host(channel: String) -> Self {
        Self {
            host: String::from(HOST),
            channel,
        }
    }
}

#[derive(Resource, Clone)]
pub struct WorldInfo {
    pub size: Vec2,
    pub origin: Vec2,
}

#[derive(Resource)]
struct UpdateTimer(Timer);

#[derive(Resource, Default)]
pub struct ExtraCss(pub String);

impl UpdateTimer {
    fn new(duration: Duration) -> Self {
        Self(Timer::new(duration, TimerMode::Repeating))
    }
}

#[derive(Resource)]
struct Channels {
    client_events: Mutex<std::sync::mpsc::Receiver<ClientEvent>>,
    server_events: std::sync::mpsc::Sender<ServerEvent>,
}

pub struct TwitchMinimapPlugin {
    pub send_interval: Duration,
    pub world: WorldInfo,
    pub auto_connect: Option<Connect>,
}

impl Plugin for TwitchMinimapPlugin {
    fn build(&self, app: &mut App) {
        app.add_event::<ServerEvent>()
            .add_event::<ClickEvent>()
            .add_event::<ClientEvent>()
            .add_event::<Connect>()
            .insert_resource(self.world.clone())
            .insert_resource(UpdateTimer::new(self.send_interval))
            .init_resource::<ExtraCss>()
            .add_systems(
                Update,
                (
                    spread_client_event,
                    handle_connect_event,
                    (translate_client_event, translate_server_event)
                        .run_if(resource_exists::<Channels>),
                    update_unit_positions,
                    update_css,
                ),
            );

        if let Some(connect) = self.auto_connect.clone() {
            app.add_systems(Startup, move |mut writer: EventWriter<Connect>| {
                writer.send(connect.clone());
            });
        }
    }
}

fn handle_connect_event(mut commands: Commands, mut connect: EventReader<Connect>) {
    for connect in connect.read() {
        let connect = connect.clone();

        let (client_sender, client_recv) = mpsc::channel();
        let (server_sender, server_recv) = mpsc::channel();

        let channels = Channels {
            client_events: Mutex::new(client_recv),
            server_events: server_sender,
        };
        commands.insert_resource(channels);

        thread::spawn(move || {
            establish_connection(connect, client_sender, server_recv);
        });
    }
}

fn establish_connection(
    connect: Connect,
    client_events: mpsc::Sender<ClientEvent>,
    server_events: mpsc::Receiver<ServerEvent>,
) {
    let url = format!(
        "https://{}/lobby/new?user={}",
        connect.host, connect.channel
    );
    let key = reqwest::blocking::Client::new()
        .post(url)
        .send()
        .unwrap()
        .text()
        .unwrap();

    let url = format!(
        "ws://{}/lobby/connect/streamer?user={}&key={}",
        connect.host, connect.channel, key
    );
    let client = ws::client::ClientBuilder::new(&url)
        .unwrap()
        .connect_insecure()
        .unwrap();

    let (reader, writer) = client.split().unwrap();

    thread::spawn(move || handle_client_events(reader, client_events));
    thread::spawn(move || handle_server_events(writer, server_events));
}

fn handle_client_events(mut reader: Reader<TcpStream>, client_events: mpsc::Sender<ClientEvent>) {
    while let Ok(message) = reader.recv_message() {
        let bytes = message.take_payload();
        if let Ok(event) = serde_json::from_slice(&bytes) {
            client_events.send(event).unwrap();
        }
    }
}

fn handle_server_events(mut writer: Writer<TcpStream>, server_events: mpsc::Receiver<ServerEvent>) {
    while let Ok(event) = server_events.recv() {
        let text = serde_json::to_string(&event).unwrap();
        writer.send_message(&OwnedMessage::Text(text)).unwrap();
    }
}

fn translate_client_event(channels: Res<Channels>, mut client_event: EventWriter<ClientEvent>) {
    let Ok(client_channel) = channels.client_events.try_lock() else {
        return;
    };

    if let Ok(event) = client_channel.try_recv() {
        client_event.send(event);
    }
}

fn translate_server_event(channels: Res<Channels>, mut server_event: EventReader<ServerEvent>) {
    for event in server_event.read() {
        channels.server_events.send(event.clone()).unwrap();
    }
}

fn spread_client_event(
    mut client_event: EventReader<ClientEvent>,
    mut click_event: EventWriter<ClickEvent>,
) {
    for event in client_event.read() {
        match event {
            ClientEvent::Click(event) => {
                click_event.send(event.clone());
            }
        }
    }
}

fn update_unit_positions(
    query: Query<(Entity, &OnMinimap, &Transform)>,
    mut timer: ResMut<UpdateTimer>,
    time: Res<Time>,
    world: Res<WorldInfo>,
    mut events: EventWriter<ServerEvent>,
) {
    if timer.0.tick(time.delta()).just_finished() {
        let mut units = Vec::new();
        for (id, data, location) in &query {
            let mut world_pos = location.translation.truncate();
            world_pos.y *= -1.0;
            let map_pos = world_pos - world.origin;
            let normalized = map_pos / world.size;

            units.push(Unit {
                id: id.index(),
                kind: data.kind.clone(),
                x: normalized.x,
                y: normalized.y,
            });
        }

        events.send(ServerEvent {
            data: ServerData::Units(units),
        });
    }
}

#[derive(Resource)]
struct CssTimer(Timer);

impl Default for CssTimer {
    fn default() -> Self {
        CssTimer(Timer::new(Duration::from_secs(1), TimerMode::Repeating))
    }
}

fn update_css(
    query: Query<(Entity, &OnMinimap)>,
    extra_css: Res<ExtraCss>,
    mut server: EventWriter<ServerEvent>,
    mut timer: Local<CssTimer>,
    time: Res<Time>,
) {
    if !timer.0.tick(time.delta()).just_finished() {
        return;
    }

    let mut css_string = String::new();
    for (id, data) in &query {
        let [r, g, b, _] = data.color.to_srgba().to_u8_array();
        let color = format!("rgb({r}, {g}, {b})");
        let inner_css = format!(
            "background-color: {color};{}",
            data.extra_css.clone().unwrap_or_default()
        );

        css_string.push_str(&format!("._{} {{{inner_css}}}", id.index()));
    }

    css_string.push_str(&extra_css.0);
    server.send(ServerEvent {
        data: ServerData::Css(css_string),
    });
}
