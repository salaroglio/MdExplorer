use ferris_says::say;
use std::io::{stdout,BufWriter};
use std::fs;


fn main() {
    let readSomething = fs::read_to_string("C:\\Users\\Carlo\\Documents\\0-progetti\\artigiancassa\\artigiancassadocs\\analisi-as-is\\AS400-StoredProcedures.md");    
    let currentFile = match readSomething{
                    Ok(file)=>file,
                    Err(error)=>panic!("prolema di lettura: {:?}",error)};
    
    

    let stdout = stdout();
    let message = String::from("Hello fellow Rustaceans!");
    let width = message.chars().count();
    let mut writer = BufWriter::new(stdout.lock());
    say(message.as_str(),width,&mut writer).unwrap();
    println!("Hello, world!");
}
