use anyhow::Result;
use chrono::{DateTime, Local};

use crate::{connection::Connection, events::EventHandler, messages::Command};

pub mod states;

pub struct Message {
  pub from: Option<String>,
  pub message: String,
  pub timestamp: DateTime<Local>,
}

#[derive(Debug, PartialEq, Eq)]
pub enum AppState {
  ServerInput,
  Connecting,
  Connected,
  Disconnecting,
}

/// Holds the current state of the application, along with other necessary properties.
pub struct App {
  /// The current application state.
  pub state: AppState,
  /// The hostname of the server.
  pub server: String,
  /// The local user's identifier.
  pub user_id: Option<String>,
  /// The identifiers of all connected users.
  pub online_users: Vec<String>,
  /// Current value of the user's input.
  pub input: String,
  /// Received messages from the server.
  pub messages: Vec<Message>,
  /// The connection to the server.
  connection: Option<Connection>,
}
impl Default for App {
  fn default() -> Self {
    App {
      state: AppState::ServerInput,
      server: String::new(),
      user_id: None,
      online_users: Vec::new(),
      input: String::new(),
      messages: Vec::new(),
      connection: None,
    }
  }
}
impl App {
  /// Adds a message to the local messages vector without sending it to the server.
  pub fn add_message(&mut self, msg: &str) {
    self.messages.push(Message {
      from: None,
      message: msg.to_string(),
      timestamp: Local::now(),
    });
  }

  pub fn send_message(&self, msg: Command) -> Result<()> {
    self.connection.as_ref().unwrap().send(msg)?;
    Ok(())
  }

  pub fn connect(&mut self, event_handler: &EventHandler) -> Result<()> {
    self.connection = Some(Connection::connect(&self.server, event_handler)?);
    Ok(())
  }
}
