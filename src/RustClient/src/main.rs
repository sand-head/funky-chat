use app::{App, Message};
use chrono::Local;
use connection::Connection;
use crossterm::event;
use events::EventHandler;
use ui::draw_app;

use std::io;

use anyhow::Result;
use tui::{backend::CrosstermBackend, Terminal};

use crate::messages::{Command, ChatCommand, DirectChatCommand, ExitCommand, command::Command as CommandType};

mod app;
mod connection;
mod events;
mod ui;

pub mod messages {
  include!(concat!(env!("OUT_DIR"), "/funkychat.protos.rs"));
}

fn main() -> Result<()> {
  // set up terminal
  let stdout = io::stdout();
  let backend = CrosstermBackend::new(stdout);
  let mut terminal = Terminal::new(backend)?;
  terminal.clear()?;

  // set up application struct
  let mut app = App::default();

  // set up event handler
  let event = EventHandler::default();

  // connect to the FunkyChat server
  let connection = Connection::connect("127.0.0.1:13337", &event)?;

  loop {
    // draw the UI frame onto the terminal
    terminal.draw(|f| draw_app(f, &mut app))?;

    // wait for incoming events
    let next_event = event.next()?;
    match next_event {
      events::Event::UserInput(key_event) => match key_event.code {
        // handle user input in the text box
        event::KeyCode::Backspace => {
          app.input.pop();
        }
        event::KeyCode::Enter => {
          if app.input.len() == 0 {
            // don't send empty messages to the server!
            continue;
          }

          let user_input: String = app.input.drain(..).collect();
          let mut command = Command {
            command: None
          };

          if user_input.starts_with(".exit") {
            command.command = Some(CommandType::Exit(ExitCommand {}));
            // send command and break from loop
            connection.send(command)?;
            break;
          } else if user_input.starts_with(".chat") {
            let args: Vec<&str> = user_input[".chat".len()..].trim().split(' ').collect();
            if args.len() < 2 {
              app.add_message(r#"The ".chat" command requires two arguments: the recipient ID and the message."#);
              continue;
            } else if args[0] == app.user_id.clone().unwrap() {
              app.add_message("You can't send a direct message to yourself!");
              continue;
            }

            command.command = Some(CommandType::DirectChat(DirectChatCommand {
              user_id: args[0].to_string(),
              message: args[1..].join(" ")
            }));
          } else {
            command.command = Some(CommandType::Chat(ChatCommand {
              message: user_input
            }));
          }

          connection.send(command)?;
        }
        event::KeyCode::Char(c) => {
          app.input.push(c);
        }
        _ => {}
      },
      events::Event::ServerResponse(res) => match res {
        // handle incoming responses from the server
        messages::response::Response::Welcome(welcome) => {
          app.user_id = Some(welcome.user_id.clone());
          let connected_users = if welcome.connected_users.len() > 0 {
            welcome.connected_users.join(", ")
          } else {
            "None".to_string()
          };

          app.add_message(&format!("Welcome, {}!", welcome.user_id));
          app.add_message(&format!("Online users: {}", connected_users));
        }
        messages::response::Response::Echo(echo) => {
          app.messages.push(Message {
            from: app.user_id.clone(),
            message: echo.message,
            timestamp: Local::now()
          });
        }
        messages::response::Response::Chat(chat) => {
          let from = if chat.is_direct {
            // todo: fix how this is broken for the user who sends the direct message
            format!("{} > {}", chat.user_id, app.user_id.clone().unwrap())
          } else {
            chat.user_id
          };

          app.messages.push(Message {
            from: Some(from),
            message: chat.message,
            timestamp: Local::now()
          });
        }
        messages::response::Response::Join(join) => {
          app.add_message(&format!("{} has joined the funky chat.", join.user_id));
        }
        messages::response::Response::Leave(leave) => {
          app.add_message(&format!("{} has left the funky chat.", leave.user_id));
        }
      },
    }
  }

  Ok(())
}
