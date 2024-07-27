//! Game server

#![warn(
    clippy::pedantic,
    clippy::clone_on_ref_ptr,
    clippy::create_dir,
    clippy::filetype_is_file,
    clippy::fn_to_numeric_cast_any,
    clippy::if_then_some_else_none,
    missing_docs,
    clippy::missing_docs_in_private_items,
    missing_copy_implementations,
    missing_debug_implementations,
    clippy::missing_const_for_fn,
    clippy::mixed_read_write_in_expression,
    clippy::panic,
    clippy::partial_pub_fields,
    clippy::same_name_method,
    clippy::str_to_string,
    clippy::suspicious_xor_used_as_pow,
    clippy::try_err,
    clippy::unneeded_field_pattern,
    clippy::use_debug,
    clippy::verbose_file_reads,
    clippy::expect_used
)]
#![deny(
    clippy::unwrap_used,
    clippy::unreachable,
    clippy::unimplemented,
    clippy::todo,
    clippy::dbg_macro,
    clippy::error_impl_error,
    clippy::exit,
    clippy::panic_in_result_fn,
    clippy::tests_outside_test_module
)]

#[macro_use]
extern crate rocket;

use std::collections::HashMap;
use std::sync::{Arc, RwLock};

use rocket::http::Status;
use rocket::response::status;
use rocket::serde::json::Json;
use rocket::{Request, State};
use serde::{Deserialize, Serialize};

/// The host we are at
const HOST: &str = "http://localhost:8000";

/// Twitch user id
type UserId = Arc<str>;

/// A lobby is one instance of a game, one per channel
#[derive(Serialize, Deserialize, Debug, Clone)]
struct Lobby {
    /// Streamer who created the lobby
    owner: UserId,
    /// Key required for the streamer to connect to the socket
    streamer_key: String,
}

impl Lobby {
    /// Create a new lobby
    fn new(owner: UserId) -> Self {
        Self {
            owner,
            streamer_key: "Viv is cool".into(),
        }
    }
}

/// Holds information on the lobbies
#[derive(Default)]
struct Lobbies {
    /// Lookup from userid to lobby
    channels: RwLock<HashMap<UserId, Lobby>>,
}

/// Errors that can happen in the api
#[derive(Responder, Debug, PartialEq, Eq)]
enum Errors {
    /// Not found
    #[response(status = 404)]
    NotFound(String),
    /// Already Exsists
    #[response(status = 409)]
    LobbyAlreadyExsists(String),
    /// We have no clue what happend
    #[response(status = 500)]
    Unknown(String),
}

/// Extension for Result for convenient shit
trait ResultExt<T, E> {
    /// Unknwon with custom msg
    fn unknown_with(self, msg: impl Into<String>) -> Result<T, Errors>;
    /// Use errors `Debug` as message
    fn unknown(self) -> Result<T, Errors>
    where
        E: std::fmt::Debug;
}

impl<T, E> ResultExt<T, E> for Result<T, E> {
    fn unknown_with(self, msg: impl Into<String>) -> Result<T, Errors> {
        match self {
            Ok(val) => Ok(val),
            Err(_) => Err(Errors::Unknown(msg.into())),
        }
    }
    fn unknown(self) -> Result<T, Errors>
    where
        E: std::fmt::Debug,
    {
        match self {
            Ok(val) => Ok(val),
            Err(err) => Err(Errors::Unknown(format!("{err:?}"))),
        }
    }
}

/// A generic unknown error
#[derive(Serialize)]
struct GenericError {
    /// Status code
    status: u16,
    /// Reason
    reason: Option<String>,
}

#[catch(default)]
fn default_catcher(status: Status, _request: &Request) -> Json<GenericError> {
    Json(GenericError {
        status: status.code,
        reason: status.reason().map(ToOwned::to_owned),
    })
}

/// Return a simple message to show we are working
#[get("/")]
const fn index() -> &'static str {
    "I am online!"
}

/// Create a new lobby for the specifed user
#[post("/lobby/new?<user>")]
fn new_lobby(user: &str, lobbies: &State<Lobbies>) -> Result<status::Created<String>, Errors> {
    let user = Arc::from(user);
    let mut channels = lobbies.channels.write().unknown()?;

    if channels.contains_key(&user) {
        return Err(Errors::LobbyAlreadyExsists("Lobby already in play, please close exsisting game instance or wait for previous lobby to timeout".into()));
    }

    let lobby = Lobby::new(Arc::clone(&user));
    channels.insert(Arc::clone(&user), lobby.clone());

    Ok(status::Created::new(format!(
        "{HOST}/lobby/connect?user={user}&key={}",
        lobby.streamer_key
    ))
    .body(lobby.streamer_key))
}

/// Get info on the specific lobby
#[get("/debug/get_lobby?<user>")]
fn get_lobby(user: &str, lobbies: &State<Lobbies>) -> Result<Json<Lobby>, Errors> {
    let user = Arc::from(user);
    let channels = lobbies.channels.read().unknown()?;

    if let Some(lobby) = channels.get(&user) {
        Ok(Json(lobby.clone()))
    } else {
        Err(Errors::NotFound((*user).into()))
    }
}

/// Start the server
#[launch]
fn rocket() -> _ {
    rocket::build()
        .mount("/", routes![index, new_lobby, get_lobby])
        .register("/", catchers![default_catcher])
        .manage(Lobbies::default())
}

#[cfg(test)]
mod tests {
    #![allow(clippy::unwrap_used)]

    use super::*;
    mod result_ext {
        use super::*;

        #[test]
        fn unknown_with_pass() {
            let res: Result<u8, ()> = Ok(10);
            assert_eq!(res.unknown_with("test"), Ok(10));
        }
        #[test]
        fn unknown_pass() {
            let res: Result<u8, ()> = Ok(10);
            assert_eq!(res.unknown(), Ok(10));
        }
        #[test]
        fn unknown_with_fail() {
            let res: Result<u8, ()> = Err(());
            assert_eq!(
                res.unknown_with("test"),
                Err(Errors::Unknown("test".into()))
            );
        }
        #[test]
        fn unknown_fail() {
            let x = "hello world";
            let res: Result<u8, _> = Err(x);
            assert_eq!(res.unknown(), Err(Errors::Unknown(format!("{x:?}"))));
        }
    }

    mod create_lobby {
        use rocket::local::blocking::Client;

        use super::*;

        #[test]
        fn create() {
            let client = Client::tracked(rocket()).unwrap();
            let response = client.post(uri!(new_lobby("viv"))).dispatch();

            assert_eq!(response.status(), Status::Created);
        }

        #[test]
        fn duplicate() {
            let client = Client::tracked(rocket()).unwrap();

            client.post(uri!(new_lobby("viv"))).dispatch();
            let response = client.post(uri!(new_lobby("viv"))).dispatch();

            assert_eq!(response.status(), Status::Conflict);
        }

        #[test]
        fn get_found() {
            let client = Client::tracked(rocket()).unwrap();
            client.post(uri!(new_lobby("viv"))).dispatch();
            let response = client.get(uri!(get_lobby("viv"))).dispatch();

            assert_eq!(response.status(), Status::Ok);
            let lobby: Lobby = response.into_json().unwrap();
            assert_eq!(lobby.owner, "viv".into());
        }

        #[test]
        fn get_non_exsistant() {
            let client = Client::tracked(rocket()).unwrap();
            let response = client.get(uri!(get_lobby("viv"))).dispatch();

            assert_eq!(response.status(), Status::NotFound);
        }
    }
}
