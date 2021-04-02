use crate::{Message, app::AppState, events::EventHandler};
use anyhow::Result;
use chrono::Local;
use crossterm::event;
use events::Event;
use messages::response::Response;
use tui::{backend::Backend, Terminal};

use crate::messages::{
  chat_response::OptionalToId, command::Command as CommandType, ChatCommand, Command,
  DirectChatCommand, ExitCommand,
};
use crate::{app::App, events, messages, ui::draw_app};

use super::StateHandler;

pub struct ConnectedState {}
impl StateHandler for ConnectedState {
  fn handle<B>(terminal: &mut Terminal<B>, event: &EventHandler, app: &mut App) -> Result<()>
  where
    B: Backend,
  {
    // draw the UI frame onto the terminal
    terminal.draw(|f| draw_app(f, app))?;

    // wait for incoming events
    let next_event = event.next()?;
    match next_event {
      Event::Resize(_, _) => {
        // stop blocking to allow the frame to re-draw
        return Ok(());
      }
      Event::UserInput(key_event) => {
        match key_event.code {
          // handle user input in the text box
          event::KeyCode::Backspace => {
            app.input.pop();
          }
          event::KeyCode::Enter if app.input.len() > 0 => {
            let user_input: String = app.input.drain(..).collect();
            let mut command = Command { command: None };

            if user_input.starts_with(".exit") {
              command.command = Some(CommandType::Exit(ExitCommand {}));
              // send command and switch to the disconnecting state
              app.send_message(command)?;
              app.state = AppState::Disconnecting;
              return Ok(());
            } else if user_input.starts_with(".chat") {
              let args: Vec<&str> = user_input[".chat".len()..].trim().split(' ').collect();
              if args.len() < 2 {
                app.add_message(
                r#"The ".chat" command requires two arguments: the recipient ID and the message."#,
              );
                return Ok(());
              } else if args[0] == app.user_id.clone().unwrap() {
                app.add_message("You can't send a direct message to yourself!");
                return Ok(());
              }

              command.command = Some(CommandType::DirectChat(DirectChatCommand {
                user_id: args[0].to_string(),
                message: args[1..].join(" "),
              }));
            } else {
              command.command = Some(CommandType::Chat(ChatCommand {
                message: user_input,
              }));
            }

            app.send_message(command)?;
          }
          event::KeyCode::Char(c) => {
            app.input.push(c);
          }
          _ => {}
        }
      }
      Event::ServerResponse(res) => match res {
        // handle incoming responses from the server
        Response::Welcome(mut welcome) => {
          app.user_id = Some(welcome.user_id.clone());
          app.online_users.append(&mut welcome.connected_users);
          // todo: do this better
          // maybe have the local user's name italicized?
          app.online_users.push(welcome.user_id.clone());

          app.add_message(&format!("Welcome, {}!", welcome.user_id));
        }
        Response::Echo(echo) => {
          app.messages.push(Message {
            from: app.user_id.clone(),
            message: echo.message,
            timestamp: Local::now(),
          });
        }
        Response::Chat(chat) => {
          // format `from` differently for direct messages
          let from = if let Some(OptionalToId::ToId(to_id)) = chat.optional_to_id {
            format!("{} > {}", chat.from_id, to_id)
          } else {
            chat.from_id
          };

          app.messages.push(Message {
            from: Some(from),
            message: chat.message,
            timestamp: Local::now(),
          });
        }
        Response::Join(join) => {
          app.online_users.push(join.user_id.clone());
          app.add_message(&format!("{} has joined the funky chat.", join.user_id));
        }
        Response::Leave(leave) => {
          if let Some(pos) = app.online_users.iter().position(|x| *x == leave.user_id) {
            app.online_users.remove(pos);
          }
          app.add_message(&format!("{} has left the funky chat.", leave.user_id));
        }
      },
    }

    Ok(())
  }
}
