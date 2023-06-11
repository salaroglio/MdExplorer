use std::fs::File;
use std::io::{Error,Read};
use regex::Regex;
use sqlx::{Row, Connection};
use sqlx::mssql;
use sqlx::MssqlPool;


// fn main() {
    
//     //callDB();
//     //let file_path = ;
//     let result_read_file = read_file("C:\\Users\\Carlo\\Documents\\0-progetti\\artigiancassa\\artigiancassadocs\\analisi-as-is\\AS400-StoredProcedures.md");
//     let file_content = match result_read_file{
//         Ok(file)=>file,
//         Err(error)=>panic!("problema di lettura: {:?}",error)};
        
//         let re = Regex::new(r"[^!]\[[^\]]*\]\(([^\)]*)\)").unwrap();        
        
//         let mut link_count:u16= 0;
        
//         for capture in re.captures_iter(file_content.as_str()){
//             if let Some(url) = capture.get(1){
//                 link_count +=1;
//                 println!("found link {}:{}",link_count,url.as_str());                
//             }
//         }
        
//     }
    
    // fn read_file(filename:&str)->Result<String,Error>{
    //     let mut file_handle = File::open(filename)?;
    //     let mut file_data = String::new();
    //     file_handle.read_to_string(&mut file_data)?;
    //     Ok(file_data)
    // }
    
    
    

#[tokio::main]
async fn main() -> Result<(), sqlx::Error> {
    // Connection string for the SQL Server database
    //let conn_str = "Provider=sqloledb;Data Source=127.0.0.1;Initial Catalog=test;User Id=sa;Password=Porcagal72!;";

    // Connect to the database
    //let pool = MssqlPool::connect(&conn_str).await?;
    let my_pool = MssqlPool::connect("mssql://sa:Porcagal72!@localhost/test").await?;

    // Perform a query
    let query = "SELECT * FROM Table_1";
    let rows = sqlx::query(query).fetch_all(&my_pool).await?;

    // Process the results
    for row in rows {
        let id: i32 = row.get("intero");
        let name: bool = row.get("booleano");        

        println!("intero: {}, booleano: {}", id, name);
    }

    Ok(())
}

    