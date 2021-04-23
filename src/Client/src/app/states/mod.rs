use anyhow::Result;
use tui::{backend::Backend, Terminal};

use crate::events::EventHandler;

use super::App;

pub mod connected;
pub mod server_input;

pub trait StateHandler {
  fn handle<B: Backend>(
    terminal: &mut Terminal<B>,
    event: &EventHandler,
    app: &mut App,
  ) -> Result<()>;
}
