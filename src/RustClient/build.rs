use std::io::Error;

fn main() -> Result<(), Error> {
  // build the protobuf structs
  prost_build::compile_protos(&["../protos/messages.proto"], &["../protos/"])?;
  Ok(())
}
