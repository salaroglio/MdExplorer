use ferris_says::say;
use std::io::{stdout,BufWriter};
use std::fs;
use regex::Regex;

fn main() {
    
    //let file_path = ;
    let read_something = fs::read_to_string("C:\\Users\\Carlo\\Documents\\0-progetti\\artigiancassa\\artigiancassadocs\\analisi-as-is\\AS400-StoredProcedures.md");
    let current_file = match read_something{
                    Ok(file)=>file,
                    Err(error)=>panic!("problema di lettura: {:?}",error)};
        
    let re = Regex::new(r"[^!]\[[^\]]*\]\(([^\)]*)\)").unwrap();
    re.find(current_file.as_str());

    
    let stdout = stdout();
    let message = String::from("Hello fellow Rustaceans!");
    let width = message.chars().count();
    let mut writer = BufWriter::new(stdout.lock());
    say(message.as_str(),width,&mut writer).unwrap();
    println!("Hello, world again!");
}
